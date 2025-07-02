# Krod

An item pack adding 20(-ish) new items, with a focus on making decisions and filling gaps in the base game.

## What?

tl;dr Unsatisfied how Gearbox handled their first solo effort, Seekers of the Storm. It seemed like they did not understand the game they were working with. I feel that items give Risk of Rain 2 its identity, and their designs did not fit with how the game actually works. I challenged myself as a new solo developer to create a higher quantity and quality of items than a team at Gearbox.

## Project Layout

`Assets` -- Unity Project with Thunderkit

`Krod` -- C# project

`Out` -- Empty. Thunderkit asset export location as krod.assetbundle

`ProjectSettings` -- Thunderkit stuff

`Thunderstore` -- Helpers for packaging an upload to Thunderstore.io

`WWise Project` -- WWise

`WWise Project/Originals` -- Master completed sound files

`WWise Project/GeneratedSoundBanks/Windows` -- Output sound bank file as krod.sound

## Initial Setup

- Using [r2modman](https://r2modman.com/), create a profile named 'dev'.
- Create the assetbundle by executing the pipeline in the Unity project
- Create the soundbank in WWise
- Create a new directory `%APPDATA%/r2modmanplus-local/profiles/dev/BepInEx/plugins/Krod/`
- Copy `Out/krod.assetbundle` and `WWise Project/GeneratedSoundBanks/Windows/krod.sound` to `%APPDATA%/r2modmanplus-local/profiles/dev/BepInEx/plugins/Krod/`
- Build the C# project, which automatically copies the dll and language files to the r2modman dev profile
- Manually add the R2API dependencies with r2modman. Reference the [project file](Krod/Krod.csproj)
- Optional ease of use: create a shortcut to launch the exe without having to go through r2modman with target `"%PROGRAMFILES(X86)%\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2.exe" --doorstop-enabled true --doorstop-target-assembly "%APPDATA%\r2modmanPlus-local\RiskOfRain2\profiles\dev\BepInEx\core\BepInEx.Preloader.dll"`

## Full(-ish) Spoilers

## Why?

After some decades of business dev work, never having any time for myself, I burnt out hard for the last time. I've always had creative ambition and desire, and never a space for it. Several months after crashing hard, and then watching Gearbox really shit the bed with their first update, I felt dismayed. RoR2 was a connection with friends over the years and it was falling apart. Initially I started making a couple jokes for our group. And my brain couldn't keep still, keep from coming up with ideas. Finally I committed. I have some free time, what if I set out to make an item set with a higher quantity and quality than SotS, and saw it through like a real project. Since I'm trying to figure out a new path, asking two questions. Would it be any good? Would I finish it? The latter, I did finish it. The former, well, I don't hate it and surprised myself at what I came up with. Observing discussions on it, I think I accomplished my design goal and ultimately I'm on to something as a solo developer.

I felt that Gearbox really missed with their initial designs, not understanding how the game works or taking any opportunities to fill in any gaps. I'm not saying anything new with that take. Every game of RoR2 is the same when it goes on long enough, being on autopilot overpowered. The fun part is puzzling out how to get there. I focused on two design areas: filling gaps in the base item set, and introducing difficult choices to make it more interesting on how to get to the autopilot step.

## Attributions

### Credits

Rory: Concept, item designs, implementation, sound, lore
Erin: Item visual designs

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
