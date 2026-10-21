#!/usr/bin/env bash
# Queue the Android build in the running editor and pump it until the build report is written.
cd "$(dirname "$0")/.."
R="Oathdeep/Builds/Android/build-report.txt"
rm -f "$R"
node tools/run-cs.mjs tools/playtest/apk.cs || exit 1
for i in $(seq 1 900); do
  node tools/run-cs.mjs tools/playtest/editorstate.cs >/dev/null 2>&1
  [ -f "$R" ] && ! grep -q "^queued" "$R" && break
  sleep 2
done
cat "$R"
ls -la Oathdeep/Builds/Android/*.apk
