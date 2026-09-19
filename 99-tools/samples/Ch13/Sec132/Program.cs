// ==================== 第 13.2 节：无权最短路 —— BFS 的正确用法 ====================
//
// 12.3 节已经讲过 BFS 求最短路的原理。本节回答三个「接下来怎么办」的问题：
//
//   实验一：有多个起点怎么办？—— 多源 BFS（一次遍历 vs 每个起点各跑一次）
//   实验二：双向 BFS 到底快多少？—— 12.3 节只算了理论值，这里在两种真实图上实测
//   实验三：BFS 什么时候失效？—— 带权图的反例，以及边权只有 0/1 时的补救
//
// 全节的性能度量一律用【访问顶点数】而不是毫秒 —— 与机器无关，任何电脑跑出来都一样。

// ==================== 实验一：多源 BFS ====================

Console.WriteLine("=== 实验一：多源 BFS —— 每个小区到【最近配送站】的距离 ===");
Console.WriteLine();

const int GridSide = 300;
var city = GridGraph(GridSide, GridSide);
int cityVertices = city.VertexCount;

// 10 个配送站，均匀撒在城市里（编号 = 行 × 300 + 列，所以都是 300 的倍数）
var stations = new[] { 0, 3030, 6060, 9090, 12120, 15150, 18180, 21210, 24240, 27270 };

Console.WriteLine($"  城市：{GridSide}×{GridSide} 的网格，共 {cityVertices:N0} 个小区");
Console.WriteLine($"  配送站：{stations.Length} 个");
Console.WriteLine();

var (multiDist, multiVisited) = MultiSourceBfs(city, stations);
Console.WriteLine("  多源 BFS（所有配送站一起入队，只跑 1 次）：");
Console.WriteLine($"    访问顶点数 = {multiVisited:N0}");
Console.WriteLine();

// 对照：每个配送站各跑一次单源 BFS，再逐点取最小值
long singleTotal = 0;
var singleDist = new int[cityVertices];
Array.Fill(singleDist, int.MaxValue);
foreach (int s in stations)
{
    var (d, _, visited) = BfsWithPrev(city, s);
    singleTotal += visited;
    for (int v = 0; v < cityVertices; v++)
        if (d[v] >= 0 && d[v] < singleDist[v]) singleDist[v] = d[v];
}

Console.WriteLine($"  对照：{stations.Length} 个配送站各跑一次单源 BFS，再逐点取最小：");
Console.WriteLine($"    访问顶点数 = {singleTotal:N0}（{stations.Length} × {cityVertices:N0}）");
Console.WriteLine();

bool same = true;
for (int v = 0; v < cityVertices; v++)
    if (multiDist[v] != singleDist[v]) { same = false; break; }

Console.WriteLine($"  两种做法算出的距离数组完全一致：{same}");
Console.WriteLine($"  访问顶点数之比：{singleTotal / (double)multiVisited:F1} 倍");
Console.WriteLine();
Console.WriteLine("  多源 BFS 只多了一行代码：初始化时把所有源点一起入队（距离都是 0）。");
Console.WriteLine("  一次遍历就同时算出「到最近源点的距离」—— 因为所有源点相当于同时向外扩散，");
Console.WriteLine("  波前相遇的地方就是两个配送站的分界线。");
Console.WriteLine();

// ==================== 实验二：双向 BFS 到底快多少？ ====================

Console.WriteLine("=== 实验二：双向 BFS 到底快多少？ ===");
Console.WriteLine();
Console.WriteLine("  12.3 节练习 12.3.5 推导过：单向 O(b^d)，双向 O(2·b^(d/2))，");
Console.WriteLine("  对 b=200、d=6 算出来是 400 万倍。下面在两种形状的图上实测。");
Console.WriteLine();

// ---------- 图 A：二维网格 ----------
Console.WriteLine("  ---------- 图 A：二维网格（500×500）----------");
var plane = GridGraph(500, 500);
int fromA = 250 * 500 + 250;            // 城市正中间
int toA = 250 * 500 + 450;              // 往右走 200 格
var (uniDistA, uniVisitedA) = BfsToTarget(plane, fromA, toA);
var (biDistA, biVisitedA) = BiBfs(plane, fromA, toA);
Console.WriteLine($"    起点 {fromA} → 终点 {toA}，真实距离 {uniDistA}");
Console.WriteLine($"      单向 BFS：访问 {uniVisitedA,10:N0} 个顶点");
Console.WriteLine($"      双向 BFS：访问 {biVisitedA,10:N0} 个顶点");
Console.WriteLine($"      提升 {uniVisitedA / (double)biVisitedA:F2} 倍");
Console.WriteLine($"      两者算出的距离一致：{uniDistA == biDistA}");
Console.WriteLine();

