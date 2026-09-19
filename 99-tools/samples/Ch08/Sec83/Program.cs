using System.Diagnostics;

// ==================== 堆排序 ====================

/// <summary>
/// 下沉：把 a[i] 往下调整，直到满足「父节点 >= 子节点」（大顶堆）。
/// size 是当前堆的有效范围（堆排序过程中堆会不断缩小）。
/// </summary>
static void SiftDown(int[] a, int i, int size, ref long cmp, ref long swaps)
{
    while (true)
    {
        int largest = i;
        int left = 2 * i + 1;          // 左孩子的下标
        int right = 2 * i + 2;         // 右孩子的下标

        if (left < size)
        {
            cmp++;
            if (a[left] > a[largest]) largest = left;
        }
        if (right < size)
        {
            cmp++;
            if (a[right] > a[largest]) largest = right;
        }

        if (largest == i) break;       // 已经比两个孩子都大了，停

        (a[i], a[largest]) = (a[largest], a[i]);
        swaps++;
        i = largest;                   // 继续往下检查
    }
}

/// <summary>建堆：从最后一个「非叶子节点」开始，依次往前下沉。</summary>
static void BuildHeap(int[] a, ref long cmp, ref long swaps)
{
    for (int i = a.Length / 2 - 1; i >= 0; i--)
        SiftDown(a, i, a.Length, ref cmp, ref swaps);
}

/// <summary>堆排序：先建大顶堆，然后反复把堆顶（最大值）换到末尾。</summary>
static void HeapSort(int[] a, ref long cmp, ref long swaps)
{
    BuildHeap(a, ref cmp, ref swaps);

    for (int end = a.Length - 1; end > 0; end--)
    {
        (a[0], a[end]) = (a[end], a[0]);      // 堆顶（当前最大值）换到末尾
        swaps++;
        SiftDown(a, 0, end, ref cmp, ref swaps);   // 堆缩小一位，重新调整堆顶
    }
}

// 对照用的快速排序（随机化基准）
static int Partition(int[] a, int lo, int hi, ref long cmp, ref long swaps)
{
    int pivot = a[hi];
    int i = lo - 1;
    for (int j = lo; j < hi; j++)
    {
        cmp++;
        if (a[j] <= pivot) { i++; (a[i], a[j]) = (a[j], a[i]); swaps++; }
    }
    (a[i + 1], a[hi]) = (a[hi], a[i + 1]);
    swaps++;
    return i + 1;
}

static void QuickSort(int[] a, int lo, int hi, ref long cmp, ref long swaps, Random rng)
{
    if (lo >= hi) return;
    int r = rng.Next(lo, hi + 1);
    (a[r], a[hi]) = (a[hi], a[r]);
    int p = Partition(a, lo, hi, ref cmp, ref swaps);
    QuickSort(a, lo, p - 1, ref cmp, ref swaps, rng);
    QuickSort(a, p + 1, hi, ref cmp, ref swaps, rng);
}

// ==================== 实验一：建堆过程可视化 ====================

Console.WriteLine("=== 实验一：建堆过程 ===");
Console.WriteLine();

int[] demo = { 4, 10, 3, 5, 1, 8, 9, 2, 7, 6 };
Console.WriteLine($"  原始数组: [{string.Join(", ", demo)}]");
Console.WriteLine("  对应的完全二叉树（下标 i 的孩子是 2i+1 和 2i+2）：");
Console.WriteLine();
Console.WriteLine("                4");
Console.WriteLine("             /     \\");
Console.WriteLine("           10       3");
Console.WriteLine("          /  \\     / \\");
Console.WriteLine("         5    1   8   9");
Console.WriteLine("        / \\  /");
Console.WriteLine("       2  7 6");
Console.WriteLine();
Console.WriteLine("  建堆：从最后一个非叶节点（下标 4，值 1）开始往前，依次下沉。");
Console.WriteLine();

var heapDemo = (int[])demo.Clone();
long c = 0, s = 0;
for (int i = heapDemo.Length / 2 - 1; i >= 0; i--)
{
    Console.Write($"    下标 {i}（值 {heapDemo[i]}）下沉 -> ");
    SiftDown(heapDemo, i, heapDemo.Length, ref c, ref s);
    Console.WriteLine($"[{string.Join(", ", heapDemo)}]");
}
Console.WriteLine();
Console.WriteLine($"  建堆完成，堆顶（最大值）= {heapDemo[0]} ✓");
Console.WriteLine();

// ==================== 实验二：排序过程 ====================

Console.WriteLine("=== 实验二：堆排序的两阶段 ===");
Console.WriteLine();

var sortDemo = (int[])demo.Clone();
long c2 = 0, s2 = 0;
BuildHeap(sortDemo, ref c2, ref s2);
Console.WriteLine($"  阶段 1 —— 建堆后: [{string.Join(", ", sortDemo)}]");
Console.WriteLine($"            堆顶 ({sortDemo[0]}) 是全局最大值");
Console.WriteLine();

