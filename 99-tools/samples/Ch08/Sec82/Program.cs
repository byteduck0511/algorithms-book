using System.Diagnostics;

// ==================== 快速排序：三种基准选择策略 ====================

/// <summary>分区（Lomuto 方案）：返回基准元素的最终位置。</summary>
static int Partition(int[] a, int lo, int hi, ref long cmp, ref long swaps)
{
    int pivot = a[hi];                    // 基准取最后一个元素
    int i = lo - 1;                       // i 指向「小于等于基准」区域的末尾

    for (int j = lo; j < hi; j++)
    {
        cmp++;
        if (a[j] <= pivot)
        {
            i++;
            (a[i], a[j]) = (a[j], a[i]);
            swaps++;
        }
    }

    (a[i + 1], a[hi]) = (a[hi], a[i + 1]);   // 把基准放到正确位置
    swaps++;
    return i + 1;
}

// ---------- 策略 1：朴素快排（永远取最后一个元素当基准）----------
static void QuickSortNaive(int[] a, int lo, int hi, ref long cmp, ref long swaps)
{
    if (lo >= hi) return;
    int p = Partition(a, lo, hi, ref cmp, ref swaps);
    QuickSortNaive(a, lo, p - 1, ref cmp, ref swaps);
    QuickSortNaive(a, p + 1, hi, ref cmp, ref swaps);
}

// ---------- 策略 2：随机化基准 ----------
static void QuickSortRandom(int[] a, int lo, int hi, ref long cmp, ref long swaps, Random rng)
{
    if (lo >= hi) return;

    int randomIdx = rng.Next(lo, hi + 1);            // 随机挑一个位置
    (a[randomIdx], a[hi]) = (a[hi], a[randomIdx]);   // 换到末尾，复用上面的 Partition

    int p = Partition(a, lo, hi, ref cmp, ref swaps);
    QuickSortRandom(a, lo, p - 1, ref cmp, ref swaps, rng);
    QuickSortRandom(a, p + 1, hi, ref cmp, ref swaps, rng);
}

// ---------- 策略 3：三数取中 ----------
static void QuickSortMedianOfThree(int[] a, int lo, int hi, ref long cmp, ref long swaps)
{
    if (lo >= hi) return;

    // 取 lo、mid、hi 三个位置的中位数，换到 hi 位置
    int mid = lo + (hi - lo) / 2;
    if (a[mid] < a[lo]) (a[lo], a[mid]) = (a[mid], a[lo]);
    if (a[hi] < a[lo]) (a[lo], a[hi]) = (a[hi], a[lo]);
    if (a[hi] < a[mid]) (a[mid], a[hi]) = (a[hi], a[mid]);
    // 现在 a[lo] <= a[mid] <= a[hi]，把中位数 a[mid] 换到末尾
    (a[mid], a[hi]) = (a[hi], a[mid]);

    int p = Partition(a, lo, hi, ref cmp, ref swaps);
    QuickSortMedianOfThree(a, lo, p - 1, ref cmp, ref swaps);
    QuickSortMedianOfThree(a, p + 1, hi, ref cmp, ref swaps);
}

// ==================== 输入数据 ====================

const int N = 10_000;          // 朴素快排在有序数据上是 O(n^2)，规模不能太大
var rng = new Random(42);

int[] MakeRandom(int n)
{
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = rng.Next(0, int.MaxValue);
    return a;
}

int[] MakeSorted(int n)
{
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = i;
    return a;
}

int[] MakeReversed(int n)
{
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = n - i;
    return a;
}

int[] MakeAllEqual(int n)
{
    var a = new int[n];
    Array.Fill(a, 7);
    return a;
}

// 预热
{
    var warm = MakeRandom(1000);
    long c = 0, s = 0;
    QuickSortNaive((int[])warm.Clone(), 0, warm.Length - 1, ref c, ref s);
}

// ==================== 实验一：朴素快排的致命缺陷 ====================

Console.WriteLine($"=== 实验一：朴素快排（基准取最后一个元素）在 n = {N:N0} 下的表现 ===");
Console.WriteLine();
Console.WriteLine($"  {"输入类型",-12} | {"比较次数",16} | {"交换次数",14} | {"耗时",10} | 相对 n*log2(n)");
Console.WriteLine(new string('-', 84));

double nlogn = N * Math.Log2(N);

foreach (var (name, data) in new (string, int[])[]
         {
             ("随机", MakeRandom(N)),
             ("已排序", MakeSorted(N)),
             ("完全逆序", MakeReversed(N)),
             ("全部相同", MakeAllEqual(N)),
         })
{
    var copy = (int[])data.Clone();
    long cmp = 0, swaps = 0;
    var sw = Stopwatch.StartNew();
    QuickSortNaive(copy, 0, copy.Length - 1, ref cmp, ref swaps);
    sw.Stop();

    Console.WriteLine($"  {name,-12} | {cmp,16:N0} | {swaps,14:N0} | {sw.Elapsed.TotalMilliseconds,7:F1} ms | {cmp / nlogn,10:F2} 倍");
}
Console.WriteLine();
Console.WriteLine($"  参考：n*log2(n) = {nlogn:N0}");
Console.WriteLine();
Console.WriteLine("  看出来了吗：");
Console.WriteLine("    随机数据：约 1.4 倍 nlogn  —— 正常表现");
Console.WriteLine("    已排序  ：约 n^2/2        —— 完全退化！");
Console.WriteLine("    完全逆序：约 n^2/2        —— 完全退化！");
Console.WriteLine();
Console.WriteLine("  原因：基准永远取最后一个元素。当数组已经有序时，");
Console.WriteLine("        每次分区都把数组分成【1 个元素】和【n-1 个元素】两部分，");
Console.WriteLine("        递归深度变成 n，总比较次数退化为 n^2/2。");
Console.WriteLine();
Console.WriteLine("  更危险的是：递归深度 n 还可能导致【栈溢出】（2.2 节）。");
Console.WriteLine($"  n = {N:N0} 时深度 {N:N0}，已经接近 1 MB 线程栈的极限；");
Console.WriteLine("  n 到几十万时，朴素快排会直接崩掉整个进程。");
Console.WriteLine();

