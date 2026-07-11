# Spa-Francorchamps Minimap

This mod is inspired by [nyconing's Nürburgring Minimap](https://www.gta5-mods.com/scripts/vans123-s-nurburgring-nordschleife-minimap), but adapted for the [Spa-Francorchamps 2025](https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon) circuit by [VSR Kevin](https://www.gta5-mods.com/users/VSR%20Kevin).

This project is distributed under the MIT License.

`spa.png` is traced from in-game telemetry (positions recorded while driving a full lap of the circuit), so the layout is accurate.

## Files

* `SpaMinimap.cs` — Main script that draws the map and position marker. It directly includes the real telemetry points used to detect whether the player is near the circuit.
* `SpaMinimap.ini` — Configuration file for the map and marker position/size on screen.
* `SpaMinimap/spa.png` — Real circuit layout generated from telemetry.
* `SpaMinimap/pin.png` — The same marker used by nyconing.

## Configuring Position and Size (`SpaMinimap.ini`)

```ini
[Map]
PosX=-80
PosY=0
Width=400
Height=400

[Pin]
Width=6
Height=6
```

* `PosX`/`PosY`: Top-left corner of the map on screen (reference resolution: 1280×720).
* `Width`/`Height`: Size of the map on screen.
* `[Pin] Width`/`Height`: Size of the position marker.
* Use a dot (`.`) as the decimal separator, not a comma.
* If the file is missing or a value is invalid, the script displays a warning and falls back to the default value for that field.
* Reload the scripts (or restart the game) after editing for the changes to take effect.

## 1. Building

You can compile it in Visual Studio (.NET Framework Class Library project with the `ScriptHookVDotNet3` NuGet package), or simply place the `.cs` files directly in GTA V's `scripts/` folder — SHVDN will compile them automatically when the game starts.

## 2. Installation

Copy the files into GTA V's `scripts/` folder:

* `SpaMinimap.cs`
* `SpaMinimap.ini`
* `SpaMinimap/`

  * `spa.png`
  * `pin.png`

Requires the [Spa-Francorchamps 2025 by VSR Kevin](https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon) map to be installed.

## Behavior

* The map **only appears when you are near the real circuit** (within 20 meters of the recorded racing line). Nothing is drawn outside the circuit area.
* If you want the detection radius to be stricter or more permissive, adjust `ON_TRACK_DISTANCE` in `SpaMinimap.cs` (game units, approximately meters).

## Notes

* If the marker appears slightly offset in a specific section of the track (for example Eau Rouge/Raidillon or Blanchimont), let me know and I'll review the alignment.
* Requires Script Hook V + Script Hook V .NET (ScriptHookVDotNet3).
