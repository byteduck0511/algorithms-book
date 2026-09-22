/**
 * gitbook-plugin-zh-katex —— 前端 KaTeX 数学渲染
 *
 * 为什么不直接用 honkit-plugin-katex（构建时渲染）：
 *
 *   它通过 GitBook 的 blocks 机制，在 **Markdown 解析之前**把 `$...$`
 *   短路成 `{% math_inline %}` 模板标签。这是全局配对，而本书有 **44 节**
 *   包含 C# 插值字符串（`$"{"n",8} | ..."`）——代码里那些孤立的 `$`
 *   会打乱全局配对，产生多余的 `{% endmath_inline %}`，构建直接报
 *   `unknown block tag: endmath_inline` 而失败。
 *
 *   改正文去转义 `$` 不可行：本书的约定是代码可直接复制运行。
 *
 * 本插件的做法：**构建时不碰公式**，改在前端渲染。
 * KaTeX 的 auto-render 遍历的是文本节点，并用 ignoredTags 跳过
 * pre/code —— 代码块里的 `$"` 天然不会被误当成数学起始符。
 *
 * 资源来自 katex@0.13.19 的 dist（katex.min.js / auto-render.min.js /
 * katex.min.css / fonts/*.woff2），随插件自带，离线可用。
 */
var protectMath = require('./lib/protect-math').protectMath;

module.exports = {
    hooks: {
        /**
         * 在 Markdown 解析之前，把公式里的 `_` `*` 等 Markdown 敏感字符转义掉。
         *
         * 前端渲染方案有个前提：`$$...$$` 必须原封不动地活到浏览器里。但
         * Markdown 引擎会先把公式里的成对 `_` 解析成 <em>，把定界符劈成两半，
         * auto-render 便再也匹配不上（详见 lib/protect-math.js 的说明）。
         * 放在这里统一兜住，作者写公式时就不必惦记 Markdown 的强调规则。
         */
        'page:before': function (page) {
            page.content = protectMath(page.content);
            return page;
        }
    },

    book: {
        assets: './assets',
        js: [
            'katex.min.js',
            'auto-render.min.js',
            'render-math.js'
        ],
        css: [
            'katex.min.css'
        ]
    },

    // 电子书输出（honkit 的 ebook 后端）同样带上样式；
    // 若走 pandoc 导出的路线，则由 pandoc 的 --katex 负责，与此无关。
    ebook: {
        assets: './assets',
        css: [
            'katex.min.css'
        ]
    }
};
