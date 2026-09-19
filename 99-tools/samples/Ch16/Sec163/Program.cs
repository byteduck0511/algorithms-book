// ==================== 第 16.3 节：综合项目 —— 一个任务调度器 ====================
//
// 本节把全书的三个部分合成一个可运行的项目：
//   · 拓扑排序（13.1）—— 解析任务之间的依赖
//   · 优先队列（11.4）—— 在「当前可执行的任务」里挑优先级最高的
//   · 哈希表（第 6 章） —— 跟踪每个任务的状态
//
//   实验一：跑通一个构建系统场景，并做四项验收检查
//   实验二：优先队列版 vs 线性扫描版（10 万个任务）
//   实验三：贪心调度的局限 —— 「每次选当前优先级最高的」不保证全局最优
//
// 度量说明：耗时注明机器，多轮取最快；正确性用独立实现的暴力枚举校验。

using System.Diagnostics;

var rng = new Random(20260918);

// ==================== 实验一：跑通一个构建系统 ====================

Console.WriteLine("=== 实验一：调度一个构建系统 ===");
Console.WriteLine();

var scheduler = new Scheduler();
scheduler.Add("utils", priority: 5, dependsOn: []);
scheduler.Add("core", priority: 8, dependsOn: ["utils"]);
scheduler.Add("db", priority: 6, dependsOn: ["utils"]);
scheduler.Add("api", priority: 9, dependsOn: ["core", "db"]);
scheduler.Add("ui", priority: 3, dependsOn: ["core"]);
scheduler.Add("docs", priority: 1, dependsOn: []);
scheduler.Add("tests", priority: 7, dependsOn: ["api", "ui"]);
scheduler.Add("package", priority: 2, dependsOn: ["tests"]);

Console.WriteLine("  任务（括号里的数字越大越优先）：");
foreach (var job in scheduler.Jobs)
    Console.WriteLine($"    {job.Name,-9} 优先级 {job.Priority}  依赖 [{string.Join(", ", job.DependsOn)}]");
Console.WriteLine();

var result = scheduler.Run();

Console.WriteLine("  调度结果：");
Console.WriteLine($"    {string.Join(" -> ", result.Order)}");
Console.WriteLine();

// ---------- 四项验收检查 ----------
Console.WriteLine("  ---------- 验收检查 ----------");
Console.WriteLine();

bool check1 = result.Order.Count == scheduler.Jobs.Count
              && result.Order.Distinct().Count() == scheduler.Jobs.Count;
Console.WriteLine($"    [1] 每个任务恰好执行一次：{check1}");

bool check2 = CheckDependencies(scheduler, result.Order);
Console.WriteLine($"    [2] 所有依赖都被满足（前置任务都排在前面）：{check2}");

var (check3, violations) = CheckGreedyPriority(scheduler, result.Order);
Console.WriteLine($"    [3] 每一步执行的都是「当前可执行任务里优先级最高的」：{check3}"
    + (violations.Count == 0 ? "" : $"（违反 {violations.Count} 处）"));

Console.WriteLine($"    [4] 没有环（任务全部被调度完）：{!result.HasCycle}");
Console.WriteLine();

// ---------- 有环的情况 ----------
Console.WriteLine("  ---------- 如果依赖里出现环 ----------");
var cyclic = new Scheduler();
cyclic.Add("a", 1, ["c"]);
cyclic.Add("b", 2, ["a"]);
cyclic.Add("c", 3, ["b"]);
var cycResult = cyclic.Run();
Console.WriteLine($"    调度器返回的剩余任务：[{string.Join(", ", cycResult.Stuck)}]");
Console.WriteLine($"    判定有环：{cycResult.HasCycle}");
Console.WriteLine("    —— 与 13.1 节一致：Kahn 跑不完就说明有环，");
Console.WriteLine("       但「剩下的」不等于「环上的」（13.1 实测过），要精确报环得用三色标记。");
Console.WriteLine();

// ==================== 实验二：优先队列 vs 线性扫描 ====================

Console.WriteLine("=== 实验二：调度 10 万个任务 ===");
Console.WriteLine();

const int BigCount = 100_000;
const int Sources = 5_000;                       // 前 5000 个任务是「源头」，都没有依赖
var (bigJobs, bigEdgeCount) = RandomJobGraph(rng, BigCount, Sources);

var schedHeap = new Scheduler { UseHeap = true };
foreach (var (name, pri, deps) in bigJobs) schedHeap.Add(name, pri, deps);