// ---------- 图 B：3 叉树 ----------
Console.WriteLine("  ---------- 图 B：完全 3 叉树（深度 12）----------");
const int Branching = 3, Depth = 12;
var tree = BalancedTree(Branching, Depth);
// 最后一层的【第一个】叶子：编号 = (3^12 - 1) / 2
int leaf = (int)((Math.Pow(Branching, Depth) - 1) / (Branching - 1));
var (uniDistB, uniVisitedB) = BfsToTarget(tree, 0, leaf);
var (biDistB, biVisitedB) = BiBfs(tree, 0, leaf);
Console.WriteLine($"    顶点数 {tree.VertexCount:N0}（每个非叶节点有 {Branching} 个孩子）");
Console.WriteLine($"    起点 = 根 0 → 终点 = 最后一层的第一个叶子 {leaf}，真实距离 {uniDistB}");
Console.WriteLine($"      单向 BFS：访问 {uniVisitedB,10:N0} 个顶点");
Console.WriteLine($"      双向 BFS：访问 {biVisitedB,10:N0} 个顶点");
Console.WriteLine($"      提升 {uniVisitedB / (double)biVisitedB:F1} 倍");
Console.WriteLine($"      两者算出的距离一致：{uniDistB == biDistB}");
Console.WriteLine();

Console.WriteLine("  【反预期】同一个双向 BFS，在图 A 上只快约 2 倍，在图 B 上快了几百倍。");
Console.WriteLine("  12.3 节那个 b^(d/2) 的推导，前提是【图像树一样指数分支】；");
Console.WriteLine("  二维网格每一层只是沿周长增长（第 d 层约 4d 个顶点，不是 b^d），");
Console.WriteLine("  双向只能省掉「一个大菱形」和「两个小菱形」之间的差 —— 约 2 倍。");
Console.WriteLine();

// ==================== 实验三：BFS 什么时候失效？ ====================

Console.WriteLine("=== 实验三：BFS 什么时候失效？ ===");
Console.WriteLine();

// ---------- (A) 带权图：BFS 给出「步数最少、代价最大」的路径 ----------
Console.WriteLine("  (A) 带权图的反例");
Console.WriteLine();
Console.WriteLine("      图（括号里是边的代价）：");
Console.WriteLine("        S --100-- A --100-- T              2 步，总代价 200");
Console.WriteLine("        S ---1--- B ---1--- C ---1--- T    3 步，总代价 3");
Console.WriteLine();

var weighted = new List<(int u, int v, int w)>
{
    (0, 1, 100),   // S - A
    (1, 4, 100),   // A - T
    (0, 2, 1),     // S - B
    (2, 3, 1),     // B - C
    (3, 4, 1),     // C - T
};
string[] wname = ["S", "A", "B", "C", "T"];

var wg = new Graph(5);
foreach (var (u, v, _) in weighted) wg.AddEdge(u, v);

var (wDist, wPrev, _) = BfsWithPrev(wg, 0);
var bfsPath = ReconstructPath(wPrev, 0, 4);
var (bestCost, bestPath) = BruteForceMinCost(5, weighted, 0, 4);

Console.WriteLine($"      BFS 给出的路径：{PathStr(bfsPath, wname)}，{wDist[4]} 步");
Console.WriteLine($"        这条路的实际代价：{PathCost(bfsPath!, weighted)}");
Console.WriteLine();
Console.WriteLine($"      暴力枚举所有路径后的真正最优：{PathStr(bestPath, wname)}，总代价 {bestCost}");
Console.WriteLine();
Console.WriteLine("      BFS 没算错 —— 它回答的是「边数最少」，2 步确实是最少的；");
Console.WriteLine("      错的是把「边数最少」当成了「代价最小」。");
Console.WriteLine();

// ---------- (B) 边权只有 0/1：把队列换成双端队列，BFS 立刻复活 ----------
Console.WriteLine("  (B) 如果边的代价只有 0 和 1 两种呢？");
Console.WriteLine();

// 用【小图 + 多张】来做独立验证：暴力枚举所有简单路径在稠密大图上是组合爆炸，
// 12 个顶点的稀疏图上才跑得动。20 张图 × 12 个目标 = 240 组，与 12.3 节的验证规模一致。
const int N01 = 12;
int totalChecked = 0, mismatch = 0;

for (int seed = 1; seed <= 20; seed++)
{
    var zeroOneEdges = RandomZeroOneGraph(N01, extraEdges: 6, seed);
    var z1 = ZeroOneBfs(N01, zeroOneEdges, 0);

    for (int t = 0; t < N01; t++)
    {
        var (cost, _) = BruteForceMinCost(N01, zeroOneEdges, 0, t);
        totalChecked++;
        if (cost != z1[t]) mismatch++;
    }
}

