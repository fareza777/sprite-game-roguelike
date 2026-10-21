#!/usr/bin/env bash
# Floor smoke test: for each floor, jump there, teleport to the boss, kill everything, take the portal.
cd "$(dirname "$0")/.."
R="node tools/run-cs.mjs"; P="tools/playtest"
pump() { for i in $(seq 1 "$1"); do $R $P/editorstate.cs >/dev/null 2>&1; done; }
for n in 1 2 3 4 5 6 7 8 9 10; do
  python -c "import tempfile,os;open(os.path.join(tempfile.gettempdir(),'undersun_floor.txt'),'w').write('$n')"
  $R $P/floor.cs >/dev/null 2>&1; pump 3
  $R $P/god.cs >/dev/null 2>&1
  $R $P/tpboss.cs >/dev/null 2>&1; pump 10
  echo "B$n before: $($R $P/smokestate.cs 2>&1 | tail -1 | sed 's/.*value":"//;s/"}}//')"
  $R $P/bossdown.cs >/dev/null 2>&1; pump 8
  $R $P/bossdown.cs >/dev/null 2>&1; pump 8
  echo "B$n after:  $($R $P/smokestate.cs 2>&1 | tail -1 | sed 's/.*value":"//;s/"}}//')"
done
