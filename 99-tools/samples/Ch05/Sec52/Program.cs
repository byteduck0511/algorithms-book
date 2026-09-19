using System.Diagnostics;

// ==================== 实验一：环形缓冲区是怎么转的 ====================

Console.WriteLine("=== 实验一：环形缓冲区的内部状态 ===");
Console.WriteLine();

var q = new MyQueue<string>(4);
Console.WriteLine($"  新建队列（容量 4）: {q.DebugInfo()}");
Console.WriteLine();

foreach (var v in new[] { "A", "B", "C" })
{
    q.Enqueue(v);
    Console.WriteLine($"  Enqueue({v}) -> {q.DebugInfo()}");
}
Console.WriteLine();

foreach (var _ in new[] { 1, 2 })
{
    string v = q.Dequeue();
    Console.WriteLine($"  Dequeue() 得到 {v} -> {q.DebugInfo()}");
}
Console.WriteLine();

Console.WriteLine("  注意看 head 的位置：出队时 head 只是往后挪，没有任何元素被搬动。");
Console.WriteLine();

// 再入队几个，观察「绕回去」
foreach (var v in new[] { "D", "E", "F" })
{
    q.Enqueue(v);
    Console.WriteLine($"  Enqueue({v}) -> {q.DebugInfo()}");
}
Console.WriteLine();

Console.WriteLine("  看 Enqueue(E) 那一行：E 被放到了下标 0，也就是数组的最前面 —— 队尾绕回去了。");
Console.WriteLine("  这就是「环形」：数组前面的空位（出队时腾出来的）被重新利用，不会浪费。");
Console.WriteLine("  如果是 List.RemoveAt(0)，前面那块空间就白白空着了，还得靠搬移元素来「补位」。");
Console.WriteLine();

// ==================== 实验二：为什么不能用 List 当队列 ====================

Console.WriteLine("=== 实验二：三种「队列」实现的性能对比 ===");
Console.WriteLine();

const int N = 100_000;
var sw = new Stopwatch();

// ---------- 错误做法：List.Add + List.RemoveAt(0) ----------
sw.Restart();
var listQueue = new List<int>();
for (int i = 0; i < N; i++) listQueue.Add(i);
while (listQueue.Count > 0) listQueue.RemoveAt(0);
sw.Stop();
double listMs = sw.Elapsed.TotalMilliseconds;

// ---------- 我们的环形缓冲区 ----------
sw.Restart();
var myQueue = new MyQueue<int>();
for (int i = 0; i < N; i++) myQueue.Enqueue(i);
long sum1 = 0;
while (!myQueue.IsEmpty) sum1 += myQueue.Dequeue();
sw.Stop();
double mineMs = sw.Elapsed.TotalMilliseconds;

// ---------- C# 内置 Queue<T> ----------
sw.Restart();
var dotnetQueue = new Queue<int>();
for (int i = 0; i < N; i++) dotnetQueue.Enqueue(i);
long sum2 = 0;
while (dotnetQueue.Count > 0) sum2 += dotnetQueue.Dequeue();
sw.Stop();
double dotnetMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  入队 + 出队各 {N:N0} 次：");
Console.WriteLine();
Console.WriteLine($"  List.Add + List.RemoveAt(0) : {listMs,9:F2} ms   出队 O(n)，整体 O(n^2)");
Console.WriteLine($"  环形缓冲区（我们自己实现）  : {mineMs,9:F2} ms   出入队都是 O(1)");
Console.WriteLine($"  C# 内置 Queue<T>            : {dotnetMs,9:F2} ms   出入队都是 O(1)");
Console.WriteLine();
Console.WriteLine($"  List 做法慢了 {listMs / mineMs:F0} 倍。");
Console.WriteLine($"  校验和一致: {sum1 == sum2}（{sum1:N0}）");
Console.WriteLine();

// 放大规模看差距怎么变
Console.WriteLine("  规模放大后的差距：");
Console.WriteLine($"  {"数据量",12} | {"List.RemoveAt(0)",18} | {"环形缓冲区",14} | {"倍数",8}");
foreach (int n in new[] { 25_000, 50_000, 100_000 })
{
    sw.Restart();
    var lq = new List<int>();
    for (int i = 0; i < n; i++) lq.Add(i);
    while (lq.Count > 0) lq.RemoveAt(0);
    sw.Stop();
    double lms = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    var mq = new MyQueue<int>();
    for (int i = 0; i < n; i++) mq.Enqueue(i);
    while (!mq.IsEmpty) mq.Dequeue();
    sw.Stop();
    double mms = sw.Elapsed.TotalMilliseconds;

    Console.WriteLine($"  {n,12:N0} | {lms,15:F2} ms | {mms,11:F2} ms | {lms / mms,7:F0} 倍");
}
Console.WriteLine();
Console.WriteLine("  数据量涨 4 倍，List 做法的耗时涨约 16 倍 —— 典型的 O(n^2)。");
Console.WriteLine("  环形缓冲区始终是线性的。");
Console.WriteLine();

