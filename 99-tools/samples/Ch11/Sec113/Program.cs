using System.Diagnostics;

// ==================== 完整实现一个优先队列 ====================

var pq = new MyPriorityQueue<int>();
var tasks = new (string Name, int Priority)[]
{
    ("写文档", 3), ("修 bug", 1), ("开会", 5), ("代码评审", 2), ("回邮件", 4),
};

Console.WriteLine("=== 实验一：优先队列的基本用法 ===");
Console.WriteLine();
Console.WriteLine("  任务清单（数字越小越优先）：");
foreach (var (name, priority) in tasks)
    Console.WriteLine($"    优先级 {priority}: {name}");
Console.WriteLine();

foreach (var (name, priority) in tasks) pq.Enqueue(priority);
Console.WriteLine($"  全部入队后，队列里有 {pq.Count} 个任务");
Console.WriteLine($"  当前最优先的（Peek）= 优先级 {pq.Peek()}");
Console.WriteLine();

Console.Write("  按优先级依次取出: ");
while (!pq.IsEmpty) Console.Write($"{pq.Dequeue()} ");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  注意：取出顺序是 1 2 3 4 5 —— 但入队顺序是 3 1 5 2 4。");
Console.WriteLine("  优先队列【不保证 FIFO】，它保证的是「每次取出当前最小的」。");
Console.WriteLine();

// ==================== 实验二：自定义比较器（变成最大堆）====================

Console.WriteLine("=== 实验二：用自定义比较器实现最大堆 ===");
Console.WriteLine();

// 传入一个「反向」比较器，小顶堆就变成了大顶堆
var maxPq = new MyPriorityQueue<int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
foreach (var (name, priority) in tasks) maxPq.Enqueue(priority);

Console.Write("  用反向比较器后，取出顺序: ");
while (!maxPq.IsEmpty) Console.Write($"{maxPq.Dequeue()} ");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  从 1 2 3 4 5 变成了 5 4 3 2 1 —— 同一个堆，换了个比较器就换了个方向。");
Console.WriteLine("  这就是为什么「大顶堆 vs 小顶堆」在实现上只是一行比较符号的差别。");
Console.WriteLine();

// ==================== 实验三：处理自定义类型 ====================

Console.WriteLine("=== 实验三：用自定义类型当元素 ===");
Console.WriteLine();

var jobQueue = new MyPriorityQueue<Job>();
jobQueue.Enqueue(new Job("低优先级任务", 3));
jobQueue.Enqueue(new Job("紧急任务", 1));
jobQueue.Enqueue(new Job("中等任务", 2));

Console.Write("  按优先级执行: ");
while (!jobQueue.IsEmpty)
{
    var job = jobQueue.Dequeue();
    Console.Write($"[{job.Name}] ");
}
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  PriorityQueue<Job> 通过 Job 实现的 IComparable<Job> 来比较。");
Console.WriteLine("  也可以像实验二那样，显式传入一个 IComparer<Job>。");
Console.WriteLine();

// ==================== 实验四：性能对比 ====================

Console.WriteLine("=== 实验四：优先队列 vs 每次排序 vs 线性扫描 ===");
Console.WriteLine();

const int N = 20_000;
var rng = new Random(42);
var data = new int[N];
for (int i = 0; i < N; i++) data[i] = rng.Next(0, 1_000_000);

// ---- 方案 A：优先队列，逐个取出 ----
var sw = Stopwatch.StartNew();
var heapQ = new MyPriorityQueue<int>();
foreach (int x in data) heapQ.Enqueue(x);
var resultA = new List<int>();
while (!heapQ.IsEmpty) resultA.Add(heapQ.Dequeue());
sw.Stop();
double heapMs = sw.Elapsed.TotalMilliseconds;

// ---- 方案 B：直接排序 ----
sw.Restart();
var resultB = (int[])data.Clone();
Array.Sort(resultB);
sw.Stop();
double sortMs = sw.Elapsed.TotalMilliseconds;

// ---- 方案 C：每次线性扫描找最小（模拟"不用堆"的做法）----
sw.Restart();
var resultC = new List<int>();
var remaining = new List<int>(data);
while (remaining.Count > 0)
{
    int minIdx = 0;
    for (int i = 1; i < remaining.Count; i++)
        if (remaining[i] < remaining[minIdx]) minIdx = i;
    resultC.Add(remaining[minIdx]);
    remaining.RemoveAt(minIdx);
}
sw.Stop();
double scanMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  把 {N:N0} 个数字全部按升序取出：");
Console.WriteLine();
Console.WriteLine($"  {"方案",-32} | {"耗时",12} | 复杂度");
Console.WriteLine("  " + new string('-', 62));
Console.WriteLine($"  {"优先队列（逐个 Enqueue/Dequeue）",-32} | {heapMs,9:F1} ms | O(n log n)");
Console.WriteLine($"  {"直接 Array.Sort",-32} | {sortMs,9:F1} ms | O(n log n)");
Console.WriteLine($"  {"线性扫描找最小（n 次）",-32} | {scanMs,9:F1} ms | O(n^2)");
Console.WriteLine();

