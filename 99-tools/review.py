"""全书审校脚本：检查章节结构完整性、状态卡有效性、交叉引用、术语一致性。

用法：python 99-tools/review.py
"""
import json
import os
import re
import sys
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CHAPTERS = os.path.join(ROOT, "chapters")

# 每节应当包含的结构元素
REQUIRED = ["学习目标", "先修", "练习", "常见错误", "总结", "状态外显"]
# 部分小节用「本节总结」，有的用「本章小结」，做宽松匹配
REQUIRED_SOFT = {
    "总结": ["本节总结", "本章小结"],
}


def collect_files():
    """收集所有章节文件，按 章/节 排序。"""
    result = []
    for ch in sorted(os.listdir(CHAPTERS)):
        ch_path = os.path.join(CHAPTERS, ch)
        if not os.path.isdir(ch_path):
            continue
        for fn in sorted(os.listdir(ch_path)):
            if fn.endswith(".md"):
                result.append((ch, fn, os.path.join(ch_path, fn)))
    return result


def extract_section_number(filename):
    """从文件名提取节号，如 1.2-big-o-notation.md -> (1, 2)。"""
    m = re.match(r"(\d+)\.(\d+)-", filename)
    if not m:
        return None
    return (int(m.group(1)), int(m.group(2)))


def check_structure(files):
    """检查每节的结构完整性。"""
    issues = []
    for ch, fn, path in files:
        with open(path, encoding="utf-8") as f:
            content = f.read()

        missing = []
        for key in REQUIRED:
            if key in REQUIRED_SOFT:
                if not any(v in content for v in REQUIRED_SOFT[key]):
                    missing.append(key)
            elif key not in content:
                missing.append(key)

        if missing:
            issues.append((f"{ch}/{fn}", f"缺少结构元素: {', '.join(missing)}"))

        # 检查状态外显 JSON 是否可解析
        m = re.search(r"\*\*状态外显\*\*\s*```json\s*(.*?)```", content, re.DOTALL)
        if not m:
            issues.append((f"{ch}/{fn}", "状态外显 JSON 块缺失或格式不对"))
        else:
            try:
                status = json.loads(m.group(1))
                for field in ["book", "section", "covered", "next"]:
                    if field not in status:
                        issues.append((f"{ch}/{fn}", f"状态卡缺少字段: {field}"))
            except json.JSONDecodeError as e:
                issues.append((f"{ch}/{fn}", f"状态外显 JSON 解析失败: {e}"))

        # 检查字数统计是否声明
        if "word_count_actual" not in content:
            issues.append((f"{ch}/{fn}", "状态卡缺少 word_count_actual"))
    return issues


def check_cross_refs(files):
    """检查正文里引用的「X.X 节」是否存在。"""
    # 已完成的节号集合
    existing = set()
    for ch, fn, _ in files:
        num = extract_section_number(fn)
        if num:
            existing.add(f"{num[0]}.{num[1]}")

    # 规划中但尚未写作的节号（来自 section-cards）
    planned = set()
    cards_path = os.path.join(ROOT, "00-plan", "section-cards.md")
    if os.path.exists(cards_path):
        with open(cards_path, encoding="utf-8") as f:
            for m in re.finditer(r'"section":\s*"(\d+\.\d+)"', f.read()):
                planned.add(m.group(1))

    all_valid = existing | planned

    refs = defaultdict(set)   # 被引用的节号 -> 引用它的文件
    for ch, fn, path in files:
        with open(path, encoding="utf-8") as f:
            content = f.read()
        for m in re.finditer(r"(\d+)\.(\d+)\s*节", content):
            refs[f"{m.group(1)}.{m.group(2)}"].add(f"{ch}/{fn}")

    dangling = []      # 引用了既不存在也没规划的节
    pending = []       # 引用了「已规划但还没写」的节
    for ref, sources in sorted(refs.items()):
        if ref in existing:
            continue
        if ref in planned:
            pending.append((ref, sorted(sources)))
        else:
            dangling.append((ref, sorted(sources)))
    return dangling, pending, refs


