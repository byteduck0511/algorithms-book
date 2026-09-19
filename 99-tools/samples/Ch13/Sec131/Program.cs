// ==================== 第 13.1 节：拓扑排序与依赖解析 ====================
//
// 运行方式：
//   dotnet run --project ... -c Release             跑实验一到实验四
//   dotnet run --project ... -c Release -- --deep   只跑实验五（深链栈溢出）
//
// 注意：实验五会【故意触发栈溢出】让进程崩溃退出，这是教学内容，不是 bug。
//       所以它被隔离在单独的命令行开关后面，不影响其他实验。

if (args.Contains("--deep"))
{
    RunDeepChainExperiment();
    return;
}

// ==================== 算法实现 ====================

// ---------- Kahn 算法（BFS 版）----------
//
// 三步：
//   1. 统计每个顶点的入度
//   2. 把所有入度为 0 的顶点放进队列（它们没有前置依赖，随时可以做）
//   3. 反复出队：输出它，并把它的所有邻居入度减 1；谁的入度减到 0 谁就入队
//
// 结束条件：队列为空。
// 此时若【已输出的顶点数 < 总顶点数】，说明剩下的顶点入度永远归不了零 —— 有环。

static (List<int> order, List<int> remaining) TopoSortKahn(DiGraph g)
{
    var indeg = new int[g.VertexCount];
    for (int v = 0; v < g.VertexCount; v++)
        foreach (int next in g.Neighbors(v))
            indeg[next]++;                        // 有一条 v -> next，next 就多一个前驱

    var queue = new Queue<int>();
    for (int v = 0; v < g.VertexCount; v++)
        if (indeg[v] == 0) queue.Enqueue(v);

    var order = new List<int>(g.VertexCount);
    while (queue.Count > 0)
    {
        int v = queue.Dequeue();
        order.Add(v);
        foreach (int next in g.Neighbors(v))
            if (--indeg[next] == 0)               // v 做完了，next 少一个前置条件
                queue.Enqueue(next);
    }

    var remaining = new List<int>();
    for (int v = 0; v < g.VertexCount; v++)
        if (indeg[v] > 0) remaining.Add(v);       // 入度没归零 = 还没被排进去

    return (order, remaining);
}

// ---------- 同一个 Kahn 算法，只把队列换成栈 ----------
//
// 唯一改动：Queue.Enqueue/Dequeue  ->  Stack.Push/Pop
// 结果：输出的顺序完全变了（但依然合法）—— 这是本节第一个反预期实测。

static List<int> TopoSortKahnStack(DiGraph g)
{
    var indeg = new int[g.VertexCount];
    for (int v = 0; v < g.VertexCount; v++)
        foreach (int next in g.Neighbors(v))
            indeg[next]++;

    var stack = new Stack<int>();                 // ★ 唯一的区别在这里
    for (int v = 0; v < g.VertexCount; v++)
        if (indeg[v] == 0) stack.Push(v);

    var order = new List<int>(g.VertexCount);
    while (stack.Count > 0)
    {
        int v = stack.Pop();                      // 后进先出
        order.Add(v);
        foreach (int next in g.Neighbors(v))
            if (--indeg[next] == 0) stack.Push(next);
    }
    return order;
}

// ---------- 同一个 Kahn 算法，把容器换成最小堆 ----------
//
// 改动：候选顶点按【编号】排序，编号小（字典序小）的先输出。
// 结果：得到【字典序最小】的那个拓扑序。

static List<int> TopoSortKahnMinFirst(DiGraph g)
{
    var indeg = new int[g.VertexCount];
    for (int v = 0; v < g.VertexCount; v++)
        foreach (int next in g.Neighbors(v))
            indeg[next]++;

    var heap = new PriorityQueue<int, int>();     // ★ 换成优先队列
    for (int v = 0; v < g.VertexCount; v++)
        if (indeg[v] == 0) heap.Enqueue(v, v);    // 优先级 = 顶点编号

    var order = new List<int>(g.VertexCount);
    while (heap.Count > 0)
    {
        int v = heap.Dequeue();
        order.Add(v);
        foreach (int next in g.Neighbors(v))
            if (--indeg[next] == 0) heap.Enqueue(next, next);
    }
    return order;
}

