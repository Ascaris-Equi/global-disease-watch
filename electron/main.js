'use strict';

/*
 * MJH-IdleBoard — Electron 全屏壳
 *
 * 只做窗口/生命周期：加载 ../web/index.html，启动即全屏，Esc 或双击退出。
 * 鼠标指针隐藏、2.6 秒无操作淡出等交互留在 web/ 页面里，这里不重复实现。
 */

const { app, BrowserWindow, Menu } = require('electron');
const fs = require('fs');
const path = require('path');

const QUIT_URL = 'mjh-idleboard://quit';

/* ---------------------------------------------------------------- 配置 */

function truthy(value) {
  return /^(1|true|yes|on)$/i.test(String(value || '').trim());
}

function falsy(value) {
  return /^(0|false|no|off)$/i.test(String(value || '').trim());
}

function readKioskFromPackage() {
  try {
    const pkg = require('./package.json');
    return Boolean(pkg && pkg.mjh && pkg.mjh.kiosk);
  } catch (err) {
    return false;
  }
}

// 关闭按钮/进程退出的开关：环境变量 MJH_KIOSK，命令行 --kiosk / --no-kiosk。
function resolveKiosk() {
  const argv = process.argv;
  if (argv.includes('--kiosk')) return true;
  if (argv.includes('--no-kiosk')) return false;
  if (truthy(process.env.MJH_KIOSK)) return true;
  if (falsy(process.env.MJH_KIOSK)) return false;
  return readKioskFromPackage();
}

const KIOSK = resolveKiosk();

/* ------------------------------------------------------------ 页面路径 */

function firstExisting(candidates) {
  for (const candidate of candidates) {
    try {
      if (candidate && fs.existsSync(candidate)) return candidate;
    } catch (err) {
      /* 路径不可读就试下一个 */
    }
  }
  return null;
}

/*
 * 开发态：electron/../web/index.html
 * 打包态：extraResources 把 web/ 放在 resources/web/（process.resourcesPath）
 * 兼容 asar 内 web/（files 的 ../web/** 命中时）与 electron/web/ 的兜底。
 */
function resolveWebIndex() {
  const candidates = [];
  if (app.isPackaged) {
    candidates.push(path.join(process.resourcesPath, 'web', 'index.html'));
  } else {
    candidates.push(path.join(__dirname, '..', 'web', 'index.html'));
  }
  candidates.push(
    path.join(__dirname, '..', 'web', 'index.html'),
    path.join(process.resourcesPath || '', 'web', 'index.html'),
    path.join(__dirname, 'web', 'index.html')
  );

  const match = firstExisting(candidates);
  if (!match) {
    console.error('[MJH-IdleBoard] 找不到 web/index.html，尝试过：');
    for (const candidate of candidates) console.error('  ' + candidate);
    return candidates[0];
  }
  return match;
}

/* ------------------------------------------------------------ 单实例锁 */

const hasSingleInstanceLock = app.requestSingleInstanceLock();

