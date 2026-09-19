// ==================== 题型一：求深度/高度（后序，自底向上）====================

/// <summary>最大深度（节点数口径）。返回值 = 以 node 为根的子树的最大深度。</summary>
static int MaxDepth(TreeNode? node)
    => node == null ? 0 : 1 + Math.Max(MaxDepth(node.Left), MaxDepth(node.Right));

// ==================== 题型二：判断平衡（自底向上，带"提前失败"）====================

/// <summary>
/// 判断是否为平衡二叉树（左右子树高度差不超过 1）。
/// 技巧：用返回 -1 表示「已经不平衡了」，这样只需要遍历一次。
/// </summary>
static int CheckBalance(TreeNode? node)
{
    if (node == null) return 0;

    int left = CheckBalance(node.Left);
    if (left == -1) return -1;                  // 左子树已经不平衡，不用再算了

    int right = CheckBalance(node.Right);
    if (right == -1) return -1;                 // 右子树不平衡

    if (Math.Abs(left - right) > 1) return -1;  // 自己不平衡

    return 1 + Math.Max(left, right);           // 返回高度
}

static bool IsBalanced(TreeNode? root) => CheckBalance(root) != -1;

/// <summary>反面教材：自顶向下，每个节点都重复计算高度，O(n^2)。</summary>
static bool IsBalancedNaive(TreeNode? node)
{
    if (node == null) return true;
    int left = Height(node.Left);
    int right = Height(node.Right);
    if (Math.Abs(left - right) > 1) return false;
    return IsBalancedNaive(node.Left) && IsBalancedNaive(node.Right);
}

static int Height(TreeNode? node)
    => node == null ? -1 : 1 + Math.Max(Height(node.Left), Height(node.Right));

// ==================== 题型三：最近公共祖先（后序，自底向上）====================

/// <summary>
/// 找 p 和 q 的最近公共祖先。
/// 返回值的含义有三种：
///   1. 找到了 p 或 q        -> 返回那个节点
///   2. 找到了「p 和 q 的 LCA」-> 返回 LCA
///   3. 什么都没找到          -> 返回 null
/// </summary>
static TreeNode? LowestCommonAncestor(TreeNode? root, TreeNode p, TreeNode q)
{
    if (root == null) return null;
    if (root == p || root == q) return root;          // 找到了其中一个

    var left = LowestCommonAncestor(root.Left, p, q);
    var right = LowestCommonAncestor(root.Right, p, q);

    // 左右都找到了 -> 说明 p 和 q 分居两侧，root 就是 LCA
    if (left != null && right != null) return root;

    // 只有一边找到 -> 说明两个目标都在那一边（或者只找到了一个）
    return left ?? right;
}

/// <summary>辅助：按值找节点。</summary>
static TreeNode? Find(TreeNode? node, int value)
{
    if (node == null) return null;
    if (node.Value == value) return node;
    return Find(node.Left, value) ?? Find(node.Right, value);
}

// ==================== 题型四：路径相关（自顶向下带状态）====================

/// <summary>从根到目标节点的路径。</summary>
static bool FindPath(TreeNode? node, int target, List<int> path)
{
    if (node == null) return false;

    path.Add(node.Value);                       // 先假设它在路径上

    if (node.Value == target) return true;

    if (FindPath(node.Left, target, path)) return true;    // 左边找到了
    if (FindPath(node.Right, target, path)) return true;   // 右边找到了

    path.RemoveAt(path.Count - 1);              // 两边都没找到 -> 回溯，把它移出路径
    return false;
}

// ==================== 题型五：树的结构判断（需要"双节点递归"）====================

/// <summary>判断两棵树是否完全相同。</summary>
static bool IsSameTree(TreeNode? a, TreeNode? b)
{
    if (a == null && b == null) return true;    // 都为空 -> 相同
    if (a == null || b == null) return false;   // 一空一非空 -> 不同
    if (a.Value != b.Value) return false;       // 值不同

    return IsSameTree(a.Left, b.Left) && IsSameTree(a.Right, b.Right);
}

/// <summary>判断两棵树是否镜像对称（左右翻转后相同）。</summary>
static bool IsSymmetric(TreeNode? a, TreeNode? b)
{
    if (a == null && b == null) return true;
    if (a == null || b == null) return false;
    if (a.Value != b.Value) return false;

    // 关键：左的左 vs 右的右，左的右 vs 右的左 —— 交叉比较
    return IsSymmetric(a.Left, b.Right) && IsSymmetric(a.Right, b.Left);
}

