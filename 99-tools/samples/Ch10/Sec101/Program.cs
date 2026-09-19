// ==================== BST 的定义 ====================
//
// 二叉搜索树的性质：
//   对任意节点 node：
//     1. node 左子树里【所有】节点的值 < node.Value
//     2. node 右子树里【所有】节点的值 > node.Value
//
// 注意是「所有」，不是「直接孩子」—— 这个区别在下面会看到有多重要。

static TreeNode? Insert(TreeNode? node, int value)
{
    if (node == null) return new TreeNode(value);
    if (value < node.Value) node.Left = Insert(node.Left, value);
    else if (value > node.Value) node.Right = Insert(node.Right, value);
    // value == node.Value：重复值，忽略（也可以选择计数）
    return node;
}

static bool Contains(TreeNode? node, int value)
{
    while (node != null)
    {
        if (value == node.Value) return true;
        node = value < node.Value ? node.Left : node.Right;
    }
    return false;
}

static void InOrder(TreeNode? node, List<int> result)
{
    if (node == null) return;
    InOrder(node.Left, result);
    result.Add(node.Value);
    InOrder(node.Right, result);
}

// ==================== 验证 BST：错误写法 vs 正确写法 ====================

/// <summary>
/// 【错误】只检查「直接父子」的大小关系。
/// 这是最经典的错误 —— 它漏掉了「孙子辈」的约束。
/// </summary>
static bool IsValidBstWrong(TreeNode? node)
{
    if (node == null) return true;

    if (node.Left != null && node.Left.Value >= node.Value) return false;
    if (node.Right != null && node.Right.Value <= node.Value) return false;

    return IsValidBstWrong(node.Left) && IsValidBstWrong(node.Right);
}

/// <summary>
/// 【正确】带着「上下界」递归。
/// 每个节点不仅要和父亲比较，还要满足「祖先传下来的范围」。
/// </summary>
static bool IsValidBst(TreeNode? node, long min = long.MinValue, long max = long.MaxValue)
{
    if (node == null) return true;
    if (node.Value <= min || node.Value >= max) return false;      // 超出祖先划定的范围

    return IsValidBst(node.Left, min, node.Value)                  // 左子树：上界收紧为自己
        && IsValidBst(node.Right, node.Value, max);                // 右子树：下界收紧为自己
}

// ==================== 有序性带来的能力 ====================

/// <summary>范围查询：找出 [lo, hi] 之间的所有值。</summary>
static void RangeQuery(TreeNode? node, int lo, int hi, List<int> result)
{
    if (node == null) return;

    // 关键：利用有序性「剪枝」——不符合范围的分支直接跳过
    if (node.Value > lo) RangeQuery(node.Left, lo, hi, result);

    if (node.Value >= lo && node.Value <= hi) result.Add(node.Value);

    if (node.Value < hi) RangeQuery(node.Right, lo, hi, result);
}

/// <summary>查找第 k 小的元素（k 从 1 开始）。</summary>
static int? KthSmallest(TreeNode? node, int k, ref int count)
{
    if (node == null) return null;

    var left = KthSmallest(node.Left, k, ref count);
    if (left != null) return left;

    count++;
    if (count == k) return node.Value;

    return KthSmallest(node.Right, k, ref count);
}

/// <summary>查找最小值 / 最大值 —— 一路向左 / 向右。</summary>
static int MinValue(TreeNode node)
{
    while (node.Left != null) node = node.Left;
    return node.Value;
}

static int MaxValue(TreeNode node)
{
    while (node.Right != null) node = node.Right;
    return node.Value;
}

// ==================== 构造测试数据 ====================

// 一棵正常的 BST
//                 20
//               /    \
//             10      30
//            /  \    /  \
//           5   15  25   35

TreeNode? bst = null;
foreach (int v in new[] { 20, 10, 30, 5, 15, 25, 35 })
    bst = Insert(bst, v);

Console.WriteLine("=== 实验一：中序遍历得到有序序列 ===");
Console.WriteLine();
Console.WriteLine("  树的结构：");
Console.WriteLine("                 20");
Console.WriteLine("               /    \\");
Console.WriteLine("             10      30");
Console.WriteLine("            /  \\    /  \\");
Console.WriteLine("           5   15  25   35");
Console.WriteLine();

var sorted = new List<int>();
InOrder(bst, sorted);
Console.WriteLine($"  中序遍历: {string.Join(" ", sorted)}");

bool isSorted = true;
for (int i = 1; i < sorted.Count; i++)
    if (sorted[i] < sorted[i - 1]) isSorted = false;

