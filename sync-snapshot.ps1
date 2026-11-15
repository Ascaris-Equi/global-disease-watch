# sync-snapshot.ps1 - publish the collected outbreak snapshot to the web build and GitHub.
# ASCII only: PowerShell 5.1 reads .ps1 as ANSI on a GBK system.
param(
  [switch]$NoPush,
  [switch]$Force
)

$ErrorActionPreference = 'Stop'
$root = 'C:\ProgramData\MJH-IdleBoard'
$src  = Join-Path $root 'science\outbreaks.json'
$dist = Join-Path $root 'dist\latest.json'
$web  = Join-Path $root 'web\snapshot.json'
$log  = Join-Path $root 'data\sync-snapshot.log'

function Write-Log([string]$m) {
  $line = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') + ' ' + $m
  Add-Content -LiteralPath $log -Value $line -Encoding UTF8
  Write-Output $m
}

if (-not (Test-Path $src)) { Write-Log 'ERROR source missing: science\outbreaks.json'; exit 1 }

# Validate before publishing: never ship a truncated or malformed snapshot.
try {
  $raw = [IO.File]::ReadAllText($src, [Text.UTF8Encoding]::new($false))
  $obj = $raw | ConvertFrom-Json
  $events = @($obj.events)
  if ($events.Count -lt 5) { Write-Log ("ERROR snapshot too small: " + $events.Count + " events"); exit 1 }
  $dates = $events | Where-Object { $_.date } | ForEach-Object { [string]$_.date }
} catch {
  Write-Log ('ERROR snapshot json invalid: ' + $_.Exception.Message)
  exit 1
}

New-Item -ItemType Directory -Force -Path (Split-Path $dist) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $web)  | Out-Null

$before = if (Test-Path $web) { (Get-FileHash $web -Algorithm SHA256).Hash } else { '' }
Copy-Item $src $dist -Force
Copy-Item $src $web  -Force
$after = (Get-FileHash $web -Algorithm SHA256).Hash

$latest = ($dates | Sort-Object | Select-Object -Last 1)
Write-Log ("snapshot published: " + $events.Count + " events, latest report " + $latest + ", changed=" + ($before -ne $after))

if ($before -eq $after -and -not $Force) {
  Write-Log 'no content change, skip commit'
  exit 0
}

if ($NoPush) { Write-Log 'NoPush set, stopping after local copy'; exit 0 }

# --- publish to GitHub ---
$tokenFile = Join-Path $root '.push-token'
if (-not (Test-Path $tokenFile)) { Write-Log 'ERROR .push-token missing, cannot push'; exit 1 }
$token = (Get-Content $tokenFile -Raw).Trim()
$remote = 'https://' + $token + '@github.com/Ascaris-Equi/global-disease-watch.git'

Push-Location $root
try {
  git remote set-url origin $remote 2>&1 | Out-Null
  git add -- web/snapshot.json 2>&1 | Out-Null
  if (Test-Path (Join-Path $root 'dist\latest.json')) { git add -f -- dist/latest.json 2>&1 | Out-Null }
  $staged = git diff --cached --name-only
  if (-not $staged) {
    Write-Log 'nothing staged, skip'
  } else {
    $msg = 'data: snapshot ' + (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mmZ') + ' (' + $events.Count + ' events)'
    git -c user.name='MJH' -c user.email='jiahao.ma@connect.hku.hk' commit -q -m $msg 2>&1 | Out-Null
    $push = git push origin main 2>&1
    if ($LASTEXITCODE -ne 0) {
      Write-Log ('WARN push failed: ' + ($push | Select-Object -Last 1))
    } else {
      Write-Log ('pushed: ' + $msg)
    }
  }
} finally {
  git remote set-url origin 'https://github.com/Ascaris-Equi/global-disease-watch.git' 2>&1 | Out-Null
  Pop-Location
}
