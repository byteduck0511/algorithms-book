# 数据结构与算法自学：从工程直觉到复杂度思维

一本写给**有编程经验、但没系统学过数据结构与算法**的工程师的自学书。

> ## ✅ 全书完成（63/63 节）
>
> **要继续写这本书，请先读 [`00-plan/HANDOFF.md`](00-plan/HANDOFF.md)** ——
> 那份文档包含项目全貌、写作流程与所有约定 —— **写作已完成，但它同时是这本书的"制作说明"**。
> **它假定你完全不知道之前的对话，只依赖仓库里的文件。**

## 读者定位

| 项 | 说明 |
|---|---|
| 受众 | 有 1–3 年工作经验，会写循环、函数、类，用过 `List<T>` 和 `Dictionary` |
| 不假设 | 不需要高等数学，不需要算法基础，不需要竞赛经历 |
| 代码 | C# 12 / .NET 8（每段代码均附等效伪代码，可用任意语言对照） |
| 数学 | 只用四则运算、乘方、对数概念、求和。不涉及微积分与概率论 |
| 深度 | 中级：覆盖本科数据结构与算法核心，砍掉严格证明与冷门结构 |

## 如何使用

**建议节奏**：16 周，每周 1 章，每章 3–5 节，每周投入 4–6 小时。

每节的结构是固定的：

```
学习目标 → 直觉 → 形式化 → 例题 → 可运行代码 → 练习 → 答案 → 常见错误 → 本节总结 → 状态外显
```

**每节的代码都请亲手跑一遍。** 每节的练习都有完整答案（含关键步骤，不只是结果）。

详细的几条学习路径（标准路径 / 面试急救路径 / 工程补强路径）见 `00-plan/timeline.md`。

## 当前状态（**全书完成**，63/63 节）

| 指标 | 数值 |
|---|---|
| 已完成章节 | **第 1–16 章全部完成，共 63 节** |
| 正文中文字数 | 约 **20.3 万字**（不含代码块） |
| 平均每节 | 约 3,130 字 |
| 代码项目 | **63 个，全部编译运行通过** |
| 术语表 | 200 条（另含符号表 7 条，合计 207 条） |
| 审校状态 | ✅ 结构完整、无悬空引用、状态卡全部有效 |

⚠️ **字数偏离说明**：技能默认单节 600–1000 字，本书单节区间为 **1,500–6,400 字**，实际平均约 3,100 字。
主要原因是**练习答案**（占 35–45%）和**反预期实测的完整分析** —— 两者都无法压缩。
详见 **`00-plan/review-report.md`**。

**全书索引见 [`chapters/INDEX.md`](chapters/INDEX.md)** —— 含按章索引、按主题索引、25 处反预期实测汇总、按问题类型索引。

## 目录结构

