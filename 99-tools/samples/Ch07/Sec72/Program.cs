// ==================== 三种排序（针对订单，按价格排序） ====================

static Order[] BubbleSort(Order[] source)
{
    var a = (Order[])source.Clone();
    int n = a.Length;
    for (int i = 0; i < n - 1; i++)
    {
        bool swapped = false;
        for (int j = 0; j < n - 1 - i; j++)
        {
            if (a[j].Price > a[j + 1].Price)      // 注意：严格大于才交换
            {
                (a[j], a[j + 1]) = (a[j + 1], a[j]);
                swapped = true;
            }
        }
        if (!swapped) break;
    }
    return a;
}

static Order[] SelectionSort(Order[] source)
{
    var a = (Order[])source.Clone();
    int n = a.Length;
    for (int i = 0; i < n - 1; i++)
    {
        int minIdx = i;
        for (int j = i + 1; j < n; j++)
        {
            if (a[j].Price < a[minIdx].Price)     // 注意：严格小于才更新
                minIdx = j;
        }
        if (minIdx != i)
            (a[i], a[minIdx]) = (a[minIdx], a[i]);   // <- 问题就出在这一步「长距离交换」
    }
    return a;
}

static Order[] InsertionSort(Order[] source)
{
    var a = (Order[])source.Clone();
    for (int i = 1; i < a.Length; i++)
    {
        Order key = a[i];
        int j = i - 1;
        while (j >= 0 && a[j].Price > key.Price)   // 注意：严格大于才继续往前
        {
            a[j + 1] = a[j];
            j--;
        }
        a[j + 1] = key;
    }
    return a;
}

/// <summary>检查排序结果是否「稳定」：价格相同的元素，原始下标必须保持递增。</summary>
static bool IsStable(Order[] sorted)
{
    for (int i = 1; i < sorted.Length; i++)
    {
        if (sorted[i].Price == sorted[i - 1].Price &&
            sorted[i].Seq < sorted[i - 1].Seq)
            return false;
    }
    return true;
}

static string Format(Order[] orders) => string.Join(" ", orders.Select(o => o.Id));

// ==================== 主流程 ====================

Console.WriteLine("=== 实验一：什么是排序稳定性 ===");
Console.WriteLine();

var orders = new[]
{
    new Order("A", 100, 0),
    new Order("B", 50, 1),
    new Order("C", 100, 2),
    new Order("D", 50, 3),
    new Order("E", 100, 4),
    new Order("F", 50, 5),
};

Console.WriteLine("  原始数据（Id, 价格）：");
foreach (var o in orders) Console.Write($"{o.Id}({o.Price}) ");
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("  按价格从小到大排序。");
Console.WriteLine("  注意：价格 50 的有 B、D、F，价格 100 的有 A、C、E。");
Console.WriteLine("  如果排序【稳定】，结果中它们的相对顺序应该保持 B->D->F 和 A->C->E。");
Console.WriteLine();

Console.WriteLine("=== 实验二：三种排序的稳定性 ===");
Console.WriteLine();

var bubbleResult = BubbleSort(orders);
Console.WriteLine($"  冒泡排序: {Format(bubbleResult)}");
Console.WriteLine($"    稳定性: {(IsStable(bubbleResult) ? "稳定 ✓" : "不稳定 ✗")}");
Console.WriteLine();

var selectionResult = SelectionSort(orders);
Console.WriteLine($"  选择排序: {Format(selectionResult)}");
Console.WriteLine($"    稳定性: {(IsStable(selectionResult) ? "稳定 ✓" : "不稳定 ✗")}");
Console.WriteLine();

var insertionResult = InsertionSort(orders);
Console.WriteLine($"  插入排序: {Format(insertionResult)}");
Console.WriteLine($"    稳定性: {(IsStable(insertionResult) ? "稳定 ✓" : "不稳定 ✗")}");
Console.WriteLine();

Console.WriteLine("=== 选择排序为什么不稳定：手工推演 ===");
Console.WriteLine();
Console.WriteLine("  原始: [A(100), B(50), C(100), D(50)]");
Console.WriteLine();
Console.WriteLine("  第 1 轮：在 [A,B,C,D] 中找最小值 -> B(50)，在下标 1");
Console.WriteLine("           把 B 和下标 0 的 A 交换 -> [B(50), A(100), C(100), D(50)]");
Console.WriteLine("                ^^^^ A 被甩到了后面，它和 C 的相对顺序还没乱，但...");
Console.WriteLine();
Console.WriteLine("  第 2 轮：在 [A,C,D] 中找最小值（从下标 1 开始）-> D(50)，在下标 3");
Console.WriteLine("           把 D 和下标 1 的 A 交换 -> [B(50), D(50), C(100), A(100)]");
Console.WriteLine("                                      ^^^^^^^^^^^^^^^^^^^^^^^");
Console.WriteLine("           注意最后两个：原来是 A 在前、C 在后，现在变成了 C 在前、A 在后！");
Console.WriteLine();
Console.WriteLine("  根源：选择排序做的是【长距离交换】——");
Console.WriteLine("        一次交换可能跨越好几个位置，把沿途元素的相对顺序全部打乱。");
Console.WriteLine("        而冒泡和插入只做【相邻移动】，等值元素永远不会互相跨越。");
Console.WriteLine();

// ==================== 实验三：稳定性有什么用 ====================

Console.WriteLine("=== 实验三：稳定性有什么用 —— 多关键字排序 ===");
Console.WriteLine();

var employees = new[]
{
    new Order("张三", 20000, 0),
    new Order("李四", 15000, 1),
    new Order("王五", 20000, 2),
    new Order("赵六", 15000, 3),
    new Order("钱七", 20000, 4),
};

Console.WriteLine("  需求：按薪资【降序】排列，薪资相同的按【姓名的原始顺序】排列。");
Console.WriteLine();

Console.WriteLine($"  原始顺序: {Format(employees)}（下标 0..4）");
Console.WriteLine();

Console.WriteLine("  做法一：直接按薪资降序排一次（用稳定排序）——");
var sorted1 = employees.OrderByDescending(e => e.Price).ToArray();
Console.WriteLine($"    结果: {Format(sorted1)}");
Console.WriteLine($"    薪资: {string.Join(" ", sorted1.Select(e => e.Price))}");
Console.WriteLine("    LINQ 的 OrderBy 是稳定排序，所以同薪资的保持了原始顺序 ✓");
Console.WriteLine();

Console.WriteLine("  做法二：显式利用稳定性做「多关键字排序」——");
Console.WriteLine("    1. 先按【次要关键字】排序（这里没有次要关键字，跳过）");
Console.WriteLine("    2. 再按【主要关键字】排序");
Console.WriteLine();
Console.WriteLine("    经典例子：员工表要「先按部门分组，组内按薪资降序」。");
Console.WriteLine("      第一步：按薪资【升序】排一次；");
Console.WriteLine("      第二步：按部门排一次（稳定）。");
Console.WriteLine("    结果：部门有序，且每个部门内部的薪资是升序的 —— 一步搞定两个关键字。");
Console.WriteLine();

Console.WriteLine("  这个技巧叫做【基数排序式的从低位到高位排序】，");
Console.WriteLine("  它在 8.4 节的基数排序里会被发挥到极致。");
Console.WriteLine();

// ==================== 数据定义 ====================

/// <summary>订单：Id、价格、原始序号（用来判断稳定性）。</summary>
public record Order(string Id, int Price, int Seq);
