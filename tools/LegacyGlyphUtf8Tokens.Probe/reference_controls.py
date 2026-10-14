"""Original scalar/byte fixtures, independent of the C# port and private DLLs."""
import argparse
import json
import random
import struct
from pathlib import Path

PRIMARY = ['a', 'A', 'é', 'e\u0301', 'ё', 'й', '∫', '=', '\n', ' ', '\u00a0', '😀', 'Ж', 'x'*2048]
SPECIAL = {0xfffc: '\u00a0', 0xfffb: ' ', 0xfff9: '\n', 0xfff6: '\U000f0000',
           **{0xfff3-i: chr(0xf0010+i) for i in range(9)}}


def alphabet_payload():
    data = bytearray(struct.pack('<8H', *([0]*7+[len(PRIMARY)])))
    data += struct.pack('<2H', 1, 0)
    for size in [32, 64]:
        data += struct.pack('<'+'H'*size, 0, *([65535]*(size-1)))
    data += struct.pack('<2H', 0, len(PRIMARY))
    offset = 0
    for i, text in enumerate(PRIMARY):
        data += struct.pack('<II', offset, i)
        offset += len(text.encode('utf8'))
    data += struct.pack('<II', offset, 65535)
    data += ''.join(PRIMARY).encode('utf8')
    data += struct.pack('<'+'H'*len(PRIMARY), *range(len(PRIMARY)))
    data += b'\0\0\0'  # no flags, no related forms; version103 ends here
    return data.hex()


def expand(flags, symbol):
    kind = flags & 7
    count = (symbol >> 2) & 0x1fff if kind in [1, 3] else 1
    if count == 0:
        return [0, 0, False]
    if kind == 1:
        label = ((symbol & 3) - 7) & 65535
    elif kind == 3:
        if symbol & 3 == 1:
            return None
        label = {0: 8, 2: 9, 3: 10}[symbol & 3]
    elif kind in [0, 2, 4]:
        label = symbol
    else:
        label = 65535
    return [count, label, kind == 4]


def emit(tokens, alternate):
    out = bytearray()
    for label, alt in tokens:
        if alt and alternate is not None:
            out += bytes.fromhex(alternate[label])
        elif label < len(PRIMARY):
            out += PRIMARY[label].encode('utf8')
        else:
            out += SPECIAL.get(label, '\ufffd').encode('utf8')
    return out.hex()


def main():
    p = argparse.ArgumentParser()
    p.add_argument('output', type=Path)
    args = p.parse_args()
    rng = random.Random(139_274)
    descriptors = []
    for kind in range(8):
        for count in [0, 1, 2, 5, 6, 13, 8191]:
            for subtype in range(4):
                symbol = (count << 2) | subtype
                flags = (rng.randrange(8192) << 3) | kind
                descriptors.append([flags, symbol, expand(flags, symbol)])
    for i in range(512):
        flags, symbol = rng.randrange(65536), rng.randrange(65536)
        descriptors.append([flags, symbol, expand(flags, symbol)])
    byte_cases = []
    interesting = list(range(14)) + list(SPECIAL) + [14, 65535, 0xfffa, 0xfff4, 0xfff5, 65500]
    for label in interesting:
        for flag in [False, True]:
            tokens = [[label, flag]]
            byte_cases.append({'tokens': tokens, 'alternate': None, 'expected': emit(tokens, None)})
    for _ in range(256):
        tokens = [[rng.choice(interesting[:-6]+[65535]), bool(rng.randrange(2))] for _ in range(rng.randrange(1, 15))]
        byte_cases.append({'tokens': tokens, 'alternate': None, 'expected': emit(tokens, None)})
    alt = [s.encode('utf8').hex() for s in ['Ж', '', 'γ', 'e\u0301', 'Й', '😀']]
    alt += ['00ff80']  # raw stage preserves bytes; final native normalization is separate
    for i in range(128):
        tokens = [[rng.randrange(len(alt)), bool(rng.randrange(2))] for _ in range(1+i%8)]
        byte_cases.append({'tokens': tokens, 'alternate': alt, 'expected': emit(tokens, alt)})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps({'schema': 1, 'alphabet': alphabet_payload(),
                                        'descriptors': descriptors, 'byteCases': byte_cases},
                                       separators=(',', ':'), ensure_ascii=True)+'\n').encode('ascii'))
    print(f'Wrote {len(descriptors)} descriptor and {len(byte_cases)} exact-byte fixtures.')


if __name__ == '__main__':
    main()
