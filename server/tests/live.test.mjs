import test from 'node:test';
import assert from 'node:assert/strict';
import http from 'node:http';
import https from 'node:https';
import { randomBytes, createHash } from 'node:crypto';

const base = new URL(process.env.PEN_TEST_URL || 'http://127.0.0.1:5079');
const request = (path, key, body) => fetch(new URL(path, base), {
  method: body ? 'POST' : 'GET', headers: { ...(key ? { Authorization: `Bearer ${key}` } : {}), ...(body ? { 'Content-Type': 'application/json' } : {}) },
  body: body ? JSON.stringify(body) : undefined
});
const create = async () => {
  const response = await request('/api/boards', null, { title: 'Live integration', background: 'white' });
  assert.equal(response.status, 201); return response.json();
};
// Tiny test-only RFC6455 client permits Authorization headers without npm deps.
async function connect(id, key, headers = {}) {
  const nonce = randomBytes(16).toString('base64');
  return new Promise((resolve, reject) => {
    const req = (base.protocol === 'https:' ? https : http).request(new URL(`/api/boards/${id}/live`, base), {
      headers: { Connection: 'Upgrade', Upgrade: 'websocket', 'Sec-WebSocket-Version': '13', 'Sec-WebSocket-Key': nonce, ...(key ? { Authorization: `Bearer ${key}` } : {}), ...headers }
    });
    req.setTimeout(5000, () => req.destroy(new Error('Upgrade timeout')));
    req.on('error', reject);
    req.on('response', response => { response.resume(); reject(new Error(`HTTP ${response.statusCode}`)); });
    req.on('upgrade', (response, socket, head) => {
      assert.equal(response.headers['sec-websocket-accept'], createHash('sha1').update(nonce + '258EAFA5-E914-47DA-95CA-C5AB0DC85B11').digest('base64'));
      let buffer = Buffer.alloc(0), ended = false;
      const frames = [], waiters = [];
      const closed = new Promise(done => socket.once('close', () => { ended = true; done(); for (const w of waiters.splice(0)) w.reject(new Error('Socket closed')); }));
      const write = (payload, opcode = 1) => {
        const bytes = Buffer.isBuffer(payload) ? payload : Buffer.from(JSON.stringify(payload));
        const size = bytes.length, ext = size < 126 ? 0 : size <= 65535 ? 2 : 8;
        const header = Buffer.alloc(2 + ext + 4);
        header[0] = 0x80 | opcode; header[1] = 0x80 | (ext ? (ext === 2 ? 126 : 127) : size);
        if (ext === 2) header.writeUInt16BE(size, 2); else if (ext === 8) header.writeBigUInt64BE(BigInt(size), 2);
        const mask = randomBytes(4); mask.copy(header, 2 + ext);
        const masked = Buffer.from(bytes); for (let i = 0; i < masked.length; i++) masked[i] ^= mask[i % 4];
        socket.write(Buffer.concat([header, masked]));
      };
      const consume = data => {
        buffer = Buffer.concat([buffer, data]);
        while (buffer.length >= 2) {
          const opcode = buffer[0] & 15, short = buffer[1] & 127;
          const header = short < 126 ? 2 : short === 126 ? 4 : 10;
          if (buffer.length < header) return;
          const length = short < 126 ? short : short === 126 ? buffer.readUInt16BE(2) : Number(buffer.readBigUInt64BE(2));
          if (buffer.length < header + length) return;
          const bytes = buffer.subarray(header, header + length); buffer = buffer.subarray(header + length);
          if (opcode === 9) { write(bytes, 10); continue; }
          if (opcode === 8) { socket.destroy(); continue; }
          if (opcode !== 1) continue;
          const frame = JSON.parse(bytes.toString()), index = waiters.findIndex(w => w.type === frame.type);
          if (index >= 0) waiters.splice(index, 1)[0].resolve(frame); else frames.push(frame);
        }
      };
      socket.on('data', consume); socket.on('error', () => {});
      if (head.length) consume(head);
      resolve({ send: write, close: () => socket.destroy(), closed, next(type, ms = 4000) {
        const index = frames.findIndex(f => f.type === type);
        if (index >= 0) return Promise.resolve(frames.splice(index, 1)[0]);
        if (ended) return Promise.reject(new Error('Socket closed'));
        return new Promise((done, fail) => {
          const w = { type, resolve: value => { clearTimeout(timer); done(value); }, reject: error => { clearTimeout(timer); fail(error); } };
          const timer = setTimeout(() => { const i = waiters.indexOf(w); if (i >= 0) waiters.splice(i, 1); fail(new Error(`Timeout waiting for ${type}`)); }, ms);
          waiters.push(w);
        });
      } });
    });
    req.end();
  });
}
const mark = (tool = 'Pen') => ({ id: crypto.randomUUID(), tool, color: '#ff0011', width: 4, text: '', fontSize: 28, points: [{ x: 20, y: 30 }] });
const waitClosed = socket => Promise.race([socket.closed, new Promise((_, fail) => { const timer = setTimeout(() => fail(new Error('Socket was not rejected')), 4000); timer.unref(); })]);

