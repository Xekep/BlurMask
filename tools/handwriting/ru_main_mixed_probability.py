"""Probability-only independent RU-bundle E9360/E69F0 reference.

No feature extraction, NN evaluator, weights, candidates or expected text.
Actual-resource use is ROOT/CI only. Synthetic controls may supply a fake table.
Special rounding uses Decimal intervals under the documented native/platform
EXP relative2^-40 plus least-subnormal allowance; no universal CRT identity.
"""
import decimal
import functools
import hashlib
import struct

_AK_SHA = "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978"
_TABLE_SHA = "f05ff672e4399147c00eb9eb48a17ea4dc15e3f71a541d0508d84bbc4d618470"
_BIAS = 4 * 1024 * 1024
_THRESHOLD = (351 << 16) - _BIAS

def _label(header, width):
    if len(header) != 16:
        raise ValueError("The actual serialized AL16-byte header is required")
    special = struct.unpack_from("<H", header, 6)[0]
    if special == 65535 or not 0 <= special < width:
        raise ValueError("Mixed AL special label is absent or outside output mapping")
    return special

@functools.lru_cache(maxsize=1)
def _exp_table():
    # Independently reconstructed mathematical table, pinned to authenticated
    # native TextRVA260A50 bytes; no C# function or platform Math.Exp reference.
    with decimal.localcontext() as context:
        context.prec = 110
        D = decimal.Decimal
        table = tuple(int(D(25) * context.exp(D(-4) + D(22) * i / D(351))) for i in range(352))
    if hashlib.sha256(struct.pack("<352I", *table)).hexdigest() != _TABLE_SHA:
        raise ValueError("Independent EXP table differs from the authenticated native bytes")
    return table

def _sigmoid(raw):
    if type(raw) is not int or not -(1 << 31) <= raw < (1 << 31):
        raise ValueError("I32 special raw dot required")
    f32 = lambda value: struct.unpack("<f", struct.pack("<f", value))[0]
    # CVTDQ2PS, MULSS, DIVSS separately rounded. Q10 only.
    exponent = f32(f32(f32(raw) * -2.0) / 1048576.0)
    if exponent > 75.0:
        return 0
    with decimal.localcontext() as context:
        context.prec = 110
        D = decimal.Decimal
        e = context.exp(D.from_float(exponent))
        allowance = D(2) ** -40
        low = max(D(0), e * (1 - allowance) - D(2) ** -1074)
        high = e * (1 + allowance) + D(2) ** -1074
        lower = D(32768) / (1 + high) + D(".5")
        upper = D(32768) / (1 + low) + D(".5")
        if int(lower) != int(upper):
            raise ValueError("Scoped native EXP interval crosses a sigmoid integer boundary")
        exact = D(32768) / (1 + e) + D(".5")
        fraction = exact - int(exact)
        if min(fraction, 1 - fraction) <= D(2) ** -24:
            raise ValueError("Sigmoid outside the production rounding-margin domain")
        return int(lower)

def _mixed(raw, special, table):
    if type(special) is not int or not 0 <= special < len(raw) or not 2 <= len(raw) <= 512:
        raise ValueError("A bounded vector and its actual AL special index are required")
    if len(table) != 352 or any(type(v) is not int or not 0 <= v < (1 << 31) for v in table):
        raise ValueError("Nonnegative I32 EXP table required")
    if any(type(v) is not int or not -(1 << 31) <= v < (1 << 31) for v in raw):
        raise ValueError("I32 raw dots required")
    ordinary = [i for i in range(len(raw)) if i != special]
    offset = max(0, max(raw[i] for i in ordinary) - _THRESHOLD)
    if offset >= (1 << 31):
        raise ValueError("Unsupported ordinary stabilization range")
    values = [0] * len(raw)
    for i in ordinary:
        adjusted = raw[i] - offset
        if adjusted < -(1 << 31) or adjusted + _BIAS >= (1 << 31):
            raise ValueError("Unsupported ordinary subtraction/lookup range")
        index = adjusted + _BIAS
        values[i] = 0 if index < 0 else table[min(351, index >> 16)]
    while sum(values) > (1 << 31) - 1:
        values = [v >> 1 for v in values]
    maximum = max(values)
    if maximum == 0:
        raise ValueError("All-zero ordinary EXP leaves native Q cache history-dependent")
    total = sum(values)
    sum_shift = max(0, total.bit_length() - 1 - 6)
    divisor = total >> sum_shift
    value_shift = 31 - maximum.bit_length()
    probability = [min(32767, (((v << value_shift) & 0xffffffff) // divisor) >>
                       ((value_shift + sum_shift - 15) & 31)) for v in values]
    probability[special] = _sigmoid(raw[special])
    return probability

def for_russian_main(resource):
    """Authenticate the SAME fresh RU-bundle config; no weight evaluation."""
    if len(resource) != 572763 or hashlib.sha256(resource).hexdigest() != _AK_SHA:
        raise ValueError("Exact Russian MAIN resource required")
    if resource[566330] != 0 or resource[99:5233] != resource[6219:11353]:
        raise ValueError("Actual unmasked MAIN and selected AL identity required")
    special = _label(resource[99:115], 180)
    for offset, width in [(195895, 256), (283697, 238), (367899, 313), (467101, 313)]:
        if (struct.unpack_from("<HH", resource, offset) != (0xfea0, 0x106) or
            resource[offset + 8] != 0x12 or struct.unpack_from("<5I", resource, offset + 11) !=
                (width, 201, 0, 0, 181) or struct.unpack_from("<H", resource, offset + 31)[0] != 10):
            raise ValueError("Actual four Q10 two-layer family shapes required")
    table = _exp_table()
    def probability(raw):
        if len(raw) != 180:
            raise ValueError("Actual180 semantic output dots required")
        return _mixed(raw, special, table)
    return probability
