using System.Diagnostics;

// ==================== 普通 BST（对照组）====================

/// <summary>
/// 普通 BST 的插入 —— 注意这里是【迭代版】。
/// 递归版在退化的树上会栈溢出：插入 10 万个递增元素时递归深度就是 10 万层，
/// 远超 2.2 节实测的 1.6 万层上限。（这正是「退化」的又一个后果。）
/// </summary>
static BstNode BstInsertIterative(BstNode? root, int value)
{
    var node = new BstNode(value);
    if (root == null) return node;

    var cur = root;
    while (true)
    {
        if (value < cur.Value)
        {
            if (cur.Left == null) { cur.Left = node; break; }
            cur = cur.Left;
        }
        else if (value > cur.Value)
        {
            if (cur.Right == null) { cur.Right = node; break; }
            cur = cur.Right;
        }
        else break;                     // 重复值，忽略
    }
    return root;
}

/// <summary>树高（迭代 BFS 版）—— 递归版在退化的树上同样会栈溢出。</summary>
static int BstHeight(BstNode? root)
{
    if (root == null) return -1;

    int height = -1;
    var queue = new Queue<BstNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
        int size = queue.Count;
        height++;
        for (int i = 0; i < size; i++)
        {
            var n = queue.Dequeue();
            if (n.Left != null) queue.Enqueue(n.Left);
            if (n.Right != null) queue.Enqueue(n.Right);
        }
    }
    return height;
}

// ==================== AVL 树 ====================
//
// AVL 在普通 BST 的基础上，给每个节点记录「高度」，
// 并在插入/删除后检查「平衡因子」，一旦超标就通过【旋转】调整。

static int H(AvlNode? n) => n?.Height ?? 0;

static void UpdateHeight(AvlNode n)
    => n.Height = 1 + Math.Max(H(n.Left), H(n.Right));

/// <summary>平衡因子 = 左子树高度 - 右子树高度。AVL 要求它在 -1 到 1 之间。</summary>
static int BalanceFactor(AvlNode n) => H(n.Left) - H(n.Right);

/// <summary>右旋：把左孩子提上来当根。</summary>
/// <remarks>
///        y                x
///       / \              / \
///      x   T3    -->    T1  y
///     / \                  / \
///    T1  T2               T2  T3
/// </remarks>
static AvlNode RotateRight(AvlNode y)
{
    var x = y.Left!;
    var t2 = x.Right;

    x.Right = y;          // y 变成 x 的右孩子
    y.Left = t2;          // T2 挂到 y 的左边（保持 BST 性质：T2 里的值都在 x 和 y 之间）

    UpdateHeight(y);      // 先更新下面的
    UpdateHeight(x);
    return x;             // 返回新的子树根
}

/// <summary>左旋：把右孩子提上来当根。</summary>
static AvlNode RotateLeft(AvlNode x)
{
    var y = x.Right!;
    var t2 = y.Left;

    y.Left = x;
    x.Right = t2;

    UpdateHeight(x);
    UpdateHeight(y);
    return y;
}

static AvlNode AvlInsert(AvlNode? node, int value, ref int rotationCount)
{
    // ---- 第 1 步：普通的 BST 插入 ----
    if (node == null) return new AvlNode(value);
    if (value < node.Value) node.Left = AvlInsert(node.Left, value, ref rotationCount);
    else if (value > node.Value) node.Right = AvlInsert(node.Right, value, ref rotationCount);
    else return node;

    // ---- 第 2 步：更新高度 ----
    UpdateHeight(node);

    // ---- 第 3 步：检查平衡因子，必要时旋转 ----
    int bf = BalanceFactor(node);

    // 情况 LL：左边太高，且新节点插在左孩子的左边 -> 一次右旋
    if (bf > 1 && value < node.Left!.Value)
    {
        rotationCount++;
        return RotateRight(node);
    }

    // 情况 RR：右边太高，且新节点插在右孩子的右边 -> 一次左旋
    if (bf < -1 && value > node.Right!.Value)
    {
        rotationCount++;
        return RotateLeft(node);
    }

    // 情况 LR：左边太高，但新节点插在左孩子的右边 -> 先左旋再右旋
    if (bf > 1 && value > node.Left!.Value)
    {
        rotationCount += 2;
        node.Left = RotateLeft(node.Left);
        return RotateRight(node);
    }

    // 情况 RL：右边太高，但新节点插在右孩子的左边 -> 先右旋再左旋
    if (bf < -1 && value < node.Right!.Value)
    {
        rotationCount += 2;
        node.Right = RotateRight(node.Right);
        return RotateLeft(node);
    }

    return node;      // 平衡的，不用调整
}

