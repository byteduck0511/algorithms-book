/**
 * gitbook-plugin-zh-search —— 中文友好的全文搜索
 *
 * 为什么不用 gitbook-plugin-lunr（默认方案）：
 *
 *   1. **中文分不了词**。lunr 0.5.12 按 `/[\s\-]+/` 切分，中文没有空格，
 *      一整段变成一个 token；最终索引里只剩 910 个**单字**词条，
 *      搜「哈希表」实际靠单字碰运气，不能按词检索。
 *
 *   2. **索引是坏的，会崩搜索框**。lunr 的 Index 被 `JSON.stringify` 后
 *      `Index.load` 回来，內部结构不一致：documentStore 说某文档含 token X，
 *      tokenStore[X] 里却没有该文档。于是 `lunr.js:1150` 的
 *      `this.tokenStore.get(token)[documentRef].tf` 读到 undefined，
 *      抛 TypeError。实测「递归」「big」「stack」等查询直接崩溃。
 *
 *   3. **代码块全文进了索引**。插件只剥 HTML 标签不剔代码，
 *      全书 16 万个代码围栏的正文全部入索引，search_index.json 膨胀到 14.32 MB。
 *
 * 本插件的做法：
 *   - 剔除 <pre>（代码块，状态外显 JSON 也在其中），保留行内 <code>（多为术语）
 *   - 中文切 2-gram，英文/数字按词切，纯数字短 token 丢弃
 *   - 自建倒排索引 + BM25 打分，只存 {token: [[docIdx, tf], ...]}
 *   - 前端 engine 接口与 gitbook-plugin-search 完全兼容，搜索 UI 无需改动
 *
 * 注意：前端 assets/zh-search.js 里有一份**必须保持一致**的分词实现。
 *       改动分词规则时两边都要改。
 */
'use strict';

/**
 * 解码常见 HTML 实体。
 * 不引第三方包，避免插件在不同 node_modules 布局下解析失败。
 */
function decodeEntities(s) {
    return s
        .replace(/&#(\d+);/g, (_, d) => String.fromCharCode(parseInt(d, 10)))
        .replace(/&#[xX]([0-9a-fA-F]+);/g, (_, h) => String.fromCharCode(parseInt(h, 16)))
        .replace(/&nbsp;/g, ' ')
        .replace(/&lt;/g, '<')
        .replace(/&gt;/g, '>')
        .replace(/&quot;/g, '"')
        .replace(/&#39;/g, "'")
        .replace(/&amp;/g, '&');
}

/**
 * 从渲染后的 HTML 抽取用于建索引的纯文本。
 *
 * 关键取舍：剔除 <pre> 但保留 <code>。
 *   - <pre> 是代码块与实测输出，占全书体量近半，检索价值低 —— 剔除
 *   - <code> 是行内术语（Dictionary<TKey,TValue>、List.RemoveAt(0)）—— 保留
 *   - 每节末尾的「状态外显」JSON 位于 <pre><code class="lang-json"> 中，随 <pre> 一并剔除
 */
function extractText(html) {
    return decodeEntities(
        html
            .replace(/<script[\s\S]*?<\/script>/gi, ' ')
            .replace(/<style[\s\S]*?<\/style>/gi, ' ')
            .replace(/<pre[\s\S]*?<\/pre>/gi, ' ')
            .replace(/<[^>]+>/g, ' ')
    ).replace(/\s+/g, ' ').trim();
}

var CJK = '\\u3400-\\u4dbf\\u4e00-\\u9fff\\uf900-\\ufaff';
var CJK_RE = new RegExp('[' + CJK + ']');
// 中文串 | 英文/数字串（含 + # 以支持 C++ / C#）
var TOKEN_RE = new RegExp('[' + CJK + ']+|[A-Za-z0-9_+#]+', 'g');

/**
 * 分词。规则必须与 assets/zh-search.js 中的实现一致。
 *
 * - 中文：输出全部 2-gram；串长为 1 时输出该单字
 * - 英文/数字：转小写后原样输出；长度 < 2 的纯数字丢弃
 */
function tokenize(text) {
    var out = [];
    var m;
    TOKEN_RE.lastIndex = 0;
    while ((m = TOKEN_RE.exec(text)) !== null) {
        var s = m[0];
        if (CJK_RE.test(s)) {
            if (s.length === 1) {
                out.push(s);
            } else {
                for (var i = 0; i + 2 <= s.length; i++) {
                    out.push(s.substr(i, 2));
                }
            }
        } else {
            if (/^\d+$/.test(s) && s.length < 2) continue;
            out.push(s.toLowerCase());
        }
    }
    return out;
}

// 每个页面调一次 page hook，此处累积
var docs = [];        // [{url, title}]
var store = {};       // url -> 正文纯文本（前端做摘要与高亮）
var inverted = {};    // token -> { docIdx: tf }
var docLen = [];      // 每篇的 token 总数
var enabled = true;

module.exports = {
    book: {
        assets: './assets',
        js: ['zh-search.js']
    },

    hooks: {
        'page': function (page) {
            if (this.output.name !== 'website' || !enabled) return page;
            if (page.search === false) return page;

            var text = extractText(page.content || '');
            if (!text) return page;

            var docIdx = docs.length;
            var url = this.output.toURL(page.path);
            docs.push({ url: url, title: page.title || '' });
            store[url] = text;

            var tokens = tokenize((page.title || '') + ' ' + text);
            docLen.push(tokens.length);

            var seen = Object.create(null);
            for (var i = 0; i < tokens.length; i++) {
                seen[tokens[i]] = (seen[tokens[i]] || 0) + 1;
            }
            for (var t in seen) {
                (inverted[t] || (inverted[t] = {}))[docIdx] = seen[t];
            }

            this.log.debug.ln('zh-search indexed', page.path, tokens.length + ' tokens');
            return page;
        },

        'finish': function () {
            if (this.output.name !== 'website') return;

            // 压成 [docIdx, tf, docIdx, tf, ...] 的扁平数组，比对象省不少体积
            var index = {};
            var totalLen = 0;
            for (var i = 0; i < docLen.length; i++) totalLen += docLen[i];

            for (var token in inverted) {
                var postings = inverted[token];
                var flat = [];
                for (var d in postings) {
                    flat.push(Number(d), postings[d]);
                }
                index[token] = flat;
            }

            var payload = {
                version: 1,
                docs: docs,
                store: store,
                index: index,
                docLen: docLen,
                avgLen: docLen.length ? totalLen / docLen.length : 1
            };

            var json = JSON.stringify(payload);
            // 用字节数而不是 json.length：中文在 UTF-8 下每字 3 字节，
            // 按字符数报会低报约 1/3（曾把 1.83 MB 报成 1.17 MB）
            var bytes = Buffer.byteLength(json, 'utf8');
            this.log.info.ln(
                'zh-search: ' + docs.length + ' 篇, ' +
                Object.keys(index).length.toLocaleString() + ' 个词条, ' +
                (bytes / 1048576).toFixed(2) + ' MB'
            );
            return this.output.writeFile('search_index.json', json);
        }
    }
};

// 供测试脚本复用
module.exports._internals = { tokenize: tokenize, extractText: extractText };
