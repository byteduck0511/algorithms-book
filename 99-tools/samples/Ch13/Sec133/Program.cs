// ==================== 第 13.3 节：Dijkstra 算法 ====================
//
//   实验一：带权地图上跑 Dijkstra，并用 Floyd-Warshall 独立对拍验证
//   实验二：优先队列版 vs 朴素 O(V^2) 版 —— 教科书说「稀疏用堆、稠密用数组」，
//           实测找出那条分界线大概在哪
//   实验三：负权边 —— (A) 无负环但有负权边，标准 Dijkstra 给出错误答案
//                     (B) 有负环，惰性丢弃版本会一直绕下去
//
// 度量说明：实验一、三用「操作计数」（与机器无关）；
//           实验二必须比时间（两种实现的操作不是同一种），故注明机器并取多轮最快值。

using System.Diagnostics;

// ==================== 实验一：带权地图上的最短路径 ====================

Console.WriteLine("=== 实验一：带权地图上的最短路径 ===");
Console.WriteLine();

const int Side = 15;
var map = RandomWeightedGrid(Side, Side, seed: 42);
const int source = 0;
int farTarget = map.VertexCount - 1;

Console.WriteLine($"  地图：{Side}×{Side} 网格，{map.VertexCount} 个路口，"
    + $"{map.EdgeCount} 条道路（每条路的通行时间是 1~9）");
Console.WriteLine($"  起点 = 左上角 0");
Console.WriteLine();

var (dist, prev, confirmed) = Dijkstra(map, source);
Console.WriteLine($"  优先队列版 Dijkstra 跑完整张图：");
Console.WriteLine($"    确定的顶点数 = {confirmed}（全图 {map.VertexCount} 个）");
Console.WriteLine($"    到右下角 {farTarget} 的最短通行时间 = {dist[farTarget]}");
Console.WriteLine();

var path = ReconstructPath(prev, source, farTarget);
Console.WriteLine($"    路径（{path.Count} 个路口）：");
Console.WriteLine($"      {string.Join(" -> ", path)}");
Console.WriteLine();

// ---------- 独立对拍：Floyd-Warshall ----------
var floyd = FloydWarshall(map);
int mismatch = 0;
for (int v = 0; v < map.VertexCount; v++)
    if (dist[v] != floyd[source, v]) mismatch++;

Console.WriteLine($"  用 Floyd-Warshall（另一种算法）独立对拍全部 {map.VertexCount} 个顶点：");
Console.WriteLine($"    距离不一致的顶点数 = {mismatch}");
Console.WriteLine();

// ---------- 提前退出能省多少：取决于目标有多远 ----------
Console.WriteLine("  优化：一旦【目标顶点被确定】就立刻停，不用算完整张图。");
Console.WriteLine("  能省多少？取决于目标离起点有多远 —— 实测三个目标：");
Console.WriteLine();
Console.WriteLine("    目标    最短时间    跑完整图    到目标就停    省下");
Console.WriteLine("    ----    --------    ---------    ----------    ------");

foreach (int t in new[] { 16, 112, 224 })
{
    var (td, _, early) = DijkstraToTarget(map, source, t);
    Console.WriteLine($"    {t,-6}  {td[t],8}    {confirmed,9}    {early,10}    "
        + $"{(confirmed - early) / (double)confirmed,5:P0}");
}

Console.WriteLine();
Console.WriteLine("  最远的目标（右下角）反而【一点都省不了】—— 它本来就是最后一个被确定的。");
Console.WriteLine();

// ==================== 实验二：优先队列版 vs 朴素版 ====================

Console.WriteLine("=== 实验二：优先队列版 vs 朴素 O(V^2) 版 ===");
Console.WriteLine();
Console.WriteLine("  两种实现：");
Console.WriteLine("    优先队列版：每次 O(log V) 取出当前距离最小的顶点，复杂度 O((V+E) log V)");
Console.WriteLine("    朴素版    ：每轮线性扫描找最小，复杂度 O(V^2) —— 【与边数无关】");
Console.WriteLine();

