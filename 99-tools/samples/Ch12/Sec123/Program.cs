using System.Diagnostics;

// ==================== 实验一：BFS 的基本过程 ====================

var graph = new Graph();
foreach (var (a, b) in new[]
         {
             (0, 1), (0, 2),
             (1, 3),
             (2, 3), (2, 4),
             (4, 5),
         })
    graph.AddEdge(a, b);

Console.WriteLine("=== 要遍历的图 ===");
Console.WriteLine();
Console.WriteLine("        0 —— 1");
Console.WriteLine("        |    |");
Console.WriteLine("        2 —— 3");
Console.WriteLine("        |");
Console.WriteLine("        4 —— 5");
Console.WriteLine();

/// <summary>BFS：一层一层扩散，记录每个顶点的距离和前驱。</summary>
static (int[] Dist, int[] Prev) Bfs(Graph g, int start, bool trace = false)
{
    var dist = new int[g.VertexCount];
    var prev = new int[g.VertexCount];
    Array.Fill(dist, -1);              // -1 表示「没访问过」
    Array.Fill(prev, -1);
    var queue = new Queue<int>();

    dist[start] = 0;
    queue.Enqueue(start);

    int level = 0;
    while (queue.Count > 0)
    {
        int levelSize = queue.Count;    // ★ 这一层有多少个（9.3 节的技巧）
        if (trace) Console.WriteLine($"    第 {level} 层:");

        for (int i = 0; i < levelSize; i++)
        {
            int v = queue.Dequeue();
            if (trace) Console.Write($"      {v}");

            foreach (int next in g.Neighbors(v))
            {
                if (dist[next] != -1) continue;    // 已经访问过
                dist[next] = dist[v] + 1;          // 距离 = 前一个的距离 + 1
                prev[next] = v;                    // 记录前驱，用来还原路径
                queue.Enqueue(next);
            }
        }
        if (trace) Console.WriteLine();
        level++;
    }

    return (dist, prev);
}

Console.WriteLine("=== 实验一：BFS 一层一层扩散（从 0 出发）===");
Console.WriteLine();
var (dist, prev) = Bfs(graph, 0, trace: true);
Console.WriteLine();

Console.WriteLine($"  各顶点的距离（到 0 的最少边数）：");
for (int i = 0; i < graph.VertexCount; i++)
    Console.WriteLine($"    顶点 {i}: 距离 {dist[i]}");
Console.WriteLine();

// ==================== 实验二：BFS 一定找到最短路径 ====================

Console.WriteLine("=== 实验二：BFS 找最短路径 ===");
Console.WriteLine();

/// <summary>用 prev 数组还原从 start 到 target 的路径。</summary>
static List<int>? ReconstructPath(int[] prev, int start, int target)
{
    if (prev[target] == -1 && target != start) return null;    // 不可达

    var path = new List<int>();
    for (int v = target; v != -1; v = prev[v])
        path.Add(v);

    path.Reverse();
    return path[0] == start ? path : null;
}

// 注意：prev/dist 数组是「从 0 出发」的 BFS 树，
// 要查其他起点的路径，必须【从那个起点重新跑一次 BFS】。
foreach (var (from, to) in new[] { (0, 5), (0, 3), (1, 5), (2, 2) })
{
    var (d, p) = Bfs(graph, from);          // 每次都从 from 重新跑
    var path = ReconstructPath(p, from, to);
    string pathText = path == null ? "不可达" : string.Join(" -> ", path);
    Console.WriteLine($"  {from} 到 {to}: {pathText}（{d[to]} 步）");
}
Console.WriteLine();

Console.WriteLine("  对比 12.2 节 DFS 找的路径（同样的图，从 0 出发）：");
Console.WriteLine("    DFS:  0 -> 1 -> 3 -> 2 -> 4 -> ...   （可能绕远路）");
Console.WriteLine("    BFS:  0 -> 2 -> 4 -> 5               （一定是最短的）");
Console.WriteLine();

// ==================== 实验三：BFS 为什么保证最短 ====================

