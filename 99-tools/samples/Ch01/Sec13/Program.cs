// 本节同样用「数操作次数」而不是「测时间」，结论与机器无关。

// ---------- 线性查找：统计比较次数 ----------
static int LinearSearchComparisons(int[] data, int target)
{
    int comparisons = 0;
    for (int i = 0; i < data.Length; i++)
    {
        comparisons++;
        if (data[i] == target)
            return comparisons;      // 找到就停，剩下的元素不用看了
    }
    return comparisons;              // 找不到，必须比完全部 n 个
}

// ---------- 插入排序：统计比较次数 ----------
static long InsertionSortComparisons(int[] source)
{
    int[] a = (int[])source.Clone();
    long comparisons = 0;

    for (int i = 1; i < a.Length; i++)
    {
        int key = a[i];
        int j = i - 1;
        while (j >= 0)
        {
            comparisons++;           // 每轮循环先做一次 a[j] > key 的判断
            if (a[j] <= key)
                break;               // 找到插入位置，停止
            a[j + 1] = a[j];
            j--;
        }
        a[j + 1] = key;
    }
    return comparisons;
}

const int N = 2_000;
var rng = new Random(42);

var sorted = new int[N];
for (int i = 0; i < N; i++) sorted[i] = i;                    // 已有序

var reversed = new int[N];
for (int i = 0; i < N; i++) reversed[i] = N - i;              // 完全逆序

var random = new int[N];
for (int i = 0; i < N; i++) random[i] = rng.Next(0, N);       // 随机

Console.WriteLine($"=== 线性查找的比较次数（n = {N}）===");
int[] probe = new int[N];
for (int i = 0; i < N; i++) probe[i] = i;
Console.WriteLine($"  目标在第 1 位    : {LinearSearchComparisons(probe, 0),6} 次");
Console.WriteLine($"  目标在正中间     : {LinearSearchComparisons(probe, N / 2),6} 次");
Console.WriteLine($"  目标在最后一位   : {LinearSearchComparisons(probe, N - 1),6} 次");
Console.WriteLine($"  目标不存在       : {LinearSearchComparisons(probe, -1),6} 次");

Console.WriteLine();
Console.WriteLine($"=== 插入排序的比较次数（n = {N}）===");
Console.WriteLine($"  已有序数组（最好）: {InsertionSortComparisons(sorted),10} 次");
Console.WriteLine($"  随机数组（平均）  : {InsertionSortComparisons(random),10} 次");
Console.WriteLine($"  完全逆序（最坏）  : {InsertionSortComparisons(reversed),10} 次");

Console.WriteLine();
Console.WriteLine("理论值对照：");
Console.WriteLine($"  最好 n-1           = {N - 1}");
Console.WriteLine($"  平均 n^2/4         = {(long)N * N / 4}");
Console.WriteLine($"  最坏 n(n-1)/2      = {(long)N * (N - 1) / 2}");