static int AvlHeight(AvlNode? n) => n?.Height ?? 0;

static bool AvlIsValid(AvlNode? n)
{
    if (n == null) return true;
    int bf = BalanceFactor(n);
    if (Math.Abs(bf) > 1) return false;
    return AvlIsValid(n.Left) && AvlIsValid(n.Right);
}

// ==================== 实验一：旋转是怎么回事 ====================

Console.WriteLine("=== 实验一：旋转 —— 不破坏 BST 性质地改变形状 ===");
Console.WriteLine();

Console.WriteLine("  场景：插入 10、20、30（递增，普通 BST 会退化成链）");
Console.WriteLine();

// 先看普通 BST 的结果
BstNode? plain = null;
foreach (int v in new[] { 10, 20, 30 }) plain = BstInsertIterative(plain, v);
Console.WriteLine($"    普通 BST:  {plain!.Value}");
Console.WriteLine($"                └─ {plain.Right!.Value}");
Console.WriteLine($"                   └─ {plain.Right.Right!.Value}     高度 = {BstHeight(plain)}");
Console.WriteLine();

// AVL：插入 30 时会触发左旋
int rots = 0;
AvlNode? avl = null;
foreach (int v in new[] { 10, 20, 30 }) avl = AvlInsert(avl, v, ref rots);
Console.WriteLine($"    AVL 树:    {avl!.Value}");
Console.WriteLine($"              /  \\");
Console.WriteLine($"            {avl.Left!.Value}    {avl.Right!.Value}              高度 = {AvlHeight(avl)}（触发了 {rots} 次旋转）");
Console.WriteLine();

Console.WriteLine("  左旋前后的形状变化：");
Console.WriteLine("       10                    20");
Console.WriteLine("        \\                   /  \\");
Console.WriteLine("         20       -->      10    30");
Console.WriteLine("           \\");
Console.WriteLine("            30");
Console.WriteLine();
Console.WriteLine("  注意：20 成了新的根，10 变成它的左孩子，30 还是右孩子。");
Console.WriteLine("  中序遍历仍然是 10 20 30 —— BST 性质没有破坏！");
Console.WriteLine();

// ==================== 实验二：四种旋转情况 ====================

Console.WriteLine("=== 实验二：四种不平衡情况 ===");
Console.WriteLine();
Console.WriteLine($"  {"情况",-8} | {"插入模式",-22} | {"调整方式",-24} | 触发场景");
Console.WriteLine("  " + new string('-', 78));
Console.WriteLine($"  {"LL",-8} | {"左孩子的左边",-22} | {"一次右旋",-24} | 递减序列插入");
Console.WriteLine($"  {"RR",-8} | {"右孩子的右边",-22} | {"一次左旋",-24} | 递增序列插入");
Console.WriteLine($"  {"LR",-8} | {"左孩子的右边",-22} | {"先左旋再右旋",-24} | 锯齿形插入");
Console.WriteLine($"  {"RL",-8} | {"右孩子的左边",-22} | {"先右旋再左旋",-24} | 锯齿形插入");
Console.WriteLine();

// 验证 LR 情况
var lrTest = new List<int> { 30, 10, 20 };
int lrRots = 0;
AvlNode? lrTree = null;
foreach (int v in lrTest) lrTree = AvlInsert(lrTree, v, ref lrRots);
Console.WriteLine($"  验证 LR：插入 {string.Join(", ", lrTest)}（锯齿形）");
Console.WriteLine($"    结果根节点 = {lrTree!.Value}（中间的 20 被提上来了）");
Console.WriteLine($"    触发了 {lrRots} 次旋转（因为是 LR，要转两次）");
Console.WriteLine($"    树仍然平衡：{AvlIsValid(lrTree)}");
Console.WriteLine();

// ==================== 实验三：AVL 真的能防退化吗 ====================