Console.WriteLine("=== 实验三：BFS 为什么一定找到最短路径？ ===");
Console.WriteLine();
Console.WriteLine("  因为 BFS 是【按距离一层一层】扩展的：");
Console.WriteLine();
Console.WriteLine("    第 0 层：起点自己              （距离 0）");
Console.WriteLine("    第 1 层：起点的所有邻居         （距离 1）");
Console.WriteLine("    第 2 层：距离 2 的所有顶点      （距离 2）");
Console.WriteLine("    ...");
Console.WriteLine();
Console.WriteLine("  关键：一个顶点【第一次被访问时】，它的距离就已经确定了 ——");
Console.WriteLine("        因为所有距离更小的顶点都已经处理完了，不可能有更短的路径。");
Console.WriteLine();

// 实测验证：BFS 的距离 vs 暴力枚举所有路径的最短长度
Console.WriteLine("  实测验证（随机图上对比 BFS 距离与暴力最短距离）：");
var rng = new Random(42);
int mismatches = 0;

for (int trial = 0; trial < 20; trial++)
{
    var g = new Graph();
    for (int i = 0; i < 12; i++)
        for (int j = i + 1; j < 12; j++)
            if (rng.NextDouble() < 0.25) g.AddEdge(i, j);

    var (d, _) = Bfs(g, 0);

    // 暴力：DFS 枚举所有路径，找最短的
    for (int target = 0; target < 12; target++)
    {
        int brute = BruteForceShortest(g, 0, target);
        if (brute != d[target]) mismatches++;
    }
}

Console.WriteLine($"    对比了 20 张随机图 × 12 个目标点 = 240 组");
Console.WriteLine($"    不一致的组数: {mismatches}");
Console.WriteLine($"    {(mismatches == 0 ? "BFS 的距离与暴力最短距离完全一致 ✓" : "有偏差！")}");
Console.WriteLine();

/// <summary>暴力枚举所有简单路径，找最短的（只用于小图验证）。</summary>
static int BruteForceShortest(Graph g, int from, int to)
{
    int best = int.MaxValue;
    var visited = new bool[g.VertexCount];

    void Dfs(int v, int len)
    {
        if (len >= best) return;                 // 剪枝
        if (v == to) { best = len; return; }

        visited[v] = true;
        foreach (int next in g.Neighbors(v))
            if (!visited[next]) Dfs(next, len + 1);
        visited[v] = false;
    }

    Dfs(from, 0);
    return best == int.MaxValue ? -1 : best;
}

// ==================== 实验四：BFS vs DFS 的复杂度对比 ====================

const int N = 200_000;
Console.WriteLine($"=== 实验四：大规模图上的 BFS vs DFS（{N:N0} 个顶点的链式图）===");
Console.WriteLine();

var bigGraph = new Graph();
for (int i = 0; i + 1 < N; i++) bigGraph.AddEdge(i, i + 1);

var sw = Stopwatch.StartNew();
var (bigDist, _) = Bfs(bigGraph, 0);
sw.Stop();
double bfsMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
var seen = new bool[N];
var stack = new Stack<int>();
stack.Push(0);
int visitedCount = 0;
while (stack.Count > 0)
{
    int v = stack.Pop();
    if (seen[v]) continue;
    seen[v] = true;
    visitedCount++;
    foreach (int n in bigGraph.Neighbors(v)) if (!seen[n]) stack.Push(n);
}
sw.Stop();
double dfsMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  BFS: {bfsMs,7:F1} ms（访问 {N:N0} 个顶点）");
Console.WriteLine($"  DFS: {dfsMs,7:F1} ms（访问 {visitedCount:N0} 个顶点）");
Console.WriteLine();
Console.WriteLine($"  复杂度都是 O(V + E)，但 BFS 实际慢了 {bfsMs / dfsMs:F2} 倍。原因是：");
Console.WriteLine($"    1. BFS 每次出队后还要维护 dist[] 和 prev[] 两个数组；");
Console.WriteLine($"    2. Queue<T>（环形缓冲区）的常数比 Stack<T>（数组尾部操作）大。");
Console.WriteLine($"  —— 这个差距是【常数级】的，不是量级差别。");
Console.WriteLine();
Console.WriteLine($"  验证：BFS 算出的最远距离 = {bigDist[N - 1]}（链式图，从 0 到 N-1 就是 N-1 步）");
Console.WriteLine();

