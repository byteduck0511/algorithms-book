// ==================== 第 16.2 节：从复杂度到实测 —— 性能剖析 ====================
//
//   实验一：缓存的影响 —— 同一个算法、同样的操作次数，只改遍历顺序
//   实验二：基准测试的四个陷阱，逐个演示（并给出修正）
//           (1) 没有预热  (2) 只跑一次  (3) 忽略 GC  (4) 忽略噪声区间
//
// 说明：本节全在讲「怎么测」，所以每个数字都是【故意测出来的】，
//       包括那些【错误的测法】测出来的数字 —— 它们会原样保留，用来对照。

using System.Diagnostics;

const int Size = 4000;
var matrix = new int[Size, Size];

// 下面两个变量要在顶级语句里被局部函数捕获，必须先声明
var sharedBuffer = new int[1_000_000];
long sink = 0;                    // 让计算结果「流向一个能被观测到的地方」
var rng = new Random(20260918);
for (int r = 0; r < Size; r++)
    for (int c = 0; c < Size; c++)
        matrix[r, c] = rng.Next(100);

// 陷阱 1 专用的小矩阵：数据量小，好让 JIT 的开销在总耗时里占得看得见
var smallMatrix = new int[500, 500];
for (int r = 0; r < 500; r++)
    for (int c = 0; c < 500; c++)
        smallMatrix[r, c] = rng.Next(100);

// ==================== 实验一：缓存的影响 ====================

Console.WriteLine("=== 实验一：同一个算法，只改遍历顺序 ===");
Console.WriteLine();
Console.WriteLine($"  数据：{Size}×{Size} 的二维数组（{Size * (long)Size:N0} 个 int，约 {Size * (long)Size * 4 / 1024 / 1024} MB）");
Console.WriteLine("  任务：把全部元素加起来");
Console.WriteLine();
Console.WriteLine($"  两种写法【操作次数完全相同】—— 都是 {Size * (long)Size:N0} 次加法：");
Console.WriteLine("    行优先：外层走行，内层走列（顺着内存排布走）");
Console.WriteLine("    列优先：外层走列，内层走行（跨着内存跳）");
Console.WriteLine();

SumRowMajor(matrix);
SumColumnMajor(matrix);          // 预热

double tRow = BestMs(() => SumRowMajor(matrix), 10);
double tCol = BestMs(() => SumColumnMajor(matrix), 10);

Console.WriteLine($"    行优先：{tRow,7:F2} ms");
Console.WriteLine($"    列优先：{tCol,7:F2} ms");
Console.WriteLine($"    列优先慢 {tCol / tRow:F2} 倍");
Console.WriteLine();
Console.WriteLine("  这一条在 3.1 节测过一次（顺序 vs 随机访问差 15 倍），");
Console.WriteLine("  那里换的是数据结构，这里【连数据结构都没换】—— 只换了循环的嵌套顺序。");
Console.WriteLine();

// ==================== 实验二：四个基准测试的陷阱 ====================

Console.WriteLine("=== 实验二：基准测试的四个陷阱 ===");
Console.WriteLine();

// ---------- 陷阱 1：没有预热 ----------
Console.WriteLine("  ---------- 陷阱 1：没有预热 ----------");
Console.WriteLine();
Console.WriteLine("  先看一种很常见的测法：直接跑、跑一次、下结论。");
Console.WriteLine();
Console.WriteLine("  下面这个 ColdSum 方法【在此之前从没被调用过】：");
Console.WriteLine();

var sw = Stopwatch.StartNew();
long a1 = ColdSum(smallMatrix);          // 第一次：含 JIT 编译
sw.Stop();
double naiveA = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long a2 = ColdSum(smallMatrix);          // 第二次：已经编译好了
sw.Stop();
double naiveB = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"    第一次测「A」：{naiveA,7:F3} ms");
Console.WriteLine($"    第二次测「B」：{naiveB,7:F3} ms");
Console.WriteLine($"    结论：B 比 A 快 {naiveA / naiveB:F1} 倍！");
Console.WriteLine();
Console.WriteLine($"  但 A 和 B 是【同一个方法】—— 两次调用做的事一模一样（结果都是 {a1} 和 {a2}）。");
Console.WriteLine($"  第一次多花的那 {naiveA - naiveB:F3} ms，是 JIT 把这段代码【编译成机器码】的时间。");
Console.WriteLine();
Console.WriteLine("  这个陷阱在对比两个方法时更隐蔽：先测的那个总是吃亏，");
Console.WriteLine("  于是你会得出「后测的那个更快」—— 而它可能只是【沾了前面热身的光】。");
Console.WriteLine();

