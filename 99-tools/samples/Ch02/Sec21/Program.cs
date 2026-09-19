// ---------- 例 1：递归求和，并把完整调用过程打印出来 ----------
static int SumWithTrace(int[] a, int i, int depth)
{
    string indent = new string(' ', depth * 2);
    Console.WriteLine($"{indent}进入 Sum(i={i})");

    if (i == a.Length)
    {
        Console.WriteLine($"{indent}返回 0   <- 基准情形，不再递归");
        return 0;
    }

    int rest = SumWithTrace(a, i + 1, depth + 1);   // 递归情形：问题缩小到 i+1
    int result = a[i] + rest;
    Console.WriteLine($"{indent}返回 {result}");
    return result;
}

// ---------- 例 2：阶乘 ----------
static long Factorial(int n)
{
    if (n <= 1) return 1;             // 基准情形
    return n * Factorial(n - 1);      // 递归情形：规模从 n 缩到 n-1
}

// ---------- 例 3：反转字符串 ----------
static string Reverse(string s)
{
    if (s.Length <= 1) return s;              // 基准情形：空串或单字符
    return Reverse(s.Substring(1)) + s[0];    // 递归情形：去掉首字符
}

// ---------- 例 4：朴素斐波那契（为 15.1 节埋伏笔） ----------
long fibCalls = 0;

long Fib(int n)
{
    fibCalls++;
    if (n <= 1) return n;
    return Fib(n - 1) + Fib(n - 2);
}

// ==================== 主流程 ====================

int[] data = { 1, 2, 3, 4 };
Console.WriteLine("=== 例 1：递归求和的完整调用过程 ===");
Console.WriteLine($"数组: [{string.Join(", ", data)}]");
Console.WriteLine();
int total = SumWithTrace(data, 0, 0);
Console.WriteLine();
Console.WriteLine($"最终结果: {total}");

Console.WriteLine();
Console.WriteLine("=== 例 2：阶乘 ===");
foreach (int n in new[] { 0, 1, 5, 10, 20 })
    Console.WriteLine($"  {n,2}! = {Factorial(n)}");

Console.WriteLine();
Console.WriteLine("=== 例 3：反转字符串 ===");
foreach (var s in new[] { "(空串)", "a", "ab", "hello", "算法" })
{
    string input = s == "(空串)" ? "" : s;
    Console.WriteLine($"  \"{input}\" -> \"{Reverse(input)}\"");
}

Console.WriteLine();
Console.WriteLine("=== 例 4：朴素斐波那契的递归调用次数 ===");
foreach (int n in new[] { 20, 25, 30, 35 })
{
    fibCalls = 0;
    long r = Fib(n);
    Console.WriteLine($"  Fib({n,2}) = {r,-9} 递归调用次数 = {fibCalls,12:N0}");
}
