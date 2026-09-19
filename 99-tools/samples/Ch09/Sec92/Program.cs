// ==================== 三种深度优先遍历 ====================
//
// 唯一区别：「访问当前节点」这一行，放在两次递归的哪个位置。

/// <summary>前序：访问自己 -> 递归左 -> 递归右</summary>
static void PreOrder(TreeNode? node, List<int> result)
{
    if (node == null) return;
    result.Add(node.Value);          // ← 在两次递归【之前】
    PreOrder(node.Left, result);
    PreOrder(node.Right, result);
}

/// <summary>中序：递归左 -> 访问自己 -> 递归右</summary>
static void InOrder(TreeNode? node, List<int> result)
{
    if (node == null) return;
    InOrder(node.Left, result);
    result.Add(node.Value);          // ← 夹在两次递归【中间】
    InOrder(node.Right, result);
}

/// <summary>后序：递归左 -> 递归右 -> 访问自己</summary>
static void PostOrder(TreeNode? node, List<int> result)
{
    if (node == null) return;
    PostOrder(node.Left, result);
    PostOrder(node.Right, result);
    result.Add(node.Value);          // ← 在两次递归【之后】
}

// ==================== 迭代版：用显式栈 ====================

/// <summary>
/// 前序遍历的迭代版。
/// 注意：因为栈是后进先出，所以要先压【右】再压【左】，
/// 这样左孩子才会先出栈（保证「先左后右」的访问顺序）。
/// </summary>
static List<int> PreOrderIterative(TreeNode? root)
{
    var result = new List<int>();
    if (root == null) return result;

    var stack = new Stack<TreeNode>();
    stack.Push(root);

    while (stack.Count > 0)
    {
        var node = stack.Pop();
        result.Add(node.Value);
        if (node.Right != null) stack.Push(node.Right);   // 先压右
        if (node.Left != null) stack.Push(node.Left);     // 后压左 -> 先出栈
    }
    return result;
}

/// <summary>
/// 中序遍历的迭代版：一路向左压栈，走到头就弹出一个访问，再转向它的右子树。
/// </summary>
static List<int> InOrderIterative(TreeNode? root)
{
    var result = new List<int>();
    var stack = new Stack<TreeNode>();
    var current = root;

    while (current != null || stack.Count > 0)
    {
        while (current != null)          // 一路向左，全部压栈
        {
            stack.Push(current);
            current = current.Left;
        }
        current = stack.Pop();           // 弹出最左的
        result.Add(current.Value);       // 访问它
        current = current.Right;         // 转向右子树
    }
    return result;
}

/// <summary>
/// 后序遍历的迭代版：用「反向」的技巧 —— 按「根右左」的顺序遍历，最后整体反转。
/// </summary>
static List<int> PostOrderIterative(TreeNode? root)
{
    var result = new List<int>();
    if (root == null) return result;

    var stack = new Stack<TreeNode>();
    stack.Push(root);

    while (stack.Count > 0)
    {
        var node = stack.Pop();
        result.Add(node.Value);
        if (node.Left != null) stack.Push(node.Left);     // 先压左
        if (node.Right != null) stack.Push(node.Right);   // 后压右 -> 先出栈
    }

    result.Reverse();                                     // 关键：整体反转
    return result;
}

static string Show(List<int> list) => string.Join(" ", list);

// ==================== 构造一棵树 ====================

//                 1
//               /   \
//              2     3
//             / \   / \
//            4   5 6   7

var root = new TreeNode(1)
{
    Left = new TreeNode(2) { Left = new TreeNode(4), Right = new TreeNode(5) },
    Right = new TreeNode(3) { Left = new TreeNode(6), Right = new TreeNode(7) },
};

Console.WriteLine("=== 实验一：三种遍历顺序 ===");
Console.WriteLine();
Console.WriteLine("  树的结构：");
Console.WriteLine("                 1");
Console.WriteLine("               /   \\");
Console.WriteLine("              2     3");
Console.WriteLine("             / \\   / \\");
Console.WriteLine("            4   5 6   7");
Console.WriteLine();

var pre = new List<int>(); PreOrder(root, pre);
var ino = new List<int>(); InOrder(root, ino);
var post = new List<int>(); PostOrder(root, post);

Console.WriteLine($"  前序（自己→左→右）: {Show(pre)}");
Console.WriteLine($"  中序（左→自己→右）: {Show(ino)}");
Console.WriteLine($"  后序（左→右→自己）: {Show(post)}");
Console.WriteLine();

Console.WriteLine("  代码上的唯一区别，就是 `result.Add(node.Value)` 这一行的位置：");
Console.WriteLine("    前序：写在两次递归【之前】");
Console.WriteLine("    中序：夹在两次递归【中间】");
Console.WriteLine("    后序：写在两次递归【之后】");
Console.WriteLine();

Console.WriteLine("  这正是 2.1 节讲的规律 ——「写在递归调用前/后的语句，分别在去程/回程执行」。");
Console.WriteLine();