// ==================== 构造测试数据 ====================

// 平衡的树：            1
//                    /   \
//                   2     3
//                  / \   / \
//                 4   5 6   7

var balanced = new TreeNode(1)
{
    Left = new TreeNode(2) { Left = new TreeNode(4), Right = new TreeNode(5) },
    Right = new TreeNode(3) { Left = new TreeNode(6), Right = new TreeNode(7) },
};

// 不平衡的树：    1
//               /
//              2
//             /
//            3
//           /
//          4

var unbalanced = new TreeNode(1)
{
    Left = new TreeNode(2)
    {
        Left = new TreeNode(3)
        {
            Left = new TreeNode(4),
        },
    },
};

// LCA 测试用的树：        3
//                       /   \
//                      5     1
//                     / \   / \
//                    6   2 0   8
//                       / \
//                      7   4

var lcaTree = new TreeNode(3)
{
    Left = new TreeNode(5)
    {
        Left = new TreeNode(6),
        Right = new TreeNode(2) { Left = new TreeNode(7), Right = new TreeNode(4) },
    },
    Right = new TreeNode(1)
    {
        Left = new TreeNode(0),
        Right = new TreeNode(8),
    },
};

Console.WriteLine("=== 题型一：最大深度 ===");
Console.WriteLine();
Console.WriteLine($"  平衡树的最大深度: {MaxDepth(balanced)}（预期 3）");
Console.WriteLine($"  链状树的最大深度: {MaxDepth(unbalanced)}（预期 4）");
Console.WriteLine();

Console.WriteLine("=== 题型二：判断平衡 ===");
Console.WriteLine();
Console.WriteLine($"  平衡树  是否平衡: {IsBalanced(balanced)}（预期 True）");
Console.WriteLine($"  链状树  是否平衡: {IsBalanced(unbalanced)}（预期 False）");
Console.WriteLine();

// ---- 场景 A：链状树（不平衡发生在根节点，朴素版会【提前失败】）----
var skewed = BuildSkewed(8000);
var sw = System.Diagnostics.Stopwatch.StartNew();
bool sa = IsBalanced(skewed);
sw.Stop();
double skewedFast = sw.Elapsed.TotalMilliseconds;

sw.Restart();
bool sb = IsBalancedNaive(skewed);
sw.Stop();
double skewedNaive = sw.Elapsed.TotalMilliseconds;

Console.WriteLine("  场景 A：8000 个节点的【链状树】（处处不平衡，根节点就能发现）");
Console.WriteLine($"    自底向上: {skewedFast,8:F3} ms");
Console.WriteLine($"    自顶向下: {skewedNaive,8:F3} ms");
Console.WriteLine($"    结果一致: {sa == sb}");
Console.WriteLine("    -> 注意：这里【朴素版反而更快】！因为它一进根节点就算出左右子树高度差极大，");
Console.WriteLine("       立刻返回 false，根本没往下递归。");
Console.WriteLine();

// ---- 场景 B：平衡树（朴素版不会提前失败，必须走完全程）----
var bigBalanced = BuildPerfect(12);      // 2^13 - 1 = 8191 个节点
Console.WriteLine($"  场景 B：{CountNodes(bigBalanced):N0} 个节点的【完美平衡树】（处处平衡，必须走完全程）");

sw.Restart();
bool ba = IsBalanced(bigBalanced);
sw.Stop();
double balFast = sw.Elapsed.TotalMilliseconds;

sw.Restart();
bool bb = IsBalancedNaive(bigBalanced);
sw.Stop();
double balNaive = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"    自底向上: {balFast,8:F3} ms");
Console.WriteLine($"    自顶向下: {balNaive,8:F3} ms");
Console.WriteLine($"    结果一致: {ba == bb}，自底向上快 {(balNaive / Math.Max(balFast, 0.0001)):F1} 倍");
Console.WriteLine();
Console.WriteLine("  【结论】朴素版的最坏情况不是链状树（会提前失败），而是【平衡树】——");
Console.WriteLine("  因为它必须走到每一个节点，而每个节点都要重新算一遍子树高度。");
Console.WriteLine("  代价 = Σ(每个节点的子树大小) ≈ n*log2(n)，所以是 O(n log n) 而不是 O(n^2)。");
Console.WriteLine();

