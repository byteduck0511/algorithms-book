// ==================== 第 15.1 节：从暴力递归到记忆化 ====================
//
//   实验一：暴力递归有多慢 —— 逐个子问题统计调用次数，看清「重叠」有多严重
//   实验二：加一张表（记忆化）之后，同样的递归变成什么样
//   实验三：自顶向下（记忆化）vs 自底向上（递推）—— 三个版本的正面对比
//
// 度量说明：调用次数是【操作计数】，与机器无关；耗时与分配字节数注明机器，
//           各跑多轮取最快值。

using System.Diagnostics;

const int N = 33;                 // 与 2.1 节的 Fib(35) 同量级，但跑得快些

// ==================== 实验一：暴力递归有多慢 ====================

Console.WriteLine("=== 实验一：暴力递归有多慢 ===");
Console.WriteLine();

var callCount = new long[N + 1];

long NaiveFib(int n)
{
    callCount[n]++;
    if (n <= 1) return n;
    return NaiveFib(n - 1) + NaiveFib(n - 2);
}

var sw = Stopwatch.StartNew();
long naiveResult = NaiveFib(N);
sw.Stop();

long totalCalls = callCount.Sum();
int distinctSubproblems = callCount.Count(c => c > 0);

Console.WriteLine($"  朴素递归 Fib({N}) = {naiveResult:N0}");
Console.WriteLine($"    总调用次数：{totalCalls:N0}");
Console.WriteLine($"    不同的子问题：{distinctSubproblems} 个（就是 Fib(0) ~ Fib({N})）");
Console.WriteLine($"    耗时：{sw.Elapsed.TotalMilliseconds:F1} ms");
Console.WriteLine();

// 找出被调用最多的几个子问题
Console.WriteLine("  每个子问题被调用了多少次（挑几个看）：");
Console.WriteLine();
Console.WriteLine("      子问题      调用次数        占总调用");
Console.WriteLine("      --------    ------------    --------");
foreach (int k in new[] { 1, 2, 5, 10, 20, N })
{
    Console.WriteLine($"      Fib({k,-3})    {callCount[k],12:N0}    {callCount[k] / (double)totalCalls,7:P1}");
}
Console.WriteLine();

long maxCalls = callCount.Max();
int maxK = Array.IndexOf(callCount, maxCalls);
Console.WriteLine($"  调用最多的子问题：Fib({maxK})，被调用了 {maxCalls:N0} 次");
Console.WriteLine($"    —— 而它只需要算【一次】。");
Console.WriteLine();

Console.WriteLine($"  {totalCalls:N0} 次调用 vs {distinctSubproblems} 个不同子问题：");
Console.WriteLine($"    平均每个子问题被算了 {totalCalls / (double)distinctSubproblems:N0} 次。");
Console.WriteLine();

// ==================== 实验二：加一张表 ====================

Console.WriteLine("=== 实验二：加一张表（记忆化）===");
Console.WriteLine();

var memo = new long[N + 1];
Array.Fill(memo, -1);
long memoFnCalls = 0, memoComputes = 0;

long MemoFib(int n)
{
    memoFnCalls++;
    if (n <= 1) return n;
    if (memo[n] != -1) return memo[n];        // ★ 命中缓存，直接返回
    memoComputes++;
    return memo[n] = MemoFib(n - 1) + MemoFib(n - 2);
}

sw.Restart();
long memoResult = MemoFib(N);
sw.Stop();

Console.WriteLine($"  记忆化递归 Fib({N}) = {memoResult:N0}（和朴素版一致：{memoResult == naiveResult}）");
Console.WriteLine($"    函数被调用次数：{memoFnCalls:N0}");
Console.WriteLine($"    其中【真正计算】的次数：{memoComputes:N0}");
Console.WriteLine($"    耗时：{sw.Elapsed.TotalMilliseconds:F3} ms");
Console.WriteLine();

Console.WriteLine("  三个数字对比：");
Console.WriteLine($"    总调用次数：  朴素 {totalCalls,14:N0}   ->  记忆化 {memoFnCalls,8:N0}"
    + $"   （{totalCalls / (double)memoFnCalls:N0} 分之 1）");
Console.WriteLine($"    实际计算次数：朴素 {totalCalls,14:N0}   ->  记忆化 {memoComputes,8:N0}"
    + $"   （{totalCalls / (double)memoComputes:N0} 分之 1）");
Console.WriteLine();
Console.WriteLine("  记忆化只多了两行：算之前先查表、算完把结果写进表。");
Console.WriteLine();

// ==================== 实验三：自顶向下 vs 自底向上 ====================

Console.WriteLine("=== 实验三：自顶向下（记忆化）vs 自底向上（递推）===");
Console.WriteLine();

