/**
 * Markdown 层的公式保护。
 *
 * 为什么需要它
 * ------------
 * GitBook/HonKit 是先用 Markdown 引擎解析正文、之后才在前端用 KaTeX 渲染
 * `$...$` 的。Markdown 并不认识 LaTeX 的语法，于是会从两个地方把公式改坏：
 *
 * 1) 强调配对。Markdown 看到公式里有成对的 `_`（哪怕相隔几十个字符、
 *    `_` 前面还是字母），就把它们配成 <em> 标签：
 *
 *        $$T(n) = \underbrace{2T(n/2)}_{\text{解决左右两半}} + \underbrace{O(1)}_{\text{合并}}$$
 *
 *    变成
 *
 *        <p>$$T(n) = \underbrace{2T(n/2)}<em>{\text{解决左右两半}} + ...</em>$$</p>
 *
 *    定界符被 HTML 标签从中间劈开，KaTeX 的 auto-render 无法跨文本节点
 *    匹配，整个公式就原样裸露在页面上。实测这个引擎的配对规则相当宽松，
 *    公式里只要出现两个以上 `_` 就有风险。
 *
 * 2) 反斜杠转义。Markdown 会把 `\` + ASCII 标点 解析成标点本身，于是
 *    LaTeX 的 `\\`（换行）、`\{`、`\}`、`\,`、`\&` 都会丢掉反斜杠：
 *
 *        $$b^e = \begin{cases} ... \\[4pt] ... \end{cases}$$
 *
 *    到了浏览器里变成了 `\[4pt]`，`\[` 被 KaTeX 当成显示公式定界符，
 *    整行报错标红。注意 `\text`、`\underbrace` 这类 `\` 后跟字母的写法
 *    不受影响（字母不在 Markdown 的转义表里），所以出问题的只是标点。
 *
 * 做法
 * ----
 * 在 Markdown 解析之前（page:before hook）把公式区间内的内容做一次
 * "逆 Markdown 转义"，让解析结果正好等于作者写的 LaTeX 源码：
 *
 *   - 敏感字符 `_` `*` `` ` `` `~` 前面补一个 `\`，Markdown 解析后又变回原字符；
 *   - `\` 后面紧跟 ASCII 标点时把它写两遍（`\\` → `\\\\`），
 *     Markdown 解析后正好还原成一个 `\`。
 *
 * 转义必须跳过代码区域（围栏代码块、行内代码）：本书代码里有大量 C# 插值
 * 字符串（$"{"n",8}"），其中的 `$` 不是数学定界符，动它们会把代码改坏。
 */

// 会触发 Markdown 解析、进而劈开公式定界符的字符。
// 不包含 `[` `]` `(` `)`：它们要构成 `[x](y)` 才算链接，公式里极少出现这种形状，
// 误伤的代价（把 \left[ 之类改坏）反而更高。
var SENSITIVE = { '_': true, '*': true, '`': true, '~': true };

// CommonMark 的反斜杠转义表：`\` 后面是这些字符时，反斜杠会被吃掉。
var ESCAPABLE = {};
'!"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~'.split('').forEach(function (c) {
    ESCAPABLE[c] = true;
});

var FENCE_RE = /^( {0,3})(`{3,}|~{3,})/;

/**
 * 把一段公式内容编码成"经 Markdown 解析后恰好还原"的写法。
 */
function encodeSegment(seg) {
    var out = '';
    for (var i = 0; i < seg.length; i++) {
        var c = seg.charAt(i);
        if (c === '\\') {
            var next = seg.charAt(i + 1);
            // 后跟 ASCII 标点 → 写两遍，抵消 Markdown 的转义；
            // 后跟字母（\text、\underbrace…）→ 原样保留。
            out += (next !== '' && ESCAPABLE[next]) ? '\\\\' : '\\';
            continue;
        }
        out += SENSITIVE[c] ? '\\' + c : c;
    }
    return out;
}

/** 返回围栏代码块的结束位置（结束行之后的偏移）；没有结束则返回文本末尾。 */
function findFenceEnd(content, start, fence) {
    var marker = fence.charAt(0);
    var minLen = fence.length;
    var closer = new RegExp('^ {0,3}\\' + marker + '{' + minLen + ',}\\s*$');

    var lineEnd = content.indexOf('\n', start);
    while (lineEnd !== -1) {
        var next = lineEnd + 1;
        var end = content.indexOf('\n', next);
        var line = content.slice(next, end === -1 ? content.length : end);
        if (closer.test(line)) {
            return end === -1 ? content.length : end + 1;
        }
        if (end === -1) break;
        lineEnd = end;
    }
    return content.length;
}

/** 跳过 `...` 或 ``...`` 行内代码，返回结束位置。 */
function skipCodeSpan(content, start) {
    var ticks = 0;
    while (content.charAt(start + ticks) === '`') ticks++;
    var closer = new Array(ticks + 1).join('`');
    var idx = content.indexOf(closer, start + ticks);
    return idx === -1 ? content.length : idx + ticks;
}

