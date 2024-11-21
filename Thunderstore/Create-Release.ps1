if ( -Not (Test-Path "..\Krod\bin\Release\netstandard2.1\Krod.dll") ) {
    Write-Host "Build release"
    Break
}

if (-Not (Test-Path "..\Out\krod.assetbundle") ) {
    Write-Host "Build assets"
    Break
}

if (Test-Path ".\Thunderstore.zip") {
    Remove-Item ".\Thunderstore.zip"
}

$p = @{
    Path = "CHANGELOG.md", 
        "icon.png", 
        "manifest.json", 
        "README.md", 
        "..\Krod\Krod.language",
        "..\Krod\bin\Release\netstandard2.1\Krod.dll",
        "..\Out\krod.assetbundle"
    CompressionLevel = "Optimal"
    DestinationPath = ".\Thunderstore.zip"
}

Compress-Archive @p