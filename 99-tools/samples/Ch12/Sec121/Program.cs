using System.Diagnostics;

// ==================== 两种图表示 ====================
//
// 示例图（城市航线，无向图）：
//        北京 —— 上海
//         |  \      |
//         |   \     |
//       广州 —— 深圳
//         |
//       成都

string[] cities = { "北京", "上海", "广州", "深圳", "成都" };

// 边列表（无向图，每条边存两次）
var edges = new (string A, string B)[]
{
    ("北京", "上海"), ("北京", "广州"), ("北京", "深圳"),
    ("上海", "深圳"),
    ("广州", "深圳"), ("广州", "成都"),
};

// ---------- 表示法 1：邻接矩阵 ----------
// matrix[i][j] = true 表示 i 和 j 之间有边
var cityIndex = cities.Select((c, i) => (c, i)).ToDictionary(x => x.c, x => x.i);
var matrix = new bool[cities.Length, cities.Length];

foreach (var (a, b) in edges)
{
    int i = cityIndex[a], j = cityIndex[b];
    matrix[i, j] = true;
    matrix[j, i] = true;              // 无向图：两个方向都要标记
}

// ---------- 表示法 2：邻接表 ----------
var adjList = new Dictionary<string, List<string>>();
foreach (var c in cities) adjList[c] = new List<string>();

foreach (var (a, b) in edges)
{
    adjList[a].Add(b);
    adjList[b].Add(a);                // 无向图：两个方向都要加
}

// ==================== 实验一：邻接矩阵长什么样 ====================

Console.WriteLine("=== 实验一：邻接矩阵 ===");
Console.WriteLine();
Console.WriteLine("  示例图（城市航线）：");
Console.WriteLine("        北京 —— 上海");
Console.WriteLine("         |  \\      |");
Console.WriteLine("         |   \\     |");
Console.WriteLine("       广州 —— 深圳");
Console.WriteLine("         |");
Console.WriteLine("       成都");
Console.WriteLine();

Console.Write("  ".PadRight(8));
foreach (var c in cities) Console.Write($"{c,6}");
Console.WriteLine();

for (int i = 0; i < cities.Length; i++)
{
    Console.Write($"  {cities[i],-6}");
    for (int j = 0; j < cities.Length; j++)
        Console.Write($"{(matrix[i, j] ? "1" : "·"),6}");
    Console.WriteLine();
}
Console.WriteLine();
Console.WriteLine("  1 表示有航线，· 表示没有。注意矩阵是【对称】的（无向图）。");
Console.WriteLine();

// ==================== 实验二：邻接表长什么样 ====================

Console.WriteLine("=== 实验二：邻接表 ===");
Console.WriteLine();
Console.WriteLine("  每个城市记录它【直接相连】的城市：");
Console.WriteLine();

foreach (var c in cities)
    Console.WriteLine($"    {c,-6} -> {string.Join(", ", adjList[c])}");
Console.WriteLine();

// ==================== 实验三：两种表示的操作代价对比 ====================

Console.WriteLine("=== 实验三：两种表示的操作代价 ===");
Console.WriteLine();

const int V = 10_000;        // 顶点数
const int AVG_DEGREE = 10;   // 平均度数

Console.WriteLine($"  假设一个图有 {V:N0} 个顶点，每个顶点平均连 {AVG_DEGREE} 条边");
Console.WriteLine($"  （总边数 E ≈ {V * AVG_DEGREE / 2:N0}）");
Console.WriteLine();

// 实际测量空间占用。
//
// 这里踩了三次坑，值得记下来 —— 用 GC.GetTotalMemory 的"差值法"在这个场景不可靠：
//   坑 1：不"使用"大对象时 JIT 认为它已死，强制 GC 会把它收走 -> 实测 0.0 MB
//   坑 2：加 GC.KeepAlive 后，第一个对象在后一次测量时被回收 -> 实测 -94.0 MB（污染）
//   坑 3：加 GC.Collect() 也挡不住（JIT 的存活分析与我们的直觉不一致）
//
// 最终方案：改用 GC.GetAllocatedBytesForCurrentThread() ——
//   它统计的是「本线程累计分配了多少字节」，与何时回收完全无关。
double matrixMb;
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    var m = new bool[V, V];                   // 后面对照：V² 个 bool
    m[0, 0] = true;
    long after = GC.GetAllocatedBytesForCurrentThread();
    matrixMb = (after - before) / 1024.0 / 1024.0;
    GC.KeepAlive(m);
}

// 邻接表后面实验五还要用，所以声明在外层
var bigAdj = new Dictionary<int, List<int>>();
double adjMb;
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    for (int i = 0; i < V; i++)
    {
        bigAdj[i] = new List<int>(AVG_DEGREE);
        for (int k = 0; k < AVG_DEGREE; k++)
            bigAdj[i].Add((i + k + 1) % V);
    }
    long after = GC.GetAllocatedBytesForCurrentThread();
    adjMb = (after - before) / 1024.0 / 1024.0;
}

