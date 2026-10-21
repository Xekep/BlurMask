"""Portable bounded raw215 FEA0/107 architecture02 wire reference.

No vendor code, inference, feature extraction or activation construction.
The parser keeps the absolute original stream origin and all alignment/row bytes.
"""
from dataclasses import dataclass
from hashlib import sha256
import struct

MAX_BYTES = 16 * 1024 * 1024
MAX_WIDTH = 4096
RESOURCE_SHA = '8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
# Static inventory inside two actual13A5 frames, not recovered slot ordinals.
ACTUAL = ((8702, 177650, 0x3f, 276, 'first13A3-owned13A5'),
          (1007739, 1151090, 0x1f, 222, 'base-tail13A4-owned13A5'),
          (1151097, 1288050, 0x2f, 204, 'base-tail13A4-owned13A5'),
          (1289175, 1458130, 0x3f, 279, 'base-tail13A4-owned13A5'),
          (1459255, 1628210, 0x4f, 279, 'base-tail13A4-owned13A5'),
          (1629111, 1772466, 0x5f, 223, 'base-tail13A4-owned13A5'))


@dataclass(frozen=True)
class Matrix:
    columns: int
    rows: int
    stride: int
    element_bytes: int
    start: int
    alignment: bytes
    weights: bytes
    scales: bytes


@dataclass(frozen=True)
class Raw215:
    origin: int
    kind: int
    inputs: int
    hidden_stored: int
    outputs_stored: int
    quantizer: int
    matrices: tuple


def _require(value, reason):
    if not value:
        raise ValueError(reason)


def parse_envelope(blob, original_origin):
    """Read an owned envelope with its real absolute resource offset.

    Architecture12, other versions/quantizers and extra middle layers are
    explicitly unsupported here. These refusals are narrower than native.
    Alignment and weight-padding bytes are retained, not normalized away.
    """
    _require(isinstance(original_origin, int) and 0 <= original_origin <= 2**31-1,
             'Invalid absolute stream origin')
    _require(isinstance(blob, (bytes, bytearray, memoryview)), 'Byte envelope required')
    if isinstance(blob, memoryview):
        _require(blob.ndim == 1 and blob.itemsize == 1 and blob.contiguous,
                 'Contiguous one-dimensional byte envelope required')
    _require(33 <= len(blob) <= MAX_BYTES,
             'Invalid bounded envelope length')
    data = bytes(blob)
    magic, version, length = struct.unpack_from('<HHI', data)
    _require((magic, version) == (0xfea0, 0x107) and length == len(data)-8,
             'Complete FEA0/version107 envelope required')
    flags, kind, inputs, hidden, extra1, extra2, outputs, quantizer = struct.unpack_from('<BHIIIIIH', data, 8)
    _require(flags == 2 and extra1 == extra2 == 0 and quantizer == 10,
             'Only architecture02/no-middle-layer/quantizer10 is supported')
    _require(all(2 <= x <= MAX_WIDTH for x in (inputs, hidden, outputs)), 'Width bound exceeded')
    # Preflight both complete matrix extents before extracting any weight bytes.
    pos = 33
    layout = []
    for columns, rows, element_bytes in ((inputs, hidden-1, 2), (hidden, outputs-1, 1)):
        aligned = ((original_origin + pos + 15) & ~15) - original_origin
        stride = (columns + 15) & ~15
        count = stride * rows * element_bytes
        end_weights = aligned + count
        end = end_weights + (rows+1 if element_bytes == 1 else 0)
        _require(0 <= pos <= aligned <= end_weights <= end <= len(data), 'Truncated aligned matrix')
        layout.append((columns, rows, stride, element_bytes, pos, aligned, end_weights, end))
        pos = end
    _require(pos == len(data), 'Unconsumed matrix data or wrong original origin')
    matrices = []
    for columns, rows, stride, element_bytes, before, aligned, end_weights, end in layout:
        scales = data[end_weights:end]
        if element_bytes == 1:
            _require(scales[0] == 1 and all(scales), 'Invalid leading or zero row scale')
        matrices.append(Matrix(columns, rows, stride, element_bytes, original_origin+aligned,
                               data[before:aligned], data[aligned:end_weights], scales))
    return Raw215(original_origin, kind, inputs, hidden, outputs, quantizer, tuple(matrices))


def write_envelope(model):
    """Rebuild the envelope from typed header/matrix fields and retained padding."""
    _require(isinstance(model, Raw215) and len(model.matrices) == 2, 'Raw215 model required')
    body = bytearray(struct.pack('<BHIIIIIH', 2, model.kind, model.inputs, model.hidden_stored,
                                 0, 0, model.outputs_stored, model.quantizer))
    for matrix, (columns, rows, element_bytes) in zip(model.matrices,
            ((model.inputs, model.hidden_stored-1, 2), (model.hidden_stored, model.outputs_stored-1, 1))):
        before = model.origin + 8 + len(body)
        _require((matrix.columns, matrix.rows, matrix.stride, matrix.element_bytes) ==
                 (columns, rows, (columns+15)&~15, element_bytes), 'Matrix identity mismatch')
        _require(matrix.start == (before+15)&~15 and len(matrix.alignment) == matrix.start-before,
                 'Absolute alignment mismatch')
        _require(len(matrix.weights) == matrix.stride*rows*element_bytes, 'Weight extent mismatch')
        _require((element_bytes == 2 and not matrix.scales) or
                 (element_bytes == 1 and len(matrix.scales) == rows+1 and matrix.scales[0] == 1 and all(matrix.scales)),
                 'Scale extent mismatch')
        body.extend(matrix.alignment)
        body.extend(matrix.weights)
        body.extend(matrix.scales)
    result = struct.pack('<HHI', 0xfea0, 0x107, len(body)) + body
    _require(parse_envelope(result, model.origin) == model, 'Rebuilt field mismatch')
    return result


