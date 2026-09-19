/**
 * 前端数学渲染：用 KaTeX auto-render 处理页面正文。
 *
 * 关键点是 ignoredTags 里的 pre / code —— 本书代码块中大量出现
 * C# 插值字符串（$"..."），它们必须原样显示，不能被当成数学定界符。
 * auto-render 遍历的是文本节点，跳过这些标签后，代码块天然免疫。
 *
 * 关于加载方式：katex.min.js / auto-render.min.js 是 UMD 包，在 GitBook 的
 * requirejs 环境下会走 `define()` 注册成匿名模块而不挂到 window 上，
 * 因此这里不依赖 assets 的自动加载，改为按需插入 <script> 标签
 * （运行时插入的脚本不经过 requirejs），保证 window.katex /
 * window.renderMathInElement 一定可用。
 */
require(['gitbook'], function (gitbook) {
    'use strict';

    var BASE = (gitbook.state && gitbook.state.basePath) || '';
    var ASSET_DIR = BASE + '/gitbook/gitbook-plugin-zh-katex/';

    // 顺序要紧：$$ 必须排在 $ 前面，否则 $$x$$ 会被 $ 先匹配掉
    var DELIMITERS = [
        { left: '$$', right: '$$', display: true },
        { left: '$', right: '$', display: false }
    ];

    // pre / code 是必须的：代码里的 $ 不是数学
    var IGNORED_TAGS = ['script', 'noscript', 'style', 'textarea', 'pre', 'code', 'option'];

    function insertScript(src) {
        return new Promise(function (resolve) {
            var s = document.createElement('script');
            s.src = src;
            s.onload = resolve;
            s.onerror = resolve;   // 失败也要 resolve，否则后续永远卡住
            document.head.appendChild(s);
        });
    }

    var loading = null;
    function ensureKatex() {
        if (typeof window.renderMathInElement === 'function') return Promise.resolve();
        if (loading) return loading;
        loading = insertScript(ASSET_DIR + 'katex.min.js')
            .then(function () { return insertScript(ASSET_DIR + 'auto-render.min.js'); });
        return loading;
    }

    function renderMath() {
        // 渲染整个 body 而不只是正文区：侧边栏的章节标题里也有公式，
        // 例如「7.1 冒泡、选择、插入：三种 $O(n^2)$ 排序」「7.4 ……绕不开 $n \log n$」，
        // 只处理 .page-inner 会让目录里残留裸 LaTeX。
        var root = document.body;
        if (!root) return;
        // 先落标记再渲染，避免 page.change 与 DOMContentLoaded 同时触发时重复渲染
        if (root.getAttribute('data-math-rendered') === '1') return;
        root.setAttribute('data-math-rendered', '1');

        ensureKatex().then(function () {
            if (typeof window.renderMathInElement !== 'function') return;
            window.renderMathInElement(root, {
                delimiters: DELIMITERS,
                ignoredTags: IGNORED_TAGS,
                ignoredClasses: ['katex'],
                throwOnError: false,
                errorColor: '#cc0000'
            });
        });
    }

    gitbook.events.bind('page.change', renderMath);
    gitbook.events.bind('start', renderMath);

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', renderMath);
    } else {
        renderMath();
    }
});
