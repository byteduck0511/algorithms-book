using System.Diagnostics;

// ==================== 实验一：Top-K —— 为什么用小顶堆 ====================

/// <summary>
/// 找出最大的 K 个元素。
/// 关键：用【小顶堆】，而且只保留 K 个元素。
/// </summary>
static List<int> TopK(int[] data, int k)
{
    var heap = new PriorityQueue<int, int>();     // .NET 内置：元素, 优先级

    foreach (int x in data)
    {
        if (heap.Count < k)
        {
            heap.Enqueue(x, x);                   // 还没满 K 个，直接进
        }
        else if (x > heap.Peek())                 // 比"当前 K 个里最小的"大
        {
            heap.Dequeue();                       // 淘汰掉那个最小的
            heap.Enqueue(x, x);
        }
        // 否则 x 不够格，直接扔掉
    }

    var result = new List<int>();
    while (heap.Count > 0) result.Add(heap.Dequeue());
    result.Reverse();                             // 小顶堆取出是升序，反转成降序
    return result;
}

Console.WriteLine("=== 实验一：Top-K —— 为什么用【小】顶堆？ ===");
Console.WriteLine();

int[] demo = { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
Console.WriteLine($"  数据: [{string.Join(", ", demo)}]");
Console.WriteLine($"  找最大的 3 个: [{string.Join(", ", TopK(demo, 3))}]");
Console.WriteLine($"  验证（排序后取末 3 个）: [{string.Join(", ", demo.OrderByDescending(x => x).Take(3))}]");
Console.WriteLine();

Console.WriteLine("  为什么用【小】顶堆而不是大顶堆？");
Console.WriteLine();
Console.WriteLine("    我们要的是「最大的 K 个」，直觉上好像该用大顶堆。");
Console.WriteLine("    但关键在于：我们【只需要淘汰最小的那个候选】。");
Console.WriteLine();
Console.WriteLine("    小顶堆的根 = 当前 K 个候选里【最小的】—— 正是最该被淘汰的；");
Console.WriteLine("    所以拿它和新元素比较，一次 Peek 就知道该不该换。");
Console.WriteLine();
Console.WriteLine("    如果用大顶堆：根是当前最大的，你没法用它判断「新元素该不该进来」——");
Console.WriteLine("    因为你要淘汰的是最小的那个，而它在堆底，拿不到。");
Console.WriteLine();
Console.WriteLine("  记忆方法：『要 K 个最大的，就用最小的那个当守门员』。");
Console.WriteLine();

// ==================== 实验二：Top-K 的性能优势 ====================

const int N = 1_000_000;
const int K = 10;
var rng = new Random(42);
var big = new int[N];
for (int i = 0; i < N; i++) big[i] = rng.Next(0, int.MaxValue);

// 预热
TopK(big.Take(1000).ToArray(), K);

var sw = Stopwatch.StartNew();
var topK = TopK(big, K);
sw.Stop();
double heapMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
var sorted = (int[])big.Clone();
Array.Sort(sorted);
var topBySort = sorted[^K..].Reverse().ToList();
sw.Stop();
double sortMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"=== 实验二：Top-{K} 的性能（n = {N:N0}）===");
Console.WriteLine();
Console.WriteLine($"  {"方案",-34} | {"耗时",12} | 复杂度");
Console.WriteLine("  " + new string('-', 66));
Console.WriteLine($"  {"小顶堆（只保留 K 个）",-34} | {heapMs,9:F1} ms | O(n log K)");
Console.WriteLine($"  {"完整排序后取前 K 个",-34} | {sortMs,9:F1} ms | O(n log n)");
Console.WriteLine();
Console.WriteLine($"  堆方案快 {sortMs / heapMs:F1} 倍，结果一致: {topK.SequenceEqual(topBySort)}");
Console.WriteLine();
Console.WriteLine($"  理论分析：");
Console.WriteLine($"    堆方案：n × log2(K) = {N:N0} × {Math.Log2(K):F1} ≈ {N * Math.Log2(K):N0} 次操作");
Console.WriteLine($"    排序  ：n × log2(n) = {N:N0} × {Math.Log2(N):F1} ≈ {N * Math.Log2(N):N0} 次操作");
Console.WriteLine($"    理论倍率：{Math.Log2(N) / Math.Log2(K):F1} 倍");
Console.WriteLine();
Console.WriteLine($"  【K 越小，优势越大】—— 因为堆方案和 K 的对数相关，和 n 无关。");
Console.WriteLine($"  如果 K = 100，log2(100) ≈ 6.6，优势会更小；");
Console.WriteLine($"  如果 K = 100 万（接近 n），那还不如直接排序。");
Console.WriteLine();

