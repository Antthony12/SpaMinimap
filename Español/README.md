# Spa-Francorchamps Minimap

Este es un mod, inspirado en el de [nyconing para Nürburgring](https://www.gta5-mods.com/scripts/vans123-s-nurburgring-nordschleife-minimap), pero adaptado al circuito de [Spa-Francorchamps 2025](https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon) de [VSR Kevin](https://www.gta5-mods.com/users/VSR%20Kevin).

Este proyecto se distribuye bajo la licencia MIT.

`spa.png` está trazada a partir de telemetría del juego (posiciones grabadas dando una vuelta al circuito), así que el trazado es exacto.

## Archivos

- `SpaMinimap.cs` — Script principal, dibuja el mapa y el pin. Incluye directamente dentro del código los puntos de telemetría real usados para detectar si el jugador está cerca del circuito.
- `SpaMinimap.ini` — Archivo de configuración de la posición y tamaño del mapa/pin en pantalla.
- `SpaMinimap/spa.png` — Trazado real del circuito, generado desde telemetría.
- `SpaMinimap/pin.png` — El mismo marcador que nyconing.

## Configurar posición y tamaño (`SpaMinimap.ini`)

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

- `PosX`/`PosY`: Esquina superior izquierda del mapa en pantalla (resolución de referencia 1280x720).
- `Width`/`Height`: Tamaño del mapa en pantalla.
- `[Pin] Width`/`Height`: Tamaño del punto que marca tu posición.
- Usa punto (`.`) como separador decimal, no coma.
- Si el archivo falta o algún valor no es válido, el script avisa por pantalla y usa el valor por defecto para ese campo (no rompe el resto).
- Tras editarlo, recarga los scripts (o reinicia el juego) para que se aplique.

## 1. Compilar

Puedes compilarlo en Visual Studio (proyecto Class Library .NET Framework +
paquete NuGet `ScriptHookVDotNet3`), o simplemente dejar los `.cs` sueltos en
la carpeta `scripts/` de GTA V — SHVDN los compila solo al iniciar el juego.

## 2. Instalar

Copia los archivos a la carpeta `scripts/` de GTA V:
- `SpaMinimap.cs`
- `SpaMinimap.ini`
- `SpaMinimap/`
  - `spa.png`
  - `pin.png`

Requiere tener instalado el mapa [Spa-Francorchamps 2025 de VSR Kevin](https://www.gta5-mods.com/maps/spa-francorchamps-2025-singleplayer-addon).

## Comportamiento

- El mapa **aparece solo cuando estás cerca del circuito real** (a menos de 20 metros del trazado). Fuera del circuito no se dibuja nada.
- Si quieres que el radio de detección sea más estricto o más permisivo, cambia `ON_TRACK_DISTANCE` en `SpaMinimap.cs` (está en unidades del juego, aproximadamente metros).

## Notas

- Si el pin se ve ligeramente desplazado en alguna zona concreta del trazado (por ejemplo Eau Rouge/Raidillon o Blanchimont), dímelo y reviso el ajuste.
- Requiere Script Hook V + Script Hook V .NET (ScriptHookVDotNet3).
