# Changelog

[.NET Framework v4.7.2 or newer](https://dotnet.microsoft.com/download/dotnet-framework-runtime) required!
[Click here](https://dotnet.microsoft.com/download/dotnet-framework-runtime) to download the latest version of [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework-runtime).

### [Download 1.1.5.0 Alpha](http://dudley.gg/squirrelies/re2/RE2REmakeSRT-1150-Alpha-Signed-Release.7z) (2019-02-07)
* Added --Transparent which sets the background to be transparent.
* Added ScalingFactor=n which defines what the size of an inventory icon will be relative to the original size where n is a value between 0.0 and 1.0. The default value is 0.75 (75%).

Notes: Using --Always-On-Top and --Transparent will allow the SRT to function LIKE an in-game overlay. It is not an overlay in technical terms but it will fit the request without using DirectX 2D drawing and additional custom code.

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