```
00-plan/          规划产物（不在成书中）
  HANDOFF.md        ★ 交接文档：接手写这本书先读这个
  blueprint.md      蓝图：受众、目标、必须包含/避免、偏离声明
  toc.md            目录：7 部分 / 16 章 / 63 节
  roles.md          角色表：小陈、老周、贯穿场景
  glossary.md       固定术语表 + 符号表（术语 200 条 / 符号 7 条，全书禁止同义替换）
  timeline.md       16 周学习路径 + 自测检查点
  section-cards.md  63 张分节目标卡（含第 13–16 章的）
  review-report.md  审校报告：结构/引用/术语/字数的完整检查结果

chapters/         正文
  ch01/             第 1 章：算法思维与复杂度分析
    1.1-why-algorithm-analysis.md
    1.2-big-o-notation.md
    1.3-best-worst-average.md
    1.4-space-and-tradeoffs.md
  ch02/             第 2 章：递归与分治
    2.1-two-conditions-of-recursion.md
    2.2-call-stack.md
    2.3-recursion-to-iteration.md
    2.4-divide-and-conquer.md
  ch03/             第 3 章：数组与字符串
    3.1-contiguous-memory.md
    3.2-dynamic-array.md
    3.3-two-pointers.md
    3.4-sliding-window.md
  ch04/             第 4 章：链表
    4.1-nodes-and-references.md
    4.2-linked-list-variants.md
    4.3-insert-delete-boundaries.md
    4.4-fast-and-slow-pointers.md
  ch05/             第 5 章：栈与队列
    5.1-stack.md
    5.2-queue-and-deque.md
    5.3-bracket-and-expression.md
    5.4-monotonic-stack-and-queue.md
  ch06/             第 6 章：哈希表
    6.1-from-index-to-hash.md
    6.2-collision-resolution.md
    6.3-load-factor-and-resize.md
    6.4-dictionary-in-practice.md
  ch07/             第 7 章：基础排序与比较模型
    7.1-quadratic-sorts.md
    7.2-stability.md
    7.3-inversions-and-comparison-model.md
    7.4-lower-bound.md
  ch08/             第 8 章：高效排序
    8.1-merge-sort.md
    8.2-quick-sort.md
    8.3-heap-sort.md
    8.4-non-comparison-sorts.md
    8.5-choosing-a-sort.md
  ch09/             第 9 章：二叉树与遍历
    9.1-tree-basics.md
    9.2-dfs-traversals.md
    9.3-bfs-level-order.md
    9.4-recursion-patterns.md
  ch10/             第 10 章：二叉搜索树与平衡思想
    10.1-bst-ordering.md
    10.2-search-insert-delete.md
    10.3-degradation.md
    10.4-balancing-trees.md
  ch11/             第 11 章：堆与优先队列
    11.1-heap-basics.md
    11.2-sift-up-down.md
    11.3-priority-queue.md
    11.4-top-k-and-practice.md
  ch12/             第 12 章：图与遍历
    12.1-graph-basics.md
    12.2-dfs.md
    12.3-bfs.md
    12.4-components-cycles-bipartite.md
  ch13/             第 13 章：最短路径与拓扑排序
    13.1-topological-sort.md
    13.2-unweighted-shortest-path.md
    13.3-dijkstra.md
    13.4-bellman-ford.md
  ch14/             第 14 章：贪心
    14.1-when-greedy-works.md
    14.2-interval-scheduling-and-huffman.md
    14.3-when-greedy-fails.md
  ch15/             第 15 章：动态规划
    15.1-from-recursion-to-memoization.md
    15.2-state-and-transition.md
    15.3-knapsack-and-lis.md
    15.4-space-optimization.md
  ch16/             第 16 章：工程实践与综合
    16.1-choosing-data-structures.md
    16.2-from-complexity-to-measurement.md
    16.3-task-scheduler.md

99-tools/         代码与验证脚本、构建工具（不在成书中）
  build-ebook.ps1   一键生成 EPUB + PDF（见下文「构建网页版与电子书」）
  build_ebook.py    拼接 63 节为 book.md，剥离「状态外显」元数据块
  ebook.css         电子书排版样式（中文字体、行高、代码折行）
  print.css         PDF 打印样式（A4 分页、章另起一页）
  plugins/          两个自建 HonKit 插件
    gitbook-plugin-zh-search/   中文全文搜索（2-gram 分词，替代 lunr）
    gitbook-plugin-zh-katex/    前端 KaTeX 数学渲染
  samples/          各节的完整可运行项目，一节一个
    Ch01/Sec11/ Sec12/ Sec13/ Sec14/
    Ch02/Sec21/ Sec22/ Sec23/ Sec24/
    Ch03/Sec31/ Sec32/ Sec33/ Sec34/
    Ch04/Sec41/ Sec42/ Sec43/ Sec44/
    Ch05/Sec51/ Sec52/ Sec53/ Sec54/
    Ch06/Sec61/ Sec62/ Sec63/ Sec64/
    Ch07/Sec71/ Sec72/ Sec73/ Sec74/
    Ch08/Sec81/ Sec82/ Sec83/ Sec84/ Sec85/
    Ch09/Sec91/ Sec92/ Sec93/ Sec94/
    Ch10/Sec101/ Sec102/ Sec103/ Sec104/
    Ch11/Sec111/ Sec112/ Sec113/ Sec114/
    Ch12/Sec121/ Sec122/ Sec123/ Sec124/
    Ch13/Sec131/ Sec132/ Sec133/ Sec134/
    Ch14/Sec141/ Sec142/ Sec143/
    Ch15/Sec151/ Sec152/ Sec153/ Sec154/
    Ch16/Sec161/ Sec162/ Sec163/
  verify_1_1.py     1.1 节的独立交叉验证脚本（Python）
```

> **运行 2.2 / 2.3 节代码时请注意**：`Ch02/Sec22` 会**故意触发栈溢出**让进程崩溃退出（这是教学内容）；`Ch02/Sec23` 会启动一个 64 MB 栈的线程。两者都不是 bug。
>
> **13.1 节同理**：`Ch13/Sec131` 正常运行时跑完五个实验；加 `-- --deep` 参数时，实验五会**故意触发栈溢出**让进程崩溃（退出码 `0xC00000FD`），这也是教学内容。

