// ==================== 三个"在 DFS/BFS 上加一点东西"的问题 ====================

// ---------- 1. 连通分量 ----------
//
// 思路：遍历所有顶点，遇到没访问过的就启动一次 DFS —— 一次 DFS 能走到的所有顶点
//       就属于同一个连通分量。DFS 的次数 = 连通分量的个数。

static List<List<int>> FindComponents(Graph g)
{
    var visited = new bool[g.VertexCount];
    var components = new List<List<int>>();

    for (int v = 0; v < g.VertexCount; v++)
    {
        if (visited[v]) continue;              // 已经属于某个分量了

        var component = new List<int>();
        var stack = new Stack<int>();
        stack.Push(v);

        while (stack.Count > 0)
        {
            int cur = stack.Pop();
            if (visited[cur]) continue;
            visited[cur] = true;
            component.Add(cur);

            foreach (int next in g.Neighbors(cur))
                if (!visited[next]) stack.Push(next);
        }

        components.Add(component);
    }
    return components;
}

// ---------- 2. 无向图环检测 ----------
//
// 思路：DFS 时记住「我是从哪个顶点来的」（parent）。
//       如果遇到一个【已经访问过、且不是我的 parent】的邻居 -> 说明有环。

static bool HasCycleUndirected(Graph g)
{
    var visited = new bool[g.VertexCount];

    bool Dfs(int v, int parent)
    {
        visited[v] = true;
        foreach (int next in g.Neighbors(v))
        {
            if (!visited[next])
            {
                if (Dfs(next, v)) return true;
            }
            else if (next != parent)
            {
                return true;      // 遇到了「访问过且不是父节点」的邻居 -> 有环
            }
        }
        return false;
    }

    for (int v = 0; v < g.VertexCount; v++)
        if (!visited[v] && Dfs(v, -1)) return true;

    return false;
}

// ---------- 3. 有向图环检测（三色标记）----------
//
// 白色(0) = 没访问过；灰色(1) = 正在访问（在当前 DFS 路径上）；黑色(2) = 访问完了
// 遇到【灰色】节点 -> 说明绕回了当前路径上的某个点 -> 有环

static bool HasCycleDirected(DiGraph g)
{
    var color = new int[g.VertexCount];        // 默认全是 0（白色）

    bool Dfs(int v)
    {
        color[v] = 1;                          // 标记为灰色（进入）
        foreach (int next in g.Neighbors(v))
        {
            if (color[next] == 1) return true;          // 遇到灰色 -> 有环！
            if (color[next] == 0 && Dfs(next)) return true;
        }
        color[v] = 2;                          // 标记为黑色（离开）
        return false;
    }

    for (int v = 0; v < g.VertexCount; v++)
        if (color[v] == 0 && Dfs(v)) return true;

    return false;
}

// ---------- 4. 二分图判定（染色法）----------
//
// 二分图：能把顶点分成两组，使得【每条边的两端都在不同的组】。
// 等价于：能用两种颜色给所有顶点染色，且相邻顶点颜色不同。

static bool IsBipartite(Graph g, out int[] color)
{
    color = new int[g.VertexCount];
    Array.Fill(color, -1);                     // -1 = 还没染色

    for (int start = 0; start < g.VertexCount; start++)
    {
        if (color[start] != -1) continue;

        color[start] = 0;
        var queue = new Queue<int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            int v = queue.Dequeue();
            foreach (int next in g.Neighbors(v))
            {
                if (color[next] == -1)
                {
                    color[next] = 1 - color[v];       // 染成相反的颜色
                    queue.Enqueue(next);
                }
                else if (color[next] == color[v])
                {
                    return false;                     // 相邻同色 -> 不是二分图
                }
            }
        }
    }
    return true;
}

// ==================== 实验一：连通分量 ====================

Console.WriteLine("=== 实验一：连通分量 ===");
Console.WriteLine();

var g1 = new Graph(9);
foreach (var (a, b) in new[] { (0, 1), (1, 2), (0, 2), (3, 4), (6, 7), (7, 8) })
    g1.AddEdge(a, b);

Console.WriteLine("  图的构成（9 个顶点）：");
Console.WriteLine("    分量 1: 0 —— 1 —— 2（三角形）");
Console.WriteLine("    分量 2: 3 —— 4");
Console.WriteLine("    分量 3: 5（孤立顶点）");
Console.WriteLine("    分量 4: 6 —— 7 —— 8（一条链）");
Console.WriteLine();

var components = FindComponents(g1);
Console.WriteLine($"  找到了 {components.Count} 个连通分量：");
for (int i = 0; i < components.Count; i++)
    Console.WriteLine($"    分量 {i + 1}: {{{string.Join(", ", components[i].OrderBy(x => x))}}}");
