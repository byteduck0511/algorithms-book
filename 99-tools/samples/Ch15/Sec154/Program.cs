// ==================== 第 15.4 节：空间优化与滚动数组 ====================
//
//   实验一：0/1 背包 —— 二维表 vs 一维数组，压缩比 = 容量维度的长度
//   实验二：编辑距离 —— 一个「依赖上一行 + 本行左边」的问题，压缩时要多一个临时变量
//   实验三：LIS —— 依赖「全部历史」时，压不了
//
// 度量说明：内存一律用 GC.GetAllocatedBytesForCurrentThread()（不要用 GC.GetTotalMemory 差值法，
//           12.1 节实测过它会给出 0.0 MB 和 -94.0 MB 两次假结果）。

using System.Diagnostics;

// ==================== 实验一：0/1 背包的二维 vs 一维 ====================

Console.WriteLine("=== 实验一：0/1 背包 —— 二维表 vs 一维数组 ===");
Console.WriteLine();

const int ItemCount = 200;
const int Capacity = 20_000;
var rng = new Random(20260918);

var weights = new int[ItemCount];
var values = new int[ItemCount];
for (int i = 0; i < ItemCount; i++) { weights[i] = rng.Next(1, 200); values[i] = rng.Next(1, 500); }

Console.WriteLine($"  规模：{ItemCount} 件物品，容量 {Capacity:N0}");
Console.WriteLine();

// 预热
Knapsack2D(weights, values, Capacity);
Knapsack1D(weights, values, Capacity);

long alloc2D = MeasureAlloc(() => Knapsack2D(weights, values, Capacity));
long alloc1D = MeasureAlloc(() => Knapsack1D(weights, values, Capacity));
double t2D = BestMs(() => Knapsack2D(weights, values, Capacity));
double t1D = BestMs(() => Knapsack1D(weights, values, Capacity));

int r2D = Knapsack2D(weights, values, Capacity);
int r1D = Knapsack1D(weights, values, Capacity);

Console.WriteLine("    实现                    结果          单次分配        耗时");
Console.WriteLine("    --------------------    ----------    ------------    --------");
Console.WriteLine($"    二维 dp[i][c]           {r2D,10:N0}    {alloc2D,10:N0} B    {t2D,6:F2} ms");
Console.WriteLine($"    一维 dp[c]              {r1D,10:N0}    {alloc1D,10:N0} B    {t1D,6:F2} ms");
Console.WriteLine();
Console.WriteLine($"  两者结果一致：{r2D == r1D}");
Console.WriteLine($"  内存压缩比：{alloc2D / (double)alloc1D:F1} 倍");
Console.WriteLine();
Console.WriteLine("  这个倍数怎么来的？");
Console.WriteLine($"    二维表有 {ItemCount + 1} 行 × {Capacity + 1} 列 = {(ItemCount + 1) * (long)(Capacity + 1):N0} 个格子；");
Console.WriteLine($"    一维数组只有 {Capacity + 1:N0} 个格子。");
Console.WriteLine($"    比值 ≈ 行数 = {ItemCount + 1} —— 正好对得上实测的 {alloc2D / (double)alloc1D:F1} 倍。");
Console.WriteLine();
Console.WriteLine("  【容量越大，压缩赚得越多】：一维数组的长度是 cap，二维是 n×cap，");
Console.WriteLine("  所以容量翻倍，省下的内存也翻倍。");
Console.WriteLine();

// ==================== 实验二：编辑距离 ====================

Console.WriteLine("=== 实验二：编辑距离 —— 依赖「上一行 + 本行左边」时怎么压 ===");
Console.WriteLine();
Console.WriteLine("  问题：把字符串 A 改成字符串 B，每次可以插入/删除/替换一个字符，求最少操作次数。");
Console.WriteLine();

Console.WriteLine("  转移方程共有三个来源：");
Console.WriteLine("    dp[i][j] = min( dp[i-1][j]   + 1,      删除");
Console.WriteLine("                     dp[i][j-1]   + 1,      插入");
Console.WriteLine("                     dp[i-1][j-1] + cost )  替换/匹配");
Console.WriteLine();
Console.WriteLine("  注意第二项 dp[i][j-1] 是【本行左边】—— 所以本行必须【从左往右】算。");
Console.WriteLine();

