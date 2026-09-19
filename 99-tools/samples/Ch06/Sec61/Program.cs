using System.Diagnostics;
using System.Text;

// ==================== 实验一：直接寻址表 ====================

Console.WriteLine("=== 实验一：直接寻址表 —— 最快，但最不实用 ===");
Console.WriteLine();

// 场景：一所学校，学生学号是 0 ~ 999 的整数。要按学号快速查到姓名。
string?[] students = new string?[1000];
students[42] = "小陈";
students[7] = "老周";

Console.WriteLine($"  学号 42 的学生: {students[42]}");
Console.WriteLine($"  学号 7  的学生: {students[7]}");
Console.WriteLine($"  学号 100 的学生: {students[100] ?? "(没有这个学号)"}");
Console.WriteLine();
Console.WriteLine("  这就是「直接寻址表」：键就是数组下标，一次跳转就到位，O(1)。");
Console.WriteLine();

var sw = Stopwatch.StartNew();
long found = 0;
for (int i = 0; i < 10_000_000; i++)
    if (students[i % 1000] != null) found++;
sw.Stop();
Console.WriteLine($"  1000 万次随机访问: {sw.Elapsed.TotalMilliseconds:F0} ms（命中 {found:N0} 次）");
Console.WriteLine();

// ==================== 实验二：直接寻址的问题 ====================

Console.WriteLine("=== 实验二：什么时候直接寻址不能用 ===");
Console.WriteLine();

Console.WriteLine("  问题 1：键的范围太大");
Console.WriteLine("    如果学号是 9 位数字（最大 999,999,999），总不能开一个 10 亿长度的数组。");
Console.WriteLine();
Console.WriteLine("  问题 2：键不是整数");
Console.WriteLine("    如果键是用户名（字符串），根本没有「下标」这回事。");
Console.WriteLine();
Console.WriteLine("  问题 3：有效数据很少");
Console.WriteLine("    就算开了 10 亿长度的数组，实际只有 5000 个学生 —— 99.9995% 的空间是浪费的。");
Console.WriteLine();
Console.WriteLine("  解决思路：把「大范围的键」映射到「小范围的下标」。");
Console.WriteLine("  这个映射函数，就叫【哈希函数】。");
Console.WriteLine();

// ==================== 实验三：一个好哈希函数有多重要 ====================

Console.WriteLine("=== 实验三：坏哈希函数的致命缺陷 ===");
Console.WriteLine();

// 坏哈希：把所有字符的 ASCII 码相加
static int BadHash(string s)
{
    int hash = 0;
    foreach (char c in s) hash += c;
    return hash;
}

// 好哈希：多项式滚动哈希（乘一个质数再加下一个字符）
static int GoodHash(string s)
{
    int hash = 17;
    foreach (char c in s)
        hash = hash * 31 + c;
    return hash;
}

// 更好的哈希：多项式 + 末尾的「混合步骤」（finalizer）
// 混合步骤把高位的变化扩散到低位，这是 MurmurHash 等专业哈希函数的标配
static int BetterHash(string s)
{
    int hash = 17;
    foreach (char c in s)
        hash = hash * 31 + c;

    // ---- finalizer ----
    unchecked
    {
        hash ^= hash >> 16;
        hash *= (int)0x85ebca6b;
        hash ^= hash >> 13;
        hash *= (int)0xc2b2ae35;
        hash ^= hash >> 16;
    }
    return hash;
}

Console.WriteLine("  测试数据是一组「字母重排词」（由完全相同的字母组成，只是顺序不同）：");
Console.WriteLine();
Console.WriteLine($"  {"单词",-10} {"坏哈希(求和)",14} {"好哈希(多项式)",16}");
Console.WriteLine(new string('-', 46));

string[] anagrams = { "listen", "silent", "enlist", "tinsel", "inlets", "lentis" };
foreach (var w in anagrams)
    Console.WriteLine($"  {w,-10} {BadHash(w),14} {GoodHash(w),16}");

Console.WriteLine();
Console.WriteLine("  看出来了吗：坏哈希算出来【全都是同一个值】！");
Console.WriteLine("  因为字符求和的结果只跟「用了哪些字母」有关，跟顺序完全无关。");
Console.WriteLine("  这六个词会全部挤进同一个桶，哈希表直接退化成一条链表 —— 查找变成 O(n)。");
Console.WriteLine();
Console.WriteLine("  好哈希引入了「位置」的影响（每读一个字符就乘 31），所以顺序一变，结果就变。");
Console.WriteLine();

// ==================== 实验四：分布均匀性 ====================

Console.WriteLine("=== 实验四：哈希值的分布均匀性 ===");
Console.WriteLine();

var rng = new Random(42);
var words = new List<string>();
for (int i = 0; i < 10_000; i++)
{
    var sb = new StringBuilder();
    int len = rng.Next(4, 12);
    for (int j = 0; j < len; j++) sb.Append((char)('a' + rng.Next(26)));
    words.Add(sb.ToString());
}

