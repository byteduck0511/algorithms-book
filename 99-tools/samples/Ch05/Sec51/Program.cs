using System.Runtime.CompilerServices;

// ==================== 实验一：栈的基本操作 ====================

Console.WriteLine("=== 实验一：后进先出（LIFO）===");
Console.WriteLine();

var stack = new MyStack<string>();

foreach (var name in new[] { "第一本", "第二本", "第三本" })
{
    stack.Push(name);
    Console.WriteLine($"  push({name,-8})  栈内: {stack}   (栈顶 = {stack.Peek()})");
}
Console.WriteLine();

while (!stack.IsEmpty)
{
    string top = stack.Pop();
    Console.WriteLine($"  pop() -> {top,-8}  栈内: {stack}");
}
Console.WriteLine();

Console.WriteLine("  注意：最后 push 进去的「第三本」最先出来 —— 这就是 LIFO（Last In First Out）。");
Console.WriteLine();

// 空栈操作
Console.WriteLine("  空栈上调用 Pop() 会怎样？");
try
{
    stack.Pop();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"    抛出异常: {ex.Message}   <- 而不是返回一个错误的值");
}
Console.WriteLine();

// ==================== 实验二：应用 —— 撤销操作 ====================

Console.WriteLine("=== 实验二：应用场景 —— 文本编辑器的撤销 ===");
Console.WriteLine();

var undoStack = new MyStack<string>();
var document = new System.Text.StringBuilder();

void Type(string text)
{
    undoStack.Push(document.ToString());     // 改之前先存一份快照
    document.Append(text);
    Console.WriteLine($"  输入 \"{text}\"      -> 当前内容: \"{document}\"");
}

void Undo()
{
    if (undoStack.IsEmpty) { Console.WriteLine("  没有可撤销的操作了"); return; }
    string previous = undoStack.Pop();        // 回到上一个快照
    Console.WriteLine($"  撤销          -> 当前内容: \"{previous}\"");
    document.Clear();
    document.Append(previous);
}

Type("你好");
Type("，世界");
Type("！");
Console.WriteLine();
Undo();
Undo();
Undo();
Undo();
Console.WriteLine();
Console.WriteLine("  撤销的实现天然就是栈：最后编辑的先被撤销。");
Console.WriteLine("  （真实编辑器用的是「命令模式 + 栈」，但栈这个核心是一样的）");
Console.WriteLine();

// ==================== 实验三：一个容易忽略的内存陷阱 ====================

Console.WriteLine("=== 实验三：弹出元素后，底层数组还持有它吗？ ===");
Console.WriteLine();

// 关键：对象的创建必须隔离到独立方法里（并且禁止内联）。
// 如果写在顶级语句里，那个局部变量的栈槽会一直活到方法结束，
// 导致「对象还活着」恒为 true —— 那是测试方法的假象，不是真实的泄漏。
WeakReference weakLeaky = LeakTest.PushThenPopLeaky();
WeakReference weakFixed = LeakTest.PushThenPopFixed();

GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

Console.WriteLine($"  LeakyStack（弹出时不清引用）: 对象还活着 = {weakLeaky.IsAlive}");
Console.WriteLine($"                                <- 底层数组 _items[0] 仍然指向它，GC 回收不了");
Console.WriteLine($"  MyStack（弹出时清引用）     : 对象还活着 = {weakFixed.IsAlive}");
Console.WriteLine($"                                <- 引用被置为 null，GC 正常回收");
Console.WriteLine();

Console.WriteLine("  这就是为什么 MyStack.Pop() 里要多写一行 `_items[_count] = default!;`");
Console.WriteLine("  对 int 这类值类型无所谓，但对引用类型，漏掉这行会造成「隐形内存泄漏」：");
Console.WriteLine("  元素明明已经出栈了，却因为底层数组还指着它而无法被回收。");
Console.WriteLine();

// ==================== 实验四：性能 —— 数组栈 vs 链表栈 ====================

Console.WriteLine("=== 实验四：数组实现的栈，性能如何 ===");
Console.WriteLine();

const int N = 1_000_000;
var perfStack = new MyStack<int>();

var sw = System.Diagnostics.Stopwatch.StartNew();
for (int i = 0; i < N; i++) perfStack.Push(i);
long afterPush = GC.GetAllocatedBytesForCurrentThread();

long sum = 0;
for (int i = 0; i < N; i++) sum += perfStack.Pop();
sw.Stop();

Console.WriteLine($"  压入并弹出 {N:N0} 个元素: {sw.Elapsed.TotalMilliseconds:F1} ms");
Console.WriteLine($"  push 阶段新分配内存: {(afterPush) / 1024.0 / 1024.0:F2} MB（数组扩容所致）");
Console.WriteLine($"  校验和 = {sum:N0}（应为 {(long)N * (N - 1) / 2:N0}）");
Console.WriteLine();
Console.WriteLine("  数组实现栈的代价和动态数组完全一样：push 是摊还 O(1)，偶尔扩容搬运一次。");
Console.WriteLine();

// ==================== 栈的实现 ====================

/// <summary>基于数组的栈。所有操作都是 O(1)（push 为摊还 O(1)）。</summary>
public class MyStack<T>
{
    private T[] _items;
    private int _count;

    public MyStack(int capacity = 4)
    {
        _items = new T[capacity];
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void Push(T item)
    {
        if (_count == _items.Length)
        {
            var bigger = new T[_items.Length * 2];
            Array.Copy(_items, bigger, _count);
            _items = bigger;
        }
        _items[_count] = item;
        _count++;
    }

    public T Pop()
    {
        if (_count == 0)
            throw new InvalidOperationException("栈为空，无法 Pop");

        _count--;
        T item = _items[_count];
        _items[_count] = default!;      // ← 清掉引用！对引用类型至关重要
        return item;
    }

    public T Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("栈为空，无法 Peek");
        return _items[_count - 1];
    }

    public override string ToString()
    {
        var parts = new string[_count];
        for (int i = 0; i < _count; i++) parts[i] = _items[i]?.ToString() ?? "null";
        return "[" + string.Join(", ", parts) + "]";
    }
}

/// <summary>故意写错的版本：Pop 时不清引用，用来演示「隐形内存泄漏」。</summary>
public class LeakyStack<T>
{
    private T[] _items = new T[16];
    private int _count;

    public void Push(T item) => _items[_count++] = item;

    public T Pop() => _items[--_count];     // ← 少了 _items[_count] = default!;
}

/// <summary>
/// 内存泄漏实验的辅助类。
/// 两个栈对象作为静态字段「活下去」，而测试对象只在方法内部创建 ——
/// 方法一返回，对测试对象的局部引用就消失了，此时还活着的就只能是栈内部持有的引用。
/// </summary>
public static class LeakTest
{
    private static readonly MyStack<object> FixedHolder = new();
    private static readonly LeakyStack<object> LeakyHolder = new();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static WeakReference PushThenPopLeaky()
    {
        object payload = new object();
        LeakyHolder.Push(payload);
        LeakyHolder.Pop();
        return new WeakReference(payload);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static WeakReference PushThenPopFixed()
    {
        object payload = new object();
        FixedHolder.Push(payload);
        FixedHolder.Pop();
        return new WeakReference(payload);
    }
}
