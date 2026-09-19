// ==================== 层序遍历（BFS）====================

/// <summary>最基础的层序遍历：按层从上到下、每层从左到右访问所有节点。</summary>
static List<int> LevelOrder(TreeNode? root)
{
    var result = new List<int>();
    if (root == null) return result;

    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
        var node = queue.Dequeue();        // 从队首取出
        result.Add(node.Value);

        if (node.Left != null) queue.Enqueue(node.Left);     // 孩子从队尾加入
        if (node.Right != null) queue.Enqueue(node.Right);
    }
    return result;
}

/// <summary>按层分组：返回每一层的节点值。</summary>
static List<List<int>> LevelOrderGrouped(TreeNode? root)
{
    var result = new List<List<int>>();
    if (root == null) return result;

    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
        int levelSize = queue.Count;       // ★ 关键：先记住「这一层有多少个」
        var currentLevel = new List<int>();

        for (int i = 0; i < levelSize; i++)   // 只处理这一层的节点
        {
            var node = queue.Dequeue();
            currentLevel.Add(node.Value);
            if (node.Left != null) queue.Enqueue(node.Left);
            if (node.Right != null) queue.Enqueue(node.Right);
        }
        result.Add(currentLevel);
    }
    return result;
}

/// <summary>带层号的层序遍历。</summary>
static List<(int Value, int Level)> LevelOrderWithLevel(TreeNode? root)
{
    var result = new List<(int, int)>();
    if (root == null) return result;

    var queue = new Queue<(TreeNode Node, int Level)>();
    queue.Enqueue((root, 0));

    while (queue.Count > 0)
    {
        var (node, level) = queue.Dequeue();
        result.Add((node.Value, level));
        if (node.Left != null) queue.Enqueue((node.Left, level + 1));
        if (node.Right != null) queue.Enqueue((node.Right, level + 1));
    }
    return result;
}

/// <summary>右视图：每一层最右边的节点。</summary>
static List<int> RightSideView(TreeNode? root)
{
    var result = new List<int>();
    if (root == null) return result;

    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
        int levelSize = queue.Count;
        for (int i = 0; i < levelSize; i++)
        {
            var node = queue.Dequeue();
            if (i == levelSize - 1) result.Add(node.Value);   // 这一层最后一个
            if (node.Left != null) queue.Enqueue(node.Left);
            if (node.Right != null) queue.Enqueue(node.Right);
        }
    }
    return result;
}

// ==================== 深度优先（对照）====================

static int MaxDepth(TreeNode? node)
    => node == null ? 0 : 1 + Math.Max(MaxDepth(node.Left), MaxDepth(node.Right));

/// <summary>最小深度：从根到最近叶子的节点数。</summary>
static int MinDepthDFS(TreeNode? node)
{
    if (node == null) return 0;
    // 注意：如果一个孩子为空，不能把它当成深度 0 —— 必须走另一边
    if (node.Left == null) return 1 + MinDepthDFS(node.Right);
    if (node.Right == null) return 1 + MinDepthDFS(node.Left);
    return 1 + Math.Min(MinDepthDFS(node.Left), MinDepthDFS(node.Right));
}

/// <summary>最小深度：用 BFS —— 第一次遇到叶子就可以停。</summary>
static int MinDepthBFS(TreeNode? root)
{
    if (root == null) return 0;

    var queue = new Queue<(TreeNode Node, int Depth)>();
    queue.Enqueue((root, 1));

    while (queue.Count > 0)
    {
        var (node, depth) = queue.Dequeue();
        if (node.Left == null && node.Right == null) return depth;   // 第一个叶子
        if (node.Left != null) queue.Enqueue((node.Left, depth + 1));
        if (node.Right != null) queue.Enqueue((node.Right, depth + 1));
    }
    return 0;
}

// ==================== 构造树 ====================

//                 1
//               /   \
//              2     3
//             / \     \
//            4   5     6
//           /
//          7

var root = new TreeNode(1)
{
    Left = new TreeNode(2)
    {
        Left = new TreeNode(4) { Left = new TreeNode(7) },
        Right = new TreeNode(5),
    },
    Right = new TreeNode(3)
    {
        Right = new TreeNode(6),
    },
};

Console.WriteLine("=== 实验一：层序遍历 ===");
Console.WriteLine();
Console.WriteLine("  树的结构：");
Console.WriteLine("                 1");
Console.WriteLine("               /   \\");
Console.WriteLine("              2     3");
Console.WriteLine("             / \\     \\");
Console.WriteLine("            4   5     6");
Console.WriteLine("           /");
Console.WriteLine("          7");
Console.WriteLine();

Console.WriteLine($"  层序遍历（BFS）: {string.Join(" ", LevelOrder(root))}");
Console.WriteLine();

