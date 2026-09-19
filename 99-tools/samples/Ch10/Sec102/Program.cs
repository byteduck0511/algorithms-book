// ==================== 查找 ====================

/// <summary>查找：从根往下走，比当前节点小就往左、大就往右。</summary>
static TreeNode? Search(TreeNode? node, int value, ref long comparisons)
{
    while (node != null)
    {
        comparisons++;
        if (value == node.Value) return node;
        node = value < node.Value ? node.Left : node.Right;
    }
    return null;
}

// ==================== 插入 ====================

/// <summary>插入：沿着查找路径走到空位，把新节点挂上去。</summary>
static TreeNode? Insert(TreeNode? node, int value)
{
    if (node == null) return new TreeNode(value);       // 找到空位，新建节点

    if (value < node.Value) node.Left = Insert(node.Left, value);
    else if (value > node.Value) node.Right = Insert(node.Right, value);
    // value == node.Value：重复值，忽略

    return node;
}

// ==================== 删除（三种情况）====================

/// <summary>找以 node 为根的子树里的最小节点（一路向左）。</summary>
static TreeNode MinNode(TreeNode node)
{
    while (node.Left != null) node = node.Left;
    return node;
}

static TreeNode? Delete(TreeNode? node, int value)
{
    if (node == null) return null;                       // 没找到

    if (value < node.Value)
    {
        node.Left = Delete(node.Left, value);
    }
    else if (value > node.Value)
    {
        node.Right = Delete(node.Right, value);
    }
    else
    {
        // 找到要删的节点，分三种情况
        if (node.Left == null) return node.Right;        // 情况 1：没有左孩子（含叶子）
        if (node.Right == null) return node.Left;        // 情况 2：没有右孩子

        // 情况 3：两个孩子都有 —— 用「中序后继」（右子树最小值）替代
        var successor = MinNode(node.Right);
        node.Value = successor.Value;
        node.Right = Delete(node.Right, successor.Value);   // 把后继从右子树里删掉
    }
    return node;
}

// ==================== 工具 ====================

static void PrintTree(TreeNode? node, string indent = "", bool isLast = true, bool isRoot = true)
{
    if (node == null) return;
    if (isRoot) Console.WriteLine($"      {node.Value}");
    else Console.WriteLine($"{indent}{(isLast ? "└─ " : "├─ ")}{node.Value}");

    string childIndent = isRoot ? "      " : indent + (isLast ? "   " : "│  ");
    bool hasLeft = node.Left != null, hasRight = node.Right != null;
    if (hasLeft) PrintTree(node.Left, childIndent, !hasRight, false);
    if (hasRight) PrintTree(node.Right, childIndent, true, false);
}

static void InOrder(TreeNode? node, List<int> result)
{
    if (node == null) return;
    InOrder(node.Left, result);
    result.Add(node.Value);
    InOrder(node.Right, result);
}

/// <summary>把树的中序遍历结果拼成字符串，方便打印。</summary>
static string InOrderString(TreeNode? node)
{
    var list = new List<int>();
    InOrder(node, list);
    return string.Join(" ", list);
}

static int Height(TreeNode? node)
    => node == null ? -1 : 1 + Math.Max(Height(node.Left), Height(node.Right));

static int CountNodes(TreeNode? node)
    => node == null ? 0 : 1 + CountNodes(node.Left) + CountNodes(node.Right);

/// <summary>检查整棵树是否满足 BST 性质。</summary>
static bool IsValidBst(TreeNode? node, long min = long.MinValue, long max = long.MaxValue)
{
    if (node == null) return true;
    if (node.Value <= min || node.Value >= max) return false;
    return IsValidBst(node.Left, min, node.Value) && IsValidBst(node.Right, node.Value, max);
}

// ==================== 实验一：查找 ====================

TreeNode? root = null;
foreach (int v in new[] { 20, 10, 30, 5, 15, 25, 35 })
    root = Insert(root, v);

Console.WriteLine("=== 实验一：查找 ===");
Console.WriteLine();
Console.WriteLine("  树的结构：");
PrintTree(root);
Console.WriteLine();