Console.WriteLine($"  实际内存占用（{V:N0} 个顶点，每个平均 {AVG_DEGREE} 条边）：");
Console.WriteLine($"    邻接矩阵: {matrixMb,8:F1} MB   （{(long)V * V * 1 / 1024.0 / 1024.0:F1} MB = V² 个 bool）");
Console.WriteLine($"    邻接表  : {adjMb,8:F1} MB   （约 V + 2E 个元素）");
Console.WriteLine($"    矩阵是邻接表的 {matrixMb / adjMb:F1} 倍");
Console.WriteLine();

Console.WriteLine($"  {"操作",-28} | {"邻接矩阵",-18} | {"邻接表",-18}");
Console.WriteLine("  " + new string('-', 70));
Console.WriteLine($"  {"判断两点是否相邻",-28} | {"O(1) 直接查表",-18} | {"O(度数) 遍历链表",-18}");
Console.WriteLine($"  {"遍历某点的所有邻居",-26} | {"O(V) 扫一整行",-18} | {"O(度数) 只走有边的",-18}");
Console.WriteLine($"  {"空间",-30} | {"O(V²)",-18} | {"O(V + E)",-18}");
Console.WriteLine($"  {"加一条边",-28} | {"O(1)",-18} | {"O(1)（追加到列表）",-18}");
Console.WriteLine($"  {"删一条边",-28} | {"O(1)",-18} | {"O(度数)（要在链表里找）",-18}");
Console.WriteLine();

// ==================== 实验四：什么时候该用哪个 ====================

Console.WriteLine("=== 实验四：选型判断 ===");
Console.WriteLine();

Console.WriteLine("  关键看【图的稠密程度】：");
Console.WriteLine();
Console.WriteLine($"    {"图类型",-20} | {"边数 E",-20} | {"推荐",-14} | 理由");
Console.WriteLine("    " + new string('-', 76));
Console.WriteLine($"    {"稀疏图",-22} | {"E ≈ V",-22} | {"邻接表",-16} | 矩阵浪费 O(V²) 空间");
Console.WriteLine($"    {"稠密图",-22} | {"E ≈ V²",-20} | {"邻接矩阵",-16} | 表省不了多少，矩阵更快");
Console.WriteLine($"    {"需要频繁判断相邻",-18} | {"看情况",-22} | {"邻接矩阵",-16} | 判断是 O(1) vs O(度数)");
Console.WriteLine();

Console.WriteLine("  实际场景：");
Console.WriteLine("    - 社交网络好友关系：稀疏（平均好友数几十）-> 邻接表");
Console.WriteLine("    - 地图道路网络：稀疏（每个路口连 3~4 条路）-> 邻接表");
Console.WriteLine("    - 网页链接：稀疏 -> 邻接表");
Console.WriteLine("    - 小规模稠密图（比如 100 个节点的完全图）-> 邻接矩阵");
Console.WriteLine("    - 需要频繁判断'两点是否直接相连'-> 邻接矩阵");
Console.WriteLine();
Console.WriteLine("  【结论】绝大多数真实世界的图都是稀疏图 -> 默认用【邻接表】。");
Console.WriteLine();

// ==================== 实验五：邻接表的实现选择 ====================

Console.WriteLine("=== 实验五：邻接表用什么容器实现？ ===");
Console.WriteLine();

var sw = new Stopwatch();

// 方案 A：List<int>[]（数组套 List）
var listArray = new List<int>[V];
for (int i = 0; i < V; i++) listArray[i] = new List<int>(AVG_DEGREE);
for (int i = 0; i < V; i++)
    for (int k = 0; k < AVG_DEGREE; k++)
        listArray[i].Add((i + k + 1) % V);

sw.Restart();
long sum1 = 0;
for (int round = 0; round < 100; round++)
    for (int i = 0; i < V; i++)
        foreach (int neighbor in listArray[i]) sum1 += neighbor;
sw.Stop();
double arrayMs = sw.Elapsed.TotalMilliseconds;

// 方案 B：Dictionary<int, List<int>>
sw.Restart();
long sum2 = 0;
for (int round = 0; round < 100; round++)
    for (int i = 0; i < V; i++)
        foreach (int neighbor in bigAdj[i]) sum2 += neighbor;
sw.Stop();
double dictMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  遍历全图 100 遍（{V:N0} 个顶点，每个 {AVG_DEGREE} 条边）：");
Console.WriteLine();
Console.WriteLine($"    List<int>[] 数组套表    : {arrayMs,8:F1} ms");
Console.WriteLine($"    Dictionary<int, List>   : {dictMs,8:F1} ms");
Console.WriteLine();
Console.WriteLine($"    数组版快 {dictMs / arrayMs:F2} 倍");
Console.WriteLine();
Console.WriteLine("  两者结果一致: " + (sum1 == sum2));
Console.WriteLine();
Console.WriteLine("  【为什么数组版更快？】");
Console.WriteLine("    1. 数组下标直接定位，不需要算哈希、查字典；");
Console.WriteLine($"    2. {V:N0} 个 List 对象的引用连续排列，CPU 缓存命中率更高。");
Console.WriteLine();
Console.WriteLine("  但 Dictionary 版更通用：");
Console.WriteLine("    - 顶点编号不连续时（比如是字符串 ID）只能用它；");
Console.WriteLine("    - 顶点是动态增加的时候，数组要先扩容。");
Console.WriteLine();
