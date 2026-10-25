# 9 Kings — God Mode

A "God Mode" window, styled like the game, to control a run in real time, plus the game's own developer console unlocked. For **9 Kings on macOS (Steam)**.

*Leia em português: [README.pt-BR.md](README.pt-BR.md)*

## How to play
1. Run **`Play 9 Kings God Mode.command`** (double-click). Opening the game from Steam starts it normally, without the mod.
2. **Start → Single Player → New Run**. On the "Choose a King" screen, switch **God Mode** on (top-right corner). All kings and perks are unlocked for that run.
3. Pick a king and play. During the run, click **GOD** (below the gold counter).
4. Game console: the "Game console" button in the menu, or **right Command + Delete**.

God Mode also works when you press **Continue**. While it is on, **nothing in your profile is written** (XP, levels, unlocks, stats, Steam achievements); when you go back to the menu, your real profile is reloaded. It never turns on in multiplayer.

Hover a card, decree or terrain and keep the mouse still to see its effect (the game's official text).

## Language
The mod's interface is currently in **Brazilian Portuguese** only. English equivalents:

| In the game | Meaning |
|---|---|
| Modo Deus | God Mode |
| DEUS | GOD (the button during a run) |
| Recursos · Cartas · Reino · Decretos · Combate | Resources · Cards · Kingdom · Decrees · Combat |
| Ouro · Vidas · Limite da mão | Gold · Lives · Hand limit |
| Invencível · Vidas infinitas · Castelo atira sozinho | Invincible · Infinite lives · Castle shoots by itself |
| Console do jogo · Fechar · Zerar | Game console · Close · Zero |

## Tabs
| Tab | What it does |
|---|---|
| Resources | gold (+/−, +1,000, +100,000, zero), lives, hand card limit, invincible, infinite lives, castle shoots by itself, speed 0.5x–8x |
| Cards | every card per king; click to put in hand, force on the next draw, or ban/unban |
| Kingdom | 5x5 board: raise/lower level, +/−10 troops, ×2/×10 buff, clear, unlock terrain; unlock all; raise every level |
| Decrees | activate any decree; clear decrees |
| Combat | war/peace with each king; remove enemies from the battle; invincible; castle shoots |

## Compatibility
Tested on **9 Kings 0.9.8.3** (Early Access) on macOS. The game is still being updated (version 1.0 is announced for November 12, 2026), and an update can break this mod until it is adjusted. If the game does not start after an update, run `python3 install.py uninstall` to restore the original files.

## Requirements
- **9 Kings** (Steam) on **macOS**. The installer patches the Mac app's `Data` folder, so Windows and Linux are not supported.
- `python3` (included with the Xcode command line tools).

## Install
1. Download the zip from the **Releases** page and unzip it.
2. Double-click **`Play 9 Kings God Mode.command`**. It installs the mod on the first run and opens the game with it enabled.
   You can also run `python3 install.py status | install | uninstall` yourself. `uninstall` restores the original game files.

If a Steam update removes the registration, the launcher redoes it by itself.

## How it works
9 Kings uses Unity 6 (Mono). BepInEx/Doorstop does not inject into this version on macOS, so the mod is loaded by Unity's own mechanism: `install.py` copies `NineKingsGodMode.dll` and `0Harmony.dll` (Harmony 2.4.3, with ARM64 support) into `Data/Managed` and registers `Loader.Init` in `RuntimeInitializeOnLoads.json`. The mod only turns on when the `GODMODE_9K=1` environment variable exists, which the launcher sets. The original JSON files are saved in `Data/.godmode_backup/`.

- Code: `src/` — `GodMode.cs` (state/protection), `Patches.cs` (Harmony), `Actions.cs` (actions), `GodMenu.cs` (window), `Ui.cs` (game-style components), `KingSelectionToggle.cs`, `GodModeHost.cs`.
- Build: `DOTNET_ROOT=/opt/homebrew/opt/dotnet/libexec dotnet build -c Release` (needs the game installed, because the project references its Unity assemblies, which are **not** included here).
- Test build: `-p:Dev=1` also compiles `src/Dev/` (a file-based command channel, active only with `GODMODE_DEV_DIR`). Never ship that build.

## Disclaimer
This is an unofficial fan-made mod. It is **not affiliated with or endorsed by Sad Socket**. 9 Kings and its related names and assets belong to their owners; this repository contains none of the game's files. Use it at your own risk and keep a backup of your profile (`~/Library/Application Support/SadSocket/9Kings`).

## Support

This mod is and will always be **free and complete**. If you enjoy it and want to say thanks, a voluntary donation is welcome (nothing is locked behind it, and there are no perks):

- **PIX (Brazil):** `pedro.comitre@gmail.com`

## License
[MIT](LICENSE). Harmony (`lib/0Harmony.dll`) is included under its own MIT license; see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
