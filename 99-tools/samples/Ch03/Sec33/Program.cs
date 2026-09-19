using System.Diagnostics;

// ==================== 相向双指针：有序数组两数之和 ====================

// 暴力：所有配对都试一遍，O(n^2)
static (int, int)? TwoSumBrute(int[] a, int target, ref long ops)
{
    for (int i = 0; i < a.Length; i++)
    {
        for (int j = i + 1; j < a.Length; j++)
        {
            ops++;
            if (a[i] + a[j] == target) return (i, j);
        }
    }
    return null;
}

// 相向双指针：一左一右向中间逼近，O(n)
static (int, int)? TwoSumTwoPointers(int[] a, int target, ref long ops)
{
    int lo = 0, hi = a.Length - 1;
    while (lo < hi)
    {
        ops++;
        int sum = a[lo] + a[hi];
        if (sum == target) return (lo, hi);
        if (sum < target) lo++;      // 和太小 -> 左指针右移，换个大一点的数
        else hi--;                   // 和太大 -> 右指针左移，换个小一点的数
    }
    return null;
}

// ==================== 同向双指针：原地去重 ====================

// 数组已有序，把所有重复元素就地删掉，返回新的长度
static int RemoveDuplicates(int[] a, ref long ops)
{
    if (a.Length == 0) return 0;

    int slow = 0;                         // slow 指向「保留区」的最后一个元素
    for (int fast = 1; fast < a.Length; fast++)
    {
        ops++;
        if (a[fast] != a[slow])           // 发现新值
        {
            slow++;
            a[slow] = a[fast];            // 把它搬到保留区的下一个位置
        }
    }
    return slow + 1;                      // 新长度
}

// ==================== 主流程 ====================

const int N = 20_000;
var sorted = new int[N];
for (int i = 0; i < N; i++) sorted[i] = i * 2;      // 0, 2, 4, ... 全是偶数

Console.WriteLine("=== 实验一：有序数组两数之和（目标不存在，最坏情况）===");
Console.WriteLine($"数据规模: {N:N0}，目标: -1（不存在，两种做法都必须跑完）");
Console.WriteLine();

long bruteOps = 0;
var sw = Stopwatch.StartNew();
var r1 = TwoSumBrute(sorted, -1, ref bruteOps);
sw.Stop();
double bruteMs = sw.Elapsed.TotalMilliseconds;

long tpOps = 0;
sw.Restart();
var r2 = TwoSumTwoPointers(sorted, -1, ref tpOps);
sw.Stop();
double tpMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  暴力双层循环: 比较 {bruteOps,14:N0} 次 | {bruteMs,9:F2} ms | 结果 = {r1?.ToString() ?? "未找到"}");
Console.WriteLine($"  相向双指针  : 比较 {tpOps,14:N0} 次 | {tpMs,9:F2} ms | 结果 = {r2?.ToString() ?? "未找到"}");
Console.WriteLine($"  双指针少比较 {bruteOps / (double)tpOps:N0} 倍");
Console.WriteLine();

// 换一个目标值，结论会反转 —— 这一点非常重要
Console.WriteLine("  换一个目标值（target = 100），结论反转了：");
long o1 = 0, o2 = 0;
var x1 = TwoSumBrute(sorted, 100, ref o1);
var x2 = TwoSumTwoPointers(sorted, 100, ref o2);
Console.WriteLine($"    暴力  : 比较 {o1,8:N0} 次，找到下标 {x1}");
Console.WriteLine($"    双指针: 比较 {o2,8:N0} 次，找到下标 {x2}");
Console.WriteLine();
Console.WriteLine("    暴力这次只比了 50 次就命中（i=0 时很快配上了 a[50]）；");
Console.WriteLine("    双指针反而比了 19,950 次（右边界要从最右边一路退到 50）。");
Console.WriteLine();
Console.WriteLine("    -> 单次跑得快不快，取决于具体的输入数据。");
Console.WriteLine("       真正有意义的是「规模放大时的趋势」—— 见下面的实验三。");
Console.WriteLine();

Console.WriteLine("=== 实验二：原地去重（同向双指针）===");

int[] dup = { 1, 1, 2, 2, 2, 3, 5, 5, 8, 9, 9, 9, 9, 10 };
Console.WriteLine($"  原数组: [{string.Join(", ", dup)}]");

long dedupOps = 0;
int newLen = RemoveDuplicates(dup, ref dedupOps);

Console.WriteLine($"  去重后: [{string.Join(", ", dup.Take(newLen))}]   新长度 = {newLen}");
Console.WriteLine($"  比较次数 = {dedupOps}（元素个数 - 1），空间开销 = O(1)，没有新建任何数组");
Console.WriteLine();

// 用 HashSet 的做法做交叉验证
var expect = dup.Take(newLen).Distinct().Count();
Console.WriteLine($"  交叉验证：去重后无重复 = {expect == newLen}");
Console.WriteLine();

Console.WriteLine("=== 实验三：规模放大后的对比 ===");
Console.WriteLine($"{"数据规模",12} | {"暴力比较次数",16} | {"双指针比较次数",16} | {"倍数",10}");

foreach (int n in new[] { 1_000, 5_000, 20_000 })
{
    var arr = new int[n];
    for (int i = 0; i < n; i++) arr[i] = i * 2;

    long b = 0, t = 0;
    TwoSumBrute(arr, -1, ref b);
    TwoSumTwoPointers(arr, -1, ref t);

    Console.WriteLine($"{n,12:N0} | {b,16:N0} | {t,16:N0} | {(double)b / t,9:N0} 倍");
}

Console.WriteLine();
Console.WriteLine("  暴力是 n(n-1)/2，双指针是 n-1。数据规模涨 20 倍，倍数涨 20 倍 —— 这就是 O(n^2) 与 O(n) 的区别。");
