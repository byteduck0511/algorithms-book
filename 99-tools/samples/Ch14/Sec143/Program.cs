// ==================== 第 14.3 节：贪心失效的反例 ====================
//
//   实验一：0/1 背包的贪心反例 —— 一个具体例子，以及「差距可以任意大」
//   实验二：随机背包实例上，贪心到底有多准（正确率 + 平均比值 + 最差的一次）
//   实验三：同一个贪心，只把「不能切分」改成「可以切分」，就精确了
//
// 度量说明：全部用「价值」「比值」「正确实例数」，与机器无关，任何电脑结果相同。

// ==================== 实验一：0/1 背包的贪心反例 ====================

Console.WriteLine("=== 实验一：0/1 背包的贪心反例 ===");
Console.WriteLine();
Console.WriteLine("  问题：背包容量固定，每个物品有重量和价值，");
Console.WriteLine("        0/1 背包要求【要么整个拿走、要么不拿】，求能装下的最大总价值。");
Console.WriteLine("  贪心策略：按【单位价值】（价值 / 重量）从高到低拿，装不下就跳过。");
Console.WriteLine();

// ---------- (A) 一个具体的小反例 ----------
Console.WriteLine("  ---------- (A) 一个具体的例子 ----------");
var small = new List<(int w, int v)> { (6, 30), (5, 20), (5, 20) };
string[] names = ["A", "B", "C"];
const int SmallCap = 10;

Console.WriteLine($"      容量 = {SmallCap}");
foreach (var (idx, item) in small.Select((x, i) => (i, x)))
    Console.WriteLine($"      物品 {names[idx]}：重 {item.w,2}，价值 {item.v,2}"
        + $"（单位价值 {item.v / (double)item.w:F1}）");
Console.WriteLine();

int smallGreedy = Knapsack01Greedy(small, SmallCap);
int smallBest = Knapsack01BruteForce(small, SmallCap);
Console.WriteLine($"      贪心（按单位价值）：先拿 A（重 6），剩余容量 4，B 和 C 都装不下 -> 总价值 {smallGreedy}");
Console.WriteLine($"      最优：B + C（重 5+5=10，刚好装满）-> 总价值 {smallBest}");
Console.WriteLine($"      贪心只拿到最优的 {smallGreedy / (double)smallBest:P1}");
Console.WriteLine();

// ---------- (B) 差距可以任意大 ----------
Console.WriteLine("  ---------- (B) 更糟的是：这个差距【可以任意大】----------");
Console.WriteLine();
Console.WriteLine("      构造：容量 W，只放两个物品");
Console.WriteLine("        物品 A：重 1，价值 2      （单位价值 2.0）");
Console.WriteLine("        物品 B：重 W，价值 W      （单位价值 1.0）");
Console.WriteLine();

Console.WriteLine("      W          贪心                最优                比值");
Console.WriteLine("      ---------  ------------------  ------------------  ----------");

foreach (int w in new[] { 100, 1_000, 10_000, 100_000 })
{
    var items = new List<(int w, int v)> { (1, 2), (w, w) };
    int g = Knapsack01Greedy(items, w);
    int b = Knapsack01BruteForce(items, w);
    Console.WriteLine($"      {w,-9:N0}  {g,4}（拿 A 就满了）     {b,10:N0}（拿 B）      {b / (double)g,6:F1} 倍");
}

Console.WriteLine();
Console.WriteLine("      比值 = W / 2，【随 W 无限增长】—— 这在算法里叫「近似比无界」。");
Console.WriteLine("      换句话说：0/1 背包的贪心，不存在「最多差百分之多少」的保证。");
Console.WriteLine();

// ==================== 实验二：随机实例上贪心有多准 ====================

Console.WriteLine("=== 实验二：随机背包实例上，贪心到底有多准？ ===");
Console.WriteLine();

const int Trials = 1000;
const int MaxItems = 15;
var rng = new Random(20260918);

int exactHits = 0;
double ratioSum = 0;
double worstRatio = 1.0;
List<(int w, int v)> worstItems = new();
int worstCap = 0, worstGreedy = 0, worstBest = 0;

for (int t = 0; t < Trials; t++)
{
    int n = rng.Next(6, MaxItems + 1);
    var items = RandomItems(rng, n, maxW: 10, maxV: 30);
    int cap = rng.Next(10, 41);

    int g = Knapsack01Greedy(items, cap);
    int b = Knapsack01BruteForce(items, cap);

    if (g == b) exactHits++;
    double ratio = b == 0 ? 1 : g / (double)b;
    ratioSum += ratio;

    if (ratio < worstRatio)
    {
        worstRatio = ratio;
        worstItems = items;
        worstCap = cap;
        worstGreedy = g;
        worstBest = b;
    }
}

Console.WriteLine($"  {Trials} 个随机实例（每组 6~{MaxItems} 个物品，容量 10~40）：");
Console.WriteLine();
Console.WriteLine($"    贪心恰好等于最优的：{exactHits} / {Trials}（{exactHits / (double)Trials:P1}）");
Console.WriteLine($"    平均比值（贪心 / 最优）：{ratioSum / Trials:F4}");
Console.WriteLine($"    最差的一次：{worstRatio:P1}");
Console.WriteLine();

Console.WriteLine($"  最差那个实例（容量 {worstCap}）：");
foreach (var (idx, item) in worstItems.Select((x, i) => (i, x)))
    Console.WriteLine($"      物品 {idx + 1,2}：重 {item.w,2}，价值 {item.v,2}（单位价值 {item.v / (double)item.w:F2}）");
