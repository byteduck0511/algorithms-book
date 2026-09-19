# 分节目标卡（Section Cards）

**说明**：`_global_defaults` 中的字段对所有卡片生效，卡片内只写差异字段。

```json
{
  "_global_defaults": {
    "book": "数据结构与算法自学：从工程直觉到复杂度思维",
    "environment": ".NET 8 / C# 12（每节均附等效伪代码）",
    "math_level": "基础",
    "word_count_range": "600-1000 中文字（不含代码块）",
    "answer_included": true,
    "checks": [
      "代码可运行，或明确标注为伪代码",
      "运行输出与文中预期一致",
      "练习答案给出关键步骤，不只给结果",
      "术语写法与 glossary.md 完全一致"
    ]
  },
  "sections": [
    {
      "section": "1.1", "title": "从\"能跑\"到\"能扛\"：为什么需要算法分析",
      "goal": "能解释同功能不同实现的性能差可达万倍，能用输入规模与基本操作描述代码代价",
      "prereq": ["循环、方法、List<T> 的基本使用"],
      "outline": ["工程事故引入", "输入规模与基本操作", "两种实现的对比推导", "实测代码"],
      "word_count": 900, "minutes": 25, "difficulty": "入门", "exercise_count": 5,
      "canonical_terms": ["数据结构", "算法", "输入规模", "基本操作", "Σ"],
      "next": "1.2 大 O 记法"
    },
    {
      "section": "1.2", "title": "大 O 记法：描述增长，而不是秒数",
      "goal": "能对给定代码写出大 O 表达式，能忽略常数与低阶项",
      "prereq": ["1.1"],
      "outline": ["为什么不用秒", "O 的定义与读法", "常见量级表", "化简规则", "例题：嵌套循环的判定"],
      "word_count": 950, "minutes": 25, "difficulty": "入门", "exercise_count": 5,
      "canonical_terms": ["大 O 记法", "渐进分析", "log n"],
      "next": "1.3 最好、最坏与平均"
    },
    {
      "section": "1.3", "title": "最好、最坏与平均：三个不同的答案",
      "goal": "能区分三种情况并说明工程中该依据哪一个",
      "prereq": ["1.2"],
      "outline": ["三种情况的定义", "以线性查找与插入排序为例", "平均情况的假设前提", "Ω 与 Θ"],
      "word_count": 850, "minutes": 22, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["Ω(f(n))", "Θ(f(n))"],
      "next": "1.4 空间复杂度与时间-空间权衡"
    },
    {
      "section": "1.4", "title": "空间复杂度与时间-空间权衡",
      "goal": "能计算算法的额外空间，能说出用空间换时间的典型场景",
      "prereq": ["1.3"],
      "outline": ["额外空间的定义", "递归的空间成本预告", "以缓存/索引为例的权衡", "摊还代价"],
      "word_count": 850, "minutes": 22, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["摊还代价"],
      "next": "2.1 递归的两个必要条件"
    },
    {
      "section": "2.1", "title": "递归的两个必要条件",
      "goal": "能写出满足基准情形与规模缩减的递归函数",
      "prereq": ["1.2"],
      "outline": ["递归的定义", "基准情形", "递归情形与规模缩减", "例题：阶乘、斐波那契、反转字符串"],
      "word_count": 900, "minutes": 25, "difficulty": "入门", "exercise_count": 5,
      "canonical_terms": ["递归", "基准情形", "递归情形"],
      "next": "2.2 调用栈：递归的代价与深度限制"
    },
    {
      "section": "2.2", "title": "调用栈：递归的代价与深度限制",
      "goal": "能画出递归调用栈，能估算递归深度并解释栈溢出",
      "prereq": ["2.1"],
      "outline": ["调用栈的物理含义", "栈帧与内存", "栈溢出复现", "尾递归为何在 C# 中不保证优化"],
      "word_count": 900, "minutes": 25, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["调用栈", "栈溢出"],
      "next": "2.3 把递归改写成迭代"
    },
    {
      "section": "2.3", "title": "把递归改写成迭代",
      "goal": "能用显式栈或循环改写递归，能判断改写是否值得",
      "prereq": ["2.2"],
      "outline": ["改写动机", "手动模拟调用栈", "两类可改写形态", "改写对照表"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "2.4 分治：切开、解决、合并"
    },
    {
      "section": "2.4", "title": "分治：切开、解决、合并",
      "goal": "能识别分治结构，能写出分治三步并估算复杂度",
      "prereq": ["2.3", "1.2"],
      "outline": ["分治三步范式", "例题：归并排序预告、快速幂、数组求和", "分治 vs 减治"],
      "word_count": 950, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["分治"],
      "next": "3.1 连续内存：数组的红利与代价"
    },
    {
      "section": "3.1", "title": "连续内存：数组的红利与代价",
      "goal": "能解释数组随机访问 O(1) 的成因，能说出中间插入的代价",
      "prereq": ["1.2"],
      "outline": ["内存地址与下标换算", "为什么访问是 O(1)", "插入删除为何是 O(n)", "缓存友好性"],
      "word_count": 900, "minutes": 25, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["数组"],
      "next": "3.2 动态数组与扩容的摊还代价"
    },
    {
      "section": "3.2", "title": "动态数组与扩容的摊还代价",
      "goal": "能解释 List<T> 的扩容策略，能算出摊还 O(1)",
      "prereq": ["3.1", "1.4"],
      "outline": ["定长数组的困境", "倍增扩容", "摊还分析速算", "Capacity 的工程用法"],
      "word_count": 950, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["动态数组", "⌊x⌋"],
      "next": "3.3 双指针技巧"
    },
    {
      "section": "3.3", "title": "双指针技巧",
      "goal": "能用相向/同向双指针把 O(n²) 降为 O(n)",
      "prereq": ["3.1"],
      "outline": ["相向双指针：有序数组两数之和", "同向双指针：原地去重", "正确性直觉"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["双指针"],
      "next": "3.4 滑动窗口"
    },
    {
      "section": "3.4", "title": "滑动窗口",
      "goal": "能识别窗口型问题并写出左右边界推进的模板",
      "prereq": ["3.3"],
      "outline": ["窗口的定义", "模板四步", "例题：最长无重复子串、最小覆盖子串", "窗口 vs 前缀和"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["滑动窗口"],
      "next": "4.1 节点与引用：链表的本质"
    },
    {
      "section": "4.1", "title": "节点与引用：链表的本质",
      "goal": "能画出链表的内存图，能定义 Node 类",
      "prereq": ["3.1"],
      "outline": ["引用即地址", "节点结构", "头指针与空链表", "与数组的取舍表"],
      "word_count": 850, "minutes": 25, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["链表", "节点"],
      "next": "4.2 单链表、双向链表与循环链表"
    },
    {
      "section": "4.2", "title": "单链表、双向链表与循环链表",
      "goal": "能说清三种链表的操作代价差异与适用场景",
      "prereq": ["4.1"],
      "outline": ["三种形态图解", "prev 指针换来的好处", "循环链表的终止条件", "C# LinkedList<T> 对照"],
      "word_count": 900, "minutes": 26, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "4.3 插入与删除：边界处理是全部难点"
    },
    {
      "section": "4.3", "title": "插入与删除：边界处理是全部难点",
      "goal": "能手写插入/删除并通过头、尾、单元素、空表四类边界测试",
      "prereq": ["4.2"],
      "outline": ["四类边界", "哨兵节点消除特判", "完整实现与测试", "常见空引用错误"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["哨兵节点"],
      "next": "4.4 快慢指针与经典应用"
    },
    {
      "section": "4.4", "title": "快慢指针与经典应用",
      "goal": "能用快慢指针判断环、找中点、找倒数第 k 个节点",
      "prereq": ["4.3"],
      "outline": ["速度差原理", "判环与找环入口的推导", "找中点", "一次遍历找倒数第 k 个"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["快慢指针"],
      "next": "5.1 栈：后进先出"
    },
    {
      "section": "5.1", "title": "栈：后进先出",
      "goal": "能实现基于数组的栈并说明各操作代价",
      "prereq": ["3.2"],
      "outline": ["LIFO 直觉：撤销/回退", "push/pop/peek 实现", "栈溢出与空栈错误", "C# Stack<T> 用法"],
      "word_count": 850, "minutes": 24, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["栈"],
      "next": "5.2 队列与双端队列：先进先出"
    },
    {
      "section": "5.2", "title": "队列与双端队列：先进先出",
      "goal": "能用循环数组实现队列，并解释为什么不能简单用 List 出队",
      "prereq": ["5.1"],
      "outline": ["FIFO 直觉：排队/缓冲", "环形缓冲区实现", "双端队列", "C# Queue<T> / LinkedList 对照"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["队列", "双端队列"],
      "next": "5.3 应用：括号匹配与表达式求值"
    },
    {
      "section": "5.3", "title": "应用：括号匹配与表达式求值",
      "goal": "能用栈实现括号匹配与中缀表达式求值",
      "prereq": ["5.2"],
      "outline": ["括号匹配", "中缀转后缀", "后缀求值", "完整可运行计算器"],
      "word_count": 950, "minutes": 35, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": [],
      "next": "5.4 单调栈与单调队列"
    },
    {
      "section": "5.4", "title": "单调栈与单调队列",
      "goal": "能用单调栈求解下一个更大元素，用单调队列求滑动窗口最大值",
      "prereq": ["5.3"],
      "outline": ["单调栈的维护规则", "下一个更大元素", "单调队列", "窗口最大值 O(n)"],
      "word_count": 950, "minutes": 32, "difficulty": "挑战", "exercise_count": 5,
      "canonical_terms": ["单调栈"],
      "next": "6.1 从数组下标到哈希函数"
    },
    {
      "section": "6.1", "title": "从数组下标到哈希函数",
      "goal": "能解释哈希表如何用 O(1) 完成查找，能写出简单哈希函数",
      "prereq": ["3.1"],
      "outline": ["直接寻址表的局限", "哈希函数的作用", "取模映射", "一个手写哈希函数"],
      "word_count": 900, "minutes": 26, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["哈希表", "哈希函数"],
      "next": "6.2 冲突解决：链地址法与开放寻址"
    },
    {
      "section": "6.2", "title": "冲突解决：链地址法与开放寻址",
      "goal": "能实现链地址法哈希表，能说清两种方案的取舍",
      "prereq": ["6.1"],
      "outline": ["冲突不可避免", "链地址法实现", "开放寻址与探测序列", "删除的麻烦", "对比表"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["冲突", "链地址法", "开放寻址"],
      "next": "6.3 负载因子、扩容与再哈希"
    },
    {
      "section": "6.3", "title": "负载因子、扩容与再哈希",
      "goal": "能解释负载因子如何影响性能，能实现扩容",
      "prereq": ["6.2"],
      "outline": ["负载因子定义", "冲突率与链表长度", "扩容与再哈希", "摊还代价"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["负载因子", "再哈希"],
      "next": "6.4 C# Dictionary 的内部与工程用法"
    },
    {
      "section": "6.4", "title": "C# Dictionary 的内部与工程用法",
      "goal": "能预判 Dictionary 的行为（顺序、null、并发、扩容），能避免常见误用",
      "prereq": ["6.3"],
      "outline": ["结果顺序为何不确定", "Equals/GetHashCode 契约", "TryGetValue 模式", "并发的坑", "键类型选择"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": [],
      "next": "7.1 冒泡、选择、插入：三种 O(n²) 排序"
    },
    {
      "section": "7.1", "title": "冒泡、选择、插入：三种 O(n²) 排序",
      "goal": "能手写三种排序并说明各自的最好/最坏情况",
      "prereq": ["3.1", "1.3"],
      "outline": ["三种算法逐步演示", "交换次数对比", "插入排序为何在近乎有序时很快", "代码与实测"],
      "word_count": 950, "minutes": 30, "difficulty": "入门", "exercise_count": 5,
      "canonical_terms": [],
      "next": "7.2 稳定性：同分时谁在前"
    },
    {
      "section": "7.2", "title": "稳定性：同分时谁在前",
      "goal": "能判断一种排序是否稳定，能说明稳定性在工程中的意义",
      "prereq": ["7.1"],
      "outline": ["稳定性定义", "以订单按时间/金额双关键字排序为例", "三种基础排序的稳定性判定", "不稳定时如何补救"],
      "word_count": 850, "minutes": 24, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["排序稳定性"],
      "next": "7.3 比较模型与交换次数"
    },
    {
      "section": "7.3", "title": "比较模型与交换次数",
      "goal": "能统计比较与交换次数，能理解逆序对与插入排序的关系",
      "prereq": ["7.2"],
      "outline": ["比较模型的定义", "逆序对", "插入排序交换次数 = 逆序对数", "实测计数"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["逆序对"],
      "next": "7.4 下界：为什么比较排序绕不开 n log n"
    },
    {
      "section": "7.4", "title": "下界：为什么比较排序绕不开 n log n",
      "goal": "能复述决策树下界论证，并能说明它不适用于非比较排序",
      "prereq": ["7.3"],
      "outline": ["决策树模型", "叶子数 n! 的推导", "下界结论", "边界条件：它约束的是谁"],
      "word_count": 900, "minutes": 30, "difficulty": "挑战", "exercise_count": 3,
      "canonical_terms": [],
      "next": "8.1 归并排序：稳定且可预测"
    },
    {
      "section": "8.1", "title": "归并排序：稳定且可预测",
      "goal": "能手写归并排序，能用递推式说明其复杂度",
      "prereq": ["2.4", "7.4"],
      "outline": ["合并两个有序数组", "自顶向下递归实现", "复杂度递推 T(n)=2T(n/2)+O(n)", "空间代价与工程变体"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": [],
      "next": "8.2 快速排序：平均最快，最坏要防"
    },
    {
      "section": "8.2", "title": "快速排序：平均最快，最坏要防",
      "goal": "能手写快排与分区，能解释最坏情况成因与规避手段",
      "prereq": ["8.1"],
      "outline": ["分区过程逐步演示", "递归实现", "最坏 O(n²) 的成因", "随机化与三数取中", "与归并的对比"],
      "word_count": 1000, "minutes": 35, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": [],
      "next": "8.3 堆与堆排序"
    },
    {
      "section": "8.3", "title": "堆与堆排序",
      "goal": "能实现堆排序并说明它与快排、归并的取舍",
      "prereq": ["8.2"],
      "outline": ["堆序回顾", "建堆", "原地排序过程", "复杂度与缓存表现", "三者对比表"],
      "word_count": 900, "minutes": 30, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "8.4 非比较排序：计数排序与基数排序"
    },
    {
      "section": "8.4", "title": "非比较排序：计数排序与基数排序",
      "goal": "能实现计数排序与基数排序，能说清它们的适用边界",
      "prereq": ["7.4"],
      "outline": ["跳出比较模型", "计数排序与稳定性", "基数排序（LSD）", "适用条件与内存代价"],
      "word_count": 900, "minutes": 30, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "8.5 工程中如何选排序"
    },
    {
      "section": "8.5", "title": "工程中如何选排序",
      "goal": "能按数据特征与需求选择排序方案，并能解释 C# 排序 API 的行为",
      "prereq": ["8.4"],
      "outline": ["决策流程图", "Array.Sort / List.Sort 是内省排序", "OrderBy 是稳定排序", "实测对比表"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "9.1 树的术语与二叉树的形态"
    },
    {
      "section": "9.1", "title": "树的术语与二叉树的形态",
      "goal": "能准确使用根/叶/深度/高度等术语，能算出满二叉树的节点数",
      "prereq": ["2.1"],
      "outline": ["术语表", "深度与高度", "满/完全/完美二叉树", "2^h 与 log n 的关系"],
      "word_count": 850, "minutes": 24, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["2^h"],
      "next": "9.2 深度优先遍历：前序、中序、后序"
    },
    {
      "section": "9.2", "title": "深度优先遍历：前序、中序、后序",
      "goal": "能写出三种递归遍历与统一迭代模板",
      "prereq": ["9.1", "2.2"],
      "outline": ["三种顺序的定义与用途", "递归实现", "统一迭代模板", "Morris 遍历简介"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": [],
      "next": "9.3 广度优先遍历与层序"
    },
    {
      "section": "9.3", "title": "广度优先遍历与层序",
      "goal": "能用队列实现层序遍历并处理按层分组",
      "prereq": ["9.2", "5.2"],
      "outline": ["BFS 模板", "记录层号", "按层分组输出", "与 DFS 的适用场景对比"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "9.4 树题递归套路：返回值该是什么"
    },
    {
      "section": "9.4", "title": "树题递归套路：返回值该是什么",
      "goal": "能根据题目要求设计递归函数的返回值与语义",
      "prereq": ["9.3"],
      "outline": ["自顶向下 vs 自底向上", "返回值设计三问", "例题：最大深度、平衡判断、最近公共祖先"],
      "word_count": 1000, "minutes": 35, "difficulty": "挑战", "exercise_count": 5,
      "canonical_terms": [],
      "next": "10.1 二叉搜索树的有序性"
    },
    {
      "section": "10.1", "title": "二叉搜索树的有序性",
      "goal": "能判断一棵树是否为 BST，能说明中序遍历与有序序列的关系",
      "prereq": ["9.2"],
      "outline": ["BST 定义", "中序有序的推论", "验证 BST 的常见错误写法", "有序性带来的能力"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["二叉搜索树（Binary Search Tree, BST）"],
      "next": "10.2 查找、插入、删除"
    },
    {
      "section": "10.2", "title": "查找、插入、删除",
      "goal": "能实现 BST 三种操作，能正确处理删除的两子节点情形",
      "prereq": ["10.1"],
      "outline": ["查找与插入", "删除的三种情形", "用后继替换", "完整实现与测试"],
      "word_count": 1000, "minutes": 35, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": [],
      "next": "10.3 退化问题：为什么需要平衡"
    },
    {
      "section": "10.3", "title": "退化问题：为什么需要平衡",
      "goal": "能构造退化的 BST 并解释其对性能的影响",
      "prereq": ["10.2"],
      "outline": ["有序插入导致退化成链表", "性能对比实测", "平衡的目标", "实际工作中的概率"],
      "word_count": 850, "minutes": 24, "difficulty": "中级", "exercise_count": 3,
      "canonical_terms": [],
      "next": "10.4 平衡树思想：AVL 与红黑树概览"
    },
    {
      "section": "10.4", "title": "平衡树思想：AVL 与红黑树概览",
      "goal": "能解释旋转的作用，能说出 AVL 与红黑树的取舍",
      "prereq": ["10.3"],
      "outline": ["平衡因子", "四种旋转图示", "AVL vs 红黑树", "C# 中的 SortedDictionary 与 SortedSet"],
      "word_count": 950, "minutes": 32, "difficulty": "挑战", "exercise_count": 4,
      "canonical_terms": ["平衡因子"],
      "next": "11.1 堆的定义与数组表示"
    },
    {
      "section": "11.1", "title": "堆的定义与数组表示",
      "goal": "能用数组表示完全二叉树，能写出父子下标公式",
      "prereq": ["9.1", "3.1"],
      "outline": ["堆序性质", "大顶堆与小顶堆", "下标换算 2i+1 / 2i+2", "为什么用数组而不用链"],
      "word_count": 850, "minutes": 25, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["堆（Heap）"],
      "next": "11.2 上浮与下沉"
    },
    {
      "section": "11.2", "title": "上浮与下沉",
      "goal": "能手写 siftUp 与 siftDown 并通过断言测试",
      "prereq": ["11.1"],
      "outline": ["上浮的触发与过程", "下沉的触发与过程", "为什么这两种操作足够", "复杂度 log n 的由来"],
      "word_count": 900, "minutes": 30, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["上浮", "下沉"],
      "next": "11.3 建堆与堆排序"
    },
    {
      "section": "11.3", "title": "建堆与堆排序",
      "goal": "能解释自底向上建堆为何是 O(n)，能实现原地堆排序",
      "prereq": ["11.2"],
      "outline": ["逐个插入建堆 O(n log n)", "自底向上建堆 O(n) 的推导", "堆排序过程", "与快排对比"],
      "word_count": 950, "minutes": 32, "difficulty": "挑战", "exercise_count": 4,
      "canonical_terms": [],
      "next": "11.4 Top-K 与优先队列的工程用法"
    },
    {
      "section": "11.4", "title": "Top-K 与优先队列的工程用法",
      "goal": "能用堆解决 Top-K、合并 K 个有序序列、任务调度",
      "prereq": ["11.3"],
      "outline": ["Top-K 用多大顶堆", "为什么不用全排序", "合并 K 个有序链表", "C# PriorityQueue 用法"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["优先队列"],
      "next": "12.1 图的基本概念与两种存储方式"
    },
    {
      "section": "12.1", "title": "图的基本概念与两种存储方式",
      "goal": "能根据场景选择邻接矩阵或邻接表，能建图",
      "prereq": ["4.1", "3.1"],
      "outline": ["顶点与边", "有向/无向/带权", "邻接矩阵", "邻接表", "空间与操作代价对比"],
      "word_count": 950, "minutes": 30, "difficulty": "入门", "exercise_count": 4,
      "canonical_terms": ["图", "顶点", "边", "邻接矩阵", "邻接表"],
      "next": "12.2 深度优先搜索"
    },
    {
      "section": "12.2", "title": "深度优先搜索",
      "goal": "能写递归与迭代两种 DFS，能处理 visited 标记",
      "prereq": ["12.1", "2.2"],
      "outline": ["DFS 的思想", "递归实现", "迭代实现", "visited 的作用与时机", "回溯的区别"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["深度优先搜索（Depth-First Search, DFS）"],
      "next": "12.3 广度优先搜索"
    },
    {
      "section": "12.3", "title": "广度优先搜索",
      "goal": "能用 BFS 求无权图最短路径并记录路径",
      "prereq": ["12.2", "5.2"],
      "outline": ["BFS 的思想", "队列实现", "为什么 BFS 能保证最短", "记录前驱还原路径", "迷宫实例"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["广度优先搜索（Breadth-First Search, BFS）"],
      "next": "12.4 连通分量、环检测与二分图"
    },
    {
      "section": "12.4", "title": "连通分量、环检测与二分图",
      "goal": "能用 DFS/BFS 求连通分量、检测环、判断二分图",
      "prereq": ["12.3"],
      "outline": ["连通分量计数", "无向图环检测", "有向图环检测与三色标记", "二分图判定"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["连通分量"],
      "next": "13.1 拓扑排序与依赖解析"
    },
    {
      "section": "13.1", "title": "拓扑排序与依赖解析",
      "goal": "能实现 Kahn 算法拓扑排序并检测环",
      "prereq": ["12.4"],
      "outline": ["DAG 与依赖", "入度与 Kahn 算法", "DFS 后序解法", "课程表 / 构建顺序实例"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["拓扑排序", "入度"],
      "next": "13.2 无权最短路：BFS 的正确用法"
    },
    {
      "section": "13.2", "title": "无权最短路：BFS 的正确用法",
      "goal": "能区分 BFS 最短路与 Dijkstra 的适用边界",
      "prereq": ["13.1", "12.3"],
      "outline": ["无权图的最短路", "与 12.3 的差异（多终点/剪枝）", "双向 BFS 提速", "何时必须换 Dijkstra"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["最短路径"],
      "next": "13.3 Dijkstra 算法"
    },
    {
      "section": "13.3", "title": "Dijkstra 算法",
      "goal": "能用优先队列实现 Dijkstra，能解释贪心正确性的前提",
      "prereq": ["13.2", "11.4"],
      "outline": ["松弛操作", "贪心选择与正确性直觉", "优先队列实现", "复杂度分析", "为什么不能有负权边"],
      "word_count": 1000, "minutes": 38, "difficulty": "挑战", "exercise_count": 5,
      "canonical_terms": ["松弛"],
      "next": "13.4 负权边与 Bellman-Ford 简介"
    },
    {
      "section": "13.4", "title": "负权边与 Bellman-Ford 简介",
      "goal": "能说明 Bellman-Ford 的适用场景与负环检测",
      "prereq": ["13.3"],
      "outline": ["Dijkstra 失效的反例", "Bellman-Ford 的 n-1 轮松弛", "负环检测", "SPFA 简介与争议"],
      "word_count": 900, "minutes": 28, "difficulty": "挑战", "exercise_count": 3,
      "canonical_terms": [],
      "next": "14.1 贪心的适用条件"
    },
    {
      "section": "14.1", "title": "贪心的适用条件",
      "goal": "能判断一个问题是否适合贪心，能说出贪心与 DP 的分界",
      "prereq": ["13.3"],
      "outline": ["贪心的定义", "局部最优到全局最优的两个前提", "与 DP 的关系", "验证贪心的两条路径"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": ["贪心算法"],
      "next": "14.2 区间调度与 Huffman 编码"
    },
    {
      "section": "14.2", "title": "区间调度与 Huffman 编码",
      "goal": "能实现区间调度与 Huffman 编码，能说明各自的贪心选择依据",
      "prereq": ["14.1", "11.4"],
      "outline": ["区间调度：按结束时间排序", "Huffman 编码构造", "前缀码的意义", "完整实现"],
      "word_count": 950, "minutes": 35, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "14.3 贪心失效的反例"
    },
    {
      "section": "14.3", "title": "贪心失效的反例",
      "goal": "能构造贪心失效的反例，能识别需要改用 DP 的信号",
      "prereq": ["14.2"],
      "outline": ["0/1 背包的贪心反例", "找零问题的币制依赖", "贪心失败的四类信号", "转向 DP 的判断"],
      "word_count": 900, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "15.1 从暴力递归到记忆化"
    },
    {
      "section": "15.1", "title": "从暴力递归到记忆化",
      "goal": "能识别重叠子问题，能把暴力递归改成记忆化搜索",
      "prereq": ["14.3", "2.1"],
      "outline": ["斐波那契的暴力递归有多慢", "重叠子问题的识别", "记忆化改写", "自顶向下 vs 自底向上"],
      "word_count": 950, "minutes": 32, "difficulty": "中级", "exercise_count": 5,
      "canonical_terms": ["动态规划（Dynamic Programming, DP）", "记忆化"],
      "next": "15.2 状态定义与转移方程"
    },
    {
      "section": "15.2", "title": "状态定义与转移方程",
      "goal": "能独立定义状态并写出转移方程",
      "prereq": ["15.1"],
      "outline": ["什么是状态", "状态定义的四个自检问题", "转移方程的写法", "例题：爬楼梯、打家劫舍、最长递增子序列"],
      "word_count": 1000, "minutes": 38, "difficulty": "挑战", "exercise_count": 5,
      "canonical_terms": ["状态转移方程"],
      "next": "15.3 经典题型：背包与最长递增子序列"
    },
    {
      "section": "15.3", "title": "经典题型：背包与最长递增子序列",
      "goal": "能实现 0/1 背包与 LIS，并解释遍历顺序的由来",
      "prereq": ["15.2"],
      "outline": ["0/1 背包二维写法", "一维写法与逆序遍历的原因", "完全背包对比", "LIS 的 O(n²) 与 O(n log n)"],
      "word_count": 1000, "minutes": 40, "difficulty": "挑战", "exercise_count": 5,
      "canonical_terms": [],
      "next": "15.4 空间优化与滚动数组"
    },
    {
      "section": "15.4", "title": "空间优化与滚动数组",
      "goal": "能把 DP 表从二维压缩到一维，能判断压缩是否安全",
      "prereq": ["15.3"],
      "outline": ["状态依赖关系分析", "滚动数组", "原地覆盖的条件", "压缩带来的可读性代价"],
      "word_count": 900, "minutes": 30, "difficulty": "挑战", "exercise_count": 4,
      "canonical_terms": [],
      "next": "16.1 选型手册：按场景选数据结构"
    },
    {
      "section": "16.1", "title": "选型手册：按场景选数据结构",
      "goal": "能按操作特征（查/增/删/序/范围）快速选定结构",
      "prereq": ["第 1–15 章"],
      "outline": ["按操作建索引的速查表", "常见误用案例", "内存与并发的额外考量", "决策流程"],
      "word_count": 950, "minutes": 28, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "16.2 从复杂度到实测：性能剖析"
    },
    {
      "section": "16.2", "title": "从复杂度到实测：性能剖析",
      "goal": "能用 BenchmarkDotNet 思路做对比实测，能区分复杂度与实际性能",
      "prereq": ["16.1"],
      "outline": ["理论 vs 实测的差距来源", "缓存、分支预测、GC", "基准测试的常见陷阱", "一个完整对比实验"],
      "word_count": 950, "minutes": 30, "difficulty": "中级", "exercise_count": 4,
      "canonical_terms": [],
      "next": "16.3 综合项目：一个任务调度器"
    },
    {
      "section": "16.3", "title": "综合项目：一个任务调度器",
      "goal": "能综合运用拓扑排序、优先队列、哈希表完成可运行项目",
      "prereq": ["第 12–15 章"],
      "outline": ["需求与约束", "依赖解析（拓扑排序）", "优先级调度（堆）", "状态跟踪（哈希表）", "完整代码与验收标准"],
      "word_count": 1000, "minutes": 60, "difficulty": "挑战", "exercise_count": 3,
      "canonical_terms": [],
      "next": "（全书完）"
    }
  ]
}
```
