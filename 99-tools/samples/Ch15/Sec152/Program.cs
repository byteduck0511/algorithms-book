// ==================== 第 15.2 节：状态定义与转移方程 ====================
//
//   实验一：LIS 的「错误状态定义」—— 差一个词，答案就错了
//   实验二：LIS 的正确状态 + 随机对拍（错误版错得有多频繁）
//   实验三：打家劫舍 —— 两种状态定义都对，但一个比另一个简洁得多
//
// 度量说明：全部用「答案是否一致」「正确率」，与机器无关。

// ==================== 实验一：LIS 的状态定义 ====================

Console.WriteLine("=== 实验一：最长递增子序列（LIS）—— 状态定义差一个词 ===");
Console.WriteLine();
Console.WriteLine("  问题：在一个数组里找出最长的【严格递增】子序列（不要求连续）。");
Console.WriteLine();

int[] demo = [4, 5, 1, 2, 3];
Console.WriteLine($"  反例数组：[{string.Join(", ", demo)}]");
Console.WriteLine();

// 错误的状态定义
var wrongF = LisWrongTrace(demo);
Console.WriteLine("  错误的状态定义：f[i] = 【前 i 个元素】的最长递增子序列长度");
Console.WriteLine($"    f = [{string.Join(", ", wrongF)}]  ->  答案 {wrongF[^1]}");
Console.WriteLine();

// 正确的状态定义
var rightDp = LisCorrectTrace(demo);
Console.WriteLine("  正确的状态定义：dp[i] = 【以 a[i] 结尾】的最长递增子序列长度");
Console.WriteLine($"    dp = [{string.Join(", ", rightDp)}]  ->  答案 {rightDp.Max()}");
Console.WriteLine();

int brute = LisBruteForce(demo);
Console.WriteLine($"  暴力枚举所有子序列：{brute}");
Console.WriteLine();

Console.WriteLine("  这两行差在哪？");
Console.WriteLine("    错误版问的是「前 i 个里最长有多长」—— 它丢掉了【结尾是什么】这条信息；");
Console.WriteLine("    于是当数组从 4,5 突然降到 1 时，它还以为自己有一条长度 2 的链可以用，");
Console.WriteLine("    实际上那条链的结尾是 5，接不上后面的 1。");
Console.WriteLine();

// ---------- 随机对拍 ----------
Console.WriteLine("  ---------- 随机对拍 ----------");
const int Trials = 3000;
var rng = new Random(20260918);
int wrongHits = 0, rightHits = 0;

for (int t = 0; t < Trials; t++)
{
    int n = rng.Next(5, 13);
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = rng.Next(1, 10);

    int bf = LisBruteForce(a);
    if (LisWrong(a) == bf) wrongHits++;
    if (LisCorrect(a) == bf) rightHits++;
}

Console.WriteLine($"  {Trials} 组随机数组（长度 5~12，元素 1~9），与暴力枚举对比：");
Console.WriteLine();
Console.WriteLine($"    错误的状态定义：{wrongHits,5} / {Trials} 组正确（{wrongHits / (double)Trials:P1}）");
Console.WriteLine($"    正确的状态定义：{rightHits,5} / {Trials} 组正确（{rightHits / (double)Trials:P1}）");
Console.WriteLine();
Console.WriteLine("  错误版不是「总是错」，而是「有时错」—— 和 14.1 节贪心的情况一样；");
Console.WriteLine("  但它错的时候不会报错，只会给一个偏大的数。");
Console.WriteLine();

// ==================== 实验二：打家劫舍 ====================

Console.WriteLine("=== 实验二：打家劫舍 —— 状态不是越多越好 ===");
Console.WriteLine();
Console.WriteLine("  问题：一排房子各有金额，不能偷相邻的两家，求最多能偷多少。");
Console.WriteLine();

int[] houses = [2, 7, 9, 3, 1];
Console.WriteLine($"  例子：金额 [{string.Join(", ", houses)}]");

Console.WriteLine($"    简洁版（一维）：rob(i) = max(rob(i-1), rob(i-2) + a[i])  ->  {RobSimple(houses)}");
Console.WriteLine($"    多维版（二维）：rob(i, 偷没偷)                          ->  {RobWithState(houses)}");
Console.WriteLine($"    暴力枚举所有「不相邻」的选法                            ->  {RobBruteForce(houses)}");
Console.WriteLine();

Console.WriteLine("  两种状态定义都得到了正确答案。但它们的「状态」是：");
Console.WriteLine("    简洁版：f(i)                    —— i 一个维度");
Console.WriteLine("    多维版：f(i, 第 i 家偷没偷)      —— 两个维度，状态数翻倍");
Console.WriteLine();

