using System.Diagnostics;

// ==================== 实验一：冲突是不可避免的 ====================

Console.WriteLine("=== 实验一：冲突为什么无法避免 ===");
Console.WriteLine();

Console.WriteLine("  想做一个存储「用户名 -> 年龄」的哈希表。键是字符串。");
Console.WriteLine();
Console.WriteLine("  字符串的可能取值：几乎无限（长度不限、字符不限）");
Console.WriteLine("  数组的下标范围  ：有限（比如 1000 个桶）");
Console.WriteLine();
Console.WriteLine("  把无限多的东西映射到有限多的位置 —— 根据【鸽巢原理】，");
Console.WriteLine("  必然有不止一个键落到同一个位置。");
Console.WriteLine();
Console.WriteLine("  这不是哈希函数写得不好，而是数学上不可能避免。");
Console.WriteLine("  所以哈希表的设计重点不是「怎么避免冲突」，而是「冲突了怎么办」。");
Console.WriteLine();

// ==================== 实验二：链地址法 ====================

Console.WriteLine("=== 实验二：链地址法（Separate Chaining）===");
Console.WriteLine();

// 容量 8192、5000 个元素 -> 负载因子约 0.61
const int Capacity = 8192;
const int N = 5_000;

var chained = new ChainedHashTable(Capacity);
var rng = new Random(42);

for (int i = 0; i < N; i++)
{
    int key = rng.Next(0, 1_000_000);
    chained.Put(key, $"值{key}");
}

chained.PrintStats();

// 验证正确性
int okCount = 0, failCount = 0;
rng = new Random(42);
for (int i = 0; i < N; i++)
{
    int key = rng.Next(0, 1_000_000);
    if (chained.TryGet(key, out string? v) && v == $"值{key}") okCount++;
    else failCount++;
}
Console.WriteLine($"  正确性校验: 命中 {okCount:N0} 次，失败 {failCount} 次");
Console.WriteLine();

// ==================== 实验三：开放寻址（线性探测） ====================

Console.WriteLine("=== 实验三：开放寻址 —— 线性探测（Linear Probing）===");
Console.WriteLine();

// 同样的容量和元素个数 —— 但注意：开放寻址法【必须】有足够多的空槽位
var open = new OpenAddressHashTable(Capacity);
rng = new Random(42);
for (int i = 0; i < N; i++)
{
    int key = rng.Next(0, 1_000_000);
    open.Put(key, $"值{key}");
}

open.PrintStats();

okCount = 0; failCount = 0;
rng = new Random(42);
for (int i = 0; i < N; i++)
{
    int key = rng.Next(0, 1_000_000);
    if (open.TryGet(key, out string? v) && v == $"值{key}") okCount++;
    else failCount++;
}
Console.WriteLine($"  正确性校验: 命中 {okCount:N0} 次，失败 {failCount} 次");
Console.WriteLine();

// ==================== 实验四：性能对比 ====================

Console.WriteLine("=== 实验四：两种方案的性能对比 ===");
Console.WriteLine();

const int M = 200_000;
const int OpCount = 200_000;

var chained2 = new ChainedHashTable(1 << 18);      // 262144 个桶
var open2 = new OpenAddressHashTable(1 << 19);     // 缓存友好，但需要更多空间

rng = new Random(42);
var keys = new int[M];
for (int i = 0; i < M; i++) keys[i] = rng.Next(0, 100_000_000);

var sw = Stopwatch.StartNew();
foreach (int k in keys) chained2.Put(k, "x");
foreach (int k in keys) chained2.TryGet(k, out _);
sw.Stop();
double chainedMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
foreach (int k in keys) open2.Put(k, "x");
foreach (int k in keys) open2.TryGet(k, out _);
sw.Stop();
double openMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  {M:N0} 次插入 + {M:N0} 次查找：");
Console.WriteLine($"    链地址法: {chainedMs,8:F1} ms   平均探测 {chained2.TotalProbes / (double)(chained2.OpCount),5:F2} 次/操作");
Console.WriteLine($"    线性探测: {openMs,8:F1} ms   平均探测 {open2.TotalProbes / (double)(open2.OpCount),5:F2} 次/操作");
Console.WriteLine();
Console.WriteLine("  线性探测更快，原因是「内存局部性」：");
Console.WriteLine("    它只用一个数组，探测的几个位置就在附近，大概率在同一条缓存行上；");
Console.WriteLine("    链地址法要跳转到链表节点，每个节点都是一次可能的内存跳转。");
Console.WriteLine();

