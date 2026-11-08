#Requires -Version 5.1
<#
  一键打包脚本（主要给没有 npm/npx、只有内置 node + pnpm 的机器用）。

  用法：
    .\build.ps1 -Target win     # Windows: portable + nsis
    .\build.ps1 -Target mac     # macOS: dmg
    .\build.ps1 -Target linux   # Linux: AppImage
    .\build.ps1 -Target dir     # 只出 unpacked 目录，调试用
    .\build.ps1 -Target all     # 三平台（需要对应系统）
#>
param(
  [ValidateSet('win', 'mac', 'linux', 'dir', 'all')]
  [string]$Target = 'win'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$shimDir = $null

Push-Location $root
try {
  # 1. 找 node
  $node = (Get-Command node -ErrorAction SilentlyContinue).Source
  if (-not $node) {
    $bundledNode = 'C:\Users\Administrator\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\node\bin\node.exe'
    if (Test-Path $bundledNode) { $node = $bundledNode }
  }
  if (-not $node) {
    throw '找不到 node。请安装 Node.js >= 22.12 并加入 PATH。'
  }

  # 2. electron-builder 会调用包管理器探测生产依赖；本机 pnpm 只有 .mjs，包一层 .cmd
  $pnpm = (Get-Command pnpm -ErrorAction SilentlyContinue).Source
  if (-not $pnpm) {
    $bundledPnpm = 'C:\Users\Administrator\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\pnpm\bin\pnpm.cjs'
    if (Test-Path $bundledPnpm) {
      $shimDir = Join-Path $root '.build-tmp'
      New-Item -ItemType Directory -Force -Path $shimDir | Out-Null
      $shim = Join-Path $shimDir 'pnpm.cmd'
      @(
        '@echo off',
        ('"' + $node + '" "' + $bundledPnpm + '" %*')
      ) | Set-Content -LiteralPath $shim -Encoding ASCII
      $env:Path = $shimDir + ';' + $env:Path
    }
  }
  if (-not (Get-Command pnpm -ErrorAction SilentlyContinue)) {
    throw '找不到 pnpm。请安装 pnpm 并加入 PATH，或改用 npm run dist:win。'
  }

  # 3. 下载镜像（不覆盖用户已设置的值）
  if (-not $env:ELECTRON_MIRROR) {
    $env:ELECTRON_MIRROR = 'https://npmmirror.com/mirrors/electron/'
  }
  if (-not $env:ELECTRON_BUILDER_BINARIES_MIRROR) {
    $env:ELECTRON_BUILDER_BINARIES_MIRROR = 'https://npmmirror.com/mirrors/electron-builder-binaries/'
  }
  if (-not $env:CSC_IDENTITY_AUTO_DISCOVERY) {
    $env:CSC_IDENTITY_AUTO_DISCOVERY = 'false'
  }

  $builder = Join-Path $root 'node_modules\electron-builder\cli.js'
  if (-not (Test-Path $builder)) {
    throw 'electron-builder 未安装。先运行 npm install（本机：pnpm install）。'
  }

  $buildArgs = @(switch ($Target) {
    'dir' { @('--win', '--dir') }
    'all' { @('--win', '--mac', '--linux') }
    default { @('--' + $Target) }
  })

  & $node $builder @buildArgs
  if ($LASTEXITCODE -ne 0) {
    throw "electron-builder 退出码 $LASTEXITCODE"
  }
}
finally {
  Pop-Location
  if ($shimDir -and (Test-Path $shimDir)) {
    Remove-Item -LiteralPath $shimDir -Recurse -Force -ErrorAction SilentlyContinue
  }
}
