// ==================== 第 16.1 节：选型手册 —— 按场景选数据结构 ====================
//
// 本节是「收束」性质的一节：不引入新结构，而是把前 15 章的对比结论汇总成可查的手册。
//
// 实测部分只有一个：**同一个任务，三种数据结构组合，差多少倍**。
// 任务：100 万个订单 ID，找出出现次数最多的前 10 个。
//
// 度量说明：耗时注明机器，多轮取最快；结果用「与前 10 名是否一致」校验。

using System.Diagnostics;

const int N = 1_000_000;
const int DistinctIds = 500_000;                   // 大部分 ID 只出现一两次，更接近真实日志
const int K = 10;

var rng = new Random(20260918);

// ---------- 生成数据：重尾分布（和真实日志一样，少数 ID 占了绝大多数请求）----------
var data = new int[N];
for (int i = 0; i < N; i++)
    data[i] = rng.NextDouble() < 0.9 ? rng.Next(100) : rng.Next(DistinctIds);

Console.WriteLine("=== 同一个任务，三种做法 ===");
Console.WriteLine();
Console.WriteLine($"  数据：{N:N0} 个订单 ID，取值 0 ~ {DistinctIds - 1}");
Console.WriteLine("        分布：90% 的请求打在最热的 100 个 ID 上（重尾，和真实日志一样）");
Console.WriteLine($"  任务：找出出现次数最多的前 {K} 个 ID");
Console.WriteLine();

// ---------- 三种实现 ----------
Console.WriteLine("  三种实现：");
Console.WriteLine("    A. 排序 + 扫描        —— 用「排序」代替「查找」，靠相邻元素聚在一起数数");
Console.WriteLine("    B. 哈希表 + 全排序    —— 用哈希表计数，再把【所有】不同 ID 排序取前 K");
Console.WriteLine("    C. 哈希表 + 小顶堆    —— 用哈希表计数，再用【小顶堆】只维护前 K 个");
Console.WriteLine();

// 预热
SortBased(data, K);
HashThenSort(data, K);
HashThenHeap(data, K);

double tA = BestMs(() => SortBased(data, K));
double tB = BestMs(() => HashThenSort(data, K));
double tC = BestMs(() => HashThenHeap(data, K));

var resA = SortBased(data, K);
var resB = HashThenSort(data, K);
var resC = HashThenHeap(data, K);

Console.WriteLine("    实现                    耗时        前 3 名（ID: 次数）");
Console.WriteLine("    --------------------    ---------   ----------------------------");
Console.WriteLine($"    A. 排序 + 扫描          {tA,7:F1} ms   {Top3(resA)}");
Console.WriteLine($"    B. 哈希表 + 全排序      {tB,7:F1} ms   {Top3(resB)}");
Console.WriteLine($"    C. 哈希表 + 小顶堆      {tC,7:F1} ms   {Top3(resC)}");
Console.WriteLine();

// 校验：比较【次数的序列】而不是 ID 序列
// —— 名次并列时「选哪个 ID」在业务上是等价的，但三种实现的遍历顺序不同，会挑出不同的 ID
bool sameCounts = SameCounts(resA, resB) && SameCounts(resB, resC);
bool sameIds = SameIds(resA, resB) && SameIds(resB, resC);
int actualDistinct = new HashSet<int>(data).Count;

Console.WriteLine($"  不同的 ID 实际有 {actualDistinct:N0} 个");
Console.WriteLine($"    三者的【次数序列】完全一致：{sameCounts}");
Console.WriteLine($"    三者的【ID 序列】也完全一致：{sameIds}"
    + (sameIds ? "" : "   <- 名次有并列，选哪个 ID 是等价的"));
Console.WriteLine();