// ---------- DFS 后序解法（递归版）----------
//
// 思路：DFS 到某个顶点时，先递归处理它的所有后继，再记录自己 —— 这叫【后序】。
//       记录下来的序列反转之后，就是拓扑序。
// 为什么？一个顶点只有在【它的所有后继都记录完了】之后才会被记录，
//       所以在后序序列里它排在所有后继的前面；反转后，它排在所有后继的后面。✓

static (List<int> order, List<int> postOrder) TopoSortDfsRecursive(DiGraph g)
{
    var visited = new bool[g.VertexCount];
    var post = new List<int>(g.VertexCount);

    void Dfs(int v)
    {
        visited[v] = true;
        foreach (int next in g.Neighbors(v))
            if (!visited[next]) Dfs(next);
        post.Add(v);                              // ★ 后序：所有后继都处理完才记录自己
    }

    for (int v = 0; v < g.VertexCount; v++)
        if (!visited[v]) Dfs(v);

    var order = new List<int>(post);
    order.Reverse();                              // ★ 逆后序 = 拓扑序
    return (order, post);
}

// ---------- DFS 后序解法（迭代版）----------
//
// 递归版在长链上会栈溢出（见实验五），所以工程上要用显式栈的迭代版。
// 用 (顶点, 下一条待处理邻居的下标) 模拟「递归到一半」的状态。

static List<int> TopoSortDfsIterative(DiGraph g)
{
    var visited = new bool[g.VertexCount];
    var postOrder = new List<int>(g.VertexCount);
    var stack = new Stack<(int v, int idx)>();

    for (int start = 0; start < g.VertexCount; start++)
    {
        if (visited[start]) continue;
        visited[start] = true;
        stack.Push((start, 0));

        while (stack.Count > 0)
        {
            var (v, idx) = stack.Pop();
            var neighbors = g.Neighbors(v);

            if (idx < neighbors.Count)
            {
                stack.Push((v, idx + 1));         // 回来时从下一个邻居继续
                int next = neighbors[idx];
                if (!visited[next])
                {
                    visited[next] = true;
                    stack.Push((next, 0));
                }
            }
            else
            {
                postOrder.Add(v);                 // 邻居全处理完了，记录自己
            }
        }
    }

    var order = new List<int>(postOrder);
    order.Reverse();
    return order;
}

// ---------- 合法性验证器 ----------
//
// 怎么知道一个序列是不是合法拓扑序？
// 检查【每一条边 u -> v】：u 必须排在 v 前面。全部满足才是合法拓扑序。

static bool IsValidTopoOrder(DiGraph g, List<int> order)
{
    if (order.Count != g.VertexCount) return false;

    var pos = new int[g.VertexCount];
    for (int i = 0; i < order.Count; i++) pos[order[i]] = i;

    for (int v = 0; v < g.VertexCount; v++)
        foreach (int next in g.Neighbors(v))
            if (pos[v] >= pos[next]) return false;   // 前驱排到了后面 -> 不合法

    return true;
}

// ---------- 用三色标记精确找出【在环上】的顶点 ----------
//
// 这是 12.4 节三色标记的加强版：维护一条「当前 DFS 路径」（path）。
// 遇到灰色顶点 next 时，path 中从 next 开始到末尾的那一段，正好构成一个环。

static List<int> FindCycleVertices(DiGraph g)
{
    var color = new int[g.VertexCount];           // 0 白 / 1 灰 / 2 黑
    var path = new List<int>();
    var onCycle = new HashSet<int>();

    void Dfs(int v)
    {
        color[v] = 1;                             // 进入：变灰
        path.Add(v);
        foreach (int next in g.Neighbors(v))
        {
            if (color[next] == 1)
            {
                int start = path.IndexOf(next);   // ★ 绕回路径上了 -> 发现环
                for (int i = start; i < path.Count; i++) onCycle.Add(path[i]);
            }
            else if (color[next] == 0)
            {
                Dfs(next);
            }
        }
        path.RemoveAt(path.Count - 1);
        color[v] = 2;                             // 离开：变黑
    }

    for (int v = 0; v < g.VertexCount; v++)
        if (color[v] == 0) Dfs(v);

    return onCycle.OrderBy(x => x).ToList();
}

