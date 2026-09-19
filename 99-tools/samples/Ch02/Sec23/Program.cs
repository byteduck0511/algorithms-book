using System.Diagnostics;

// ==================== 例 1：数组求和 ====================

static int SumRecursive(int[] a, int i)
{
    if (i == a.Length) return 0;          // 基准情形
    return a[i] + SumRecursive(a, i + 1);
}

static int SumIterative(int[] a)
{
    int s = 0;
    foreach (var x in a) s += x;
    return s;
}

// ==================== 例 2：斐波那契 ====================

static long FibRecursive(int n)
{
    if (n <= 1) return n;
    return FibRecursive(n - 1) + FibRecursive(n - 2);
}

static long FibIterative(int n)
{
    if (n <= 1) return n;
    long prev = 0, curr = 1;
    for (int i = 2; i <= n; i++)
    {
        long next = prev + curr;     // 只保留最近两个值，不需要整张表
        prev = curr;
        curr = next;
    }
    return curr;
}

// ==================== 例 3：十进制转二进制 ====================

// 递归版：靠「回程」把余数倒着打印出来
static void ToBinaryRecursive(int n)
{
    if (n == 0) return;
    ToBinaryRecursive(n / 2);
    Console.Write(n % 2);            // 写在递归调用之后 -> 回程执行
}

// 迭代版：用一个显式的栈，把「回程要用的数据」先存起来
static void ToBinaryIterative(int n)
{
    var stack = new Stack<int>();
    while (n > 0)
    {
        stack.Push(n % 2);           // 余数入栈
        n /= 2;
    }
    while (stack.Count > 0)
        Console.Write(stack.Pop());  // 出栈顺序正好是倒序
}

// ==================== 主流程 ====================

const int N = 500_000;
int[] data = new int[N];
Array.Fill(data, 1);

Console.WriteLine($"=== 例 1：{N:N0} 个元素求和，递归 vs 循环 ===");
Console.WriteLine("注意：递归版必须跑在 64 MB 栈的线程上（默认 1 MB 栈在约 1.6 万层就崩）。");
Console.WriteLine("      循环版则完全不需要关心栈的大小。");
Console.WriteLine();

int recursiveSum = 0;
double recursiveMs = 0;

var worker = new Thread(() =>
{
    var sw = Stopwatch.StartNew();
    recursiveSum = SumRecursive(data, 0);
    sw.Stop();
    recursiveMs = sw.Elapsed.TotalMilliseconds;
}, maxStackSize: 64 * 1024 * 1024);
worker.Start();
worker.Join();

var swIter = Stopwatch.StartNew();
int iterativeSum = SumIterative(data);
swIter.Stop();
double iterativeMs = swIter.Elapsed.TotalMilliseconds;

Console.WriteLine($"  递归版（64 MB 栈）: {recursiveMs,8:F2} ms   结果 = {recursiveSum:N0}");
Console.WriteLine($"  循环版             : {iterativeMs,8:F2} ms   结果 = {iterativeSum:N0}");
Console.WriteLine($"  循环快 {recursiveMs / iterativeMs:F1} 倍");
Console.WriteLine();

Console.WriteLine("=== 例 2：斐波那契，递归 vs 迭代 ===");
foreach (int n in new[] { 30, 35, 40 })
{
    var sw = Stopwatch.StartNew();
    long r1 = FibRecursive(n);
    sw.Stop();
    double recMs = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    long r2 = FibIterative(n);
    sw.Stop();
    double iterMs = sw.Elapsed.TotalMilliseconds;

    Console.WriteLine($"  Fib({n,2}) = {r1,-12:N0} | 递归 {recMs,9:F2} ms | 迭代 {iterMs,8:F4} ms | 一致={r1 == r2}");
}
Console.WriteLine();

Console.WriteLine("=== 例 3：十进制转二进制，递归 vs 显式栈 ===");
foreach (int n in new[] { 10, 255, 1024, 123456 })
{
    Console.Write($"  {n,8} -> ");
    ToBinaryRecursive(n);
    Console.Write("  (递归)   ");
    ToBinaryIterative(n);
    Console.WriteLine("  (显式栈)");
}
