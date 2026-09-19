// ==================== 决策树下界：log2(n!) ====================

// 精确计算 log2(n!)：累加 log2(1) + log2(2) + ... + log2(n)
static double Log2Factorial(int n)
{
    double sum = 0;
    for (int i = 2; i <= n; i++) sum += Math.Log2(i);
    return sum;
}

// Stirling 近似：log2(n!) ≈ n*log2(n/e) + 0.5*log2(2*pi*n)
// 用于 n 很大时（累加会太慢）
static double Log2FactorialStirling(double n)
{
    return n * Math.Log2(n / Math.E) + 0.5 * Math.Log2(2 * Math.PI * n);
}

// ==================== 实验一：n! 有多可怕 ====================

Console.WriteLine("=== 实验一：n 个元素有多少种排列 ===");
Console.WriteLine();
Console.WriteLine("  决策树的叶子数 = 可能的排列数 = n!");
Console.WriteLine();
Console.WriteLine($"  {"n",12} | {"n!",32} | {"log2(n!)",14}");
Console.WriteLine(new string('-', 66));

int[] ns = { 3, 5, 10, 20, 50 };
foreach (int n in ns)
{
    // 用 BigInteger 算精确的 n!（只有小 n 能算得动）
    System.Numerics.BigInteger factorial = 1;
    for (int i = 2; i <= n; i++) factorial *= i;

    string factorialText = factorial.ToString("N0");
    if (factorialText.Length > 30) factorialText = factorialText[..27] + "...";

    Console.WriteLine($"{n,12} | {factorialText,32} | {Log2Factorial(n),14:F1}");
}
Console.WriteLine();

Console.WriteLine("  注意 n=50 时：n! ≈ 3 × 10^64 种排列，对应的 log2(n!) ≈ 214 次比较。");
Console.WriteLine("  排列数增长得极其恐怖，但它的【对数】增长得很温和 —— 这就是关键。");
Console.WriteLine();

Console.WriteLine("=== 实验二：下界 vs n log2 n ===");
Console.WriteLine();
Console.WriteLine("  比较排序的最少比较次数 >= log2(n!)");
Console.WriteLine("  而 log2(n!) ≈ n log2(n) - 1.44n");
Console.WriteLine();
Console.WriteLine($"  {"n",14} | {"下界 log2(n!)",18} | {"n*log2(n)",18} | {"差距",14} | {"差距/n",10}");
Console.WriteLine(new string('-', 84));

foreach (int n in new[] { 10, 100, 1_000, 10_000, 100_000, 1_000_000 })
{
    double lowerBound = n <= 100_000 ? Log2Factorial(n) : Log2FactorialStirling(n);
    double nLogN = n * Math.Log2(n);
    double gap = nLogN - lowerBound;

    Console.WriteLine($"{n,14:N0} | {lowerBound,18:N1} | {nLogN,18:N1} | {gap,14:N1} | {gap / n,10:F4}");
}
Console.WriteLine();
Console.WriteLine("  最后一列稳定在 1.44 附近 —— 这正是「log2(e) - 1 + ...」的结果，");
Console.WriteLine("  也就是公式里的那个系数 1.44（严格说是 log2(e) ≈ 1.4427）。");
Console.WriteLine();

// ==================== 实验三：可视化下界的增长 ====================

Console.WriteLine("=== 实验三：为什么「下界」意味着 n log n 是绕不过去的 ===");
Console.WriteLine();
Console.WriteLine("  假设每次比较只能区分 2 种情况（大 / 小），那么：");
Console.WriteLine("    k 次比较最多能区分 2^k 种输入");
Console.WriteLine("    而我们需要区分 n! 种输入");
    Console.WriteLine("    所以 2^k >= n!  =>  k >= log2(n!)");
Console.WriteLine();
Console.WriteLine("  用具体的数字感受一下：");
Console.WriteLine();
Console.WriteLine($"  {"n",10} | {"需要区分的情况数 n!",26} | {"至少几次比较",14}");
Console.WriteLine(new string('-', 56));

