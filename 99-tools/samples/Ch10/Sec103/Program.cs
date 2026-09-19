using System.Diagnostics;

// ==================== 基础 BST ====================

static TreeNode? Insert(TreeNode? node, int value)
{
    if (node == null) return new TreeNode(value);
    if (value < node.Value) node.Left = Insert(node.Left, value);
    else if (value > node.Value) node.Right = Insert(node.Right, value);
    return node;
}

static bool Contains(TreeNode? node, int value, ref long comparisons)
{
    while (node != null)
    {
        comparisons++;
        if (value == node.Value) return true;
        node = value < node.Value ? node.Left : node.Right;
    }
    return false;
}

static int Height(TreeNode? node)
    => node == null ? -1 : 1 + Math.Max(Height(node.Left), Height(node.Right));

/// <summary>计算「从根往下走」的平均深度（用来衡量查找的平均代价）。</summary>
static (long TotalDepth, int Count) SumDepth(TreeNode? node, int depth = 0)
{
    if (node == null) return (0, 0);
    var (l, lc) = SumDepth(node.Left, depth + 1);
    var (r, rc) = SumDepth(node.Right, depth + 1);
    return (depth + l + r, 1 + lc + rc);
}

// ==================== 实验一：插入顺序决定树的形状 ====================

const int N = 10_000;
var rng = new Random(42);

Console.WriteLine($"=== 实验一：插入顺序如何决定树的形状（n = {N:N0}）===");
Console.WriteLine();

// 场景 A：随机顺序插入
var randomOrder = Enumerable.Range(1, N).ToArray();
for (int i = randomOrder.Length - 1; i > 0; i--)
{
    int j = rng.Next(i + 1);
    (randomOrder[i], randomOrder[j]) = (randomOrder[j], randomOrder[i]);
}

TreeNode? bstRandom = null;
foreach (int v in randomOrder) bstRandom = Insert(bstRandom, v);

// 场景 B：有序（递增）插入
TreeNode? bstSorted = null;
for (int i = 1; i <= N; i++) bstSorted = Insert(bstSorted, i);

// 场景 C：完全逆序插入
TreeNode? bstReversed = null;
for (int i = N; i >= 1; i--) bstReversed = Insert(bstReversed, i);

Console.WriteLine($"  {"插入顺序",-16} | {"树高",10} | {"理论 log2(n)",14} | {"平均深度",10} | 形状");
Console.WriteLine("  " + new string('-', 74));

double log2n = Math.Log2(N);

foreach (var (name, tree, shape) in new (string, TreeNode?, string)[]
         {
             ("随机顺序", bstRandom, "比较平衡"),
             ("递增 1,2,3...", bstSorted, "退化成右斜链"),
             ("递减 n,n-1...", bstReversed, "退化成左斜链"),
         })
{
    int h = Height(tree);
    var (totalDepth, count) = SumDepth(tree);
    double avgDepth = totalDepth / (double)count;

    Console.WriteLine($"  {name,-16} | {h,10:N0} | {log2n,14:F1} | {avgDepth,10:F1} | {shape}");
}
Console.WriteLine();
Console.WriteLine("  随机顺序的高度接近 log2(n)，而递增/递减顺序的高度就是 n-1 —— 完全退化成链。");
Console.WriteLine();

// ==================== 实验二：退化的代价 ====================

const int M = 20_000;
Console.WriteLine($"=== 实验二：查找代价的差距（n = {M:N0}）===");
Console.WriteLine();

TreeNode? rTree = null;
var rOrder = Enumerable.Range(1, M).ToArray();
for (int i = rOrder.Length - 1; i > 0; i--)
{
    int j = rng.Next(i + 1);
    (rOrder[i], rOrder[j]) = (rOrder[j], rOrder[i]);
}
foreach (int v in rOrder) rTree = Insert(rTree, v);

TreeNode? sTree = null;
for (int i = 1; i <= M; i++) sTree = Insert(sTree, i);

// 查找同样的一批值
var targets = new int[2000];
for (int i = 0; i < targets.Length; i++) targets[i] = rng.Next(1, M + 1);

long c1 = 0;
foreach (int t in targets) Contains(rTree, t, ref c1);

long c2 = 0;
foreach (int t in targets) Contains(sTree, t, ref c2);

Console.WriteLine($"  对 {targets.Length:N0} 个值做查找：");
Console.WriteLine($"    随机插入的 BST: 总共比较 {c1,12:N0} 次，平均 {c1 / (double)targets.Length,8:F1} 次/查找");
Console.WriteLine($"    有序插入的 BST: 总共比较 {c2,12:N0} 次，平均 {c2 / (double)targets.Length,8:F1} 次/查找");
Console.WriteLine($"    差了 {c2 / (double)c1:F0} 倍");
Console.WriteLine();

