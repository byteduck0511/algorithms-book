using System.Diagnostics;

const long MOD = 1_000_000_007;

// ==================== 例 1：分治求最大值 ====================
//
// 分治三步：切开（分成左右两半）-> 解决（各自递归求最大值）-> 合并（取两者较大值）

static int MaxDivideConquer(int[] a, int lo, int hi)
{
    if (lo == hi) return a[lo];                          // 基准情形：只剩一个元素

    int mid = (lo + hi) / 2;
    int leftMax = MaxDivideConquer(a, lo, mid);          // 切开 + 解决左半
    int rightMax = MaxDivideConquer(a, mid + 1, hi);     // 切开 + 解决右半
    return Math.Max(leftMax, rightMax);                  // 合并
}

static int MaxLinear(int[] a)
{
    int best = a[0];
    for (int i = 1; i < a.Length; i++)
        if (a[i] > best) best = a[i];
    return best;
}

// ==================== 例 2：快速幂 ====================
//
// 朴素做法：b 连乘 e 次，O(e)
// 分治做法：b^e = (b^(e/2))^2，每次把指数砍一半，O(log e)

long naiveMultiplications = 0;
long fastMultiplications = 0;

static long PowerNaive(long b, int e, ref long counter)
{
    long result = 1;
    for (int i = 0; i < e; i++)
    {
        result = result * b % MOD;
        counter++;
    }
    return result;
}

static long PowerFast(long b, int e, ref long counter)
{
    if (e == 0) return 1;

    long half = PowerFast(b, e / 2, ref counter);
    counter++;                                   // half * half 这一次乘法
    long result = half * half % MOD;

    if (e % 2 == 1)
    {
        result = result * b % MOD;
        counter++;                               // 奇数时还要多乘一次 b
    }
    return result;
}

// ==================== 例 3：分治求和的代价 ====================

// 注意返回类型用 long：10 万个百万以内的数相加约 5×10^10，超出 int 上限会静默溢出
static long SumDivideConquer(int[] a, int lo, int hi)
{
    if (lo == hi) return a[lo];
    int mid = (lo + hi) / 2;
    return SumDivideConquer(a, lo, mid) + SumDivideConquer(a, mid + 1, hi);
}

static long SumLinear(int[] a)
{
    long s = 0;
    foreach (var x in a) s += x;
    return s;
}

// ==================== 主流程 ====================

var rng = new Random(42);
int[] data = new int[100_000];
for (int i = 0; i < data.Length; i++) data[i] = rng.Next(0, 1_000_000);

Console.WriteLine("=== 例 1：找最大值，分治 vs 线性扫描 ===");
int maxDC = MaxDivideConquer(data, 0, data.Length - 1);
int maxLinear = MaxLinear(data);
Console.WriteLine($"  分治结果   : {maxDC}");
Console.WriteLine($"  线性结果   : {maxLinear}   一致={maxDC == maxLinear}");
Console.WriteLine($"  数据规模   : {data.Length:N0}");
Console.WriteLine($"  两者比较次数都是约 n 次 —— 分治在这里没有优势，只是多了一层调用开销。");
Console.WriteLine();

Console.WriteLine("=== 例 2：快速幂的乘法次数对比（结果对 1,000,000,007 取模）===");
Console.WriteLine($"{"指数 e",10} | {"朴素乘法次数",14} | {"分治乘法次数",14} | {"倍数",8}");
foreach (int e in new[] { 100, 1_000, 10_000, 100_000 })
{
    naiveMultiplications = 0;
    fastMultiplications = 0;

    long r1 = PowerNaive(2, e, ref naiveMultiplications);
    long r2 = PowerFast(2, e, ref fastMultiplications);

    Console.WriteLine($"{e,10:N0} | {naiveMultiplications,14:N0} | {fastMultiplications,14:N0} | " +
                      $"{(double)naiveMultiplications / fastMultiplications,8:F1}  一致={r1 == r2}");
}
Console.WriteLine();

Console.WriteLine("=== 例 3：求和，分治 vs 线性（分治不一定更快）===");

var sw = Stopwatch.StartNew();
long s1 = SumDivideConquer(data, 0, data.Length - 1);
sw.Stop();
double dcMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long s2 = SumLinear(data);
sw.Stop();
double linearMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  分治求和: {dcMs,8:F3} ms   结果 = {s1:N0}");
Console.WriteLine($"  线性求和: {linearMs,8:F3} ms   结果 = {s2:N0}");
Console.WriteLine($"  线性快 {dcMs / linearMs:F1} 倍 —— 分治的递归调用是有成本的。");
