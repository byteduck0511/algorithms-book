using System.Diagnostics;

// ==================== 单调栈：下一个更大元素 ====================
//
// 问题：对每个元素，找出它右边第一个比它大的元素。没有则为 -1。

// 暴力：对每个位置都往右找一遍，O(n^2)
static int[] NextGreaterBrute(int[] a, ref long ops)
{
    var result = new int[a.Length];
    for (int i = 0; i < a.Length; i++)
    {
        result[i] = -1;
        for (int j = i + 1; j < a.Length; j++)
        {
            ops++;
            if (a[j] > a[i]) { result[i] = a[j]; break; }
        }
    }
    return result;
}

// 单调栈：栈里存的是「还没找到答案的下标」，对应的值从栈底到栈顶单调递减，O(n)
static int[] NextGreaterStack(int[] a, ref long ops, bool trace = false)
{
    var result = new int[a.Length];
    Array.Fill(result, -1);
    var stack = new Stack<int>();               // 存下标，不是存值

    for (int i = 0; i < a.Length; i++)
    {
        ops++;
        // 当前元素比栈顶对应的值大 -> 栈顶元素找到答案了
        while (stack.Count > 0 && a[stack.Peek()] < a[i])
        {
            int idx = stack.Pop();
            result[idx] = a[i];
            if (trace) Console.WriteLine($"      a[{idx}]={a[idx]} 的答案 = {a[i]}（在 i={i} 处找到）");
        }
        stack.Push(i);

        if (trace)
            Console.WriteLine($"    i={i} (值 {a[i]}) 入栈 -> 栈内下标: [{string.Join(",", stack.Reverse())}]");
    }
    return result;
}

// ==================== 单调队列：滑动窗口最大值 ====================

// 暴力：每个窗口都重新找最大值，O(n*k)
static int[] MaxWindowBrute(int[] a, int k, ref long ops)
{
    var result = new int[a.Length - k + 1];
    for (int i = 0; i + k <= a.Length; i++)
    {
        int best = int.MinValue;
        for (int j = i; j < i + k; j++)
        {
            ops++;
            if (a[j] > best) best = a[j];
        }
        result[i] = best;
    }
    return result;
}

// 单调队列：队列里存下标，对应的值从队首到队尾单调递减，O(n)
static int[] MaxWindowDeque(int[] a, int k, ref long ops)
{
    var result = new int[a.Length - k + 1];
    var deque = new LinkedList<int>();          // 存下标

    for (int i = 0; i < a.Length; i++)
    {
        ops++;
        // 1. 队首如果已经滑出窗口，踢掉
        while (deque.Count > 0 && deque.First!.Value <= i - k)
            deque.RemoveFirst();

        // 2. 队尾如果比当前值小，它永远不可能成为最大值了，踢掉
        while (deque.Count > 0 && a[deque.Last!.Value] <= a[i])
            deque.RemoveLast();

        deque.AddLast(i);

        // 3. 窗口形成后，队首就是当前窗口的最大值
        if (i >= k - 1)
            result[i - k + 1] = a[deque.First!.Value];
    }
    return result;
}

// ==================== 主流程 ====================

Console.WriteLine("=== 实验一：下一个更大元素 —— 单调栈的执行过程 ===");
Console.WriteLine();

int[] demo = { 3, 1, 4, 1, 5, 9, 2, 6 };
Console.WriteLine($"  数组: [{string.Join(", ", demo)}]");
Console.WriteLine();
long dummy = 0;
int[] demoResult = NextGreaterStack(demo, ref dummy, trace: true);
Console.WriteLine();
Console.WriteLine($"  结果: [{string.Join(", ", demoResult)}]");
Console.WriteLine();

Console.WriteLine("  注意栈里存的是「下标」不是「值」—— 因为最后要把答案写回到 result[下标]。");
Console.WriteLine("  还要注意：每个元素最多入栈一次、出栈一次，所以内层 while 的总执行次数不超过 n。");
Console.WriteLine();

Console.WriteLine("=== 实验二：下一个更大元素 —— 两种输入分布 ===");
Console.WriteLine();

var rng = new Random(42);
const int M = 20_000;

var randomArr = new int[M];
for (int i = 0; i < M; i++) randomArr[i] = rng.Next(0, 1_000_000);

var descArr = new int[M];
for (int i = 0; i < M; i++) descArr[i] = M - i;          // 严格降序

