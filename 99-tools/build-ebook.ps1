<#
    构建电子书：EPUB + PDF

    用法（在项目根目录或任意位置）：
        powershell -ExecutionPolicy Bypass -File 99-tools\build-ebook.ps1

    流程：
        1. python 99-tools/build_ebook.py   按 SUMMARY.md 拼接 63 节，剥离状态卡、规整中文引号
        2. pandoc  ->  EPUB （--mathml，离线可用）
        3. pandoc  ->  自包含 HTML
        4. Edge headless --print-to-pdf  ->  PDF

    为什么 PDF 走 HTML 而不是 LaTeX：
        本机没有任何 LaTeX 引擎（xelatex/lualatex/pdflatex/tectonic 全无），
        而书里有 3566 个行内公式 + 252 个块级公式。Chromium 自带 MathML Core
        渲染，配合 pandoc --mathml 可以完全离线出 PDF，不必装几 GB 的 TeX Live。

    注意：pandoc 的 markdown 默认要求列表前有空行，而本书每节的
        「学习目标 / 先修」后面直接跟 `- ` 列表，所以要开
        +lists_without_preceding_blankline，否则列表会被压成一行。
        同时用 -smart 关掉 pandoc 的引号转换：它按英文规则判断开合，
        中文「个"重复"」会被判成闭引号，方向反掉（引号已由 python 侧转好）。
#>
$ErrorActionPreference = 'Stop'

$ToolsDir = $PSScriptRoot
$Root     = Split-Path -Parent $ToolsDir
$EbookDir = Join-Path $Root '_ebook'
$Md       = Join-Path $EbookDir 'book.md'
$Epub     = Join-Path $EbookDir 'algorithms-book.epub'
$Html     = Join-Path $EbookDir 'book.html'
$Pdf      = Join-Path $EbookDir 'algorithms-book.pdf'

$Python = 'C:\Users\qinsf\AppData\Local\Programs\Python\Python314\python.exe'
$Edge   = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'

$PandocArgs = @(
    '-f', 'markdown-smart+lists_without_preceding_blankline',
    '--toc', '--toc-depth=2',
    '--mathml',
    '--syntax-highlighting=tango',
    '--metadata', 'lang=zh-CN'
)

function Step($msg) { Write-Host "`n=== $msg ===" -ForegroundColor Cyan }
function Need($path, $what) {
    if (-not (Test-Path $path)) { throw "找不到 $what：$path" }
}

Need $Python 'Python'
Need $Edge   'Edge'

# --- 1. 拼书 -----------------------------------------------------------------
Step '1/4  生成电子书源 (book.md)'
$env:PYTHONIOENCODING = 'utf-8'
& $Python (Join-Path $ToolsDir 'build_ebook.py')
if ($LASTEXITCODE -ne 0) { throw 'build_ebook.py 失败' }

# --- 2. EPUB -----------------------------------------------------------------
Step '2/4  生成 EPUB'
& pandoc $Md -o $Epub @PandocArgs --css (Join-Path $ToolsDir 'ebook.css')
if ($LASTEXITCODE -ne 0) { throw 'pandoc EPUB 失败' }
Write-Host ("  EPUB  {0:N2} MB" -f ((Get-Item $Epub).Length / 1MB)) -ForegroundColor Green

# --- 3. 自包含 HTML（PDF 的中间产物）-----------------------------------------
Step '3/4  生成自包含 HTML'
& pandoc $Md -o $Html -t html5 --standalone --embed-resources @PandocArgs `
    --css (Join-Path $ToolsDir 'ebook.css') `
    --css (Join-Path $ToolsDir 'print.css')
if ($LASTEXITCODE -ne 0) { throw 'pandoc HTML 失败' }
Write-Host ("  HTML  {0:N2} MB" -f ((Get-Item $Html).Length / 1MB)) -ForegroundColor Green

# --- 4. PDF ------------------------------------------------------------------
Step '4/4  打印 PDF（Edge headless，约 20 秒）'
if (Test-Path $Pdf) { Remove-Item $Pdf -Force }
$url  = 'file:///' + ($Html -replace '\\', '/')
$prof = Join-Path $env:TEMP 'edge-pdf-profile'
$sw   = [Diagnostics.Stopwatch]::StartNew()

& $Edge --headless=new --disable-gpu --no-pdf-header-footer `
        --run-all-compositor-stages-before-draw --virtual-time-budget=60000 `
        --user-data-dir="$prof" --print-to-pdf="$Pdf" $url 2>&1 |
    Select-String 'bytes written' | ForEach-Object { Write-Host "  $_" }
$sw.Stop()

if (-not (Test-Path $Pdf)) { throw 'PDF 未生成' }
Write-Host ("  PDF   {0:N2} MB  ({1:N1} 秒)" -f ((Get-Item $Pdf).Length / 1MB), $sw.Elapsed.TotalSeconds) -ForegroundColor Green

Step '完成'
Get-ChildItem $EbookDir -File | Where-Object { $_.Extension -in '.epub', '.pdf', '.md', '.html' } |
    Select-Object Name, @{n = 'MB'; e = { [math]::Round($_.Length / 1MB, 2) } } |
    Format-Table -AutoSize