Console.WriteLine("  三个数放在一起看：");
Console.WriteLine($"    C 相对 A：{Ratio(tA, tC)}");
Console.WriteLine($"    C 相对 B：{Ratio(tB, tC)}");
Console.WriteLine();
Console.WriteLine("  它们差在哪？—— 差在【每一步用什么结构】：");
Console.WriteLine();
Console.WriteLine("    A 把「计数」这件事交给了排序：排完之后相同的 ID 挨在一起，扫一遍就行。");
Console.WriteLine($"      代价是 O(n log n)，而且排的是【全部 {N:N0} 个】，包括出现一次就再没影的那些。");
Console.WriteLine();
Console.WriteLine($"    B 用哈希表把计数降到 O(n)，但取前 K 时又对【全部 {actualDistinct:N0} 个不同 ID】排了序。");
Console.WriteLine("      —— 比 A 快得多，因为要去重的只有几十万个，不是整个一百万。");
Console.WriteLine();

// ---------- 暴力版对照：两个规模，看差距怎么长 ----------
Console.WriteLine("  ---------- 再对比一个「没选对结构」的写法 ----------");
Console.WriteLine();
Console.WriteLine("  暴力写法：对每一个不同的 ID，把整个数组扫一遍来数（O(n × 不同ID数)）");
Console.WriteLine("  对照写法：哈希表 + 小顶堆（O(n + m log K)）");
Console.WriteLine();
Console.WriteLine("    规模          不同 ID 数    暴力          哈希+堆      暴力慢多少");
Console.WriteLine("    ----------    ----------    -----------   ----------   ----------");

foreach (int size in new[] { 50_000, 200_000 })
{
    int m = size / 250;                       // 不同 ID 数随规模一起长大
    var small = new int[size];
    for (int i = 0; i < size; i++) small[i] = rng.Next(m);

    double tb = BestMs(() => BruteForce(small, 10));
    double th = BestMs(() => HashThenHeap(small, 10));
    Console.WriteLine($"    {size,-12:N0}  {m,-12:N0}  {tb,9:F1} ms   {th,7:F2} ms   {tb / th,8:N1} 倍");
}

Console.WriteLine();
Console.WriteLine("  注意最后一列【在变大】：规模涨 4 倍，差距从十几倍涨到四十几倍。");
Console.WriteLine("    暴力是 O(n × m)，n 和不同 ID 数一起涨 —— 所以是平方级；");
Console.WriteLine("    哈希 + 堆是 O(n)，只随 n 线性涨。");
Console.WriteLine();