foreach (int target in new[] { 15, 25, 100 })
{
    long cmp = 0;
    var found = Search(root, target, ref cmp);
    Console.WriteLine($"  查找 {target,3}: {(found != null ? "找到" : "未找到")}，比较了 {cmp} 次");
}
Console.WriteLine();
Console.WriteLine("  比较次数 = 从根到目标的路径长度 + 1，也就是 O(树高)。");
Console.WriteLine();

// ==================== 实验二：插入 ====================

Console.WriteLine("=== 实验二：插入 ===");
Console.WriteLine();
Console.WriteLine($"  插入前: {InOrderString(root)}");

root = Insert(root, 12);
Console.WriteLine($"  插入 12 后: {InOrderString(root)}");
Console.WriteLine("    （12 比 10 大、比 15 小 —— 它落到了 15 的左孩子位置）");
Console.WriteLine();

root = Insert(root, 12);      // 重复插入
Console.WriteLine($"  再插入一次 12: {InOrderString(root)}");
Console.WriteLine("    （重复值被忽略，元素个数不变）");
Console.WriteLine();

// ==================== 实验三：删除的三种情况 ====================

Console.WriteLine("=== 实验三：删除 —— 三种情况 ===");
Console.WriteLine();

void TestDelete(string label, int value)
{
    Console.WriteLine($"  【{label}】删除 {value}：");
    Console.Write("    删除前: ");
    PrintTree(root, "      ", true, true);
    root = Delete(root, value);
    Console.Write("    删除后: ");
    PrintTree(root, "      ", true, true);
    Console.WriteLine($"    中序: {InOrderString(root)}");
    Console.WriteLine($"    仍是合法 BST: {IsValidBst(root)}");
    Console.WriteLine();
}

// 情况 1：叶子节点
root = Insert(root, 5);       // 确保 5 还在
TestDelete("情况 1：删除叶子", 5);

// 情况 2：只有一个孩子
root = Insert(root, 5);
root = Insert(root, 7);       // 5 现在有一个右孩子 7
TestDelete("情况 2：只有一个孩子", 5);

// 情况 3：两个孩子
TestDelete("情况 3：有两个孩子", 30);

Console.WriteLine("  三种情况的处理方式：");
Console.WriteLine("    情况 1（叶子）      : 直接删掉，父节点的对应指针置空");
Console.WriteLine("    情况 2（只有一个孩子）: 用孩子「顶替」自己的位置");
Console.WriteLine("    情况 3（两个孩子）   : 用【中序后继】（右子树最小值）替代自己，");
Console.WriteLine("                          再去右子树里删掉那个后继（它会退化成情况 1 或 2）");
Console.WriteLine();

// 为什么用中序后继？
Console.WriteLine("  为什么情况 3 要用「右子树的最小值」？");
Console.WriteLine("    因为它是「所有比当前节点大的元素中最小的那个」——");
Console.WriteLine("    用它替代后：左边所有元素仍小于它 ✓，右边所有元素仍大于它 ✓");
Console.WriteLine("    BST 的性质得以保持。");
Console.WriteLine();

// ==================== 实验四：复杂度总结 ====================

const int N = 1_000_000;
Console.WriteLine($"=== 实验四：三种操作的理论复杂度（n = {N:N0}）===");
Console.WriteLine();
Console.WriteLine($"  {"操作",-12} | {"平均",-14} | {"最坏",-14} | 说明");
Console.WriteLine("  " + new string('-', 70));
Console.WriteLine($"  {"查找",-12} | {"O(log n)",-14} | {"O(n)",-14} | 从根往下走，比较次数 = 树高");
Console.WriteLine($"  {"插入",-12} | {"O(log n)",-14} | {"O(n)",-14} | 先查找位置，再挂上去");
Console.WriteLine($"  {"删除",-12} | {"O(log n)",-14} | {"O(n)",-14} | 查找 + 调整指针");
Console.WriteLine();
Console.WriteLine($"  平均情况（树比较平衡）：树高约 log2({N:N0}) ≈ 20，所以约 20 次比较。");
Console.WriteLine($"  最坏情况（树退化成链）：树高 {N:N0}，需要 {N:N0} 次比较 —— 和链表一样慢！");
Console.WriteLine();
Console.WriteLine("  最坏情况是怎么发生的？下一节（10.3）会实测给你看。");
Console.WriteLine();

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
