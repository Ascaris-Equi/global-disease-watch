# MJH-IdleBoard · Electron 桌面壳

把 `../web/` 的纯静态屏保包成 Windows / macOS / Linux 桌面应用。只加窗口壳，不复制、不修改页面。

## 文件

| 文件 | 说明 |
|---|---|
| `main.js` | 主进程：无边框全屏、单实例锁、Esc / 双击退出、禁右键和 DevTools 快捷键 |
| `package.json` | `main` 指向 `main.js`，`start` 脚本，devDependencies 只有 electron / electron-builder |
| `electron-builder.yml` | 三平台打包目标、web 资源、镜像 |
| `build.ps1` | 本机（无 npm/npx）的一键打包脚本 |
| `pnpm-lock.yaml` | pnpm 锁文件 |
| `dist/` | 构建产物（构建后生成） |

## 安装

要求 Node.js >= 22.12（Electron 45 的要求）。依赖只有：

- `electron@45.2.3`
- `electron-builder@26.18.0`

```bash
npm install
```

> 用 npm 构建时先删掉 `pnpm-lock.yaml`：electron-builder 会按 lockfile 选择包管理器，留着它会去找 pnpm。本机用 pnpm，保留即可。

本机运行时没有 npm/npx，只有内置 node 和 pnpm：

```powershell
$node = "C:\Users\Administrator\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\node\bin\node.exe"
$pnpm = "C:\Users\Administrator\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\pnpm\bin\pnpm.cjs"
& $node $pnpm install
```

如果 `node_modules\electron\dist\electron.exe` 不存在（pnpm 拦截了 postinstall），手动补一次 Electron 二进制：

```powershell
$env:ELECTRON_MIRROR = "https://npmmirror.com/mirrors/electron/"
& $node "node_modules\electron\install.js"
```

首次下载约 158 MB（压缩包）。`electron-builder.yml` 里已配置 `electronDownload.mirror` 指向 npmmirror；打包时的 NSIS / winCodeSign 等工具走 `ELECTRON_BUILDER_BINARIES_MIRROR`，`build.ps1` 会自动设置。

## 本地跑

```bash
npm start
```

本机没有 npx，用等价的 CLI（`node node_modules/electron/cli.js .` 与 `npx electron .` 行为一致）：

```powershell
& $node "node_modules\electron\cli.js" .
```

kiosk 模式（默认关闭，四种开法任选）：

```powershell
npm run start:kiosk
# 或
electron . --kiosk
# 或
$env:MJH_KIOSK = "1"; npm start
# 或把 package.json 里 mjh.kiosk 改成 true
```

退出：

- `Esc`
- 鼠标双击
- 原生关闭（Alt+F4、窗口管理器、远程桌面软件发来的 WM_CLOSE）会被忽略，这是屏保语义；Windows 关机 / 注销仍会正常退出。

鼠标指针不在窗口层处理：`web/app.js` 在 2.6 秒无操作后给 `body` 加 `idle`，`style.css` 的 `body.idle { cursor: none }` 负责隐藏。

## 打包

标准命令：

```bash
npm run dist:win     # Windows: portable + nsis
npm run dist:mac     # macOS: dmg（x64 + arm64）
npm run dist:linux   # Linux: AppImage（x64）
npm run dist         # 三平台
npm run pack:dir     # Windows 解包目录，调试用
```

macOS 的 dmg 只能在 macOS 上打；Linux AppImage 建议在 Linux / CI 上打。Windows 上跑 `dist:mac` 会失败。

本机（没有 npm/npx）：

```powershell
.\build.ps1 -Target win      # portable + nsis
.\build.ps1 -Target dir      # 只出 win-unpacked
.\build.ps1 -Target mac
.\build.ps1 -Target linux
.\build.ps1 -Target all
```

`build.ps1` 会自动找 node / pnpm，临时生成 electron-builder 需要的 pnpm 探针，跑完清理。脚本带 UTF-8 BOM，Windows PowerShell 5.1 和 pwsh 都能直接跑。

### 产物（dist/）

| 平台 | 目标 | 文件 | 本机实测 |
|---|---|---|---|
| Windows | portable | `MJH-IdleBoard-1.0.0-x64-portable.exe` | 105.28 MB（110,390,787 字节） |
| Windows | nsis | `MJH-IdleBoard-1.0.0-x64-setup.exe` | 115.76 MB（121,388,122 字节） |
| Windows | 解包目录 | `win-unpacked\MJH-IdleBoard.exe` | 238.3 MB |
| macOS | dmg | `MJH-IdleBoard-1.0.0-x64.dmg` / `MJH-IdleBoard-1.0.0-arm64.dmg` | 未在本机产出 |
| Linux | AppImage | `MJH-IdleBoard-1.0.0-x64.AppImage` | 未在本机产出 |

## web/ 是怎么进包的

- 开发态：`main.js` 读 `path.join(__dirname, '..', 'web', 'index.html')`，即 `C:\ProgramData\MJH-IdleBoard\web\index.html`。
- 打包态：`electron-builder.yml` 的 `extraResources` 把 `../web` 复制到 `resources/web`；`main.js` 优先读 `path.join(process.resourcesPath, 'web', 'index.html')`。
- `files` 里按需求只列了 `main.js` 和 `../web/**`。但 electron-builder 的 `files` 不能收录 app 目录之外的文件，`../web/**` 实际匹配不到；真正把 web 打进去的是 `extraResources`。`app.asar` 里只有 `main.js` / `package.json`，`web/` 在 asar 外，约 500 KB。
- 如果找不到页面，窗口会显示一个带路径的深色错误页，不会只留黑屏。

## 已验证（Windows 10 19045，Electron 45.2.3）

- `node --check main.js` 通过。
- 开发态启动：窗口 2560×1440 全屏，页面加载 40 条事件 / 31 国 / 177 个世界国家，Canvas 动画持续出帧；CDP 截图确认画面完整。
- 路径解析：开发态 `/web/index.html`；`win-unpacked` 和 portable 均为 `resources\web\index.html`。
- Esc：开发态、`win-unpacked` 实测退出。
- 双击：开发态、portable 实测退出（portable 退出后自解压目录和 launcher 一起结束）。
- kiosk：`--kiosk` 启动日志 `kiosk = true`，窗口正常，Esc 退出。
- 单实例：第二个实例 1.1 秒内退出，只保留一个窗口。
- portable：从 `%TEMP%\<随机>\resources\web\index.html` 正常加载。

## 已知限制

- 未配置代码签名：Windows SmartScreen / macOS Gatekeeper 会提示，正式分发需自备证书。
- 未配置自定义图标，使用 Electron 默认图标。
- macOS dmg、Linux AppImage 只完成配置，本机没有实际产出。
- 本机安全软件曾把 `node_modules\electron\dist\electron.exe` 改名为 `electron.exe.off`；已恢复。若开发态突然提示找不到 electron.exe，先检查这个文件，把 `.off` 改回来即可。打包出的 exe 未受影响。