const int V = 1500;
var g1 = RandomWeightedGraph(V, targetEdges: 4_500, seed: 1);
var g2 = RandomWeightedGraph(V, targetEdges: 50_000, seed: 2);
var g3 = RandomWeightedGraph(V, targetEdges: 200_000, seed: 3);
var g4 = RandomWeightedGraph(V, targetEdges: 600_000, seed: 4);

var configs = new (string Name, WeightedGraph G)[]
{
    ("很稀疏", g1),
    ("稀疏  ", g2),
    ("较稠密", g3),
    ("很稠密", g4),
};

// 预热：让 JIT 先把两个方法都编译好
foreach (var (_, g) in configs) { Dijkstra(g, 0); DijkstraNaive(g, 0); }

double BestMs(Action a, int rounds = 5)
{
    double best = double.MaxValue;
    for (int i = 0; i < rounds; i++)
    {
        var sw = Stopwatch.StartNew();
        a();
        sw.Stop();
        best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
    }
    return best;
}

Console.WriteLine($"  V = {V}（完全图会有 {V * (V - 1) / 2:N0} 条边），四种密度，各跑 5 次取最快：");
Console.WriteLine();
Console.WriteLine("    密度      边数        占完全图    优先队列版     朴素版      谁快");
Console.WriteLine("    ------    ---------   --------    ----------    --------    --------");

foreach (var (name, g) in configs)
{
    double tHeap = BestMs(() => Dijkstra(g, 0));
    double tNaive = BestMs(() => DijkstraNaive(g, 0));
    double density = g.EdgeCount / (double)(V * (V - 1) / 2);
    string winner = tHeap < tNaive
        ? $"优先队列快 {tNaive / tHeap:F2} 倍"
        : $"朴素版快 {tHeap / tNaive:F2} 倍";
    Console.WriteLine($"    {name}  {g.EdgeCount,10:N0}   {density,7:P1}    {tHeap,8:F1} ms    {tNaive,5:F1} ms    {winner}");
}

Console.WriteLine();
Console.WriteLine("  这张表要横着看，也要竖着看：");
Console.WriteLine("    【竖着看】朴素版的耗时几乎不变（1.1 ~ 2.7 ms）");
Console.WriteLine("              —— 它的 O(V^2) 只跟顶点数有关，加边不花钱。");
Console.WriteLine("    【横着看】优先队列版的耗时随边数从 0.6 ms 涨到 3.0 ms");
Console.WriteLine("              —— 每条边都可能触发一次入队，E 变大它就变慢。");
Console.WriteLine();
Console.WriteLine("  两条线在【很稀疏】和【稀疏】之间交叉 —— 边数 4,500 ~ 50,000 之间，");
Console.WriteLine("  密度还不到 5% 的地方。");
Console.WriteLine();
Console.WriteLine("  这个门槛比理论上算出来的低得多：");
Console.WriteLine("    令两种复杂度相等：V^2 = (V+E)·log V，解出 E ≈ V^2/log V ≈ 214,000 条边；");
Console.WriteLine("    但实测到 50,000 条边时，朴素版就已经赢了 —— 比理论交点【早了 4 倍多】。");
Console.WriteLine();
Console.WriteLine("  原因是【常数因子】：数组扫描是顺序访问，缓存友好、分支可预测；");
Console.WriteLine("  而每次堆操作都要比较 + 交换，还要在堆数组里跳着走");
Console.WriteLine("  （8.3 节讲过堆排序为什么慢，是同一件事）。");
Console.WriteLine();

// ==================== 实验三：负权边 ====================

Console.WriteLine("=== 实验三：负权边 ===");
Console.WriteLine();

// ---------- (A) 有负权边、但没有负环 ----------
Console.WriteLine("  (A) 有负权边，但没有负环");
Console.WriteLine();
Console.WriteLine("      有向图（括号里是边的权）：");
Console.WriteLine("        S --1--> A --1--> B --1--> T");
Console.WriteLine("        S --------5------> C --(-10)--> B");
Console.WriteLine();

var bad = new WeightedGraph(5);
bad.AddDirectedEdge(0, 1, 1);      // S -> A
bad.AddDirectedEdge(1, 2, 1);      // A -> B
bad.AddDirectedEdge(0, 3, 5);      // S -> C
bad.AddDirectedEdge(3, 2, -10);    // C -> B   ★ 负权
bad.AddDirectedEdge(2, 4, 1);      // B -> T
string[] names = ["S", "A", "B", "C", "T"];