// ==================== 实验五：BFS 的内存代价 ====================

Console.WriteLine("=== 实验五：BFS 的队列可能很大 ===");
Console.WriteLine();

// 造一个"星形图"：中心连着一堆叶子
var star = new Graph();
const int LEAVES = 100_000;
for (int i = 1; i <= LEAVES; i++) star.AddEdge(0, i);

sw.Restart();
var (starDist, _) = Bfs(star, 0);
sw.Stop();

Console.WriteLine($"  星形图：中心 0 连着 {LEAVES:N0} 个叶子");
Console.WriteLine($"  BFS 处理第 0 层时，会把 {LEAVES:N0} 个叶子【全部】压进队列");
Console.WriteLine($"  队列峰值大小 ≈ {LEAVES:N0} —— 这就是 BFS 的内存代价：O(最宽的一层)");
Console.WriteLine($"  耗时 {sw.Elapsed.TotalMilliseconds:F1} ms，最远距离 = {starDist.Max()}");
Console.WriteLine();

Console.WriteLine("  对比【递归版 DFS】处理同一张图（栈深度 = 递归深度）：");
sw.Restart();
var seenStar = new bool[LEAVES + 1];
int maxDepth = 0;
int starVisited = 0;

void DfsRecursive(int v, int depth)
{
    seenStar[v] = true;
    starVisited++;
    if (depth > maxDepth) maxDepth = depth;
    foreach (int n in star.Neighbors(v))
        if (!seenStar[n]) DfsRecursive(n, depth + 1);
}

DfsRecursive(0, 0);
sw.Stop();
Console.WriteLine($"  递归 DFS 的最大深度 = {maxDepth}（栈上同时只有 {maxDepth + 1} 个栈帧）");
Console.WriteLine($"  耗时 {sw.Elapsed.TotalMilliseconds:F1} ms，访问 {starVisited:N0} 个顶点");
Console.WriteLine();

Console.WriteLine("  有意思的对比：");
Console.WriteLine($"    - BFS 的队列峰值 ≈ {LEAVES:N0}（最宽的一层）");
Console.WriteLine($"    - 递归 DFS 的栈深度 = {maxDepth}（最深的路径）");
Console.WriteLine();
Console.WriteLine("  【结论】两者的空间开销取决于图的【不同维度】：");
Console.WriteLine("    BFS 看【宽度】：峰值 = 最宽的一层有多少个顶点");
Console.WriteLine("    DFS 看【深度】：峰值 = 最长路径有多少个顶点");
Console.WriteLine();
Console.WriteLine("  所以：");
Console.WriteLine("    星形图（宽而浅）  -> BFS 队列大，DFS 栈小");
Console.WriteLine("    链式图（窄而深）  -> BFS 队列小，DFS 栈大（递归太深会栈溢出）");
Console.WriteLine();
Console.WriteLine("  【注意】上面的 DFS 结论针对【递归版】。迭代版如果用"
    + "「一次把所有邻居都压栈」的写法，栈峰值也可能很大 ——");
Console.WriteLine("  这也是 12.2 节练习 12.2.5 讨论的「重复压栈」带来的空间问题。");
Console.WriteLine();

// ==================== 图的实现 ====================

public class Graph
{
    private readonly Dictionary<int, List<int>> _adj = new();
    public int VertexCount { get; private set; } = 0;

    public void AddEdge(int a, int b)
    {
        if (!_adj.ContainsKey(a)) { _adj[a] = new List<int>(); VertexCount = Math.Max(VertexCount, a + 1); }
        if (!_adj.ContainsKey(b)) { _adj[b] = new List<int>(); VertexCount = Math.Max(VertexCount, b + 1); }
        _adj[a].Add(b);
        _adj[b].Add(a);
    }

    public List<int> Neighbors(int v) => _adj.TryGetValue(v, out var l) ? l : new List<int>();
}
