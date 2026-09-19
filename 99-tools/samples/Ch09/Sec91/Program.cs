// ==================== 树的工具函数 ====================

/// <summary>把树画出来（横向缩进版，方便看清层级）。</summary>
static void PrintTree(TreeNode? node, string indent = "", bool isLast = true, bool isRoot = true)
{
    if (node == null) return;

    if (isRoot)
        Console.WriteLine($"  {node.Value}");
    else
        Console.WriteLine($"{indent}{(isLast ? "└─ " : "├─ ")}{node.Value}");

    string childIndent = isRoot ? "  " : indent + (isLast ? "   " : "│  ");

    bool hasLeft = node.Left != null;
    bool hasRight = node.Right != null;

    if (hasLeft) PrintTree(node.Left, childIndent, !hasRight, false);
    if (hasRight) PrintTree(node.Right, childIndent, true, false);
}

/// <summary>按层打印（横向展示每一层有哪些节点）。</summary>
static void PrintByLevel(TreeNode? root)
{
    if (root == null) { Console.WriteLine("  (空树)"); return; }

    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);
    int level = 0;

    while (queue.Count > 0)
    {
        int count = queue.Count;              // 当前层的节点数
        Console.Write($"    第 {level} 层: ");
        for (int i = 0; i < count; i++)
        {
            var node = queue.Dequeue();
            Console.Write($"{node.Value}  ");
            if (node.Left != null) queue.Enqueue(node.Left);
            if (node.Right != null) queue.Enqueue(node.Right);
        }
        Console.WriteLine();
        level++;
    }
}

/// <summary>节点总数。</summary>
static int CountNodes(TreeNode? node)
    => node == null ? 0 : 1 + CountNodes(node.Left) + CountNodes(node.Right);

/// <summary>叶子节点数（没有孩子的节点）。</summary>
static int CountLeaves(TreeNode? node)
{
    if (node == null) return 0;
    if (node.Left == null && node.Right == null) return 1;
    return CountLeaves(node.Left) + CountLeaves(node.Right);
}

/// <summary>树的高度（从根到最远叶子的边数）。空树高度记为 -1。</summary>
static int Height(TreeNode? node)
{
    if (node == null) return -1;
    return 1 + Math.Max(Height(node.Left), Height(node.Right));
}

/// <summary>深度：某个节点到根的距离。这里用「查找到目标值时的深度」来演示。</summary>
static int DepthOf(TreeNode? node, int target, int currentDepth = 0)
{
    if (node == null) return -1;
    if (node.Value == target) return currentDepth;
    int left = DepthOf(node.Left, target, currentDepth + 1);
    if (left != -1) return left;
    return DepthOf(node.Right, target, currentDepth + 1);
}

// ==================== 构造一棵树 ====================

//                 1
//               /   \
//              2     3
//             / \   / \
//            4   5 6   7
//           / \
//          8   9

var root = new TreeNode(1)
{
    Left = new TreeNode(2)
    {
        Left = new TreeNode(4) { Left = new TreeNode(8), Right = new TreeNode(9) },
        Right = new TreeNode(5),
    },
    Right = new TreeNode(3)
    {
        Left = new TreeNode(6),
        Right = new TreeNode(7),
    },
};

Console.WriteLine("=== 实验一：一棵二叉树长什么样 ===");
Console.WriteLine();
PrintTree(root);
Console.WriteLine();

Console.WriteLine("  按层看：");
PrintByLevel(root);
Console.WriteLine();

Console.WriteLine("  它对应的完全二叉树数组表示（下标 i 的孩子是 2i+1 和 2i+2）：");
Console.WriteLine("    [1, 2, 3, 4, 5, 6, 7, 8, 9]");
Console.WriteLine("     下标 0  1  2  3  4  5  6  7  8");
Console.WriteLine();

// ==================== 实验二：常用术语的度量 ====================

Console.WriteLine("=== 实验二：树的各种度量 ===");
Console.WriteLine();
Console.WriteLine($"  节点总数: {CountNodes(root)}");
Console.WriteLine($"  叶子节点: {CountLeaves(root)}（值是 8、9、5、6、7）");
Console.WriteLine($"  树的高度: {Height(root)}（根到最远叶子 8 的边数：1→2→4→8，共 3 条边）");
Console.WriteLine();

Console.WriteLine("  各个节点的深度（到根的距离）：");
foreach (int v in new[] { 1, 2, 3, 4, 6, 8, 9 })
{
    int d = DepthOf(root, v);
    Console.WriteLine($"    节点 {v}: 深度 = {d}");
}
Console.WriteLine();

