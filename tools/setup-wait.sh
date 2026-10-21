#!/usr/bin/env bash
# Queue Build Everything in the running Unity editor and wait for its report (pumps the editor via MCP).
cd "$(dirname "$0")/.."
R="Oathdeep/Logs/setup-report.txt"
rm -f "$R"
node tools/run-cs.mjs tools/playtest/setup.cs || exit 1
[ -f "$R" ] || { echo "setup was not queued (compile error?)"; exit 1; }
for i in $(seq 1 200); do
  node tools/run-cs.mjs tools/playtest/editorstate.cs >/dev/null 2>&1
  head -1 "$R" | grep -qv queued && break
  sleep 3
done
cat "$R"