Console.WriteLine($"  数据量都是 {M:N0}，只换输入数据的分布：");
Console.WriteLine();

foreach (var (label, arr) in new[]
         {
             ("随机数组", randomArr),
             ("降序数组（最坏情况）", descArr)
         })
{
    long b = 0, s = 0;

    var swB = Stopwatch.StartNew();
    var r1 = NextGreaterBrute(arr, ref b);
    swB.Stop();

    var swS = Stopwatch.StartNew();
    var r2 = NextGreaterStack(arr, ref s);
    swS.Stop();

    Console.WriteLine($"  {label}:");
    Console.WriteLine($"    暴力  : 比较 {b,14:N0} 次 | {swB.Elapsed.TotalMilliseconds,8:F2} ms");
    Console.WriteLine($"    单调栈: 比较 {s,14:N0} 次 | {swS.Elapsed.TotalMilliseconds,8:F2} ms");
    Console.WriteLine($"    结果一致 = {r1.SequenceEqual(r2)}");
    Console.WriteLine();
}

Console.WriteLine("  看出来了吗：");
Console.WriteLine("    随机数组里，「下一个更大元素」往往就在不远处，暴力几步就 break 了 —— 它运气好。");
Console.WriteLine("    但降序数组里，每个元素右边都没有更大的，暴力必须扫到数组末尾 —— 退化成 O(n^2)。");
Console.WriteLine("    而单调栈在两种输入下都是稳定的 O(n)，一步不多一步不少。");
Console.WriteLine();
Console.WriteLine("  所以单调栈的价值和滑动窗口一样：不是「总是更快」，而是「最坏情况有保障」。");
Console.WriteLine();

Console.WriteLine("=== 实验三：滑动窗口最大值 ===");
Console.WriteLine();

var sw = new Stopwatch();
const int N = 200_000;

int[] windowDemo = { 1, 3, -1, -3, 5, 3, 6, 7 };
const int K = 3;
long d1 = 0, d2 = 0;
var wr1 = MaxWindowBrute(windowDemo, K, ref d1);
var wr2 = MaxWindowDeque(windowDemo, K, ref d2);

Console.WriteLine($"  数组: [{string.Join(", ", windowDemo)}]，窗口大小 k={K}");
Console.WriteLine($"  暴力结果  : [{string.Join(", ", wr1)}]");
Console.WriteLine($"  单调队列  : [{string.Join(", ", wr2)}]");
Console.WriteLine($"  一致: {wr1.SequenceEqual(wr2)}");
Console.WriteLine();

Console.WriteLine($"  {"数据量",12} | {"窗口",6} | {"暴力比较次数",16} | {"单调队列比较",14} | {"倍数",8}");
foreach (int n in new[] { 10_000, 50_000 })
{
    foreach (int k in new[] { 100, 1_000 })
    {
        var arr = new int[n];
        for (int i = 0; i < n; i++) arr[i] = rng.Next(0, 1_000_000);

        long b = 0, q = 0;
        var r1 = MaxWindowBrute(arr, k, ref b);
        var r2 = MaxWindowDeque(arr, k, ref q);

        Console.WriteLine($"{n,12:N0} | {k,6:N0} | {b,16:N0} | {q,14:N0} | {(double)b / q,7:N0} 倍   一致={r1.SequenceEqual(r2)}");
    }
}
Console.WriteLine();

// 大数据的实际耗时
var bigWindowArr = new int[N];
for (int i = 0; i < N; i++) bigWindowArr[i] = rng.Next(0, 1_000_000);

sw.Restart();
long wb = 0;
var wBrute = MaxWindowBrute(bigWindowArr, 1000, ref wb);
sw.Stop();
double wBruteMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long wq = 0;
var wDeque = MaxWindowDeque(bigWindowArr, 1000, ref wq);
sw.Stop();
double wDequeMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  实际耗时（数据量 {N:N0}，窗口 1000）：");
Console.WriteLine($"    暴力    : {wBruteMs,8:F2} ms");
Console.WriteLine($"    单调队列: {wDequeMs,8:F2} ms");
Console.WriteLine($"    快 {wBruteMs / wDequeMs:F0} 倍，结果一致 = {wBrute.SequenceEqual(wDeque)}");
Console.WriteLine();
Console.WriteLine("  单调队列的关键：队列里存的下标对应的值，从队首到队尾严格递减。");
Console.WriteLine("  所以队首永远是当前窗口的最大值，取最大值是 O(1)。");
