using System.Diagnostics;

// ==================== 计数排序 ====================
//
// 前提：元素是非负整数，且最大值不大（能开一个 maxValue+1 的计数数组）

/// <summary>朴素计数排序（不保证稳定）。</summary>
static void CountingSort(int[] a, int maxValue, ref long ops)
{
    var count = new int[maxValue + 1];

    foreach (int x in a) { count[x]++; ops++; }          // 统计每个值出现几次

    int idx = 0;
    for (int v = 0; v <= maxValue; v++)                  // 按值的顺序从小到大输出
        for (int k = 0; k < count[v]; k++)
        {
            a[idx++] = v;
            ops++;
        }
}

/// <summary>稳定版计数排序 —— 基数排序的基础。</summary>
static int[] CountingSortStable(int[] a, int maxValue, ref long ops)
{
    var count = new int[maxValue + 1];

    foreach (int x in a) { count[x]++; ops++; }

    // 前缀和：count[i] 变成「小于等于 i 的元素有几个」
    for (int i = 1; i <= maxValue; i++) { count[i] += count[i - 1]; ops++; }

    var output = new int[a.Length];
    // 关键：从后往前遍历，把元素放到「它该在的最后一个位置」
    // 这样相等的元素会保持原来的相对顺序 —— 这就是稳定性的来源
    for (int i = a.Length - 1; i >= 0; i--)
    {
        output[--count[a[i]]] = a[i];
        ops++;
    }
    return output;
}

// ==================== 基数排序（LSD）====================

/// <summary>
/// 基数排序：从最低位到最高位，每一位做一次【稳定】的计数排序。
/// </summary>
static void RadixSort(int[] a, ref long ops)
{
    if (a.Length == 0) return;

    int max = a.Max();

    // exp 依次取 1, 10, 100, ... 代表当前处理的是哪一位
    for (int exp = 1; max / exp > 0; exp *= 10)
    {
        // 按 (a[i] / exp) % 10 这一位做稳定的计数排序（数字 0~9，只有 10 个桶）
        var count = new int[10];
        foreach (int x in a) { count[(x / exp) % 10]++; ops++; }

        for (int i = 1; i < 10; i++) { count[i] += count[i - 1]; ops++; }

        var output = new int[a.Length];
        for (int i = a.Length - 1; i >= 0; i--)
        {
            output[--count[(a[i] / exp) % 10]] = a[i];
            ops++;
        }

        Array.Copy(output, a, a.Length);
    }
}

// 对照用的快排
static void QuickSort(int[] a, int lo, int hi, ref long cmp, Random rng)
{
    if (lo >= hi) return;
    int r = rng.Next(lo, hi + 1);
    (a[r], a[hi]) = (a[hi], a[r]);
    int pivot = a[hi], i = lo - 1;
    for (int j = lo; j < hi; j++)
    {
        cmp++;
        if (a[j] <= pivot) { i++; (a[i], a[j]) = (a[j], a[i]); }
    }
    (a[i + 1], a[hi]) = (a[hi], a[i + 1]);
    QuickSort(a, lo, i, ref cmp, rng);
    QuickSort(a, i + 2, hi, ref cmp, rng);
}

// ==================== 实验一：计数排序 ====================

Console.WriteLine("=== 实验一：计数排序 ===");
Console.WriteLine();

int[] grades = { 85, 92, 78, 85, 95, 78, 92, 85, 60, 100, 78, 85 };
Console.WriteLine($"  原始成绩: [{string.Join(", ", grades)}]");

var gc = (int[])grades.Clone();
long o1 = 0;
CountingSort(gc, 100, ref o1);
Console.WriteLine($"  排序后  : [{string.Join(", ", gc)}]");
Console.WriteLine($"  操作次数: {o1:N0}（统计 {grades.Length} 次 + 输出 {grades.Length} 次）");
Console.WriteLine();

Console.WriteLine("  注意：它【一次比较都没做】—— 直接看「这个数等于几」，放到对应位置。");
Console.WriteLine("  这就是它跳出「比较模型」（7.4 节）的方式。");
Console.WriteLine();

