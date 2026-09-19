using System.Diagnostics;

// ==================== 实验一：删除「已知节点」的代价 ====================
//
// 注意前提：你已经拿到了要删除的那个节点本身的引用，
// 不需要先查找它。这个区别很重要。

static void RemoveSingly(ref Node? head, Node target, ref long ops)
{
    if (head == target)
    {
        head = head!.Next;
        return;
    }

    Node? prev = head;
    while (prev != null && prev.Next != target)     // 必须从头找它的「前驱」
    {
        prev = prev.Next;
        ops++;
    }
    if (prev != null) prev.Next = target.Next;
}

static void RemoveDoubly(DNode node)
{
    // 因为节点自己记着前驱，两个引用赋值就完事了
    if (node.Prev != null) node.Prev.Next = node.Next;
    if (node.Next != null) node.Next.Prev = node.Prev;
}

const int N = 500_000;

Node? sHead = new Node(0);
Node sc = sHead;
for (int i = 1; i < N; i++)
{
    sc.Next = new Node(i);
    sc = sc.Next;
}
Node lastSingly = sc;                       // 最后一个节点

DNode dHead = new DNode(0);
DNode dc = dHead;
for (int i = 1; i < N; i++)
{
    dc.Next = new DNode(i);
    dc.Next.Prev = dc;
    dc = dc.Next;
}
DNode lastDoubly = dc;

var sw = Stopwatch.StartNew();

long ops = 0;
RemoveSingly(ref sHead, lastSingly, ref ops);
sw.Stop();
double singlyMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
RemoveDoubly(lastDoubly);
sw.Stop();
double doublyMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine("=== 实验一：删除「已知节点」（不用先查找）===");
Console.WriteLine($"链表中一共 {N:N0} 个节点，删除最后一个：");
Console.WriteLine();
Console.WriteLine($"  单向链表: 走了 {ops,10:N0} 步 | {singlyMs,8:F3} ms | 因为必须从头找它的前驱");
Console.WriteLine($"  双向链表: 走了 {0,10} 步 | {doublyMs,8:F3} ms | 节点自己记着前驱，O(1)");
Console.WriteLine($"  双向快 {singlyMs / doublyMs:F0} 倍");
Console.WriteLine();

// ==================== 实验二：循环链表 ====================

Console.WriteLine("=== 实验二：循环链表 ====================");
Console.WriteLine();

Node c1 = new(1), c2 = new(2), c3 = new(3);
c1.Next = c2;
c2.Next = c3;
c3.Next = c1;                               // 尾节点指回开头 —— 这就是「循环」

Console.Write("  正确遍历（do-while，判断条件是「回到起点」）: ");
Node p = c1;
do
{
    Console.Write($"{p.Value} -> ");
    p = p.Next!;
} while (p != c1);
Console.WriteLine("(回到起点，结束)");
Console.WriteLine();

Console.WriteLine("  常见错误：套用普通链表的写法 `for (p = c1; p != null; p = p.Next)`");
Console.WriteLine("    因为 c3.Next 是 c1 而不是 null，p 永远不会变成 null；");
Console.WriteLine("    结果就是无限循环，一直打印 1 -> 2 -> 3 -> 1 -> 2 -> 3 -> ...");
Console.WriteLine();
Console.WriteLine("  记忆点：判断循环链表结束的依据是「回到起点」，不是「变成 null」。");
Console.WriteLine();

// ==================== 实验三：C# 的 LinkedList<T> ====================

Console.WriteLine("=== 实验三：C# 内置的 LinkedList<T> ===");
Console.WriteLine();

var ll = new LinkedList<int>();
ll.AddLast(1);
ll.AddLast(2);
ll.AddLast(3);
Console.WriteLine($"  初始: {string.Join(" -> ", ll)}");

LinkedListNode<int>? found = ll.Find(2);     // Find 返回的是「节点」，不是下标
if (found != null)
{
    ll.AddBefore(found, 99);                 // 拿着节点引用，插入是 O(1)
    Console.WriteLine($"  在 2 前面插入 99: {string.Join(" -> ", ll)}");

    ll.Remove(found);                        // 同样是 O(1)
    Console.WriteLine($"  再删掉 2:        {string.Join(" -> ", ll)}");
}
Console.WriteLine();

Console.WriteLine("  LinkedList<T> 是【双向】链表，所以：");
Console.WriteLine("    优点：拿到节点引用后，插入/删除都是 O(1)；能在两端 O(1) 增删");
Console.WriteLine("    缺点：没有下标访问 —— 没有 ll[2] 这种写法，想找第 3 个必须走 3 步");
Console.WriteLine("          每个节点额外多存一个 Prev 引用，内存开销更大");
Console.WriteLine();

// 大规模实测「按下标访问」的代价
const int M = 500_000;
var arrRef = new List<int>(M);
var linked = new LinkedList<int>();
for (int i = 0; i < M; i++)
{
    arrRef.Add(i);
    linked.AddLast(i);
}

sw.Restart();
int v1 = arrRef[M - 1];                       // O(1)：直接算地址
sw.Stop();
double arrAccessMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
int v2 = linked.ElementAt(M - 1);             // O(n)：必须一步一步走过去
sw.Stop();
double linkedAccessMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  访问第 {M:N0} 个元素（下标 {M - 1:N0}）：");
Console.WriteLine($"    List<int>       : {arrAccessMs,8:F4} ms   O(1)，一次乘加算出地址");
Console.WriteLine($"    LinkedList<int> : {linkedAccessMs,8:F4} ms   O(n)，必须从头一步步走");
Console.WriteLine($"    链表慢 {linkedAccessMs / arrAccessMs:F0} 倍");
Console.WriteLine($"    （两者取到的值相同: {v1 == v2}）");
Console.WriteLine();

// ==================== 节点定义 ====================

public class Node
{
    public int Value;
    public Node? Next;
    public Node(int value) => Value = value;
}

public class DNode
{
    public int Value;
    public DNode? Prev;
    public DNode? Next;
    public DNode(int value) => Value = value;
}