Console.WriteLine();
Console.WriteLine("  算法要点：对每个没访问过的顶点启动一次 DFS，一次 DFS 覆盖的就是一个分量。");
Console.WriteLine("           次数 = 分量个数。这是 12.2 节 DFS 的直接应用。");
Console.WriteLine();

// ==================== 实验二：无向图环检测 ====================

Console.WriteLine("=== 实验二：无向图环检测 ===");
Console.WriteLine();

var g2 = new Graph(6);
foreach (var (a, b) in new[] { (0, 1), (1, 2), (2, 0), (2, 3), (3, 4) })
    g2.AddEdge(a, b);

Console.WriteLine("  情形 A（有三角形 0-1-2）：");
Console.WriteLine("        0 —— 1");
Console.WriteLine("         \\  /");
Console.WriteLine("          2 —— 3 —— 4");
Console.WriteLine($"    有环吗？{HasCycleUndirected(g2)}");
Console.WriteLine();

var g3 = new Graph(6);
foreach (var (a, b) in new[] { (0, 1), (1, 2), (2, 3), (3, 4) })
    g3.AddEdge(a, b);

Console.WriteLine("  情形 B（一条链，无环）：");
Console.WriteLine("        0 —— 1 —— 2 —— 3 —— 4");
Console.WriteLine($"    有环吗？{HasCycleUndirected(g3)}");
Console.WriteLine();

Console.WriteLine("  关键在「parent」参数：");
Console.WriteLine("    无向图里，从 v 走到 next，next 一定有一条边回到 v —— 那是【来路】，不是环；");
Console.WriteLine("    所以要跳过 parent，只有遇到「已访问且不是 parent」的邻居才算环。");
Console.WriteLine();

// 一个容易踩的坑：三角形
Console.WriteLine("  验证三角形（0-1-2-0）：");
Console.WriteLine("    从 0 出发，走到 1，再走到 2；");
Console.WriteLine("    在 2 的位置，邻居有 1（parent，跳过）和 0（已访问，不是 parent）-> 发现环 ✓");
Console.WriteLine();

// ==================== 实验三：有向图环检测 ====================

Console.WriteLine("=== 实验三：有向图环检测（三色标记）===");
Console.WriteLine();

// 有向无环图（DAG）：任务依赖
var dag = new DiGraph(5);
dag.AddEdge(0, 1);      // 0 -> 1
dag.AddEdge(1, 2);
dag.AddEdge(0, 2);
dag.AddEdge(2, 3);
dag.AddEdge(3, 4);

Console.WriteLine("  情形 A（任务依赖，无环）：");
Console.WriteLine("        0 -> 1 -> 2 -> 3 -> 4");
Console.WriteLine("         \\______/");
Console.WriteLine($"    有环吗？{HasCycleDirected(dag)}");
Console.WriteLine();

// 有环的有向图
var cyclic = new DiGraph(4);
cyclic.AddEdge(0, 1);
cyclic.AddEdge(1, 2);
cyclic.AddEdge(2, 3);
cyclic.AddEdge(3, 1);      // 3 -> 1，形成环 1->2->3->1

Console.WriteLine("  情形 B（有环）：");
Console.WriteLine("        0 -> 1 -> 2 -> 3");
Console.WriteLine("              ^_________|");
Console.WriteLine($"    有环吗？{HasCycleDirected(cyclic)}");
Console.WriteLine();

Console.WriteLine("  为什么无向图的方法不能直接用？");
Console.WriteLine("    无向图靠「parent」排除来路，但有向图里 A->B 不代表 B->A ——");
Console.WriteLine("    所以「遇到已访问的邻居」可能是【正常的交叉】，而不是环。");
Console.WriteLine();
Console.WriteLine("  三色标记的含义：");
Console.WriteLine("    白色(0)：还没访问过");
Console.WriteLine("    灰色(1)：正在访问 —— 在【当前 DFS 路径】上");
Console.WriteLine("    黑色(2)：访问完了 —— 它的所有后代都探索过了");
Console.WriteLine();
Console.WriteLine("    遇到【灰色】= 绕回了当前路径 -> 有环；");
Console.WriteLine("    遇到【黑色】= 走到了别的分支探索过的地方 -> 是交叉边，不是环。");
Console.WriteLine();

// ==================== 实验四：二分图判定 ====================

Console.WriteLine("=== 实验四：二分图判定 ===");
Console.WriteLine();

// 二分图：一条链（可以用两种颜色交替染）
var bip = new Graph(4);
foreach (var (a, b) in new[] { (0, 1), (1, 2), (2, 3) })
    bip.AddEdge(a, b);