foreach (int n in new[] { 10, 20, 30 })
{
    System.Numerics.BigInteger factorial = 1;
    for (int i = 2; i <= n; i++) factorial *= i;

    Console.WriteLine($"{n,10} | {factorial.ToString("N0"),26} | {Math.Ceiling(Log2Factorial(n)),14:F0}");
}
Console.WriteLine();

Console.WriteLine("  结论：");
Console.WriteLine("    任何【只靠比较元素大小】来排序的算法，");
Console.WriteLine("    在最坏情况下至少需要 log2(n!) ≈ n log2 n 次比较。");
Console.WriteLine();
Console.WriteLine("    所以 O(n log n) 不是一个「还不错的成绩」，而是【理论最优】。");
Console.WriteLine("    快速排序、归并排序、堆排序都达到了这个量级 —— 它们已经到顶了。");
Console.WriteLine();

Console.WriteLine("=== 实验四：那还能更快吗？ ===");
Console.WriteLine();
Console.WriteLine("  可以 —— 但必须【跳出比较模型】。");
Console.WriteLine();
Console.WriteLine("  这条下界只约束「通过比较两个元素的大小来决定顺序」的算法。");
Console.WriteLine("  如果不比较，而是【直接利用键本身的数值信息】，下界就不适用了。");
Console.WriteLine();
Console.WriteLine("  这就是计数排序和基数排序的思路（8.4 节）：");
Console.WriteLine("    计数排序：直接看「这个数等于几」，把它放到对应位置，O(n + k)");
Console.WriteLine("    基数排序：按位分配，O(d * n)");
Console.WriteLine();
Console.WriteLine("  代价是它们对数据有额外要求（比如取值范围有限、或者能按位拆分），");
Console.WriteLine("  而且通用性不如比较排序 —— 这就是「用适用性换速度」。");
Console.WriteLine();

// ==================== 小 n 的最少比较次数 ====================

Console.WriteLine("=== 附：小规模下的最优比较次数 ===");
Console.WriteLine();
Console.WriteLine($"  {"n",6} | {"log2(n!)",12} | {"向上取整",10} | {"实际最优",12} | 说明");
Console.WriteLine(new string('-', 74));
Console.WriteLine($"  {2,6} | {Log2Factorial(2),12:F3} | {Math.Ceiling(Log2Factorial(2)),10:F0} | {1,12} | 比较 1 次就够了");
Console.WriteLine($"  {3,6} | {Log2Factorial(3),12:F3} | {Math.Ceiling(Log2Factorial(3)),10:F0} | {3,12} | 需要 3 次（下界 2.58 不够）");
Console.WriteLine($"  {4,6} | {Log2Factorial(4),12:F3} | {Math.Ceiling(Log2Factorial(4)),10:F0} | {5,12} | 需要 5 次（下界 4.58 不够）");
Console.WriteLine($"  {5,6} | {Log2Factorial(5),12:F3} | {Math.Ceiling(Log2Factorial(5)),10:F0} | {7,12} | 需要 7 次");
Console.WriteLine();
Console.WriteLine("  注意 n=3 和 n=4：下界「向上取整」分别是 3 和 5，但这【只是下界】——");
Console.WriteLine("  它说的是「不可能少于这么多」，不代表「一定能做到这么多」。");
Console.WriteLine();
Console.WriteLine("  实际上 n=3 时确实能做到 3 次，n=4 时确实能做到 5 次。");
Console.WriteLine("  但 n 再大一些，能达到下界的算法就越来越难写 —— 而且常数极大。");
Console.WriteLine();
Console.WriteLine("  所以工程上不追求「达到理论下界」，而是追求「达到 n log n 这个量级，");
Console.WriteLine("  同时常数尽量小」。这就是为什么快排（平均常数小）比归并排序更常用。");
Console.WriteLine();