// ---------- 生成一个随机 DAG ----------
//
// 诀窍：只允许【小编号指向大编号】的边。这样永远不可能有环，
//       因为沿着任何一条路径走，编号都严格递增。

static DiGraph RandomDag(int n, int edgeAttempts, int seed)
{
    var rng = new Random(seed);
    var g = new DiGraph(n);
    for (int i = 0; i < edgeAttempts; i++)
    {
        int a = rng.Next(n), b = rng.Next(n);
        if (a == b) continue;
        if (a > b) (a, b) = (b, a);
        g.AddEdge(a, b);
    }
    return g;
}

// ---------- 实验五：深链上的递归 DFS ----------

static void RunDeepChainExperiment()
{
    const int N = 100_000;

    Console.WriteLine("=== 实验五：递归 DFS 后序在长链 DAG 上会发生什么 ===");
    Console.WriteLine();
    Console.WriteLine($"  构造一条链状 DAG：0 -> 1 -> 2 -> ... -> {N - 1:N0}");
    Console.WriteLine($"  顶点数 = {N:N0}，边数 = {N - 1:N0}，它显然是 DAG（编号严格递增）。");
    Console.WriteLine();

    var chain = new DiGraph(N);
    for (int i = 0; i + 1 < N; i++) chain.AddEdge(i, i + 1);

    Console.WriteLine("  先用 Kahn 算法试（队列 + 循环，不占调用栈）：");
    var (order, _) = TopoSortKahn(chain);
    Console.WriteLine($"    Kahn 完成，顺序长度 = {order.Count:N0} ✓");
    Console.WriteLine();

    Console.WriteLine("  换成迭代版 DFS（显式栈）：");
    var it = TopoSortDfsIterative(chain);
    Console.WriteLine($"    迭代 DFS 完成，顺序长度 = {it.Count:N0} ✓");
    Console.WriteLine();

    Console.WriteLine("  最后用递归版 DFS —— DFS 深度会达到 100,000 层：");
    Console.WriteLine();
    Console.Out.Flush();

    var rec = TopoSortDfsRecursive(chain);
    Console.WriteLine($"    递归 DFS 完成，顺序长度 = {rec.order.Count:N0}");
}

// ==================== 实验一：Kahn 算法跑通课程表 ====================

Console.WriteLine("=== 实验一：Kahn 算法 —— 课程表的修读顺序 ===");
Console.WriteLine();

var course = new DiGraph(8);
string[] name =
[
    "高等数学", "线性代数", "程序设计基础", "数据结构",
    "算法分析", "操作系统", "编译原理", "计算机网络"
];

// 边 u -> v 表示「u 是 v 的先修课」，即 v 依赖 u
course.AddEdge(0, 1);   // 高等数学 -> 线性代数
course.AddEdge(0, 2);   // 高等数学 -> 程序设计基础
course.AddEdge(1, 3);   // 线性代数 -> 数据结构
course.AddEdge(2, 3);   // 程序设计基础 -> 数据结构
course.AddEdge(3, 4);   // 数据结构 -> 算法分析
course.AddEdge(2, 5);   // 程序设计基础 -> 操作系统
course.AddEdge(4, 6);   // 算法分析 -> 编译原理
course.AddEdge(5, 6);   // 操作系统 -> 编译原理
course.AddEdge(5, 7);   // 操作系统 -> 计算机网络

Console.WriteLine("  课程依赖（u -> v 表示 u 是 v 的先修课）：");
Console.WriteLine();
for (int v = 0; v < course.VertexCount; v++)
{
    var preds = course.Predecessors(v);
    if (preds.Count > 0)
        Console.WriteLine($"    {name[v],-12} <- {string.Join("、", preds.Select(p => name[p]))}");
}
Console.WriteLine();

