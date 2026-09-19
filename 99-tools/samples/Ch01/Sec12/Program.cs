// 本节不比较时间，而是直接数「基本操作执行了多少次」。
// 这样得到的结论与机器无关：任何电脑上跑出来都是同一张表。

static long OpsConstant(int n) => 1;                       // O(1)

static long OpsLog(int n)                                  // O(log n)
{
    long ops = 0;
    for (int i = 1; i < n; i *= 2) ops++;                  // 1, 2, 4, 8, ... 成倍增长
    return ops;
}

static long OpsLinear(int n)                               // O(n)
{
    long ops = 0;
    for (int i = 0; i < n; i++) ops++;
    return ops;
}

static long OpsNLogN(int n)                                // O(n log n)
{
    long ops = 0;
    for (int i = 1; i < n; i *= 2)                         // 外层 log n 次
        for (int j = 0; j < n; j++) ops++;                 // 内层 n 次
    return ops;
}

static long OpsQuadratic(int n)                            // O(n^2)
{
    long ops = 0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++) ops++;
    return ops;
}

Console.WriteLine($"{"n",8} | {"O(1)",10} | {"O(log n)",10} | {"O(n)",10} | {"O(n log n)",12} | {"O(n^2)",14}");
foreach (int n in new[] { 1_000, 2_000, 4_000, 8_000 })
{
    Console.WriteLine(
        $"{n,8} | {OpsConstant(n),10} | {OpsLog(n),10} | {OpsLinear(n),10} | " +
        $"{OpsNLogN(n),12} | {OpsQuadratic(n),14}");
}

// n 翻倍时操作次数涨几倍 —— 这才是大 O 真正描述的东西
Console.WriteLine();
Console.WriteLine("n 从 4000 涨到 8000（翻倍），操作次数涨几倍：");

double r1 = (double)OpsLog(8_000) / OpsLog(4_000);
double r2 = (double)OpsLinear(8_000) / OpsLinear(4_000);
double r3 = (double)OpsNLogN(8_000) / OpsNLogN(4_000);
double r4 = (double)OpsQuadratic(8_000) / OpsQuadratic(4_000);

Console.WriteLine($"  O(log n)   : {OpsLog(4_000),12} -> {OpsLog(8_000),12}   涨 {r1,5:F2} 倍");
Console.WriteLine($"  O(n)       : {OpsLinear(4_000),12} -> {OpsLinear(8_000),12}   涨 {r2,5:F2} 倍");
Console.WriteLine($"  O(n log n) : {OpsNLogN(4_000),12} -> {OpsNLogN(8_000),12}   涨 {r3,5:F2} 倍");
Console.WriteLine($"  O(n^2)     : {OpsQuadratic(4_000),12} -> {OpsQuadratic(8_000),12}   涨 {r4,5:F2} 倍");
