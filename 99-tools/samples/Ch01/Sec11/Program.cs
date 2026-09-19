using System.Diagnostics;

// ---------- 做法 A：两层循环，检查所有配对 ----------
static long CountEqualPairsSlow(int[] data)
{
    long count = 0;
    for (int i = 0; i < data.Length; i++)
    {
        for (int j = i + 1; j < data.Length; j++)
        {
            if (data[i] == data[j])
                count++;          // 注意：没有 break，每一对都必须检查
        }
    }
    return count;
}

// ---------- 做法 B：一遍扫描 + 哈希表 ----------
static long CountEqualPairsFast(int[] data)
{
    var seen = new Dictionary<int, int>();   // 值 -> 之前出现过多少次
    long count = 0;
    foreach (int x in data)
    {
        seen.TryGetValue(x, out int prev);   // prev = 之前出现过几次（没有则为 0）
        count += prev;                       // 与之前的每一个都构成一对
        seen[x] = prev + 1;
    }
    return count;
}

// ---------- 造数据：固定随机种子，保证结果可复现 ----------
static int[] MakeData(int n)
{
    var rng = new Random(42);
    var a = new int[n];
    for (int i = 0; i < n; i++)
        a[i] = rng.Next(0, n / 10);   // 取值范围偏小，保证有大量重复
    return a;
}

// 每个规模跑 3 次，取最快的一次：最快值受 GC 与系统调度干扰最小
// 用微秒而不是毫秒：快速实现快到毫秒计不出来，用毫秒会显示成 0.0
static double MeasureFastest(Func<long> work, int repeats = 3)
{
    double best = double.MaxValue;
    for (int r = 0; r < repeats; r++)
    {
        var sw = Stopwatch.StartNew();
        work();
        sw.Stop();
        if (sw.Elapsed.TotalMicroseconds < best)
            best = sw.Elapsed.TotalMicroseconds;
    }
    return best;
}

// 预热：先让 JIT 编译好，避免第一轮测量被编译开销污染
_ = CountEqualPairsSlow(MakeData(500));
_ = CountEqualPairsFast(MakeData(500));

Console.WriteLine($"{"n",7} | {"慢速(μs)",10} | {"快速(μs)",9} | {"结果一致",8}");
foreach (int n in new[] { 5_000, 50_000 })
{
    int[] data = MakeData(n);

    long slow = CountEqualPairsSlow(data);   // 先各跑一遍拿到结果
    long fast = CountEqualPairsFast(data);

    double slowUs = MeasureFastest(() => CountEqualPairsSlow(data));
    double fastUs = MeasureFastest(() => CountEqualPairsFast(data));

    Console.WriteLine($"{n,7} | {slowUs,10:F0} | {fastUs,9:F0} | {slow == fast,8}");
}
