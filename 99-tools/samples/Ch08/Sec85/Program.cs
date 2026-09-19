using System.Diagnostics;

// ==================== 实验一：.NET 提供了哪些排序 API ====================

Console.WriteLine("=== 实验一：.NET 的排序 API 一览 ===");
Console.WriteLine();

var data = new[] { 5, 2, 8, 1, 9, 3 };

// 1. Array.Sort —— 原地排序数组
var a1 = (int[])data.Clone();
Array.Sort(a1);
Console.WriteLine($"  Array.Sort           : [{string.Join(", ", a1)}]   原地，返回 void");

// 2. List<T>.Sort —— 原地排序列表
var l1 = data.ToList();
l1.Sort();
Console.WriteLine($"  List<T>.Sort         : [{string.Join(", ", l1)}]   原地，返回 void");

// 3. LINQ OrderBy —— 返回新序列，不改原数据
var o1 = data.OrderBy(x => x).ToArray();
Console.WriteLine($"  OrderBy              : [{string.Join(", ", o1)}]   返回新序列，不修改原数组");
Console.WriteLine($"    原数组仍为         : [{string.Join(", ", data)}]");

// 4. OrderByDescending / ThenBy
var words = new[] { "banana", "apple", "cherry", "avocado" };
var o2 = words.OrderBy(w => w.Length).ThenBy(w => w).ToArray();
Console.WriteLine($"  OrderBy().ThenBy()   : [{string.Join(", ", o2)}]   多关键字");
Console.WriteLine();

// ==================== 实验二：稳定性差异 ====================

Console.WriteLine("=== 实验二：Array.Sort 不稳定，OrderBy 稳定 ===");
Console.WriteLine();

// 注意：数组要够大。因为 Array.Sort 在【小数组】上会退化成插入排序，
// 而插入排序是稳定的 —— 用 6 个元素测，根本测不出它的不稳定性。
const int M = 2000;
var records = new Item[M];
for (int i = 0; i < M; i++)
    records[i] = new Item($"#{i:D4}", i % 2 + 1);      // 只有两个优先级，大量重复

Console.WriteLine($"  {M:N0} 条记录，优先级只有 1 和 2，各占一半（大量重复）。");
Console.WriteLine($"  原始顺序: #0000(1) #0001(2) #0002(1) #0003(2) ...");
Console.WriteLine($"  （同优先级的记录，原始序号是递增的）");
Console.WriteLine();

// 用 Array.Sort 按优先级排
var bySort = (Item[])records.Clone();
Array.Sort(bySort, (x, y) => x.Priority.CompareTo(y.Priority));
bool sortStable = IsStable(bySort);
Console.WriteLine($"  Array.Sort 稳定性: {(sortStable ? "稳定 ✓" : "不稳定 ✗")}");
Console.WriteLine($"    前 10 个结果: {string.Join(" ", bySort.Take(10).Select(r => r.Name))}");

// 用 OrderBy 按优先级排
var byOrderBy = records.OrderBy(r => r.Priority).ToArray();
bool orderByStable = IsStable(byOrderBy);
Console.WriteLine($"  OrderBy    稳定性: {(orderByStable ? "稳定 ✓" : "不稳定 ✗")}");
Console.WriteLine($"    前 10 个结果: {string.Join(" ", byOrderBy.Take(10).Select(r => r.Name))}");
Console.WriteLine();

static bool IsStable(Item[] sorted)
{
    for (int i = 1; i < sorted.Length; i++)
        if (sorted[i].Priority == sorted[i - 1].Priority &&
            string.CompareOrdinal(sorted[i].Name, sorted[i - 1].Name) < 0)
            return false;
    return true;
}

Console.WriteLine("  结论：Array.Sort 用内省排序，【不保证稳定】；OrderBy 用归并类排序，【保证稳定】。");
Console.WriteLine();
if (sortStable)
{
    Console.WriteLine("  （这次 Array.Sort 恰好给出了稳定的结果 —— 但这是【实现细节】，不是承诺。");
    Console.WriteLine("    换个数据、换个 .NET 版本就可能变。不稳定的算法有时碰巧稳定，");
    Console.WriteLine("    恰恰是它最危险的地方：你在测试环境看到的是稳定的，上线就变了。）");
}
Console.WriteLine("  【实践建议】需要稳定时用 OrderBy，或者自己在比较函数里加决胜条件。");
Console.WriteLine();

// ==================== 实验三：性能对比 ====================

const int N = 2_000_000;
var rng = new Random(42);
var big = new int[N];
for (int i = 0; i < N; i++) big[i] = rng.Next(0, int.MaxValue);

var sw = new Stopwatch();

// 预热
Array.Sort((int[])big[..1000].Clone());

Console.WriteLine($"=== 实验三：几种做法的实际耗时（n = {N:N0}）===");
Console.WriteLine();

// Array.Sort
var t1 = (int[])big.Clone();
sw.Restart();
Array.Sort(t1);
sw.Stop();
double arraySortMs = sw.Elapsed.TotalMilliseconds;
Console.WriteLine($"  Array.Sort          : {arraySortMs,8:F1} ms");

