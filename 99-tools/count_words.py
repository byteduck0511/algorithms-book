"""精确统计正文字数：排除代码块、表格、状态卡 JSON，只数中文正文。

用法：python 99-tools/count_words.py
"""
import os
import re
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CHAPTERS = os.path.join(ROOT, "chapters")


def strip_non_prose(text):
    """去掉代码块、状态卡 JSON、表格、公式等非正文内容。"""
    # 1. 去掉围栏代码块
    text = re.sub(r"```.*?```", "", text, flags=re.DOTALL)
    # 2. 去掉状态外显 JSON（在代码块里，已被上一步去掉，这里兜底）
    text = re.sub(r"\*\*状态外显\*\*.*", "", text, flags=re.DOTALL)
    # 3. 去掉表格行（以 | 开头）
    text = re.sub(r"^\s*\|.*$", "", text, flags=re.MULTILINE)
    # 4. 去掉 LaTeX 公式
    text = re.sub(r"\$\$.*?\$\$", "", text, flags=re.DOTALL)
    text = re.sub(r"\$[^$\n]*\$", "", text)
    # 5. 去掉 markdown 链接和图片
    text = re.sub(r"!?\[[^\]]*\]\([^)]*\)", "", text)
    # 6. 去掉标题符号、列表符号、加粗标记
    text = re.sub(r"^#{1,6}\s*", "", text, flags=re.MULTILINE)
    text = re.sub(r"^\s*[-*+]\s+", "", text, flags=re.MULTILINE)
    text = re.sub(r"^\s*\d+\.\s+", "", text, flags=re.MULTILINE)
    text = text.replace("**", "").replace("`", "")
    return text


def count_chinese(text):
    """只统计中文字符 + 英文单词（粗略口径：中文字符数）。"""
    return len(re.findall(r"[一-鿿]", text))


def main():
    per_section = []
    per_chapter = defaultdict(int)

    for ch in sorted(os.listdir(CHAPTERS)):
        ch_path = os.path.join(CHAPTERS, ch)
        if not os.path.isdir(ch_path):
            continue
        for fn in sorted(os.listdir(ch_path)):
            if not fn.endswith(".md"):
                continue
            path = os.path.join(ch_path, fn)
            with open(path, encoding="utf-8") as f:
                raw = f.read()

            prose = strip_non_prose(raw)
            words = count_chinese(prose)
            m = re.match(r"(\d+\.\d+)-", fn)
            sec = m.group(1) if m else fn
            per_section.append((sec, words, len(raw)))
            per_chapter[ch] += words

    print(f"{'节号':<8} {'正文中文字数':>12} {'文件总字节':>12}  说明")
    print("-" * 62)
    in_range = 0
    for sec, words, total in sorted(per_section, key=lambda x: [int(p) for p in x[0].split(".")]):
        flag = ""
        if 600 <= words <= 1000:
            flag = "✓ 在 600-1000 区间"
            in_range += 1
        elif words < 600:
            flag = "偏短"
        else:
            flag = f"超出 {words / 1000:.1f} 倍上限"
        print(f"{sec:<8} {words:>12,} {total:>12,}  {flag}")

    print()
    print("=" * 62)
    total_prose = sum(w for _, w, _ in per_section)
    print(f"正文中文字数合计 : {total_prose:,}")
    print(f"平均每节         : {total_prose // len(per_section):,} 字")
    print(f"落在 600-1000 区间的节数 : {in_range} / {len(per_section)}")
    print()
    print("按章统计：")
    for ch in sorted(per_chapter):
        print(f"  {ch}: {per_chapter[ch]:>8,} 字")


if __name__ == "__main__":
    main()
