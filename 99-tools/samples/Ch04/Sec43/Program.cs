// ==================== 用带哨兵节点的实现，跑一遍所有边界情况 ====================

var list = new MyLinkedList<string>();
int passed = 0, failed = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"  [{(condition ? "通过" : "失败")}] {name}");
    if (condition) passed++; else failed++;
}

Console.WriteLine("=== 边界测试：链表最容易写错的地方都在这 ===");
Console.WriteLine();

// ---------- 边界 1：空表 ----------
Console.WriteLine("边界 1：空表");
Check("空表 Count 是 0", list.Count == 0);
Check("空表 Remove 返回 false（不能崩）", list.Remove("X") == false);
Check("空表 Contains 返回 false", list.Contains("X") == false);
Check("空表打印出来是空串", list.ToString() == "");
Console.WriteLine();

// ---------- 边界 2：往空表里插入 ----------
Console.WriteLine("边界 2：往空表里插入");
list.AddFirst("A");
Check("空表 AddFirst 后 Count = 1", list.Count == 1);
Check("空表 AddFirst 后内容正确", list.ToString() == "A");

list.Clear();
list.AddLast("B");
Check("空表 AddLast 后 Count = 1", list.Count == 1);
Check("空表 AddLast 后内容正确", list.ToString() == "B");
Console.WriteLine();

// ---------- 边界 3：头尾操作 ----------
Console.WriteLine("边界 3：头尾操作");
list.Clear();
list.AddLast("1");
list.AddLast("2");
list.AddLast("3");
Check("连续 AddLast 得到 1->2->3", list.ToString() == "1 -> 2 -> 3");

list.AddFirst("0");
Check("AddFirst 后得到 0->1->2->3", list.ToString() == "0 -> 1 -> 2 -> 3");
Check("AddFirst 后 Count = 4", list.Count == 4);

Check("删除头节点成功", list.Remove("0"));
Check("删除头后得到 1->2->3", list.ToString() == "1 -> 2 -> 3");

Check("删除尾节点成功", list.Remove("3"));
Check("删除尾后得到 1->2", list.ToString() == "1 -> 2");
Console.WriteLine("   ^ 删除尾节点后，内部尾指针必须更新，否则下一次 AddLast 会断链");

// 验证尾指针确实更新了
list.AddLast("4");
Check("删尾后再 AddLast，链接仍然正确", list.ToString() == "1 -> 2 -> 4");
Console.WriteLine();

// ---------- 边界 4：单元素链表 ----------
Console.WriteLine("边界 4：单元素链表");
list.Clear();
list.AddLast("only");
Check("单元素链表 Count = 1", list.Count == 1);
Check("删除唯一的元素成功", list.Remove("only"));
Check("删完后变成空表", list.Count == 0 && list.ToString() == "");

// 删空之后还能继续用
list.AddLast("new");
Check("删空后仍可继续 AddLast", list.ToString() == "new");
Console.WriteLine();

// ---------- 边界 5：删除中间元素与不存在的元素 ----------
Console.WriteLine("边界 5：中间删除与不存在的元素");
list.Clear();
foreach (var v in new[] { "a", "b", "c", "d", "e" }) list.AddLast(v);

Check("删除中间元素 c 成功", list.Remove("c"));
Check("删除中间后得到 a->b->d->e", list.ToString() == "a -> b -> d -> e");
Check("删除不存在的元素返回 false", list.Remove("zzz") == false);
Check("删除不存在后内容不变", list.ToString() == "a -> b -> d -> e");
Check("删除不存在的元素后 Count 不变", list.Count == 4);
Console.WriteLine();

// ---------- 汇总 ----------
Console.WriteLine(new string('=', 50));
Console.WriteLine($"测试汇总：通过 {passed} 项，失败 {failed} 项");
Console.WriteLine();

// ---------- 性能：AddLast 是否真是 O(1) ----------
Console.WriteLine("=== 附加实验：验证 AddLast 是 O(1) ===");
Console.WriteLine();

const int N = 1_000_000;
var big = new MyLinkedList<int>();
var sw = System.Diagnostics.Stopwatch.StartNew();
for (int i = 0; i < N; i++) big.AddLast(i);
sw.Stop();
Console.WriteLine($"  连续 AddLast {N:N0} 次: {sw.Elapsed.TotalMilliseconds:F1} ms，Count = {big.Count:N0}");
Console.WriteLine($"  平均每次 {(sw.Elapsed.TotalMilliseconds * 1000 / N):F3} 微秒");
Console.WriteLine();
Console.WriteLine($"  如果实现里没有维护尾指针（每次都从头找到尾），");
Console.WriteLine($"  这 {N:N0} 次 AddLast 就是 O(n^2)，会慢上几千倍。");
Console.WriteLine();

// ==================== 带哨兵节点的单向链表实现 ====================

/// <summary>链表节点</summary>
public class Node<T>
{
    public T Value;
    public Node<T>? Next;
    public Node(T value) => Value = value;
}

/// <summary>
/// 带「哨兵节点」的单向链表。
/// 哨兵节点（dummy）永远存在、不存数据，它的 Next 就是链表的第一个真实节点。
/// 好处：插入和删除都不需要特判「空表」和「头节点」两种情况。
/// </summary>
public class MyLinkedList<T>
{
    private readonly Node<T> _dummy = new(default!);   // 哨兵节点
    private Node<T> _tail;                              // 指向最后一个真实节点
    private int _count;

    public MyLinkedList()
    {
        _tail = _dummy;         // 空表时，尾指针指向哨兵
    }

    public int Count => _count;

    public void AddFirst(T value)
    {
        var node = new Node<T>(value) { Next = _dummy.Next };
        _dummy.Next = node;
        if (_tail == _dummy)
            _tail = node;       // 原来是空表，新节点同时也是尾节点
        _count++;
    }

    public void AddLast(T value)
    {
        var node = new Node<T>(value);
        _tail.Next = node;      // 空表时 _tail 就是 _dummy，等价于 _dummy.Next = node
        _tail = node;
        _count++;
    }

    public bool Remove(T value)
    {
        Node<T> prev = _dummy;  // 从哨兵开始找，头节点也因此有了「前驱」
        while (prev.Next != null && !EqualityComparer<T>.Default.Equals(prev.Next.Value, value))
            prev = prev.Next;

        if (prev.Next == null) return false;        // 没找到

        if (prev.Next == _tail)
            _tail = prev;       // 删掉的正好是尾节点，尾指针要回退
                            // 忘了这一行，下次 AddLast 就会接到一个已经脱离链表的节点上
        prev.Next = prev.Next.Next;
        _count--;
        return true;
    }

    public bool Contains(T value)
    {
        for (Node<T>? p = _dummy.Next; p != null; p = p.Next)
            if (EqualityComparer<T>.Default.Equals(p.Value, value)) return true;
        return false;
    }

    public void Clear()
    {
        _dummy.Next = null;
        _tail = _dummy;
        _count = 0;
    }

    public override string ToString()
    {
        var parts = new List<string>();
        for (Node<T>? p = _dummy.Next; p != null; p = p.Next)
            parts.Add(p.Value?.ToString() ?? "null");
        return string.Join(" -> ", parts);
    }
}