// ==================== 实验二：稳定性 ====================

Console.WriteLine("=== 实验二：稳定版计数排序 ===");
Console.WriteLine();

// 用 (值, 原始下标) 来检验稳定性
int[] keys = { 3, 1, 3, 2, 1, 3 };
Console.WriteLine($"  原始: [{string.Join(", ", keys)}]（下标 0..5）");
Console.WriteLine();

// 手工演示稳定版的过程
var count = new int[4];
foreach (int x in keys) count[x]++;
Console.WriteLine($"  第一步：统计次数      count = [_, {count[1]}, {count[2]}, {count[3]}]  (下标 0..3)");

for (int i = 1; i < 4; i++) count[i] += count[i - 1];
Console.WriteLine($"  第二步：转成前缀和    count = [_, {count[1]}, {count[2]}, {count[3]}]");
Console.WriteLine($"          含义：值 <= i 的元素有 count[i] 个");

long o2 = 0;
var sorted = CountingSortStable(keys, 3, ref o2);
Console.WriteLine($"  第三步：从后往前放置 -> [{string.Join(", ", sorted)}]");
Console.WriteLine();

// 用带标记的版本验证稳定性
int[] withMark = { 30, 10, 31, 20, 11, 32 };   // 十位是值，个位是原始序号
Console.WriteLine($"  稳定性验证（十位是排序关键字，个位是原始顺序）:");
Console.WriteLine($"    原始: [{string.Join(", ", withMark)}]");
long o3 = 0;
var sortedMark = CountingSortStable(withMark, 32, ref o3);
Console.WriteLine($"    排序后: [{string.Join(", ", sortedMark)}]");
Console.WriteLine($"    （注意：这条演示用的是整个数值当键，下面用真正的方式验证）");
Console.WriteLine();

