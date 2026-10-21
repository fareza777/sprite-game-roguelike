#!/usr/bin/env node
// Minimal Unity-MCP client (streamable HTTP) for calling tools from the shell.
// Usage: node tools/unity-mcp.mjs <tool-name> [json-args | @file.json]
//        node tools/unity-mcp.mjs --list
import { readFileSync } from 'node:fs';

const URL = process.env.UNITY_MCP_URL ?? 'http://127.0.0.1:8080/';
const HEADERS = { 'Content-Type': 'application/json', Accept: 'application/json, text/event-stream' };

async function rpc(body, sessionId) {
  const res = await fetch(URL, {
    method: 'POST',
    headers: sessionId ? { ...HEADERS, 'mcp-session-id': sessionId } : HEADERS,
    body: JSON.stringify(body),
  });
  const text = await res.text();
  const sid = res.headers.get('mcp-session-id') ?? sessionId;
  if (!text) return { sid, msg: null };
  const dataLines = text.split('\n').filter((l) => l.startsWith('data:')).map((l) => l.slice(5).trim());
  const raw = dataLines.length ? dataLines[dataLines.length - 1] : text;
  return { sid, msg: JSON.parse(raw) };
}

async function main() {
  const [tool, argText] = process.argv.slice(2);
  if (!tool) throw new Error('usage: unity-mcp.mjs <tool> [json|@file]');

  const init = await rpc({ jsonrpc: '2.0', id: 1, method: 'initialize', params: { protocolVersion: '2025-03-26', capabilities: {}, clientInfo: { name: 'oathdeep-cli', version: '1' } } });
  const sid = init.sid;
  await rpc({ jsonrpc: '2.0', method: 'notifications/initialized' }, sid);

  if (tool === '--list') {
    const { msg } = await rpc({ jsonrpc: '2.0', id: 2, method: 'tools/list' }, sid);
    for (const t of msg.result.tools) console.log(`${t.name}: ${(t.description ?? '').split('\n')[0]}`);
    return;
  }
  if (tool === '--schema') {
    const { msg } = await rpc({ jsonrpc: '2.0', id: 2, method: 'tools/list' }, sid);
    const t = msg.result.tools.find((x) => x.name === argText);
    console.log(JSON.stringify(t, null, 2));
    return;
  }

  const args = !argText ? {} : argText.startsWith('@') ? JSON.parse(readFileSync(argText.slice(1), 'utf8')) : JSON.parse(argText);
  const { msg } = await rpc({ jsonrpc: '2.0', id: 3, method: 'tools/call', params: { name: tool, arguments: args } }, sid);
  if (msg.error) { console.error(JSON.stringify(msg.error, null, 2)); process.exit(1); }
  for (const c of msg.result.content ?? []) console.log(c.type === 'text' ? c.text : `[${c.type}] ${c.mimeType ?? ''}`);
  if (msg.result.isError) process.exit(1);
}

main().catch((e) => { console.error(e.message); process.exit(1); });