// ==================== 实验二：验证「去程 / 回程」的解释 ====================

Console.WriteLine("=== 实验二：用「去程/回程」理解三种遍历 ===");
Console.WriteLine();

Console.Write("  前序的访问顺序 = 去程的顺序: ");
VisitTrace(root, "pre");
Console.WriteLine();

Console.Write("  后序的访问顺序 = 回程的顺序: ");
VisitTrace(root, "post");
Console.WriteLine();

Console.WriteLine("  中序比较特殊：它在「访问完左子树、还没访问右子树」时访问自己，");
Console.WriteLine("  所以顺序是：一路去程到最左 -> 回程访问 -> 转去右子树 -> ...");
Console.WriteLine();

// ==================== 实验三：迭代版 ====================

Console.WriteLine("=== 实验三：三种遍历的迭代版 ===");
Console.WriteLine();

var preIt = PreOrderIterative(root);
var inoIt = InOrderIterative(root);
var postIt = PostOrderIterative(root);

Console.WriteLine($"  前序 递归: {Show(pre)}");
Console.WriteLine($"  前序 迭代: {Show(preIt)}   一致={pre.SequenceEqual(preIt)}");
Console.WriteLine();
Console.WriteLine($"  中序 递归: {Show(ino)}");
Console.WriteLine($"  中序 迭代: {Show(inoIt)}   一致={ino.SequenceEqual(inoIt)}");
Console.WriteLine();
Console.WriteLine($"  后序 递归: {Show(post)}");
Console.WriteLine($"  后序 迭代: {Show(postIt)}   一致={post.SequenceEqual(postIt)}");
Console.WriteLine();

Console.WriteLine("  三种迭代版的思路：");
Console.WriteLine("    前序：栈里先压右、后压左（这样左先出栈）");
Console.WriteLine("    中序：一路向左压栈，弹出一个就访问，再转向右子树");
Console.WriteLine("    后序：按「根右左」遍历，最后整体反转 —— 最巧妙的一个");
Console.WriteLine();

// ==================== 实验四：三种遍历各自的用途 ====================

Console.WriteLine("=== 实验四：三种遍历分别用来干什么 ===");
Console.WriteLine();

// 1. 前序：复制一棵树
Console.WriteLine("  1. 前序 -> 复制一棵树（先建根，再建子树）");
Console.WriteLine("     因为要「先有父亲，才能挂孩子」。");

// 2. 中序：BST 的有序性
Console.WriteLine();
Console.WriteLine("  2. 中序 -> 二叉搜索树的有序序列（10.1 节详讲）");

var bst = new TreeNode(20)
{
    Left = new TreeNode(10) { Left = new TreeNode(5), Right = new TreeNode(15) },
    Right = new TreeNode(30) { Left = new TreeNode(25), Right = new TreeNode(35) },
};
var bstInOrder = new List<int>(); InOrder(bst, bstInOrder);
Console.WriteLine($"     一棵二叉搜索树的中序遍历: {Show(bstInOrder)}");
Console.WriteLine($"     是不是从小到大？{IsSorted(bstInOrder)}");
Console.WriteLine("     -> 这就是 BST 的核心性质：「中序有序」");

// 3. 后序：释放内存 / 计算子树信息
Console.WriteLine();
Console.WriteLine("  3. 后序 -> 计算「依赖子树结果」的信息");
Console.WriteLine("     比如求树的高度：必须先知道左右子树的高度，才能算自己的。");
Console.WriteLine($"     这棵树的高度 = {TreeHeight(root)}");

Console.WriteLine();

// 4. 表达式树
Console.WriteLine("  4. 前序/中序/后序 -> 表达式树的不同表示（呼应 5.3 节）");
var exprTree = new TreeNode(0)     // 用节点的 Value 存不了运算符，这里只演示结构
{
    Left = new TreeNode(0),
    Right = new TreeNode(0),
};
Console.WriteLine("     表达式树 (1+2)*(3+4) 的三序遍历，正好对应：");
Console.WriteLine("       后序 -> 后缀表达式（逆波兰，5.3 节的计算器用这个）");
Console.WriteLine("       中序 -> 中缀表达式（需要加括号）");
Console.WriteLine("       前序 -> 前缀表达式（波兰表示法）");
Console.WriteLine();

// ==================== 辅助函数 ====================

static void VisitTrace(TreeNode? node, string mode, int depth = 0)
{
    if (node == null) return;
    if (mode == "pre") Console.Write($"{node.Value} ");
    VisitTrace(node.Left, mode, depth + 1);
    VisitTrace(node.Right, mode, depth + 1);
    if (mode == "post") Console.Write($"{node.Value} ");
}

static bool IsSorted(List<int> list)
{
    for (int i = 1; i < list.Count; i++)
        if (list[i] < list[i - 1]) return false;
    return true;
}

static int TreeHeight(TreeNode? node)
    => node == null ? -1 : 1 + Math.Max(TreeHeight(node.Left), TreeHeight(node.Right));

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
