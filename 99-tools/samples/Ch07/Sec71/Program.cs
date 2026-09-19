using System.Diagnostics;

// ==================== 三种 O(n^2) 排序，都带统计 ====================

// 冒泡排序：相邻两个比较，大的往后冒
static (long Comparisons, long Moves) BubbleSort(int[] source)
{
    int[] a = (int[])source.Clone();
    long cmp = 0, moves = 0;
    int n = a.Length;

    for (int i = 0; i < n - 1; i++)
    {
        bool swapped = false;
        for (int j = 0; j < n - 1 - i; j++)
        {
            cmp++;
            if (a[j] > a[j + 1])
            {
                (a[j], a[j + 1]) = (a[j + 1], a[j]);
                moves++;
                swapped = true;
            }
        }
        if (!swapped) break;      // 优化：一整轮都没交换，说明已经有序
    }
    return (cmp, moves);
}

// 选择排序：每轮找出最小的，和当前位置交换
static (long Comparisons, long Moves) SelectionSort(int[] source)
{
    int[] a = (int[])source.Clone();
    long cmp = 0, moves = 0;
    int n = a.Length;

    for (int i = 0; i < n - 1; i++)
    {
        int minIdx = i;
        for (int j = i + 1; j < n; j++)
        {
            cmp++;
            if (a[j] < a[minIdx]) minIdx = j;
        }
        if (minIdx != i)
        {
            (a[i], a[minIdx]) = (a[minIdx], a[i]);
            moves++;
        }
    }
    return (cmp, moves);
}

// 插入排序：把当前元素插到左边已排好序的部分里
static (long Comparisons, long Moves) InsertionSort(int[] source)
{
    int[] a = (int[])source.Clone();
    long cmp = 0, moves = 0;

    for (int i = 1; i < a.Length; i++)
    {
        int key = a[i];
        int j = i - 1;
        while (j >= 0)
        {
            cmp++;
            if (a[j] <= key) break;
            a[j + 1] = a[j];      // 注意：这是「移动」不是「交换」，一次赋值
            moves++;
            j--;
        }
        a[j + 1] = key;
    }
    return (cmp, moves);
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
    // 随机挑 10 对交换，制造「少量逆序」
    for (int k = 0; k < 10; k++)
    {
        int i = rng.Next(N), j = rng.Next(N);
        (a[i], a[j]) = (a[j], a[i]);
    }
    return a;
}

// 预热（避免 JIT 编译开销污染数据）
foreach (var warm in new[] { MakeRandom(), MakeSorted() })
{
    BubbleSort(warm); SelectionSort(warm); InsertionSort(warm);
}

// ==================== 主流程 ====================

Console.WriteLine($"=== 三种 O(n^2) 排序的对比（数据量 {N:N0}）===");
Console.WriteLine();

var inputs = new (string Name, int[] Data)[]
{
    ("随机", MakeRandom()),
    ("已排序", MakeSorted()),
    ("完全逆序", MakeReversed()),
    ("近乎有序", MakeNearlySorted()),
};

var algorithms = new (string Name, Func<int[], (long, long)> Fn)[]
{
    ("冒泡排序", BubbleSort),
    ("选择排序", SelectionSort),
    ("插入排序", InsertionSort),
};

Console.WriteLine($"  {"输入",-10} | {"算法",-12} | {"比较次数",14} | {"移动次数",12} | {"耗时",10}");
Console.WriteLine(new string('-', 74));

foreach (var (inputName, data) in inputs)
{
    foreach (var (algoName, fn) in algorithms)
    {
        var sw = Stopwatch.StartNew();
        var (cmp, moves) = fn(data);
        sw.Stop();

        Console.WriteLine($"  {inputName,-10} | {algoName,-12} | {cmp,14:N0} | {moves,12:N0} | {sw.Elapsed.TotalMilliseconds,7:F1} ms");
    }
    Console.WriteLine();
}

// ==================== 理论值对照 ====================

Console.WriteLine("=== 理论值对照 ===");
Console.WriteLine();
Console.WriteLine($"  n = {N:N0}");
Console.WriteLine();
Console.WriteLine($"  随机输入下的期望比较次数：");
Console.WriteLine($"    冒泡/选择/插入: 约 n^2/2 = {(long)N * N / 2:N0}");
Console.WriteLine($"    插入排序（随机）: 约 n^2/4 = {(long)N * N / 4:N0}   <- 平均只需移动一半");
Console.WriteLine();
Console.WriteLine($"  已排序输入：");
Console.WriteLine($"    冒泡（带优化）: n-1 = {N - 1:N0} 次比较，0 次移动");
Console.WriteLine($"    插入排序      : n-1 = {N - 1:N0} 次比较，0 次移动");
Console.WriteLine($"    选择排序      : 仍然是 n^2/2 = {(long)N * N / 2:N0} 次比较（它无法提前退出）");
Console.WriteLine();
Console.WriteLine($"  完全逆序输入：");
Console.WriteLine($"    冒泡/插入: n(n-1)/2 = {(long)N * (N - 1) / 2:N0} 次比较，同样次数的移动");
Console.WriteLine($"    选择排序  : 比较同样是 n(n-1)/2 次，但【移动只有约 n/2 = {N / 2:N0} 次】");
Console.WriteLine($"                （每轮最多交换一次；而且前一半轮次换完后数组已经有序，后一半不用再换）");
Console.WriteLine();

Console.WriteLine("=== 三个关键观察 ===");
Console.WriteLine();
Console.WriteLine("  1. 选择排序的比较次数【永远是 n^2/2】，和数据长什么样毫无关系。");
Console.WriteLine("     因为它的内层循环必须完整扫描右边所有元素才能确定最小值。");
Console.WriteLine();
Console.WriteLine("  2. 插入排序在「近乎有序」的数据上表现极好（接近 O(n)），");
Console.WriteLine("     因为每个元素只需要往回移动很少的几步。");
Console.WriteLine();
Console.WriteLine("  3. 选择排序的【移动次数很少】（最多 n-1 次），");
Console.WriteLine("     如果「移动元素」的代价远高于「比较元素」（比如元素是很大的结构体），");
Console.WriteLine("     选择排序反而可能是三种里最合适的。");