void AnalyzeDistribution(string name, Func<string, int> hashFunc, int bucketCount)
{
    var buckets = new int[bucketCount];
    foreach (var w in words)
    {
        int idx = Math.Abs(hashFunc(w) % bucketCount);   // 取模映射到桶
        buckets[idx]++;
    }

    int empty = 0, max = 0;
    long collisionPairs = 0;
    foreach (int c in buckets)
    {
        if (c == 0) empty++;
        if (c > max) max = c;
        collisionPairs += (long)c * (c - 1) / 2;         // 这个桶里的配对冲突数
    }

    Console.WriteLine($"  {name}:");
    Console.WriteLine($"    空桶数量      : {empty,6:N0} / {bucketCount:N0}  ({empty * 100.0 / bucketCount:F1}%)");
    Console.WriteLine($"    最长的桶      : {max,6:N0} 个元素");
    Console.WriteLine($"    冲突配对数    : {collisionPairs,6:N0}");
    Console.WriteLine($"    理论期望每桶  : {words.Count / (double)bucketCount:F1} 个元素");
}

const int Buckets = 1000;
Console.WriteLine($"  {words.Count:N0} 个随机字符串，{Buckets} 个桶");
Console.WriteLine();
AnalyzeDistribution("坏哈希（字符求和）", BadHash, Buckets);
Console.WriteLine();
AnalyzeDistribution("好哈希（多项式）  ", GoodHash, Buckets);
Console.WriteLine();

Console.WriteLine("  注意：随机字符串下，坏哈希的表现也不算太糟（因为字母分布本身是随机的）。");
Console.WriteLine("  但在真实数据里（比如大量有相同字母组成的词、或者有规律的编号），");
Console.WriteLine("  坏哈希会迅速退化 —— 实验三的字母重排词就是极端例子。");
Console.WriteLine();

// ==================== 哈希函数的三条要求 ====================

Console.WriteLine("=== 一个好哈希函数的三条要求 ===");
Console.WriteLine();
Console.WriteLine("  1. 确定性：同一个键，每次算出来必须是同一个值。");
Console.WriteLine("     （否则存进去就找不回来了）");
Console.WriteLine();
Console.WriteLine("  2. 均匀性：不同的键应该尽量分散到不同的桶。");
Console.WriteLine("     （实验三的坏哈希就违反了这条）");
Console.WriteLine();
Console.WriteLine("  3. 高效性：计算要快。哈希函数每次增删改查都要调用，它本身不能是瓶颈。");
Console.WriteLine();
Console.WriteLine("  补充：好的哈希还应该「雪崩」—— 输入变一个字符，输出应该面目全非。");
Console.WriteLine();

Console.WriteLine("  业界常用的哈希函数：");
Console.WriteLine("    - DJB2 / FNV-1a：简单快速，适合一般场景");
Console.WriteLine("    - MurmurHash / xxHash：分布更好，适合哈希表");
Console.WriteLine("    - SipHash：带密钥，能抵抗「哈希碰撞攻击」（6.4 节会讲）");
Console.WriteLine("    - SHA-256：密码学级别，但太慢，不适合当哈希表");
Console.WriteLine();

// 雪崩演示
Console.WriteLine("=== 附：雪崩效应演示（以及简单的多项式哈希为什么不够好）===");
Console.WriteLine();
Console.WriteLine($"  {"输入对",-22} {"多项式哈希",-30} {"带混合步骤",-30}");
Console.WriteLine(new string('-', 84));

foreach (var pair in new[] { ("hello", "hellp"), ("abc", "abd"), ("user1", "user2") })
{
    int g1 = GoodHash(pair.Item1), g2 = GoodHash(pair.Item2);
    int b1 = BetterHash(pair.Item1), b2 = BetterHash(pair.Item2);

    double gDiff = System.Numerics.BitOperations.PopCount((uint)(g1 ^ g2)) / 32.0;
    double bDiff = System.Numerics.BitOperations.PopCount((uint)(b1 ^ b2)) / 32.0;

    Console.WriteLine($"  \"{pair.Item1}\" -> \"{pair.Item2}\"{new string(' ', 4)}" +
                      $"{g1,12} (差异 {gDiff,4:P0})   {b1,12} (差异 {bDiff,4:P0})");
}
Console.WriteLine();
Console.WriteLine("  看第一列：多项式哈希下，\"hello\" 和 \"hellp\" 的结果【只差 1】！");
Console.WriteLine("  原因是：前四个字符的计算完全相同，只有最后一步 hash*31+c 差了 1（'o' 和 'p' 相差 1）。");
Console.WriteLine("  两个只差一个字符的键，会落到相邻的桶里 —— 如果有很多这样的键，就会挤成一堆。");
Console.WriteLine();
Console.WriteLine("  第二列加了「混合步骤」（finalizer）之后，差异位占比接近 50% —— 这才是真正的雪崩效应。");
Console.WriteLine();
Console.WriteLine("  结论：别看多项式哈希写起来简单就以为够用了。");
Console.WriteLine("  生产环境的哈希函数（MurmurHash、xxHash、SipHash）都包含了这类「打散」步骤，");
Console.WriteLine("  目的就是让输入的微小变化在输出里被彻底放大。");
