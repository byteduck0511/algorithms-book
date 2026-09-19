# Summary

* [前言](intro.md)

## 第 1 部分　把直觉变成工具

* 第 1 章　算法思维与复杂度分析
    * [1.1　从"能跑"到"能扛"：为什么需要算法分析](chapters/ch01/1.1-why-algorithm-analysis.md)
    * [1.2　大 O 记法：描述增长，而不是秒数](chapters/ch01/1.2-big-o-notation.md)
    * [1.3　最好、最坏与平均：三个不同的答案](chapters/ch01/1.3-best-worst-average.md)
    * [1.4　空间复杂度与时间-空间权衡](chapters/ch01/1.4-space-and-tradeoffs.md)

* 第 2 章　递归与分治
    * [2.1　递归的两个必要条件](chapters/ch02/2.1-two-conditions-of-recursion.md)
    * [2.2　调用栈：递归的代价与深度限制](chapters/ch02/2.2-call-stack.md)
    * [2.3　把递归改写成迭代](chapters/ch02/2.3-recursion-to-iteration.md)
    * [2.4　分治：切开、解决、合并](chapters/ch02/2.4-divide-and-conquer.md)

## 第 2 部分　线性结构

* 第 3 章　数组与字符串
    * [3.1　连续内存：数组的红利与代价](chapters/ch03/3.1-contiguous-memory.md)
    * [3.2　动态数组与扩容的摊还代价](chapters/ch03/3.2-dynamic-array.md)
    * [3.3　双指针技巧](chapters/ch03/3.3-two-pointers.md)
    * [3.4　滑动窗口](chapters/ch03/3.4-sliding-window.md)

* 第 4 章　链表
    * [4.1　节点与引用：链表的本质](chapters/ch04/4.1-nodes-and-references.md)
    * [4.2　单链表、双向链表与循环链表](chapters/ch04/4.2-linked-list-variants.md)
    * [4.3　插入与删除：边界处理是全部难点](chapters/ch04/4.3-insert-delete-boundaries.md)
    * [4.4　快慢指针与经典应用](chapters/ch04/4.4-fast-and-slow-pointers.md)

* 第 5 章　栈与队列
    * [5.1　栈：后进先出](chapters/ch05/5.1-stack.md)
    * [5.2　队列与双端队列：先进先出](chapters/ch05/5.2-queue-and-deque.md)
    * [5.3　应用：括号匹配与表达式求值](chapters/ch05/5.3-bracket-and-expression.md)
    * [5.4　单调栈与单调队列](chapters/ch05/5.4-monotonic-stack-and-queue.md)

