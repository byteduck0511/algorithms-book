// ==================== 第 13.4 节：负权边与 Bellman-Ford ====================
//
//   实验一：为什么需要 V-1 轮 —— 【边的给出顺序】直接决定收敛轮数
//   实验二：13.3 节那个负权反例，Bellman-Ford 怎么算对
//   实验三：负环检测（第 V 轮还能松弛 = 有负环）
//   实验四：性能对比 —— Bellman-Ford vs SPFA vs Dijkstra，以及实际轮数

using System.Diagnostics;

// ==================== 实验一：轮数由边的顺序决定 ====================

Console.WriteLine("=== 实验一：为什么是 V-1 轮？—— 边的给出顺序决定轮数 ===");
Console.WriteLine();

const int ChainN = 8;
var forward = new List<(int u, int v, int w)>();
var backward = new List<(int u, int v, int w)>();
for (int i = 0; i + 1 < ChainN; i++)
{
    forward.Add((i, i + 1, 1));            // 0->1, 1->2, ..., 6->7
    backward.Insert(0, (i, i + 1, 1));     // 6->7, 5->6, ..., 0->1
}

Console.WriteLine($"  图是一条链：0 -> 1 -> 2 -> ... -> {ChainN - 1}（每条边权 1），起点是 0");
Console.WriteLine($"  同一个图，同一批边，只把【边的排列顺序】换一下：");
Console.WriteLine();

RunAndTrace("情形 A：边按【正序】给出（先 0->1，最后 6->7）", ChainN, forward);
Console.WriteLine();
RunAndTrace("情形 B：边按【逆序】给出（先 6->7，最后 0->1）", ChainN, backward);
Console.WriteLine();
Console.WriteLine("  两边的边【一模一样】，只是排列顺序不同 —— 轮数差了 7 倍。");
Console.WriteLine("  原因：每一轮里，信息只能沿着「这一轮中出现的先后关系」往前传一跳。");
Console.WriteLine("        正序时边和路径方向一致，一轮就从头传到尾；");
Console.WriteLine("        逆序时每轮只能推进一条边，必须跑满 V-1 轮。");
Console.WriteLine();

// ==================== 实验二：13.3 那个负权反例 ====================

Console.WriteLine("=== 实验二：Bellman-Ford 怎么算对 13.3 那个反例 ===");
Console.WriteLine();
Console.WriteLine("  有向图（同 13.3 节）：");
Console.WriteLine("    S --1--> A --1--> B --1--> T");
Console.WriteLine("    S --------5------> C --(-10)--> B");
Console.WriteLine();

var badEdges = new List<(int u, int v, int w)>
{
    (0, 1, 1),      // S -> A
    (1, 2, 1),      // A -> B
    (0, 3, 5),      // S -> C
    (3, 2, -10),    // C -> B
    (2, 4, 1),      // B -> T
};
string[] names = ["S", "A", "B", "C", "T"];

var (bfDist, bfPrev, bfRounds, bfCycle) = BellmanFord(5, badEdges, 0, verbose: true);
Console.WriteLine();
Console.WriteLine($"  收敛用了 {bfRounds} 轮（上限是 V-1 = 4 轮），有负环 = {bfCycle}");
Console.WriteLine($"  dist[T] = {bfDist[4]}");
Console.WriteLine();
Console.WriteLine("  对照 13.3 节的实测：");
Console.WriteLine("    Dijkstra（贪心，出队即确定）：dist[T] = 3      ← 错");
Console.WriteLine("    Bellman-Ford（每轮扫全部边）  ：dist[T] = -4    ← 对");
Console.WriteLine();

// ==================== 实验三：负环检测 ====================

Console.WriteLine("=== 实验三：负环检测 ===");
Console.WriteLine();
Console.WriteLine("  有向图：A --1--> B，B --(-2)--> A（绕一圈总权 -1），A --1--> T");
Console.WriteLine();

var cycleEdges = new List<(int u, int v, int w)>
{
    (0, 1, 1),      // A -> B
    (1, 0, -2),     // B -> A   ★ 负环
    (0, 2, 1),      // A -> T
};
string[] cnames = ["A", "B", "T"];

var (cycDist, _, cycRounds, hasCycle) = BellmanFord(3, cycleEdges, 0, verbose: true, names: cnames);
Console.WriteLine();
Console.WriteLine($"  跑完 V-1 = 2 轮后，再补一轮【检测轮】：");
Console.WriteLine($"    检测轮还能不能松弛？ {(hasCycle ? "能 —— 判定为【有负环】" : "不能")}");
Console.WriteLine($"    dist 数组：{string.Join(", ", cycDist.Select((d, i) => $"{cnames[i]}={Fmt(d)}"))}");
Console.WriteLine();
Console.WriteLine("  这就是 Bellman-Ford 相对 Dijkstra 的第二项本事：");
Console.WriteLine("    它不只是「能处理负权」，还能【告诉你负环在哪里发生】——");
Console.WriteLine("    无负环时，V-1 轮必然收敛，第 V 轮不可能再松弛；");
Console.WriteLine("    第 V 轮还能松弛，就说明存在一条可以无限绕下去的路。");
Console.WriteLine();

