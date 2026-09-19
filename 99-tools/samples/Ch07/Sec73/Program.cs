using System.Diagnostics;

// ==================== 逆序对 ====================

/// <summary>
/// 数一数数组里有多少个「逆序对」。
/// 逆序对：下标 i < j，但 a[i] > a[j] —— 也就是「顺序反了的一对」。
/// 这里用暴力 O(n^2) 实现，只为了教学清晰（生产代码应该用归并排序的 O(n log n) 版本）。
/// </summary>
static long CountInversions(int[] a)
{
    long count = 0;
    for (int i = 0; i < a.Length; i++)
        for (int j = i + 1; j < a.Length; j++)
            if (a[i] > a[j]) count++;
    return count;
}

// ==================== 两种排序（统计"让元素动起来"的次数） ====================

/// <summary>插入排序：统计「移动次数」——每把一个元素往左挪一格算一次。</summary>
static long InsertionSortMoves(int[] source)
{
    int[] a = (int[])source.Clone();
    long moves = 0;

    for (int i = 1; i < a.Length; i++)
    {
        int key = a[i];
        int j = i - 1;
        while (j >= 0 && a[j] > key)
        {
            a[j + 1] = a[j];
            moves++;
            j--;
        }
        a[j + 1] = key;
    }
    return moves;
}

/// <summary>冒泡排序：统计「交换次数」——每交换一对相邻元素算一次。</summary>
static long BubbleSortSwaps(int[] source)
{
    int[] a = (int[])source.Clone();
    long swaps = 0;

    for (int i = 0; i < a.Length - 1; i++)
    {
        bool swapped = false;
        for (int j = 0; j < a.Length - 1 - i; j++)
        {
            if (a[j] > a[j + 1])
            {
                (a[j], a[j + 1]) = (a[j + 1], a[j]);
                swaps++;
                swapped = true;
            }
        }
        if (!swapped) break;
    }
    return swaps;
}

// ==================== 四种输入 ====================

const int N = 5_000;
var rng = new Random(42);

int[] MakeRandom()
{
    var a = new int[N];
    for (int i = 0; i < N; i++) a[i] = rng.Next(0, 1_000_000);
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
    for (int k = 0; k < 10; k++)
    {
        int i = rng.Next(N), j = rng.Next(N);
        (a[i], a[j]) = (a[j], a[i]);
    }
    return a;
}

// ==================== 主流程 ====================

Console.WriteLine("=== 核心实验：逆序对数量 = 插入排序的移动次数 = 冒泡排序的交换次数 ===");
Console.WriteLine();
Console.WriteLine($"  数据量 n = {N:N0}");
Console.WriteLine();
Console.WriteLine($"  {"输入",-10} | {"逆序对数量",16} | {"插入排序移动",16} | {"冒泡排序交换",16} | {"三者一致",10}");
Console.WriteLine(new string('-', 84));

foreach (var (name, data) in new (string, int[])[]
         {
             ("随机", MakeRandom()),
             ("已排序", MakeSorted()),
             ("完全逆序", MakeReversed()),
             ("近乎有序", MakeNearlySorted()),
         })
{
    long inversions = CountInversions(data);
    long insertionMoves = InsertionSortMoves(data);
    long bubbleSwaps = BubbleSortSwaps(data);

    bool allEqual = inversions == insertionMoves && insertionMoves == bubbleSwaps;

    Console.WriteLine($"  {name,-10} | {inversions,16:N0} | {insertionMoves,16:N0} | {bubbleSwaps,16:N0} | {(allEqual ? "是 ✓" : "否 ✗"),10}");
}
Console.WriteLine();

Console.WriteLine("  四个输入下，三个数字【完全相等】—— 这不是巧合，而是必然。");
Console.WriteLine();

// ==================== 为什么必然相等 ====================

Console.WriteLine("=== 为什么必然相等：一次「相邻交换」恰好消除一个逆序对 ===");
Console.WriteLine();