test('live collaboration: incremental ink, late join, cancel, commit, permissions and isolation', async t => {
  const access = await create(), other = await create(), path = `/api/boards/${access.board.id}`;
  const sockets = [];
  t.after(() => sockets.forEach(s => s.close()));
  const open = async key => { const s = await connect(access.board.id, key); sockets.push(s); await s.next('ready'); return s; };
  const writer = await open(access.editKey), viewer = await open(access.viewKey);
  const unrelated = await connect(other.board.id, other.viewKey); sockets.push(unrelated); await unrelated.next('ready');
  const draft = mark(), started = performance.now();
  const spoofedPeer = crypto.randomUUID();
  writer.send({ type: 'preview', peer: spoofedPeer, offset: 0, mark: draft });
  const begin = await viewer.next('preview');
  assert.ok(performance.now() - started < 1000, 'Preview waited for a finished stroke/poll interval');
  assert.notEqual(begin.peer, spoofedPeer); assert.deepEqual(begin.mark.points, draft.points);
  const append = { ...draft, points: Array.from({ length: 512 }, (_, i) => ({ x: 21 + i, y: 31 + i })) };
  writer.send({ type: 'preview', offset: 1, mark: append });
  const continued = await viewer.next('preview'); assert.equal(continued.offset, 1); assert.equal(continued.mark.points.length, 512); assert.equal(continued.peer, begin.peer);
  assert.equal((await (await request(path, access.editKey)).json()).marks.length, 0, 'Unfinished ink was persisted');
  await assert.rejects(unrelated.next('preview', 250), /Timeout/);
  const late = await open(access.viewKey), first = await late.next('preview'), second = await late.next('preview');
  assert.equal(first.offset, 0); assert.equal(second.offset, 512); assert.deepEqual([...first.mark.points, ...second.mark.points], [...draft.points, ...append.points]);
  writer.send({ type: 'clear' }); assert.equal((await viewer.next('clear')).revision, 0); await late.next('clear');
  const rectangle = { ...mark('Rectangle'), points: [{ x: 10, y: 10 }, { x: 60, y: 70 }] };
  writer.send({ type: 'preview', offset: 0, mark: rectangle }); await viewer.next('preview');
  const resized = { ...rectangle, points: [{ x: 10, y: 10 }, { x: 260, y: 270 }] };
  writer.send({ type: 'preview', offset: 0, mark: resized }); assert.deepEqual((await viewer.next('preview')).mark.points, resized.points);
  const saved = await request(path + '/changes', access.editKey, { operationId: crypto.randomUUID(), add: [resized], remove: [] });
  assert.equal(saved.status, 200); const board = await saved.json();
  assert.equal((await viewer.next('clear')).revision, board.revision); assert.equal((await viewer.next('changed')).revision, board.revision);
  assert.equal((await (await request(path, access.viewKey)).json()).marks.length, 1);
  // A late packet after the HTTP commit cannot resurrect its preview.
  writer.send({ type: 'preview', offset: 0, mark: resized }); await assert.rejects(viewer.next('preview', 250), /Timeout/);
  writer.send({ type: 'preview', offset: 0, mark: mark() }); await viewer.next('preview'); writer.close(); await viewer.next('clear');
  // Viewers receive live data, but any attempt to publish closes their channel.
  viewer.send({ type: 'preview', offset: 0, mark: mark() }); await waitClosed(viewer);
});

test('live security: authentication, packet/point limits, isolation of request capacity', async t => {
  assert.ok(['127.0.0.1', 'localhost', '[::1]'].includes(base.hostname), 'Run boundary tests on loopback only');
  const access = await create(), sockets = []; t.after(() => sockets.forEach(s => s.close()));
  for (const key of [null, 'bad', 'x'.repeat(43)]) await assert.rejects(connect(access.board.id, key), /HTTP 404/);
  await assert.rejects(connect(access.board.id, access.editKey, { Origin: 'https://evil.example' }), /HTTP 403/);
  for (const payload of [
    { type: 'preview', offset: 1, mark: mark() },
    { type: 'preview', offset: 0, mark: { ...mark(), points: [{ x: 100001, y: 0 }] } },
    { type: 'preview', offset: 0, mark: { ...mark(), points: Array.from({ length: 513 }, () => ({ x: 1, y: 2 })) } },
    Buffer.from(' '.repeat(65536)),
    { type: 'changed', revision: 99999 }
  ]) {
    const s = await connect(access.board.id, access.editKey); sockets.push(s); await s.next('ready'); s.send(payload); await waitClosed(s);
  }
  // Long-lived channels have their own limit and do not consume the eight API slots.
  for (let i = 0; i < 16; i++) { const s = await connect(access.board.id, access.viewKey); sockets.push(s); await s.next('ready'); }
  assert.equal((await request('/healthz')).status, 200);
  assert.equal((await request(`/api/boards/${access.board.id}`, access.viewKey)).status, 200);
  await assert.rejects(connect(access.board.id, access.viewKey), /HTTP 429/);
});
