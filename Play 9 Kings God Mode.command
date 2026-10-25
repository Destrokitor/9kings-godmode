#!/bin/bash
# Installs the God Mode mod (if needed) and opens 9 Kings with it enabled.
# Opening the game from Steam directly starts it without the mod.
HERE="$(cd "$(dirname "$0")" && pwd)"
/usr/bin/python3 -I "$HERE/install.py" install > /dev/null 2>&1
cd "$HOME/Library/Application Support/Steam/steamapps/common/9 Kings" || exit 1
GODMODE_9K=1 ./9Kings.app/Contents/MacOS/9Kings > /dev/null 2>&1 &
