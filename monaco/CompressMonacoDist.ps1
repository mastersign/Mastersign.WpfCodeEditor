$monacoDir = $PSScriptRoot
$monacoDistDir = "$monacoDir\dist"
$wpfControlDir = Resolve-Path "$monacoDir\..\Mastersign.WpfCodeEditor"
$targetFilename = "$wpfControlDir\monaco.zip"

$sources = @(
    "$monacoDistDir\*.js"
    "$monacoDistDir\*.css"
    "$monacoDistDir\*.html"
    "$monacoDistDir\*.ttf"
)
Compress-Archive -Path $sources -DestinationPath $targetFilename -CompressionLevel Optimal
