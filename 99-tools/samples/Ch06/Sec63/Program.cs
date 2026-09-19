using System.Diagnostics;

// ==================== 实验一：负载因子如何影响开放寻址的性能 ====================

Console.WriteLine("=== 实验一：负载因子 -> 探测次数（开放寻址 / 线性探测）===");
Console.WriteLine();

const int Capacity = 200_000;
var rng = new Random(42);

var allKeys = new int[200_000];
for (int i = 0; i < allKeys.Length; i++) allKeys[i] = rng.Next(0, int.MaxValue);

// 造一批「肯定不存在」的键，用来测失败查找
var missKeys = new int[10_000];
for (int i = 0; i < missKeys.Length; i++) missKeys[i] = unchecked((int)0x80000000 + i);

Console.WriteLine($"  {"负载因子",10} | {"成功查找",10} | {"失败查找",10} | {"理论成功",10} | {"理论失败",10}");
Console.WriteLine(new string('-', 62));

foreach (double lf in new[] { 0.10, 0.25, 0.50, 0.60, 0.70, 0.80, 0.90, 0.95 })
{
    int count = (int)(Capacity * lf);
    var table = new OpenAddressHashTable(Capacity);
    for (int i = 0; i < count; i++) table.Put(allKeys[i], "x");

    // ---- 成功查找 ----
    table.ResetStats();
    for (int i = 0; i < count; i++) table.TryGet(allKeys[i], out _);
    double successProbes = table.TotalProbes / (double)table.OpCount;

    // ---- 失败查找 ----
    table.ResetStats();
    foreach (int k in missKeys) table.TryGet(k, out _);
    double failProbes = table.TotalProbes / (double)table.OpCount;

    // Knuth 的线性探测理论公式
    double theoSuccess = (1 + 1 / (1 - lf)) / 2;
    double theoFail = (1 + 1 / Math.Pow(1 - lf, 2)) / 2;

    Console.WriteLine($"{lf,10:F2} | {successProbes,10:F2} | {failProbes,10:F2} | {theoSuccess,10:F2} | {theoFail,10:F2}");
}
Console.WriteLine();
Console.WriteLine("  先看「成功查找」那一列是怎么涨的（下列为 Knuth 理论值，与实测高度吻合）：");
Console.WriteLine("    负载 0.10 -> 1.06 次    负载 0.50 -> 1.50 次    负载 0.80 -> 3.00 次");
Console.WriteLine("    负载 0.90 -> 5.50 次    负载 0.95 -> 10.50 次");
Console.WriteLine();
Console.WriteLine("  从 0.80 到 0.95，负载因子只涨了 19%，探测次数却涨了 4 倍！");
Console.WriteLine("  这就是为什么开放寻址【必须】留出空位 —— 曲线在接近 1 的地方是垂直上升的。");
Console.WriteLine();
Console.WriteLine("  再看「失败查找」：负载 0.95 时要探测 200 次以上。");
Console.WriteLine("  失败查找比成功查找贵得多，因为它必须走完整条探测链才能确认「不存在」。");
Console.WriteLine();

// ==================== 实验二：可视化这条曲线 ====================

Console.WriteLine("=== 实验二：把曲线画出来 ===");
Console.WriteLine();
Console.WriteLine("  探测次数（失败查找）随负载因子的变化：");
Console.WriteLine();

foreach (double lf in new[] { 0.10, 0.25, 0.50, 0.60, 0.70, 0.80, 0.90, 0.95 })
{
    double theoFail = (1 + 1 / Math.Pow(1 - lf, 2)) / 2;
    int barLength = (int)Math.Min(theoFail, 60);
    Console.WriteLine($"    负载 {lf:F2} | {new string('#', barLength)} {theoFail:F1}");
}
Console.WriteLine();
Console.WriteLine("  形状很明显：前半段几乎是平的，到 0.8 之后开始陡峭，0.9 之后几乎垂直。");
Console.WriteLine("  这就是负载因子的「悬崖效应」—— 这也是 6.2 节说「控制在 0.5~0.75」的原因。");
Console.WriteLine();

// ==================== 实验三：扩容与再哈希 ====================