string sa = "kitten", sb = "sitting";
Console.WriteLine($"  样例：\"{sa}\" -> \"{sb}\"");
Console.WriteLine($"    二维解法：{EditDistance2D(sa, sb)}");
Console.WriteLine($"    一维解法：{EditDistance1D(sa, sb)}");
Console.WriteLine($"    （正确答案是 3：kitten -> sitten -> sittin -> sitting）");
Console.WriteLine();

// 随机对拍
int mismatch = 0;
for (int t = 0; t < 2000; t++)
{
    string x = RandomString(rng, rng.Next(0, 12));
    string y = RandomString(rng, rng.Next(0, 12));
    if (EditDistance2D(x, y) != EditDistance1D(x, y)) mismatch++;
}
Console.WriteLine($"  2000 组随机字符串：两种解法结果不一致的组数 = {mismatch}");
Console.WriteLine();

// 大规模的内存对比
string bigA = RandomString(rng, 2000);
string bigB = RandomString(rng, 2000);

EditDistance2D(bigA, bigB);
EditDistance1D(bigA, bigB);

long alloc2DEd = MeasureAlloc(() => EditDistance2D(bigA, bigB));
long alloc1DEd = MeasureAlloc(() => EditDistance1D(bigA, bigB));

Console.WriteLine($"  规模：两个长度 2,000 的字符串");
Console.WriteLine();
Console.WriteLine("    实现                    结果        单次分配");
Console.WriteLine("    --------------------    --------    ------------");
Console.WriteLine($"    二维 dp[i][j]           {EditDistance2D(bigA, bigB),8:N0}    {alloc2DEd,10:N0} B");
Console.WriteLine($"    一维 dp[j]              {EditDistance1D(bigA, bigB),8:N0}    {alloc1DEd,10:N0} B");
Console.WriteLine();
Console.WriteLine($"  内存压缩比：{alloc2DEd / (double)alloc1DEd:F1} 倍");
Console.WriteLine();

Console.WriteLine("  一维写法的关键：多存一个【临时变量】。");
Console.WriteLine("    算 dp[j] 时，需要 dp[i-1][j-1]（上一行的左邻），");
Console.WriteLine("    但那个位置已经被本行的 dp[j-1] 覆盖了 —— 所以必须提前把它存进一个变量里。");
Console.WriteLine();

// ==================== 实验三：依赖「全部历史」时压不了 ====================

Console.WriteLine("=== 实验三：LIS —— 依赖「全部历史」时，压不了 ===");
Console.WriteLine();
Console.WriteLine("  回顾 15.2 节的 LIS 转移方程：");
Console.WriteLine("    dp[i] = max( dp[j] ) + 1     （对所有 j < i 且 a[j] < a[i]）");
Console.WriteLine();
Console.WriteLine("  注意那个 max 的范围是【所有 j < i】—— 不是「上一行」，也不是「上一格」，");
Console.WriteLine("  而是【从头到现在的全部历史】。");
Console.WriteLine();

var lisArr = new int[3000];
for (int i = 0; i < lisArr.Length; i++) lisArr[i] = rng.Next(1, 100_000);

