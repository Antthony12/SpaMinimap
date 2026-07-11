<b>Spa-Francorchamps Minimap</b>

<b>Description</b>
A live minimap script for GTA V that tracks your position around <b>Spa-Francorchamps</b>. Inspired by nyconing's <a href="https://www.gta5-mods.com/scripts/vans123-s-nurburgring-nordschleife-minimap">Nurburgring Minimap</a>, but adapted to VSR Kevin's <a href="https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon">Spa-Francorchamps 2025</a> map. The track outline isn't a rough approximation, it's traced directly from real telemetry recorded driving a full lap of the circuit.

<b>Features</b>
- <b>Accurate Track Layout</b>: <code>spa.png</code> is generated from real recorded telemetry, so the shape matches the actual circuit.
- <b>Live Position Pin</b>: A marker tracks your car in real time as you drive.
- <b>Auto Show/Hide</b>: The map only appears when you're near the real circuit (within 60m of the recorded line), just like the original Nurburgring mod, no clutter when you're elsewhere on the map.
- <b>Fully Configurable</b>: Map and pin position/size are set through <code>SpaMinimap.ini</code>, no need to touch the code.

<b>Requirements</b>
- <a href="https://dev-c.com/gtav/scripthookv/">Script Hook V</a>
- <a href="https://www.gta5-mods.com/tools/script-hook-v-net-enhanced">Script Hook V .NET (ScriptHookVDotNet3)</a>
- <a href="https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon">Spa-Francorchamps 2025</a> map installed

<b>Installation</b>
1. Make sure you have Script Hook V and ScriptHookVDotNet3 installed.
2. Install the Spa-Francorchamps 2025 map mod.
3. Copy <code>SpaMinimap.dll</code>, <code>SpaMinimap.ini</code> and <code>SpaMinimap</code> into your GTA V <code>scripts</code> folder.
4. Launch the game.

<b>Configuration (<code>SpaMinimap.ini</code>)</b>
<pre>
[Map]
PosX=-80
PosY=0
Width=400
Height=400

[Pin]
Width=6
Height=6
</pre>
- <code>PosX</code>/<code>PosY</code>: top-left corner of the map on screen (reference resolution 1280x720).
- <code>Width</code>/<code>Height</code>: map size on screen.
- <code>[Pin] Width</code>/<code>Height</code>: size of the position marker.
- Use a dot (<code>.</code>) as the decimal separator, not a comma.
- If the file is missing or a value is invalid, the script warns on screen and falls back to the default for that field only.
- Reload scripts (or restart the game) after editing for changes to apply.

<b>Usage</b>
- Just drive, the map appears automatically once you're on the circuit and disappears when you leave it.

<b>Notes</b>
- If the pin looks slightly off in a specific part of the track (e.g. "at Eau Rouge the pin drifts outside the line"), let me know in the comments and I'll look at adjusting it.

<b>Credits</b>
- Track map by <a href="https://www.gta5-mods.com/users/VSR Kevin">VSR Kevin</a> - <a href="https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon">Spa-Francorchamps 2025</a>.
- Inspired by and uses the same position marker as nyconing's <a href="https://www.gta5-mods.com/scripts/vans123-s-nurburgring-nordschleife-minimap">Nurburgring Minimap</a>.

<b>Development</b>
Open-source, hosted on GitHub for customization, learning, or feedback:
<a href="https://github.com/Antthony12/SpaMinimap">https://github.com/Antthony12/SpaMinimap</a>

<b>License</b>
This project is licensed under the MIT License. See <a href="LICENSE.txt">LICENSE.txt</a> for details.