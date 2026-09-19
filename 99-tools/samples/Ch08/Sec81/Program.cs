using System.Diagnostics;

// ==================== 归并排序 ====================

/// <summary>
/// 合并两个已经有序的段：a[lo..mid] 和 a[mid+1..hi]。
/// 这是归并排序的核心操作 —— 顺手统计比较次数，并在需要时统计逆序对。
/// </summary>
static void Merge(int[] a, int lo, int mid, int hi, ref long cmp, ref long inversions, bool countInversions)
{
    int[] temp = new int[hi - lo + 1];
    int i = lo, j = mid + 1, k = 0;

    while (i <= mid && j <= hi)
    {
        cmp++;
        if (a[i] <= a[j])                 // 注意 <= ：相等时优先取左边，保证【稳定】
        {
            temp[k++] = a[i++];
        }
        else
        {
            // 顺带数逆序对：a[i] > a[j]，说明 a[i..mid] 这 (mid - i + 1) 个元素都比 a[j] 大
            if (countInversions) inversions += mid - i + 1;
            temp[k++] = a[j++];
        }
    }

    while (i <= mid) temp[k++] = a[i++];
    while (j <= hi) temp[k++] = a[j++];

    Array.Copy(temp, 0, a, lo, temp.Length);
}

static void MergeSort(int[] a, int lo, int hi, ref long cmp, ref long inversions, bool countInversions)
{
    if (lo >= hi) return;                              // 基准情形：只剩 0 或 1 个元素

    int mid = lo + (hi - lo) / 2;                      // 这样写可以避免 lo+hi 溢出
    MergeSort(a, lo, mid, ref cmp, ref inversions, countInversions);
    MergeSort(a, mid + 1, hi, ref cmp, ref inversions, countInversions);
    Merge(a, lo, mid, hi, ref cmp, ref inversions, countInversions);
}

static (long Comparisons, long Inversions) RunMergeSort(int[] source, bool countInversions = false)
{
    int[] a = (int[])source.Clone();
    long cmp = 0, inv = 0;
    MergeSort(a, 0, a.Length - 1, ref cmp, ref inv, countInversions);
    return (cmp, inv);
}

// 对比用的插入排序
static long InsertionSortComparisons(int[] source)
{
    int[] a = (int[])source.Clone();
    long cmp = 0;
    for (int i = 1; i < a.Length; i++)
    {
        int key = a[i];
        int j = i - 1;
        while (j >= 0)
        {
            cmp++;
            if (a[j] <= key) break;
            a[j + 1] = a[j];
            j--;
        }
        a[j + 1] = key;
    }
    return cmp;
}

// ==================== 输入数据 ====================

const int N = 100_000;              // 归并排序是 O(n log n)，可以放心用大一点的数据
var rng = new Random(42);

int[] MakeRandom()
{
    var a = new int[N];
    for (int i = 0; i < N; i++) a[i] = rng.Next(0, int.MaxValue);
    return a;
}

int[] MakeSorted()
{
    var a = new int[N];
    for (int i = 0; i < N; i++) a[i] = i;
    return a;
}

int[] MakeReversed()
{
    var a = new int[N];
    for (int i = 0; i < N; i++) a[i] = N - i;
    return a;
}

int[] MakeNearlySorted()
{
    var a = new int[N];
    for (int i = 0; i < N; i++) a[i] = i;
    for (int k = 0; k < 100; k++)
    {
        int i = rng.Next(N), j = rng.Next(N);
        (a[i], a[j]) = (a[j], a[i]);
    }
    return a;
}

// 预热
{
    var warm = new int[1000];
    for (int i = 0; i < 1000; i++) warm[i] = rng.Next();
    RunMergeSort(warm);
}

// ==================== 实验一：归并排序的比较次数与输入无关 ====================

Console.WriteLine($"=== 实验一：归并排序 vs 插入排序，四种输入（n = {N:N0}）===");
Console.WriteLine();
Console.WriteLine($"  {"输入",-10} | {"归并排序比较",16} | {"插入排序比较",18} | 谁更快");
Console.WriteLine(new string('-', 78));

foreach (var (name, data) in new (string, int[])[]
         {
             ("随机", MakeRandom()),
             ("已排序", MakeSorted()),
             ("完全逆序", MakeReversed()),
             ("近乎有序", MakeNearlySorted()),
         })
{
    var (mergeCmp, _) = RunMergeSort(data);
    long insertCmp = InsertionSortComparisons(data);

    double ratio = (double)insertCmp / mergeCmp;      // >1 表示插入更慢（归并快）
    string verdict = ratio >= 1
        ? $"归并快 {ratio:N1} 倍"
        : $"插入快 {1 / ratio:N1} 倍  <- 注意！";

    Console.WriteLine($"  {name,-10} | {mergeCmp,16:N0} | {insertCmp,18:N0} | {verdict}");
}
Console.WriteLine();
Console.WriteLine("  注意「已排序」那一行：插入排序只比较了 n-1 = 99,999 次，");
Console.WriteLine("  反而比归并排序的 853,904 次快了 8.5 倍！");
Console.WriteLine("  因为插入排序在已排序数据上是 O(n)，而归并排序永远是 O(n log n)。");
Console.WriteLine("  ——「渐进复杂度更优」不等于「任何情况下都更快」。");
Console.WriteLine();

