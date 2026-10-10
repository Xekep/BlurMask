import test from 'node:test';
import assert from 'node:assert/strict';
import { inviteCode, parseInvite } from '../wwwroot/invite.js';

const connection = {id:'00112233-4455-6677-8899-aabbccddeeff',key:'AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8'};
const code = 'ABEiM0RVZneImaq7zN3u_wABAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4f';

test('invitation matches the native UUID byte order and preserves the full key',()=>{
  assert.equal(inviteCode(connection),code);
  for(const input of [code,`#${code}`,`https://pen.kiiko.ru/#${code}`,`http://127.0.0.1:5079/#${code}`])
    assert.deepEqual(parseInvite(input),connection);
});
test('already issued parameter-based invitations remain valid',()=>{
  assert.deepEqual(parseInvite(`https://pen.kiiko.ru/#board=${connection.id}&key=${connection.key}`),connection);
});
test('malformed invitations and unrelated servers are rejected',()=>{
  for(const input of ['',code.slice(1),code+'!',`https://other.example/#${code}`,'A'.repeat(64),'board=x&key=y'])
    assert.throws(()=>parseInvite(input));
});
