param([string]$Source=$PSScriptRoot)
# Builds the renderer (WPF, .NET Framework 4.8) and places the native supervisor as the screen saver.
$ErrorActionPreference='Stop'
$fx=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$wpf=Join-Path $fx 'WPF'
$refs=@(
 (Join-Path $wpf 'PresentationCore.dll'),(Join-Path $wpf 'PresentationFramework.dll'),(Join-Path $wpf 'WindowsBase.dll'),
 (Join-Path $fx 'System.Xaml.dll'),'System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Web.Extensions.dll'
)|ForEach-Object {'/r:'+$_}
$files=@('WallApp.cs','WallData.cs','WallView.cs','WallCapsid.cs','WallHardware.cs','WallCards.cs','WallLang.cs','WallMolecule.cs','WallGlobe.cs','WallRig.cs','WallChain.cs','WallLinks.cs')|ForEach-Object {Join-Path $Source $_}
$renderer=Join-Path $Source 'ResearchWall.Renderer.exe'
& (Join-Path $fx 'csc.exe') /nologo /target:winexe /optimize+ /platform:anycpu /codepage:65001 $refs ('/out:'+$renderer) $files
if($LASTEXITCODE -ne 0){throw 'Renderer compilation failed'}
# Native supervisor (Rust): rebuilt when cargo is available, otherwise the checked-in binary is used.
$saverDir=Join-Path $Source 'native\saver'
$prebuilt=Join-Path $Source 'native\bin\mjh-saver.exe'
$cargo=Join-Path $env:USERPROFILE '.cargo\bin\cargo.exe'
if((Test-Path -LiteralPath $cargo) -and (Test-Path -LiteralPath (Join-Path $saverDir 'Cargo.toml'))){
 Push-Location $saverDir
 try{& $cargo build --release --quiet;if($LASTEXITCODE -ne 0){throw 'Supervisor build failed'}}finally{Pop-Location}
 New-Item -ItemType Directory -Force -Path (Split-Path $prebuilt)|Out-Null
 Copy-Item -LiteralPath (Join-Path $saverDir 'target\release\mjh-saver.exe') -Destination $prebuilt -Force
}
if(-not (Test-Path -LiteralPath $prebuilt)){throw 'Supervisor binary missing: native\bin\mjh-saver.exe'}
Copy-Item -LiteralPath $prebuilt -Destination (Join-Path $Source 'MJH-IdleBoard.scr') -Force
Copy-Item -LiteralPath $prebuilt -Destination (Join-Path $Source 'MJH-IdleBoard.exe') -Force
Write-Output ('Built renderer '+$renderer+' and supervisor MJH-IdleBoard.scr/.exe')