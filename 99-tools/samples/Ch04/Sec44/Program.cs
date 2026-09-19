using System.Diagnostics;

// ==================== 快慢指针的四个经典应用 ====================

// ---------- 应用 1：判断链表有没有环 ----------
static bool HasCycle(Node head)
{
    Node? slow = head, fast = head;
    while (fast != null && fast.Next != null)
    {
        slow = slow!.Next;              // 慢指针每次走 1 步
        fast = fast.Next.Next;          // 快指针每次走 2 步
        if (ReferenceEquals(slow, fast)) return true;   // 追上了，说明有环
    }
    return false;
}

// 对照组：用哈希表记录访问过的节点，需要 O(n) 额外空间
static bool HasCycleWithSet(Node head)
{
    var seen = new HashSet<Node>(ReferenceEqualityComparer.Instance);
    for (Node? p = head; p != null; p = p.Next)
        if (!seen.Add(p)) return true;
    return false;
}

// ---------- 应用 2：找环的入口 ----------
static Node? FindCycleStart(Node head)
{
    Node? slow = head, fast = head;

    // 第一阶段：先让两个指针相遇
    while (fast != null && fast.Next != null)
    {
        slow = slow!.Next;
        fast = fast.Next.Next;
        if (ReferenceEquals(slow, fast)) break;
    }

    if (fast == null || fast.Next == null) return null;   // 没有环

    // 第二阶段：一个从头出发，一个从相遇点出发，同速前进 —— 相遇处就是环的入口
    Node? p1 = head, p2 = slow;
    while (!ReferenceEquals(p1, p2))
    {
        p1 = p1!.Next;
        p2 = p2!.Next;
    }
    return p1;
}

// ---------- 应用 3：找中点 ----------
static Node? FindMiddle(Node head)
{
    Node? slow = head, fast = head;
    while (fast?.Next != null)
    {
        slow = slow!.Next;
        fast = fast.Next.Next;
    }
    return slow;                        // 快指针走到底时，慢指针正好在中点
}

// ---------- 应用 4：找倒数第 k 个节点 ----------
static Node? FindKthFromEnd(Node head, int k)
{
    if (k <= 0) return null;

    Node? fast = head;
    for (int i = 0; i < k; i++)         // 先让快指针先走 k 步
    {
        if (fast == null) return null;  // k 比链表还长
        fast = fast.Next;
    }

    Node? slow = head;
    while (fast != null)                // 然后一起走，快指针到尾时慢指针正好在倒数第 k 个
    {
        slow = slow!.Next;
        fast = fast.Next;
    }
    return slow;
}

// ---------- 工具：构造 1..n 的链表，返回头节点 ----------
static Node BuildList(int n)
{
    Node head = new(1);
    Node cur = head;
    for (int i = 2; i <= n; i++)
    {
        cur.Next = new Node(i);
        cur = cur.Next;
    }
    return head;
}

// ==================== 主流程 ====================

Console.WriteLine("=== 应用 1 & 2：判环与找环入口 ===");
Console.WriteLine();

// 构造 1 -> 2 -> 3 -> 4 -> 5 -> 6，然后让 6 指回 3
Node head6 = new(1);
{
    Node cur = head6;
    for (int i = 2; i <= 6; i++) { cur.Next = new Node(i); cur = cur.Next; }
    Node entry = head6.Next!.Next!;     // 节点 3
    cur.Next = entry;                   // 尾节点指回 3，形成环
}

Console.WriteLine("  链表: 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> (回到 3)");
Console.WriteLine($"  HasCycle 判定    = {HasCycle(head6)}");
Node? entryFound = FindCycleStart(head6);
Console.WriteLine($"  环的入口节点值   = {entryFound?.Value}   (预期 3)");
Console.WriteLine($"  入口节点引用正确 = {ReferenceEquals(entryFound, head6.Next!.Next)}");
Console.WriteLine();