var (kahnOrder, kahnRemaining) = TopoSortKahn(course);
Console.WriteLine("  Kahn 算法（队列版）输出：");
Console.WriteLine($"    [{string.Join(", ", kahnOrder)}]");
Console.WriteLine();
Console.WriteLine("  翻译成一个可行的修读顺序：");
for (int i = 0; i < kahnOrder.Count; i++)
    Console.WriteLine($"    第 {i + 1} 门：{name[kahnOrder[i]]}");
Console.WriteLine();
Console.WriteLine($"  未排入的课程数 = {kahnRemaining.Count}（为 0 表示没有环）");
Console.WriteLine($"  合法性验证：{IsValidTopoOrder(course, kahnOrder)}");
Console.WriteLine();

// ==================== 实验二：同一张图，四种顺序 ====================

Console.WriteLine("=== 实验二：同一张图，四种不同的合法顺序 ===");
Console.WriteLine();

var stackOrder = TopoSortKahnStack(course);
var minOrder = TopoSortKahnMinFirst(course);
var (dfsOrder, postOrder) = TopoSortDfsRecursive(course);
var iterOrder = TopoSortDfsIterative(course);

Console.WriteLine($"  Kahn（队列）：    [{string.Join(", ", kahnOrder)}]");
Console.WriteLine($"  Kahn（栈）：      [{string.Join(", ", stackOrder)}]");
Console.WriteLine($"  Kahn（最小堆）：  [{string.Join(", ", minOrder)}]");
Console.WriteLine($"  DFS 后序取逆：    [{string.Join(", ", dfsOrder)}]");
Console.WriteLine();

Console.WriteLine("  四种做法给出了三种不同的顺序（栈版和 DFS 版撞车了），但都合法：");
Console.WriteLine($"    Kahn（队列）   合法性 = {IsValidTopoOrder(course, kahnOrder)}");
Console.WriteLine($"    Kahn（栈）     合法性 = {IsValidTopoOrder(course, stackOrder)}");
Console.WriteLine($"    Kahn（最小堆） 合法性 = {IsValidTopoOrder(course, minOrder)}");
Console.WriteLine($"    DFS 后序取逆   合法性 = {IsValidTopoOrder(course, dfsOrder)}");
Console.WriteLine();

Console.WriteLine("  DFS 的【原始后序】（没反转，是错的）：");
Console.WriteLine($"    [{string.Join(", ", postOrder)}]");
Console.WriteLine($"    直接当拓扑序用，合法性 = {IsValidTopoOrder(course, postOrder)}  <- 必须反转！");
Console.WriteLine();

Console.WriteLine("  最小堆版为什么正好是 0..7 升序？");
Console.WriteLine("    它每次挑编号最小时刻可做的顶点 —— 这就是【字典序最小】的拓扑序。");
Console.WriteLine("    面试题里说「输出字典序最小的拓扑序」，指的就是这个做法。");
Console.WriteLine();

// 「栈版 Kahn」和「DFS 后序取逆」在上面给出了完全相同的顺序 —— 这是巧合还是规律？
// 拿一批随机小图实测一下。
Console.WriteLine("  注意：上面「栈版 Kahn」和「DFS 后序取逆」的结果一模一样。");
Console.WriteLine("  这是巧合还是普遍规律？拿 200 张随机的 6 顶点 DAG 实测：");
Console.WriteLine();

int sameCount = 0, diffCount = 0;
DiGraph? diffExample = null;
for (int seed = 0; seed < 200; seed++)
{
    var rg = RandomDag(6, 9, seed);
    if (TopoSortKahnStack(rg).SequenceEqual(TopoSortDfsIterative(rg)))
        sameCount++;
    else
    {
        diffCount++;
        diffExample ??= rg;
    }
}

Console.WriteLine($"    两者输出相同：{sameCount,3} / 200 张");
Console.WriteLine($"    两者输出不同：{diffCount,3} / 200 张");

