const alphabet = /^[A-Za-z0-9_-]+$/;
const encode = bytes => btoa(String.fromCharCode(...bytes)).replaceAll('+', '-').replaceAll('/', '_').replace(/=+$/, '');
const decode = value => Uint8Array.from(atob(value.replaceAll('-', '+').replaceAll('_', '/')), c => c.charCodeAt(0));

export function inviteCode({id, key}) {
  const hex = id.replaceAll('-', '');
  if (!/^[a-f\d]{32}$/i.test(hex) || /^0+$/.test(hex) || key.length !== 43 || !alphabet.test(key)) throw new Error('Некорректный код доступа');
  const secret = decode(key);
  if (encode(secret) !== key) throw new Error('Некорректный код доступа');
  return encode(Uint8Array.from([...hex.match(/../g).map(b => parseInt(b, 16)), ...secret]));
}

export function parseInvite(input) {
  let code = input.trim();
  if (code.includes('://')) {
    const url = new URL(code);
    if (url.origin !== 'https://pen.kiiko.ru' && !['localhost', '127.0.0.1', '[::1]'].includes(url.hostname)) throw new Error('Приглашение должно быть с pen.kiiko.ru');
    code = url.hash.slice(1);
  } else code = code.replace(/^#/, '');
  if (code.length !== 64 || !alphabet.test(code)) throw new Error('Вставьте корректный код приглашения или ссылку');
  const bytes = decode(code), hex = Array.from(bytes.slice(0,16), b => b.toString(16).padStart(2,'0')).join('');
  const connection = {id:`${hex.slice(0,8)}-${hex.slice(8,12)}-${hex.slice(12,16)}-${hex.slice(16,20)}-${hex.slice(20)}`, key:encode(bytes.slice(16))};
  inviteCode(connection);
  return connection;
}
