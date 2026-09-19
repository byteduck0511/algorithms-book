// ==================== 两个方向相反的操作 ====================
//
// 注意：本节用【小顶堆】（父 <= 子），配合优先队列的场景。
// 8.3 节的堆排序用的是【大顶堆】，方向相反但逻辑完全对称。

/// <summary>
/// 上浮：把下标 i 的元素往上调整，直到它的父节点不大于它。
/// 【用在插入时】—— 新元素放在末尾，然后一路上浮找位置。
/// </summary>
static void SiftUp(int[] a, int i, ref long cmp, ref long swaps, bool trace = false)
{
    while (i > 0)
    {
        int parent = (i - 1) / 2;
        cmp++;

        if (trace)
            Console.WriteLine($"      比较 a[{i}]={a[i]} 和父 a[{parent}]={a[parent]}");

        if (a[parent] <= a[i])
        {
            if (trace) Console.WriteLine($"        父节点更小或相等，停在这里");
            break;
        }

        (a[parent], a[i]) = (a[i], a[parent]);
        swaps++;
        if (trace) Console.WriteLine($"        交换 -> [{string.Join(", ", a)}]");
        i = parent;
    }
}

/// <summary>
/// 下沉：把下标 i 的元素往下调整，直到它不大于它的两个孩子。
/// 【用在删除根时，以及建堆时】。
/// </summary>
static void SiftDown(int[] a, int i, int size, ref long cmp, ref long swaps, bool trace = false)
{
    while (true)
    {
        int smallest = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        if (left < size)
        {
            cmp++;
            if (a[left] < a[smallest]) smallest = left;
        }
        if (right < size)
        {
            cmp++;
            if (a[right] < a[smallest]) smallest = right;
        }

        if (smallest == i)
        {
            if (trace) Console.WriteLine($"        两个孩子都不比它小，停在这里");
            break;
        }

        if (trace)
            Console.WriteLine($"      最小的是 a[{smallest}]={a[smallest]}，交换");

        (a[i], a[smallest]) = (a[smallest], a[i]);
        swaps++;
        if (trace) Console.WriteLine($"        交换 -> [{string.Join(", ", a)}]");
        i = smallest;
    }
}

// ==================== 实验一：插入 -> 上浮 ====================

Console.WriteLine("=== 实验一：插入元素 -> 上浮 ===");
Console.WriteLine();

int[] heap = { 2, 5, 3, 8, 7, 6 };
Console.WriteLine($"  初始堆: [{string.Join(", ", heap)}]");
Console.WriteLine("  对应的树：");
Console.WriteLine("          2");
Console.WriteLine("       /     \\");
Console.WriteLine("      5       3");
Console.WriteLine("     / \\     /");
Console.WriteLine("    8   7   6");
Console.WriteLine();

Console.Write("  现在插入 1（比所有元素都小）：");
int[] withNew = heap.Append(1).ToArray();
Console.WriteLine($"  先放到末尾 -> [{string.Join(", ", withNew)}]");
Console.WriteLine();

long cmp = 0, swaps = 0;
SiftUp(withNew, withNew.Length - 1, ref cmp, ref swaps, trace: true);
Console.WriteLine();
Console.WriteLine($"  上浮完成: [{string.Join(", ", withNew)}]");
Console.WriteLine($"  比较 {cmp} 次，交换 {swaps} 次");
Console.WriteLine();

Console.WriteLine("  新的树：");
Console.WriteLine("          1");
Console.WriteLine("       /     \\");
Console.WriteLine("      2       3");
Console.WriteLine("     / \\     / \\");
Console.WriteLine("    8   7   6   5");
Console.WriteLine();
Console.WriteLine("  1 一路从最底部浮到了根 —— 因为它比沿途所有祖先都小。");
Console.WriteLine();

// ==================== 实验二：取走根 -> 下沉 ====================

Console.WriteLine("=== 实验二：取走最小值 -> 下沉 ===");
Console.WriteLine();

int[] h2 = (int[])withNew.Clone();
Console.WriteLine($"  当前堆: [{string.Join(", ", h2)}]");
Console.WriteLine($"  取走根（最小值 {h2[0]}）");
Console.WriteLine();

// 第一步：把末尾元素移到根上
Console.WriteLine($"  第 1 步：把末尾的 {h2[h2.Length - 1]} 移到根的位置");
h2[0] = h2[h2.Length - 1];
h2 = h2[..^1];      // 删掉末尾
Console.WriteLine($"          -> [{string.Join(", ", h2)}]");
Console.WriteLine();

Console.WriteLine("  第 2 步：对根执行下沉");
cmp = 0; swaps = 0;
SiftDown(h2, 0, h2.Length, ref cmp, ref swaps, trace: true);
Console.WriteLine();
Console.WriteLine($"  下沉完成: [{string.Join(", ", h2)}]");
Console.WriteLine($"  比较 {cmp} 次，交换 {swaps} 次");
Console.WriteLine();

Console.WriteLine("  为什么不能直接删掉根？");
Console.WriteLine("    直接删掉根会在树的【顶部】留一个空洞，破坏「完全二叉树」的性质；");
Console.WriteLine("    把末尾元素移到根上，形状仍然合法（长度减一，仍然连续），");
Console.WriteLine("    然后只需要一次下沉就能恢复堆序。");
Console.WriteLine();

// ==================== 实验三：两种操作的对比 ====================