def check_glossary(files):
    """检查 glossary 中的术语首次出现位置是否已经写作完成。"""
    glossary_path = os.path.join(ROOT, "00-plan", "glossary.md")
    if not os.path.exists(glossary_path):
        return [], []

    with open(glossary_path, encoding="utf-8") as f:
        content = f.read()

    written = set()
    for ch, fn, _ in files:
        num = extract_section_number(fn)
        if num:
            written.add(f"{num[0]}.{num[1]}")

    in_scope, out_of_scope = [], []
    for m in re.finditer(r"\|\s*([^|]+?)\s*\|\s*([^|]*?)\s*\|\s*(\d+\.\d+)\s*\|", content):
        term, _, first = m.group(1), m.group(2), m.group(3)
        if first in written:
            in_scope.append((term.strip(), first))
        else:
            out_of_scope.append((term.strip(), first))
    return in_scope, out_of_scope


def main():
    files = collect_files()
    print(f"扫描到 {len(files)} 个章节文件\n")

    print("=" * 70)
    print("一、结构完整性")
    print("=" * 70)
    issues = check_structure(files)
    if issues:
        for f, msg in issues:
            print(f"  [!] {f}: {msg}")
    else:
        print("  全部通过：每节都包含学习目标/先修/练习/常见错误/总结/状态外显")
    print()

    print("=" * 70)
    print("二、交叉引用")
    print("=" * 70)
    dangling, pending, refs = check_cross_refs(files)
    print(f"  共发现 {len(refs)} 个不同的节号引用")
    if pending:
        print(f"\n  指向【已规划但尚未写作】的引用（{len(pending)} 个）—— 属于预期，"
              f"这些是后续章节的内容：")
        for ref, sources in pending[:20]:
            print(f"    {ref}  <- {', '.join(sources[:3])}")
        if len(pending) > 20:
            print(f"    ...（还有 {len(pending) - 20} 个）")
    if dangling:
        print(f"\n  [!!] 悬空引用（{len(dangling)} 个）—— 这些节号既没写也没规划，需要修正：")
        for ref, sources in dangling:
            print(f"    {ref}  <- {', '.join(sources[:3])}")
    else:
        print("\n  没有悬空引用 ✓")
    print()

    print("=" * 70)
    print("三、术语表")
    print("=" * 70)
    in_scope, out_of_scope = check_glossary(files)
    print(f"  glossary 中共 {len(in_scope) + len(out_of_scope)} 条术语")
    print(f"    已完成章节覆盖 {len(in_scope)} 条")
    print(f"    属于后续章节 {len(out_of_scope)} 条")
    if out_of_scope:
        print(f"\n  后续章节的术语（前 15 条）：")
        for term, first in out_of_scope[:15]:
            print(f"    {term}  (计划首次出现于 {first})")
        if len(out_of_scope) > 15:
            print(f"    ...（还有 {len(out_of_scope) - 15} 条）")
    print()

    print("=" * 70)
    print("四、字数统计")
    print("=" * 70)
    total_words = 0
    per_chapter = defaultdict(int)
    for ch, fn, path in files:
        with open(path, encoding="utf-8") as f:
            content = f.read()
        m = re.search(r'"word_count_actual":\s*(\d+)', content)
        if m:
            w = int(m.group(1))
            total_words += w
            per_chapter[ch] += w
    for ch in sorted(per_chapter):
        print(f"    {ch}: {per_chapter[ch]:>7,} 字")
    print(f"    {'合计':<6}: {total_words:>7,} 字")
    print(f"    平均每节: {total_words // len(files):,} 字")
    print()

    print("=" * 70)
    print("五、结论")
    print("=" * 70)
    blocking = [i for i in issues] + [("", f"悬空引用 {r}") for r, _ in dangling]
    if not blocking:
        print(f"  {len(files)} 节全部通过审校：结构完整、状态卡有效、无悬空引用。")
        return 0
    else:
        print(f"  发现 {len(blocking)} 个需要修正的问题。")
        return 1


if __name__ == "__main__":
    sys.exit(main())
