## 0.4.0

- Ninja Shower Scrub changed from giving +1 all allies within a radius to
  distributing a fixed number. Changed scaling from increasing radius to
  increasing the number given.
  - Also will not give to drones not marked as combat drones.
- Fix Jeremiah's Accident range being too short

## 0.3.2

"Don't you dare point that at me"

- Aileen's Glass Eye no longer targets temp items
- Fixes taking an Arcade Token when purchasing the last open terminal in a shop
- Discount Coffee is slightly stronger when adding temp items (Attack speed
  increase 15% for each permanent, 25% with temp. Sprint speed increase 25% for
  each permanent, 35% with temp)

## 0.3.1

"I know the pieces fit, cause I watched them fall away"

- Upload the correct DLL file, which actually contains the 0.3.0 changes
  - The previous DLL file was months old, before 1.4.0. This also fixes non
    functional printers. 1.4.0 changed how printers work, so any mod
    compatibility shims could not fix it.

## 0.3.0

"So good to see you once again!"

- Fixes compatibility with game version 1.4.0 Alloyed Collective
- Marked most items as allowed for duplication, which adds them to mechanics
  that generate temp items
- If there are only temporary stacks of Weighted Dice in an inventory, it will
  not apply negative luck. Temporary Weighted Dice do not contribute to the
  negative luck time.
  - Temp items are function as grab everything and not care. You aren't
    evaluating if you want something. It feels bad to grab this from a pile of
    items and then get punished
  - Effectively a large temp stack of Weighted Dice is at worst neutral,
    otherwise positive. Go ahead and buy a stack of them from the temp item
    vendor and tear everything up
- Temporary stacks of Weighted Dice add half as much time to the positive luck
  buff
- Old Arcade Token uses temporary stacks on use before permanent ones
- Old Arcade Token works with drone multi-shops
- Red Boinky is now temporary after evolution
  - Risk trying to keep feeding the rage, or calm him down with the final
    evolution to make him permanent
- Reduced frequency of some sounds to reduce noise fatigue
  - No longer a constant wet slapping when having high stacks of Double Fish
- Fixes Alien God Hand not working for network clients

## 0.2.1

- Fix for Alien God Hand causing errors with characters that do not have all skills
- Add Rory's Foresight to monster blacklist
  - Also useless for monsters
  
## 0.2

- Add Old Arcade Token to monster blacklist
  - Really funny though useless
- Adjust the Weighted Dice sound parameters again
  - Never going to get this right
  
## 0.1

- Update to game library 1.3.9
- Tried to normalize all sounds for 1.3.9 update
  - Something changed with 1.3.9 and everything is still too loud, hopefully mitigates it some
- Added Ship of Regret and Tim's Crucible final art
- Added Ship of Regret lore
- The Extra now only activates when in combat
  - Changed so you can kite enemies around without killing them
  - And to stop setting the shopkeeper on fire. They suffered enough of your gassy business
- Boinky no longer creates shrines if one already exists
- Boinky no longer evolves in stages with frozen time
- Added white Boinky to monster blacklist so they can no longer spawn with it
  - All other Boinky tiers were already on monster blacklist
  - Mithrix does take all Boinky levels. Pay attention!
- Renamed Boinky debuff frm Tranquility to Sedatum, to not conflate terminology with Seeker's Tranquility buff
- Adjusted Weighted Dice sounds again so they don't play for everyone
- Added descriptive README, license code GPLv3, license art CC-SA-BY-NC

## 0.0.37

- Fixed Aileen not creating a scrapper
- Fixed Ancient Recording System placeholder drop cube
- Fixed Boinky timer resetting across stages

## 0.0.36

- Reworked A Wood Hat
- Clarify Discount Coffee wording
- Reset Boinky evolution timer when losing them all

## 0.0.35

- Placeholder drop cubes for all items with final art
- Added Prismatic Coral final art
- Adding lore for Prismatic Coral and Tim's Crucible

## 0.0.34

- Weighted Dice final art
- Bottled Chaos no longer triggers any of Aileen's effects
- Jeremiah's Accident trail a little more visible
- FX for Boinky's journey to make it a bit more obvious what happens

## 0.0.33

