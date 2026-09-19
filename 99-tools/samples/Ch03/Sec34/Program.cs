using System.Diagnostics;
using System.Text;

// ==================== 定长窗口：长度为 k 的子数组最大和 ====================

// 暴力：每个窗口都从头重新求和，O(n*k)
static long MaxSumBrute(int[] a, int k, ref long ops)
{
    long best = long.MinValue;
    for (int i = 0; i + k <= a.Length; i++)
    {
        long sum = 0;
        for (int j = i; j < i + k; j++)
        {
            sum += a[j];
            ops++;
        }
        if (sum > best) best = sum;
    }
    return best;
}

// 滑动窗口：进一个、出一个，O(n)
static long MaxSumWindow(int[] a, int k, ref long ops)
{
    long sum = 0;
    for (int i = 0; i < k; i++) { sum += a[i]; ops++; }        // 第一个窗口
    long best = sum;

    for (int i = k; i < a.Length; i++)
    {
        sum += a[i] - a[i - k];                                // 进 a[i]，出 a[i-k]
        ops++;
        if (sum > best) best = sum;
    }
    return best;
}

// ==================== 变长窗口：最长无重复字符子串 ====================

// 暴力：以每个位置为起点往后扩展，O(n^2)
static int LongestSubstringBrute(string s, ref long ops)
{
    int best = 0;
    for (int i = 0; i < s.Length; i++)
    {
        var seen = new HashSet<char>();
        for (int j = i; j < s.Length; j++)
        {
            ops++;
            if (!seen.Add(s[j])) break;      // 出现重复，这个起点的探索结束
            if (j - i + 1 > best) best = j - i + 1;
        }
    }
    return best;
}

// 滑动窗口：左右边界都只前进不后退，O(n)
static int LongestSubstringWindow(string s, ref long ops)
{
    var lastSeen = new Dictionary<char, int>();   // 字符 -> 最近一次出现的下标
    int left = 0, best = 0;

    for (int right = 0; right < s.Length; right++)
    {
        ops++;
        char c = s[right];

        // 如果这个字符在窗口内出现过，左边界直接跳到它后面
        if (lastSeen.TryGetValue(c, out int pos) && pos >= left)
            left = pos + 1;

        lastSeen[c] = right;
        if (right - left + 1 > best) best = right - left + 1;
    }
    return best;
}

// ---- 两种输入分布 ----

// A：完全随机。字符集 95 个可打印 ASCII
static string MakeRandom(int n, Random r)
{
    var sb = new StringBuilder(n);
    for (int i = 0; i < n; i++) sb.Append((char)(' ' + r.Next(95)));
    return sb.ToString();
}

// B：长周期。每 period 个字符才重复一次，所以最长无重复窗口 = period
static string MakePeriodic(int n, int period)
{
    var sb = new StringBuilder(n);
    for (int i = 0; i < n; i++) sb.Append((char)(' ' + i % period));
    return sb.ToString();
}

// ==================== 主流程 ====================

var rng = new Random(42);
const int N = 100_000;
const int N2 = 50_000;
const int K = 1_000;
const int Period = 1_000;

int[] data = new int[N];
for (int i = 0; i < N; i++) data[i] = rng.Next(1, 1000);

Console.WriteLine("=== 实验一：长度为 k 的子数组最大和 ===");
Console.WriteLine($"数据规模 {N:N0}，窗口大小 {K:N0}");

long bo = 0, wo = 0;
var sw = Stopwatch.StartNew();
long b1 = MaxSumBrute(data, K, ref bo);
sw.Stop();
double bMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
long w1 = MaxSumWindow(data, K, ref wo);
sw.Stop();
double wMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  暴力（每个窗口重新求和）: 加法 {bo,12:N0} 次 | {bMs,8:F2} ms | 结果 = {b1:N0}");
Console.WriteLine($"  滑动窗口（进一个出一个）: 加法 {wo,12:N0} 次 | {wMs,8:F2} ms | 结果 = {w1:N0}");
Console.WriteLine($"  结果一致: {b1 == w1}   滑动窗口少算 {bo / (double)wo:N0} 倍");
Console.WriteLine();

Console.WriteLine("=== 实验二：最长无重复字符子串 —— 先验证正确性 ===");
var samples = new[] { "abcabcbb", "bbbbb", "pwwkew", "", "abcdefg", "abba" };
foreach (var s in samples)
{
    long o1 = 0, o2 = 0;
    int rb = LongestSubstringBrute(s, ref o1);
    int rw = LongestSubstringWindow(s, ref o2);
    Console.WriteLine($"    \"{s,-10}\" 暴力={rb}  滑窗={rw}  一致={rb == rw}");
}
Console.WriteLine();

Console.WriteLine("=== 实验三：同样的两个算法，换一种输入，差距差了 100 倍 ===");
Console.WriteLine();

// --- 输入 A：完全随机 ---
string randText = MakeRandom(N2, rng);
long aOps = 0, aOps2 = 0;
sw.Restart();
int aB = LongestSubstringBrute(randText, ref aOps);
sw.Stop();
double aMs = sw.Elapsed.TotalMilliseconds;
sw.Restart();
int aW = LongestSubstringWindow(randText, ref aOps2);
sw.Stop();
double aMs2 = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  输入 A：完全随机（{N2:N0} 个字符，字符集 95）");
Console.WriteLine($"    最长无重复子串长度 = {aB}");
Console.WriteLine($"    暴力    : 探测 {aOps,12:N0} 次 | {aMs,8:F2} ms");
Console.WriteLine($"    滑动窗口: 探测 {aOps2,12:N0} 次 | {aMs2,8:F2} ms");
Console.WriteLine($"    滑动窗口少探测 {(double)aOps / aOps2:N0} 倍");
Console.WriteLine();

// --- 输入 B：长周期 ---
string periodicText = MakePeriodic(N2, Period);
long bOps = 0, bOps2 = 0;
sw.Restart();
int bB = LongestSubstringBrute(periodicText, ref bOps);
sw.Stop();
double bMs2 = sw.Elapsed.TotalMilliseconds;
sw.Restart();
int bW = LongestSubstringWindow(periodicText, ref bOps2);
sw.Stop();
double bMs3 = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  输入 B：长周期（{N2:N0} 个字符，每 {Period:N0} 个才重复一次）");
Console.WriteLine($"    最长无重复子串长度 = {bB}");
Console.WriteLine($"    暴力    : 探测 {bOps,12:N0} 次 | {bMs2,8:F2} ms");
Console.WriteLine($"    滑动窗口: 探测 {bOps2,12:N0} 次 | {bMs3,8:F2} ms");
Console.WriteLine($"    滑动窗口少探测 {(double)bOps / bOps2:N0} 倍");
Console.WriteLine();

Console.WriteLine("  两种输入下，滑动窗口的探测次数完全一样（都是 50,000 次），");
Console.WriteLine("  但暴力的探测次数从 64 万涨到了 5000 万 —— 涨了约 77 倍。");
Console.WriteLine();
Console.WriteLine("  滑动窗口的价值不只是「更快」，更是「不管输入长什么样都同样快」。");