def load_pinned_resource(data):
    """Select six envelopes from the exact inspected Math resource only."""
    _require(isinstance(data, (bytes, bytearray)) and len(data) == 3685881,
             'Actual bounded Math resource required')
    _require(sha256(data).hexdigest() == RESOURCE_SHA, 'Actual Math resource fingerprint required')
    result = []
    for start, end, kind, inputs, containing_owner in ACTUAL:
        raw = parse_envelope(data[start:end], start)
        _require((raw.kind, raw.inputs, raw.hidden_stored, raw.outputs_stored) == (kind, inputs, 201, 258),
                 'Actual model inventory changed')
        _require(write_envelope(raw) == data[start:end], 'Exact envelope roundtrip failed')
        result.append((containing_owner, raw))
    return tuple(result)


def describe(model):
    matrices = []
    for matrix in model.matrices:
        count = matrix.stride * matrix.rows
        values = struct.unpack('<'+str(count)+('h' if matrix.element_bytes == 2 else 'b'), matrix.weights)
        padding = [values[row*matrix.stride+column] for row in range(matrix.rows)
                   for column in range(matrix.columns, matrix.stride)]
        matrices.append(dict(columns=matrix.columns, rows=matrix.rows, stride=matrix.stride,
                             elementBytes=matrix.element_bytes, start=matrix.start,
                             alignment=matrix.alignment.hex(), weightBytes=len(matrix.weights),
                             weightsSha256=sha256(matrix.weights).hexdigest(), scales=matrix.scales.hex(),
                             minimum=min(values), maximum=max(values), nonzeroPadding=sum(x != 0 for x in padding)))
    encoded = write_envelope(model)
    return dict(origin=model.origin, end=model.origin+len(encoded), kind=hex(model.kind),
                storedInputWidth=model.inputs, hiddenRows=model.hidden_stored-1,
                outputRows=model.outputs_stored-1, quantizer=model.quantizer,
                sha256=sha256(encoded).hexdigest(), matrices=matrices)


def controls(data):
    """Malformed wire controls, not recognition examples or model calibration."""
    rejected = 0
    retained_padding_controls = 0
    for _, model in load_pinned_resource(data):
        blob = write_envelope(model)
        def reject(value, origin=model.origin):
            nonlocal rejected
            try:
                parse_envelope(value, origin)
            except (ValueError, struct.error):
                rejected += 1
            else:
                raise AssertionError('Malformed reference envelope accepted')
        boundaries = {0, 1, 7, 8, 9, 10, 11, 15, 19, 23, 27, 31, 32, len(blob)-1}
        for matrix in model.matrices:
            boundaries.update((matrix.start-model.origin-1, matrix.start-model.origin,
                               matrix.start-model.origin+len(matrix.weights)-1))
        for end in sorted(boundaries):
            if 0 <= end < len(blob):
                value = bytearray(blob[:end])
                if len(value) >= 8:
                    struct.pack_into('<I', value, 4, len(value)-8)
                reject(value)
        for offset, fmt, values in ((0,'H',(0,0xfea3)), (2,'H',(0x106,0x108)),
                (4,'I',(0,len(blob)-9,len(blob)-7)), (8,'B',(0,0x12)),
                (11,'I',(0,1,4097,0xffffffff)), (15,'I',(0,1,4097)),
                (19,'I',(1,)), (23,'I',(1,)), (27,'I',(0,1,4097)), (31,'H',(0,9,11))):
            for value in values:
                changed = bytearray(blob)
                struct.pack_into('<'+fmt, changed, offset, value)
                reject(changed)
        second = model.matrices[1]
        scales = second.start-model.origin+len(second.weights)
        for at, value in ((scales,0),(scales,2),(scales+1,0),(len(blob)-1,0)):
            changed = bytearray(blob);changed[at] = value;reject(changed)
        # Native skips alignment and reads full padded rows. The raw reference
        # preserves those bytes; it does not invent a native malformed-padding rejection.
        if model.matrices[0].alignment:
            changed = bytearray(blob);changed[33] ^= 0x55
            assert write_envelope(parse_envelope(changed, model.origin)) == changed
            retained_padding_controls += 1
        first = model.matrices[0]
        if first.stride > first.columns:
            changed = bytearray(blob);at = first.start-model.origin+first.columns*2
            struct.pack_into('<h', changed, at, -32768)
            parsed = parse_envelope(changed, model.origin)
            assert write_envelope(parsed) == changed and describe(parsed)['matrices'][0]['nonzeroPadding'] == 1
            retained_padding_controls += 1
        reject(blob, model.origin+1)
        reject(blob, -1)
    return dict(rejectedMalformed=rejected, retainedOpaquePadding=retained_padding_controls,
                nativeFailureParityClaim=False, inference=False)
