"""验证 1.1 节的复杂度结论与两种实现的等价性。

语义（两种实现必须一致）：统计有多少个元素是"重复出现"的
    = Σ(每个值的出现次数 - 1)
    = n - 不同值的个数

注意：本脚本验证的是【算法逻辑与增长倍率】，不是 C# 代码的编译正确性。
"""
import random
import time


def make_data(n, seed=42):
    rng = random.Random(seed)
    return [rng.randrange(0, n // 10) for _ in range(n)]


def count_duplicates_slow(data):
    """做法 A：两层循环。对每个位置 i，若其值在 i 之后再次出现，计入一次。"""
    count = 0
    n = len(data)
    for i in range(n):
        for j in range(i + 1, n):
            if data[i] == data[j]:
                count += 1
                break
    return count


def count_duplicates_fast(data):
    """做法 B：一遍扫描 + 哈希集合。遇到已见过的值就计入一次。"""
    seen = set()
    count = 0
    for x in data:
        if x in seen:
            count += 1
        else:
            seen.add(x)
    return count


def brute_reference(data):
    """独立参考实现：n - 不同值个数。用于交叉校验两种实现的语义。"""
    return len(data) - len(set(data))


def main():
    print(f"{'n':>8} {'slow(ms)':>10} {'fast(ms)':>10} {'equal':>6}")
    results = {}
    for n in (2000, 20000):
        d = make_data(n)
        t = time.perf_counter()
        s = count_duplicates_slow(d)
        ts = (time.perf_counter() - t) * 1000
        t = time.perf_counter()
        f = count_duplicates_fast(d)
        tf = (time.perf_counter() - t) * 1000
        results[n] = (ts, tf, s, f)
        print(f"{n:>8} {ts:>10.1f} {tf:>10.1f} {str(s == f):>6}")

    ok = results[2000][2] == results[2000][3] and results[20000][2] == results[20000][3]
    ratio = results[20000][0] / results[2000][0]
    print(f"\n两种实现结果一致: {ok}")
    print(f"n 涨 10 倍，慢速耗时涨 {ratio:.1f} 倍 (理论 ~100 倍，即 n^2)")

    # 用独立参考实现交叉校验（小规模暴力枚举）
    print("\n交叉校验（n=2000 数据，对照 n - 不同值个数）:")
    d = make_data(2000)
    ref = brute_reference(d)
    print(f"  slow={count_duplicates_slow(d)} fast={count_duplicates_fast(d)} ref={ref}"
          f" -> {'全部一致' if count_duplicates_slow(d) == count_duplicates_fast(d) == ref else '不一致!'}")

    # 边界用例
    print("\n边界用例:")
    for case in ([], [1], [1, 1], [1, 1, 1], [1, 2, 1, 2], [5, 5, 5, 5, 5]):
        a, b, c = count_duplicates_slow(case), count_duplicates_fast(case), brute_reference(case)
        flag = "OK" if a == b == c else "FAIL"
        print(f"  {str(case):<20} slow={a} fast={b} ref={c}  {flag}")


if __name__ == "__main__":
    main()