const int N = 100_000;
Console.WriteLine($"=== 实验三：用「递增数据」插入 {N:N0} 个元素 ===");
Console.WriteLine();

var sw = Stopwatch.StartNew();
BstNode? plainTree = null;
for (int i = 1; i <= N; i++) plainTree = BstInsertIterative(plainTree, i);
sw.Stop();
double plainMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
AvlNode? avlTree = null;
int totalRots = 0;
for (int i = 1; i <= N; i++) avlTree = AvlInsert(avlTree, i, ref totalRots);
sw.Stop();
double avlMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  {"结构",-12} | {"树高",12} | {"插入耗时",12} | 说明");
Console.WriteLine("  " + new string('-', 66));
Console.WriteLine($"  {"普通 BST",-12} | {BstHeight(plainTree),12:N0} | {plainMs,9:F1} ms | 完全退化成链");
Console.WriteLine($"  {"AVL 树",-12} | {AvlHeight(avlTree),12:N0} | {avlMs,9:F1} ms | 高度 = {AvlHeight(avlTree)}，触发 {totalRots} 次旋转");
Console.WriteLine();
Console.WriteLine($"  理论最优高度 log2({N:N0}) ≈ {Math.Log2(N):F1}");
Console.WriteLine();
Console.WriteLine($"  AVL 的高度 {AvlHeight(avlTree)} 接近理论值，而普通 BST 是 {BstHeight(plainTree):N0}。");
Console.WriteLine($"  AVL 是合法平衡树：{AvlIsValid(avlTree)}");
Console.WriteLine();

// ==================== 实验四：AVL 的代价 ====================

Console.WriteLine("=== 实验四：平衡的代价 vs 退化的代价 ===");
Console.WriteLine();
Console.WriteLine($"  【场景 A】递增输入（普通 BST 会退化）—— 插入 {N:N0} 个元素：");
Console.WriteLine($"    普通 BST: {plainMs,9:F1} ms   每次插入都要走到链的末尾，总共 O(n^2)");
Console.WriteLine($"    AVL 树  : {avlMs,9:F1} ms   每次插入 O(log n)");
Console.WriteLine($"    -> 这种情况下 AVL 反而快 {plainMs / avlMs:F0} 倍！");
Console.WriteLine();
Console.WriteLine("  所以「平衡的代价」在有序输入下根本看不出来 —— 因为普通 BST 自己先崩了。");
Console.WriteLine();