// ---------- 陷阱 2：只跑一次 ----------
Console.WriteLine("  ---------- 陷阱 2：只跑一次 ----------");
Console.WriteLine();

Console.WriteLine("  预热之后，连续跑 10 次同一段代码，把每次的数字都列出来：");
Console.WriteLine();
var runs = new List<double>();
for (int i = 0; i < 10; i++)
{
    var s = Stopwatch.StartNew();
    SumRowMajor(matrix);
    s.Stop();
    runs.Add(s.Elapsed.TotalMilliseconds);
}
Console.WriteLine($"    {string.Join("  ", runs.Select(x => x.ToString("F2")))}");
Console.WriteLine();
Console.WriteLine($"    最快 {runs.Min():F2} ms，最慢 {runs.Max():F2} ms，"
    + $"波动 {(runs.Max() - runs.Min()) / runs.Min():P0}");
Console.WriteLine();
Console.WriteLine("  同一段代码、同样的输入，数字在跳 —— 因为后台还有 GC、还有别的进程、");
Console.WriteLine("  还有 CPU 的频率调节。所以：");
Console.WriteLine();
Console.WriteLine("    · 只跑一次 = 抽一次签；");
Console.WriteLine("    · 跑 N 次【取最快】比取平均值更可靠 —— 最快的那次最接近「没有干扰」的真实耗时。");
Console.WriteLine();

// ---------- 陷阱 3：忽略 GC ----------
Console.WriteLine("  ---------- 陷阱 3：忽略 GC ----------");
Console.WriteLine();

long allocHeavy = MeasureAlloc(() => AllocateHeavy());
long allocLight = MeasureAlloc(() => AllocateLight());

Console.WriteLine("  同一个「求和」，一个版本每轮新建一个数组，一个版本复用同一块内存：");
Console.WriteLine();
Console.WriteLine($"    每轮新建数组：单次分配 {allocHeavy,10:N0} B");
Console.WriteLine($"    复用同一块内存：单次分配 {allocLight,10:N0} B");
Console.WriteLine();

double tHeavy = BestMs(() => AllocateHeavy(), 30);
double tLight = BestMs(() => AllocateLight(), 30);
Console.WriteLine($"    每轮新建数组：{tHeavy:F3} ms");
Console.WriteLine($"    复用同一块内存：{tLight:F3} ms");
Console.WriteLine();
Console.WriteLine("  【提醒】测内存一定要用 GC.GetAllocatedBytesForCurrentThread()，");
Console.WriteLine("  不要用 GC.GetTotalMemory 的差值 —— 12.1 节实测过它会给出");
Console.WriteLine("  「0.0 MB」和「-94.0 MB」这样的假结果。");
Console.WriteLine();

// ---------- 陷阱 4：忽略噪声区间 ----------
Console.WriteLine("  ---------- 陷阱 4：忽略噪声区间 ----------");
Console.WriteLine();

var aRuns = new List<double>();
var bRuns = new List<double>();
for (int i = 0; i < 8; i++)
{
    var s = Stopwatch.StartNew();
    SumRowMajor(matrix);
    s.Stop();
    aRuns.Add(s.Elapsed.TotalMilliseconds);

    s.Restart();
    SumColumnMajor(matrix);
    s.Stop();
    bRuns.Add(s.Elapsed.TotalMilliseconds);
}

Console.WriteLine("  行优先和列优先各跑 8 轮，原样列出：");
Console.WriteLine();
Console.WriteLine($"    行优先：{string.Join("  ", aRuns.Select(x => x.ToString("F2")))}");
Console.WriteLine($"    列优先：{string.Join("  ", bRuns.Select(x => x.ToString("F2")))}");
Console.WriteLine();
Console.WriteLine($"    行优先区间 [{aRuns.Min():F2}, {aRuns.Max():F2}] ms");
Console.WriteLine($"    列优先区间 [{bRuns.Min():F2}, {bRuns.Max():F2}] ms");
Console.WriteLine();
bool overlap = aRuns.Min() <= bRuns.Max() && bRuns.Min() <= aRuns.Max();
Console.WriteLine($"    两个区间是否重叠：{overlap}");
Console.WriteLine();

