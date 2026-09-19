// ==================== 第 14.2 节：区间调度与 Huffman 编码 ====================
//
//   实验一：区间调度 —— 三个「看起来都很有道理」的贪心策略，只有一个是对的
//   实验二：Huffman 编码 —— 构造过程、前缀码性质、以及「最优前缀码」到底能省多少
//
// 度量说明：实验一用「区间个数」与「正确率」，实验二用「比特数」——
//           都是与机器无关的量，任何电脑跑出来结果相同。

// ==================== 实验一：区间调度 ====================

Console.WriteLine("=== 实验一：区间调度 —— 三个贪心策略，只有一个对 ===");
Console.WriteLine();
Console.WriteLine("  问题：给你一批时间段 [开始, 结束)，最多能参加几个【互不重叠】的？");
Console.WriteLine("  三个候选策略：");
Console.WriteLine("    策略 A：每次选【开始时间最早】的");
Console.WriteLine("    策略 B：每次选【持续时间最短】的");
Console.WriteLine("    策略 C：每次选【结束时间最早】的");
Console.WriteLine();

// ---------- 两个反例 ----------
Console.WriteLine("  ---------- 反例 1：策略 A（开始最早）会栽在这里 ----------");
var exA = new List<(int s, int e)> { (0, 10), (1, 2), (3, 4) };
ShowIntervals(exA);
Console.WriteLine($"    策略 A（开始最早）：选 [0,10)，后面两个都装不下 -> 1 个");
Console.WriteLine($"    策略 C（结束最早）：选 [1,2) 再选 [3,4) -> 2 个");
Console.WriteLine($"    暴力枚举的最优解：2 个");
Console.WriteLine();

Console.WriteLine("  ---------- 反例 2：策略 B（时长最短）会栽在这里 ----------");
var exB = new List<(int s, int e)> { (0, 4), (3, 7), (5, 14), (3, 6) };
ShowIntervals(exB);
Console.WriteLine($"    策略 B（时长最短）：[3,6) 长度 3 最短，但它把另外三个全挡住了 -> 1 个");
Console.WriteLine($"    策略 C（结束最早）：[0,4) 再选 [5,14) -> 2 个");
Console.WriteLine($"    暴力枚举的最优解：2 个");
Console.WriteLine();

// ---------- 随机对拍 ----------
Console.WriteLine("  ---------- 随机数据对拍 ----------");
const int Trials = 2000;
const int MaxIntervals = 12;
const int MaxTime = 20;
var rng = new Random(20260918);

int okA = 0, okB = 0, okC = 0;
int firstFailA = -1, firstFailB = -1;

for (int t = 0; t < Trials; t++)
{
    var ivs = RandomIntervals(rng, rng.Next(4, MaxIntervals + 1), MaxTime);
    int best = BruteForceMaxIntervals(ivs);

    int a = ScheduleByStart(ivs).Count;
    int b = ScheduleByDuration(ivs).Count;
    int c = ScheduleByEnd(ivs).Count;

    if (a == best) okA++; else if (firstFailA < 0) firstFailA = t;
    if (b == best) okB++; else if (firstFailB < 0) firstFailB = t;
    if (c == best) okC++;
}

Console.WriteLine($"  随机生成 {Trials} 组区间（每组 4~{MaxIntervals} 个，时间范围 0~{MaxTime}），");
Console.WriteLine($"  每种策略都与【暴力枚举所有子集】的最优解对比：");
Console.WriteLine();
Console.WriteLine($"    策略 A（开始最早）：{okA,5} / {Trials} 组正确  （{okA / (double)Trials:P1}）");
Console.WriteLine($"    策略 B（时长最短）：{okB,5} / {Trials} 组正确  （{okB / (double)Trials:P1}）");
Console.WriteLine($"    策略 C（结束最早）：{okC,5} / {Trials} 组正确  （{okC / (double)Trials:P1}）");
Console.WriteLine();
Console.WriteLine("  策略 C 是 100%，A 和 B 都会出错 —— 但注意它们【不是每次都错】：");
Console.WriteLine("  这正是 14.1 节说的「贪心的错误是静默的」，靠随手试几个例子根本发现不了。");
Console.WriteLine();

// ==================== 实验二：Huffman 编码 ====================

Console.WriteLine("=== 实验二：Huffman 编码 ===");
Console.WriteLine();

