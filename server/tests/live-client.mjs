import assert from 'node:assert/strict';
import http from 'node:http';
import https from 'node:https';
import { randomBytes, createHash } from 'node:crypto';

// Tiny test-only RFC6455 client permits Authorization headers without npm deps.
export async function connect(base, id, key, options = {}) {
  const { headers = {}, hostname, localAddress, onFrame, onBytes } = options;
  const nonce = randomBytes(16).toString('base64');
  return new Promise((resolve, reject) => {
    const req = (base.protocol === 'https:' ? https : http).request(new URL(`/api/boards/${id}/live`, base), {
      hostname: hostname || base.hostname, servername: base.hostname, localAddress,
      headers: { Host: base.host, Connection: 'Upgrade', Upgrade: 'websocket', 'Sec-WebSocket-Version': '13', 'Sec-WebSocket-Key': nonce, ...(key ? { Authorization: `Bearer ${key}` } : {}), ...headers }
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
        onBytes?.('sent', header.length + masked.length);
        socket.write(Buffer.concat([header, masked]));
      };
      const consume = data => {
        onBytes?.('received', data.length);
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
          if (index >= 0) waiters.splice(index, 1)[0].resolve(frame); else if (onFrame && frame.type !== 'ready') onFrame(frame); else frames.push(frame);
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
