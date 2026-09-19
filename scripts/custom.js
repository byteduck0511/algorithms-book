(function () {
  "use strict";

  // 覆盖页面的路由切换方法，并抛出一个自定义事件，以实现对页面路由切换的监听

  // 1. 保存原始方法引用
  const originalPushState = history.pushState;
  const originalReplaceState = history.replaceState;

  // 2. 定义一个自定义事件处理器
  function onRouteChange() {
    // 在这里执行你的逻辑
    console.log("HonKit 路由已变化 ->", window.location.pathname);
    // 例如：触发自定义事件，方便其他脚本监听
    document.dispatchEvent(new CustomEvent("honkit:routechange"));
  }

  // 3. 重写 pushState
  history.pushState = function (state, title, url) {
    // 调用原始方法
    const result = originalPushState.apply(this, arguments);
    // 执行你的回调
    onRouteChange();
    return result;
  };

  // 4. 重写 replaceState
  history.replaceState = function (state, title, url) {
    const result = originalReplaceState.apply(this, arguments);
    onRouteChange();
    return result;
  };

  // 5. 监听浏览器前进/后退
  window.addEventListener("popstate", onRouteChange);
  // 为以防万一，也监听 hashchange
  window.addEventListener("hashchange", onRouteChange);

  function renderKatex() {
    if (window.renderMathInElement) {
      console.log("render Katex.");

      window.renderMathInElement(document.body, {
        delimiters: [
          { left: "$$", right: "$$", display: true },
          { left: "$", right: "$", display: false },
          { left: "\\(", right: "\\)", display: false },
          { left: "\\[", right: "\\]", display: true },
        ],
        throwOnError: false,
        // 没有生效
        // ignoredTags: ["nav"],
        // ignoredClasses: ["book-summary", "chapter"],
      });
    }
  }

  // 页面加载完成后，执行渲染和显示流程
  document.addEventListener("DOMContentLoaded", function () {
    console.log("===after DOMContentLoaded ===");

    // 延迟执行，确保DOM完全加载
    setTimeout(renderKatex, 10);
  });

  // 针对 PJAX/SPA 路由的兼容处理（如果 HonKit 主题使用了类似机制）
  document.addEventListener("honkit:routechange", function () {
    console.log("===after honkit:routechange===");

    // 延迟执行，确保DOM完全加载
    setTimeout(renderKatex, 10);
  });
})();