// ==================== 实验四：性能对比 ====================

Console.WriteLine("=== 实验四：Bellman-Ford vs SPFA vs Dijkstra ===");
Console.WriteLine();

const int V = 5000;
var (randEdges, randAdj) = RandomDigraph(V, targetUndirectedEdges: 15_000, seed: 1);

// 链状图 + 逆序边表 = Bellman-Ford 的【最坏情况】（实验一已经证明：逆序要跑 V-1 轮）
var chainEdges = new List<(int u, int v, int w)>();
var chainAdj = new List<(int to, int w)>[V];
for (int i = 0; i < V; i++) chainAdj[i] = new List<(int to, int w)>();
for (int i = V - 2; i >= 0; i--)          // ★ 逆序：先给最后一条边
{
    chainEdges.Add((i, i + 1, 1));
    chainAdj[i].Add((i + 1, 1));
}

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

// 预热
BellmanFord(V, randEdges, 0);
Spfa(V, randAdj, 0);
DijkstraHeap(V, randAdj, 0);
BellmanFord(V, chainEdges, 0);
Spfa(V, chainAdj, 0);
DijkstraHeap(V, chainAdj, 0);

Console.WriteLine($"  图 1：随机稀疏图（V = {V:N0}，{randEdges.Count:N0} 条有向边）");
var (_, _, rRounds, _) = BellmanFord(V, randEdges, 0);
var (_, rDeq) = Spfa(V, randAdj, 0);
double tBf1 = BestMs(() => BellmanFord(V, randEdges, 0));
double tSpfa1 = BestMs(() => Spfa(V, randAdj, 0));
double tDij1 = BestMs(() => DijkstraHeap(V, randAdj, 0));
Console.WriteLine($"    Bellman-Ford：实际 {rRounds} 轮（上限 {V - 1:N0} 轮），耗时 {tBf1,7:F2} ms");
Console.WriteLine($"    SPFA        ：出队 {rDeq:N0} 次，耗时 {tSpfa1,7:F2} ms");
Console.WriteLine($"    Dijkstra    ：耗时 {tDij1,7:F2} ms");
Console.WriteLine();

Console.WriteLine($"  图 2：链状图 + 【逆序】边表（V = {V:N0}，{chainEdges.Count:N0} 条有向边）");
Console.WriteLine($"        —— 这是实验一验证过的 Bellman-Ford 最坏情况");
var (_, _, cRounds, _) = BellmanFord(V, chainEdges, 0);
var (_, cDeq) = Spfa(V, chainAdj, 0);
double tBf2 = BestMs(() => BellmanFord(V, chainEdges, 0));
double tSpfa2 = BestMs(() => Spfa(V, chainAdj, 0));
double tDij2 = BestMs(() => DijkstraHeap(V, chainAdj, 0));
Console.WriteLine($"    Bellman-Ford：实际 {cRounds:N0} 轮（= V-1，跑满上限），耗时 {tBf2,7:F2} ms");
Console.WriteLine($"    SPFA        ：出队 {cDeq:N0} 次，耗时 {tSpfa2,7:F2} ms");
Console.WriteLine($"    Dijkstra    ：耗时 {tDij2,7:F2} ms");
Console.WriteLine();

