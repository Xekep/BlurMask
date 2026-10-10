// Opt-in load probe. Never run automatically against production in CI.
// Tests only newly created boards; writes their IDs (never keys) for bounded cleanup.
import http from 'node:http';
import https from 'node:https';
import { randomUUID } from 'node:crypto';
import { writeFile, mkdir, access as exists } from 'node:fs/promises';
import { connect } from './live-client.mjs';

const base = new URL(process.env.PEN_TEST_URL || 'http://127.0.0.1:5094');
const boards = Number(process.env.PEN_LOAD_BOARDS || 8);
const perBoard = Number(process.env.PEN_LOAD_USERS || 2);
const writers = Number(process.env.PEN_LOAD_WRITERS || 1);
const seconds = Number(process.env.PEN_LOAD_SECONDS || 30);
const idleSeconds = Number(process.env.PEN_LOAD_IDLE_SECONDS || 5);
const hostname = process.env.PEN_LOAD_HOST;
const loopback = ['127.0.0.1', 'localhost', '[::1]'].includes(base.hostname);
if (!loopback && process.env.PEN_LOAD_ONLINE !== 'yes') throw new Error('Set PEN_LOAD_ONLINE=yes for an explicitly authorized online test.');
if (hostname && hostname !== '127.0.0.1') throw new Error('Host override is only available for local TLS proxy testing.');
if (!Number.isInteger(boards) || boards < 1 || boards > 400 || !Number.isInteger(perBoard) || perBoard < 2 || perBoard > 128 ||
    !Number.isInteger(writers) || writers < 1 || writers > perBoard || seconds < 3 || seconds > 180 || idleSeconds < 0 || idleSeconds > 60)
  throw new Error('Invalid load parameters.');