* 第 6 章　哈希表
    * [6.1　从数组下标到哈希函数](chapters/ch06/6.1-from-index-to-hash.md)
    * [6.2　冲突解决：链地址法与开放寻址](chapters/ch06/6.2-collision-resolution.md)
    * [6.3　负载因子、扩容与再哈希](chapters/ch06/6.3-load-factor-and-resize.md)
    * [6.4　C# Dictionary 的内部与工程用法](chapters/ch06/6.4-dictionary-in-practice.md)

## 第 3 部分　排序与查找

* 第 7 章　基础排序与比较模型
    * [7.1　冒泡、选择、插入：三种 $O(n^2)$ 排序](chapters/ch07/7.1-quadratic-sorts.md)
    * [7.2　稳定性：同分时谁在前](chapters/ch07/7.2-stability.md)
    * [7.3　比较模型与交换次数](chapters/ch07/7.3-inversions-and-comparison-model.md)
    * [7.4　下界：为什么比较排序绕不开 $n \log n$](chapters/ch07/7.4-lower-bound.md)

* 第 8 章　高效排序
    * [8.1　归并排序：稳定且可预测](chapters/ch08/8.1-merge-sort.md)
    * [8.2　快速排序：平均最快，最坏要防](chapters/ch08/8.2-quick-sort.md)
    * [8.3　堆与堆排序](chapters/ch08/8.3-heap-sort.md)
    * [8.4　非比较排序：计数排序与基数排序](chapters/ch08/8.4-non-comparison-sorts.md)
    * [8.5　工程中如何选排序](chapters/ch08/8.5-choosing-a-sort.md)

## 第 4 部分　树与堆

* 第 9 章　二叉树与遍历
    * [9.1　树的术语与二叉树的形态](chapters/ch09/9.1-tree-basics.md)
    * [9.2　深度优先遍历：前序、中序、后序](chapters/ch09/9.2-dfs-traversals.md)
    * [9.3　广度优先遍历与层序](chapters/ch09/9.3-bfs-level-order.md)
    * [9.4　树题递归套路：返回值该是什么](chapters/ch09/9.4-recursion-patterns.md)

* 第 10 章　二叉搜索树与平衡思想
    * [10.1　二叉搜索树的有序性](chapters/ch10/10.1-bst-ordering.md)
    * [10.2　查找、插入、删除](chapters/ch10/10.2-search-insert-delete.md)
    * [10.3　退化问题：为什么需要平衡](chapters/ch10/10.3-degradation.md)
    * [10.4　平衡树思想：AVL 与红黑树概览](chapters/ch10/10.4-balancing-trees.md)

* 第 11 章　堆与优先队列
    * [11.1　堆的定义与数组表示](chapters/ch11/11.1-heap-basics.md)
    * [11.2　上浮与下沉](chapters/ch11/11.2-sift-up-and-down.md)
    * [11.3　完整实现一个优先队列](chapters/ch11/11.3-priority-queue.md)
    * [11.4　Top-K 与优先队列的工程用法](chapters/ch11/11.4-top-k-and-practice.md)

## 第 5 部分　图

* 第 12 章　图与遍历
    * [12.1　图的基本概念与两种存储方式](chapters/ch12/12.1-graph-basics.md)
    * [12.2　深度优先搜索](chapters/ch12/12.2-dfs.md)
    * [12.3　广度优先搜索](chapters/ch12/12.3-bfs.md)
    * [12.4　连通分量、环检测与二分图](chapters/ch12/12.4-components-cycles-bipartite.md)

* 第 13 章　最短路径与拓扑排序
    * [13.1　拓扑排序与依赖解析](chapters/ch13/13.1-topological-sort.md)
    * [13.2　无权最短路：BFS 的正确用法](chapters/ch13/13.2-unweighted-shortest-path.md)
    * [13.3　Dijkstra 算法](chapters/ch13/13.3-dijkstra.md)
    * [13.4　负权边与 Bellman-Ford 简介](chapters/ch13/13.4-bellman-ford.md)

## 第 6 部分　算法设计范式

* 第 14 章　贪心
    * [14.1　贪心的适用条件](chapters/ch14/14.1-when-greedy-works.md)
    * [14.2　区间调度与 Huffman 编码](chapters/ch14/14.2-interval-scheduling-and-huffman.md)
    * [14.3　贪心失效的反例](chapters/ch14/14.3-when-greedy-fails.md)

* 第 15 章　动态规划
    * [15.1　从暴力递归到记忆化](chapters/ch15/15.1-from-recursion-to-memoization.md)
    * [15.2　状态定义与转移方程](chapters/ch15/15.2-state-and-transition.md)
    * [15.3　经典题型：背包与最长递增子序列](chapters/ch15/15.3-knapsack-and-lis.md)
    * [15.4　空间优化与滚动数组](chapters/ch15/15.4-space-optimization.md)

## 第 7 部分　收束

* 第 16 章　工程实践与综合
    * [16.1　选型手册：按场景选数据结构](chapters/ch16/16.1-choosing-data-structures.md)
    * [16.2　从复杂度到实测：性能剖析](chapters/ch16/16.2-from-complexity-to-measurement.md)
    * [16.3　综合项目：一个任务调度器](chapters/ch16/16.3-task-scheduler.md)
