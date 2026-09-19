#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
构建电子书源 —— 把 63 节按 SUMMARY.md 的顺序拼成一份适合 pandoc 消费的 Markdown。

做两件事：
  1. 按 SUMMARY.md 里链接出现的顺序取章节（这就是书的阅读顺序）
  2. 剥离每节末尾的「状态外显」JSON 块 —— 那是给写作/审校流程用的元数据
     （记录本节覆盖了什么、留了哪些坑、实测数据），不是给读者看的内容

产出 _ebook/book.md，后续由 pandoc 转 EPUB / PDF。

用法：
    PYTHONIOENCODING=utf-8 python 99-tools/build_ebook.py
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SUMMARY = os.path.join(ROOT, 'SUMMARY.md')
OUT_DIR = os.path.join(ROOT, '_ebook')
OUT_MD = os.path.join(OUT_DIR, 'book.md')

# SUMMARY.md 里形如 `    * [1.1　标题](chapters/ch01/1.1-xxx.md)` 的条目
LINK_RE = re.compile(r'\[[^\]]*\]\(([^)]+\.md)\)')

# 从 `**状态外显**` 一直到文件末尾整块删掉
STATUS_RE = re.compile(r'\n\*\*状态外显\*\*[\s\S]*\Z')

FENCE_RE = re.compile(r'^\s*(```|~~~)')
INLINE_CODE_RE = re.compile(r'`[^`\n]*`')
QUOTE_PAIR_RE = re.compile(r'"([^"\n]*)"')


def smarten_quotes(text):
    """
    把正文里的英文直引号 "..." 换成中文引号 “...”。

    源文件通篇用的是 "（U+0022），而 pandoc 的 smart 扩展按**英文**规则
    判开合：引号前是空白/开括号才算开引号。中文没有空格，`个"重复订单"`
    这种引号前面是汉字，会被判成闭引号，输出成 `个”重复订单”` —— 方向反了。
    所以这里自己转，并让 pandoc 关掉 smart（-f markdown-smart）。

    跳过代码围栏与行内代码：那里的 " 是 C# 字符串，不能动。
    """
    out = []
    in_fence = False
    for line in text.split('\n'):
        if FENCE_RE.match(line):
            in_fence = not in_fence
            out.append(line)
            continue
        if in_fence:
            out.append(line)
            continue

        # 先把行内代码摘成占位符，免得动到里面的引号
        stash = []

        def _stash(m):
            stash.append(m.group(0))
            return '\x00%d\x00' % (len(stash) - 1)

        line = INLINE_CODE_RE.sub(_stash, line)
        line = QUOTE_PAIR_RE.sub('“\\1”', line)
        for i, s in enumerate(stash):
            line = line.replace('\x00%d\x00' % i, s)
        out.append(line)
    return '\n'.join(out)


# 合并后的书在最前面加的元数据块
FRONT_MATTER = """---
title: "数据结构与算法自学：从工程直觉到复杂度思维"
author: "小陈与老周"
lang: zh-CN
...

"""


def collect_sources():
    """读 SUMMARY.md，按顺序返回所有 .md 的相对路径。"""
    with open(SUMMARY, encoding='utf-8') as f:
        text = f.read()

    paths = []
    seen = set()
    for m in LINK_RE.finditer(text):
        rel = m.group(1).strip()
        # 跳过外链
        if rel.startswith(('http://', 'https://')):
            continue
        if rel in seen:
            print(f'  ! 重复条目，已跳过：{rel}', file=sys.stderr)
            continue
        seen.add(rel)
        paths.append(rel)
    return paths


def strip_status(text):
    """删掉本节末尾的「状态外显」元数据块。"""
    return STATUS_RE.sub('\n', text).rstrip() + '\n'


def main():
    paths = collect_sources()
    if not paths:
        print('SUMMARY.md 里没有解析到任何章节，中止', file=sys.stderr)
        return 1

    os.makedirs(OUT_DIR, exist_ok=True)

    parts = [FRONT_MATTER]
    total_chars = 0
    stripped = 0
    missing = []

    for rel in paths:
        full = os.path.join(ROOT, rel.replace('/', os.sep))
        if not os.path.exists(full):
            missing.append(rel)
            continue

        with open(full, encoding='utf-8') as f:
            raw = f.read()

        if '**状态外显**' in raw:
            stripped += 1
        body = smarten_quotes(strip_status(raw))

        # 先写一行来源注释便于排查，再是正文
        parts.append(f'<!-- source: {rel} -->\n\n{body}\n')
        total_chars += len(body)

    if missing:
        print(f'  ! {len(missing)} 个文件找不到：', file=sys.stderr)
        for m in missing:
            print(f'      {m}', file=sys.stderr)

    merged = '\n'.join(parts)
    with open(OUT_MD, 'w', encoding='utf-8') as f:
        f.write(merged)

    print(f'✓ 合并 {len(paths) - len(missing)} 个文件 -> {os.path.relpath(OUT_MD, ROOT)}')
    print(f'  剥离状态外显块：{stripped} 处')
    print(f'  正文合计：{total_chars:,} 字符')
    print(f'  输出大小：{len(merged.encode("utf-8")) / 1048576:.2f} MB')
    return 0


if __name__ == '__main__':
    sys.exit(main())