## 构建网页版与电子书

书有三种成品形态：**网页版**、**EPUB**、**PDF**。

### 网页版

```bash
npm install
npm run build      # 产物在 _book/，是纯静态站点，可直接部署
npm run serve      # 本地预览 http://localhost:4000
```

默认的两个插件对这本书都不够用，因此自建了两个（源码在 `99-tools/plugins/`）：

| 插件 | 解决的问题 |
|---|---|
| `zh-search` | HonKit 默认的 lunr **对中文不可用**：它按空白分词，中文整段变成一个 token，索引里只剩 910 个单字；而且索引序列化后内部结构不一致，搜「递归」会让搜索框直接抛异常。实测索引 **14.32 MB**。改为中文 2-gram 分词 + 排除代码块后降到 **1.83 MB（-87%）**，搜「递归」26 条命中且首位是 2.1 节。 |
| `zh-katex` | HonKit 不带数学渲染，全书 **3,566 个行内公式 + 252 个块级公式**原本显示为裸 LaTeX。`honkit-plugin-katex` 走构建时短路，会被书里 C# 插值字符串 `$"..."` 打乱 `$` 配对而**构建失败**（44 节含此写法），故改为前端渲染并跳过 `pre`/`code`。 |

> ⚠️ 改完插件源码要重跑 `npm install` —— `file:` 依赖是**复制**而非符号链接，不重装则产物里还是旧代码。

### 电子书（EPUB / PDF）

```powershell
powershell -ExecutionPolicy Bypass -File 99-tools\build-ebook.ps1
```

产物在 `_ebook/`：`algorithms-book.epub`（0.75 MB）与 `algorithms-book.pdf`（25.9 MB / 729 页）。

流程四步：拼接源 → pandoc 出 EPUB → pandoc 出 HTML → Edge headless 打印 PDF。四个坑已在脚本里处理：

1. **PDF 不走 LaTeX。** 本机没有任何 LaTeX 引擎，而全书有 3,818 个公式。Chromium 自带 MathML 渲染，配合 `pandoc --mathml` 可完全离线出 PDF，不必装几 GB 的 TeX Live。
2. **引号方向会反。** 源文件通篇用英文直引号 `"`，pandoc 的 smart 转换按英文规则判开合；中文里 `个"重复"` 的引号前面是汉字，会被判成闭引号，输出成 `个”重复”`。故由 `build_ebook.py` 自行转换（跳过代码围栏与行内代码），并给 pandoc 加 `-smart`。
3. **列表会被压成一行。** pandoc 的 markdown 默认要求列表前有空行，而每节的「学习目标」「先修」后面直接跟 `- ` 列表，需要开 `+lists_without_preceding_blankline`。
4. **「状态外显」不属于成书。** 每节末尾的 JSON 是写作流程的元数据（覆盖点、实测数据、遗留问题），`build_ebook.py` 会整块剥离（63 处）。

## 运行书中的代码