/**
 * 找行内公式的结束 `$`，找不到返回 -1。
 *
 * 误判的代价不对称：漏保护只是公式渲染失败，误判却会把正文圈进"公式"里
 * 转义掉，反而破坏原本正常的排版。所以判定规则都取保守的一侧。
 */
function findInlineEnd(content, start) {
    // 前面是 ASCII 字母或数字，说明这个 `$` 是某个公式的收尾而不是开始
    // （"…= 200$** ✓" 里的 `$` 就属于这种）。
    // 只看 ASCII：中文行文里 "复杂度 $O(n)$" 这种紧贴写法很常见，
    // 把汉字也算进来会漏掉大量真实公式。
    if (/[A-Za-z0-9]/.test(content.charAt(start - 1))) return -1;

    // 开定界符后面不能是空白："$ 100" 不是公式。
    // 这里刻意不排除数字开头的 "$2 \times 100$" —— 那种写法很常见，
    // 而货币 "$5 到 $10" 靠上面和下面前后两条规则已经挡得住。
    var first = content.charAt(start + 1);
    if (first === '' || first === ' ' || first === '\t') return -1;

    for (var i = start + 1; i < content.length; i++) {
        var c = content.charAt(i);
        if (c === '\n') return -1;
        // 闭定界符前面不能是空白，挡掉 "…小于 $ 的写法" 这类散落的美元号
        if (c === '$') return content.charAt(i - 1) === ' ' ? -1 : i;
    }
    return -1;
}

/**
 * 保护正文中的公式，返回处理后的 Markdown。
 * @param {String} content Markdown 原文
 * @return {String}
 */
function protectMath(content) {
    var out = [];
    var i = 0;
    var n = content.length;

    while (i < n) {
        // 行首：围栏代码块整体放行
        if (i === 0 || content.charAt(i - 1) === '\n') {
            var fence = FENCE_RE.exec(content.slice(i, i + 8));
            if (fence) {
                var fenceEnd = findFenceEnd(content, i, fence[2]);
                out.push(content.slice(i, fenceEnd));
                i = fenceEnd;
                continue;
            }
        }

        var ch = content.charAt(i);

        // 行内代码整体放行
        if (ch === '`') {
            var codeEnd = skipCodeSpan(content, i);
            out.push(content.slice(i, codeEnd));
            i = codeEnd;
            continue;
        }

        // 显示公式 $$...$$
        if (ch === '$' && content.charAt(i + 1) === '$') {
            var dblClose = content.indexOf('$$', i + 2);
            if (dblClose !== -1) {
                out.push('$$', encodeSegment(content.slice(i + 2, dblClose)), '$$');
                i = dblClose + 2;
                continue;
            }
            out.push(ch);
            i++;
            continue;
        }

        // 行内公式 $...$
        if (ch === '$') {
            var close = findInlineEnd(content, i);
            if (close !== -1) {
                out.push('$', encodeSegment(content.slice(i + 1, close)), '$');
                i = close + 1;
                continue;
            }
            out.push(ch);
            i++;
            continue;
        }

        out.push(ch);
        i++;
    }

    return out.join('');
}

module.exports = {
    protectMath: protectMath,
    encodeSegment: encodeSegment
};