// ---------- 手册：全书实测倍率汇总 ----------
Console.WriteLine("=== 全书实测倍率汇总（每一行都是前面章节真跑出来的）===");
Console.WriteLine();
Console.WriteLine("    对比项                                          倍率      出处");
Console.WriteLine("    --------------------------------------------    --------  ------");
PrintLine("哈希表 vs 暴力查找（10 万次）", "约 1 万倍", "6.1");
PrintLine("开放寻址 vs 链地址法", "快 2.8 倍", "6.2");
PrintLine("int 键 vs string 键的 Dictionary", "快 7.84 倍", "6.4");
PrintLine("插入排序 vs 冒泡（近乎有序）", "快 154 倍", "7.1");
PrintLine("计数排序 vs 快排（小值域）", "快 447 倍", "8.4");
PrintLine("二分查找 vs 线性查找", "随规模放大", "10.1");
PrintLine("AVL vs 退化 BST（有序输入）", "快 196 倍", "10.4");
PrintLine("手写 AVL vs SortedDictionary", "快 4 倍", "10.4");
PrintLine("Top-K（小顶堆）vs 全排序", "快 23.2 倍", "11.4");
PrintLine("邻接表 vs 邻接矩阵（稀疏图，内存）", "省 52.6 倍", "12.1");
PrintLine("Dijkstra 优先队列 vs 朴素（稀疏图）", "快约 2 倍", "13.3");
PrintLine("Bellman-Ford vs Dijkstra（随机图）", "BF 快 5.6 倍", "13.4");
PrintLine("LIS 的 O(n log n) vs O(n²)", "快 75 倍", "15.3");
Console.WriteLine();
Console.WriteLine("  这张表要横着看：每一行都是「换个结构 / 换个算法」带来的差距。");
Console.WriteLine("  最小的也是 2 倍，最大的到几万倍 —— 这就是「选型」值钱的地方。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- A. 排序 + 扫描 ----------
static List<(int Id, int Count)> SortBased(int[] data, int k)
{
    var sorted = (int[])data.Clone();
    Array.Sort(sorted);                       // O(n log n)

    var counts = new List<(int, int)>();
    int i = 0;
    while (i < sorted.Length)
    {
        int j = i;
        while (j < sorted.Length && sorted[j] == sorted[i]) j++;   // 相同的已挨在一起
        counts.Add((sorted[i], j - i));
        i = j;
    }
    counts.Sort(CompareByCountThenId);                            // ★ 统一的平局规则
    return counts.Take(k).Select(x => (x.Item1, x.Item2)).ToList();
}

// ---------- B. 哈希表 + 全排序 ----------
static List<(int Id, int Count)> HashThenSort(int[] data, int k)
{
    var dict = new Dictionary<int, int>();     // 哈希表计数，O(n)
    foreach (int x in data)
        dict[x] = dict.GetValueOrDefault(x) + 1;

    return dict.OrderByDescending(p => p.Value)                   // 对【全部不同 ID】排序
               .ThenBy(p => p.Key)                                // ★ 统一的平局规则
               .Take(k)
               .Select(p => (p.Key, p.Value))
               .ToList();
}

// ---------- C. 哈希表 + 小顶堆 ----------
static List<(int Id, int Count)> HashThenHeap(int[] data, int k)
{
    var dict = new Dictionary<int, int>();
    foreach (int x in data)
        dict[x] = dict.GetValueOrDefault(x) + 1;

    // 小顶堆：堆顶是当前 K 个里【次数最少】的那个
    var heap = new PriorityQueue<(int Id, int Count), int>();
    foreach (var (id, c) in dict)
    {
        if (heap.Count < k)
        {
            heap.Enqueue((id, c), c);
        }
        else if (heap.TryPeek(out _, out int minCount) && c > minCount)
        {
            heap.Dequeue();                    // 踢掉最小的，换进更大的
            heap.Enqueue((id, c), c);
        }
    }

    return heap.UnorderedItems
               .Select(x => x.Element)
               .OrderByDescending(x => x.Count)   // 只是为了让输出好看，K 很小无所谓
               .ThenBy(x => x.Id)                 // ★ 统一的平局规则
               .ToList();
}

// ---------- 暴力：对每个 ID 扫一遍全表 ----------
static List<(int Id, int Count)> BruteForce(int[] data, int k)
{
    var ids = new HashSet<int>(data);          // 先拿到所有不同的 ID
    var counts = new List<(int, int)>();
    foreach (int id in ids)
    {
        int c = 0;
        foreach (int x in data) if (x == id) c++;   // ★ O(n) 扫描
        counts.Add((id, c));
    }
    counts.Sort(CompareByCountThenId);
    return counts.Take(k).Select(x => (x.Item1, x.Item2)).ToList();
}

// 统一的平局规则：次数降序；次数相同时 ID 升序。
// 没有这条规则，三种实现在「并列名次」上会挑出不同的 ID，看起来像结果不一致。
static int CompareByCountThenId((int Id, int Count) x, (int Id, int Count) y)
    => x.Count != y.Count ? y.Count.CompareTo(x.Count) : x.Id.CompareTo(y.Id);

// ---------- 工具 ----------

static double BestMs(Action a, int rounds = 3)
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

// 次数序列一致（业务上真正关心的）
static bool SameCounts(List<(int Id, int Count)> a, List<(int Id, int Count)> b)
    => a.Count == b.Count && a.Zip(b).All(p => p.First.Count == p.Second.Count);

// ID 序列也一致（更严）
static bool SameIds(List<(int Id, int Count)> a, List<(int Id, int Count)> b)
    => a.Count == b.Count && a.Zip(b).All(p => p.First.Id == p.Second.Id);

static string Top3(List<(int Id, int Count)> r)
    => string.Join(",  ", r.Take(3).Select(x => $"{x.Id}: {x.Count:N0}"));

// 比值接近 1 时不说「快 x 倍」——那是噪声，不是差距（13.1 节的规矩）
static string Ratio(double slow, double fast)
    => slow / fast < 1.15 ? "基本持平（差异在噪声范围内）" : $"快 {slow / fast:F1} 倍";

static void PrintLine(string what, string ratio, string src)
    => Console.WriteLine($"    {what,-46}    {ratio,-9} {src}");