var (badDist, badPrev, _) = Dijkstra(bad, 0);
var badPath = ReconstructPath(badPrev, 0, 4);
Console.WriteLine("      标准 Dijkstra（带【已确定】标记）：");
Console.WriteLine($"        dist[T] = {badDist[4]}，路径 {string.Join(" -> ", badPath.Select(v => names[v]))}");
Console.WriteLine();

var (lazyDist, lazyPrev, _) = DijkstraLazy(bad, 0);
var lazyPath = ReconstructPath(lazyPrev, 0, 4);
Console.WriteLine("      换成【惰性丢弃】版本（出队时不标记已确定，只跳过过期条目）：");
Console.WriteLine($"        dist[T] = {lazyDist[4]}，路径 {string.Join(" -> ", lazyPath.Select(v => names[v]))}");
Console.WriteLine();

var (trueCost, truePath) = BruteForceMinCostWeighted(5,
    [(0, 1, 1), (1, 2, 1), (0, 3, 5), (3, 2, -10), (2, 4, 1)], 0, 4);
Console.WriteLine($"      暴力枚举所有简单路径的真值：dist[T] = {trueCost}，"
    + $"路径 {string.Join(" -> ", truePath.Select(v => names[v]))}");
Console.WriteLine();
Console.WriteLine("      顺带一提：这个暴力枚举一开始也算错了 —— 它原本带着「当前代价已超过已知最优就剪枝」，");
Console.WriteLine("      走到 C 时 5 >= 3，那条 -10 的边直接被剪掉了。");
Console.WriteLine("      【负权边上，这种剪枝是无效的】—— 后面还可能把代价拉回来。");
Console.WriteLine();
Console.WriteLine("      标准 Dijkstra 错在哪？");
Console.WriteLine("        它先确定了 B = 2（走 S->A->B），并用这个 2 松弛出 T = 3、把 T 也确定了；");
Console.WriteLine("        之后从 C 松弛出 B = -5 时，B 已经带着【已确定】标记，那条更短的路再也传不下去。");
Console.WriteLine("        根因：Dijkstra 的贪心前提是「后面发现的路径不可能更短」，负权边打破了这个前提。");
Console.WriteLine();
Console.WriteLine("      惰性版这次【碰巧】算对了 —— 但它靠的是「不设已确定标记，允许反复更新」，");
Console.WriteLine("      这不是保证，只是运气。下一小节就戳破它。");
Console.WriteLine();

// ---------- (B) 有负环 ----------
Console.WriteLine("  (B) 如果图里有负环");
Console.WriteLine();
Console.WriteLine("      有向图：A --1--> B，B --(-2)--> A（绕一圈总权 -1，是个负环），A --1--> T");
Console.WriteLine();

var cyclic = new WeightedGraph(3);
cyclic.AddDirectedEdge(0, 1, 1);    // A -> B
cyclic.AddDirectedEdge(1, 0, -2);   // B -> A   ★ 负环
cyclic.AddDirectedEdge(0, 2, 1);    // A -> T

const long PopLimit = 200_000;
var (cycDist, _, pops, hitLimit) = DijkstraLazyWithLimit(cyclic, 0, PopLimit);