// ==================== 实验二：两种改进的效果 ====================

Console.WriteLine("=== 实验二：两种改进策略的效果 ===");
Console.WriteLine();
Console.WriteLine($"  {"输入类型",-12} | {"朴素",16} | {"随机化基准",16} | {"三数取中",16}");
Console.WriteLine(new string('-', 68));

foreach (var (name, data) in new (string, int[])[]
         {
             ("随机", MakeRandom(N)),
             ("已排序", MakeSorted(N)),
             ("完全逆序", MakeReversed(N)),
             ("全部相同", MakeAllEqual(N)),
         })
{
    long c1 = 0, s1 = 0;
    var copy1 = (int[])data.Clone();
    QuickSortNaive(copy1, 0, copy1.Length - 1, ref c1, ref s1);

    long c2 = 0, s2 = 0;
    var copy2 = (int[])data.Clone();
    QuickSortRandom(copy2, 0, copy2.Length - 1, ref c2, ref s2, new Random(12345));

    long c3 = 0, s3 = 0;
    var copy3 = (int[])data.Clone();
    QuickSortMedianOfThree(copy3, 0, copy3.Length - 1, ref c3, ref s3);

    // 验证三种都排对了
    bool ok = copy1.SequenceEqual(copy2) && copy2.SequenceEqual(copy3);

    Console.WriteLine($"  {name,-12} | {c1,16:N0} | {c2,16:N0} | {c3,16:N0}   {(ok ? "" : "结果不一致!")}");
}
Console.WriteLine();
Console.WriteLine("  随机化基准：在所有输入下都稳定在 nlogn 量级 —— 因为对手无法预测基准在哪。");
Console.WriteLine("  三数取中  ：对「已排序/完全逆序」效果极好，但对「全部相同」的数组仍然退化。");
Console.WriteLine();
Console.WriteLine("  注意「全部相同」这一行：三数取中在这种情况下每个元素都相等，");
Console.WriteLine("  取中位数没有意义，分区仍然会极度不平衡。");
Console.WriteLine("  生产级实现还需要【三路分区】（把数组分成 小于/等于/大于 三段），");
Console.WriteLine("  这样「全部相同」的数据会被一次性处理完，不会退化。");
Console.WriteLine();

// ==================== 实验三：规模放大看退化 ====================

Console.WriteLine("=== 实验三：有序数据下的退化有多快 ===");
Console.WriteLine();
Console.WriteLine($"  {"n",10} | {"朴素快排比较次数",20} | {"n^2/2",16} | {"倍数",10}");
Console.WriteLine(new string('-', 64));

foreach (int n in new[] { 2_000, 4_000, 8_000 })
{
    var data = MakeSorted(n);
    long cmp = 0, swaps = 0;
    QuickSortNaive(data, 0, n - 1, ref cmp, ref swaps);

    Console.WriteLine($"{n,10:N0} | {cmp,20:N0} | {(long)n * n / 2,16:N0} | {cmp / ((double)n * n / 2),9:F2} 倍");
}
Console.WriteLine();
Console.WriteLine("  数据量翻倍，比较次数涨约 4 倍 —— 标准的 O(n^2) 特征。");
Console.WriteLine("  而且每一层递归只减少一个元素，递归深度 = n。");
Console.WriteLine();

// ==================== 实验四：速度对比 ====================

Console.WriteLine("=== 实验四：实际耗时对比（随机数据）===");
Console.WriteLine();

const int BigN = 1_000_000;
var bigData = MakeRandom(BigN);
Console.WriteLine($"  数据量 {BigN:N0}，随机数据：");
Console.WriteLine();

long bc = 0, bs = 0;
var bigCopy = (int[])bigData.Clone();
var sw1 = Stopwatch.StartNew();
QuickSortRandom(bigCopy, 0, bigCopy.Length - 1, ref bc, ref bs, new Random(1));
sw1.Stop();
Console.WriteLine($"    随机化快排      : {sw1.Elapsed.TotalMilliseconds,8:F1} ms   （比较 {bc:N0} 次）");

var bigCopy2 = (int[])bigData.Clone();
var sw2 = Stopwatch.StartNew();
Array.Sort(bigCopy2);
sw2.Stop();
Console.WriteLine($"    Array.Sort（.NET）: {sw2.Elapsed.TotalMilliseconds,8:F1} ms   （内省排序）");
Console.WriteLine($"    结果一致 = {bigCopy.SequenceEqual(bigCopy2)}");
Console.WriteLine();
Console.WriteLine("  官方实现比我们手写的快不少 —— 它做了很多我们没做的事：");
Console.WriteLine("    - 小数组切换到插入排序（减少递归开销）");
Console.WriteLine("    - 递归太深时切换到堆排序（防止最坏情况）");
Console.WriteLine("    - 三路分区（处理大量重复元素）");
Console.WriteLine("    - 各种底层优化");
Console.WriteLine("  这就是「内省排序」（Introsort），8.5 节会详细讲。");
Console.WriteLine();
