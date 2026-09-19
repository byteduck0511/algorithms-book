// ==================== 图与 DFS ====================

var graph = new Graph();
foreach (var (a, b) in new[]
         {
             (0, 1), (0, 2), (0, 3),
             (1, 3),
             (2, 3), (2, 4),
         })
    graph.AddEdge(a, b);

Console.WriteLine("=== 要遍历的图 ===");
Console.WriteLine();
Console.WriteLine("        0 —— 1");
Console.WriteLine("        |  \\  |");
Console.WriteLine("        |   \\ |");
Console.WriteLine("        2 —— 3");
Console.WriteLine("        |");
Console.WriteLine("        4");
Console.WriteLine();

Console.Write("  邻接表: ");
for (int i = 0; i < graph.VertexCount; i++)
    Console.Write($"{i}->[{string.Join(",", graph.Neighbors(i))}]  ");
Console.WriteLine();
Console.WriteLine();

// ==================== 实验一：递归 DFS ====================

Console.WriteLine("=== 实验一：递归 DFS —— 一条路走到底 ===");
Console.WriteLine();

var visitedOrder = new List<int>();
var visited = new bool[graph.VertexCount];

void DfsRecursive(int v, int depth)
{
    visited[v] = true;
    visitedOrder.Add(v);
    Console.WriteLine($"    {"".PadLeft(depth * 2)}访问 {v}");

    foreach (int next in graph.Neighbors(v))
    {
        if (!visited[next])
        {
            Console.WriteLine($"    {"".PadLeft(depth * 2)}  从 {v} 走向 {next}");
            DfsRecursive(next, depth + 1);
        }
    }
}

DfsRecursive(0, 0);
Console.WriteLine();
Console.WriteLine($"  访问顺序: {string.Join(" -> ", visitedOrder)}");
Console.WriteLine();

// ==================== 实验二：迭代 DFS ====================

Console.WriteLine("=== 实验二：迭代 DFS（用显式栈）===");
Console.WriteLine();

List<int> DfsIterative(int start)
{
    var result = new List<int>();
    var seen = new bool[graph.VertexCount];
    var stack = new Stack<int>();

    stack.Push(start);

    while (stack.Count > 0)
    {
        int v = stack.Pop();
        if (seen[v]) continue;          // 可能被重复压栈，跳过已访问的
        seen[v] = true;
        result.Add(v);

        // 关键：为了和递归版顺序【尽量一致】，要【逆序】压栈
        //（因为栈是后进先出，逆序压入才能顺序弹出）
        var neighbors = graph.Neighbors(v);
        for (int i = neighbors.Count - 1; i >= 0; i--)
        {
            if (!seen[neighbors[i]]) stack.Push(neighbors[i]);
        }
    }
    return result;
}

var iterOrder = DfsIterative(0);
Console.WriteLine($"  访问顺序: {string.Join(" -> ", iterOrder)}");
Console.WriteLine();
Console.WriteLine($"  和递归版一致: {iterOrder.SequenceEqual(visitedOrder)}");
Console.WriteLine();

Console.WriteLine("  注意代码里的【逆序压栈】：");
Console.WriteLine("    栈是后进先出，如果按 0,1,2,3 的顺序压栈，弹出顺序会是 3,2,1,0 —— 反了；");
Console.WriteLine("    所以要先逆序遍历邻居，再压栈，才能得到和递归版一样的顺序。");
Console.WriteLine();

// 演示"不逆序"会怎样
List<int> DfsIterativeNaive(int start)
{
    var result = new List<int>();
    var seen = new bool[graph.VertexCount];
    var stack = new Stack<int>();
    stack.Push(start);

    while (stack.Count > 0)
    {
        int v = stack.Pop();
        if (seen[v]) continue;
        seen[v] = true;
        result.Add(v);
        foreach (int next in graph.Neighbors(v))
            if (!seen[next]) stack.Push(next);      // 顺序压栈
    }
    return result;
}

var naiveOrder = DfsIterativeNaive(0);
Console.WriteLine($"  如果不逆序压栈: {string.Join(" -> ", naiveOrder)}");
Console.WriteLine($"  和递归版一致: {naiveOrder.SequenceEqual(visitedOrder)}");
Console.WriteLine();
Console.WriteLine("  两者【都是合法的 DFS】—— 访问顺序不同，但都满足「一条路走到底」的语义。");
Console.WriteLine("  这说明：图的 DFS 顺序【不唯一】，取决于你以什么顺序访问邻居。");
Console.WriteLine();

// ==================== 实验三：DFS 的经典应用 —— 找路径 ====================

Console.WriteLine("=== 实验三：用 DFS 找两点之间的路径 ===");
Console.WriteLine();

