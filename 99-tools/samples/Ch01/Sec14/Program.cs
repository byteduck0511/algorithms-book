using System.Diagnostics;

// ============ 实验一：用 O(n) 额外空间，把区间求和从 O(n) 降到 O(1) ============

// 不预处理：每次查询都要把区间扫一遍，O(n)
static long RangeSumSlow(int[] a, int l, int r)
{
    long s = 0;
    for (int i = l; i <= r; i++) s += a[i];
    return s;
}

// 预处理出前缀和数组，额外空间 O(n)
static long[] BuildPrefixSums(int[] a)
{
    var prefix = new long[a.Length + 1];
    for (int i = 0; i < a.Length; i++)
        prefix[i + 1] = prefix[i] + a[i];
    return prefix;
}

// 有前缀和之后，区间和 = 两个前缀和之差，O(1)
static long RangeSumFast(long[] prefix, int l, int r)
    => prefix[r + 1] - prefix[l];

const int N = 200_000;
const int Queries = 10_000;

var rng = new Random(42);
var data = new int[N];
for (int i = 0; i < N; i++) data[i] = rng.Next(1, 100);

// 先生成好所有查询区间，两种做法查的是同一批区间，结果才可比
var queries = new (int L, int R)[Queries];
for (int i = 0; i < Queries; i++)
{
    int l = rng.Next(0, N);
    int r = rng.Next(l, N);
    queries[i] = (l, r);
}

// ---- 做法一：不预处理 ----
var sw = Stopwatch.StartNew();
long sumSlow = 0;
foreach (var (l, r) in queries) sumSlow += RangeSumSlow(data, l, r);
sw.Stop();
double slowMs = sw.Elapsed.TotalMilliseconds;

// ---- 做法二：先建前缀和，再查询 ----
sw.Restart();
long[] prefix = BuildPrefixSums(data);     // 一次性 O(n) 预处理
long sumFast = 0;
foreach (var (l, r) in queries) sumFast += RangeSumFast(prefix, l, r);
sw.Stop();
double fastMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine("=== 实验一：区间求和，10000 次查询，数据量 20 万 ===");
Console.WriteLine($"  不预处理（每次 O(n)）  : {slowMs,10:F1} ms");
Console.WriteLine($"  前缀和（建表 O(n)，查询 O(1)）: {fastMs,10:F1} ms");
Console.WriteLine($"  结果一致: {sumSlow == sumFast}");
Console.WriteLine($"  额外空间: 一个 long[{N + 1}]，约 {(N + 1) * 8 / 1024.0 / 1024.0:F2} MB");
Console.WriteLine();

// ============ 实验二：List<T> 的扩容策略与摊还代价 ============

Console.WriteLine("=== 实验二：List<int> 的扩容过程 ===");

var list = new List<int>();
int lastCapacity = list.Capacity;
long totalMoved = 0;
int expandCount = 0;

Console.WriteLine($"初始容量: {lastCapacity}");

for (int i = 0; i < 100_000; i++)
{
    list.Add(i);
    if (list.Capacity != lastCapacity)
    {
        totalMoved += list.Count;      // 这次扩容把已有的元素都搬了一遍
        expandCount++;
        // 只打印前 8 次和后 3 次，中间省略
        if (expandCount <= 8 || list.Capacity >= 65_536)
            Console.WriteLine($"  第 {list.Count,7} 个元素时扩容: {lastCapacity,7} -> {list.Capacity,7}");
        lastCapacity = list.Capacity;
    }
}

Console.WriteLine();
Console.WriteLine($"总插入次数      : {list.Count,10}");
Console.WriteLine($"总扩容次数      : {expandCount,10}");
Console.WriteLine($"总搬运元素次数  : {totalMoved,10}");
Console.WriteLine($"平均每次插入搬运: {(double)totalMoved / list.Count,10:F2} 次");
Console.WriteLine();
Console.WriteLine("每次 Add 都要搬运吗？不需要。绝大多数 Add 是 O(1)，");
Console.WriteLine("只有少数几次扩容是 O(n)，平摊下来仍是 O(1) —— 这就是摊还代价。");