const int BigN = 90;              // long 的极限是 Fib(92)，取 90 留点余量
const int Repeats = 10_000;       // 单次太快，重复多次才测得出差别

long MemoFibBig(int n)
{
    var cache = new long[n + 1];
    Array.Fill(cache, -1);
    long Dfs(int k)
    {
        if (k <= 1) return k;
        if (cache[k] != -1) return cache[k];
        return cache[k] = Dfs(k - 1) + Dfs(k - 2);
    }
    return Dfs(n);
}

// 自底向上：数组版
static long BottomUpArray(int n)
{
    var dp = new long[n + 1];
    dp[0] = 0;
    dp[1] = 1;
    for (int i = 2; i <= n; i++) dp[i] = dp[i - 1] + dp[i - 2];
    return dp[n];
}

// 自底向上：滚动变量版（O(1) 空间）
static long BottomUpRolling(int n)
{
    if (n <= 1) return n;
    long a = 0, b = 1;
    for (int i = 2; i <= n; i++)
    {
        long c = a + b;
        a = b;
        b = c;
    }
    return b;
}

// 单次调用太快，测不出来 —— 重复 Repeats 次测总耗时，跑 3 轮取最快
double BestMs(Func<long> f, int rounds = 3)
{
    double best = double.MaxValue;
    for (int r = 0; r < rounds; r++)
    {
        var s = Stopwatch.StartNew();
        for (int i = 0; i < Repeats; i++) f();
        s.Stop();
        best = Math.Min(best, s.Elapsed.TotalMilliseconds);
    }
    return best;
}

long AllocBytes(Func<long> f)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    f();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}

// 预热
MemoFibBig(BigN); BottomUpArray(BigN); BottomUpRolling(BigN);

Console.WriteLine($"  算 Fib({BigN})（这个规模朴素递归已经不可能跑完了）");
Console.WriteLine($"  每种实现各跑 {Repeats:N0} 次取总耗时，{Repeats:N0} 次里只测一次分配量：");
Console.WriteLine();
Console.WriteLine($"    实现                        结果（算一次）           {Repeats:N0} 次耗时    单次分配");
Console.WriteLine("    ------------------------    --------------------    ----------    --------");

long r1 = MemoFibBig(BigN);
double t1 = BestMs(() => MemoFibBig(BigN));
long a1 = AllocBytes(() => MemoFibBig(BigN));
Console.WriteLine($"    自顶向下（记忆化递归）      {r1,20:N0}    {t1,7:F1} ms    {a1,7:N0} B");

long r2 = BottomUpArray(BigN);
double t2 = BestMs(() => BottomUpArray(BigN));
long a2 = AllocBytes(() => BottomUpArray(BigN));
Console.WriteLine($"    自底向上（数组）            {r2,20:N0}    {t2,7:F1} ms    {a2,7:N0} B");

long r3 = BottomUpRolling(BigN);
double t3 = BestMs(() => BottomUpRolling(BigN));
long a3 = AllocBytes(() => BottomUpRolling(BigN));
Console.WriteLine($"    自底向上（滚动变量）        {r3,20:N0}    {t3,7:F1} ms    {a3,7:N0} B");

Console.WriteLine();
Console.WriteLine($"  三者结果一致：{r1 == r2 && r2 == r3}");
Console.WriteLine($"  记忆化递归比滚动变量慢 {t1 / t3:F1} 倍，而且每算一次就要分配 {a1:N0} 字节。");
Console.WriteLine();

Console.WriteLine("  三个版本的取舍：");
Console.WriteLine("    - 记忆化递归：写起来【最接近暴力递归】（加两行），只算用到的子问题；");
Console.WriteLine("                  代价是递归调用开销 + 栈深度（2.2 节：默认栈约一万多层就崩）；");
Console.WriteLine("    - 自底向上数组：没有递归开销，但要把【所有】子问题都算一遍，还要一整张表；");
Console.WriteLine("    - 滚动变量：连表都省了 —— 因为 Fib(i) 只依赖前两个，前面那些永远用不到了。");
Console.WriteLine();
Console.WriteLine("  注意最后一行这件事：");
Console.WriteLine("    「滚动变量」能成立，是因为斐波那契的【状态依赖只有两步】；");
Console.WriteLine("    换成 0/1 背包那种依赖一整行的，就滚不动了（15.4 节会讲这个边界）。");
Console.WriteLine();
Console.WriteLine("  什么时候自顶向下更划算？");
Console.WriteLine("    当【状态空间很大、但实际只用得到一小部分】时，自顶向下只算需要的那部分；");
Console.WriteLine("    而斐波那契恰好相反 —— 每个子问题都要用到，自底向上省不掉任何一个，");
Console.WriteLine("    所以在这里自底向上全面占优。");