Console.WriteLine("  但开放寻址有一个【硬约束】：负载因子必须小于 1。");
Console.WriteLine("  因为它把所有元素都存在同一个数组里 —— 元素比槽位多，就装不下了。");
Console.WriteLine("  验证一下：");
try
{
    var tooSmall = new OpenAddressHashTable(64);
    for (int i = 0; i < 100; i++) tooSmall.Put(i * 1000, "x");
    Console.WriteLine("    (没有报错？)");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"    容量 64 的开放寻址表插入 100 个元素 -> 抛出异常: \"{ex.Message}\"");
}
Console.WriteLine();
Console.WriteLine("  链地址法没有这个限制：负载因子 5 也能跑（只是链表变长、查找变慢）。");
Console.WriteLine("  这是两者在设计上的根本差别：");
Console.WriteLine("    链地址法把「超出的部分」挂到链表上，空间可以超卖；");
Console.WriteLine("    开放寻址法只能在数组内部腾挪，必须预留空位。");
Console.WriteLine();

// ==================== 实验五：删除的麻烦 ====================

Console.WriteLine("=== 实验五：开放寻址的删除问题 ===");
Console.WriteLine();

var delTest = new OpenAddressHashTable(8);
delTest.Put(1, "A");
delTest.Put(9, "B");        // 假设 1 和 9 哈希到同一个桶（8 个桶时，1 和 9 同余）
delTest.Put(17, "C");

Console.WriteLine($"  插入 1、9、17 后（容量 8，它们都哈希到下标 1）：");
delTest.PrintBuckets();

delTest.Remove(9);
Console.WriteLine();
Console.WriteLine($"  删除 9 之后：");
delTest.PrintBuckets();
Console.WriteLine();

bool found1 = delTest.TryGet(1, out string? v1);
bool found17 = delTest.TryGet(17, out string? v17);
Console.WriteLine($"  还能查到 1 吗？  {found1}  (值 {v1})");
Console.WriteLine($"  还能查到 17 吗？ {found17}  (值 {v17})");
Console.WriteLine();
Console.WriteLine("  关键点：删除 9 时【不能直接把槽位置空】，否则查找 17 时会在这里停下，");
Console.WriteLine("  误以为 17 不存在。必须留下一个【墓碑标记】，告诉查找过程「这里曾经有东西，继续往后找」。");
Console.WriteLine();

// ==================== 链地址法实现 ====================

/// <summary>链地址法哈希表：每个桶挂一条链表。</summary>
public class ChainedHashTable
{
    private readonly LinkedList<KeyValuePair<int, string>>?[] _buckets;
    private int _count;

    public long TotalProbes { get; private set; }     // 累计比较次数
    public long OpCount { get; private set; }         // 累计操作次数
    public int MaxChainLength { get; private set; }

    public ChainedHashTable(int capacity)
    {
        _buckets = new LinkedList<KeyValuePair<int, string>>?[capacity];
    }

    private int BucketIndex(int key)
    {
        int h = key.GetHashCode();
        return (h & 0x7FFFFFFF) % _buckets.Length;    // 去掉符号位再取模
    }

    public void Put(int key, string value)
    {
        int idx = BucketIndex(key);
        OpCount++;

        _buckets[idx] ??= new LinkedList<KeyValuePair<int, string>>();
        var chain = _buckets[idx]!;

        for (var node = chain.First; node != null; node = node.Next)
        {
            TotalProbes++;
            if (node.Value.Key == key)
            {
                node.Value = new KeyValuePair<int, string>(key, value);   // 已存在则覆盖
                return;
            }
        }

        chain.AddLast(new KeyValuePair<int, string>(key, value));
        _count++;
        if (chain.Count > MaxChainLength) MaxChainLength = chain.Count;
    }

    public bool TryGet(int key, out string? value)
    {
        int idx = BucketIndex(key);
        OpCount++;

        var chain = _buckets[idx];
        if (chain != null)
        {
            for (var node = chain.First; node != null; node = node.Next)
            {
                TotalProbes++;
                if (node.Value.Key == key) { value = node.Value.Value; return true; }
            }
        }
        value = null;
        return false;
    }

    public void PrintStats()
    {
        int used = 0, empty = 0;
        for (int i = 0; i < _buckets.Length; i++)
        {
            if (_buckets[i] == null) empty++;
            else used++;
        }

        Console.WriteLine($"  桶数量        : {_buckets.Length:N0}");
        Console.WriteLine($"  元素个数      : {_count:N0}");
        Console.WriteLine($"  负载因子      : {_count / (double)_buckets.Length:F3}");
        Console.WriteLine($"  有元素的桶    : {used:N0}   空桶: {empty:N0}");
        Console.WriteLine($"  最长链长度    : {MaxChainLength}");
        Console.WriteLine($"  平均探测次数  : {TotalProbes / (double)OpCount:F3} 次/操作");
        Console.WriteLine();
    }
}

