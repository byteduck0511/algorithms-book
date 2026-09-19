using System.Runtime.CompilerServices;

// ============ 实验一：探测默认线程栈的安全递归深度（不会崩溃） ============
//
// 关键点：StackOverflowException 在 .NET 里无法被 catch，一旦发生程序直接终止。
// 所以「提前探测」必须用 RuntimeHelpers.EnsureSufficientExecutionStack()，
// 它在栈即将耗尽时抛出【可以捕获】的 InsufficientExecutionStackException。

int maxDepth = 0;
long sink = 0;

void ProbeDepth(int depth)
{
    RuntimeHelpers.EnsureSufficientExecutionStack();   // 栈快满时抛可捕获异常
    maxDepth = depth;
    ProbeDepth(depth + 1);
    sink += depth;      // 这一行让递归调用不再是「尾调用」，防止被 JIT 优化成循环
}

try
{
    ProbeDepth(0);
}
catch (InsufficientExecutionStackException)
{
    // 到达极限，正常退出
}

Console.WriteLine("=== 实验一：默认线程栈的安全递归深度 ===");
Console.WriteLine($"当前环境的最大安全递归深度 ~= {maxDepth:N0}");
Console.WriteLine();

// ============ 实验二：换一个更大的栈，就能递归 20 万层 ============

static int DeepSum(int[] a, int i)
{
    if (i == a.Length) return 0;
    return a[i] + DeepSum(a, i + 1);
}

int[] big = new int[200_000];
Array.Fill(big, 1);

Console.WriteLine("=== 实验二：在 32 MB 栈的线程上递归 20 万层 ===");
Console.WriteLine("（同样深度的递归，在默认栈上必崩，换个更大的栈就没事）");

var worker = new Thread(() =>
{
    int sum = DeepSum(big, 0);
    Console.WriteLine($"  成功！20 万层递归的求和结果 = {sum:N0}");
}, maxStackSize: 32 * 1024 * 1024);     // 32 MB，是默认 1 MB 的 32 倍
worker.Start();
worker.Join();
Console.WriteLine();

// ============ 实验三：真正触发栈溢出 ============
//
// 下面这段代码没有基准情形。运行结果：程序崩溃退出。
// 这是预期行为 —— StackOverflowException 无法被捕获，进程会被直接终止。
// 注意：崩溃提示由 .NET 运行时输出，不是本程序打印的。

Console.WriteLine("=== 实验三：触发栈溢出 ===");
Console.WriteLine("警告：下面的递归函数没有基准情形。");
Console.WriteLine("程序将在几秒后崩溃退出，这是预期行为，不是 bug。");
Console.WriteLine();

InfiniteRecurse(0);

void InfiniteRecurse(int depth)
{
    if (depth % 5_000 == 0)
        Console.WriteLine($"  已递归 {depth,8:N0} 层 ...");
    InfiniteRecurse(depth + 1);        // 永远没有基准情形
}