Console.WriteLine("=== 题型三：最近公共祖先（LCA）===");
Console.WriteLine();
Console.WriteLine("  测试树：        3");
Console.WriteLine("                 /   \\");
Console.WriteLine("                5     1");
Console.WriteLine("               / \\   / \\");
Console.WriteLine("              6   2 0   8");
Console.WriteLine("                 / \\");
Console.WriteLine("                7   4");
Console.WriteLine();

var pairs = new[] { (5, 1), (5, 4), (6, 4), (7, 8), (3, 8) };
foreach (var (v1, v2) in pairs)
{
    var p = Find(lcaTree, v1)!;
    var q = Find(lcaTree, v2)!;
    var lca = LowestCommonAncestor(lcaTree, p, q);
    Console.WriteLine($"    LCA({v1}, {v2}) = {lca?.Value}");
}
Console.WriteLine();

Console.WriteLine("  这个算法的精髓在于【返回值的语义】：");
Console.WriteLine("    - 找到 p 或 q          -> 返回那个节点");
Console.WriteLine("    - 左右子树都非空        -> 当前节点就是 LCA");
Console.WriteLine("    - 只有一边非空          -> 把那边结果往上传递");
Console.WriteLine();

Console.WriteLine("=== 题型四：根到目标节点的路径 ===");
Console.WriteLine();
var path = new List<int>();
FindPath(lcaTree, 4, path);
Console.WriteLine($"  到节点 4 的路径: {string.Join(" -> ", path)}");
Console.WriteLine("  注意最后那句 path.RemoveAt —— 那就是【回溯】。");
Console.WriteLine("  路径这种「走到哪算到哪、走错了要退回来」的问题，必须用回溯。");
Console.WriteLine();

Console.WriteLine("=== 题型五：树的结构判断 ===");
Console.WriteLine();

var treeA = new TreeNode(1) { Left = new TreeNode(2), Right = new TreeNode(3) };
var treeB = new TreeNode(1) { Left = new TreeNode(2), Right = new TreeNode(3) };
var treeC = new TreeNode(1) { Left = new TreeNode(3), Right = new TreeNode(2) };

Console.WriteLine($"  树 A 和树 B 相同吗？ {IsSameTree(treeA, treeB)}（预期 True）");
Console.WriteLine($"  树 A 和树 C 相同吗？ {IsSameTree(treeA, treeC)}（预期 False）");
Console.WriteLine();

// 对称的树
var symmetric = new TreeNode(1)
{
    Left = new TreeNode(2) { Left = new TreeNode(3), Right = new TreeNode(4) },
    Right = new TreeNode(2) { Left = new TreeNode(4), Right = new TreeNode(3) },
};
Console.WriteLine($"  这棵树对称吗？ {IsSymmetric(symmetric.Left, symmetric.Right)}（预期 True）");
Console.WriteLine($"  非对称的树呢？ {IsSymmetric(treeA.Left, treeA.Right)}（预期 False）");
Console.WriteLine();

Console.WriteLine("=== 总结：两类递归套路 ===");
Console.WriteLine();
Console.WriteLine($"  {"类型",-20} | {"特点",-30} | {"典型题目",-28}");
Console.WriteLine(new string('-', 82));
Console.WriteLine($"  {"自顶向下",-20} | {"带着状态往下传，到叶子判定",-30} | {"根到叶子的路径",-28}");
Console.WriteLine($"  {"自底向上（后序）",-20} | {"先要子树结果，再算自己",-30} | {"高度、平衡、LCA、节点数",-28}");
Console.WriteLine();
Console.WriteLine("  判断方法：问自己「要算当前节点，需不需要先知道子树的结果？」");
Console.WriteLine("    需要 -> 自底向上（后序）");
Console.WriteLine("    不需要 -> 自顶向下（前序）");
Console.WriteLine();

// ==================== 辅助 ====================

static TreeNode BuildSkewed(int n)
{
    var root = new TreeNode(1);
    var cur = root;
    for (int i = 2; i <= n; i++)
    {
        cur.Left = new TreeNode(i);
        cur = cur.Left;
    }
    return root;
}

/// <summary>构造高度为 h 的完美二叉树（2^(h+1) - 1 个节点）。</summary>
static TreeNode? BuildPerfect(int h)
{
    if (h < 0) return null;
    return new TreeNode(h)
    {
        Left = BuildPerfect(h - 1),
        Right = BuildPerfect(h - 1),
    };
}

static int CountNodes(TreeNode? n) => n == null ? 0 : 1 + CountNodes(n.Left) + CountNodes(n.Right);

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