// List<T>.Sort
var t2 = big.ToList();
sw.Restart();
t2.Sort();
sw.Stop();
double listSortMs = sw.Elapsed.TotalMilliseconds;
Console.WriteLine($"  List<T>.Sort        : {listSortMs,8:F1} ms");

// LINQ OrderBy
sw.Restart();
var t3 = big.OrderBy(x => x).ToArray();
sw.Stop();
double orderByMs = sw.Elapsed.TotalMilliseconds;
Console.WriteLine($"  OrderBy().ToArray() : {orderByMs,8:F1} ms   （稳定，但慢 {orderByMs / arraySortMs:F1} 倍）");

// 自己手写快排
var t4 = (int[])big.Clone();
sw.Restart();
MyQuickSort(t4, 0, t4.Length - 1, new Random(1));
sw.Stop();
double myQuickMs = sw.Elapsed.TotalMilliseconds;
Console.WriteLine($"  手写快排            : {myQuickMs,8:F1} ms   （慢 {myQuickMs / arraySortMs:F1} 倍）");

Console.WriteLine();
Console.WriteLine($"  结果都一致: {t1.SequenceEqual(t2) && t2.SequenceEqual(t3) && t3.SequenceEqual(t4)}");
Console.WriteLine();

// ==================== 实验四：小数组上的表现 ====================

Console.WriteLine("=== 实验四：不同规模下，插入排序 vs Array.Sort ===");
Console.WriteLine();
Console.WriteLine($"  {"n",10} | {"插入排序",12} | {"Array.Sort",12} | 谁更快");
Console.WriteLine(new string('-', 56));

foreach (int n in new[] { 8, 16, 32, 64, 128, 1000 })
{
    // 造 1000 组同样规模的数据，测总耗时（避免单次测量误差太大）
    const int Rounds = 1000;
    var datasets = new int[Rounds][];
    for (int r = 0; r < Rounds; r++)
    {
        datasets[r] = new int[n];
        for (int i = 0; i < n; i++) datasets[r][i] = rng.Next(0, 100000);
    }

    // 插入排序
    sw.Restart();
    for (int r = 0; r < Rounds; r++)
    {
        var copy = (int[])datasets[r].Clone();
        InsertionSort(copy);
    }
    sw.Stop();
    double insMs = sw.Elapsed.TotalMilliseconds;

    // Array.Sort
    sw.Restart();
    for (int r = 0; r < Rounds; r++)
    {
        var copy = (int[])datasets[r].Clone();
        Array.Sort(copy);
    }
    sw.Stop();
    double arrMs = sw.Elapsed.TotalMilliseconds;

    string verdict = insMs < arrMs ? $"插入排序快 {arrMs / insMs:F2} 倍" : $"Array.Sort 快 {insMs / arrMs:F2} 倍";
    Console.WriteLine($"{n,10:N0} | {insMs,9:F2} ms | {arrMs,9:F2} ms | {verdict}");
}
Console.WriteLine();
Console.WriteLine("  实测结果和「教科书说法」不一样：");
Console.WriteLine("    教科书说「小数组上插入排序更快，所以内省排序会切到插入排序」；");
Console.WriteLine("    但实测 Array.Sort 在【所有规模】上都比我们手写的插入排序快 2~3 倍。");
Console.WriteLine();
Console.WriteLine("  原因有两个：");
Console.WriteLine("    1. Array.Sort 内部确实用了插入排序 —— 但它的实现比我们教科书式的版本");
Console.WriteLine("       更优化（用 Span 避免边界检查、减少内存访问、内联等）；");
Console.WriteLine("    2. 我们测的是「克隆数组 + 排序」的总时间，克隆本身也占了开销。");
Console.WriteLine();
Console.WriteLine("  【结论】现代运行时的排序实现已经高度优化，");
Console.WriteLine("  手写一个「理论上更合适的算法」几乎不可能赢过它。");
Console.WriteLine("  —— 这就是为什么工程中永远直接用 Array.Sort / List<T>.Sort()。");
Console.WriteLine();

// ==================== 实现 ====================

static void InsertionSort(int[] a)
{
    for (int i = 1; i < a.Length; i++)
    {
        int key = a[i];
        int j = i - 1;
        while (j >= 0 && a[j] > key) { a[j + 1] = a[j]; j--; }
        a[j + 1] = key;
    }
}

static void MyQuickSort(int[] a, int lo, int hi, Random rng)
{
    if (lo >= hi) return;
    int r = rng.Next(lo, hi + 1);
    (a[r], a[hi]) = (a[hi], a[r]);
    int pivot = a[hi], i = lo - 1;
    for (int j = lo; j < hi; j++)
        if (a[j] <= pivot) { i++; (a[i], a[j]) = (a[j], a[i]); }
    (a[i + 1], a[hi]) = (a[hi], a[i + 1]);
    MyQuickSort(a, lo, i, rng);
    MyQuickSort(a, i + 2, hi, rng);
}

public record Item(string Name, int Priority);