Console.WriteLine("=== 实验三：扩容与再哈希 ===");
Console.WriteLine();

// 预热：让 JIT 把 GrowingHashTable 编译好，否则前两次插入会带上编译开销
{
    var warm = new GrowingHashTable(16);
    for (int i = 0; i < 10_000; i++) warm.Put(i, "x");
}

var growing = new GrowingHashTable(4);
Console.WriteLine($"  {"插入第几个",12} | {"容量",8} | {"耗时",10} | 说明");
Console.WriteLine(new string('-', 62));

for (int i = 1; i <= 20; i++)
{
    long before = growing.ResizeCount;
    var sw = Stopwatch.StartNew();
    growing.Put(i * 7, $"值{i}");
    sw.Stop();

    string note = "";
    if (growing.ResizeCount > before)
        note = $"<- 触发扩容！重新哈希了 {growing.LastRehashCount} 个元素，容量 {growing.LastOldCapacity} -> {growing.Capacity}";

    Console.WriteLine($"{i,12} | {growing.Capacity,8} | {sw.Elapsed.TotalMicroseconds,8:F1} us | {note}");
}
Console.WriteLine();
Console.WriteLine("  注意两点：");
Console.WriteLine("    1. 绝大多数插入是 O(1) 的（耗时都是零点几微秒）；");
Console.WriteLine("    2. 偶尔有一次插入明显更慢 —— 那就是扩容，它要把所有元素重新哈希到新表里。");
Console.WriteLine();

// 大规模统计摊还代价
const int N = 1_000_000;
var big = new GrowingHashTable(16);
var swBig = Stopwatch.StartNew();
for (int i = 0; i < N; i++) big.Put(i, "x");
swBig.Stop();

Console.WriteLine($"  插入 {N:N0} 个元素（初始容量 16）：");
Console.WriteLine($"    总耗时        : {swBig.Elapsed.TotalMilliseconds:F1} ms");
Console.WriteLine($"    扩容次数      : {big.ResizeCount}");
Console.WriteLine($"    最终容量      : {big.Capacity:N0}");
Console.WriteLine($"    累计重新哈希  : {big.TotalRehashed:N0} 个元素");
Console.WriteLine($"    平均每个元素重新哈希 {big.TotalRehashed / (double)N:F2} 次");
Console.WriteLine();
Console.WriteLine("  累计重新哈希量小于 2n —— 和 3.2 节动态数组扩容是同一个道理：");
Console.WriteLine("  倍增策略让总搬运量构成等比数列，求和后被最后一项主导，所以摊还下来是 O(1)。");
Console.WriteLine();

// ==================== 实验四：预分配容量的收益 ====================

Console.WriteLine("=== 实验四：预分配容量的收益 ===");
Console.WriteLine();

var sw1 = Stopwatch.StartNew();
var noPre = new GrowingHashTable(16);
for (int i = 0; i < N; i++) noPre.Put(i, "x");
sw1.Stop();

var sw2 = Stopwatch.StartNew();
var pre = new GrowingHashTable(N * 2);       // 按 0.5 负载因子预分配
for (int i = 0; i < N; i++) pre.Put(i, "x");
sw2.Stop();

Console.WriteLine($"  不预分配（从容量 16 开始）: {sw1.Elapsed.TotalMilliseconds,8:F1} ms   扩容 {noPre.ResizeCount,2} 次，重哈希 {noPre.TotalRehashed,12:N0} 个");
Console.WriteLine($"  预分配（容量 {N * 2:N0}）  : {sw2.Elapsed.TotalMilliseconds,8:F1} ms   扩容 {pre.ResizeCount,2} 次，重哈希 {pre.TotalRehashed,12:N0} 个");
Console.WriteLine($"  预分配快 {sw1.Elapsed.TotalMilliseconds / sw2.Elapsed.TotalMilliseconds:F2} 倍");
Console.WriteLine();
Console.WriteLine("  和 3.2 节 List<T> 的结论一致：知道规模就预分配，一行改动换来明显收益。");
Console.WriteLine("  Dictionary 的构造函数也接受 capacity 参数，用途完全相同。");
Console.WriteLine();

// ==================== 开放寻址哈希表（带统计重置） ====================