// ---------- 修正后的完整测法 ----------
Console.WriteLine("  ---------- 把四个陷阱都避开之后的测法 ----------");
Console.WriteLine();
Console.WriteLine("    1. 先各跑几遍预热（把 JIT 和缓存都焐热）");
Console.WriteLine("    2. 每项跑多轮，取【最快】的那次");
Console.WriteLine("    3. 报告内存分配（用 GetAllocatedBytesForCurrentThread）");
Console.WriteLine("    4. 把多轮的原始数字一并列出，让读者看到噪声");
Console.WriteLine();
Console.WriteLine($"  按这个测法，本节实验一的结论是：");
Console.WriteLine($"    行优先 {tRow:F2} ms，列优先 {tCol:F2} ms，差 {tCol / tRow:F2} 倍");
Console.WriteLine("  —— 这个倍率【远大于】上面的噪声波动，所以是可信的。");
Console.WriteLine();
Console.WriteLine("  而 13.1 节测过另一个场景（Kahn vs DFS，两个 O(V+E) 的实现），");
Console.WriteLine("  那里两个区间【互相重叠】，结论就是「分不出谁快」。");
Console.WriteLine();

// ---------- 死代码消除 ----------
Console.WriteLine("  ---------- 补充：第五个陷阱，死代码消除 ----------");
Console.WriteLine();
Console.WriteLine("  如果一段计算的结果【没有被使用】，编译器/JIT 有权把它整个删掉。");
Console.WriteLine();

double tUsed = BestMs(() => { var s = SumRowMajor(matrix); Use(s); }, 10);
double tDiscarded = BestMs(() => { SumRowMajor(matrix); }, 10);

Console.WriteLine("    结果被使用（打印/累加）：{0:F2} ms", tUsed);
Console.WriteLine("    结果被丢弃（直接调用）  ：{0:F2} ms", tDiscarded);
Console.WriteLine();
Console.WriteLine("  在 .NET 上这两个数字【很接近】—— 因为这里的方法没有被内联，");
Console.WriteLine("  调用本身的副作用（读内存）JIT 不敢随便删。");
Console.WriteLine("  但在 C/C++ 里这个陷阱非常致命（整个循环可能被优化掉）。");
Console.WriteLine();
Console.WriteLine("  稳妥的做法是【让结果流向一个能被观测到的地方】—— 本节所有测量都这么做了。");
Console.WriteLine();

// ==================== 算法实现 ====================

// 行优先：外层走行，内层走列 —— 顺着内存排布走
static long SumRowMajor(int[,] m)
{
    long sum = 0;
    int rows = m.GetLength(0), cols = m.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            sum += m[r, c];
    return sum;
}

// 列优先：外层走列，内层走行 —— 每一步都跨一整行
static long SumColumnMajor(int[,] m)
{
    long sum = 0;
    int rows = m.GetLength(0), cols = m.GetLength(1);
    for (int c = 0; c < cols; c++)
        for (int r = 0; r < rows; r++)
            sum += m[r, c];
    return sum;
}

// 与 SumRowMajor 代码完全相同 —— 它的存在只为一件事：
// 保证陷阱 1 里那次调用是【这个方法第一次被调用】，好让 JIT 的开销暴露出来
static long ColdSum(int[,] m)
{
    long sum = 0;
    int rows = m.GetLength(0), cols = m.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            sum += m[r, c];
    return sum;
}

// ---------- 陷阱 3 的两个版本 ----------

long AllocateHeavy()
{
    var local = new int[1_000_000];       // ★ 每轮新建
    for (int i = 0; i < local.Length; i++) local[i] = i;
    long sum = 0;
    foreach (int x in local) sum += x;
    return sum;
}

long AllocateLight()
{
    for (int i = 0; i < sharedBuffer.Length; i++) sharedBuffer[i] = i;   // ★ 复用同一块
    long sum = 0;
    foreach (int x in sharedBuffer) sum += x;
    return sum;
}

// ---------- 工具 ----------

void Use(long v) => sink ^= v;

static double BestMs(Action a, int rounds)
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

static long MeasureAlloc(Action a)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    a();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}