const string Text = """
    the quick brown fox jumps over the lazy dog and then the dog jumps back over the fox
    while the quick brown fox keeps running through the forest and over the river
    a good algorithm is one that scales and a great algorithm is one that scales simply
    the best way to learn an algorithm is to implement it and then measure it
    data structures and algorithms are the tools of the trade for every engineer
    knowing when to use a heap and when to use a hash table is half the battle
    """;

var freq = new Dictionary<char, int>();
foreach (char ch in Text)
    if (char.IsLetter(ch) || ch == ' ')
        freq[ch] = freq.GetValueOrDefault(ch) + 1;

int totalChars = freq.Values.Sum();
Console.WriteLine($"  样本：一段 {totalChars} 个字符的英文（只保留字母和空格）");
Console.WriteLine($"  不同字符数（字母表大小）：{freq.Count}");
Console.WriteLine();

// ---------- 构造过程 ----------
Console.WriteLine("  ---------- Huffman 树的构造过程（每次合并频率最小的两个）----------");
var root = BuildHuffman(freq, verbose: true);
Console.WriteLine();

// ---------- 编码表 ----------
var codes = new Dictionary<char, string>();
BuildCodes(root, "", codes);

Console.WriteLine("  ---------- 编码表（按编码长度排序）----------");
Console.WriteLine("      字符    频率    编码              位数");
Console.WriteLine("      ----    ----    ----------------    ----");
int huffmanBits = 0;
foreach (var kv in codes.OrderBy(k => k.Value.Length).ThenByDescending(k => freq[k.Key]))
{
    char display = kv.Key == ' ' ? '␣' : kv.Key;
    Console.WriteLine($"      {display,-6}  {freq[kv.Key],4}    {kv.Value,-16}    {kv.Value.Length}");
    huffmanBits += freq[kv.Key] * kv.Value.Length;
}
Console.WriteLine();

// ---------- 前缀码性质 ----------
bool prefixFree = IsPrefixFree(codes);
Console.WriteLine($"  前缀码性质（没有任何一个编码是另一个的前缀）：{prefixFree}");
Console.WriteLine($"    这一条是【能唯一解码】的保证 —— 收到一串 0/1 时不用分隔符就能切开。");
Console.WriteLine();

// ---------- 压缩率 ----------
int alphabet = freq.Count;
int fixedBits = 1;
while ((1 << fixedBits) < alphabet) fixedBits++;      // ceil(log2(alphabet))
long fixedTotal = (long)fixedBits * totalChars;

double avgHuffman = huffmanBits / (double)totalChars;
Console.WriteLine("  ---------- 和定长编码比 ----------");
Console.WriteLine($"    字母表大小 {alphabet} -> 定长编码需要 {fixedBits} 位/字符");
Console.WriteLine($"    定长编码总位数：{fixedTotal:N0}");
Console.WriteLine($"    Huffman 总位数：{huffmanBits:N0}（平均 {avgHuffman:F3} 位/字符）");
Console.WriteLine($"    省下 {(fixedTotal - huffmanBits) / (double)fixedTotal:P1}");
Console.WriteLine();

// ---------- 理论上限 ----------
double entropy = 0;
foreach (var f in freq.Values)
{
    double p = f / (double)totalChars;
    entropy -= p * Math.Log2(p);
}
Console.WriteLine("  ---------- 还能再省吗？----------");
Console.WriteLine($"    这段文本的【一元熵】是 {entropy:F3} 位/字符 —— 这是「每次只看一个字符」时");
Console.WriteLine("    理论上不可能突破的下限（信息论，超出本书范围）。");
Console.WriteLine($"    Huffman 做到 {avgHuffman:F3} 位/字符，与它的差距只有 {avgHuffman - entropy:F3} 位。");
Console.WriteLine();
Console.WriteLine("  所以 Huffman 已经是【最优前缀码】了 —— 但「最优前缀码」离「最优压缩」还差得远：");
Console.WriteLine("  要再省，唯一的办法是换模型（比如一次看两个字符、或者用上下文预测），");
Console.WriteLine("  那是 ZIP/JPEG 里那些更复杂算法在做的事。");
Console.WriteLine();

// ==================== 算法实现 ====================

// ---------- 区间调度：三种策略 ----------