bool allSame = resultA.SequenceEqual(resultB) && resultB.SequenceEqual(resultC);
Console.WriteLine($"  三种结果一致: {allSame}");
Console.WriteLine();
Console.WriteLine("  三个观察：");
Console.WriteLine($"    1. 优先队列和 Array.Sort 都是 O(n log n)，但 Array.Sort 快 {heapMs / sortMs:F1} 倍 ——");
Console.WriteLine($"       因为它是原地排序、缓存友好、常数小；而优先队列每次操作都要");
Console.WriteLine($"       维护堆结构（上浮/下沉 + 数组重排），常数明显更大。");
Console.WriteLine($"    2. 线性扫描是 O(n^2)，比优先队列慢了 {scanMs / heapMs:F0} 倍 —— 这就是堆的价值。");
Console.WriteLine($"    3. 【如果需求是「一次性全部排序」，直接用 Array.Sort】；");
Console.WriteLine($"       优先队列的价值在于「边插入边取」的动态场景（见实验五）。");
Console.WriteLine();

// ==================== 实验五：动态场景才是优先队列的主场 ====================

Console.WriteLine("=== 实验五：动态场景 —— 优先队列的真正价值 ===");
Console.WriteLine();

// 模拟：不断有新任务进来，每次都要取最紧急的那个
var dynamicQ = new MyPriorityQueue<int>();
sw.Restart();
long processCount = 0;
for (int i = 0; i < 100_000; i++)
{
    dynamicQ.Enqueue(rng.Next(0, 1_000_000));       // 新任务到达
    if (i % 10 == 0 && !dynamicQ.IsEmpty)
    {
        dynamicQ.Dequeue();                          // 处理最紧急的
        processCount++;
    }
}
sw.Stop();
double dynamicMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  模拟 10 万次「到达 + 处理」的混合负载：");
Console.WriteLine($"    处理了 {processCount:N0} 个任务，耗时 {dynamicMs:F1} ms");
Console.WriteLine();
Console.WriteLine("  如果用「每次处理前先排序一遍」的做法：");
Console.WriteLine($"    10 万次排序 × O(n log n) —— 完全不可行。");
Console.WriteLine();
Console.WriteLine("  【结论】优先队列的核心价值不是「排序更快」，");
Console.WriteLine("          而是「在数据不断进出的情况下，始终保持 O(log n) 的增删代价」。");
Console.WriteLine();

// ==================== 实现 ====================

/// <summary>一个基于数组（二叉堆）的优先队列，默认是最小优先。</summary>
public class MyPriorityQueue<T>
{
    private T[] _items;
    private int _count;
    private readonly IComparer<T> _comparer;

    public MyPriorityQueue(IComparer<T>? comparer = null, int capacity = 4)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        _items = new T[Math.Max(capacity, 1)];
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void Enqueue(T item)
    {
        if (_count == _items.Length) Grow();
        _items[_count] = item;
        _count++;
        SiftUp(_count - 1);
    }

    public T Dequeue()
    {
        if (_count == 0)
            throw new InvalidOperationException("优先队列为空，无法 Dequeue");

        T result = _items[0];              // 根就是最优元素

        _count--;
        if (_count > 0)
        {
            _items[0] = _items[_count];    // 末尾元素顶替到根
            SiftDown(0);
        }

        _items[_count] = default!;         // 清引用，避免内存泄漏（5.1 节的教训）
        return result;
    }

    public T Peek()
    {
        if (_count == 0) throw new InvalidOperationException("优先队列为空");
        return _items[0];
    }

    private void SiftUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_comparer.Compare(_items[parent], _items[i]) <= 0) break;
            (_items[parent], _items[i]) = (_items[i], _items[parent]);
            i = parent;
        }
    }

    private void SiftDown(int i)
    {
        while (true)
        {
            int best = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < _count && _comparer.Compare(_items[left], _items[best]) < 0) best = left;
            if (right < _count && _comparer.Compare(_items[right], _items[best]) < 0) best = right;

            if (best == i) break;

            (_items[i], _items[best]) = (_items[best], _items[i]);
            i = best;
        }
    }

    private void Grow()
    {
        var bigger = new T[_items.Length * 2];
        Array.Copy(_items, bigger, _count);
        _items = bigger;
    }
}

/// <summary>一个可比较的任务类型。</summary>
public class Job : IComparable<Job>
{
    public string Name { get; }
    public int Priority { get; }      // 数字越小越优先

    public Job(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }

    public int CompareTo(Job? other)
        => other == null ? 1 : Priority.CompareTo(other.Priority);
}