// 无环链表做对照
Node acyclic = new(1);
{
    Node cur = acyclic;
    for (int i = 2; i <= 6; i++) { cur.Next = new Node(i); cur = cur.Next; }
}
Console.WriteLine("  对照：无环链表 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> null");
Console.WriteLine($"  HasCycle 判定    = {HasCycle(acyclic)}");
Console.WriteLine($"  环的入口         = {FindCycleStart(acyclic)?.Value.ToString() ?? "null（没有环）"}");
Console.WriteLine();

Console.WriteLine("=== 应用 3：找中点 ===");
Console.WriteLine();

foreach (int n in new[] { 1, 2, 3, 4, 5, 6 })
{
    Node h = new(1);
    Node c = h;
    for (int i = 2; i <= n; i++) { c.Next = new Node(i); c = c.Next; }

    Node? mid = FindMiddle(h);
    Console.Write($"  {n} 个节点 [1..{n}] 的中点 = {mid!.Value}");
    Console.WriteLine(n % 2 == 1 ? "   (奇数个：正中间那个)" : "   (偶数个：靠后的那个)");
}
Console.WriteLine();

Console.WriteLine("=== 应用 4：找倒数第 k 个节点 ===");
Console.WriteLine();

Node h10 = new(1);
{
    Node c = h10;
    for (int i = 2; i <= 10; i++) { c.Next = new Node(i); c = c.Next; }
}
Console.WriteLine("  链表: 1 -> 2 -> ... -> 10");
foreach (int k in new[] { 1, 3, 5, 10 })
{
    Node? r = FindKthFromEnd(h10, k);
    Console.WriteLine($"    倒数第 {k,2} 个 = {r?.Value}");
}
Console.WriteLine($"    倒数第 11 个 = {FindKthFromEnd(h10, 11)?.Value.ToString() ?? "null（k 超过链表长度）"}");
Console.WriteLine();

// ==================== 性能与空间对比 ====================

Console.WriteLine("=== 判环的两种做法：时间与空间 ===");
Console.WriteLine();

const int N = 1_000_000;
Node bigHead = BuildList(N);

var sw = Stopwatch.StartNew();

// 用「累计分配字节数」而不是 GC.GetTotalMemory：
// 后者测的是「当前占用」，而哈希表在方法返回后就被回收了，根本测不到。
long alloc0 = GC.GetAllocatedBytesForCurrentThread();
bool r1 = HasCycle(bigHead);
long alloc1 = GC.GetAllocatedBytesForCurrentThread();
sw.Stop();
double fastSlowMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
bool r2 = HasCycleWithSet(bigHead);
long alloc2 = GC.GetAllocatedBytesForCurrentThread();
sw.Stop();
double setMs = sw.Elapsed.TotalMilliseconds;

double fastSlowKb = (alloc1 - alloc0) / 1024.0;
double setKb = (alloc2 - alloc1) / 1024.0;

Console.WriteLine($"  链表长度 {N:N0}，无环（最坏情况：两种方法都必须走完全程）");
Console.WriteLine();
Console.WriteLine($"  快慢指针: 结果={r1,-5} | {fastSlowMs,8:F2} ms | 新分配内存 {fastSlowKb,9:F1} KB");
Console.WriteLine($"  哈希表  : 结果={r2,-5} | {setMs,8:F2} ms | 新分配内存 {setKb,9:F1} KB");
Console.WriteLine();
Console.WriteLine($"  快慢指针一点额外内存都没分配（{fastSlowKb:F1} KB），哈希表分配了 {setKb / 1024:F1} MB。");
Console.WriteLine();
Console.WriteLine("  两者都是 O(n) 时间，但快慢指针是 O(1) 额外空间，哈希表是 O(n)。");
Console.WriteLine("  在内存受限的场景（比如嵌入式、海量并发），这个差别是决定性的。");

// ==================== 节点定义 ====================

public class Node
{
    public int Value;
    public Node? Next;
    public Node(int value) => Value = value;
}