Console.WriteLine("  ---------- 随机对拍 ----------");
int mismatch = 0;
for (int t = 0; t < Trials; t++)
{
    int n = rng.Next(4, 13);
    var a = new int[n];
    for (int i = 0; i < n; i++) a[i] = rng.Next(1, 30);
    if (RobSimple(a) != RobWithState(a)) mismatch++;
}
Console.WriteLine($"  {Trials} 组随机数据：两种状态定义结果不一致的组数 = {mismatch}");
Console.WriteLine();

Console.WriteLine("  为什么会这样？");
Console.WriteLine("    「第 i 家偷没偷」这条信息，其实【已经被 f(i-1) 和 f(i-2) 的组合隐含了】：");
Console.WriteLine("      f(i-1) 对应「不偷第 i 家」，f(i-2) + a[i] 对应「偷第 i 家」——");
Console.WriteLine("      两条分支刚好把两种可能都覆盖了，不需要额外记一个标志位。");
Console.WriteLine();

Console.WriteLine("  那什么时候【必须】把「上一家偷没偷」写进状态？");
Console.WriteLine("    当规则变成「偷完必须休息一天」（冷冻期）时 ——");
Console.WriteLine("    因为那时「昨天偷了」和「昨天没偷」会让今天的选择不同，");
Console.WriteLine("    而 f(i-1) 这个数【分不出】这两种情况。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- LIS：错误的状态定义 ----------
// f[i] = 前 i 个元素的最长递增子序列长度（丢掉了「结尾是什么」）
static int LisWrong(int[] a)
{
    int n = a.Length;
    var f = new int[n];
    f[0] = 1;
    for (int i = 1; i < n; i++)
        f[i] = a[i] > a[i - 1] ? f[i - 1] + 1 : f[i - 1];
    return f[n - 1];
}

static List<int> LisWrongTrace(int[] a)
{
    int n = a.Length;
    var f = new int[n];
    f[0] = 1;
    for (int i = 1; i < n; i++)
        f[i] = a[i] > a[i - 1] ? f[i - 1] + 1 : f[i - 1];
    return f.ToList();
}

// ---------- LIS：正确的状态定义 ----------
// dp[i] = 以 a[i] 结尾的最长递增子序列长度
static int LisCorrect(int[] a)
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

static List<int> LisCorrectTrace(int[] a)
{
    int n = a.Length;
    var dp = new int[n];
    for (int i = 0; i < n; i++)
    {
        dp[i] = 1;
        for (int j = 0; j < i; j++)
            if (a[j] < a[i]) dp[i] = Math.Max(dp[i], dp[j] + 1);
    }
    return dp.ToList();
}

// ---------- LIS：暴力枚举所有子序列（验证用）----------
static int LisBruteForce(int[] a)
{
    int n = a.Length, best = 0;
    for (int mask = 1; mask < (1 << n); mask++)
    {
        int len = 0, last = int.MinValue;
        bool ok = true;
        for (int i = 0; i < n; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            if (a[i] <= last) { ok = false; break; }
            last = a[i];
            len++;
        }
        if (ok && len > best) best = len;
    }
    return best;
}

// ---------- 打家劫舍：简洁版（一维状态）----------
static int RobSimple(int[] a)
{
    int prev2 = 0, prev1 = 0;
    foreach (int x in a)
    {
        int cur = Math.Max(prev1, prev2 + x);
        prev2 = prev1;
        prev1 = cur;
    }
    return prev1;
}

// ---------- 打家劫舍：多维版（状态里显式记「第 i 家偷没偷」）----------
static int RobWithState(int[] a)
{
    int n = a.Length;
    var dp = new int[n, 2];              // [i,0] 不偷第 i 家；[i,1] 偷第 i 家
    dp[0, 0] = 0;
    dp[0, 1] = a[0];
    for (int i = 1; i < n; i++)
    {
        dp[i, 0] = Math.Max(dp[i - 1, 0], dp[i - 1, 1]);   // 不偷 i：前一家随意
        dp[i, 1] = dp[i - 1, 0] + a[i];                    // 偷 i：前一家必不能偷
    }
    return Math.Max(dp[n - 1, 0], dp[n - 1, 1]);
}

// ---------- 打家劫舍：暴力枚举所有子集，检查没有相邻 ----------
static int RobBruteForce(int[] a)
{
    int n = a.Length, best = 0;
    for (int mask = 0; mask < (1 << n); mask++)
    {
        bool ok = true;
        int sum = 0;
        for (int i = 0; i < n; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            if (i > 0 && (mask & (1 << (i - 1))) != 0) { ok = false; break; }
            sum += a[i];
        }
        if (ok && sum > best) best = sum;
    }
    return best;
}