Console.WriteLine($"      20 张随机的 0/1 权图，每张 {N01} 个顶点、约 17 条边");
Console.WriteLine($"      每张图都验证「从 0 到全部 {N01} 个顶点」的最短代价（与暴力枚举对比）：");
Console.WriteLine($"        共验证 {totalChecked} 组，代价不一致的个数 = {mismatch}");
Console.WriteLine();
Console.WriteLine("      改动只有两行：");
Console.WriteLine("        代价 0 的边 -> 插到队首（它不增加代价，应该立刻处理）");
Console.WriteLine("        代价 1 的边 -> 插到队尾（它增加 1 点代价，和普通 BFS 一样排队）");
Console.WriteLine();
Console.WriteLine("      这就是通往 Dijkstra 的桥：");
Console.WriteLine("      队列只能表达「两种代价」，双端队列能表达「0 和 1」，");
Console.WriteLine("      而优先队列能表达「任意代价」—— 13.3 节就是把双端队列再换一次。");
Console.WriteLine();

// ==================== 本节涉及的算法实现 ====================

// ---------- 单源 BFS（带前驱，跑完整个连通分量）----------
static (int[] dist, int[] prev, long visited) BfsWithPrev(Graph g, int start)
{
    var dist = new int[g.VertexCount];
    var prev = new int[g.VertexCount];
    Array.Fill(dist, -1);
    Array.Fill(prev, -1);

    var queue = new Queue<int>();
    dist[start] = 0;
    queue.Enqueue(start);
    long visited = 1;

    while (queue.Count > 0)
    {
        int v = queue.Dequeue();
        foreach (int next in g.Neighbors(v))
        {
            if (dist[next] != -1) continue;
            dist[next] = dist[v] + 1;
            prev[next] = v;
            queue.Enqueue(next);
            visited++;
        }
    }
    return (dist, prev, visited);
}

// ---------- 单源 BFS（找到目标就停，用于和双向 BFS 公平对比）----------
static (int distance, long visited) BfsToTarget(Graph g, int start, int goal)
{
    if (start == goal) return (0, 1);

    var dist = new int[g.VertexCount];
    Array.Fill(dist, -1);
    var queue = new Queue<int>();
    dist[start] = 0;
    queue.Enqueue(start);
    long visited = 1;

    while (queue.Count > 0)
    {
        int v = queue.Dequeue();
        if (v == goal) return (dist[v], visited);   // ★ 到达目标立刻停

        foreach (int next in g.Neighbors(v))
        {
            if (dist[next] != -1) continue;
            dist[next] = dist[v] + 1;
            queue.Enqueue(next);
            visited++;
        }
    }
    return (-1, visited);
}

// ---------- 多源 BFS ----------
// 和单源唯一的区别：初始化时把所有源点一起入队，距离都是 0。
static (int[] dist, long visited) MultiSourceBfs(Graph g, int[] sources)
{
    var dist = new int[g.VertexCount];
    Array.Fill(dist, -1);
    var queue = new Queue<int>();

    foreach (int s in sources)          // ★ 多源的关键就是这一行
    {
        dist[s] = 0;
        queue.Enqueue(s);
    }
    long visited = sources.Length;

    while (queue.Count > 0)
    {
        int v = queue.Dequeue();
        foreach (int next in g.Neighbors(v))
        {
            if (dist[next] != -1) continue;
            dist[next] = dist[v] + 1;
            queue.Enqueue(next);
            visited++;
        }
    }
    return (dist, visited);
}

// ---------- 双向 BFS ----------
// 两边轮流扩展，每次扩展【已扩展层数较少】的那一边。
// 某个顶点在两边都被访问到时，就找到了碰面点：distF[v] + distB[v] 是一条完整路径的长度。
static (int distance, long visited) BiBfs(Graph g, int start, int goal)
{
    if (start == goal) return (0, 1);

    var distF = new int[g.VertexCount];      // 从起点出发的距离
    var distB = new int[g.VertexCount];      // 从终点出发的距离
    Array.Fill(distF, -1);
    Array.Fill(distB, -1);

    var qF = new Queue<int>();
    var qB = new Queue<int>();
    distF[start] = 0; qF.Enqueue(start);
    distB[goal] = 0; qB.Enqueue(goal);

    long visited = 2;
    int levelF = 0, levelB = 0;
    int best = int.MaxValue;

    // 扩展一层
    void Expand(Queue<int> queue, int[] distOwn, int[] distOther)
    {
        int size = queue.Count;              // ★ 先取本层大小，才是「扩展一层」
        for (int i = 0; i < size; i++)
        {
            int v = queue.Dequeue();
            foreach (int next in g.Neighbors(v))
            {
                if (distOwn[next] != -1) continue;
                distOwn[next] = distOwn[v] + 1;
                queue.Enqueue(next);
                visited++;

                if (distOther[next] != -1)   // ★ 碰面了
                    best = Math.Min(best, distOwn[next] + distOther[next]);
            }
        }
    }

    while (qF.Count > 0 && qB.Count > 0)
    {
        if (levelF + levelB >= best) break;  // 再扩展也不可能更短了

        if (levelF <= levelB) { Expand(qF, distF, distB); levelF++; }
        else { Expand(qB, distB, distF); levelB++; }
    }

    return (best, visited);
}

