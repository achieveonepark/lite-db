// 첫 방문 시 한 번만 localStorage 의 'theme' 캐시를 비워서 defaultMode(다크)가 적용되게 한다.
// 이미 리셋된 적이 있으면(FLAG 존재) 사용자의 토글 선택을 그대로 존중.
(function () {
  if (typeof window === 'undefined') return;
  var FLAG = 'somiri-docs.theme-reset.v1';
  try {
    if (window.localStorage.getItem(FLAG) === '1') return;
    window.localStorage.removeItem('theme');
    window.localStorage.setItem(FLAG, '1');
    document.documentElement.setAttribute('data-theme', 'dark');
  } catch (e) {
    /* localStorage 비활성 환경 — 무시 */
  }
})();