// 真正验证稳定性：用元组
var items = new[] { (Key: 3, Seq: 0), (Key: 1, Seq: 1), (Key: 3, Seq: 2), (Key: 2, Seq: 3), (Key: 1, Seq: 4), (Key: 3, Seq: 5) };
Console.WriteLine("  用 (值, 原始序号) 验证：");
Console.WriteLine($"    原始: {string.Join(" ", items.Select(x => $"{x.Key}#{x.Seq}"))}");

// 稳定的计数排序（按 Key）
var cnt = new int[4];
foreach (var it in items) cnt[it.Key]++;
for (int i = 1; i < 4; i++) cnt[i] += cnt[i - 1];
var outItems = new (int Key, int Seq)[items.Length];
for (int i = items.Length - 1; i >= 0; i--)
    outItems[--cnt[items[i].Key]] = items[i];

Console.WriteLine($"    排序后: {string.Join(" ", outItems.Select(x => $"{x.Key}#{x.Seq}"))}");
bool stable = true;
for (int i = 1; i < outItems.Length; i++)
    if (outItems[i].Key == outItems[i - 1].Key && outItems[i].Seq < outItems[i - 1].Seq) stable = false;
Console.WriteLine($"    稳定性: {(stable ? "稳定 ✓（同值的 #序号 保持递增）" : "不稳定 ✗")}");
Console.WriteLine();

// ==================== 实验三：基数排序 ====================

Console.WriteLine("=== 实验三：基数排序 ===");
Console.WriteLine();

int[] ids = { 329, 457, 657, 839, 436, 720, 355 };
Console.WriteLine($"  原始: [{string.Join(", ", ids)}]");
Console.WriteLine();
Console.WriteLine("  LSD 基数排序：从【个位】开始，每一位做一次稳定的计数排序");
Console.WriteLine();

// 逐步演示
var step = (int[])ids.Clone();
for (int exp = 1, round = 1; exp <= 100; exp *= 10, round++)
{
    var c = new int[10];
    foreach (int x in step) c[(x / exp) % 10]++;
    for (int i = 1; i < 10; i++) c[i] += c[i - 1];
    var outp = new int[step.Length];
    for (int i = step.Length - 1; i >= 0; i--)
        outp[--c[(step[i] / exp) % 10]] = step[i];
    Array.Copy(outp, step, step.Length);

    string label = exp == 1 ? "个位" : exp == 10 ? "十位" : "百位";
    Console.WriteLine($"    按{label}排序后: [{string.Join(", ", step)}]");
}
Console.WriteLine();
Console.WriteLine("  关键：每一步都必须是【稳定】的排序。");
Console.WriteLine("  因为「低位已经排好的顺序」必须被保留下来 —— 这就是 7.2 节讲的「从低位到高位排序」。");
Console.WriteLine();

// ==================== 实验四：性能对比 ====================

const int N = 1_000_000;
var rng = new Random(42);
var sw = new Stopwatch();

Console.WriteLine($"=== 实验四：非比较排序 vs 快速排序（n = {N:N0}）===");
Console.WriteLine();

// --- 场景 1：小值域 ---
var smallRange = new int[N];
for (int i = 0; i < N; i++) smallRange[i] = rng.Next(0, 1000);
Console.WriteLine("  场景 1：值域 0~999（很小）");

var sr = (int[])smallRange.Clone();
long ops1 = 0;
sw.Restart(); CountingSort(sr, 999, ref ops1); sw.Stop();
double countingMs = sw.Elapsed.TotalMilliseconds;

var srq = (int[])smallRange.Clone();
long cmp1 = 0;
sw.Restart(); QuickSort(srq, 0, srq.Length - 1, ref cmp1, new Random(1)); sw.Stop();
double quick1Ms = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"    计数排序: {countingMs,8:F1} ms（操作 {ops1:N0} 次）");
Console.WriteLine($"    快速排序: {quick1Ms,8:F1} ms（比较 {cmp1:N0} 次）");
Console.WriteLine($"    计数排序快 {quick1Ms / countingMs:F1} 倍，结果一致={sr.SequenceEqual(srq)}");
Console.WriteLine();

// --- 场景 2：大值域 ---
var bigRange = new int[N];
for (int i = 0; i < N; i++) bigRange[i] = rng.Next(0, 1_000_000_000);
Console.WriteLine("  场景 2：值域 0~999,999,999（很大，10 位数）");

var br = (int[])bigRange.Clone();
long ops2 = 0;
sw.Restart(); RadixSort(br, ref ops2); sw.Stop();
double radixMs = sw.Elapsed.TotalMilliseconds;

var brq = (int[])bigRange.Clone();
long cmp2 = 0;
sw.Restart(); QuickSort(brq, 0, brq.Length - 1, ref cmp2, new Random(1)); sw.Stop();
double quick2Ms = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"    基数排序: {radixMs,8:F1} ms（操作 {ops2:N0} 次）");
Console.WriteLine($"    快速排序: {quick2Ms,8:F1} ms（比较 {cmp2:N0} 次）");
double ratio = radixMs / quick2Ms;
Console.WriteLine($"    {(ratio < 1 ? $"基数排序快 {1 / ratio:F1} 倍" : $"快速排序快 {ratio:F1} 倍")}，结果一致={br.SequenceEqual(brq)}");
Console.WriteLine();

Console.WriteLine("  结论：");
Console.WriteLine("    值域很小时，计数排序碾压快排 —— 它根本不比较，直接按值定位；");
Console.WriteLine("    值域很大时，计数排序用不了（要开 10 亿长度的数组），");
Console.WriteLine("    只能改用基数排序 —— 按位拆分，每一位只需要 10 个桶。");
Console.WriteLine();

// --- 场景 3：值域大到无法用计数排序 ---
Console.WriteLine("  场景 3：如果非要用计数排序排 0~999,999,999 的数呢？");
Console.WriteLine($"    需要开一个 10 亿长度的 int 数组 = {1_000_000_000L * 4 / 1024 / 1024 / 1024:F0} GB 内存");
Console.WriteLine($"    而实际只有 {N:N0} 个元素 —— 内存利用率 0.1%");
Console.WriteLine("    -> 完全不可行。这就是计数排序的前提条件为什么重要。");
Console.WriteLine();
