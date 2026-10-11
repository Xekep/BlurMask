"""Original independent float32 multiply, truncate and int16 wrap controls."""
import json, math, pathlib, random, struct, sys
def f(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]
def short(x):
    return ((x + 32768) & 65535) - 32768
r = random.Random(0xebb60)
cases = []
for i in range(640):
    exponent = i % 15
    values = [f(r.uniform(-128, 128)) for _ in range(r.randrange(1, 65))]
    if i % 7 == 0:
        values.extend([f(32767.99 / 2**exponent), f(-32768.99 / 2**exponent), f(65535.99 / 2**exponent)])
    cases.append(dict(exponent=exponent, values=values, expected=[short(math.trunc(f(v * 2**exponent))) for v in values]))
path = pathlib.Path(sys.argv[1]); path.parent.mkdir(parents=True, exist_ok=True)
path.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf8')
print(f'Wrote{len(cases)} own float32 quantization cases.')