bool r1 = IsBipartite(bip, out var color1);
Console.WriteLine("  情形 A（一条链 0-1-2-3）：");
Console.WriteLine($"    是二分图吗？{r1}");
Console.WriteLine($"    染色结果: {string.Join(", ", color1.Select((c, i) => $"{i}:{(c == 0 ? "红" : "蓝")}"))}");
Console.WriteLine($"    两组: 红组 = {{{string.Join(",", Enumerable.Range(0, 4).Where(i => color1[i] == 0))}}}, "
    + $"蓝组 = {{{string.Join(",", Enumerable.Range(0, 4).Where(i => color1[i] == 1))}}}");
Console.WriteLine();

// 非二分图：三角形（奇数长度的环）
var tri = new Graph(3);
foreach (var (a, b) in new[] { (0, 1), (1, 2), (2, 0) })
    tri.AddEdge(a, b);

bool r2 = IsBipartite(tri, out _);
Console.WriteLine("  情形 B（三角形 0-1-2-0）：");
Console.WriteLine($"    是二分图吗？{r2}");
Console.WriteLine("    为什么不是？3 个顶点两两相连，无论怎么分两组，总有两个在同一组 ——");
Console.WriteLine("    而它们之间有边，违反「同组内不能有边」。");
Console.WriteLine();

// 更大的例子：正方形（偶数环）是二分图
var square = new Graph(4);
foreach (var (a, b) in new[] { (0, 1), (1, 2), (2, 3), (3, 0) })
    square.AddEdge(a, b);

bool r3 = IsBipartite(square, out var color3);
Console.WriteLine("  情形 C（正方形 0-1-2-3-0，偶数环）：");
Console.WriteLine($"    是二分图吗？{r3}");
Console.WriteLine($"    两组: {{{string.Join(",", Enumerable.Range(0, 4).Where(i => color3[i] == 0))}}} "
    + $"和 {{{string.Join(",", Enumerable.Range(0, 4).Where(i => color3[i] == 1))}}}");
Console.WriteLine();

Console.WriteLine("  【规律】一个图是二分图 <=> 它【不含奇数长度的环】。");
Console.WriteLine("    三角形（3 个点）不是二分图；正方形（4 个点）是；五边形（5 个点）不是……");
Console.WriteLine();

// ==================== 实验五：三个问题的统一视角 ====================

Console.WriteLine("=== 实验五：三个问题的统一视角 ===");
Console.WriteLine();
Console.WriteLine($"  {"问题",-18} | {"基础算法",-14} | {"额外加什么",-34}");
Console.WriteLine("  " + new string('-', 74));
Console.WriteLine($"  {"连通分量",-20} | {"DFS",-16} | {"外层循环遍历所有顶点",-36}");
Console.WriteLine($"  {"无向图环检测",-17} | {"DFS",-16} | {"传递 parent 参数",-36}");
Console.WriteLine($"  {"有向图环检测",-17} | {"DFS",-16} | {"三色标记（区分灰色/黑色）",-36}");
Console.WriteLine($"  {"二分图判定",-19} | {"BFS",-16} | {"给顶点染色，检查相邻是否同色",-36}");
Console.WriteLine();
Console.WriteLine("  【共同点】都是在 DFS/BFS 的骨架上加一点点东西：");
Console.WriteLine("    要么多传一个参数（parent），要么多维护一个数组（color）。");
Console.WriteLine();
Console.WriteLine("  这就是图算法的特点：");
Console.WriteLine("    【遍历是骨架，各种问题只是往骨架上挂信息】。");
Console.WriteLine();

// ==================== 图的实现 ====================

public class Graph
{
    private readonly List<int>[] _adj;

    public Graph(int n)
    {
        _adj = new List<int>[n];
        for (int i = 0; i < n; i++) _adj[i] = new List<int>();
    }

    public int VertexCount => _adj.Length;

    public void AddEdge(int a, int b)
    {
        _adj[a].Add(b);
        _adj[b].Add(a);              // 无向图：双向
    }

    public List<int> Neighbors(int v) => _adj[v];
}

/// <summary>有向图：只加一个方向的边。</summary>
public class DiGraph
{
    private readonly List<int>[] _adj;

    public DiGraph(int n)
    {
        _adj = new List<int>[n];
        for (int i = 0; i < n; i++) _adj[i] = new List<int>();
    }

    public int VertexCount => _adj.Length;

    public void AddEdge(int from, int to) => _adj[from].Add(to);   // 只加一个方向

    public List<int> Neighbors(int v) => _adj[v];
}