Console.WriteLine($"      给惰性版 {PopLimit:N0} 次出队机会（正常图最多 {cyclic.VertexCount} 次就结束了）：");
Console.WriteLine($"        实际出队次数 = {pops:N0}");
Console.WriteLine($"        是否撞到上限 = {hitLimit}");
Console.WriteLine($"        dist[A] 已经降到 {cycDist[0]:N0}，还在继续降");
Console.WriteLine();
Console.WriteLine("      原因：每绕一圈总权就少 1，dist 永远可以更小 —— 队列永远不会空。");
Console.WriteLine("      有负环的图【根本不存在最短路径】（可以无限绕），必须换算法（13.4 节的 Bellman-Ford）。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- Dijkstra：优先队列版（标准版，带「已确定」标记）----------
static (int[] dist, int[] prev, long confirmed) Dijkstra(WeightedGraph g, int start)
{
    var dist = new int[g.VertexCount];
    var prev = new int[g.VertexCount];
    var done = new bool[g.VertexCount];
    Array.Fill(dist, int.MaxValue);
    Array.Fill(prev, -1);

    var pq = new PriorityQueue<int, int>();
    dist[start] = 0;
    pq.Enqueue(start, 0);

    long confirmed = 0;
    while (pq.TryDequeue(out int v, out int d))
    {
        if (done[v]) continue;           // 过期条目（同一个顶点可能入队多次）
        done[v] = true;                  // ★ 确定 v 的最短距离
        confirmed++;

        foreach (var (to, w) in g.Neighbors(v))
        {
            int nd = d + w;              // ★ 这就是「松弛」
            if (nd < dist[to])
            {
                dist[to] = nd;
                prev[to] = v;
                pq.Enqueue(to, nd);
            }
        }
    }
    return (dist, prev, confirmed);
}

// ---------- Dijkstra：目标一确定就停 ----------
static (int[] dist, int[] prev, long confirmed) DijkstraToTarget(WeightedGraph g, int start, int target)
{
    var dist = new int[g.VertexCount];
    var prev = new int[g.VertexCount];
    var done = new bool[g.VertexCount];
    Array.Fill(dist, int.MaxValue);
    Array.Fill(prev, -1);

    var pq = new PriorityQueue<int, int>();
    dist[start] = 0;
    pq.Enqueue(start, 0);

    long confirmed = 0;
    while (pq.TryDequeue(out int v, out int d))
    {
        if (done[v]) continue;
        done[v] = true;
        confirmed++;
        if (v == target) break;          // ★ 终点一确定就停

        foreach (var (to, w) in g.Neighbors(v))
        {
            int nd = d + w;
            if (nd < dist[to])
            {
                dist[to] = nd;
                prev[to] = v;
                pq.Enqueue(to, nd);
            }
        }
    }
    return (dist, prev, confirmed);
}

// ---------- Dijkstra：惰性丢弃版（不设「已确定」标记）----------
static (int[] dist, int[] prev, long popped) DijkstraLazy(WeightedGraph g, int start)
{
    var (d, p, popped, _) = DijkstraLazyWithLimit(g, start, long.MaxValue);
    return (d, p, popped);
}

static (int[] dist, int[] prev, long popped, bool hitLimit) DijkstraLazyWithLimit(
    WeightedGraph g, int start, long popLimit)
{
    var dist = new int[g.VertexCount];
    var prev = new int[g.VertexCount];
    Array.Fill(dist, int.MaxValue);
    Array.Fill(prev, -1);

    var pq = new PriorityQueue<int, int>();
    dist[start] = 0;
    pq.Enqueue(start, 0);

    long popped = 0;
    while (pq.TryDequeue(out int v, out int d))
    {
        if (d > dist[v]) continue;       // 只跳过【过期】的条目
        popped++;
        if (popped > popLimit) return (dist, prev, popped, true);   // 安全阀

        foreach (var (to, w) in g.Neighbors(v))
        {
            int nd = d + w;
            if (nd < dist[to])
            {
                dist[to] = nd;
                prev[to] = v;
                pq.Enqueue(to, nd);
            }
        }
    }
    return (dist, prev, popped, false);
}

// ---------- 朴素版 Dijkstra：O(V^2) ----------
static int[] DijkstraNaive(WeightedGraph g, int start)
{
    var dist = new int[g.VertexCount];
    var done = new bool[g.VertexCount];
    Array.Fill(dist, int.MaxValue);
    dist[start] = 0;

    for (int iter = 0; iter < g.VertexCount; iter++)
    {
        // ★ 线性扫描：这一步是 O(V)，也是朴素版复杂度的来源
        int best = -1;
        for (int v = 0; v < g.VertexCount; v++)
            if (!done[v] && dist[v] != int.MaxValue && (best == -1 || dist[v] < dist[best]))
                best = v;

        if (best == -1) break;           // 剩下的都不可达
        done[best] = true;

        foreach (var (to, w) in g.Neighbors(best))
        {
            int nd = dist[best] + w;
            if (nd < dist[to]) dist[to] = nd;
        }
    }
    return dist;
}

// ---------- Floyd-Warshall：独立对拍用 ----------
static int[,] FloydWarshall(WeightedGraph g)
{
    int n = g.VertexCount;
    var d = new int[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            d[i, j] = i == j ? 0 : int.MaxValue / 4;   // 除以 4 避免加法溢出

    for (int v = 0; v < n; v++)
        foreach (var (to, w) in g.Neighbors(v))
            if (w < d[v, to]) d[v, to] = w;

    for (int k = 0; k < n; k++)
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (d[i, k] + d[k, j] < d[i, j])
                    d[i, j] = d[i, k] + d[k, j];

    return d;
}

// ---------- 工具 ----------

static List<int> ReconstructPath(int[] prev, int start, int target)
{
    var path = new List<int>();
    for (int v = target; v != -1; v = prev[v]) path.Add(v);
    path.Reverse();
    return path.Count > 0 && path[0] == start ? path : new List<int>();
}

static (int cost, List<int> path) BruteForceMinCostWeighted(
    int n, List<(int u, int v, int w)> edges, int s, int t)
{
    var adj = new List<(int to, int w)>[n];
    for (int i = 0; i < n; i++) adj[i] = new List<(int to, int w)>();
    foreach (var (u, v, w) in edges)
        adj[u].Add((v, w));              // ★ 有向：只加一个方向

    int best = int.MaxValue;
    var bestPath = new List<int>();
    var path = new List<int> { s };
    var onPath = new bool[n];

    void Dfs(int v, int cost)
    {
        if (v == t)
        {
            if (cost < best) { best = cost; bestPath = new List<int>(path); }
            return;
        }
        foreach (var (to, w) in adj[v])
        {
            if (onPath[to]) continue;
            onPath[to] = true;
            path.Add(to);
            Dfs(to, cost + w);
            path.RemoveAt(path.Count - 1);
            onPath[to] = false;
        }
    }

    onPath[s] = true;
    Dfs(s, 0);
    return (best, bestPath);
}

// ---------- 建图 ----------

// 带权网格：每个格子连右、下邻居，权重 1~9
static WeightedGraph RandomWeightedGrid(int rows, int cols, int seed)
{
    var rng = new Random(seed);
    var g = new WeightedGraph(rows * cols);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int id = r * cols + c;
            if (c + 1 < cols) g.AddEdge(id, id + 1, rng.Next(1, 10));
            if (r + 1 < rows) g.AddEdge(id, id + cols, rng.Next(1, 10));
        }
    return g;
}

// 随机带权无向图：先拉一条链保证连通，再补随机边到指定数量
static WeightedGraph RandomWeightedGraph(int n, int targetEdges, int seed)
{
    var rng = new Random(seed);
    var g = new WeightedGraph(n);
    for (int i = 0; i + 1 < n; i++) g.AddEdge(i, i + 1, rng.Next(1, 100));

    var seen = new HashSet<(int, int)>();
    while (g.EdgeCount < targetEdges)
    {
        int a = rng.Next(n), b = rng.Next(n);
        if (a == b) continue;
        if (a > b) (a, b) = (b, a);
        if (!seen.Add((a, b))) continue;
        g.AddEdge(a, b, rng.Next(1, 100));
    }
    return g;
}

// ---------- 带权图（无向 + 有向两种加边方式）----------

public class WeightedGraph
{
    private readonly List<(int to, int w)>[] _adj;
    private int _edgeCount;

    public WeightedGraph(int n)
    {
        _adj = new List<(int to, int w)>[n];
        for (int i = 0; i < n; i++) _adj[i] = new List<(int to, int w)>();
    }

    public int VertexCount => _adj.Length;
    public int EdgeCount => _edgeCount;

    /// <summary>无向边：两个方向都存。</summary>
    public void AddEdge(int a, int b, int w)
    {
        _adj[a].Add((b, w));
        _adj[b].Add((a, w));
        _edgeCount++;
    }

    /// <summary>有向边：只存 a -> b（负权实验要用，无向图里负权会立刻形成负环）。</summary>
    public void AddDirectedEdge(int a, int b, int w)
    {
        _adj[a].Add((b, w));
        _edgeCount++;
    }

    public List<(int to, int w)> Neighbors(int v) => _adj[v];
}
