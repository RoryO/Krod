$p = @{
    Path = "CHANGELOG.md", 
        "icon.png", 
        "manifest.json", 
        "README.md", 
        "..\Krod\Krod.language",
        "..\Krod\bin\Release\netstandard2.1\Krod.dll"
    CompressionLevel = "Optimal"
    DestinationPath = ".\Thunderstore.zip"
}

Compress-Archive @p