Console.WriteLine($"  是否从小到大？{isSorted}");
Console.WriteLine();
Console.WriteLine("  这就是 BST 最核心的性质：「中序有序」（9.2 节）。");
Console.WriteLine("  它带来两个重要能力：");
Console.WriteLine("    1. 想按顺序处理所有元素 -> 中序遍历即可，不需要额外排序");
Console.WriteLine("    2. 查找时可以「剪枝」—— 根据大小关系决定往左还是往右");
Console.WriteLine();

Console.WriteLine("=== 实验二：验证 BST —— 一个经典错误 ===");
Console.WriteLine();

// 构造一棵「错误写法会误判」的树：
//       10
//      /  \
//     5    15
//         /
//        6     <- 6 < 10，违反了「右子树所有值都 > 10」
TreeNode? badBst = new TreeNode(10)
{
    Left = new TreeNode(5),
    Right = new TreeNode(15) { Left = new TreeNode(6) },
};

Console.WriteLine("  这棵树（注意 6 的位置）：");
Console.WriteLine("         10");
Console.WriteLine("        /  \\");
Console.WriteLine("       5    15");
Console.WriteLine("           /");
Console.WriteLine("          6      <- 6 在 10 的右子树里，但 6 < 10");
Console.WriteLine();

Console.WriteLine($"  错误写法（只查直接父子）: {IsValidBstWrong(badBst)}   <- 误判为合法！");
Console.WriteLine($"  正确写法（带上下界）    : {IsValidBst(badBst)}   <- 正确识别");
Console.WriteLine();

Console.WriteLine("  为什么错误写法的眼睛「瞎了」？");
Console.WriteLine("    它检查 15 的时候，只看了「15 的左孩子 6 是否小于 15」——是的，6 < 15，通过；");
Console.WriteLine("    但它没有检查「6 是否大于 10」——而这个约束是【祖先 10】留下来的。");
Console.WriteLine();
Console.WriteLine("  正确写法把范围一路往下传：");
Console.WriteLine("    检查 10 时：范围是 (-∞, +∞)，10 在范围内 ✓，给左子树传 (-∞, 10)，右子树传 (10, +∞)");
Console.WriteLine("    检查 15 时：范围是 (10, +∞)，15 在范围内 ✓，给左子树传 (10, 15)");
Console.WriteLine("    检查 6  时：范围是 (10, 15)，6 【不在】范围内 ✗ -> 返回 false");
Console.WriteLine();

Console.WriteLine("=== 实验三：有序性带来的能力 ===");
Console.WriteLine();

var range = new List<int>();
RangeQuery(bst, 12, 28, range);
Console.WriteLine($"  1. 范围查询 [12, 28]: {string.Join(" ", range)}");
Console.WriteLine("     注意：不需要遍历整棵树 —— 不符合范围的分支会被直接跳过（剪枝）。");
Console.WriteLine();

int count = 0;
int? kth3 = KthSmallest(bst, 3, ref count);
count = 0;
int? kth5 = KthSmallest(bst, 5, ref count);
Console.WriteLine($"  2. 第 3 小的元素: {kth3}");
Console.WriteLine($"     第 5 小的元素: {kth5}");
Console.WriteLine("     中序遍历过程中数到第 k 个就返回。");
Console.WriteLine();

Console.WriteLine($"  3. 最小值: {MinValue(bst!)}（一路向左）");
Console.WriteLine($"     最大值: {MaxValue(bst!)}（一路向右）");
Console.WriteLine();

Console.WriteLine("=== 实验四：BST 相对哈希表的独特价值 ===");
Console.WriteLine();
Console.WriteLine($"  {"能力",-24} | {"BST",-16} | {"哈希表",-16}");
Console.WriteLine("  " + new string('-', 62));
Console.WriteLine($"  {"查找某个值",-24} | {"O(log n)",-16} | {"平均 O(1)",-16}");
Console.WriteLine($"  {"按范围查询",-24} | {"O(log n + k)",-16} | {"✗ 做不到",-16}");
Console.WriteLine($"  {"找最小/最大",-24} | {"O(log n)",-16} | {"✗ 做不到",-16}");
Console.WriteLine($"  {"找第 k 小",-24} | {"O(log n)（带子树计数）",-16} | {"✗ 做不到",-16}");
Console.WriteLine($"  {"按顺序遍历",-24} | {"O(n)",-16} | {"✗ 顺序不确定",-16}");
Console.WriteLine();
Console.WriteLine("  哈希表查找更快（O(1)），但它【完全无序】——");
Console.WriteLine("  6.4 节实测过：Dictionary 的遍历顺序完全不可依赖。");
Console.WriteLine();
Console.WriteLine("  BST 的价值不在于「查找更快」，而在于「保持有序的同时还能快速增删」。");
Console.WriteLine();

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