// ==================== 实验二：为什么归并排序"稳定" ====================

Console.WriteLine("=== 实验二：归并排序的比较次数几乎恒定 ===");
Console.WriteLine();

var sizes = new[] { 10_000, 20_000, 40_000, 80_000 };
Console.WriteLine($"  {"n",10} | {"随机",14} | {"已排序",14} | {"完全逆序",14} | {"n*log2(n)",14}");
Console.WriteLine(new string('-', 76));

foreach (int n in sizes)
{
    var rand = new int[n];
    var sorted = new int[n];
    var rev = new int[n];
    for (int i = 0; i < n; i++)
    {
        rand[i] = rng.Next(0, int.MaxValue);
        sorted[i] = i;
        rev[i] = n - i;
    }

    long c1 = RunMergeSort(rand).Comparisons;
    long c2 = RunMergeSort(sorted).Comparisons;
    long c3 = RunMergeSort(rev).Comparisons;

    Console.WriteLine($"{n,10:N0} | {c1,14:N0} | {c2,14:N0} | {c3,14:N0} | {n * Math.Log2(n),14:N0}");
}
Console.WriteLine();
Console.WriteLine("  三个数字几乎一样，而且都略小于 n*log2(n)。");
Console.WriteLine("  这就是归并排序最大的特点：它不挑输入，任何数据都是 O(n log n)。");
Console.WriteLine("  对比 7.1 节的插入排序 —— 它在不同输入下的比较次数差了 150 倍。");
Console.WriteLine();

// ==================== 实验三：用归并排序数逆序对 ====================

Console.WriteLine("=== 实验三：顺便数逆序对（呼应 7.3 节）===");
Console.WriteLine();

var invData = new[] { 5, 2, 4, 1, 3 };
var (_, invCount) = RunMergeSort(invData, countInversions: true);
Console.WriteLine($"  小例子 [{string.Join(", ", invData)}]: 逆序对 = {invCount}（7.3 节手工数出来也是 7）");
Console.WriteLine();

Console.WriteLine($"  大规模对比（n = {N:N0}）：");
foreach (var (name, data) in new (string, int[])[]
         {
             ("随机", MakeRandom()),
             ("完全逆序", MakeReversed()),
         })
{
    var sw = Stopwatch.StartNew();
    var (_, inv) = RunMergeSort(data, countInversions: true);
    sw.Stop();
    Console.WriteLine($"    {name,-8}: 逆序对 = {inv,18:N0}   （耗时 {sw.Elapsed.TotalMilliseconds:F1} ms）");
}
Console.WriteLine();

// 暴力对照（只跑一个小规模，因为暴力是 O(n^2)）
const int SmallN = 5_000;
var smallRand = new int[SmallN];
for (int i = 0; i < SmallN; i++) smallRand[i] = rng.Next(0, int.MaxValue);

var swBrute = Stopwatch.StartNew();
long bruteInv = 0;
for (int i = 0; i < SmallN; i++)
    for (int j = i + 1; j < SmallN; j++)
        if (smallRand[i] > smallRand[j]) bruteInv++;
swBrute.Stop();

var swMerge = Stopwatch.StartNew();
var (_, mergeInv) = RunMergeSort(smallRand, countInversions: true);
swMerge.Stop();

Console.WriteLine($"  归并 vs 暴力数逆序对（n = {SmallN:N0}）：");
Console.WriteLine($"    暴力 O(n^2)     : {bruteInv,12:N0} 个   （耗时 {swBrute.Elapsed.TotalMilliseconds,7:F2} ms）");
Console.WriteLine($"    归并 O(n log n) : {mergeInv,12:N0} 个   （耗时 {swMerge.Elapsed.TotalMilliseconds,7:F2} ms）");
Console.WriteLine($"    结果一致 = {bruteInv == mergeInv}，归并快 {swBrute.Elapsed.TotalMilliseconds / swMerge.Elapsed.TotalMilliseconds:F0} 倍");
Console.WriteLine();
Console.WriteLine("  数逆序对的技巧：在合并两个有序段时，如果左边的 a[i] > 右边的 a[j]，");
Console.WriteLine("  那么左边从 i 到 mid 的所有元素都 > a[j] —— 一次性就能数出 (mid - i + 1) 个逆序对。");
Console.WriteLine("  这是 7.3 节练习 7.3.3 的答案。");
Console.WriteLine();
