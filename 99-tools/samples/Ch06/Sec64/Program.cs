using System.Diagnostics;

static void PrintDict(string label, Dictionary<int, string> d)
{
    Console.Write($"  {label,-22}: ");
    foreach (var kv in d) Console.Write($"{kv.Key} ");
    Console.WriteLine();
}

// ==================== 实验一：Dictionary 的顺序不可依赖 ====================

Console.WriteLine("=== 实验一：Dictionary 的遍历顺序是不确定的 ===");
Console.WriteLine();

var d1 = new Dictionary<int, string>();
for (int i = 1; i <= 5; i++) d1[i] = $"v{i}";
PrintDict("插入 1,2,3,4,5", d1);
Console.WriteLine();

d1.Remove(3);
PrintDict("删除 3 之后", d1);

d1[6] = "v6";
PrintDict("再插入 6 之后", d1);
Console.WriteLine();

Console.WriteLine("  看到没有：6 顶替了 3 原来的位置，所以顺序变成了 1 2 6 4 5。");
Console.WriteLine();
Console.WriteLine("  【结论】Dictionary 不保证任何顺序 —— 既不是插入顺序，也不是排序顺序。");
Console.WriteLine("  它内部是一个开放寻址的数组（6.2 节），元素在数组里的位置由哈希值决定，");
Console.WriteLine("  删除会留下墓碑，新元素会优先复用墓碑 —— 顺序自然就乱了。");
Console.WriteLine();
Console.WriteLine("  如果你需要顺序，用：");
Console.WriteLine("    - SortedDictionary / SortedSet：按【键】排序（内部是红黑树，操作是 O(log n)）");
Console.WriteLine("    - List<T> + 自己维护：按插入顺序（但查找是 O(n)）");
Console.WriteLine("    - .NET 9 起的 OrderedDictionary<TKey,TValue>：兼顾查找和插入顺序");
Console.WriteLine();

// ==================== 实验二：GetHashCode 契约 ====================

Console.WriteLine("=== 实验二：重写了 Equals 却忘了 GetHashCode ===");
Console.WriteLine();

var badDict = new Dictionary<BadKey, string>();
badDict[new BadKey(1)] = "A";

bool contains = badDict.ContainsKey(new BadKey(1));
Console.WriteLine($"  存了一个 BadKey(Id=1)，再查一个「相等」的 BadKey(Id=1)：");
Console.WriteLine($"    ContainsKey 返回 {contains}    <- 应该是 True！");
Console.WriteLine();
Console.WriteLine("  而直接用 Equals 比较，它们明明是相等的：");
bool equals = new BadKey(1).Equals(new BadKey(1));
Console.WriteLine($"    new BadKey(1).Equals(new BadKey(1)) = {equals}");
Console.WriteLine();
Console.WriteLine("  问题出在：BadKey 重写了 Equals，却没重写 GetHashCode，");
Console.WriteLine("  于是两个「相等」的对象继承了 object 的默认哈希（基于对象地址），哈希值不同，");
Console.WriteLine("  被分到了不同的桶里 —— Dictionary 根本不会去比较它们。");
Console.WriteLine();
Console.WriteLine("  【契约】重写 Equals 就必须重写 GetHashCode，且满足：");
Console.WriteLine("    a.Equals(b) 为 true  =>  a.GetHashCode() == b.GetHashCode()");
Console.WriteLine("  反过来不要求（哈希相同，对象不一定相等）。");
Console.WriteLine();

// ==================== 实验三：用可变对象当键 ====================

Console.WriteLine("=== 实验三：用「会被修改的对象」当键 ===");
Console.WriteLine();

var mutDict = new Dictionary<MutableKey, string>();
var key = new MutableKey { Id = 1 };
mutDict[key] = "A";

Console.WriteLine($"  存入 MutableKey(Id=1) -> \"A\"");
Console.WriteLine($"  修改前 ContainsKey = {mutDict.ContainsKey(key)}");