List<int>? FindPath(int from, int to)
{
    var path = new List<int>();
    var seen = new bool[graph.VertexCount];

    bool Dfs(int v)
    {
        seen[v] = true;
        path.Add(v);

        if (v == to) return true;

        foreach (int next in graph.Neighbors(v))
        {
            if (!seen[next] && Dfs(next)) return true;
        }

        path.RemoveAt(path.Count - 1);      // 回溯：这条路走不通，退回来
        return false;
    }

    return Dfs(from) ? path : null;
}

foreach (var (from, to) in new[] { (0, 4), (1, 4), (3, 2) })
{
    var p = FindPath(from, to);
    Console.WriteLine($"  {from} 到 {to} 的路径: {(p == null ? "不存在" : string.Join(" -> ", p))}");
}
Console.WriteLine();

Console.WriteLine("  注意最后那句 path.RemoveAt —— 那就是【回溯】（9.4 节讲过）。");
Console.WriteLine("  DFS 找路径的本质是「一条路试到底，不行就退回来换一条」。");
Console.WriteLine();

// ==================== 实验四：DFS 能回答的问题 ====================

Console.WriteLine("=== 实验四：DFS 能回答哪些问题 ===");
Console.WriteLine();

// 连通性
List<int> Reachable(int start)
{
    var seen = new bool[graph.VertexCount];
    var stack = new Stack<int>();
    stack.Push(start);
    var result = new List<int>();
    while (stack.Count > 0)
    {
        int v = stack.Pop();
        if (seen[v]) continue;
        seen[v] = true;
        result.Add(v);
        foreach (int n in graph.Neighbors(v)) if (!seen[n]) stack.Push(n);
    }
    return result;
}

var reach = Reachable(0);
Console.WriteLine($"  从 0 出发能到达: {string.Join(", ", reach.OrderBy(x => x))}");
Console.WriteLine($"  能否到达 4？{reach.Contains(4)}");
Console.WriteLine($"  能否到达 99？（不存在的顶点）{reach.Contains(99)}");
Console.WriteLine();

Console.WriteLine("  DFS 能回答的问题：");
Console.WriteLine("    - 两点之间是否连通（能不能走到）");
Console.WriteLine("    - 找出一条路径（但不保证最短）");
Console.WriteLine($"    - 从某点出发能到达哪些点（实测：{reach.Count} 个）");
Console.WriteLine("    - 图里有几个连通分量（12.4 节）");
Console.WriteLine("    - 图里有没有环（12.4 节）");
Console.WriteLine();

// ==================== 实验五：复杂度分析 ====================

const int N = 100_000;
Console.WriteLine($"=== 实验五：DFS 的复杂度（{N:N0} 个顶点的链式图）===");
Console.WriteLine();

var bigGraph = new Graph();
for (int i = 0; i + 1 < N; i++) bigGraph.AddEdge(i, i + 1);

var sw = System.Diagnostics.Stopwatch.StartNew();
var visitedCount = 0;
var seenBig = new bool[N];
var stackBig = new Stack<int>();
stackBig.Push(0);
while (stackBig.Count > 0)
{
    int v = stackBig.Pop();
    if (seenBig[v]) continue;
    seenBig[v] = true;
    visitedCount++;
    foreach (int n in bigGraph.Neighbors(v)) if (!seenBig[n]) stackBig.Push(n);
}
sw.Stop();

Console.WriteLine($"  访问了 {visitedCount:N0} 个顶点，耗时 {sw.Elapsed.TotalMilliseconds:F1} ms");
Console.WriteLine();
Console.WriteLine("  DFS 的复杂度是 O(V + E)：");
Console.WriteLine("    每个顶点最多被【访问一次】—— 因为有 visited 标记；");
Console.WriteLine("    每条边最多被【检查两次】（无向图，从两端各看一次）。");
Console.WriteLine();
Console.WriteLine("  【visited 标记是 DFS 的灵魂】：");
Console.WriteLine("    没有它，在有环的图里会【无限循环】—— 因为你会不停地绕圈。");
Console.WriteLine("    树不需要 visited（因为没有环），但图【必须有】。");
Console.WriteLine();

// ==================== 图的实现 ====================

public class Graph
{
    private readonly Dictionary<int, List<int>> _adj = new();

    public int VertexCount { get; private set; }

    public void AddEdge(int a, int b)
    {
        if (!_adj.ContainsKey(a)) { _adj[a] = new List<int>(); VertexCount = Math.Max(VertexCount, a + 1); }
        if (!_adj.ContainsKey(b)) { _adj[b] = new List<int>(); VertexCount = Math.Max(VertexCount, b + 1); }

        _adj[a].Add(b);
        _adj[b].Add(a);                  // 无向图：双向加
    }

    public List<int> Neighbors(int v) => _adj.TryGetValue(v, out var list) ? list : new List<int>();
}