long allocLis = MeasureAlloc(() => LisQuadratic(lisArr));
Console.WriteLine($"  n = {lisArr.Length:N0} 时，dp 数组必须完整保留：单次分配 {allocLis:N0} B");
Console.WriteLine($"    （{lisArr.Length:N0} 个 int = {lisArr.Length * 4:N0} B，一个不多一个不少）");
Console.WriteLine();
Console.WriteLine("  为什么压不了？因为算 dp[i] 时，前面每一个 dp[j] 都【有可能】被用到 ——");
Console.WriteLine("  你无法提前知道哪个 j 的 a[j] 会比 a[i] 小。");
Console.WriteLine();
Console.WriteLine("  这就是「依赖宽度」的第三个档位：");
Console.WriteLine("    依赖上一行    -> 压成一维        （0/1 背包）");
Console.WriteLine("    依赖上一行+本行-> 压成一维+临时变量（编辑距离）");
Console.WriteLine("    依赖全部历史  -> 【压不了】        （LIS 的 O(n^2) 解法）");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- 0/1 背包：二维 ----------
static int Knapsack2D(int[] w, int[] v, int cap)
{
    int n = w.Length;
    var dp = new int[n + 1, cap + 1];
    for (int i = 1; i <= n; i++)
        for (int c = 0; c <= cap; c++)
        {
            dp[i, c] = dp[i - 1, c];
            if (c >= w[i - 1])
                dp[i, c] = Math.Max(dp[i, c], dp[i - 1, c - w[i - 1]] + v[i - 1]);
        }
    return dp[n, cap];
}

// ---------- 0/1 背包：一维（逆序）----------
static int Knapsack1D(int[] w, int[] v, int cap)
{
    var dp = new int[cap + 1];
    for (int i = 0; i < w.Length; i++)
        for (int c = cap; c >= w[i]; c--)
            dp[c] = Math.Max(dp[c], dp[c - w[i]] + v[i]);
    return dp[cap];
}

// ---------- 编辑距离：二维 ----------
static int EditDistance2D(string a, string b)
{
    int n = a.Length, m = b.Length;
    var dp = new int[n + 1, m + 1];
    for (int i = 0; i <= n; i++) dp[i, 0] = i;
    for (int j = 0; j <= m; j++) dp[0, j] = j;

    for (int i = 1; i <= n; i++)
        for (int j = 1; j <= m; j++)
        {
            int cost = a[i - 1] == b[j - 1] ? 0 : 1;
            dp[i, j] = Math.Min(
                Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                dp[i - 1, j - 1] + cost);
        }
    return dp[n, m];
}

// ---------- 编辑距离：一维（多存一个临时变量）----------
static int EditDistance1D(string a, string b)
{
    int n = a.Length, m = b.Length;
    var dp = new int[m + 1];
    for (int j = 0; j <= m; j++) dp[j] = j;      // 第 0 行

    for (int i = 1; i <= n; i++)
    {
        int prevDiag = dp[0];                    // ★ 保存 dp[i-1][j-1]
        dp[0] = i;                               // dp[i][0] = i
        for (int j = 1; j <= m; j++)             // ★ 本行【从左往右】
        {
            int prevUp = dp[j];                  // 覆盖前先存下 dp[i-1][j]
            int cost = a[i - 1] == b[j - 1] ? 0 : 1;
            dp[j] = Math.Min(
                Math.Min(dp[j] + 1, dp[j - 1] + 1),   // 上、左（左已是本行的值）
                prevDiag + cost);                     // 左上（用临时变量）
            prevDiag = prevUp;                   // 为下一个 j 准备左上角
        }
    }
    return dp[m];
}

// ---------- LIS：O(n^2)（依赖全部历史）----------
static int LisQuadratic(int[] a)
{
    int n = a.Length, best = 0;
    var dp = new int[n];
    for (int i = 0; i < n; i++)
    {
        dp[i] = 1;
        for (int j = 0; j < i; j++)
            if (a[j] < a[i]) dp[i] = Math.Max(dp[i], dp[j] + 1);
        best = Math.Max(best, dp[i]);
    }
    return best;
}

// ---------- 工具 ----------

// 用 GetAllocatedBytesForCurrentThread 测单次分配的字节数
static long MeasureAlloc(Action a)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    a();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}

static double BestMs(Action a, int rounds = 3)
{
    double best = double.MaxValue;
    for (int r = 0; r < rounds; r++)
    {
        var sw = Stopwatch.StartNew();
        a();
        sw.Stop();
        best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
    }
    return best;
}

static string RandomString(Random rng, int len)
{
    var chars = new char[len];
    for (int i = 0; i < len; i++) chars[i] = (char)('a' + rng.Next(4));   // 只用 4 个字母，制造更多匹配
    return new string(chars);
}