int[] demo = { 5, 2, 4, 1, 3 };
Console.WriteLine($"  用一个小例子看：[{string.Join(", ", demo)}]");
Console.WriteLine();

// 列出所有逆序对
Console.WriteLine("  所有逆序对（i < j 且 a[i] > a[j]）：");
for (int i = 0; i < demo.Length; i++)
    for (int j = i + 1; j < demo.Length; j++)
        if (demo[i] > demo[j])
            Console.WriteLine($"    ({demo[i]}, {demo[j]})   在下标 ({i}, {j})");
Console.WriteLine();

Console.WriteLine("  逐轮冒泡，看每次交换消除的是哪个逆序对：");
Console.WriteLine();

int[] b = (int[])demo.Clone();
int round = 0;
for (int i = 0; i < b.Length - 1; i++)
{
    bool swapped = false;
    round++;
    for (int j = 0; j < b.Length - 1 - i; j++)
    {
        if (b[j] > b[j + 1])
        {
            Console.WriteLine($"    第 {round} 轮：交换 {b[j]} 和 {b[j + 1]}  ->  [{string.Join(", ", b)}]");
            (b[j], b[j + 1]) = (b[j + 1], b[j]);
            swapped = true;
        }
    }
    if (!swapped) break;
}
Console.WriteLine();
Console.WriteLine($"  排序完成: [{string.Join(", ", b)}]");
Console.WriteLine();
Console.WriteLine($"  逆序对总数 = {CountInversions(demo)}，交换次数 = {BubbleSortSwaps(demo)} —— 一模一样。");
Console.WriteLine();

Console.WriteLine("=== 理论：三者的等价性 ===");
Console.WriteLine();
Console.WriteLine("  1. 每次「相邻交换」恰好消除【一个】逆序对。");
Console.WriteLine("     因为交换的是一对相邻的 a[j] > a[j+1]，消除了这一个逆序；");
Console.WriteLine("     而它和其他所有元素的关系都没变（相邻交换不影响别的相对顺序）。");
Console.WriteLine();
Console.WriteLine("  2. 数组有序 <=> 逆序对数量为 0。");
Console.WriteLine("     所以排序的过程，就是不断消除逆序对的过程。");
Console.WriteLine();
Console.WriteLine("  3. 于是：冒泡的交换次数 = 初始逆序对数量。");
Console.WriteLine("        插入的移动次数 = 初始逆序对数量。");
Console.WriteLine("        （插入排序里的一次「移动」等价于冒泡里的一次「交换」）");
Console.WriteLine();

// ==================== 逆序对的量级 ====================

Console.WriteLine("=== 逆序对的量级：为什么随机数组的插入排序是 n^2/4 ===");
Console.WriteLine();

Console.WriteLine($"  {"n",10} | {"逆序对（实测）",18} | {"n^2/4",18} | {"n^2/2",18}");
Console.WriteLine(new string('-', 72));

foreach (int n in new[] { 1_000, 2_000, 4_000 })
{
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = rng.Next(0, int.MaxValue);

    long inv = CountInversions(a);
    Console.WriteLine($"{n,10:N0} | {inv,18:N0} | {(long)n * n / 4,18:N0} | {(long)n * n / 2,18:N0}");
}
Console.WriteLine();
Console.WriteLine("  随机数组的逆序对数量稳定在 n^2/4 附近 —— 恰好是最大可能值 n^2/2 的一半。");
Console.WriteLine();
Console.WriteLine("  直觉：随机取两个元素，它俩「顺序反了」和「顺序对了」的概率各是 1/2。");
Console.WriteLine("        所以期望逆序对数 = (总配对数) × 1/2 = [n(n-1)/2] × 1/2 ≈ n^2/4。");
Console.WriteLine();
Console.WriteLine("  这就解释了 7.1 节实测的现象：插入排序在随机数组上的比较次数");
Console.WriteLine("  正好是冒泡的一半 —— 因为它只需要消除 n^2/4 个逆序对，而不是 n^2/2 个。");
Console.WriteLine();