Console.WriteLine("  注意「深度」和「高度」的区别：");
Console.WriteLine("    深度：从【根】往下数到该节点（根自己的深度是 0）");
Console.WriteLine("    高度：从该节点往下数到【最远叶子】（叶子的高度是 0）");
Console.WriteLine("    整棵树的高度 = 根的高度（本例是 3）");
Console.WriteLine();

// ==================== 实验三：满二叉树与完全二叉树 ====================

Console.WriteLine("=== 实验三：满/完全/完美二叉树的节点数 ===");
Console.WriteLine();
Console.WriteLine("  高度为 h 的【完美二叉树】（每一层都填满）有多少节点？");
Console.WriteLine();
Console.WriteLine($"  {"高度 h",10} | {"节点数 2^(h+1) - 1",22} | {"实测（构造一棵）",20}");
Console.WriteLine(new string('-', 60));

for (int h = 0; h <= 5; h++)
{
    var perfect = BuildPerfect(h);
    int expected = (1 << (h + 1)) - 1;
    Console.WriteLine($"{h,10} | {expected,22} | {CountNodes(perfect),20}   一致={CountNodes(perfect) == expected}");
}
Console.WriteLine();
Console.WriteLine("  关键关系：节点数 = 2^(h+1) - 1");
Console.WriteLine("  反过来：h = log2(节点数 + 1) - 1");
Console.WriteLine();
Console.WriteLine("  这就是为什么平衡的二叉树高度是 O(log n)：");
Console.WriteLine($"    n = 1,000,000 时，完美二叉树的高度只有 {(int)Math.Floor(Math.Log2(1000000 + 1)) - 0} 左右。");
Console.WriteLine("    而如果树退化成一条链，高度就是 n - 1 = 999,999。");
Console.WriteLine();

// 构造一棵退化的树（右斜链）
var skewed = new TreeNode(1);
var cur = skewed;
for (int i = 2; i <= 100; i++)
{
    cur.Right = new TreeNode(i);
    cur = cur.Right;
}
Console.WriteLine($"  对比：100 个节点的【退化链】高度 = {Height(skewed)}（而完美二叉树只要 6）");
Console.WriteLine();

// ==================== 实验四：三种特殊的二叉树 ====================

Console.WriteLine("=== 实验四：三种二叉树形态的对比 ===");
Console.WriteLine();

var perfectTree = BuildPerfect(3);
Console.WriteLine($"  完美二叉树（高度 3，{CountNodes(perfectTree)} 个节点）:");
PrintByLevel(perfectTree);
Console.WriteLine();

// 完全二叉树：最后一层可以不满，但必须从左到右连续排列
var completeTree = new TreeNode(1)
{
    Left = new TreeNode(2) { Left = new TreeNode(4), Right = new TreeNode(5) },
    Right = new TreeNode(3) { Left = new TreeNode(6) },     // 右孩子缺失
};
Console.WriteLine("  完全二叉树（最后一层的节点都靠左）:");
PrintByLevel(completeTree);
Console.WriteLine("    -> 中间没有空洞，所以能用数组紧凑表示（堆就是这样，见 8.3 节）");
Console.WriteLine();

var fullTree = new TreeNode(1)
{
    Left = new TreeNode(2) { Left = new TreeNode(4), Right = new TreeNode(5) },
    Right = new TreeNode(3),
};
Console.WriteLine("  满二叉树（每个节点要么有 2 个孩子，要么 0 个）:");
PrintByLevel(fullTree);
Console.WriteLine("    -> 注意它【不是】完全二叉树：右子树只有 3，没有孩子，");
Console.WriteLine("       但最后一层的 3 左边是空的 —— 不完全。");
Console.WriteLine();

Console.WriteLine("  三种形态的关系：");
Console.WriteLine("    完美二叉树 ⊂ 完全二叉树 ⊂ 满二叉树？—— 不完全是这样！");
Console.WriteLine("    准确说：完美 ⊆ 完全，完美 ⊆ 满；但完全和满【互不包含】。");
Console.WriteLine();

// ==================== 辅助函数 ====================

static TreeNode? BuildPerfect(int h)
{
    if (h < 0) return null;
    return new TreeNode(h)
    {
        Left = BuildPerfect(h - 1),
        Right = BuildPerfect(h - 1),
    };
}

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
