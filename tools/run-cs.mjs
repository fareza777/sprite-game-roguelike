#!/usr/bin/env node
// Runs a C# file inside the Unity Editor through the Unity-MCP `script-execute` tool.
// The file must define `public class Script { public static <any> Main() { ... } }`.
// Usage: node tools/run-cs.mjs path/to/file.cs
import { readFileSync, writeFileSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

const file = process.argv[2];
if (!file) { console.error('usage: run-cs.mjs <file.cs>'); process.exit(1); }
const payload = { csharpCode: readFileSync(file, 'utf8'), className: 'Script', methodName: 'Main' };
const dir = mkdtempSync(join(tmpdir(), 'runcs-'));
const json = join(dir, 'args.json');
writeFileSync(json, JSON.stringify(payload));
const client = fileURLToPath(new URL('./unity-mcp.mjs', import.meta.url));
const r = spawnSync(process.execPath, [client, 'script-execute', `@${json}`], { stdio: 'inherit' });
process.exit(r.status ?? 1);
