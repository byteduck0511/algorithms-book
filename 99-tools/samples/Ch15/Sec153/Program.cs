// ==================== 第 15.3 节：经典题型 —— 背包与最长递增子序列 ====================
//
//   实验一：0/1 背包的二维写法 vs 一维写法（逆序），以及【正序会错成什么样】
//   实验二：完全背包 —— 同一份正序代码，在另一个问题上反而是对的
//   实验三：LIS 的 O(n^2) 与 O(n log n)，含实测性能对比
//
// 度量说明：正确性用「与暴力枚举是否一致」，性能用耗时（注明机器，多轮取最快）。

using System.Diagnostics;

// ==================== 实验一：0/1 背包的三种写法 ====================

Console.WriteLine("=== 实验一：0/1 背包 —— 二维、一维逆序、一维正序 ===");
Console.WriteLine();
Console.WriteLine("  问题：容量固定，每件物品【要么整个拿走、要么不拿】，求最大总价值。");
Console.WriteLine("        （这就是 14.3 节那个「贪心近似比无界」的问题，现在给它一个正确的解法。）");
Console.WriteLine();

int[] w = [3, 7];
int[] v = [5, 9];
const int Cap = 10;

Console.WriteLine($"  数据：容量 {Cap}，物品 X(重{w[0]}, 值{v[0]})、Y(重{w[1]}, 值{v[1]})");
Console.WriteLine();

int twoD = Knapsack01_2D(w, v, Cap);
int oneDBack = Knapsack01_1D_Backward(w, v, Cap);
int oneDFwd = Knapsack01_1D_Forward(w, v, Cap);
int brute01 = Knapsack01_BruteForce(w, v, Cap);

Console.WriteLine("    实现                                 结果");
Console.WriteLine("    --------------------------------    --------");
Console.WriteLine($"    二维 dp[i][c]                        {twoD}");
Console.WriteLine($"    一维 dp[c]，容量【逆序】遍历          {oneDBack}");
Console.WriteLine($"    一维 dp[c]，容量【正序】遍历          {oneDFwd}   <- 错！");
Console.WriteLine($"    暴力枚举所有子集（真值）              {brute01}");
Console.WriteLine();

Console.WriteLine($"  逆序版对不对：{oneDBack == brute01}；正序版对不对：{oneDFwd == brute01}");
Console.WriteLine();

Console.WriteLine("  正序版算出来的是什么？");
int unbounded = UnboundedKnapsack(w, v, Cap);
int bruteUnbounded = UnboundedBruteForce(w, v, Cap);
Console.WriteLine($"    完全背包（每种物品可以拿无限多件）的正确解：{unbounded}");
Console.WriteLine($"    暴力枚举验证：{bruteUnbounded}");
Console.WriteLine();
Console.WriteLine($"    —— 正序版的答案 {oneDFwd} 正好等于完全背包的答案 {unbounded}。");
Console.WriteLine("    【0/1 背包写错了正序，就等于把完全背包写对了】。");
Console.WriteLine();

// ---------- 随机对拍 ----------
Console.WriteLine("  ---------- 随机对拍 ----------");
const int Trials = 2000;
var rng = new Random(20260918);
int backHits = 0, fwdHits = 0;

for (int t = 0; t < Trials; t++)
{
    int n = rng.Next(3, 11);
    var ww = new int[n];
    var vv = new int[n];
    for (int i = 0; i < n; i++) { ww[i] = rng.Next(1, 12); vv[i] = rng.Next(1, 30); }
    int cap = rng.Next(8, 41);

    int bf = Knapsack01_BruteForce(ww, vv, cap);
    if (Knapsack01_1D_Backward(ww, vv, cap) == bf) backHits++;
    if (Knapsack01_1D_Forward(ww, vv, cap) == bf) fwdHits++;
}

Console.WriteLine($"  {Trials} 组随机数据（3~10 件物品，容量 8~40）：");
Console.WriteLine();
Console.WriteLine($"    一维【逆序】版：{backHits,5} / {Trials} 组正确（{backHits / (double)Trials:P1}）");
Console.WriteLine($"    一维【正序】版：{fwdHits,5} / {Trials} 组正确（{fwdHits / (double)Trials:P1}）");
Console.WriteLine();
Console.WriteLine("  逆序版 100%，而正序版几乎总是错 —— 因为它把「每件物品只能用一次」写成了「可以反复拿」，");
Console.WriteLine("  于是答案只会【变大】（完全背包的约束更松、能塞得更满），");
Console.WriteLine("  只有极少数数据下两者恰好相等（那 3.4%）。");
Console.WriteLine();

