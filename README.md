# Park It! — 2D Pixel Parking

A complete, self-contained 2D pixel-art parking game for Unity, built for **9:16 portrait phones**. Everything is procedural — sprites, floor plans, UI font and sound effects are generated at runtime, so the project has no external art or audio dependencies.

## Run it

1. Open the project with **Unity 6000.6.6f1** (or newer Unity 6).
2. Open `Assets/Scenes/Main.unity`.
3. Press **Play**. The game view is letterboxed to a 9:16 portrait frame at any window size.

## Controls

| Input | Action |
| --- | --- |
| **Steering wheel** (bottom centre) | Spin the rim or drag the hub; 900° lock-to-lock. It self-centres slowly while rolling and holds still when stopped |
| **GAS pedal** (bottom right, green) | Accelerate |
| **BRAKE pedal** (bottom right, red) | Brake, then reverse once stopped |
| **A / D or Left / Right** | Steer |
| **W / Up** | Gas (lights the GAS pedal) |
| **S / Down** | Brake / reverse (lights the BRAKE pedal) |
| **Enter / Space** | Confirm in menus, next on the win screen |
| **Esc** | Pause / resume |

Hold the pedals for full throttle, tap them for slow, precise parking speed. The car's front tires visibly turn with the steering wheel, and the wheel eases back toward centre while the car is rolling.

## The lots

1. **Bay One** — perpendicular bays, a wide aisle to learn the handling.
2. **T-Shape** — a T-junction: drive up, turn left, then park in the top row.
3. **Parallel** — reverse into a gap between two street-parked cars.
4. **Angled** — tight 45° stalls on both sides.
5. **Garage** — indoor concrete floor, pillars and a hazard-striped roller door.
6. **Reverse** — narrow aisle, cars on both sides: back into the stall.

Park fully inside the glowing bay, straight (within 8°) and almost stopped, then hold still for about half a second. The target stalls are deliberately snug — only a little wider than the car — so centring it matters. Three stars need a bump-free run under par time; two allow a small margin. Best time, stars and unlocked lots are saved with `PlayerPrefs`.

## Project layout

```
Assets/
  Scenes/Main.unity          Bootstrap scene (one GameApp object)
  Scripts/
    Art/Px.cs                Pixel canvas (rects, lines, rotated fills, output to sprite)
    Art/PixelFont.cs         Hand-authored 5x7 font, rendered as sprites
    Art/GroundPainter.cs     Paints each lot's floor, bay lines, kerbs and grime
    Art/Art.cs               Cars, props, icons, panels, joystick art
    Art/Pal.cs               Colour palette
    Car/CarController.cs     Arcade driving physics (tune constants here)
    Level/LevelDef.cs        Level data types
    Level/LevelCatalog.cs    All six lots — edit this to add or alter layouts
    Level/LevelRuntime.cs    Builds a lot, drives sorting, checks the park
    UI/PixelText.cs          Sprite-based bitmap text
    UI/UiKit.cs              Panels, buttons, joystick
    UI/Screens.cs            Title, level select, HUD, pause, win screens
    UI/Fx.cs                 Confetti
    Core/GameApp.cs          Camera, state flow, saves, audio hooks
    Core/Save.cs             PlayerPrefs progress
    Core/ViewportFitter.cs   9:16 letterbox + fixed world camera
    Audio/Sfx.cs             Synthesised engine loop and SFX
    Util/InputService.cs     Multi-touch / mouse / keyboard input
  Editor/ParkingTools.cs     "Park It" menu: open scene, reset progress
```

## Notes for extending

- **New lot:** add a method to `LevelCatalog.Get()` returning a `LevelDef`. Asphalt rectangles, painted bays, walls, parked cars, props, start pose and the target slot are all data.
- **Art:** change `Art.Car` or the palette for a new look; all art is 16 px per world unit, so the pixel scale stays consistent.
- **Handling:** `CarController` constants at the top control top speed, acceleration, coasting and turn rate.
- **Mobile build:** portrait orientation is already configured in Player Settings (1080×1920 reference). Add your Android/iOS platform in Build Profiles and build. The letterboxed 9:16 camera keeps the layout identical on tall screens.

Use **Park It → Reset Progress** from the menu bar to clear saved stars and best times while testing.
