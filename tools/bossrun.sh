#!/usr/bin/env bash
# Boss play test: bash tools/bossrun.sh <floor> [hpFraction] [seconds]
# Jumps to the floor (play mode must be running a run), god mode, teleports to the boss, optionally drops it to
# hpFraction to see a later phase, waits, then prints the boss state and saves a screenshot to %TEMP%/oathdeep_ui.png.
cd "$(dirname "$0")/.."
R="node tools/run-cs.mjs"; P="tools/playtest"
pump() { for i in $(seq 1 "$1"); do $R $P/editorstate.cs >/dev/null 2>&1; done; }
tmp() { python -c "import tempfile,os,sys;open(os.path.join(tempfile.gettempdir(),sys.argv[1]),'w').write(sys.argv[2])" "$1" "$2"; }
tmp undersun_floor.txt "$1"
$R $P/floor.cs >/dev/null 2>&1; pump 3
$R $P/god.cs >/dev/null 2>&1
$R $P/tpboss.cs >/dev/null 2>&1; pump 6
if [ -n "$2" ]; then tmp undersun_bosshp.txt "$2"; $R $P/bosshp.cs >/dev/null 2>&1; fi
pump "${3:-12}"
echo "$($R $P/bosshp.cs 2>&1 | tail -1 | sed 's/.*value":"//;s/"}}//')"
$R $P/shot.cs >/dev/null 2>&1
