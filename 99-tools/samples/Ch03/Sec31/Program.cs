using System.Diagnostics;

// ============ 实验一：数组元素在内存中是连续的 ============
// 用不安全代码直接读内存地址，亲眼确认「连续」这两个字。

unsafe
{
    int[] a = { 10, 20, 30, 40, 50 };

    Console.WriteLine("=== 实验一：数组元素的地址是连续的 ===");
    Console.WriteLine("（用 unsafe 代码直接读内存地址）");
    Console.WriteLine();

    fixed (int* p = a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            long offset = (long)(p + i) - (long)p;
            Console.WriteLine($"  a[{i}] = {a[i],3}   相对首地址偏移 = {offset,3} 字节");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"  每个 int 占 {sizeof(int)} 字节，所以 a[i] 的地址 = 首地址 + i * 4");
    Console.WriteLine($"  知道首地址和下标，一次乘加就能算出位置 -> 这就是随机访问是 O(1) 的原因");
}
Console.WriteLine();

// ============ 实验二：尾部追加 vs 头部插入 ============

var sw = Stopwatch.StartNew();

// 预热：先让 JIT 编译好，否则第一轮测量会把编译时间也算进去
{
    var warm = new List<int>();
    for (int i = 0; i < 50_000; i++) warm.Add(i);
    warm.Clear();
    for (int i = 0; i < 50_000; i++) warm.Insert(0, i);
}

Console.WriteLine("=== 实验二：尾部追加 vs 头部插入 ===");
Console.WriteLine($"{"操作次数",12} | {"尾部追加",12} | {"头部插入",12} | {"头部相对上次",14}");
Console.WriteLine(new string('-', 62));

double prevHeadMs = 0;

foreach (int n in new[] { 50_000, 200_000 })
{
    sw.Restart();
    var tail = new List<int>();
    for (int i = 0; i < n; i++) tail.Add(i);           // 追加到末尾，不用挪动任何元素
    sw.Stop();
    double tailMs = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var head = new List<int>();
    for (int i = 0; i < n; i++) head.Insert(0, i);     // 每次都插到最前面，后面全部挪一格
    sw.Stop();
    double headMs = sw.Elapsed.TotalMilliseconds;

    string growth = prevHeadMs > 0 ? $"{headMs / prevHeadMs:F1} 倍" : "—";
    Console.WriteLine($"{n,12:N0} | {tailMs,9:F2} ms | {headMs,9:F2} ms | {growth,14}");
    prevHeadMs = headMs;
}

Console.WriteLine();
Console.WriteLine("  数据量涨了 4 倍，两种操作的耗时变化完全不同：");
Console.WriteLine("    尾部追加：几乎不变（线性，且常数极小）");
Console.WriteLine("    头部插入：涨了 16 倍（4^2 = 16，这正是 O(n^2) 的样子）");
Console.WriteLine();

// ============ 实验三：顺序访问 vs 随机访问（CPU 缓存效应） ============

const int M = 10_000_000;
int[] data = new int[M];
for (int i = 0; i < M; i++) data[i] = i;

// ---- 顺序访问 ----
sw.Restart();
long sumSeq = 0;
for (int i = 0; i < M; i++) sumSeq += data[i];
sw.Stop();
double seqMs = sw.Elapsed.TotalMilliseconds;

// ---- 随机访问 ----
// 关键：先把下标数组洗牌，这样两个循环的结构完全一样，
// 唯一的差别就是「内存访问模式」。否则随机数生成的开销会污染结果。
int[] indices = new int[M];
for (int i = 0; i < M; i++) indices[i] = i;

var rng = new Random(42);
for (int i = M - 1; i > 0; i--)                    // Fisher-Yates 洗牌
{
    int j = rng.Next(i + 1);
    (indices[i], indices[j]) = (indices[j], indices[i]);
}

sw.Restart();
long sumRand = 0;
for (int i = 0; i < M; i++) sumRand += data[indices[i]];
sw.Stop();
double randMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine("=== 实验三：顺序访问 vs 随机访问 ===");
Console.WriteLine($"  两者都是 {M:N0} 次加法，循环结构完全相同，只有内存访问顺序不同：");
Console.WriteLine();
Console.WriteLine($"  顺序访问: {seqMs,10:F2} ms   和 = {sumSeq:N0}");
Console.WriteLine($"  随机访问: {randMs,10:F2} ms   和 = {sumRand:N0}");
Console.WriteLine($"  随机访问慢 {randMs / seqMs:F1} 倍 —— 差别全部来自 CPU 缓存");
Console.WriteLine();
Console.WriteLine("  原因：CPU 读内存不是读 1 个字节，而是整条「缓存行」（通常 64 字节）一起读。");
Console.WriteLine("        顺序访问时，读到 a[0] 意味着 a[1]~a[15] 也已经在缓存里了，后面 15 次几乎免费；");
Console.WriteLine("        随机访问时，每次都要去主内存取一整条缓存行，却只用其中 4 个字节。");
Console.WriteLine();
Console.WriteLine("  这就是数组相对链表的最大隐藏优势：链表节点在内存里是散落的，无法利用缓存行。");
