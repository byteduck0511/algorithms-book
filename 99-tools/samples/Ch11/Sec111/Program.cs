// ==================== 堆的数组表示 ====================
//
// 堆是一棵「完全二叉树」，而且：
//   小顶堆：每个父节点 <= 它的两个孩子（根是最小值）
//   大顶堆：每个父节点 >= 它的两个孩子（根是最大值）
//
// 因为「完全」这个约束，它可以紧凑地存在数组里，不需要任何指针。

/// <summary>把一个数组按「完全二叉树的层序」打印出来，方便对照下标。</summary>
static void PrintAsTree(int[] a)
{
    int n = a.Length;
    int level = 0, idx = 0;

    while (idx < n)
    {
        int count = Math.Min(1 << level, n - idx);      // 这一层有几个节点
        int leading = (1 << (Levels(n) - level - 1)) * 2;   // 缩进，让图形居中

        Console.Write(new string(' ', Math.Max(leading, 0)));
        for (int i = 0; i < count; i++)
        {
            Console.Write($"{a[idx + i],3}   ");
        }
        Console.WriteLine();
        idx += count;
        level++;
    }
}

static int Levels(int n) => n <= 1 ? 1 : (int)Math.Floor(Math.Log2(n)) + 1;

/// <summary>检查是否满足小顶堆性质，顺便统计检查了哪些父子关系。</summary>
static bool IsMinHeap(int[] a, bool verbose = false)
{
    for (int i = 0; i <= a.Length / 2 - 1; i++)
    {
        int left = 2 * i + 1, right = 2 * i + 2;

        if (left < a.Length)
        {
            if (verbose) Console.WriteLine("      父 a[" + i + "]=" + a[i] + " vs 左子 a[" + left + "]=" + a[left]);
            if (a[i] > a[left]) return false;
        }
        if (right < a.Length)
        {
            if (verbose) Console.WriteLine("      父 a[" + i + "]=" + a[i] + " vs 右子 a[" + right + "]=" + a[right]);
            if (a[i] > a[right]) return false;
        }
    }
    return true;
}

Console.WriteLine("=== 实验一：堆的数组表示 ===");
Console.WriteLine();

//               2
//             /   \
//            5     3
//           / \   /
//          8   7 6
int[] heap = { 2, 5, 3, 8, 7, 6 };

Console.WriteLine("  一个合法的小顶堆：");
PrintAsTree(heap);
Console.WriteLine();

Console.WriteLine($"  存进数组就是: [{string.Join(", ", heap)}]");
Console.WriteLine($"  下标:           0  1  2  3  4  5");
Console.WriteLine();

Console.WriteLine("  验证父子关系（用下标公式算）：");
Console.WriteLine($"    下标 0（值 {heap[0]}）的孩子是下标 1、2（值 {heap[1]}、{heap[2]}）");
Console.WriteLine($"      检查：{heap[0]} <= {heap[1]} ✓   {heap[0]} <= {heap[2]} ✓");
Console.WriteLine($"    下标 1（值 {heap[1]}）的孩子是下标 3、4（值 {heap[3]}、{heap[4]}）");
Console.WriteLine($"      检查：{heap[1]} <= {heap[3]} ✓   {heap[1]} <= {heap[4]} ✓");
Console.WriteLine($"    下标 2（值 {heap[2]}）的孩子是下标 5（值 {heap[5]}）");
Console.WriteLine($"      检查：{heap[2]} <= {heap[5]} ✓");
Console.WriteLine();
Console.WriteLine($"  结论：这是一个合法的小顶堆：{IsMinHeap(heap)}");
Console.WriteLine();

// ==================== 实验二：父子下标公式 ====================

Console.WriteLine("=== 实验二：下标公式 ===");
Console.WriteLine();
Console.WriteLine("  对下标 i 的节点：");
Console.WriteLine("    左孩子 = 2i + 1");
Console.WriteLine("    右孩子 = 2i + 2");
Console.WriteLine("    父节点 = (i - 1) / 2      （整数除法，向下取整）");
Console.WriteLine();
Console.WriteLine($"  {"下标 i",8} | {"值",6} | {"左孩子下标",12} | {"右孩子下标",12} | 父节点下标");
Console.WriteLine("  " + new string('-', 62));

for (int i = 0; i < heap.Length; i++)
{
    int l = 2 * i + 1, r = 2 * i + 2, p = (i - 1) / 2;
    string ls = l < heap.Length ? $"{l}（值{heap[l]}）" : "无";
    string rs = r < heap.Length ? $"{r}（值{heap[r]}）" : "无";
    string ps = i == 0 ? "无（它是根）" : $"{p}（值{heap[p]}）";
    Console.WriteLine($"  {i,8} | {heap[i],6} | {ls,12} | {rs,12} | {ps}");
}
Console.WriteLine();
Console.WriteLine("  注意「父节点 = (i-1)/2」用的是整除：");
Console.WriteLine("    i=1 -> (1-1)/2 = 0    i=2 -> (2-1)/2 = 0   （1 和 2 的父都是 0）");
Console.WriteLine("    i=3 -> (3-1)/2 = 1    i=4 -> (4-1)/2 = 1   （3 和 4 的父都是 1）");
Console.WriteLine();