// K 对性能的影响
Console.WriteLine("  K 的大小对优势的影响：");
Console.WriteLine($"  {"K",10} | {"堆方案(ms)",12} | {"排序(ms)",12} | {"倍率",8}");
Console.WriteLine("  " + new string('-', 52));

foreach (int k in new[] { 10, 100, 1000, 100_000 })
{
    sw.Restart();
    var _ = TopK(big, k);
    sw.Stop();
    double h = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var s = (int[])big.Clone();
    Array.Sort(s);
    var __ = s[^k..];
    sw.Stop();
    double srt = sw.Elapsed.TotalMilliseconds;

    Console.WriteLine($"{k,10:N0} | {h,12:F1} | {srt,12:F1} | {srt / h,7:F1} 倍");
}
Console.WriteLine();
Console.WriteLine("  注意 K = 100,000 那一行：堆方案的优势大幅缩小甚至反超 ——");
Console.WriteLine("  因为「保留 10 万个元素的堆」本身的维护成本也很可观，");
Console.WriteLine("  而且它还要分配 10 万个元素的数组。");
Console.WriteLine("  【结论：Top-K 用堆，是在 K << n 时才划算。】");
Console.WriteLine();

// ==================== 实验三：合并 K 个有序序列 ====================

/// <summary>用优先队列合并 K 个有序数组。</summary>
static List<int> MergeKSorted(List<int[]> lists)
{
    // 堆里存 (值, 来自哪个数组, 在该数组中的位置)，按值排序
    var pq = new PriorityQueue<(int Value, int ListIdx, int Pos), int>();

    for (int i = 0; i < lists.Count; i++)
    {
        if (lists[i].Length > 0)
            pq.Enqueue((lists[i][0], i, 0), lists[i][0]);      // 每个数组的头元素先进堆
    }

    var result = new List<int>();
    while (pq.Count > 0)
    {
        var (value, listIdx, pos) = pq.Dequeue();
        result.Add(value);

        // 从这个数组里再取下一个元素放进堆
        if (pos + 1 < lists[listIdx].Length)
        {
            int next = lists[listIdx][pos + 1];
            pq.Enqueue((next, listIdx, pos + 1), next);
        }
    }
    return result;
}

Console.WriteLine("=== 实验三：合并 K 个有序序列 ===");
Console.WriteLine();

var lists = new List<int[]>
{
    new[] { 1, 5, 9, 13 },
    new[] { 2, 6, 10 },
    new[] { 3, 7, 11, 15, 19 },
    new[] { 4, 8, 12, 16 },
};

Console.WriteLine("  要合并的 4 个有序数组：");
foreach (var list in lists) Console.WriteLine($"    [{string.Join(", ", list)}]");
Console.WriteLine();

var merged = MergeKSorted(lists);
Console.WriteLine($"  合并结果: [{string.Join(", ", merged)}]");

var expected = lists.SelectMany(x => x).OrderBy(x => x).ToList();
Console.WriteLine($"  正确结果: [{string.Join(", ", expected)}]");
Console.WriteLine($"  一致: {merged.SequenceEqual(expected)}");
Console.WriteLine();

Console.WriteLine("  算法思路：");
Console.WriteLine("    1. 先把每个数组的【头元素】放进小顶堆");
Console.WriteLine("    2. 每次从堆里取最小的 -> 它就是全局下一个该输出的");
Console.WriteLine("    3. 从【刚取出的那个元素所在的数组】里，再取下一个放进堆");
Console.WriteLine();
Console.WriteLine("  复杂度：O(N log K)，N 是总元素数，K 是数组个数。");
Console.WriteLine("  如果改成「两个两个合并」（归并排序的思路），是 O(N log K) 但常数更大；");
Console.WriteLine("  如果改成「每次线性扫描 K 个头元素」，是 O(N·K)。");
Console.WriteLine();

// ==================== 实验四：.NET 内置的 PriorityQueue ====================

Console.WriteLine("=== 实验四：.NET 内置的 PriorityQueue ===");
Console.WriteLine();

// 基本用法：元素，优先级
var pq = new PriorityQueue<string, int>();
pq.Enqueue("低优先级", 3);
pq.Enqueue("高优先级", 1);
pq.Enqueue("中优先级", 2);

Console.Write("  依次出队: ");
while (pq.TryDequeue(out string? item, out int priority))
    Console.Write($"{item}({priority}) ");