// ---------- 0-1 BFS：队列换成双端队列 ----------
static int[] ZeroOneBfs(int n, List<(int u, int v, int w)> edges, int start)
{
    var adj = new List<(int to, int w)>[n];
    for (int i = 0; i < n; i++) adj[i] = new List<(int to, int w)>();
    foreach (var (u, v, w) in edges)
    {
        adj[u].Add((v, w));
        adj[v].Add((u, w));                  // 无向图
    }

    var dist = new int[n];
    Array.Fill(dist, int.MaxValue);
    var deque = new LinkedList<int>();
    dist[start] = 0;
    deque.AddFirst(start);

    while (deque.Count > 0)
    {
        int v = deque.First!.Value;
        deque.RemoveFirst();

        foreach (var (to, w) in adj[v])
        {
            if (dist[v] + w >= dist[to]) continue;
            dist[to] = dist[v] + w;

            if (w == 0) deque.AddFirst(to);  // ★ 代价 0：插队首，立刻处理
            else deque.AddLast(to);          // ★ 代价 1：插队尾，正常排队
        }
    }
    return dist;
}

// ---------- 工具：路径还原、代价计算 ----------

static List<int>? ReconstructPath(int[] prev, int start, int target)
{
    if (target != start && prev[target] == -1) return null;   // 不可达

    var path = new List<int>();
    for (int v = target; v != -1; v = prev[v]) path.Add(v);
    path.Reverse();
    return path.Count > 0 && path[0] == start ? path : null;
}

static string PathStr(List<int>? path, string[] names)
    => path is null ? "（不可达）" : string.Join(" -> ", path.Select(v => names[v]));

static int PathCost(List<int> path, List<(int u, int v, int w)> edges)
{
    int total = 0;
    for (int i = 0; i + 1 < path.Count; i++)
    {
        int a = path[i], b = path[i + 1];
        foreach (var (u, v, w) in edges)
            if ((u == a && v == b) || (u == b && v == a)) { total += w; break; }
    }
    return total;
}

// 暴力枚举所有简单路径，返回最小代价和那条路径（用作独立验证）
static (int cost, List<int> path) BruteForceMinCost(int n, List<(int u, int v, int w)> edges, int s, int t)
{
    var adj = new List<(int to, int w)>[n];
    for (int i = 0; i < n; i++) adj[i] = new List<(int to, int w)>();
    foreach (var (u, v, w) in edges)
    {
        adj[u].Add((v, w));
        adj[v].Add((u, w));
    }

    int bestCost = int.MaxValue;
    var bestPath = new List<int>();
    var path = new List<int>();
    var onPath = new bool[n];

    void Dfs(int v, int cost)
    {
        if (cost >= bestCost) return;        // 剪枝
        if (v == t)
        {
            bestCost = cost;
            bestPath = new List<int>(path);
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
    path.Add(s);
    Dfs(s, 0);
    return (bestCost, bestPath);
}

// ---------- 图的构造 ----------

// 二维网格：每个格子连右、下两个邻居（无向）
static Graph GridGraph(int rows, int cols)
{
    var g = new Graph(rows * cols);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int id = r * cols + c;
            if (c + 1 < cols) g.AddEdge(id, id + 1);
            if (r + 1 < rows) g.AddEdge(id, id + cols);
        }
    return g;
}

// 完全 b 叉树：节点 i 的父亲是 (i - 1) / b
static Graph BalancedTree(int branching, int depth)
{
    int n = (int)((Math.Pow(branching, depth + 1) - 1) / (branching - 1));
    var g = new Graph(n);
    for (int i = 1; i < n; i++) g.AddEdge((i - 1) / branching, i);
    return g;
}

// 随机 0/1 权图（先拉一条链保证连通，再加随机边）
static List<(int u, int v, int w)> RandomZeroOneGraph(int n, int extraEdges, int seed)
{
    var rng = new Random(seed);
    var edges = new List<(int u, int v, int w)>();
    for (int i = 0; i + 1 < n; i++) edges.Add((i, i + 1, rng.Next(2)));
    for (int i = 0; i < extraEdges; i++)
    {
        int a = rng.Next(n), b = rng.Next(n);
        if (a != b) edges.Add((a, b, rng.Next(2)));
    }
    return edges;
}

// ---------- 无向图：邻接表 ----------

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