if (diffExample is not null)
{
    var edges = new List<string>();
    for (int v = 0; v < diffExample.VertexCount; v++)
        foreach (int next in diffExample.Neighbors(v))
        {
            string e = $"{v}->{next}";
            if (!edges.Contains(e)) edges.Add(e);     // 随机生成时可能有重复边，去重
        }
    Console.WriteLine();
    Console.WriteLine("    一张不同的例子（6 个顶点）：");
    Console.WriteLine($"      边：{string.Join(", ", edges)}");
    Console.WriteLine($"      栈版 Kahn：   [{string.Join(", ", TopoSortKahnStack(diffExample))}]");
    Console.WriteLine($"      DFS 后序取逆：[{string.Join(", ", TopoSortDfsIterative(diffExample))}]");
}

Console.WriteLine();
Console.WriteLine("  结论：那张课表上的一致是【巧合】。两者都是合法的拓扑序，但顺序不保证相同。");
Console.WriteLine();

// ==================== 实验三：有环时，Kahn 剩下谁？ ====================

Console.WriteLine("=== 实验三：有环时，Kahn 剩下来的顶点是环上的顶点吗？ ===");
Console.WriteLine();

var cyc = new DiGraph(4);
cyc.AddEdge(0, 1);      // 0 -> 1
cyc.AddEdge(1, 2);      // 1 -> 2
cyc.AddEdge(2, 1);      // 2 -> 1   ★ 环：1 <-> 2
cyc.AddEdge(2, 3);      // 2 -> 3   ★ 3 在环外，但它依赖环上的 2

Console.WriteLine("  图的构成：");
Console.WriteLine("        0 ---> 1 <---> 2 ---> 3");
Console.WriteLine("                       ");
Console.WriteLine("    环是 1 <-> 2；顶点 3 【不在环上】，它只是依赖环上的 2。");
Console.WriteLine();

var (cycOrder, cycRemaining) = TopoSortKahn(cyc);
Console.WriteLine($"  Kahn 输出：        [{string.Join(", ", cycOrder)}]");
Console.WriteLine($"  Kahn 剩下来的：    [{string.Join(", ", cycRemaining)}]");
Console.WriteLine();
Console.WriteLine("  【反预期】Kahn 剩下来的是 {1, 2, 3} —— 但真正的环只有 {1, 2}。");
Console.WriteLine("    顶点 3 不在环上，它只是【排在环的下游】，被环挡住了。");
Console.WriteLine();
Console.WriteLine($"  用三色标记精确找出环上的顶点：{{{string.Join(", ", FindCycleVertices(cyc))}}}");
Console.WriteLine();
Console.WriteLine("  结论：Kahn 的「剩余顶点集合」未必等于「环上的顶点集合」，");
Console.WriteLine("        它包含的是【所有被环挡住（含环本身及其下游）】的顶点。");
Console.WriteLine("        要精确指出环在哪，得用 12.4 节的三色标记。");
Console.WriteLine();

// ==================== 实验四：性能对比 ====================

Console.WriteLine("=== 实验四：三种实现的性能对比，10 万顶点的 DAG ===");
Console.WriteLine();

const int N = 100_000;
const int Attempts = 300_000;
var big = RandomDag(N, Attempts, seed: 42);

int edgeCount = 0;
for (int v = 0; v < big.VertexCount; v++) edgeCount += big.Neighbors(v).Count;
Console.WriteLine($"  随机 DAG：顶点 {N:N0}，边 {edgeCount:N0}（编号小的指向编号大的，所以一定是 DAG）");
Console.WriteLine();

// 预热：让 JIT 先把这些方法编译好，否则第一次调用会混入编译开销
TopoSortKahn(big);
TopoSortDfsIterative(big);
TopoSortKahnMinFirst(big);

double BestOf(Func<List<int>> f, int rounds = 10)
{
    double best = double.MaxValue;
    for (int i = 0; i < rounds; i++)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        f();
        sw.Stop();
        best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
    }
    return best;
}

double tKahn = BestOf(() => TopoSortKahn(big).order);
double tDfs = BestOf(() => TopoSortDfsIterative(big));
double tMin = BestOf(() => TopoSortKahnMinFirst(big));

