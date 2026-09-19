using System.Diagnostics;

// ==================== 实验一：手动构造一条链表 ====================

Console.WriteLine("=== 实验一：链表长什么样 ===");
Console.WriteLine();

Node a = new(10);
Node b = new(20);
Node c = new(30);
a.Next = b;
b.Next = c;
// c.Next 保持 null，表示链表到此结束

Node head = a;                       // 只要记住「头」，就能找到整条链

Console.Write("  链表内容: ");
for (Node? p = head; p != null; p = p.Next)
    Console.Write($"{p.Value} -> ");
Console.WriteLine("null");
Console.WriteLine();

// 只持有 b 这个节点，能访问到它后面的全部内容吗？
Console.Write("  从中间节点 b 出发: ");
for (Node? p = b; p != null; p = p.Next)
    Console.Write($"{p.Value} -> ");
Console.WriteLine("null   <- 但访问不到前面的 10 了，这就是「单向」的含义");
Console.WriteLine();

// ==================== 实验二：内存占用对比 ====================

const int N = 1_000_000;

Console.WriteLine($"=== 实验二：存储 {N:N0} 个整数的内存占用 ===");
Console.WriteLine();

long before = GC.GetTotalMemory(true);
int[] arr = new int[N];
for (int i = 0; i < N; i++) arr[i] = i;
long afterArr = GC.GetTotalMemory(true);

// 链表 A：顺序分配。节点一个接一个 new 出来，在堆上大概率挨在一起
Node listHead = new(0);
Node cur = listHead;
for (int i = 1; i < N; i++)
{
    cur.Next = new Node(i);
    cur = cur.Next;
}
long afterList = GC.GetTotalMemory(true);

double arrMb = (afterArr - before) / 1024.0 / 1024.0;
double listMb = (afterList - afterArr) / 1024.0 / 1024.0;

Console.WriteLine($"  int[]  : {arrMb,7:F1} MB   (每个元素 4 字节，紧密排列)");
Console.WriteLine($"  链表   : {listMb,7:F1} MB   (每个节点都要单独分配一个对象)");
Console.WriteLine($"  链表多占 {listMb / arrMb:F1} 倍内存");
Console.WriteLine();

// ==================== 实验三：遍历性能对比 ====================

Console.WriteLine("=== 实验三：遍历 100 万个元素 ===");
Console.WriteLine();

// 链表 B：分散分配。每建一个节点就夹一个无用对象，模拟真实运行时堆的碎片化
//（真实系统里，节点是陆续创建删除的，中间还夹着别的对象）
var junk = new List<byte[]>();
Node scatteredHead = new(0);
Node sc = scatteredHead;
for (int i = 1; i < N; i++)
{
    junk.Add(new byte[16]);              // 制造内存「噪音」
    sc.Next = new Node(i);
    sc = sc.Next;
}
GC.KeepAlive(junk);

// 预热：让 JIT 编译好
long warm = 0;
for (int i = 0; i < N; i++) warm += arr[i];
for (Node? p = listHead; p != null; p = p.Next) warm += p.Value;
for (Node? p = scatteredHead; p != null; p = p.Next) warm += p.Value;
GC.KeepAlive(warm);

var sw = Stopwatch.StartNew();
long sumArr = 0;
for (int i = 0; i < N; i++) sumArr += arr[i];
sw.Stop();
double arrMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long sumList = 0;
for (Node? p = listHead; p != null; p = p.Next) sumList += p.Value;
sw.Stop();
double listMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long sumScattered = 0;
for (Node? p = scatteredHead; p != null; p = p.Next) sumScattered += p.Value;
sw.Stop();
double scatteredMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  数组               : {arrMs,8:F2} ms   和 = {sumArr:N0}");
Console.WriteLine($"  链表（节点挨在一起）: {listMs,8:F2} ms   和 = {sumList:N0}   慢 {listMs / arrMs,5:F1} 倍");
Console.WriteLine($"  链表（节点分散）    : {scatteredMs,8:F2} ms   和 = {sumScattered:N0}   慢 {scatteredMs / arrMs,5:F1} 倍");
Console.WriteLine();
Console.WriteLine("  三者都是 O(n)，加法次数完全一样。差距全部来自内存布局：");
Console.WriteLine("    数组元素连续排列，CPU 一次读一整条缓存行（64 字节）就能拿到 16 个 int；");
Console.WriteLine("    链表每跳一个节点都要先读它的 Next 引用，而节点散落在堆上，容易缓存未命中。");
Console.WriteLine();
Console.WriteLine("  注意第二行和第三行的区别：同样是链表、同样的数据、同样的代码，");
Console.WriteLine("  只因为节点在内存里「挨着」还是「散着」，速度就能差好几倍。");
Console.WriteLine("  而这个分布往往不由你控制 —— 真实系统里的节点是陆续创建、删除的。");
Console.WriteLine();

// ==================== 实验四：头部插入对比 ====================

const int M = 100_000;

Console.WriteLine($"=== 实验四：在头部插入 {M:N0} 个元素 ===");
Console.WriteLine();

// 预热
{
    var wl = new List<int>();
    for (int i = 0; i < 20_000; i++) wl.Insert(0, i);
    Node wh = new(-1);
    for (int i = 0; i < M; i++) wh = new Node(i) { Next = wh };
}

sw.Restart();
var arrHead = new List<int>();
for (int i = 0; i < M; i++) arrHead.Insert(0, i);       // 每次都要把后面全部挪一格
sw.Stop();
double arrHeadMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
Node listHead2 = new(-1);
for (int i = 0; i < M; i++)
{
    Node newNode = new(i);
    newNode.Next = listHead2;                            // 改两个引用就完事
    listHead2 = newNode;
}
sw.Stop();
double listHeadMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  List<int>.Insert(0,..) : {arrHeadMs,9:F2} ms   每次 O(n)，总共 O(n^2)");
Console.WriteLine($"  链表头插               : {listHeadMs,9:F2} ms   每次 O(1)");
Console.WriteLine($"  链表快 {arrHeadMs / listHeadMs:F1} 倍");
Console.WriteLine();
Console.WriteLine("  这就是链表存在的理由：插入删除不用挪动任何已有元素，只改几个引用。");
Console.WriteLine();

// ==================== 节点定义 ====================

/// <summary>单向链表的一个节点：一份数据 + 一个指向下一个节点的引用</summary>
public class Node
{
    public int Value;
    public Node? Next;

    public Node(int value) => Value = value;
}