Console.WriteLine($"    贪心拿到 {worstGreedy}，最优是 {worstBest} —— 只拿到 {worstRatio:P1}");
Console.WriteLine();
Console.WriteLine("  这里有两个容易读错的地方：");
Console.WriteLine();
Console.WriteLine($"    1) 69.9% 这个数字比 14.1 节找零的 2.5% 好看得多 —— 但【口径不同，不能直接比】：");
Console.WriteLine("       14.1 问的是「同一个币制下，所有金额是否都对」；");
Console.WriteLine("       这里问的是「每个实例各自是否恰好命中」—— 实例之间是独立的。");
Console.WriteLine("       换句话说：69.9% 只说明「随机撞上一半以上的概率不低」，不是「多数情况下可靠」。");
Console.WriteLine();
Console.WriteLine("    2) 实验一 (B) 已经证明，只要刻意构造，差距可以大到【没有边】。");
Console.WriteLine("       平均比值再好看，也挡不住一个精心设计的输入。");
Console.WriteLine();

// ==================== 实验三：只改一个字，贪心就精确了 ====================

Console.WriteLine("=== 实验三：同一个贪心，换成【分数背包】就精确了 ===");
Console.WriteLine();
Console.WriteLine("  分数背包：允许把物品【切开来拿】—— 比如「拿这袋米的 3/5」。");
Console.WriteLine("  贪心策略一个字没改：还是按单位价值从高到低拿。");
Console.WriteLine();

// 用小反例展示
Console.WriteLine("  ---------- 回到实验一那个小例子 ----------");
Console.WriteLine($"      容量 = {SmallCap}，物品 A(重6,值30)、B(重5,值20)、C(重5,值20)");
double fracSmall = FractionalGreedy(small, SmallCap);
Console.WriteLine($"      0/1 背包的贪心：{smallGreedy}（拿不了 B+C）");
Console.WriteLine($"      分数背包的贪心：{fracSmall:F1}（拿 A 的 6/6，再拿 B 的 4/5）");
Console.WriteLine($"      而 0/1 背包的【最优】才 {smallBest}");
Console.WriteLine();
Console.WriteLine("      分数背包的答案是「装到再也装不下为止」，比 0/1 的最优还高 ——");
Console.WriteLine("      因为它【允许切分】，约束更松。这不矛盾。");
Console.WriteLine();

// 随机验证：分数背包的贪心值 一定 >= 0/1 背包的最优值
int violations = 0;
double fracGapSum = 0;
for (int t = 0; t < Trials; t++)
{
    int n = rng.Next(6, MaxItems + 1);
    var items = RandomItems(rng, n, maxW: 10, maxV: 30);
    int cap = rng.Next(10, 41);

    double f = FractionalGreedy(items, cap);
    int b = Knapsack01BruteForce(items, cap);

    if (f < b - 1e-9) violations++;
    fracGapSum += (f - b) / b;
}

Console.WriteLine($"  在实验二那 {Trials} 个实例上再验一遍：");
Console.WriteLine($"    分数背包贪心值 < 0/1 背包最优值的实例数：{violations}（应为 0）");
Console.WriteLine($"    分数背包比 0/1 最优平均高出：{fracGapSum / Trials:P1}");
Console.WriteLine();
Console.WriteLine("  为什么「允许切分」这个改动这么关键？");
Console.WriteLine("    不能切分时，一个物品是【全有或全无】的 —— 拿了它，容量就少一截，");
Console.WriteLine("    可能刚好让另外两个【本来能装下】的物品装不下。");
Console.WriteLine();
Console.WriteLine("    能切分时，这种「全有或全无」的跳变消失了：");
Console.WriteLine("    容量永远可以被【用满】，每一步的选择都不影响后面「还能不能继续装」——");
Console.WriteLine("    局部最优于是真的导向全局最优。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- 0/1 背包的贪心：按单位价值降序，装得下就拿 ----------
static int Knapsack01Greedy(List<(int w, int v)> items, int capacity)
{
    int total = 0, rest = capacity;
    foreach (var (w, v) in items.OrderByDescending(x => x.v / (double)x.w))
    {
        if (w > rest) continue;          // 装不下就跳过（不能切分）
        rest -= w;
        total += v;
    }
    return total;
}

// ---------- 0/1 背包的最优：暴力枚举所有子集（只用于小规模验证）----------
// 真正的标准解法是动态规划，那是第 15 章的内容。
static int Knapsack01BruteForce(List<(int w, int v)> items, int capacity)
{
    int n = items.Count, best = 0;
    for (int mask = 0; mask < (1 << n); mask++)
    {
        int wsum = 0, vsum = 0;
        for (int i = 0; i < n; i++)
            if ((mask & (1 << i)) != 0) { wsum += items[i].w; vsum += items[i].v; }

        if (wsum <= capacity && vsum > best) best = vsum;
    }
    return best;
}

// ---------- 分数背包的贪心：按单位价值降序，能拿多少拿多少（含切分）----------
static double FractionalGreedy(List<(int w, int v)> items, int capacity)
{
    double total = 0;
    int rest = capacity;
    foreach (var (w, v) in items.OrderByDescending(x => x.v / (double)x.w))
    {
        if (rest <= 0) break;
        if (w <= rest)
        {
            rest -= w;
            total += v;
        }
        else
        {
            total += rest * (v / (double)w);   // 切开：只拿得下的那部分
            rest = 0;
        }
    }
    return total;
}

static List<(int w, int v)> RandomItems(Random rng, int n, int maxW, int maxV)
{
    var list = new List<(int w, int v)>();
    for (int i = 0; i < n; i++)
        list.Add((rng.Next(1, maxW + 1), rng.Next(1, maxV + 1)));
    return list;
}
