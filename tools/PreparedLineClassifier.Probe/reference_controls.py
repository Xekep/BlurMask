"""Own and optional actual embedded-line model numerical controls.

Uses the independent NumPy arithmetic in tools/ink-reference.py. Input frames
are generated pair features, never a handwriting accuracy fixture.
"""
import importlib.util
import json
import struct
import sys
from pathlib import Path
import numpy as np

spec = importlib.util.spec_from_file_location('ink_reference', Path(__file__).parents[1] / 'ink-reference.py')
reference = importlib.util.module_from_spec(spec)
spec.loader.exec_module(reference)
align = reference.align


def synthetic(width, path):
    rng = np.random.default_rng(width)
    payload = bytearray()

    def matrix(rows, columns):
        padded = align(align(columns+1), 16)
        weights = np.zeros((rows, padded), dtype='<i2')
        weights[:, :columns+1] = rng.integers(-8, 9, (rows, columns+1), dtype=np.int16)
        metadata = np.zeros((align(rows), 2), dtype='<f4')
        metadata[:rows, 0] = np.float32(.004)
        metadata[:rows, 1] = weights.sum(axis=1)
        payload.extend(weights.tobytes()); payload.extend(metadata.tobytes())
        payload.extend(struct.pack('<8f', 32767., 0, 0, 0, 0, 0, 0, 0))

    matrix(100, 42)
    for _ in range(2):
        for _ in range(4): matrix(width, 100)
    for _ in range(2):
        for _ in range(4): matrix(width, width)
        for _ in range(3):
            values = np.zeros(align(width), dtype='<f4'); values[:width] = rng.uniform(-.02, .02, width)
            payload.extend(values.tobytes())
    matrix(2, 2*width)
    fields = [1, 3, 1, 42, 0, 2, 1, 100, 0, 2, 8, width, 0, 2, 0, 2, 1, 0,
              0, 0, 0, 0, 0, 1, 1, 3, 2, 0, 8, len(payload)]
    resource = bytearray(struct.pack('<4sIIHH', b'\x90\xfe\1\1', 0, 0x5f, 0, 0x106))
    resource.extend(struct.pack('<'+'I'*len(fields), *fields))
    resource.extend(b'\xff' * (-len(resource) % 32)); resource.extend(payload)
    resource.extend(struct.pack('<I', 336))
    resource.extend(np.linspace(-.3, .3, 42, dtype='<f4').tobytes())
    resource.extend(np.linspace(.5, 1.5, 42, dtype='<f4').tobytes())
    struct.pack_into('<I', resource, 4, len(resource)-8)
    path.write_bytes(resource)


def load(path):
    data = path.read_bytes()
    fields = struct.unpack_from('<30I', data, 16)
    width, payload_bytes = fields[11], fields[-1]
    if width not in [30, 50]: raise ValueError('Unsupported classifier width')
    payload = data[160:160+payload_bytes]
    pos = 0

    def matrix(rows, columns):
        nonlocal pos
        padded = align(align(columns+1), 16)
        count = rows*padded
        w = np.frombuffer(payload, '<i2', count, pos).reshape(rows, padded); pos += count*2
        m = np.frombuffer(payload, '<f4', align(rows)*2, pos).reshape(align(rows), 2); pos += align(rows)*8
        aux = np.frombuffer(payload, '<f4', 8, pos); pos += 32
        return dict(weights=w, metadata=m[:rows], aux=aux, width=columns, code=2)

    preparation = matrix(100, 42)
    incoming, recurrent = [], []
    for _ in range(2): incoming.append({g: matrix(width, 100) for g in ['g', 'i', 'f', 'o']})
    for _ in range(2):
        group = {g: matrix(width, width) for g in ['g', 'i', 'f', 'o']}
        for g in ['i', 'f', 'o']:
            group['p'+g] = np.frombuffer(payload, '<f4', width, pos); pos += align(width)*4
        recurrent.append(group)
    output = matrix(2, 2*width)
    if pos != len(payload): raise ValueError('Tensor layout did not consume payload')
    tail = 160 + payload_bytes
    if struct.unpack_from('<I', data, tail)[0] != 336: raise ValueError('Bad normalization tail')
    means = np.frombuffer(data, '<f4', 42, tail+4)
    scales = np.frombuffer(data, '<f4', 42, tail+4+168)
    return dict(input_width=100, layers=[dict(bidirectional=True,
        forward_input=incoming[0], backward_input=incoming[1],
        forward_recurrent=recurrent[0], backward_recurrent=recurrent[1])], output=output), preparation, means, scales


def main():
    output = Path(sys.argv[1]); output.parent.mkdir(parents=True, exist_ok=True)
    resources = []
    for width in [30, 50]:
        path = output.with_name(f'own-line-{width}.res'); synthetic(width, path); resources.append(path)
    resources += [Path(p) for p in sys.argv[2:]]
    records = []
    for path in resources:
        network, preparation, means, scales = load(path)
        rng = np.random.default_rng(0x2e3)
        controls = []
        for frames in [1, 2, 7, 16]:
            for normalized in [False, True]:
                features = rng.normal(0, .4, (frames, 42)).astype(np.float32)
                prepared = features if normalized else (features-means)*scales
                hidden = np.tanh(np.array([reference.affine(preparation, x) for x in prepared])).astype(np.float32)
                expected, _ = reference.infer(network, hidden)
                controls.append(dict(frames=frames, normalized=normalized, input=features.ravel().tolist(), expected=expected.ravel().tolist()))
        records.append(dict(resource=str(path.resolve()), controls=controls))
    output.write_text(json.dumps(records, separators=(',', ':')), encoding='utf8')
    print(f'Wrote {len(resources)*8} independent affine/BLSTM/normalization controls from {len(resources)} resources.')


if __name__ == '__main__': main()
