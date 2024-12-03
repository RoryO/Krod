if ( -Not (Test-Path "..\Krod\bin\Release\netstandard2.1\Krod.dll") ) {
    Write-Host "Build release"
    Break
}

if (-Not (Test-Path "..\Out\krod.assetbundle") ) {
    Write-Host "Build assets"
    Break
}

if (-Not (Test-Path "..\WWise Project\GeneratedSoundBanks\Windows\krod.sound") ) {
    Write-Host "Create sound bank"
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
        "..\Out\krod.assetbundle",
        "..\WWise Project\GeneratedSoundBanks\Windows\krod.sound"
    CompressionLevel = "Optimal"
    DestinationPath = ".\Thunderstore.zip"
}

Compress-Archive @p
Remove-Item "..\Krod\bin\Release\netstandard2.1\Krod.dll"