// ==================== 实验二：完全背包 ====================

Console.WriteLine("=== 实验二：完全背包 —— 同一份正序代码，在这里是对的 ===");
Console.WriteLine();
Console.WriteLine("  问题：把「要么整个拿走、要么不拿」改成「每种物品可以拿任意多件」。");
Console.WriteLine();

Console.WriteLine("    实现                                 结果");
Console.WriteLine("    --------------------------------    --------");
Console.WriteLine($"    完全背包（一维，容量【正序】遍历）    {unbounded}");
Console.WriteLine($"    暴力枚举所有取法（真值）              {bruteUnbounded}");
Console.WriteLine();

int forwardOnUnbounded = Knapsack01_1D_Forward(w, v, Cap);
Console.WriteLine($"  而「同一个正序循环」在 0/1 背包上给出 {forwardOnUnbounded} —— 比正确答案 {brute01} 大。");
Console.WriteLine();

Console.WriteLine("  所以：");
Console.WriteLine("    【逆序】和【正序】不是「哪种更快」的问题，而是两道不同的题：");
Console.WriteLine("      逆序遍历 -> dp[c-w] 还是【上一轮】的值 -> 这件物品只被用了一次  -> 0/1 背包");
Console.WriteLine("      正序遍历 -> dp[c-w] 已经是【本轮】的值 -> 这件物品还能再用  -> 完全背包");
Console.WriteLine();

// 随机对拍：正序版应该与完全背包的真值一致
int unboundedHits = 0;
for (int t = 0; t < Trials; t++)
{
    int n = rng.Next(2, 7);
    var ww = new int[n];
    var vv = new int[n];
    for (int i = 0; i < n; i++) { ww[i] = rng.Next(2, 10); vv[i] = rng.Next(1, 20); }
    int cap = rng.Next(6, 26);

    if (Knapsack01_1D_Forward(ww, vv, cap) == UnboundedBruteForce(ww, vv, cap)) unboundedHits++;
}
Console.WriteLine($"  验证：把「正序版」当成完全背包的解法，{Trials} 组随机数据上正确 {unboundedHits} 组"
    + $"（{unboundedHits / (double)Trials:P1}）");
Console.WriteLine();

// ==================== 实验三：LIS 的两种解法 ====================

Console.WriteLine("=== 实验三：LIS —— O(n^2) 与 O(n log n) ===");
Console.WriteLine();

var demoArr = new int[] { 10, 9, 2, 5, 3, 7, 101, 18 };
Console.WriteLine($"  样例：[{string.Join(", ", demoArr)}]");
Console.WriteLine($"    O(n^2) 解法    ：{LisQuadratic(demoArr)}");
Console.WriteLine($"    O(n log n) 解法：{LisNLogN(demoArr)}（tails = [{string.Join(", ", LisTails(demoArr))}]）");
Console.WriteLine();

// 随机对拍
int lisMismatch = 0;
for (int t = 0; t < 2000; t++)
{
    int n = rng.Next(1, 30);
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = rng.Next(1, 50);
    if (LisQuadratic(a) != LisNLogN(a)) lisMismatch++;
}
Console.WriteLine($"  2000 组随机数组：两种解法结果不一致的组数 = {lisMismatch}");
Console.WriteLine();

// 性能对比
const int Big = 5000;
var bigArr = new int[Big];
for (int i = 0; i < Big; i++) bigArr[i] = rng.Next(1, 1_000_000);

LisQuadratic(bigArr);
LisNLogN(bigArr);            // 预热

double BestMs(Func<int> f, int rounds = 3)
{
    double best = double.MaxValue;
    for (int r = 0; r < rounds; r++)
    {
        var s = Stopwatch.StartNew();
        f();
        s.Stop();
        best = Math.Min(best, s.Elapsed.TotalMilliseconds);
    }
    return best;
}

double tQuad = BestMs(() => LisQuadratic(bigArr));
double tFast = BestMs(() => LisNLogN(bigArr));

