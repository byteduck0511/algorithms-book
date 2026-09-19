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
module.exports = {
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
