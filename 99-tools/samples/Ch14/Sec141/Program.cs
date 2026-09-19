// ==================== 第 14.1 节：贪心的适用条件 ====================
//
//   实验一：找零问题 —— 同一份贪心代码，只换币制，就从「全对」变成「会错」
//   实验二：随机扫描 1000 种币制，统计贪心有多少种是对的
//   实验三：贪心 vs 动态规划 —— 代码只差一层循环，一个快而可能错，一个慢而一定对
//
// 度量说明：实验一、二用「金额个数 × 币制个数」这样的操作计数与一致率，与机器无关；
//           实验三的性能对比注明机器并取多轮最快值。

// ==================== 实验一：同一份贪心代码，两种币制 ====================

Console.WriteLine("=== 实验一：找零问题 —— 同一份贪心代码，只换币制 ===");
Console.WriteLine();
Console.WriteLine("  问题：用最少的硬币凑出指定金额。");
Console.WriteLine("  贪心策略：每次都拿【不超过剩余金额的最大面额】。");
Console.WriteLine();

int[] usCoins = [25, 10, 5, 1];        // 美元币制
int[] oddCoins = [4, 3, 1];            // 只是把面额换了一下

CompareSystems("币制 A：1, 5, 10, 25（美元的币制）", usCoins, 1, 30);
Console.WriteLine();
CompareSystems("币制 B：1, 3, 4（只换了面额，代码一个字没改）", oddCoins, 1, 30);
Console.WriteLine();

// 单独把 6 元那个反例展开
Console.WriteLine("  把币制 B 里出问题的那个金额展开看：");
int amount6 = 6;
var greedyTrace = GreedyTrace(oddCoins, amount6);
Console.WriteLine($"    金额 = {amount6}");
Console.WriteLine($"      贪心：{TraceStr(greedyTrace)}  ->  {greedyTrace.Count} 枚");
Console.WriteLine($"      最优：3 + 3  ->  2 枚");
Console.WriteLine($"      贪心错在第一步：拿了 4，而最优解根本不碰 4。");
Console.WriteLine();

// ==================== 实验二：随机币制里，贪心有多少能对？ ====================

Console.WriteLine("=== 实验二：随机扫 1000 种币制，贪心有多少种是对的？ ===");
Console.WriteLine();

const int Systems = 1000;
const int MaxAmount = 200;
const int ExtraCoins = 4;             // 除 1 以外再随机取 4 个面额
const int MaxDenom = 30;

var rng = new Random(20260918);
int okSystems = 0;
var failures = new List<(int[] coins, int amount, int greedy, int best)>();

for (int s = 0; s < Systems; s++)
{
    var coins = RandomCoinSystem(rng, ExtraCoins, MaxDenom);
    bool ok = true;

    for (int a = 1; a <= MaxAmount; a++)
    {
        int g = GreedyCount(coins, a);
        int b = DpCount(coins, a);
        if (g != b)
        {
            ok = false;
            if (failures.Count < 3) failures.Add((coins, a, g, b));
            break;                      // 这一种币制已经判定为「贪心会错」
        }
    }
    if (ok) okSystems++;
}

Console.WriteLine($"  随机生成 {Systems} 种币制（每种都含面额 1，另加 {ExtraCoins} 个 2~{MaxDenom} 的随机面额）");
Console.WriteLine($"  对每种币制，检查金额 1 ~ {MaxAmount} 贪心与最优解是否全部一致：");
Console.WriteLine();
Console.WriteLine($"    贪心【全部正确】的币制：{okSystems} 种（{okSystems / (double)Systems:P1}）");
Console.WriteLine($"    贪心【至少错一次】的币制：{Systems - okSystems} 种（{(Systems - okSystems) / (double)Systems:P1}）");
Console.WriteLine();

Console.WriteLine("  几个反例（面额已按降序排列）：");
foreach (var (coins, amount, greedy, best) in failures)
{
    Console.WriteLine($"    币制 {{{string.Join(", ", coins)}}}，金额 {amount}："
        + $"贪心 {greedy} 枚，最优 {best} 枚");
}
Console.WriteLine();
Console.WriteLine("  这个比例值得记住：贪心能用是【特例】，不是常态。");
Console.WriteLine();

// ==================== 实验三：贪心 vs 动态规划 ====================

Console.WriteLine("=== 实验三：贪心 vs 动态规划 —— 代码只差一层循环 ===");
Console.WriteLine();
Console.WriteLine("  两种写法（都求最少硬币数）：");
Console.WriteLine();
Console.WriteLine("  贪心：每个状态只走【一条路】（拿最大的）");
Console.WriteLine("      while (rest > 0) { c = 不超过 rest 的最大面额; rest -= c; }");
Console.WriteLine();
Console.WriteLine("  动态规划：每个状态尝试【所有路】，取最好的");
Console.WriteLine("      for (a = 1..amount) for (c in coins) dp[a] = Min(dp[a], dp[a-c] + 1);");
Console.WriteLine();