const prefix = 'Load-' + new Date().toISOString().replaceAll(':', '-') + '-' + randomUUID().slice(0, 8);
const output = process.env.PEN_LOAD_OUTPUT || '.artifacts/load-' + prefix + '.json';
const ids = [], clients = [], errors = [], previews = new Map(), offsets = new Map(), latency = [], httpLatency = [];
let bytesSent = 0, bytesReceived = 0, expected = 0, received = 0, commits = 0, notModified = 0, healthRequests = 0, healthMaxMs = 0, stopping = false;
let nextApiAt = 0;
const address = i => hostname ? `127.0.${1 + Math.floor(i / 200)}.${1 + i % 200}` : undefined;
const sleep = ms => new Promise(done => setTimeout(done, ms));
async function api(path, key, body, localAddress, etag) {
  const at = Math.max(performance.now(), nextApiAt); nextApiAt = at + 1000 / 60;
  await sleep(Math.max(0, at - performance.now()));
  const start = performance.now();
  const result = await new Promise((resolve, reject) => {
    const data = body ? Buffer.from(JSON.stringify(body)) : null;
    const request = (base.protocol === 'https:' ? https : http).request(new URL(path, base), {
      hostname: hostname || base.hostname, servername: base.hostname, localAddress,
      method: data ? 'POST' : 'GET', headers: { Host: base.host, ...(key ? { Authorization: `Bearer ${key}` } : {}),
        ...(data ? { 'Content-Type': 'application/json', 'Content-Length': data.length } : {}), ...(etag ? { 'If-None-Match': etag } : {}) }
    }, response => {
      const chunks = []; response.on('data', b => { chunks.push(b); bytesReceived += b.length; });
      response.on('end', () => resolve({ status: response.statusCode, etag: response.headers.etag, text: Buffer.concat(chunks).toString() }));
    });
    request.setTimeout(8000, () => request.destroy(new Error('API timeout'))); request.on('error', reject);
    if (data) { bytesSent += data.length; request.write(data); } request.end();
  });
  result.ms = performance.now() - start;
  if (result.status !== 304 && (result.status < 200 || result.status >= 300)) throw new Error(`HTTP ${result.status} ${path}`);
  return result;
}
async function bounded(items, fn, count = 4) {
  let next = 0;
  await Promise.all(Array.from({ length: Math.min(count, items.length) }, async () => { while (next < items.length) await fn(items[next++]); }));
}
function distribution(values) {
  const sorted = values.slice().sort((a, b) => a - b);
  const percentile = p => sorted[Math.min(sorted.length - 1, Math.floor(sorted.length * p))] || 0;
  return { count: sorted.length, p50: percentile(.5), p95: percentile(.95), p99: percentile(.99), max: sorted.at(-1) || 0 };
}
async function health() {
  if (process.env.PEN_LOAD_STOP_FILE && await exists(process.env.PEN_LOAD_STOP_FILE).then(() => true, () => false)) throw new Error('Resource monitor requested stop.');
  const response = await api('/healthz', null, null, address(399));
  healthRequests++; healthMaxMs = Math.max(healthMaxMs, response.ms);
  if (response.ms > 2000) throw new Error('Health latency exceeded 2s; stopping load.');
}
await mkdir(output.slice(0, Math.max(output.lastIndexOf('/'), output.lastIndexOf('\\'))) || '.', { recursive: true });
console.log(JSON.stringify({ phase: 'start', prefix, boards, perBoard, writers, seconds, idleSeconds, route: hostname ? 'TLS proxy on server loopback' : base.origin }));
let activeStart = 0, activeElapsed = 0;
try {
  await health();
  const rooms = [];
  for (let i = 0; i < boards; i++) {
    const result = await api('/api/boards', null, { title: prefix + '-' + i, background: 'white' }, address(i));
    const access = JSON.parse(result.text); ids.push(access.board.id); rooms.push(access);
    await writeFile(output + '.boards.json', JSON.stringify({ prefix, ids }));
    await sleep(65); // Respect the public proxy's request limiter.
  }
  for (let i = 0; i < boards; i++) {
    for (let j = 0; j < perBoard; j++) {
      const access = rooms[i], writer = j < writers, index = clients.length;
      const client = { access, index, writer, room: i, marks: [], address: address(i) };
      client.socket = await connect(base, access.board.id, writer ? access.editKey : access.viewKey, {
        hostname, localAddress: client.address,
        onBytes: (direction, length) => { if (direction === 'sent') bytesSent += length; else bytesReceived += length; },
        onFrame: frame => {
          if (frame.type !== 'preview') return;
          const key = frame.mark.id + ':' + frame.offset, record = previews.get(key);
          if (!record || record.room !== i) { errors.push('Cross-board or unexpected preview'); return; }
          const track = index + ':' + frame.mark.id;
          if ((offsets.get(track) || 0) !== frame.offset) errors.push('Out-of-order/duplicate points');
          offsets.set(track, frame.offset + frame.mark.points.length);
          record.count++; received++; latency.push(performance.now() - record.time);
        }
      });
      await client.socket.next('ready', 8000); clients.push(client);
      client.socket.closed.then(() => { if (!stopping) errors.push('Unexpected disconnect'); });
      await sleep(25); // <40 handshakes/s, below public admission limits.
    }
    if (i % 10 === 0) await health();
  }
  console.log(JSON.stringify({ phase: 'connected', connections: clients.length }));
  for (let i = 0; i < idleSeconds; i++) { await sleep(1000); await health(); }
  console.log(JSON.stringify({ phase: 'drawing', connections: clients.length }));
  const active = clients.filter(c => c.writer);
  activeStart = performance.now();
  const cycles = Math.ceil(seconds / 3);
  let commitTail = Promise.resolve();
  for (let cycle = 0; cycle < cycles; cycle++) {
    const cycleStart = performance.now();
    for (const c of active) c.mark = { id: randomUUID(), tool: 'Pen', color: '#67925A', width: 4, text: '', fontSize: 28, points: [] };
    for (let tick = 0; tick < 60; tick++) {
      for (const c of active) {
        const points = Array.from({ length: 6 }, (_, p) => ({ x: 100 + tick * 6 + p, y: 100 + c.index % 50 + Math.sin((tick * 6 + p) / 9) * 20 }));
        const offset = c.mark.points.length; c.mark.points.push(...points);
        const key = c.mark.id + ':' + offset;
        previews.set(key, { room: c.room, time: performance.now(), count: 0 }); expected += perBoard - 1;
        c.socket.send({ type: 'preview', offset, mark: { ...c.mark, points } });
      }
      if (tick % 20 === 0) await health();
      if (errors.length) throw new Error(errors[0]);
      await sleep(Math.max(1, cycleStart + (tick + 1) * 50 - performance.now()));
    }
    const finished = active.map(c => ({ c, mark: c.mark }));
    commitTail = commitTail.then(() => bounded(finished, async ({ c, mark }) => {
      const result = await api('/api/boards/' + c.access.board.id + '/changes', c.access.editKey,
        { operationId: randomUUID(), add: [mark], remove: [] }, c.address);
      httpLatency.push(result.ms); commits++; c.marks.push(mark);
    }));
    // Propagate failures while subsequent strokes continue, without unhandled rejection.
    commitTail.catch(error => { errors.push(error.message); });
  }
  activeElapsed = performance.now() - activeStart;
  await commitTail;
  await sleep(1000);
  if (received !== expected || [...previews.values()].some(p => p.count !== perBoard - 1)) throw new Error(`Preview delivery mismatch: ${received}/${expected}`);
  await bounded(rooms.map((access, i) => ({ access, i })), async ({ access, i }) => {
    const result = await api('/api/boards/' + access.board.id, access.viewKey, null, address(i));
    const board = JSON.parse(result.text), expectedMarks = clients.filter(c => c.room === i && c.writer).flatMap(c => c.marks);
    if (board.marks.length !== expectedMarks.length || expectedMarks.some(m => !board.marks.some(n => n.id === m.id && JSON.stringify(n.points) === JSON.stringify(m.points))))
      throw new Error('Persisted ink lost points or leaked another board.');
    const unchanged = await api('/api/boards/' + access.board.id, access.viewKey, null, address(i), result.etag);
    if (unchanged.status !== 304) throw new Error('Unchanged board was downloaded again'); notModified++;
  });
  await health();
} catch (error) { errors.push(error.message); process.exitCode = 1; }
finally {
  stopping = true; for (const c of clients) c.socket.close();
  const report = { prefix, route: hostname ? 'server TLS proxy, verified certificate, loopback transport' : base.origin,
    boards: ids.length, connections: clients.length, activeWriters: clients.filter(c => c.writer).length,
    activeElapsedMs: activeElapsed, previewsSent: previews.size, deliveriesExpected: expected, deliveriesReceived: received,
    latencyMs: distribution(latency), commitLatencyMs: distribution(httpLatency), commits, conditionalReads304: notModified,
    bytesSent, bytesReceived, healthRequests, healthMaxMs, errors };
  await writeFile(output, JSON.stringify(report, null, 2)); console.log(JSON.stringify(report));
}
