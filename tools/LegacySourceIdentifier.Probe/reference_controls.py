"""Original independently authored prefix/string controls. No vendor code execution."""
import base64
import json
import random
import struct
import sys
from pathlib import Path

if len(sys.argv) != 2 or Path(sys.argv[1]).suffix.lower() != '.json':
    raise SystemExit('Usage: reference_controls.py output.json (one output argument only)')
rng = random.Random(133089)
encode = lambda value: base64.b64encode(value).decode('ascii')
prefix_cases = []
for n in range(1024):
    name = bytes(rng.randrange(256) for _ in range(n % 60))
    prefix = name[:n % 12] if n % 3 == 0 else bytes(rng.randrange(256) for _ in range(n % 9))
    if n % 5 == 0:
        name = b'zh_' + name
        prefix = b'zh_'
    if n % 7 == 0:
        name = b'foo' + name
    if n % 11 == 0:
        name = name[:n % (len(name) + 1)] + b'\0zh_'
    if n % 13 == 0:
        prefix = prefix[:n % (len(prefix) + 1)] + b'\0ignored'
    a, b = name.split(b'\0')[0], prefix.split(b'\0')[0]
    # Element-by-element original specification, independent of the C# StartsWith call.
    expected = len(a) >= len(b) and all(a[i] == b[i] for i in range(len(b)))
    prefix_cases.append(dict(name=encode(name), prefix=encode(prefix), expected=expected))

names = ['', 'ru_RU-ak-cur.lite', 'zh_CN-ak-cur', 'foozh_CN', 'ZH_CN', 'русский', 'éclair', '数学']
names += [''.join(rng.choice('abcXYZ_012рЖяé') for _ in range(n % 26)) for n in range(256)]
resource_cases = []
for n, name in enumerate(names):
    data = bytearray(99)
    struct.pack_into('<4I2HI', data, 0, 0x0101fe90, len(data) - 8, 0x247, 0x010d0000, 0x201, 0x100, 0xe17557)
    plain = name.encode() + b'\0'
    # Native implicit-length string ignores nonzero or non-UTF8 tail after its NUL.
    plain += bytes(rng.randrange(256) for _ in range(56 - len(plain)))
    data[24:80] = bytes(c ^ (0x14, 0x12, 0x19, 0x77)[i % 4] for i, c in enumerate(plain))
    struct.pack_into('<I', data, 80, 0xffffffff)
    struct.pack_into('<IH', data, 93, 0x2b, 0x103)
    resource_cases.append(dict(resource=encode(data), identifier=name))
Path(sys.argv[1]).write_text(json.dumps(dict(prefix=prefix_cases, resource=resource_cases), ensure_ascii=False), encoding='utf8')
print(f'Generated {len(prefix_cases)} prefix cases and {len(resource_cases)} source133 prefixes')