Console.WriteLine($"  Kahn（队列）    ：{tKahn,7:F1} ms   （10 次取最快）");
Console.WriteLine($"  DFS 后序（迭代）：{tDfs,7:F1} ms   （10 次取最快）");
Console.WriteLine($"  Kahn（最小堆）  ：{tMin,7:F1} ms   （10 次取最快）");
Console.WriteLine();
Console.WriteLine($"  最小堆 / 队列 = {tMin / tKahn,5:F2} 倍   <- log n 的代价，稳定可复现");
Console.WriteLine($"  DFS    / 队列 = {tDfs / tKahn,5:F2} 倍   <- 同量级，见下面的噪声实测");
Console.WriteLine();

// 计时噪声：把两个 O(V+E) 的实现各单独跑 8 轮，原样列出每次的数字。
Console.WriteLine("  这两个 O(V+E) 的实现到底谁快？各单独跑 8 轮，原样列出：");
var kahnRuns = new List<double>();
var dfsRuns = new List<double>();
for (int i = 0; i < 8; i++)
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    TopoSortKahn(big);
    sw.Stop();
    kahnRuns.Add(sw.Elapsed.TotalMilliseconds);

    sw.Restart();
    TopoSortDfsIterative(big);
    sw.Stop();
    dfsRuns.Add(sw.Elapsed.TotalMilliseconds);
}
Console.WriteLine($"    Kahn（队列）：{string.Join("  ", kahnRuns.Select(x => x.ToString("F1")))}");
Console.WriteLine($"    DFS 后序    ：{string.Join("  ", dfsRuns.Select(x => x.ToString("F1")))}");
Console.WriteLine($"    Kahn 区间 [{kahnRuns.Min():F1}, {kahnRuns.Max():F1}] ms，"
    + $"DFS 区间 [{dfsRuns.Min():F1}, {dfsRuns.Max():F1}] ms  —— 两者【互相重叠】");
Console.WriteLine();

// 三种结果都验证一遍
var bigKahn = TopoSortKahn(big).order;
var bigDfs = TopoSortDfsIterative(big);
var bigMin = TopoSortKahnMinFirst(big);
Console.WriteLine($"  大图上三者都是合法拓扑序：Kahn = {IsValidTopoOrder(big, bigKahn)}，"
    + $"DFS = {IsValidTopoOrder(big, bigDfs)}，最小堆 = {IsValidTopoOrder(big, bigMin)}");
Console.WriteLine($"  Kahn 与 DFS 输出是否相同：{bigKahn.SequenceEqual(bigDfs)}");
Console.WriteLine();
Console.WriteLine("  三个都是 O(V+E)（最小堆是 O((V+E) log V)），实测量级一致 ——");
Console.WriteLine("  在同量级下，常数因子的差距小到被计时噪声淹没。");
Console.WriteLine();

// ==================== 实验五提示 ====================

Console.WriteLine("=== 实验五（需单独运行）===");
Console.WriteLine();
Console.WriteLine("  递归版 DFS 在长链 DAG 上会栈溢出。它被隔离在单独的命令行开关后：");
Console.WriteLine("    dotnet run -c Release -- --deep");
Console.WriteLine("  运行后进程会崩溃退出，这是教学内容。");

// ==================== 有向图的实现 ====================

/// <summary>有向图：邻接表存储，只加一个方向的边。</summary>
public class DiGraph
{
    private readonly List<int>[] _adj;

    public DiGraph(int n)
    {
        _adj = new List<int>[n];
        for (int i = 0; i < n; i++) _adj[i] = new List<int>();
    }

    public int VertexCount => _adj.Length;

    /// <summary>加一条 from -> to 的边。</summary>
    public void AddEdge(int from, int to) => _adj[from].Add(to);

    /// <summary>v 的邻居（v 指向谁）。</summary>
    public List<int> Neighbors(int v) => _adj[v];

    /// <summary>谁指向 v（v 的前驱）。为了打印依赖关系用，不影响算法。</summary>
    public List<int> Predecessors(int v)
    {
        var result = new List<int>();
        for (int u = 0; u < _adj.Length; u++)
            if (_adj[u].Contains(v)) result.Add(u);
        return result;
    }
}