public class OpenAddressHashTable
{
    private enum SlotState : byte { Empty, Occupied, Tombstone }

    private readonly int[] _keys;
    private readonly string[] _values;
    private readonly SlotState[] _states;
    private int _count;

    public long TotalProbes { get; private set; }
    public long OpCount { get; private set; }

    public OpenAddressHashTable(int capacity)
    {
        _keys = new int[capacity];
        _values = new string[capacity];
        _states = new SlotState[capacity];
    }

    public void ResetStats() { TotalProbes = 0; OpCount = 0; }

    private int BucketIndex(int key) => (key & 0x7FFFFFFF) % _keys.Length;

    public void Put(int key, string value)
    {
        int idx = BucketIndex(key);
        int firstTombstone = -1;

        for (int step = 0; step < _keys.Length; step++)
        {
            int pos = (idx + step) % _keys.Length;

            if (_states[pos] == SlotState.Occupied)
            {
                if (_keys[pos] == key) { _values[pos] = value; return; }
                continue;
            }
            if (_states[pos] == SlotState.Tombstone)
            {
                if (firstTombstone < 0) firstTombstone = pos;
                continue;
            }

            int target = firstTombstone >= 0 ? firstTombstone : pos;
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

            if (_states[pos] == SlotState.Empty) { value = null; return false; }
            if (_states[pos] == SlotState.Occupied && _keys[pos] == key)
            {
                value = _values[pos];
                return true;
            }
        }
        value = null;
        return false;
    }
}

// ==================== 自动扩容的哈希表 ====================

/// <summary>带自动扩容（倍增 + 再哈希）的链地址法哈希表。</summary>
public class GrowingHashTable
{
    private LinkedList<KeyValuePair<int, string>>?[] _buckets;
    private int _count;

    public int ResizeCount { get; private set; }
    public int LastOldCapacity { get; private set; }
    public int LastRehashCount { get; private set; }
    public long TotalRehashed { get; private set; }

    public int Capacity => _buckets.Length;
    public int Count => _count;
    public double LoadFactor => _count / (double)_buckets.Length;

    private const double MaxLoadFactor = 0.75;

    public GrowingHashTable(int capacity)
    {
        _buckets = new LinkedList<KeyValuePair<int, string>>?[Math.Max(capacity, 4)];
    }

    private int BucketIndex(int key, int capacity)
        => (key & 0x7FFFFFFF) % capacity;

    public void Put(int key, string value)
    {
        if ((_count + 1) / (double)_buckets.Length > MaxLoadFactor)
            Resize();

        int idx = BucketIndex(key, _buckets.Length);
        _buckets[idx] ??= new LinkedList<KeyValuePair<int, string>>();
        var chain = _buckets[idx]!;

        for (var node = chain.First; node != null; node = node.Next)
        {
            if (node.Value.Key == key)
            {
                node.Value = new KeyValuePair<int, string>(key, value);
                return;
            }
        }

        chain.AddLast(new KeyValuePair<int, string>(key, value));
        _count++;
    }

    public bool TryGet(int key, out string? value)
    {
        var chain = _buckets[BucketIndex(key, _buckets.Length)];
        if (chain != null)
        {
            for (var node = chain.First; node != null; node = node.Next)
                if (node.Value.Key == key) { value = node.Value.Value; return true; }
        }
        value = null;
        return false;
    }

    /// <summary>扩容：容量翻倍，把所有元素重新哈希到新表。</summary>
    private void Resize()
    {
        int oldCapacity = _buckets.Length;
        var oldBuckets = _buckets;

        _buckets = new LinkedList<KeyValuePair<int, string>>?[oldCapacity * 2];

        int moved = 0;
        foreach (var chain in oldBuckets)
        {
            if (chain == null) continue;
            foreach (var kv in chain)
            {
                int idx = BucketIndex(kv.Key, _buckets.Length);   // 用新容量重新算下标
                _buckets[idx] ??= new LinkedList<KeyValuePair<int, string>>();
                _buckets[idx]!.AddLast(kv);
                moved++;
            }
        }

        ResizeCount++;
        LastOldCapacity = oldCapacity;
        LastRehashCount = moved;
        TotalRehashed += moved;
    }
}