// 正确性：拿「已知会错的金额」和「刚好不错的金额」各验一遍
Console.WriteLine("  币制 B（1, 3, 4）上，任取两个金额：");
foreach (int amt in new[] { 6, 300 })
{
    int g = GreedyCount(oddCoins, amt);
    int b = DpCount(oddCoins, amt);
    Console.WriteLine($"    金额 {amt,3}：贪心 = {g,3} 枚，动态规划 = {b,3} 枚，一致 = {g == b}");
}
Console.WriteLine();
Console.WriteLine("  注意 300 那个：贪心这次【没错】。贪心的问题是「有时错」，不是「总是错」——");
Console.WriteLine("  而这恰恰是最危险的：它不会每次都露馅。");
Console.WriteLine();

// 性能：贪心是 O(面额数)，DP 是 O(金额 × 面额数)
int[] perfCoins = [200, 100, 50, 25, 10, 5, 1];   // 降序，供贪心使用
int perfAmount = 200_000;

var sw = System.Diagnostics.Stopwatch.StartNew();
int gc = 0;
for (int i = 0; i < 1000; i++) gc += GreedyCount(perfCoins, perfAmount);
sw.Stop();
double tGreedy = sw.Elapsed.TotalMilliseconds;

sw.Restart();
int dc = 0;
for (int i = 0; i < 1000; i++) dc += DpCount(perfCoins, perfAmount);
sw.Stop();
double tDp = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  性能对比（币制 {string.Join(",", perfCoins)}，金额 {perfAmount:N0}，各跑 1000 次）：");
Console.WriteLine($"    贪心      ：{tGreedy,8:F1} ms   （结果 {gc / 1000} 枚）");
Console.WriteLine($"    动态规划：{tDp,8:F1} ms   （结果 {dc / 1000} 枚）");
Console.WriteLine($"    动态规划是贪心的 {tDp / tGreedy:N0} 倍耗时");
Console.WriteLine();
Console.WriteLine("  复杂度上也能看出来：");
Console.WriteLine("    贪心      O(k)            k = 面额种数，与金额【无关】");
Console.WriteLine("    动态规划 O(amount × k)    金额翻倍，耗时翻倍");
Console.WriteLine();
Console.WriteLine("  所以这一节的结论不是「用 DP 更保险」——");
Console.WriteLine("  而是：【能用贪心时就用贪心，但你必须能证明它这里是对的】。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- 贪心：每次拿不超过剩余金额的最大面额 ----------
// coins 必须按【降序】排列
static int GreedyCount(int[] coins, int amount)
{
    int rest = amount, count = 0;
    foreach (int c in coins)
    {
        while (rest >= c)
        {
            rest -= c;
            count++;
        }
    }
    return rest == 0 ? count : int.MaxValue;    // 凑不出（面额含 1 时不会发生）
}

// 上面那个过程的可读版：把每一步拿了什么记下来
static List<int> GreedyTrace(int[] coins, int amount)
{
    var used = new List<int>();
    int rest = amount;
    foreach (int c in coins)
        while (rest >= c)
        {
            used.Add(c);
            rest -= c;
        }
    return used;
}

static string TraceStr(List<int> used) => string.Join(" + ", used);

// ---------- 动态规划：每个状态尝试所有面额 ----------
static int DpCount(int[] coins, int amount)
{
    var dp = new int[amount + 1];
    Array.Fill(dp, int.MaxValue / 2);
    dp[0] = 0;

    for (int a = 1; a <= amount; a++)
        foreach (int c in coins)
            if (c <= a && dp[a - c] + 1 < dp[a])
                dp[a] = dp[a - c] + 1;

    return dp[amount];
}

// ---------- 随机币制：一定含 1，另加若干个随机面额，返回【降序】 ----------
static int[] RandomCoinSystem(Random rng, int extraCoins, int maxDenom)
{
    var set = new SortedSet<int> { 1 };
    while (set.Count < extraCoins + 1)
        set.Add(rng.Next(2, maxDenom + 1));
    return set.Reverse().ToArray();      // 降序，供贪心使用
}

// ---------- 对比两种币制 ----------
static void CompareSystems(string title, int[] coins, int from, int to)
{
    Console.WriteLine($"  {title}");
    Console.WriteLine($"    面额（降序）：{string.Join(", ", coins)}");
    Console.WriteLine();

    int mismatch = 0;
    int firstBad = -1;
    Console.WriteLine("      金额    贪心枚数    最优枚数    一致");
    Console.WriteLine("      ----    --------    --------    ----");

    for (int a = from; a <= to; a++)
    {
        int g = GreedyCount(coins, a);
        int b = DpCount(coins, a);
        bool same = g == b;
        if (!same) { mismatch++; if (firstBad < 0) firstBad = a; }

        // 打印前几行 + 所有不一致的行，避免刷屏
        if (a <= from + 6 || !same)
        {
            string note = same ? "" : $"   <- 贪心拿的是 {TraceStr(GreedyTrace(coins, a))}";
            Console.WriteLine($"      {a,4}    {g,8}    {b,8}    {(same ? "✓" : "✗")}{note}");
        }
    }

    Console.WriteLine();
    if (mismatch == 0)
        Console.WriteLine($"    {from} ~ {to} 共 {to - from + 1} 个金额，贪心【全部正确】✓");
    else
        Console.WriteLine($"    {from} ~ {to} 共 {to - from + 1} 个金额，贪心有 {mismatch} 个出错"
            + $"（第一个是 {firstBad}）");
}
