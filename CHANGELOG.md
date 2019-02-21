# Changelog and Downloads

Prerequisites:

[.NET Framework v4.7.2 or newer](https://dotnet.microsoft.com/download/dotnet-framework-runtime) required!
[Click here](https://dotnet.microsoft.com/download/dotnet-framework-runtime) to download the latest version of [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework-runtime).

To extract .7z archives, use [7-Zip](https://www.7-zip.org/)!

## [Click here](http://dudley.gg/squirrelies/re2/latest.7z) to download the latest release!

## [Download 1.3.0.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1300-Beta-Signed-Release.7z) (2019-02-20)
* Fixed Poison gif not animating.
* Removed version checking code (incl. --Skip-Checksum).
* Added code to close the SRT if re2.exe is closed.
* Restructured some code.
* Adjusted enemy hp text from "Enemies" to "Enemy HP".
* Debug moe now shows all 4 timer values. (A)lways running, (C)utscenes, (M)enus and (P)aused. As a reminder, IGT = A - C - P. Menu timer is tracked but not used in the IGT calculation.

### [Download 1.2.4.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1240-Beta-Signed-Release.7z) (2019-02-20)
* Updated for latest patch.

### [Download 1.2.3.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1230-Beta-Signed-Release.7z) (2019-02-17)
* Fixed an issue where dual-slot items would be split and tiled if they were in odd numbered slots.

### [Download 1.2.2.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1220-Beta-Signed-Release.7z) (2019-02-17)
* Added poison status.

### [Download 1.2.1.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1210-Beta-Signed-Release.7z) (2019-02-17)
* Fixed the missing inventory icon for the Old Key.
* Fixed infinite ammo quantity text from -1 to the infinite symbol.

### [Download 1.2.0.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1200-Beta-Signed-Release.7z) (2019-02-15)
* Updated for Ghost Survivors DLC.

### [Download 1.1.8.5 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1185-Beta-Signed-Release.7z) (2019-02-14)
* Added checksum hash reported by T710MA on Speedrun.com forums.

### [Download 1.1.8.4 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1184-Beta-Signed-Release.7z) (2019-02-14)
* Fixes issue #12 Win32Exception Win32 Error 299 (ERROR_PARTIAL_COPY) and ArgumentException with scalingFactor out of range (less than or equal to 0 or greater than 4). The program now requires the scaling factor to be greater than 0 (0%) and less than or equal to 4 (400%).

### [Download 1.1.8.3 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1183-Beta-Signed-Release.7z) (2019-02-12)
* Uses new ProcessMemory 1.0.3 which added code for WoW64 detection of processes. This could help detect the issue some users are facing.

### [Download 1.1.8.2 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1182-Beta-Signed-Release.7z) (2019-02-12)
* Adjusted the code for getting the base address to filter only 64-bit modules.
* Changed how unhandled exceptions were handled. They still FailFast and write to event log but now they'll also display a MessageBox to the user so they don't have to hunt down the error message.

### [Download 1.1.8.1 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1181-Beta-Signed-Release.7z) (2019-02-12)
* Possible fix for an error some users are experiencing. ERROR_PARTIAL_COPY (299) - Only part of a ReadProcessMemory or WriteProcessMemory request was completed.

### [Download 1.1.8.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1180-Beta-Signed-Release.7z) (2019-02-12)
* Improved performance by switching from DrawImage to TextureBrush/DrawRectangle.
* Added some additional exception handling.
* Changed how unknown versions are handled. It now warns users about the unknown version but proceeds anyways.

### [Download 1.1.7.1 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1171-Beta-Signed-Release.7z) (2019-02-10)
* Fixed a bug where Always on Top would steal focus away from the new Options UI... again!

### [Download 1.1.7.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1170-Beta-Signed-Release.7z) (2019-02-10)
* Added --NoInventory option to disable the inventory display. This option also exists in the Options UI. Upon setting this setting you must restart the SRT.

### [Download 1.1.6.1 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1161-Beta-Signed-Release.7z) (2019-02-10)
* Fixed a bug where Always on Top would steal focus away from the new Options UI.

### [Download 1.1.6.0 Beta](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1160-Beta-Signed-Release.7z) (2019-02-10)
* Added options UI as a context menu. Right-click anywhere on the user interface and selection Options to open the dialog.

### [Download 1.1.5.2 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1152-Alpha-Signed-Release.7z) (2019-02-09)
* Enemy HP is now sorted by percentage. Enemies which have been damaged will show up at the top of the list to better help you see which enemies you're engaging.
* Raw IGT value is now hidden. You can show this value again by using the --Debug command-line argument. The human-readable IGT still shows by default.

### [Download 1.1.5.1 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1151-Alpha-Signed-Release.7z) (2019-02-08)
* Fixed a bug where the program would crash and not open if a --ScalingFactor was not set.

### [Download 1.1.5.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1150-Alpha-Signed-Release.7z) (2019-02-07)
* Added --Transparent which sets the background to be transparent.
* Added ScalingFactor=n which defines what the size of an inventory icon will be relative to the original size where n is a value between 0.0 and 1.0. The default value is 0.75 (75%).

Notes: Using --No-Titlebar --Always-On-Top --Transparent will allow the SRT to function LIKE an in-game overlay. It is not an overlay in technical terms but it will fit the request without using DirectX 2D drawing and additional custom code.

### [Download 1.1.4.3 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1143-Alpha-Signed-Release.7z) (2019-02-06)
* Potential fix for inventory display bugs that were happening mid to late game and after auto-save loading/continuing.
* Initial code for --No-Titlebar and --Always-On-Top support. It works but there is no transparency yet so it is not ideal yet.

### [Download 1.1.4.2 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1142-Alpha-Signed-Release.7z) (2019-02-04)
* Fixed some incorrectly mapped weapons. The attachment portion was mapped incorrectly and caused Red X error images to be displayed for Spark Shot and Chemical Flamethrower.

### [Download 1.1.4.1 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1141-Alpha-Signed-Release.7z) (2019-02-04)
* Fixed (potentially) the inventory weirdness that happens occasionally.

### [Download 1.1.3.1 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1131-Alpha-Signed-Release.7z) (2019-02-04)
* Added some sanity checking for invalid Item/Weapon IDs in the inventory display. It now shows a red X when these are encountered.
* Added some sanity checking for invalid Slot IDs. It now skips any inventory entry with a Slot ID not within 0 through 19.

### [Download 1.1.3.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1130-Alpha-Signed-Release.7z) (2019-02-04)
* Fixed a text display bug with large quantities being cut off in the inventory display.

### [Download 1.1.2.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1120-Alpha-Signed-Release.7z) (2019-02-03)
* Added resizing/scaling to the inventory icons. Set to 75% of the original size so the inventory doesn't take up so much screen space.

### [Download 1.1.1.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1110-Alpha-Signed-Release.7z) (2019-02-03)
* Fixed a KeyNotFoundException when the SRT was ran from the main menu.

### [Download 1.1.0.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1100-Alpha-Signed-Release.7z) (2019-02-03)
* Added inventory display.

### [Download 1.0.6.1 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1061-Alpha-Signed-Release.7z) (2019-02-02)
* Added --Skip-Checksum command-line option to check the file checksum validation step.

### [Download 1.0.6.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1060-Alpha-Signed-Release.7z) (2019-02-02)
* Fixed a bug where rank and score would show 0 instead of their expected value.

### [Download 1.0.5.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1050-Alpha-Signed-Release.7z) (2019-01-31)
* Renamed 'Boss' HP to Enemy HP. Now shows all enemies HP (including inactive enemies). Options planned for future release whether to just show boss HP, show all enemy HP or only show enemies that are active currently.

### [Download 1.0.4.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1040-Alpha-Signed-Release.7z) (2019-01-30)
* Increased resolution of the raw IGT to the highest possible amount available by the game engine.
* Performance improvements by not checking for arithmatic overflows in IGT calculation.
* Adjustments made to IGT in the hopes of fixing another overflow with TimeSpan.
* Adjusted where some logic for game version checking is performed.
* Cleaned up a lot of hold-over code that was commented out.
* Added Rank and Score to the UI.

### [Download 1.0.3.2 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1032-Alpha-Signed-Release.7z) (2019-01-28)
* First public alpha.
* Added requireAdministrator to app.manifest.

### [1.0.3.1 Alpha](about:blank) (2019-01-28)
* Fixed a bug where the in-game timer would thrown an exception if the SRT was started before starting a new game.

### [1.0.3.0 Alpha](about:blank) (2019-01-28)
* In-game timer added.

### [1.0.2.0 Alpha](about:blank) (2019-01-28)
* Performance improvements.

### [1.0.1.0 Alpha](about:blank) (2019-01-26)
* Player health added.
* Boss health and percentage added.

### [1.0.0.0 Alpha](about:blank) (2019-01-24)
* Initial commit.