// 统一写法：传入【已按某种规则排好序】的区间表，依次挑选互不重叠的
static List<(int s, int e)> Pick(List<(int s, int e)> ivs)
{
    var result = new List<(int s, int e)>();
    int lastEnd = int.MinValue;
    foreach (var iv in ivs)
        if (iv.s >= lastEnd)
        {
            result.Add(iv);
            lastEnd = iv.e;
        }
    return result;
}

// 策略 A：开始时间最早
static List<(int s, int e)> ScheduleByStart(List<(int s, int e)> ivs)
    => Pick(ivs.OrderBy(x => x.s).ToList());

// 策略 B：持续时间最短
static List<(int s, int e)> ScheduleByDuration(List<(int s, int e)> ivs)
    => Pick(ivs.OrderBy(x => x.e - x.s).ToList());

// 策略 C：结束时间最早（正确的那个）
static List<(int s, int e)> ScheduleByEnd(List<(int s, int e)> ivs)
    => Pick(ivs.OrderBy(x => x.e).ToList());

// 暴力枚举：n 不大时枚举所有子集，返回最大不重叠个数
static int BruteForceMaxIntervals(List<(int s, int e)> ivs)
{
    int n = ivs.Count, best = 0;
    for (int mask = 0; mask < (1 << n); mask++)
    {
        var chosen = new List<(int s, int e)>();
        for (int i = 0; i < n; i++)
            if ((mask & (1 << i)) != 0) chosen.Add(ivs[i]);

        chosen.Sort();
        bool ok = true;
        for (int i = 1; i < chosen.Count; i++)
            if (chosen[i].s < chosen[i - 1].e) { ok = false; break; }

        if (ok && chosen.Count > best) best = chosen.Count;
    }
    return best;
}

static List<(int s, int e)> RandomIntervals(Random rng, int n, int maxTime)
{
    var list = new List<(int s, int e)>();
    for (int i = 0; i < n; i++)
    {
        int s = rng.Next(maxTime);
        int e = s + rng.Next(1, maxTime / 2 + 1);
        list.Add((s, e));
    }
    return list;
}

static void ShowIntervals(List<(int s, int e)> ivs)
{
    Console.Write("    区间：");
    Console.WriteLine(string.Join("  ", ivs.Select(x => $"[{x.s},{x.e})")));
}

// ---------- Huffman 编码 ----------

static HuffmanNode BuildHuffman(Dictionary<char, int> freq, bool verbose = false)
{
    var pq = new PriorityQueue<HuffmanNode, int>();
    foreach (var (ch, f) in freq)
        pq.Enqueue(new HuffmanNode { Ch = ch, Freq = f }, f);

    int step = 0;
    while (pq.Count > 1)
    {
        var a = pq.Dequeue();
        var b = pq.Dequeue();
        var parent = new HuffmanNode { Freq = a.Freq + b.Freq, Left = a, Right = b };
        pq.Enqueue(parent, parent.Freq);
        step++;

        if (verbose && step <= 12)
            Console.WriteLine($"    第 {step,2} 步：合并 {Describe(a),-14} 和 {Describe(b),-14}"
                + $" -> 新节点，频率 {parent.Freq}");
    }
    if (verbose && step > 12)
        Console.WriteLine($"    ...（共 {step} 步，最后剩一个根节点）");

    return pq.Dequeue();
}

static string Describe(HuffmanNode n)
    => n.Ch.HasValue ? $"{Display(n.Ch.Value)}({n.Freq})" : $"内部节点({n.Freq})";

static char Display(char c) => c == ' ' ? '␣' : c;

static void BuildCodes(HuffmanNode node, string prefix, Dictionary<char, string> codes)
{
    if (node.Ch.HasValue) { codes[node.Ch.Value] = prefix.Length == 0 ? "0" : prefix; return; }
    if (node.Left is not null) BuildCodes(node.Left, prefix + "0", codes);
    if (node.Right is not null) BuildCodes(node.Right, prefix + "1", codes);
}

static bool IsPrefixFree(Dictionary<char, string> codes)
{
    foreach (var a in codes)
        foreach (var b in codes)
            if (a.Key != b.Key && b.Value.StartsWith(a.Value, StringComparison.Ordinal))
                return false;
    return true;
}

// ---------- Huffman 树节点 ----------

public class HuffmanNode
{
    public char? Ch;                 // 只有叶子节点有字符
    public int Freq;
    public HuffmanNode? Left;
    public HuffmanNode? Right;
}
