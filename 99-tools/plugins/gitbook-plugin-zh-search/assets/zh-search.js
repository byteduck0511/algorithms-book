/**
 * gitbook-plugin-zh-search —— 前端搜索引擎
 *
 * 与 gitbook-plugin-search 的 ui 完全兼容：只注册一个 engine，
 * 实现 init / search 两个方法，搜索框与结果面板沿用 GitBook 原有 UI。
 *
 * ⚠️ 下面 tokenize 的实现**必须与 ../index.js 中的保持一致**。
 *    索引侧和查询侧分词规则不同的话，检索结果会完全错位。
 *    改动任一侧时，另一侧要同步改。
 */
require(['gitbook', 'jquery'], function (gitbook, $) {
    'use strict';

    // ===== 分词（与 ../index.js 保持一致）=====
    var CJK = '㐀-䶿一-鿿豈-﫿';
    var CJK_RE = new RegExp('[' + CJK + ']');
    var TOKEN_RE = new RegExp('[' + CJK + ']+|[A-Za-z0-9_+#]+', 'g');

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

    // ===== BM25 参数 =====
    var K1 = 1.2;
    var B = 0.75;
    var MAX_DESCRIPTION_SIZE = 200;

    function ZhSearchEngine() {
        this.name = 'ZhSearchEngine';
        this.docs = [];
        this.store = {};
        this.index = {};
        this.docLen = [];
        this.avgLen = 1;
        this.df = {};   // 词条 -> 文档频率，init 时算一次
    }

    ZhSearchEngine.prototype.init = function () {
        var that = this;
        var d = $.Deferred();

        $.getJSON(gitbook.state.basePath + '/search_index.json')
            .then(function (data) {
                that.docs = data.docs || [];
                that.store = data.store || {};
                that.index = data.index || {};
                that.docLen = data.docLen || [];
                that.avgLen = data.avgLen || 1;

                for (var token in that.index) {
                    that.df[token] = that.index[token].length / 2;
                }
                d.resolve();
            })
            .fail(function (err) {
                d.reject(err);
            });

        return d.promise();
    };

    /**
     * 取一片摘要：优先以正文中第一次出现查询词的位置为中心。
     * ui 会对 body 调 .trim()，所以必须返回非空字符串。
     */
    ZhSearchEngine.prototype.excerpt = function (url, terms) {
        var body = this.store[url] || '';
        if (!body) return '';

        var at = -1;
        for (var i = 0; i < terms.length; i++) {
            var p = body.indexOf(terms[i]);
            if (p !== -1 && (at === -1 || p < at)) at = p;
        }
        if (at === -1) {
            return body.slice(0, MAX_DESCRIPTION_SIZE);
        }

        var start = Math.max(0, at - 60);
        var end = Math.min(body.length, start + MAX_DESCRIPTION_SIZE);
        return (start > 0 ? '…' : '') + body.slice(start, end) + (end < body.length ? '…' : '');
    };

    ZhSearchEngine.prototype.search = function (q, offset, length) {
        var that = this;
        var results = [];
        var terms = tokenize(q || '');

        if (terms.length && this.docs.length) {
            // 每个查询词 -> {docIdx: 该词在文档中的 tf}
            var perTerm = [];
            var rawTerms = [];
            for (var i = 0; i < terms.length; i++) {
                var postings = this.index[terms[i]];
                if (!postings) continue;
                var hits = {};
                for (var j = 0; j < postings.length; j += 2) {
                    hits[postings[j]] = postings[j + 1];
                }
                perTerm.push(hits);
                rawTerms.push(terms[i]);
            }

            if (perTerm.length) {
                var N = this.docs.length;

                // 先做 AND（所有查询词都命中），结果太少再降级为 OR。
                // 中文 2-gram 查询词多（「动态规划」-> 动态/态规/规划），
                // 一律 AND 会漏掉只命中部分 gram 的页面。
                var candidates = [];
                var docIdx;
                for (docIdx in perTerm[0]) {
                    var all = true;
                    for (var k = 1; k < perTerm.length; k++) {
                        if (perTerm[k][docIdx] === undefined) { all = false; break; }
                    }
                    if (all) candidates.push(Number(docIdx));
                }
                var mode = 'and';
                if (!candidates.length) {
                    mode = 'or';
                    // 降级到 OR 时必须要求命中足够多的查询词，否则会出假阳性：
                    // 「不存在的词xyzzy」会被拆成 不存/存在/在的/的词，
                    // 这些都是常见组合，不加限制能捞回 42 个不相干的页面。
                    var minHit = Math.max(2, Math.ceil(perTerm.length * 0.6));
                    var hitCount = {};
                    for (var a = 0; a < perTerm.length; a++) {
                        for (docIdx in perTerm[a]) {
                            hitCount[docIdx] = (hitCount[docIdx] || 0) + 1;
                        }
                    }
                    for (docIdx in hitCount) {
                        if (hitCount[docIdx] >= minHit) candidates.push(Number(docIdx));
                    }
                }

                for (var c = 0; c < candidates.length; c++) {
                    var di = candidates[c];
                    var score = 0;
                    for (var t = 0; t < perTerm.length; t++) {
                        var tf = perTerm[t][di];
                        if (tf === undefined) continue;
                        var df = this.df[rawTerms[t]] || 1;
                        var idf = Math.log(1 + (N - df + 0.5) / (df + 0.5));
                        var len = this.docLen[di] || this.avgLen;
                        score += idf * (tf * (K1 + 1)) /
                                 (tf + K1 * (1 - B + B * len / this.avgLen));
                    }
                    // AND 命中的排在 OR 命中之前
                    if (mode === 'or') score *= 0.5;

                    var doc = this.docs[di];
                    results.push({
                        score: score,
                        title: doc.title,
                        url: doc.url,
                        body: this.excerpt(doc.url, rawTerms)
                    });
                }

                results.sort(function (x, y) { return y.score - x.score; });
            }
        }

        var count = results.length;
        return $.Deferred().resolve({
            query: q,
            results: results.slice(offset || 0, (offset || 0) + (length || 10)),
            count: count
        }).promise();
    };

    gitbook.events.bind('start', function (e, config) {
        var engine = gitbook.search.getEngine();
        if (!engine) {
            gitbook.search.setEngine(ZhSearchEngine, config);
        }
    });
});
