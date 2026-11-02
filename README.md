# LMU GT3 Overlay

A lightweight, always-on-top telemetry overlay for GT3 cars in *Le Mans Ultimate* (LMU). The application reads live simulator data and presents key race information in transparent WPF windows while you drive.

## Features

- Live telemetry, including current and best lap and sector times, sector status, and fuel level.
- A session leaderboard with driver positions, sector times, lap times, gaps, and status.
- A player best-laps view backed by XML lap-time data.
- Transparent, movable overlay windows designed to remain visible over the game.

## Requirements

- Windows
- .NET 8 Desktop Runtime
- Le Mans Ultimate
- The 64-bit shared-memory plugin DLL, `rFactor2SharedMemoryMapPlugin64.dll`, from the [rF2SharedMemoryMapPlugin GitHub repository](https://github.com/TheIronWolfModding/rF2SharedMemoryMapPlugin)

### Install the shared-memory plugin

1. Download the 64-bit `rFactor2SharedMemoryMapPlugin64.dll` from the repository's Releases page.
2. Copy the DLL into the `Plugins` folder inside your Le Mans Ultimate installation directory (create the folder if it does not exist).
3. Start Le Mans Ultimate so it can load the plugin and expose shared-memory telemetry.

The overlay cannot read live telemetry unless LMU loads this plugin.

## Build and run

Open `LmuOverlay.sln` in Visual Studio and build the `LmuOverlay` project, or build from a terminal:

```powershell
dotnet build .\LmuOverlay.sln -p:Platform=x64
```

Run the resulting `LmuOverlay.exe` while LMU is running. The application targets `net8.0-windows` and uses WPF.

## Lap-time data

`LapTimes.xml` in the repository root is a sample data file containing 100 laps from seven different LMU circuits. It was prepared from the lap-time data file in the project's Debug output directory. The application also reads and writes lap-time XML data at runtime; keep the file's XML structure compatible with the `Lap` model when editing it.

## Third-party source acknowledgement

This project uses two source files from [rF2SharedMemoryMapPlugin by TheIronWolfModding](https://github.com/TheIronWolfModding/rF2SharedMemoryMapPlugin/tree/master): `rF2Data.cs` and `MappedBuffer.cs`. They provide shared-memory data structures and mapped-buffer access used to read simulator data. The files were taken from that project and adapted for this application.

Please refer to the upstream repository for its license and attribution terms.