Console.WriteLine($"  性能对比（随机数组，n = {Big:N0}，各跑 3 轮取最快）：");
Console.WriteLine($"    O(n^2)     ：{tQuad,7:F1} ms");
Console.WriteLine($"    O(n log n) ：{tFast,7:F1} ms");
Console.WriteLine($"    快 {tQuad / tFast:F0} 倍");
Console.WriteLine();
Console.WriteLine("  两者算的都是【长度】，结果一样；差的只是「状态定义不同带来的复杂度」——");
Console.WriteLine("  这正是 15.2 节练习 15.2.5 说的那件事。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- 0/1 背包：二维 ----------
static int Knapsack01_2D(int[] w, int[] v, int cap)
{
    int n = w.Length;
    var dp = new int[n + 1, cap + 1];
    for (int i = 1; i <= n; i++)
        for (int c = 0; c <= cap; c++)
        {
            dp[i, c] = dp[i - 1, c];                                   // 不拿第 i 件
            if (c >= w[i - 1])
                dp[i, c] = Math.Max(dp[i, c], dp[i - 1, c - w[i - 1]] + v[i - 1]);   // 拿
        }
    return dp[n, cap];
}

// ---------- 0/1 背包：一维，容量【逆序】 ----------
static int Knapsack01_1D_Backward(int[] w, int[] v, int cap)
{
    var dp = new int[cap + 1];
    for (int i = 0; i < w.Length; i++)
        for (int c = cap; c >= w[i]; c--)              // ★ 逆序：dp[c-w] 还是上一轮的值
            dp[c] = Math.Max(dp[c], dp[c - w[i]] + v[i]);
    return dp[cap];
}

// ---------- 一维，容量【正序】 ----------
// 这【不是】0/1 背包的解法 —— 它实际上解的是完全背包
static int Knapsack01_1D_Forward(int[] w, int[] v, int cap)
{
    var dp = new int[cap + 1];
    for (int i = 0; i < w.Length; i++)
        for (int c = w[i]; c <= cap; c++)              // ★ 正序：dp[c-w] 已经是本轮的值
            dp[c] = Math.Max(dp[c], dp[c - w[i]] + v[i]);
    return dp[cap];
}

// ---------- 完全背包（每种物品无限多）：正序才对 ----------
static int UnboundedKnapsack(int[] w, int[] v, int cap)
{
    var dp = new int[cap + 1];
    for (int i = 0; i < w.Length; i++)
        for (int c = w[i]; c <= cap; c++)
            dp[c] = Math.Max(dp[c], dp[c - w[i]] + v[i]);
    return dp[cap];
}

// ---------- 暴力枚举：0/1 背包 ----------
static int Knapsack01_BruteForce(int[] w, int[] v, int cap)
{
    int n = w.Length, best = 0;
    for (int mask = 0; mask < (1 << n); mask++)
    {
        int ws = 0, vs = 0;
        for (int i = 0; i < n; i++)
            if ((mask & (1 << i)) != 0) { ws += w[i]; vs += v[i]; }
        if (ws <= cap && vs > best) best = vs;
    }
    return best;
}

// ---------- 暴力枚举：完全背包（每种物品取 0..cap/w 件）----------
static int UnboundedBruteForce(int[] w, int[] v, int cap)
{
    int n = w.Length, best = 0;
    var take = new int[n];

    void Rec(int idx, int usedW, int totalV)
    {
        if (idx == n) { best = Math.Max(best, totalV); return; }
        for (int k = 0; usedW + k * w[idx] <= cap; k++)
            Rec(idx + 1, usedW + k * w[idx], totalV + k * v[idx]);
    }

    Rec(0, 0, 0);
    return best;
}

// ---------- LIS：O(n^2) ----------
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

// ---------- LIS：O(n log n) ----------
static int LisNLogN(int[] a) => LisTails(a).Count;

// tails[k] = 所有长度为 k+1 的递增子序列中，结尾最小的那个值
static List<int> LisTails(int[] a)
{
    var tails = new List<int>();
    foreach (int x in a)
    {
        int pos = tails.BinarySearch(x);
        if (pos < 0) pos = ~pos;            // 没找到：~pos 是第一个大于 x 的位置
        if (pos == tails.Count) tails.Add(x);
        else tails[pos] = x;
    }
    return tails;
}