key.Id = 2;                       // 修改了作为键的对象！
Console.WriteLine($"  把 key.Id 改成 2 之后...");
Console.WriteLine($"  修改后 ContainsKey = {mutDict.ContainsKey(key)}");
Console.WriteLine($"  字典里的元素个数   = {mutDict.Count}（元素还在，但再也找不到了）");
Console.WriteLine();
Console.WriteLine("  原因：GetHashCode 基于 Id，Id 一变，哈希值就变了，");
Console.WriteLine("  但元素还待在【原来那个桶】里 —— 相当于把钥匙弄丢了。");
Console.WriteLine();
Console.WriteLine("  【结论】用引用类型当键时，这个对象应该是【不可变的】。");
Console.WriteLine("  推荐用 string、int、Guid、或者 record / 只读结构体当键。");
Console.WriteLine();

// ==================== 实验四：TryGetValue vs ContainsKey ====================

Console.WriteLine("=== 实验四：TryGetValue 比 ContainsKey + 索引器快多少 ===");
Console.WriteLine();

const int N = 3_000_000;
var bigDict = new Dictionary<int, int>(N);
for (int i = 0; i < N; i++) bigDict[i] = i * 2;

var keysToFind = new int[1_000_000];
var rng = new Random(42);
for (int i = 0; i < keysToFind.Length; i++) keysToFind[i] = rng.Next(0, N);

// 预热
_ = bigDict.TryGetValue(0, out _);

// ---- 做法一：ContainsKey + 索引器（查找两次）----
var sw = Stopwatch.StartNew();
long sum1 = 0;
foreach (int k in keysToFind)
{
    if (bigDict.ContainsKey(k))       // 第一次查找
        sum1 += bigDict[k];           // 第二次查找！
}
sw.Stop();
double twoLookupsMs = sw.Elapsed.TotalMilliseconds;

// ---- 做法二：TryGetValue（查找一次）----
sw.Restart();
long sum2 = 0;
foreach (int k in keysToFind)
{
    if (bigDict.TryGetValue(k, out int v))    // 一次查找同时拿到值
        sum2 += v;
}
sw.Stop();
double oneLookupMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  {keysToFind.Length:N0} 次查找（数据量 {N:N0}）：");
Console.WriteLine($"    ContainsKey + 索引器: {twoLookupsMs,8:F2} ms   （每个键查找两次）");
Console.WriteLine($"    TryGetValue         : {oneLookupMs,8:F2} ms   （每个键只查找一次）");
Console.WriteLine($"    TryGetValue 快 {twoLookupsMs / oneLookupMs:F2} 倍（省下了约 {(1 - oneLookupMs / twoLookupsMs):P0} 的查找次数）");
Console.WriteLine($"    结果一致: {sum1 == sum2}");
Console.WriteLine();
Console.WriteLine("  这里快了约 1/3，没有到理论上的 2 倍 —— 因为「命中」的键只占一部分，");
Console.WriteLine("  未命中的键用 ContainsKey 时也只需要查一次。");
Console.WriteLine("  但两者都是 O(1)，这属于「常数优化」：只改一行代码、零风险，值得养成习惯。");
Console.WriteLine();

// ==================== 实验五：并发写 Dictionary ====================

Console.WriteLine("=== 实验五：多线程并发写 Dictionary ===");
Console.WriteLine();

var concurrentDict = new Dictionary<int, int>();
var task = Task.Run(() =>
{
    Parallel.For(0, 200_000, i => concurrentDict[i] = i);
});

// 注意：task.Wait(timeout) 在任务抛异常时会【抛出 AggregateException】，
// 而不是返回 false。必须先捕获它，否则这里会让整个程序崩溃。
bool completed;
try
{
    completed = task.Wait(TimeSpan.FromSeconds(5));
}
catch (AggregateException)
{
    completed = true;                 // 以异常方式结束了，也算「结束」
}