Console.WriteLine("  两处对比值得停下来看：");
Console.WriteLine();
Console.WriteLine("  1) 图 1 上 Bellman-Ford 只跑了几轮 —— 随机顺序的边表让信息传播得很快。");
Console.WriteLine("     教科书说它 O(V·E)、比 Dijkstra 慢得多，但那个上界【很少被触发】。");
Console.WriteLine($"  2) 图 2 上它跑满了 {cRounds:N0} 轮 —— 这才是 O(V·E) 的真实形态：");
Console.WriteLine($"     {cRounds:N0} 轮 × {chainEdges.Count:N0} 条边 ≈ {cRounds * (long)chainEdges.Count:N0} 次松弛尝试。");
Console.WriteLine();
Console.WriteLine("  而 SPFA 在图 2 上只出队 5,000 次 —— 因为它【只处理距离刚被改进过的顶点】，");
Console.WriteLine("  不做「每轮把全部边扫一遍」这种重复劳动。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- Bellman-Ford ----------
// 反复扫【全部边】做松弛，最多 V-1 轮。
// 返回：dist、prev、实际用了几轮、是否有负环。
static (int[] dist, int[] prev, int rounds, bool hasNegativeCycle) BellmanFord(
    int n, List<(int u, int v, int w)> edges, int start, bool verbose = false, string[]? names = null)
{
    var dist = new int[n];
    var prev = new int[n];
    Array.Fill(dist, int.MaxValue / 2);
    Array.Fill(prev, -1);
    dist[start] = 0;

    int rounds = 0;
    for (int i = 0; i < n - 1; i++)
    {
        bool changed = false;
        foreach (var (u, v, w) in edges)
        {
            if (dist[u] == int.MaxValue / 2) continue;   // u 还不可达，这条边无从松弛
            if (dist[u] + w < dist[v])
            {
                dist[v] = dist[u] + w;
                prev[v] = u;
                changed = true;
            }
        }

        if (verbose)
        {
            string body = names is null
                ? string.Join(" ", dist.Select(Fmt))
                : string.Join("  ", dist.Select((d, k) => $"{names[k]}={Fmt(d)}"));
            Console.WriteLine($"    第 {i + 1} 轮后：{body}{(changed ? "" : "   （本轮没有任何变化）")}");
        }

        if (!changed) break;      // ★ 一轮下来一次松弛都没有 -> 已经收敛
        rounds++;
    }

    // 检测轮：再扫一遍，还能松弛就是有负环
    bool hasNegativeCycle = false;
    foreach (var (u, v, w) in edges)
    {
        if (dist[u] == int.MaxValue / 2) continue;
        if (dist[u] + w < dist[v]) { hasNegativeCycle = true; break; }
    }

    return (dist, prev, rounds, hasNegativeCycle);
}

// ---------- SPFA：Bellman-Ford + 队列 ----------
// 只把「距离刚刚被改进过」的顶点的出边拿去松弛，而不是每轮盲目扫全部边。
// 返回值第三个是「出队次数」，用来和 Bellman-Ford 的轮数对比。
static (int[] dist, long dequeueCount) Spfa(int n, List<(int to, int w)>[] adj, int start)
{
    var dist = new int[n];
    var inQueue = new bool[n];
    var enqueueCount = new int[n];
    Array.Fill(dist, int.MaxValue / 2);
    dist[start] = 0;

    var queue = new Queue<int>();
    queue.Enqueue(start);
    inQueue[start] = true;
    long dequeueCount = 0;

    while (queue.Count > 0)
    {
        int u = queue.Dequeue();
        inQueue[u] = false;
        dequeueCount++;

        foreach (var (v, w) in adj[u])
        {
            if (dist[u] + w >= dist[v]) continue;
            dist[v] = dist[u] + w;
            if (inQueue[v]) continue;

            queue.Enqueue(v);
            inQueue[v] = true;
            if (++enqueueCount[v] > n) return (dist, dequeueCount);   // 入队超过 n 次 -> 有负环
        }
    }
    return (dist, dequeueCount);
}

// ---------- Dijkstra（优先队列版，用来做性能参照）----------
static int[] DijkstraHeap(int n, List<(int to, int w)>[] adj, int start)
{
    var dist = new int[n];
    var done = new bool[n];
    Array.Fill(dist, int.MaxValue);
    dist[start] = 0;

    var pq = new PriorityQueue<int, int>();
    pq.Enqueue(start, 0);

    while (pq.TryDequeue(out int v, out int d))
    {
        if (done[v]) continue;
        done[v] = true;
        foreach (var (to, w) in adj[v])
        {
            if (done[to]) continue;
            int nd = d + w;
            if (nd < dist[to]) { dist[to] = nd; pq.Enqueue(to, nd); }
        }
    }
    return dist;
}

// ---------- 工具 ----------

static string Fmt(int d) => d == int.MaxValue / 2 ? " ∞" : $"{d,2}";

static void RunAndTrace(string title, int n, List<(int u, int v, int w)> edges)
{
    Console.WriteLine($"  {title}");
    var (dist, _, rounds, _) = BellmanFord(n, edges, 0, verbose: true);
    Console.WriteLine($"    -> 共 {rounds} 轮收敛");
}

// 随机有向图：先生成 targetUndirectedEdges 条无向边（保证连通），每条边转成两个方向
static (List<(int u, int v, int w)> edges, List<(int to, int w)>[] adj) RandomDigraph(
    int n, int targetUndirectedEdges, int seed)
{
    var rng = new Random(seed);
    var undirected = new List<(int a, int b, int w)>();
    for (int i = 0; i + 1 < n; i++) undirected.Add((i, i + 1, rng.Next(1, 100)));

    var seen = new HashSet<(int, int)>();
    while (undirected.Count < targetUndirectedEdges)
    {
        int a = rng.Next(n), b = rng.Next(n);
        if (a == b) continue;
        if (a > b) (a, b) = (b, a);
        if (!seen.Add((a, b))) continue;
        undirected.Add((a, b, rng.Next(1, 100)));
    }

    var edges = new List<(int u, int v, int w)>();
    var adj = new List<(int to, int w)>[n];
    for (int i = 0; i < n; i++) adj[i] = new List<(int to, int w)>();

    foreach (var (a, b, w) in undirected)
    {
        edges.Add((a, b, w));
        edges.Add((b, a, w));
        adj[a].Add((b, w));
        adj[b].Add((a, w));
    }
    return (edges, adj);
}