Console.WriteLine();
Console.WriteLine();

Console.WriteLine("  注意它的泛型签名：PriorityQueue<TElement, TPriority>");
Console.WriteLine("    - 第一个类型是「元素」（真正想存的东西）");
Console.WriteLine("    - 第二个类型是「优先级」（用来排序的键）");
Console.WriteLine("    - 两者可以不同！比如 PriorityQueue<Job, int> —— 任务对象 + 优先级数字");
Console.WriteLine();

Console.WriteLine($"  {"常用 API",-34} | 说明");
Console.WriteLine("  " + new string('-', 70));
Console.WriteLine($"  {"Enqueue(element, priority)",-34} | 入队");
Console.WriteLine($"  {"Dequeue() / TryDequeue(...)",-34} | 出队（优先级最小的先出）");
Console.WriteLine($"  {"Peek() / TryPeek(...)",-34} | 看队首（不移除）");
Console.WriteLine($"  {"Count",-34} | 元素个数");
Console.WriteLine($"  {"Remove(element, out ...)",-34} | 删除指定元素（.NET 6+，O(log n)）");
Console.WriteLine($"  {"EnqueueDequeue(...)",-34} | 入队并出队，比分开调用快");
Console.WriteLine($"  {"UnorderedItems",-34} | 无序枚举（比有序取出快得多）");
Console.WriteLine();

Console.WriteLine("  【和我们的 MyPriorityQueue 对比】：");
Console.WriteLine("    - 内置版支持「元素」和「优先级」分离，我们的版本要求元素自己可比较");
Console.WriteLine("    - 内置版有 Remove / EnqueueDequeue / UnorderedItems 等完整 API");
Console.WriteLine("    - 内置版经过了充分测试，处理了各种边界情况");
Console.WriteLine("    → 生产环境用内置版");
Console.WriteLine();

// ==================== 实验五：常见陷阱 ====================

Console.WriteLine("=== 实验五：两个容易踩的坑 ===");
Console.WriteLine();

// 坑 1：相同优先级的出队顺序不确定
var samePriority = new PriorityQueue<string, int>();
samePriority.Enqueue("A", 1);
samePriority.Enqueue("B", 1);
samePriority.Enqueue("C", 1);
samePriority.Enqueue("D", 1);

Console.Write("  坑 1 —— 四个元素优先级都是 1，出队顺序: ");
while (samePriority.TryDequeue(out string? s, out _)) Console.Write($"{s} ");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("    这不是入队顺序（A B C D），也不是字母序 —— 它是【堆的内部结构顺序】。");
Console.WriteLine("    【结论】优先队列不保证相同优先级元素的先后 —— 需要 FIFO 时，");
Console.WriteLine("            要在优先级里加上「入队序号」当决胜条件（呼应 7.2 节的多关键字排序）。");
Console.WriteLine();

// 坑 2：约定「数字越大越紧急」时的方向问题
Console.WriteLine("  坑 2 —— 业务约定「数字越大越紧急」时，直接传数字会【反】");
Console.WriteLine();
Console.WriteLine("    假设业务约定：优先级数字【越大越紧急】（5 = 最紧急，1 = 最不紧急）");
Console.WriteLine();

var wrongDir = new PriorityQueue<string, int>();
wrongDir.Enqueue("普通任务", 1);       // 不紧急
wrongDir.Enqueue("紧急任务", 5);       // 很紧急
Console.Write("    直接传数字 -> ");
while (wrongDir.TryDequeue(out string? s2, out _)) Console.Write($"{s2} ");
Console.WriteLine("  <- 错了！最不紧急的先出队");
Console.WriteLine();
Console.WriteLine("    原因：.NET 的 PriorityQueue 永远是【优先级最小的先出】——");
Console.WriteLine("          它不关心你的业务约定是「数字大更紧急」还是「数字小更紧急」。");
Console.WriteLine();

var maxFirst = new PriorityQueue<string, int>();
maxFirst.Enqueue("普通任务", -1);      // 取负：-1
maxFirst.Enqueue("紧急任务", -5);      // 取负：-5
Console.Write("    把数字【取负】后再传 -> ");
while (maxFirst.TryDequeue(out string? s3, out _)) Console.Write($"{s3} ");
Console.WriteLine("  <- 对了 ✓");
Console.WriteLine();
Console.WriteLine("    取负之后，原本大的数字变成了小的（5 -> -5），就会被优先取出。");
Console.WriteLine("    另一种做法：传一个自定义的 IComparer<int>，把比较方向反过来。");
Console.WriteLine();