Console.WriteLine("  阶段 2 —— 反复「把堆顶换到末尾 + 重新调整堆」：");
for (int end = sortDemo.Length - 1; end >= sortDemo.Length - 4; end--)
{
    (sortDemo[0], sortDemo[end]) = (sortDemo[end], sortDemo[0]);
    SiftDown(sortDemo, 0, end, ref c2, ref s2);
    Console.WriteLine($"    把 {sortDemo[end]} 换到下标 {end}，调整后: [{string.Join(", ", sortDemo)}]");
}
Console.WriteLine("    ...（继续直到堆只剩 1 个元素）");
Console.WriteLine();
Console.WriteLine($"  最终: [{string.Join(", ", sortDemo)}]");
Console.WriteLine();

// ==================== 实验三：性能对比 ====================

const int N = 1_000_000;
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

// 预热
{
    var warm = MakeRandom(1000);
    long wc = 0, ws = 0;
    HeapSort((int[])warm.Clone(), ref wc, ref ws);
}

Console.WriteLine($"=== 实验三：堆排序 vs 手写快排 vs Array.Sort（n = {N:N0}）===");
Console.WriteLine();
Console.WriteLine($"  {"输入",-10} | {"算法",-14} | {"比较次数",16} | {"耗时",10}");
Console.WriteLine(new string('-', 64));

foreach (var (name, data) in new (string, int[])[]
         {
             ("随机", MakeRandom(N)),
             ("已排序", MakeSorted(N)),
         })
{
    // 堆排序
    var h = (int[])data.Clone();
    long hc = 0, hs = 0;
    var sw = Stopwatch.StartNew();
    HeapSort(h, ref hc, ref hs);
    sw.Stop();
    double heapMs = sw.Elapsed.TotalMilliseconds;

    // 手写快排
    var q = (int[])data.Clone();
    long qc = 0, qs = 0;
    sw.Restart();
    QuickSort(q, 0, q.Length - 1, ref qc, ref qs, new Random(1));
    sw.Stop();
    double quickMs = sw.Elapsed.TotalMilliseconds;

    // 官方实现（内省排序）
    var o = (int[])data.Clone();
    sw.Restart();
    Array.Sort(o);
    sw.Stop();
    double officialMs = sw.Elapsed.TotalMilliseconds;

    bool allSame = h.SequenceEqual(q) && q.SequenceEqual(o);

    Console.WriteLine($"  {name,-10} | {"堆排序",-14} | {hc,16:N0} | {heapMs,7:F1} ms");
    Console.WriteLine($"  {name,-10} | {"手写快排",-14} | {qc,16:N0} | {quickMs,7:F1} ms");
    Console.WriteLine($"  {name,-10} | {"Array.Sort",-14} | {"（未统计）",16} | {officialMs,7:F1} ms");
    Console.WriteLine($"  {name,-10} | {"结果一致",-14} | {"",16} | {allSame}");
    Console.WriteLine();
}

// ==================== 实验四：堆排序的独特优势 ====================

Console.WriteLine("=== 实验四：三种 O(n log n) 排序的横向对比 ===");
Console.WriteLine();
Console.WriteLine($"  {"算法",-12} | {"最坏情况",-12} | {"额外空间",-12} | {"稳定",-6} | {"实际速度（随机）",-16}");
Console.WriteLine(new string('-', 74));
Console.WriteLine($"  {"快速排序",-12} | {"O(n^2)",-12} | {"O(log n)",-12} | {"否",-6} | {"最快",-16}");
Console.WriteLine($"  {"归并排序",-12} | {"O(n log n)",-12} | {"O(n)",-12} | {"是",-6} | {"中等（要复制数据）",-16}");
Console.WriteLine($"  {"堆排序",-12} | {"O(n log n)",-12} | {"O(1)",-12} | {"否",-6} | {"较慢（缓存不友好）",-16}");
Console.WriteLine();

Console.WriteLine("  堆排序的独特价值：");
Console.WriteLine("    它是【唯一】同时做到「最坏 O(n log n)」和「额外空间 O(1)」的排序算法。");
Console.WriteLine();
Console.WriteLine("    快速排序：空间省，但最坏 O(n^2)");
Console.WriteLine("    归并排序：最坏有保障，但要 O(n) 额外空间");
Console.WriteLine("    堆排序  ：两个都要 —— 代价是实际速度慢一些");
Console.WriteLine();
Console.WriteLine("  这正是内省排序（8.5 节）在「快排要退化时」切换到堆排序的原因：");
Console.WriteLine("  堆排序不需要额外空间，而且能保证 O(n log n)。");
Console.WriteLine();

Console.WriteLine("  为什么堆排序实际更慢？");
Console.WriteLine("    1. 访问模式跳跃：堆是树形结构，父节点跳到子节点（下标 2i+1）是跳跃访问，");
Console.WriteLine("       缓存命中率远低于快排的顺序扫描；");
Console.WriteLine("    2. 交换次数多：每次「换堆顶到末尾」都是长距离交换。");
Console.WriteLine();
