using System.Diagnostics;

// ============ 实验一：自己实现的动态数组，观察扩容过程 ============

Console.WriteLine("=== 实验一：MyList 的扩容过程 ===");

var myList = new MyList<int>();
Console.WriteLine($"初始容量: {myList.Capacity}");
Console.WriteLine();

int applied = 0;
int lastCapacity = myList.Capacity;

for (int i = 0; i < 1_000_000; i++)
{
    myList.Add(i);
    if (myList.Capacity != lastCapacity)
    {
        applied++;
        if (applied <= 8 || myList.Capacity >= 262_144)
            Console.WriteLine($"  第 {myList.Count,9:N0} 个元素时扩容: {lastCapacity,9:N0} -> {myList.Capacity,9:N0}");
        lastCapacity = myList.Capacity;
    }
}

Console.WriteLine();
Console.WriteLine($"总插入次数      : {myList.Count,12:N0}");
Console.WriteLine($"总扩容次数      : {myList.GrowCount,12:N0}");
Console.WriteLine($"总搬运元素次数  : {myList.TotalMoved,12:N0}");
Console.WriteLine($"平均每次插入搬运: {(double)myList.TotalMoved / myList.Count,12:F3} 次");
Console.WriteLine();
Console.WriteLine($"理论：等比数列 4+8+16+... 的和 < 2n = {2_000_000:N0}");
Console.WriteLine($"      实测搬运量 {myList.TotalMoved:N0}，确实小于 2n -> 摊还 O(1) 成立");
Console.WriteLine();

// ============ 实验二：正确性验证 ============

Console.WriteLine("=== 实验二：和官方 List<T> 逐项对比 ===");

var mine = new MyList<int>();
var official = new List<int>();
var rng = new Random(42);

for (int i = 0; i < 100_000; i++)
{
    int v = rng.Next(0, 1_000_000);
    mine.Add(v);
    official.Add(v);
}

bool sameCount = mine.Count == official.Count;
bool sameContent = true;
for (int i = 0; i < mine.Count; i++)
{
    if (mine[i] != official[i]) { sameContent = false; break; }
}

Console.WriteLine($"  元素个数一致: {sameCount}  ({mine.Count:N0} vs {official.Count:N0})");
Console.WriteLine($"  逐项内容一致: {sameContent}");
Console.WriteLine();

// ============ 实验三：预分配容量的收益 ============

Console.WriteLine("=== 实验三：预分配容量 vs 让它自己扩容 ===");

const int N = 10_000_000;
var sw = new Stopwatch();

// 预热
{
    var warm = new MyList<int>();
    for (int i = 0; i < 100_000; i++) warm.Add(i);
}

sw.Restart();
var auto = new MyList<int>();
for (int i = 0; i < N; i++) auto.Add(i);
sw.Stop();
double autoMs = sw.Elapsed.TotalMilliseconds;

sw.Restart();
var pre = new MyList<int>(N);                 // 提前告诉它要装多少
for (int i = 0; i < N; i++) pre.Add(i);
sw.Stop();
double preMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  不预分配（{N:N0} 个元素）: {autoMs,9:F2} ms   扩容 {auto.GrowCount,3} 次，搬运 {auto.TotalMoved,12:N0} 次");
Console.WriteLine($"  预分配  （容量 {N:N0}）: {preMs,9:F2} ms   扩容 {pre.GrowCount,3} 次，搬运 {pre.TotalMoved,12:N0} 次");
Console.WriteLine($"  预分配快 {autoMs / preMs:F2} 倍");
Console.WriteLine();

// 类声明必须放在顶级语句之后
public class MyList<T>
{
    private T[] _items;
    private int _count;

    public MyList(int capacity = 0)
    {
        _items = capacity > 0 ? new T[capacity] : Array.Empty<T>();
    }

    public int Count => _count;
    public int Capacity => _items.Length;

    /// <summary>扩容次数，仅用于本节的统计演示</summary>
    public int GrowCount { get; private set; }

    /// <summary>累计搬运的元素个数，仅用于本节的统计演示</summary>
    public long TotalMoved { get; private set; }

    public void Add(T item)
    {
        if (_count == _items.Length)          // 装满了，先扩容
            Grow();
        _items[_count] = item;
        _count++;
    }

    private void Grow()
    {
        int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;   // 倍增
        var newItems = new T[newCapacity];
        Array.Copy(_items, newItems, _count);                            // 把老元素搬过去
        TotalMoved += _count;
        GrowCount++;
        _items = newItems;
    }

    public T this[int index]
    {
        get
        {
            // 用 (uint) 转换可以一次性挡掉负数和越界两种情况
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _items[index];
        }
        set
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _items[index] = value;
        }
    }
}
