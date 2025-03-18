## Project Layout

Assets -- Unity Project with Thunderkit
Krod -- C# project
Out -- Empty. Thunderkit asset export location as krod.assetbundle
ProjectSettings -- Thunderkit stuff
Thunderstore -- Helpers for packaging an upload to Thunderstore.io
WWise Project -- WWise
WWise Project/Originals -- Master completed sound files
WWise Project/GeneratedSoundBanks/Windows -- Output sound bank file as krod.sound

## Initial Setup

- Using [r2modman](https://r2modman.com/), create a profle named 'dev'.
- Create the assetbundle by executing the pipeline in the Unity project
- Create the soundbank by in WWise
- Create a new directory `%APPDATA%/r2modmanplus-local/profiles/dev/BepInEx/plugins/Krod/`
- Copy `Out/krod.assetbundle` and `WWise Project/GeneratedSoundBanks/Windows/krod.sound` to `%APPDATA%/r2modmanplus-local/profiles/dev/BepInEx/plugins/Krod/`
- Build the C# project, which automatically copies the dll and language files to the r2modman dev profile
- Manually add the R2API dependencies with r2modman. Reference the [project file](Krod/Krod.csproj)
- Optional ease of use: create a shortcut to launch the exe without having to go through r2modman with target `"%PROGRAMFILES(X86)%\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2.exe" --doorstop-enabled true --doorstop-target-assembly "%APPDATA%\r2modmanPlus-local\RiskOfRain2\profiles\dev\BepInEx\core\BepInEx.Preloader.dll"`

## Attributions

### Logo font

IvySoft Variable -- https://fonts.adobe.com/fonts/ivysoft-variable

### Sound Attributions

FilmCow Sound Packs -- https://filmcow.itch.io/

Sonniss GDC Archive 2024 -- https://gdc.sonniss.com/

Sonniss GDC Archive 2015-2023 -- https://sonniss.com/gameaudiogdc

Library of The Gods -- https://baseheadinc.com/

fish slap ground or snow writhing wet.wav by kyles -- https://freesound.org/s/450830/ -- License: Creative Commons 0

slot_machine_insert_3_coins_and_spin.wav by lukaso -- https://freesound.org/s/69698/ -- License: Sampling+

machine hydraulic metal cutter close crunch1.flac by kyles -- https://freesound.org/s/453303/ -- License: Creative Commons 0

Applause, huge, thunderous by peridactyloptrix -- https://freesound.org/s/196094/ -- License: Creative Commons 0

Small Clap by kellieskitchen -- https://freesound.org/s/209989/ -- License: Attribution 3.0

Cheer 2.wav by jayfrosting -- https://freesound.org/s/333404/ -- License: Creative Commons 0

Clapping at a conference by lucyve -- https://freesound.org/s/370244/ -- License: Creative Commons 0

Clapping Large Crowd at Choir Concert 2 by pluralz -- https://freesound.org/s/735562/ -- License: Creative Commons 0

Pouring the water/coffee by CaKon -- https://freesound.org/s/522143/ -- License: Attribution 4.0

Card Shuffle by empraetorius -- https://freesound.org/s/201253/ -- License: Attribution 4.0

Dice Set A.m4a by Phorgador -- https://freesound.org/s/678196/ -- License: Attribution 4.0