Console.WriteLine("  按层分组：");
foreach (var (level, nodes) in LevelOrderGrouped(root).Select((l, i) => (i, l)))
{
    Console.WriteLine($"    第 {level} 层: {string.Join(" ", nodes)}");
}
Console.WriteLine();

Console.WriteLine("  带层号：");
Console.WriteLine($"    {string.Join("  ", LevelOrderWithLevel(root).Select(x => $"{x.Value}(L{x.Level})"))}");
Console.WriteLine();

Console.WriteLine("=== 实验二：BFS 的经典应用 ===");
Console.WriteLine();

Console.WriteLine($"  右视图（每层最右边的节点）: {string.Join(" ", RightSideView(root))}");
Console.WriteLine("    用途：从右侧看这棵树能看到哪些节点。");
Console.WriteLine();

Console.WriteLine("=== 实验三：BFS vs DFS —— 求最小深度 ===");
Console.WriteLine();

Console.WriteLine($"  最大深度（DFS 后序）: {MaxDepth(root)}");
Console.WriteLine($"  最小深度（DFS）      : {MinDepthDFS(root)}");
Console.WriteLine($"  最小深度（BFS）      : {MinDepthBFS(root)}");
Console.WriteLine($"  两者一致: {MinDepthDFS(root) == MinDepthBFS(root)}");
Console.WriteLine();

Console.WriteLine("  为什么最小深度用 BFS 更好？");
Console.WriteLine("    BFS 是「一层一层」访问的，所以【第一次遇到叶子】就是最浅的那个叶子 —— 可以立刻返回；");
Console.WriteLine("    DFS 必须遍历完整棵树才能确定哪个叶子最浅。");
Console.WriteLine();

// 造一个「左边很深、右边很浅」的树来对比
var lopsided = new TreeNode(1)
{
    Left = new TreeNode(2)
    {
        Left = new TreeNode(3)
        {
            Left = new TreeNode(4)
            {
                Left = new TreeNode(5),
            },
        },
    },
    Right = new TreeNode(9),      // 这个叶子就在第 1 层
};

Console.WriteLine("  对比一棵「左深右浅」的树：");
Console.WriteLine("         1");
Console.WriteLine("        / \\");
Console.WriteLine("       2   9   <- 9 是叶子，深度只有 1");
Console.WriteLine("      /");
Console.WriteLine("     3");
Console.WriteLine("    /");
Console.WriteLine("   4");
Console.WriteLine("  /");
Console.WriteLine(" 5");
Console.WriteLine();

int bfsVisits = 0;
var q = new Queue<TreeNode>();
q.Enqueue(lopsided);
while (q.Count > 0)
{
    var n = q.Dequeue();
    bfsVisits++;
    if (n.Left == null && n.Right == null) break;
    if (n.Left != null) q.Enqueue(n.Left);
    if (n.Right != null) q.Enqueue(n.Right);
}

Console.WriteLine($"    这棵树共 {CountNodes(lopsided)} 个节点");
Console.WriteLine($"    BFS 找最小深度只访问了 {bfsVisits} 个节点就停了（找到 9 就返回）");
Console.WriteLine($"    DFS 必须访问全部 {CountNodes(lopsided)} 个节点");
Console.WriteLine();

Console.WriteLine("  结论：BFS 找到第一个叶子就能停，而 DFS 必须走完全程。");
Console.WriteLine("        ——这就是「BFS 适合求最短/最少」的原因（12 章的最短路径会用到大）。");
Console.WriteLine();

Console.WriteLine("=== 实验四：BFS 和 DFS 的对比 ===");
Console.WriteLine();
Console.WriteLine($"  {"维度",-16} | {"BFS（层序）",-26} | {"DFS（前/中/后序）",-26}");
Console.WriteLine(new string('-', 74));
Console.WriteLine($"  {"数据结构",-16} | {"队列",-26} | {"栈（递归隐含）",-26}");
Console.WriteLine($"  {"访问顺序",-16} | {"一层一层",-26} | {"一条路走到底",-26}");
Console.WriteLine($"  {"空间",-16} | {"O(最宽的一层)",-26} | {"O(树高)",-26}");
Console.WriteLine($"  {"适合",-16} | {"最短路径、层相关信息",-26} | {"路径、子树信息、回溯",-26}");
Console.WriteLine();
Console.WriteLine("  空间开销的差异很关键：");
Console.WriteLine("    完全二叉树：最宽的一层约 n/2 个节点 -> BFS 要 O(n) 空间");
Console.WriteLine("    退化链    ：树高 n -> DFS 要 O(n) 空间");
Console.WriteLine("    ——「哪个更省空间」取决于树的形状。");
Console.WriteLine();

static int CountNodes(TreeNode? n) => n == null ? 0 : 1 + CountNodes(n.Left) + CountNodes(n.Right);

// ==================== 节点定义 ====================

public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value) => Value = value;
}