// 【场景 B】随机输入：普通 BST 也是平衡的，这时才能看出 AVL 的维护成本
var rng2 = new Random(42);
var shuffled = Enumerable.Range(1, N).ToArray();
for (int i = shuffled.Length - 1; i > 0; i--)
{
    int j = rng2.Next(i + 1);
    (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
}

sw.Restart();
BstNode? randomBst = null;
foreach (int v in shuffled) randomBst = BstInsertIterative(randomBst, v);
sw.Stop();
double randomBstMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
AvlNode? randomAvl = null;
int randomRots = 0;
foreach (int v in shuffled) randomAvl = AvlInsert(randomAvl, v, ref randomRots);
sw.Stop();
double randomAvlMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  【场景 B】随机输入 —— 插入 {N:N0} 个元素：");
Console.WriteLine($"    普通 BST: {randomBstMs,9:F1} ms   高度 {BstHeight(randomBst):N0}（也是平衡的）");
Console.WriteLine($"    AVL 树  : {randomAvlMs,9:F1} ms   高度 {AvlHeight(randomAvl):N0}，触发 {randomRots:N0} 次旋转");
Console.WriteLine($"    -> 这种情况下 AVL 慢 {randomAvlMs / randomBstMs:F2} 倍");
Console.WriteLine();
Console.WriteLine("  【结论】「平衡的代价」只在【数据本来就不会让 BST 退化】时才看得出来。");
Console.WriteLine("  而一旦数据有序（现实中最常见的情况），普通 BST 的代价会远超 AVL 的维护成本。");
Console.WriteLine();

// ==================== 实验五：.NET 提供了什么 ====================

Console.WriteLine("=== 实验五：.NET 内置的有序结构 ===");
Console.WriteLine();

var sortedDict = new SortedDictionary<int, string>();
sw.Restart();
for (int i = 1; i <= N; i++) sortedDict[i] = $"值{i}";
sw.Stop();
double sortedDictMs = sw.Elapsed.TotalMilliseconds;

// 查找 1000 个元素
var targets = Enumerable.Range(1, 1000).Select(i => i * (N / 1000)).ToArray();
sw.Restart();
long found = 0;
foreach (int t in targets) if (sortedDict.ContainsKey(t)) found++;
sw.Stop();
double lookupMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long avlFound = 0;
foreach (int t in targets)
{
    var cur = avlTree;
    while (cur != null) { if (cur.Value == t) { avlFound++; break; } cur = t < cur.Value ? cur.Left : cur.Right; }
}
sw.Stop();
double avlLookupMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  插入 {N:N0} 个递增元素：");
Console.WriteLine($"    SortedDictionary（.NET 内置）: {sortedDictMs,8:F1} ms");
Console.WriteLine($"    我们手写的 AVL            : {avlMs,8:F1} ms");
Console.WriteLine();
Console.WriteLine($"  查找 1000 个元素：");
Console.WriteLine($"    SortedDictionary: {lookupMs,8:F3} ms（命中 {found}）");
Console.WriteLine($"    手写 AVL        : {avlLookupMs,8:F3} ms（命中 {avlFound}）");
Console.WriteLine();
Console.WriteLine("  有意思的是：我们手写的 AVL 反而【更快】。");
Console.WriteLine("  原因是它只存 int、只支持插入和查找，没有任何通用性开销；");
Console.WriteLine("  而 SortedDictionary 要处理泛型、比较器委托、键值对、删除、迭代器……");
Console.WriteLine();
Console.WriteLine("  但【工程上仍然应该用 SortedDictionary】，因为：");
Console.WriteLine("    1. 它经过充分测试，处理了各种边界情况（重复键、删除、并发枚举……）");
Console.WriteLine("    2. 它支持任意可比较的键类型，我们的 AVL 只支持 int");
Console.WriteLine("    3. 它提供了完整的 API（Remove、枚举器、索引器……）");
Console.WriteLine("    4. 【我们的 AVL 只实现了插入，没有实现删除】—— 而删除才是自平衡树最难的部分");
Console.WriteLine();

// ==================== 实验六：AVL vs 红黑树 ====================

Console.WriteLine("=== 实验六：AVL 与红黑树的取舍 ===");
Console.WriteLine();
Console.WriteLine($"  {"维度",-18} | {"AVL 树",-26} | {"红黑树",-26}");
Console.WriteLine("  " + new string('-', 76));
Console.WriteLine($"  {"平衡程度",-18} | {"严格（高度差 <= 1）",-26} | {"近似（最长 <= 2 倍最短）",-26}");
Console.WriteLine($"  {"树高上界",-18} | {"1.44 log2(n)",-26} | {"2 log2(n)",-26}");
Console.WriteLine($"  {"查找速度",-18} | {"更快（树更矮）",-26} | {"稍慢",-26}");
Console.WriteLine($"  {"插入/删除速度",-18} | {"更慢（旋转更频繁）",-26} | {"更快",-26}");
Console.WriteLine($"  {"适用场景",-18} | {"读多写少",-26} | {"读写均衡",-26}");
Console.WriteLine($"  {"典型实现",-18} | {"较少见",-26} | {"C# SortedDictionary",-26}");
Console.WriteLine();
Console.WriteLine("  C# 的 SortedDictionary / SortedSet / SortedList 内部都是【红黑树】。");
Console.WriteLine("  为什么选红黑树而不是 AVL？—— 因为工程上「读写均衡」更常见，");
Console.WriteLine("  而红黑树在插入删除时需要的旋转更少（统计上约少 40%）。");
Console.WriteLine();

// ==================== 节点定义 ====================

public class BstNode
{
    public int Value;
    public BstNode? Left;
    public BstNode? Right;
    public BstNode(int value) => Value = value;
}

public class AvlNode
{
    public int Value;
    public AvlNode? Left;
    public AvlNode? Right;
    public int Height = 1;          // 叶子节点高度为 1（这里用「节点数」口径）

    public AvlNode(int value) => Value = value;
}