// ==================== 实验三：大顶堆 vs 小顶堆 ====================

Console.WriteLine("=== 实验三：大顶堆 vs 小顶堆 ===");
Console.WriteLine();

int[] maxHeap = { 9, 7, 8, 3, 5, 6 };
Console.WriteLine("  同一批数据，组织成大顶堆：");
PrintAsTree(maxHeap);
Console.WriteLine($"  数组表示: [{string.Join(", ", maxHeap)}]");
Console.WriteLine("  性质：每个父节点 >= 孩子，根是【最大值】9");
Console.WriteLine();

// 验证大顶堆
bool validMax = true;
for (int i = 0; i <= maxHeap.Length / 2 - 1; i++)
{
    int l = 2 * i + 1, r = 2 * i + 2;
    if (l < maxHeap.Length && maxHeap[i] < maxHeap[l]) validMax = false;
    if (r < maxHeap.Length && maxHeap[i] < maxHeap[r]) validMax = false;
}
Console.WriteLine($"  验证：{(validMax ? "是合法的大顶堆 ✓" : "不是")}");
Console.WriteLine();

Console.WriteLine("  两种堆的用途完全不同：");
Console.WriteLine($"    {"堆类型",-10} | {"根是什么",-12} | {"典型用途",-30}");
Console.WriteLine("    " + new string('-', 58));
Console.WriteLine($"    {"小顶堆",-12} | {"最小值",-14} | {"优先队列（最小的先出）、Dijkstra",-32}");
Console.WriteLine($"    {"大顶堆",-14} | {"最大值",-14} | {"Top-K（找最大的 K 个）、堆排序",-32}");
Console.WriteLine();

// ==================== 实验四：为什么用数组而不是链表 ====================

Console.WriteLine("=== 实验四：为什么堆用数组而不是链表？ ===");
Console.WriteLine();
Console.WriteLine("  树结构通常需要指针（像 BST 那样）。但堆是特例，因为它有两个特殊条件：");
Console.WriteLine();
Console.WriteLine("    1. 它是【完全二叉树】—— 每一层从左到右填满，中间没有空洞");
Console.WriteLine("    2. 它【不需要在中途插入/删除】—— 只在末尾加、只在根上删");
Console.WriteLine();
Console.WriteLine("  第 1 条保证了「层序编号」和「数组下标」一一对应，不会有空洞浪费；");
Console.WriteLine("  第 2 条保证了「末尾加」和「根上删」这两个位置在数组里都是 O(1) 可达的。");
Console.WriteLine();
Console.WriteLine("  对比一下：");
Console.WriteLine($"    {"维度",-20} | {"数组实现",-24} | {"链表实现",-24}");
Console.WriteLine("    " + new string('-', 72));
Console.WriteLine($"    {"每节点额外内存",-22} | {"0（紧凑排列）",-26} | {"约 16 字节（两个指针）",-26}");
Console.WriteLine($"    {"缓存友好性",-23} | {"好（连续内存）",-26} | {"差（节点散落）",-26}");
Console.WriteLine($"    {"找父/子节点",-22} | {"O(1) 下标计算",-26} | {"需要额外维护父指针",-26}");
Console.WriteLine();
Console.WriteLine("  所以堆的数组实现不是「省事」，而是【严格更优】。");
Console.WriteLine("  这也是 3.1 节「连续内存红利」的又一次体现。");
Console.WriteLine();

// ==================== 实验五：堆只保证局部有序 ====================

Console.WriteLine("=== 实验五：堆是「局部有序」，不是「全局有序」===");
Console.WriteLine();
Console.WriteLine("  一个常见的误解是「堆是排好序的」。其实不是 ——");
Console.WriteLine("  堆只保证【父 <= 子】这一条局部关系。");
Console.WriteLine();

int[] demo = { 2, 5, 3, 8, 7, 6 };
Console.WriteLine($"  堆的数组表示: [{string.Join(", ", demo)}]");
Console.WriteLine($"  层序遍历就是数组顺序: {string.Join(" ", demo)}");
Console.WriteLine();

var sortedCopy = (int[])demo.Clone();
Array.Sort(sortedCopy);
Console.WriteLine($"  如果按【从小到大】排序，应该是: {string.Join(" ", sortedCopy)}");
Console.WriteLine();
Console.WriteLine("  对比一下就能看出：");
Console.WriteLine("    堆的数组顺序   : 2 5 3 8 7 6");
Console.WriteLine("    完全排序的顺序 : 2 3 5 6 7 8");
Console.WriteLine();
Console.WriteLine("  堆【不是】有序数组 —— 比如 3 排在 5 后面，但 3 < 5。");
Console.WriteLine("  它只保证「任何一个父节点都 <= 它的直接孩子」。");
Console.WriteLine();
Console.WriteLine("  这个「弱一点」的保证，正是堆的价值所在：");
Console.WriteLine("    维护全局有序要 O(n log n)（排序），");
Console.WriteLine("    维护堆序只要 O(log n)（一次上浮或下沉）—— 增删代价小得多。");
Console.WriteLine("    而你从堆里【取最小值】仍然是 O(1)（就是根）。");
Console.WriteLine();