if (!hasSingleInstanceLock) {
  app.quit();
} else {
  let mainWindow = null;
  let quitting = false;

  function quitApp() {
    if (quitting) return;
    quitting = true;
    app.quit();
  }

  /*
   * 双击退出：窗口层没有 dblclick 事件，往页面里注入一个监听。
   * 不引 preload，所以用一次 will-navigate 拦截来通知主进程。
   */
  function installDoubleClickQuit(win) {
    win.webContents
      .executeJavaScript(
        `(() => {
          if (window.__mjhDoubleClickQuit) return;
          window.__mjhDoubleClickQuit = true;
          window.addEventListener('dblclick', () => {
            window.location.href = ${JSON.stringify(QUIT_URL)};
          }, true);
        })();`,
        true
      )
      .catch(() => {});
  }

  function showLoadError(win, indexPath) {
    const html = [
      '<!doctype html><meta charset="utf-8">',
      '<body style="margin:0;background:#050607;color:#e8e2d8;font:16px/1.6 system-ui,sans-serif;padding:48px">',
      '<h1 style="font-size:22px">MJH-IdleBoard 加载失败</h1>',
      '<p>找不到页面文件：</p>',
      '<pre style="white-space:pre-wrap;color:#b9b0a4">' +
        String(indexPath).replace(/[&<>]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;' }[c])) +
        '</pre>',
      '<p>请确认 <code>web/index.html</code> 与 main.js 的相对位置，或重新打包。</p>',
      '</body>',
    ].join('');
    win.loadURL('data:text/html;charset=utf-8,' + encodeURIComponent(html));
  }

  function createWindow() {
    // 应用菜单（含 Ctrl+R / F12 等默认加速键）整体移除。
    Menu.setApplicationMenu(null);

    const indexPath = resolveWebIndex();
    console.log('[MJH-IdleBoard] web/index.html ->', indexPath);
    console.log('[MJH-IdleBoard] kiosk =', KIOSK, '| packaged =', app.isPackaged);

    const win = new BrowserWindow({
      show: false,
      frame: false,
      fullscreen: true,
      kiosk: KIOSK,
      autoHideMenuBar: true,
      backgroundColor: '#050607',
      title: '全球传染病监测 · GLOBAL WATCH',
      webPreferences: {
        contextIsolation: true,
        nodeIntegration: false,
        sandbox: true,
        devTools: false,
        spellcheck: false,
        backgroundThrottling: false,
      },
    });
    mainWindow = win;

    // 菜单栏：Windows/Linux 移除窗口菜单；macOS 的菜单是全局的，由上面 setApplicationMenu(null) 处理。
    if (process.platform !== 'darwin') {
      win.removeMenu();
      win.setMenuBarVisibility(false);
    }

    win.once('ready-to-show', () => {
      win.show();
      if (!win.isFullScreen()) win.setFullScreen(true);
      if (KIOSK && !win.isKiosk()) win.setKiosk(true);
      win.focus();
    });

    win.webContents.on('dom-ready', () => installDoubleClickQuit(win));

    // 禁止一切页面内跳转；只放行自定义的退出 URL。
    win.webContents.on('will-navigate', (event, url) => {
      event.preventDefault();
      if (String(url).indexOf(QUIT_URL) === 0) quitApp();
    });
    win.webContents.on('will-redirect', (event) => event.preventDefault());
    win.webContents.setWindowOpenHandler(() => ({ action: 'deny' }));
    win.webContents.on('will-attach-webview', (event) => event.preventDefault());

    // 右键菜单：Electron 默认不弹，这里再明确关一次。
    win.webContents.on('context-menu', (event) => event.preventDefault());

    // 禁用 DevTools 快捷键；Esc 退出。
    win.webContents.on('before-input-event', (event, input) => {
      if (input.type !== 'keyDown') return;
      const key = String(input.key || '');
      const upper = key.toUpperCase();
      const ctrlOrCmd = input.control || input.meta;

      if (key === 'Escape') {
        event.preventDefault();
        quitApp();
        return;
      }
      if (key === 'F12') {
        event.preventDefault();
        return;
      }
      // Windows/Linux: Ctrl+Shift+I/J/C；macOS: Cmd+Alt+I/J/C
      if (ctrlOrCmd && input.shift && (upper === 'I' || upper === 'J' || upper === 'C')) {
        event.preventDefault();
        return;
      }
      if (input.meta && input.alt && (upper === 'I' || upper === 'J' || upper === 'C')) {
        event.preventDefault();
      }
    });

    win.webContents.on('did-fail-load', (event, code, description, validatedURL, isMainFrame) => {
      if (!isMainFrame || code === -3 /* ERR_ABORTED */) return;
      console.error('[MJH-IdleBoard] did-fail-load', code, description, validatedURL);
      showLoadError(win, indexPath);
    });

    win.on('close', (event) => {
      // 屏保语义：只允许 Esc / 双击 / 系统关机退出。
      // 忽略 Alt+F4、窗口管理器和远程桌面等外部发来的关闭请求。
      if (!quitting) event.preventDefault();
    });
    win.on('query-session-end', () => {
      quitting = true;
    });
    win.on('session-end', () => {
      quitting = true;
    });
    win.on('closed', () => {
      if (mainWindow === win) mainWindow = null;
    });

    win.loadFile(indexPath).catch((err) => {
      console.error('[MJH-IdleBoard] loadFile failed:', err);
      showLoadError(win, indexPath);
    });
  }

  app.on('second-instance', () => {
    if (!mainWindow) {
      if (app.isReady()) createWindow();
      return;
    }
    if (mainWindow.isMinimized()) mainWindow.restore();
    mainWindow.show();
    mainWindow.focus();
  });

  app.on('before-quit', () => {
    // 系统退出 / Cmd+Q / 我们自己的 app.quit() 都从这条路放行 close。
    quitting = true;
  });

  app.on('window-all-closed', () => {
    // 屏保壳：窗口关掉就退出，不保留 macOS 的驻留行为。
    app.quit();
  });

  app.on('activate', () => {
    if (mainWindow === null && app.isReady()) createWindow();
  });

  app.whenReady().then(() => {
    createWindow();
  });
}