var schedScan = new Scheduler { UseHeap = false };
foreach (var (name, pri, deps) in bigJobs) schedScan.Add(name, pri, deps);

Console.WriteLine($"  规模：{BigCount:N0} 个任务，{bigEdgeCount:N0} 条依赖边");
Console.WriteLine($"        其中 {Sources:N0} 个是「源头」（没有任何依赖），所以【就绪集合】长期维持在几千个");
Console.WriteLine();

schedHeap.Run();
schedScan.Run();      // 预热

double tHeap = BestMs(() => schedHeap.Run(), 5);
double tScan = BestMs(() => schedScan.Run(), 3);   // 扫描版太慢，少跑两轮

var rHeap = schedHeap.Run();
var rScan = schedScan.Run();

Console.WriteLine("    实现                        耗时        调度结果");
Console.WriteLine("    ------------------------    ---------   --------");
Console.WriteLine($"    优先队列（堆）              {tHeap,7:F1} ms   {rHeap.Order.Count:N0} 个任务");
Console.WriteLine($"    线性扫描（每次找最大）      {tScan,7:F1} ms   {rScan.Order.Count:N0} 个任务");
Console.WriteLine();
Console.WriteLine($"  两者结果一致：{rHeap.Order.SequenceEqual(rScan.Order)}");
Console.WriteLine($"  堆版比扫描版快 {tScan / tHeap:F1} 倍");
Console.WriteLine();
Console.WriteLine("  为什么差这么多？");
Console.WriteLine("    扫描版每执行一个任务，都要把【当前就绪集合】整个扫一遍找最大，");
Console.WriteLine("    而且选完之后还要从列表中间删掉它 —— 两步都是 O(就绪集合大小)；");
Console.WriteLine("    堆版「取最小」和「插入」都只要 O(log n)。");
Console.WriteLine();
Console.WriteLine("  【关键】差距大小取决于【就绪集合有多大】：");
Console.WriteLine("    如果图是一条链（就绪集合始终只有 1 个），两种写法没差别；");
Console.WriteLine($"    本例有 {Sources:N0} 个源头，就绪集合长期维持在几千，差距才显出来。");
Console.WriteLine();
Console.WriteLine("  这正是一个「换个容器」的例子：");
Console.WriteLine("    13.1 节换容器换的是【输出顺序】；");
Console.WriteLine("    这里换容器换的是【性能】—— 调度逻辑一个字没改。");
Console.WriteLine("    而在真实系统里，两者【同时】发生了：换容器既定了顺序，也定了快慢。");
Console.WriteLine();

// ==================== 实验三：贪心调度的局限 ====================

Console.WriteLine("=== 实验三：贪心调度的局限 ===");
Console.WriteLine();
Console.WriteLine("  「每次在可执行任务里挑优先级最高的」是一个【贪心】——");
Console.WriteLine("  它保证「每一步都最优」，但不保证「整体最优」。");
Console.WriteLine();

// 反例：先做能解锁一串高权重任务的「钥匙任务」，比先做当前优先级最高的更划算
var trap = new Scheduler();
trap.Add("key", priority: 5, dependsOn: []);        // 权重不高，但它是三个高权重任务的共同前置
trap.Add("hot1", priority: 100, dependsOn: ["key"]);
trap.Add("hot2", priority: 100, dependsOn: ["key"]);
trap.Add("hot3", priority: 100, dependsOn: ["key"]);
trap.Add("solo", priority: 6, dependsOn: []);       // 单独一个，优先级比 key 高一点点

Console.WriteLine("  构造的任务图：");
Console.WriteLine("    key (优先级 5)  <- 无依赖，但它是下面三个的前置");
Console.WriteLine("    hot1/hot2/hot3 (优先级 100)  <- 都依赖 key");
Console.WriteLine("    solo (优先级 6) <- 无依赖");
Console.WriteLine();

var greedy = trap.Run();
var (bestOrder, bestCost) = BruteForceBestOrder(trap);
int greedyCost = WeightedCompletionCost(trap, greedy.Order);