Console.WriteLine("=== 实验三：上浮 vs 下沉 ===");
Console.WriteLine();
Console.WriteLine($"  {"维度",-16} | {"上浮 SiftUp",-26} | {"下沉 SiftDown",-26}");
Console.WriteLine("  " + new string('-', 74));
Console.WriteLine($"  {"移动方向",-18} | {"往上（朝根）",-28} | {"往下（朝叶）",-28}");
Console.WriteLine($"  {"用在什么时候",-16} | {"插入新元素后",-28} | {"删除根之后 / 建堆时",-28}");
Console.WriteLine($"  {"和谁比较",-18} | {"只和【一个】父节点比",-26} | {"和【两个孩子】比",-28}");
Console.WriteLine($"  {"每次比较的代价",-14} | {"1 次比较",-28} | {"最多 2 次比较",-28}");
Console.WriteLine($"  {"最多走几层",-16} | {"树高 = log n",-28} | {"树高 = log n",-28}");
Console.WriteLine();
Console.WriteLine("  注意一个不对称的地方：");
Console.WriteLine("    上浮时，每个节点只有一个父节点 —— 比较 1 次就知道该不该继续；");
Console.WriteLine("    下沉时，每个节点有两个孩子 —— 要先找出「较小/较大的那个孩子」，再比较。");
Console.WriteLine();
Console.WriteLine("  所以【下沉比上浮稍贵】，这也是为什么「建堆用下沉」而不是「逐个插入用上浮」——");
Console.WriteLine("  后者要 n 次上浮，每次 O(log n)，总共 O(n log n)；");
Console.WriteLine("  而自底向上建堆虽然也用下沉，但大部分节点在底层、下沉距离很短，总共只要 O(n)。");
Console.WriteLine("  （8.3 节详细推导过这个 O(n)）");
Console.WriteLine();

// ==================== 实验四：为什么插入不能"直接放对位置" ====================

Console.WriteLine("=== 实验四：为什么插入必须放末尾？ ===");
Console.WriteLine();

int[] demo = { 2, 5, 3, 8, 7, 6 };
Console.WriteLine($"  堆: [{string.Join(", ", demo)}]");
Console.WriteLine();
Console.WriteLine("  有人会想：「插入 1 的时候，为什么不直接把它放到根上，");
Console.WriteLine("              然后把原来的根往下挪？」");
Console.WriteLine();

// 演示错误的做法
int[] wrong = new int[7];
wrong[0] = 1;                      // 直接放根
Array.Copy(demo, 0, wrong, 1, 6);  // 其他元素整体后移
Console.WriteLine($"  错误做法（直接插到根，其他元素整体后移）:");
Console.WriteLine($"    [{string.Join(", ", wrong)}]");
Console.WriteLine("    虽然堆序碰巧对了，但——");

Console.WriteLine();
Console.WriteLine("  问题在于【这个数组不再对应一棵合法的完全二叉树】：");
Console.WriteLine("    原来的树：根 2，左 5，右 3，5 的孩子是 8、7，3 的孩子是 6");
Console.WriteLine("    整体后移后，节点之间的父子关系全变了！");
Console.WriteLine("    比如下标 1 的 2，它的孩子变成了下标 3 的 5 和下标 4 的 3 ——");
Console.WriteLine("    但原来是 2 的孩子是 5 和 3，现在 5 和 3 变成了 2 的孙子辈。");
Console.WriteLine();
Console.WriteLine("  【结论】插入必须放在【末尾】—— 这是保持「完全二叉树」的唯一位置。");
Console.WriteLine("          放好之后再靠上浮调整，既不破坏结构，代价也只有 O(log n)。");
Console.WriteLine();

// ==================== 实验五：验证正确性 ====================

Console.WriteLine("=== 实验五：用一组随机操作验证两个操作的正确性 ===");
Console.WriteLine();

var rng = new Random(42);
var testHeap = new List<int>();
cmp = 0; swaps = 0;

// 插入 200 个随机数
for (int i = 0; i < 200; i++)
{
    testHeap.Add(rng.Next(0, 1000));
    var arr = testHeap.ToArray();
    SiftUp(arr, arr.Length - 1, ref cmp, ref swaps);
    testHeap = arr.ToList();
}

// 逐个取出最小值，验证是升序的
var extracted = new List<int>();
for (int i = 0; i < 200; i++)
{
    var arr = testHeap.ToArray();
    extracted.Add(arr[0]);                         // 取根
    arr[0] = arr[^1];                              // 末尾移到根
    arr = arr[..^1];                               // 删末尾
    SiftDown(arr, 0, arr.Length, ref cmp, ref swaps);
    testHeap = arr.ToList();
}

bool isSorted = true;
for (int i = 1; i < extracted.Count; i++)
    if (extracted[i] < extracted[i - 1]) isSorted = false;

Console.WriteLine($"  插入 200 个随机数，再逐个取出最小值");
Console.WriteLine($"  取出的序列是升序吗？{isSorted}");
Console.WriteLine($"  前 10 个取出的值: {string.Join(" ", extracted.Take(10))}");
Console.WriteLine($"  累计比较 {cmp:N0} 次，交换 {swaps:N0} 次");
Console.WriteLine();
Console.WriteLine("  这就是「优先队列」的核心：不断插入、不断取最小值。");
Console.WriteLine("  下一节会把它封装成一个完整的类。");
Console.WriteLine();