// ==================== 开放寻址（线性探测）实现 ====================

/// <summary>
/// 开放寻址哈希表（线性探测）：所有元素都存在同一个数组里。
/// 冲突时就往后找下一个空位。删除时留下「墓碑」。
/// </summary>
public class OpenAddressHashTable
{
    private enum SlotState : byte { Empty, Occupied, Tombstone }

    private readonly int[] _keys;
    private readonly string[] _values;
    private readonly SlotState[] _states;
    private int _count;
    private int _tombstones;

    public long TotalProbes { get; private set; }
    public long OpCount { get; private set; }

    public OpenAddressHashTable(int capacity)
    {
        _keys = new int[capacity];
        _values = new string[capacity];
        _states = new SlotState[capacity];
    }

    private int BucketIndex(int key)
    {
        int h = key.GetHashCode();
        return (h & 0x7FFFFFFF) % _keys.Length;
    }

    public void Put(int key, string value)
    {
        int idx = BucketIndex(key);
        int firstTombstone = -1;
        OpCount++;

        // 线性探测：从 idx 开始往后找
        for (int step = 0; step < _keys.Length; step++)
        {
            int pos = (idx + step) % _keys.Length;
            TotalProbes++;

            if (_states[pos] == SlotState.Occupied)
            {
                if (_keys[pos] == key)
                {
                    _values[pos] = value;              // 已存在，覆盖
                    return;
                }
                continue;                              // 被别人占了，继续往后
            }

            if (_states[pos] == SlotState.Tombstone)
            {
                if (firstTombstone < 0) firstTombstone = pos;   // 记下第一个墓碑，可复用
                continue;
            }

            // 找到空位
            int target = firstTombstone >= 0 ? firstTombstone : pos;
            if (firstTombstone >= 0) _tombstones--;
            _keys[target] = key;
            _values[target] = value;
            _states[target] = SlotState.Occupied;
            _count++;
            return;
        }

        throw new InvalidOperationException("哈希表已满");
    }

    public bool TryGet(int key, out string? value)
    {
        int idx = BucketIndex(key);
        OpCount++;

        for (int step = 0; step < _keys.Length; step++)
        {
            int pos = (idx + step) % _keys.Length;
            TotalProbes++;

            if (_states[pos] == SlotState.Empty)
            {
                // 遇到真正的空位才能断定「不存在」；墓碑不能作为停止依据
                value = null;
                return false;
            }

            if (_states[pos] == SlotState.Occupied && _keys[pos] == key)
            {
                value = _values[pos];
                return true;
            }
        }

        value = null;
        return false;
    }

    public bool Remove(int key)
    {
        int idx = BucketIndex(key);

        for (int step = 0; step < _keys.Length; step++)
        {
            int pos = (idx + step) % _keys.Length;

            if (_states[pos] == SlotState.Empty) return false;

            if (_states[pos] == SlotState.Occupied && _keys[pos] == key)
            {
                _states[pos] = SlotState.Tombstone;    // 立墓碑，而不是清空
                _values[pos] = null!;
                _count--;
                _tombstones++;
                return true;
            }
        }
        return false;
    }

    public void PrintStats()
    {
        int occ = 0, empty = 0, tomb = 0;
        for (int i = 0; i < _states.Length; i++)
        {
            if (_states[i] == SlotState.Occupied) occ++;
            else if (_states[i] == SlotState.Empty) empty++;
            else tomb++;
        }

        Console.WriteLine($"  槽位数量      : {_states.Length:N0}");
        Console.WriteLine($"  元素个数      : {_count:N0}");
        Console.WriteLine($"  负载因子      : {_count / (double)_states.Length:F3}");
        Console.WriteLine($"  已占用/空/墓碑: {occ:N0} / {empty:N0} / {tomb:N0}");
        Console.WriteLine($"  平均探测次数  : {TotalProbes / (double)OpCount:F3} 次/操作");
        Console.WriteLine();
    }

    public void PrintBuckets()
    {
        Console.Write("    ");
        for (int i = 0; i < _states.Length; i++)
        {
            string mark = _states[i] switch
            {
                SlotState.Occupied => $"k{_keys[i]}",
                SlotState.Tombstone => "墓碑",
                _ => "_"
            };
            Console.Write($"[{i}:{mark}] ");
        }
        Console.WriteLine();
    }
}