// ==================== 实验三：与其它结构对比 ====================

Console.WriteLine("=== 实验三：退化后的 BST 还不如…… ===");
Console.WriteLine();

const int K = 10_000;
var sortedArray = Enumerable.Range(1, K).ToArray();      // 有序数组
TreeNode? degradedBst = null;
for (int i = 1; i <= K; i++) degradedBst = Insert(degradedBst, i);

var sw = Stopwatch.StartNew();
long arrCmp = 0;
foreach (int t in Enumerable.Range(1, 2000).Select(_ => rng.Next(1, K + 1)))
{
    // 二分查找
    int lo = 0, hi = sortedArray.Length - 1;
    while (lo <= hi)
    {
        arrCmp++;
        int mid = (lo + hi) / 2;
        if (sortedArray[mid] == t) break;
        if (sortedArray[mid] < t) lo = mid + 1; else hi = mid - 1;
    }
}
sw.Stop();
double arrMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long bstCmp = 0;
foreach (int t in Enumerable.Range(1, 2000).Select(_ => rng.Next(1, K + 1)))
    Contains(degradedBst, t, ref bstCmp);
sw.Stop();
double bstMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  {K:N0} 个元素，做 2000 次查找：");
Console.WriteLine();
Console.WriteLine($"  {"结构",-28} | {"平均比较次数",14} | {"耗时",10}");
Console.WriteLine("  " + new string('-', 60));
Console.WriteLine($"  {"有序数组（二分查找）",-28} | {arrCmp / 2000.0,14:F1} | {arrMs,7:F2} ms");
Console.WriteLine($"  {"退化 BST（有序插入）",-28} | {bstCmp / 2000.0,14:F1} | {bstMs,7:F2} ms");
Console.WriteLine();
Console.WriteLine($"  退化后的 BST 比有序数组慢 {bstMs / arrMs:F1} 倍。");
Console.WriteLine();
Console.WriteLine("  这说明：BST 的「查找快」完全依赖于「树是平衡的」。");
Console.WriteLine("  一旦退化，它不仅比平衡 BST 慢，甚至比一个简单的有序数组 + 二分还慢。");
Console.WriteLine();

// ==================== 实验四：为什么会退化成链 ====================

Console.WriteLine("=== 实验四：退化是怎么一步步发生的 ===");
Console.WriteLine();

TreeNode? demo = null;
Console.Write("  依次插入 1~7，看看树长什么样：");
foreach (int v in new[] { 1, 2, 3, 4, 5, 6, 7 })
{
    demo = Insert(demo, v);
}
Console.WriteLine();
Console.WriteLine();

void PrintShape(TreeNode? n, string indent = "", bool isLast = true, bool isRoot = true)
{
    if (n == null) return;
    if (isRoot) Console.WriteLine($"      {n.Value}");
    else Console.WriteLine($"{indent}{(isLast ? "└─ " : "├─ ")}{n.Value}");
    string ci = isRoot ? "      " : indent + (isLast ? "   " : "│  ");
    if (n.Left != null) PrintShape(n.Left, ci, n.Right == null, false);
    if (n.Right != null) PrintShape(n.Right, ci, true, false);
}

PrintShape(demo);
Console.WriteLine();
Console.WriteLine("  每插入一个更大的数，它都只能往右走到底 —— 因为「所有已有的数都比它小」。");
Console.WriteLine("  于是树退化成了一条「右斜链」，高度 = n - 1 = 6。");
Console.WriteLine();
Console.WriteLine("  这就是 BST 的致命弱点：");
Console.WriteLine("    它完全由【插入顺序】决定形状，而它自己【无法感知】这一点。");
Console.WriteLine();

// ==================== 实验五：哪些真实数据的插入顺序是有序的 ====================

Console.WriteLine("=== 实验五：现实中，哪些数据的插入顺序是「天然有序」的？ ===");
Console.WriteLine();
Console.WriteLine("  1. 自增主键（数据库 ID）      -> 严格递增");
Console.WriteLine("  2. 时间戳（日志、订单）        -> 基本递增");
Console.WriteLine("  3. 从已排序的文件/表里导入数据  -> 严格有序");
Console.WriteLine("  4. 用户手动输入 1, 2, 3...    -> 有序");
Console.WriteLine();
Console.WriteLine("  【结论】「有序输入」不是小概率事件，而是【默认情况】。");
Console.WriteLine("  所以普通 BST 在生产环境里几乎注定会退化 ——");
Console.WriteLine("  这就是为什么必须有【自平衡】的树（下一节）。");
Console.WriteLine();

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