每节代码都是一个独立的控制台项目。安装 [.NET SDK](https://dotnet.microsoft.com/download) 后：

```bash
dotnet run --project 99-tools/samples/Ch01/Sec11/Sec11.csproj -c Release
```

请务必加 `-c Release`。Debug 模式下的计时结果没有参考价值。

## 代码验证标准

本书对代码的要求是**可复现**，具体做法：

1. **每段 C# 代码都在 .NET 8 Release 下实跑**，正文中的输出是从控制台直接复制的。
2. **涉及算法正确性的结论，另用独立实现交叉校验**（如 `verify_1_1.py`），避免"自己验证自己"。
3. **优先用「操作计数」而非「计时」** 展示复杂度（1.2 节、1.3 节）—— 操作次数与机器无关，任何电脑上跑出来都是同一张表。
4. 计时类实验一律注明**机器配置**，并提示"你的数字会不同，倍率关系才重要"。

## 进度

| 章节 | 状态 | 实测亮点 |
|---|---|---|
| 第 1 章 算法思维与复杂度分析 | ✅ 完成（4/4 节） | $n$ 涨 10 倍时慢速实现涨 85.6 倍；前缀和提速 170 倍 |
| 第 2 章 递归与分治 | ✅ 完成（4/4 节） | 栈溢出复现（16,074 层，`0xC00000FD`）；Fib(40) 迭代比递归快约 30 万倍 |
| 第 3 章 数组与字符串 | ✅ 完成（4/4 节） | 用 unsafe 读到地址偏移 0/4/8/12/16；顺序 vs 随机访问差 15 倍；双指针比暴力快 10,000 倍 |
| 第 4 章 链表 | ✅ 完成（4/4 节） | 链表内存开销 8 倍；节点紧凑 vs 分散遍历差 2 倍；链表头插比 `List.Insert(0)` 快 843 倍；快慢指针判环 0 KB vs 哈希表 51.4 MB |
| 第 5 章 栈与队列 | ✅ 完成（4/4 节） | 栈弹出不清引用导致对象无法回收（`WeakReference` 实测）；`List.RemoveAt(0)` 当队列慢 70 倍；单调栈/队列在降序输入下快 17 倍 |
| 第 6 章 哈希表 | ✅ 完成（4/4 节） | 字符求和哈希让字母重排词全部碰撞；开放寻址探测更多却快 2.8 倍；负载因子 0.95 时失败查找要探测 168 次；`int` 键比 `string` 键快 7.84 倍 |
| 第 7 章 基础排序 | ✅ 完成（4/4 节） | 近乎有序时插入排序比冒泡快 154 倍；选择排序把 `A C E` 变成 `A E C`（不稳定）；**逆序对 = 交换次数**三方实测完全相等；随机逆序对稳定在 $n^2/4$ |
| 第 8 章 高效排序 | ✅ 完成（5/5 节） | 已排序数据上插入排序比归并**快 8.5 倍**；朴素快排退化到 **376 倍**；计数排序比快排快 **447 倍**；`Array.Sort` 不稳定 vs `OrderBy` 稳定 |
| 第 9 章 二叉树与遍历 | ✅ 完成（4/4 节） | 平衡树高度 19 vs 退化链 999,999；三种遍历的递归/迭代版结果完全一致；链状树上朴素平衡判断**反而更快**（提前失败），平衡树上才慢 9.3 倍 |
| 第 10 章 二叉搜索树与平衡 | ✅ 完成（4/4 节） | 有序插入让 BST 退化，查找从 17.5 次涨到 **10,059.9 次（576 倍）**，比有序数组还慢 10 倍；AVL 把高度从 **99,999 压到 17** |
| 第 11 章 堆与优先队列 | ✅ 完成（4/4 节） | 堆不是有序数组（`2 5 3 8 7 6`）；Top-10 比完整排序快 **23.2 倍**；相同优先级出队顺序是 `A D C B` |
| 第 12 章 图与遍历 | ✅ 完成（4/4 节） | 邻接矩阵比邻接表多占 **52.6 倍**内存；BFS 最短路径经 240 组暴力验证；星形图上 BFS 队列 10 万 vs 递归 DFS 栈深 1 |
| 第 13 章 最短路径与拓扑排序 | ✅ 完成（4/4 节） | 同一张图四种做法给出**三种**合法顺序；**Kahn 剩下的 ≠ 环上的顶点**；递归 DFS 在 10 万顶点链上**崩在 9,628 层**；**双向 BFS 在网格上只快 2.14 倍、在树上快 695.6 倍**；**Dijkstra 的堆版/数组版分界点比理论早 4 倍多**；**Bellman-Ford 在随机图上反而比 Dijkstra 快 5.6 倍** |
| 第 14 章 贪心 | ✅ 完成（3/3 节） | **随机 1000 种币制里"找零用贪心"只有 2.5% 全对**；贪心 vs 动态规划差 **403 倍**；区间调度三种策略正确率 **62.5% / 21.3% / 100%**；Huffman 在真实英文上只省 **16.7%**，但离一元熵只差 0.048 位；**0/1 背包贪心的近似比无界**（差 5 万倍），只改成"允许切分"就精确了 |
| 第 15 章 动态规划 | ✅ 完成（4/4 节） | `Fib(33)` 朴素递归**调用 1140 万次、只有 34 个不同子问题**，记忆化降到 **65 次**；**LIS 状态差一个词，正确率 37.4% vs 100%**；**0/1 背包逆序 vs 正序：100% vs 3.4%**（正序其实解的是完全背包）；LIS 的 $O(n\log n)$ 快 **75 倍**；**依赖宽度**决定空间压缩，背包 **200.9 倍**、编辑距离 **1994 倍** |
| 第 16 章 工程实践与综合 | ✅ 完成（3/3 节） | 全书对比表汇总成两张速查表；同一算法只改遍历顺序差 **2.15 倍**；**五个基准测试陷阱**实测；**综合项目：Kahn 换个容器 = 优先调度，堆版比扫描版快 108 倍**；贪心调度 vs 最优（**NP 难**） |

当前进度：**63 / 63 节 —— 全书完成**（63 个示例项目全部编译运行通过）