// ==================== 实验三：双端队列 ====================

Console.WriteLine("=== 实验三：双端队列（Deque）===");
Console.WriteLine();

var deque = new MyDeque<string>(4);
deque.PushBack("B");
deque.PushBack("C");
deque.PushFront("A");           // 从前面插入
deque.PushBack("D");
Console.WriteLine($"  依次 PushBack(B/C/D)、PushFront(A) 后: {deque}");

Console.WriteLine($"  PopFront() = {deque.PopFront()}   剩余: {deque}");
Console.WriteLine($"  PopBack()  = {deque.PopBack()}   剩余: {deque}");
Console.WriteLine();
Console.WriteLine("  双端队列 = 栈 + 队列。两端都能 O(1) 进出。");
Console.WriteLine("  C# 没有内置的 Deque，需要时可以用 LinkedList<T>，或者自己写一个（就像上面这样）。");
Console.WriteLine();

// ==================== 队列的实现 ====================

/// <summary>
/// 基于「环形缓冲区」的队列。
/// 关键：出队时不搬移任何元素，只移动 head 下标，绕回数组开头继续用。
/// </summary>
public class MyQueue<T>
{
    private T[] _items;
    private int _head;      // 队首元素所在的下标
    private int _count;     // 元素个数

    public MyQueue(int capacity = 4)
    {
        _items = new T[Math.Max(capacity, 1)];
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    // 队尾的下标 = 队首 + 元素个数，超出容量就绕回去
    private int TailIndex => (_head + _count) % _items.Length;

    public void Enqueue(T item)
    {
        if (_count == _items.Length) Grow();
        _items[TailIndex] = item;
        _count++;
    }

    public T Dequeue()
    {
        if (_count == 0)
            throw new InvalidOperationException("队列为空，无法 Dequeue");

        T item = _items[_head];
        _items[_head] = default!;                       // 清引用，避免隐形内存泄漏
        _head = (_head + 1) % _items.Length;            // head 前移，不搬任何元素
        _count--;
        return item;
    }

    public T Peek()
    {
        if (_count == 0) throw new InvalidOperationException("队列为空");
        return _items[_head];
    }

    private void Grow()
    {
        var bigger = new T[_items.Length * 2];
        // 扩容时把环形「拉直」：按出队顺序依次复制到新数组的开头
        for (int i = 0; i < _count; i++)
            bigger[i] = _items[(_head + i) % _items.Length];
        _items = bigger;
        _head = 0;
    }

    /// <summary>把底层数组原样打印出来，好看清「环形」到底是怎么绕的。</summary>
    public string DebugInfo()
    {
        var slots = new string[_items.Length];
        for (int i = 0; i < _items.Length; i++)
            slots[i] = _items[i]?.ToString() ?? "_";
        return $"底层数组[{string.Join(",", slots)}]  head={_head}  ->  队列: [{ToString()}]";
    }

    public override string ToString()
    {
        var parts = new string[_count];
        for (int i = 0; i < _count; i++)
            parts[i] = _items[(_head + i) % _items.Length]?.ToString() ?? "null";
        return string.Join(", ", parts);
    }
}

/// <summary>双端队列：两端都能 O(1) 插入和删除。</summary>
public class MyDeque<T>
{
    private T[] _items;
    private int _head;
    private int _count;

    public MyDeque(int capacity = 4)
    {
        _items = new T[Math.Max(capacity, 1)];
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void PushBack(T item)
    {
        if (_count == _items.Length) Grow();
        _items[(_head + _count) % _items.Length] = item;
        _count++;
    }

    public void PushFront(T item)
    {
        if (_count == _items.Length) Grow();
        _head = (_head - 1 + _items.Length) % _items.Length;   // 往前挪一格（绕回）
        _items[_head] = item;
        _count++;
    }

    public T PopFront()
    {
        if (_count == 0) throw new InvalidOperationException("双端队列为空");
        T item = _items[_head];
        _items[_head] = default!;
        _head = (_head + 1) % _items.Length;
        _count--;
        return item;
    }

    public T PopBack()
    {
        if (_count == 0) throw new InvalidOperationException("双端队列为空");
        int tail = (_head + _count - 1) % _items.Length;
        T item = _items[tail];
        _items[tail] = default!;
        _count--;
        return item;
    }

    private void Grow()
    {
        var bigger = new T[_items.Length * 2];
        for (int i = 0; i < _count; i++)
            bigger[i] = _items[(_head + i) % _items.Length];
        _items = bigger;
        _head = 0;
    }

    public override string ToString()
    {
        var parts = new string[_count];
        for (int i = 0; i < _count; i++)
            parts[i] = _items[(_head + i) % _items.Length]?.ToString() ?? "null";
        return "[" + string.Join(", ", parts) + "]";
    }
}