Console.WriteLine($"  贪心调度：{string.Join(" -> ", greedy.Order)}");
Console.WriteLine($"    加权完成时间 = {greedyCost}");
Console.WriteLine();
Console.WriteLine($"  暴力枚举全部合法顺序里的最优：{string.Join(" -> ", bestOrder)}");
Console.WriteLine($"    加权完成时间 = {bestCost}");
Console.WriteLine();
Console.WriteLine($"  贪心比最优差 {greedyCost / (double)bestCost:F2} 倍");
Console.WriteLine();
Console.WriteLine("  贪心错在哪？");
Console.WriteLine("    第 1 步可执行的是 {key(5), solo(6)}，贪心选了 solo —— 因为 6 > 5，");
Console.WriteLine("    但它没看到：key 一旦做完，会【同时解锁三个优先级 100 的任务】。");
Console.WriteLine();
Console.WriteLine("  这正是 14.1 节说的「贪心选择性质」不成立：");
Console.WriteLine("    · 最优子结构【成立】（做完 key 之后，剩下的还是同一类问题）；");
Console.WriteLine("    · 但「选当前优先级最高的」【不属于任何全局最优解】。");
Console.WriteLine();

Console.WriteLine("  ⚠️ 而这个问题的全局最优本身也是 NP 难的（带依赖的加权调度）——");
Console.WriteLine("     所以工程上的做法不是「求最优」，而是【接受启发式 + 说清它不最优】。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- 验收检查 ----------

// [2] 每条依赖的「前置」都排在「后继」前面
static bool CheckDependencies(Scheduler s, List<string> order)
{
    var pos = new Dictionary<string, int>();
    for (int i = 0; i < order.Count; i++) pos[order[i]] = i;

    foreach (var job in s.Jobs)
        foreach (var dep in job.DependsOn)
            if (pos[dep] >= pos[job.Name]) return false;
    return true;
}

// [3] 每一步执行的，都必须是「当时可执行集合里优先级最高的」
static (bool Ok, List<string> Violations) CheckGreedyPriority(Scheduler s, List<string> order)
{
    var remaining = new Dictionary<string, int>();
    var dependents = new Dictionary<string, List<string>>();
    foreach (var job in s.Jobs)
    {
        remaining[job.Name] = job.DependsOn.Length;
        foreach (var dep in job.DependsOn)
        {
            if (!dependents.TryGetValue(dep, out var l)) dependents[dep] = l = new List<string>();
            l.Add(job.Name);
        }
    }

    var ready = new List<string>();
    foreach (var job in s.Jobs)
        if (remaining[job.Name] == 0) ready.Add(job.Name);

    var violations = new List<string>();
    foreach (var name in order)
    {
        if (ready.Count == 0) { violations.Add($"{name} 执行时没有可执行任务"); break; }

        int best = 0;
        for (int i = 1; i < ready.Count; i++)
            if (s.PriorityOf(ready[i]) > s.PriorityOf(ready[best])) best = i;

        if (s.PriorityOf(ready[best]) != s.PriorityOf(name))
            violations.Add($"执行 {name}({s.PriorityOf(name)}) 时，{ready[best]}({s.PriorityOf(ready[best])}) 更该先做");

        ready.Remove(name);
        if (dependents.TryGetValue(name, out var nexts))
            foreach (var nx in nexts)
                if (--remaining[nx] == 0) ready.Add(nx);
    }
    return (violations.Count == 0, violations);
}

// ---------- 暴力枚举最优顺序（只用于小规模）----------

static (List<string> Order, int Cost) BruteForceBestOrder(Scheduler s)
{
    var names = s.Jobs.Select(j => j.Name).ToList();
    var deps = names.ToDictionary(n => n, n => s.DependsOf(n));
    int bestCost = int.MaxValue;
    var bestOrder = new List<string>();

    void Rec(List<string> done, List<string> order, int cost)
    {
        if (cost >= bestCost) return;                       // 剪枝
        if (order.Count == names.Count)
        {
            bestCost = cost;
            bestOrder = new List<string>(order);
            return;
        }
        foreach (var n in names)
        {
            if (done.Contains(n)) continue;
            if (!deps[n].All(done.Contains)) continue;      // 前置还没做完
            done.Add(n);
            order.Add(n);
            Rec(done, order, cost + s.PriorityOf(n) * order.Count);
            order.RemoveAt(order.Count - 1);
            done.RemoveAt(done.Count - 1);
        }
    }

    Rec(new List<string>(), new List<string>(), 0);
    return (bestOrder, bestCost);
}

// 加权完成时间：Σ(优先级 × 完成序号)
static int WeightedCompletionCost(Scheduler s, List<string> order)
{
    int total = 0;
    for (int i = 0; i < order.Count; i++) total += s.PriorityOf(order[i]) * (i + 1);
    return total;
}

// ---------- 随机任务图 ----------

// 随机任务图：前 sources 个任务是「源头」（无依赖），其余每个依赖 1~3 个编号更小的任务。
// 只连「小编号 -> 大编号」的边，保证无环 —— 这是 13.1 节生成随机 DAG 的同一个技巧。
static (List<(string Name, int Priority, string[] DependsOn)> Jobs, int EdgeCount) RandomJobGraph(
    Random rng, int count, int sources)
{
    var jobs = new List<(string Name, int Priority, string[] DependsOn)>();
    int edges = 0;

    for (int i = 0; i < count; i++)
    {
        string[] deps;
        if (i < sources)
        {
            deps = [];                       // 源头：没有依赖，一开始就都就绪
        }
        else
        {
            int depCount = rng.Next(1, 4);
            var set = new HashSet<int>();
            while (set.Count < depCount) set.Add(rng.Next(i));   // 只依赖编号更小的
            deps = set.Select(x => $"job{x}").ToArray();
            edges += deps.Length;
        }
        jobs.Add(($"job{i}", rng.Next(1, 1000), deps));
    }
    return (jobs, edges);
}

static double BestMs(Action a, int rounds)
{
    double best = double.MaxValue;
    for (int r = 0; r < rounds; r++)
    {
        var sw = Stopwatch.StartNew();
        a();
        sw.Stop();
        best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
    }
    return best;
}

// ==================== 类型声明（必须放在所有顶级语句之后）====================

// ---------- 调度器 ----------

public enum JobState { Waiting, Ready, Running, Done }

public class Scheduler
{
    private readonly Dictionary<string, int> _priority = new();
    private readonly Dictionary<string, List<string>> _dependents = new();  // 我做完之后，解锁谁
    private readonly Dictionary<string, int> _remaining = new();            // 我还差几个前置
    private readonly Dictionary<string, JobState> _state = new();
    private readonly List<(string Name, int Priority, string[] DependsOn)> _jobs = new();

    public IReadOnlyList<(string Name, int Priority, string[] DependsOn)> Jobs => _jobs;

    // 用线性扫描版还是优先队列版 —— 实验二要对比这两种
    public bool UseHeap { get; set; } = true;

    public void Add(string name, int priority, string[] dependsOn)
    {
        _priority[name] = priority;
        _dependents[name] = new List<string>();
        _remaining[name] = dependsOn.Length;
        _state[name] = JobState.Waiting;
        _jobs.Add((name, priority, dependsOn));

        foreach (var dep in dependsOn)
        {
            if (!_dependents.TryGetValue(dep, out var list))
                _dependents[dep] = list = new List<string>();
            list.Add(name);
        }
    }

    /// <summary>跑一遍调度，返回执行顺序（有环时只返回能执行的部分）。</summary>
    public (List<string> Order, bool HasCycle, List<string> Stuck) Run()
    {
        // 每次都从干净状态开始，方便重复测量
        foreach (var name in _priority.Keys) _state[name] = JobState.Waiting;
        var remaining = new Dictionary<string, int>(_remaining);

        // 就绪集合：两种实现
        var heap = new PriorityQueue<string, (int, string)>();   // 键是 (-优先级, 名字)，保证并列时结果确定
        var list = new List<string>();

        void Push(string name)
        {
            _state[name] = JobState.Ready;
            if (UseHeap) heap.Enqueue(name, (-_priority[name], name));
            else list.Add(name);
        }

        string Pop()
        {
            if (UseHeap) return heap.Dequeue();
            // 线性扫描：找优先级最高的（并列取名字小的）
            int best = 0;
            for (int i = 1; i < list.Count; i++)
                if (_priority[list[i]] > _priority[list[best]]
                    || (_priority[list[i]] == _priority[list[best]] && string.CompareOrdinal(list[i], list[best]) < 0))
                    best = i;
            var name = list[best];
            list.RemoveAt(best);
            return name;
        }

        int ReadyCount() => UseHeap ? heap.Count : list.Count;

        foreach (var (name, _, deps) in _jobs)
            if (deps.Length == 0) Push(name);

        var order = new List<string>();
        while (ReadyCount() > 0)
        {
            var name = Pop();
            _state[name] = JobState.Running;
            order.Add(name);

            foreach (var next in _dependents[name])
            {
                if (!_priority.ContainsKey(next)) continue;      // 依赖了一个不存在的任务
                if (--remaining[next] == 0) Push(next);
            }
            _state[name] = JobState.Done;
        }

        var stuck = _state.Where(kv => kv.Value != JobState.Done).Select(kv => kv.Key).ToList();
        return (order, stuck.Count > 0, stuck);
    }

    public int PriorityOf(string name) => _priority[name];
    public string[] DependsOf(string name) => _jobs.First(j => j.Name == name).DependsOn;
}