- Adding The Extra and Alien God Hand final art
- The Extra FX now only triggers when there are nearby enemies to reduce FX fatigue
- Adjusted Boinky's evolution conditions to be more intentional. You can keep wrathful Boinky if you prefer that over serene
- Fixed Foresight triggering on printers
- Fixed Weighted Dice not rolling on first pickup

## 0.0.32

- Fix for missing content

## 0.0.31

- Double Fish final art
- Caudal Fin final art
- boINKyshiDDenSecReTs

## 0.0.30

- SFX for The Extra
- Increased The Extra timer to 8 seconds
- Weighted Dice successful roll base time down to 30 seconds
- Foresight now works with void potentials
- Foresight no longer resets timer when using on something it cannot see

## 0.0.29

- SFX for Loose TGC cards and Weighted Dice

## 0.0.28

- Adding art for Jeremiah's Accident and Loose TCG Cards

## 0.0.27

- Foresight works with chance shrines
- A Wood Hat adds 30 armor (+15 per stack), up from 10 armor (+10 per stack)

## 0.0.26

- SFX now works multiplayer
- Discount Coffee SFX
- Toy Motorcycle final art

## 0.0.25

- Ship of Regret downside is much more harsh
- Each Toy Motorcycle now adds an additional 5% bonus per stack
- Lore dump

## 0.0.24

- Fixed Prismatic Coral taking all scrap when number held is more than the cost
- Vastly increase Jeremiah's Accident damage to 10,000%
- Changed Weighted Dice behavior to match description

## 0.0.23

- Lore dump

## 0.0.22

- Adding the final planned item
- Rework all buff assignments to work in multiplayer
- Caudal Fin temporarily prevents all fall damage
  - The intended behavior is to only prevent fall damage after leaping until hitting the ground. Temp work around until that behavior works consistently in multiplayer.
- Description tweaks and clarifications

## 0.0.21

- Adding Prismatic Coral

## 0.0.20

- Added Weighted Dice
- Adjusted Ship of Regret scaling for multiple players

## 0.0.19

- Fixed Ship of Regret not appearing
- Fixed Caudal Fin not working on host
- Fixed Caudal Fin allowing fall damage on client

## 0.0.18

- Added Ship of Regret
- Added final art for Aileen's Glas Eye
- Fixed taking Arcade Token while also holding Executive Card

## 0.0.17

- Fixed Caudal Fin not working for network clients
  - Known bug: network clients suffer fall damage after taking off
- Added Ninja Shower Scrub final art

## 0.0.16

- Added Caudal Fin
- Double Jeremiah's Accident damage (800% -> 1,600%)
- Adding final art for Ancient Recording System and Discount Coffee

## 0.0.15

- Added Alien God Hand
- Fixed Jeremiah's Accident lore formatting

## 0.0.14

- Added Jeremiah's Accident

## 0.0.13

- Fixed Toy Motorcycle always active to everything. Again.
- Replaced some placeholder art with final

## 0.0.12

- Fixed Tim's Crucible not working
- Fixed armor stacks not applying
- Increased The Extra radius to 30m
- Jeff's Service Medal increases limit based on available equipment charges

## 0.0.11

- Added item tier halos for all placeholder icon art

## 0.0.10

- Added Ninja Shower Scrub

## 0.0.9

- Boinky now adds 5 seconds of invulnerability when consumed
- Recorder now stuns for 2 seconds before applying damage
- Recorder damage does not count in its own damage tally
- Adds some sound fx
- More lore
- Internal rework, fixed all found errors
- Fixed Toy Motorcycle always active to everything
- Fixed (?) Recorder targeter

## 0.0.8

- Fix recording system not doing damage
- Added Toy Motorcycle
- Allow scrapping of dead Boinkys (you monster)
- More lore

## 0.0.7

- Fix equipment not applying globally

## 0.0.6

- Added Ancient Recording Equipment
- Added lore

## 0.0.5

- Added The Extra
- Fixed tri-shops staying open after use
- More internal fixes

## 0.0.4

- Add placeholder icons
- Use Unity supported null checking

## 0.0.3

- Added Discount Coffee
- Added Old Arcade Token
- Colorize item descriptions

## 0.0.2

- Added Aileen's Glass Eye
- Boinky named properly
- Fix non-attack damage hitting multiple times
- Fix blazing enemies stronger than they should be
- Fix equipment not in drop tables

## 0.0.1

- First testing