if (!completed)
{
    Console.WriteLine("  超过 5 秒还没结束 —— 很可能已经卡死。");
    Console.WriteLine("  （Dictionary 在并发写下可能进入死循环，这是它的已知行为之一）");
}
else
{
    // Parallel.For 会把每个线程的异常打包成 AggregateException 再包一层，
    // 所以要用 Flatten() 拍平，并且只取第一条 —— 否则消息会重复十几遍
    var first = task.Exception?.Flatten().InnerExceptions.FirstOrDefault();
    if (first != null)
    {
        string msg = first.Message;
        if (msg.Length > 88) msg = msg[..88] + "...";

        Console.WriteLine($"  抛出异常: {first.GetType().Name}");
        Console.WriteLine($"  消息: {msg}");
        Console.WriteLine();
        Console.WriteLine("  .NET 的 Dictionary 内置了「版本号检查」，检测到并发修改就主动抛异常，");
        Console.WriteLine("  而不是像某些老实现那样【静默地损坏数据】—— 这是好事。");
    }
    else
    {
        Console.WriteLine($"  这次侥幸没出问题，最终元素个数 = {concurrentDict.Count}（期望 200,000）");
        Console.WriteLine("  —— 但并发 bug 是概率性的，多跑几次就会暴露。");
    }
}
Console.WriteLine();
Console.WriteLine("  【规则】Dictionary 不是线程安全的，绝对不能多线程并发写。");
Console.WriteLine("  需要并发时用：");
Console.WriteLine("    - ConcurrentDictionary<TKey,TValue>：多线程读写安全，API 也是线程安全的");
Console.WriteLine("    - 或者用锁（lock）把整个字典保护起来（读多写少时更简单）");
Console.WriteLine("    - 或者「写时复制」：读多写极少时，写的时候复制一份新字典再替换引用");
Console.WriteLine();

// ==================== 实验六：键类型的选择 ====================

Console.WriteLine("=== 实验六：键类型对性能的影响 ===");
Console.WriteLine();

const int M = 500_000;
var keysStr = new string[M];
for (int i = 0; i < M; i++) keysStr[i] = i.ToString();

var keysInt = new int[M];
for (int i = 0; i < M; i++) keysInt[i] = i;

// string 键
var strDict = new Dictionary<string, int>(M);
sw.Restart();
for (int i = 0; i < M; i++) strDict[keysStr[i]] = i;
foreach (var k in keysStr) _ = strDict[k];
sw.Stop();
double strMs = sw.Elapsed.TotalMilliseconds;

// int 键
var intDict = new Dictionary<int, int>(M);
sw.Restart();
for (int i = 0; i < M; i++) intDict[keysInt[i]] = i;
foreach (var k in keysInt) _ = intDict[k];
sw.Stop();
double intMs = sw.Elapsed.TotalMilliseconds;

Console.WriteLine($"  {M:N0} 次插入 + {M:N0} 次查找：");
Console.WriteLine($"    string 键: {strMs,8:F1} ms");
Console.WriteLine($"    int 键   : {intMs,8:F1} ms");
Console.WriteLine($"    int 键快 {strMs / intMs:F2} 倍");
Console.WriteLine();
Console.WriteLine("  int 的哈希就是它自己（一次赋值），而 string 要遍历所有字符才能算哈希。");
Console.WriteLine("  如果数据量很大且键可以转成整数（比如自增 ID），用整数键会明显更快。");
Console.WriteLine();

// ==================== 自定义类型 ====================

/// <summary>反面教材：重写了 Equals 却没重写 GetHashCode。</summary>
public class BadKey
{
    public int Id;
    public BadKey(int id) => Id = id;

    public override bool Equals(object? obj) => obj is BadKey b && b.Id == Id;
    // 故意不重写 GetHashCode —— 这就是 bug
}

/// <summary>可变对象当键的反面教材。</summary>
public class MutableKey
{
    public int Id;

    public override int GetHashCode() => Id;                      // 哈希值跟着 Id 变
    public override bool Equals(object? obj) => obj is MutableKey m && m.Id == Id;
}
