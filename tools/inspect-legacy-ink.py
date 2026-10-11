#!/usr/bin/env python3
"""Bounded, static inspection of user-supplied legacy fixed-vector ink resources.

No vendor code or weights are included, loaded as executable code, or downloaded.
This is a format diagnostic, not a raw-handwriting recognizer. Field names with
hex offsets identify native object fields whose semantics are not yet established.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import struct

MAGIC = b'\x90\xfe\x01\x01'
MAX_RESOURCE = 64 * 1024 * 1024


class Reader:
    def __init__(self, data, start=0, end=None):
        self.data, self.pos, self.end = data, start, len(data) if end is None else end
        if not 0 <= start <= self.end <= len(data):
            raise ValueError('Invalid reader bounds')

    def read(self, fmt, count=1):
        if count < 0 or count > 65536:
            raise ValueError('Invalid array count')
        size = struct.calcsize('<' + fmt) * count
        if size > self.end - self.pos:
            raise ValueError('Truncated field')
        values = struct.unpack_from('<' + fmt * count, self.data, self.pos)
        self.pos += size
        return values[0] if len(values) == 1 else list(values)

    def array(self, fmt, count):
        value = self.read(fmt, count)
        return [value] if count == 1 else value

    def flag(self):
        value = self.read('B')
        if value not in (0, 1):
            raise ValueError('Unsupported boolean flag')
        return value

    def array16(self):
        return self.array('H', self.read('H'))


def header(data, offset, parent_end):
    r = Reader(data, offset, parent_end)
    magic, length, kind, flags, version = r.read('4sIIHH')
    end = offset + 8 + length
    if magic != MAGIC or flags or length < 8 or end > parent_end:
        raise ValueError('Invalid resource object')
    return dict(offset=offset, end=end, kind=kind, version=version)


def network(r):
    start = r.pos
    marker, version, length = r.read('HHI')
    end = start + 8 + length
    if marker != 0xfea0 or version != 0x106 or not start + 33 <= end <= r.end:
        raise ValueError('Unsupported legacy network bounds/version')
    flags, kind, inputs, hidden, third, lookup, outputs, exponent = r.read('BHIIIIIH')
    if flags != 0x12 or third or lookup or exponent != 10:
        raise ValueError('Unsupported legacy network profile')
    if not all(1 < width <= 4096 for width in (inputs, hidden, outputs)):
        raise ValueError('Invalid legacy network dimensions')
    matrices = []
    for name, columns, rows in [('input_to_hidden', inputs, hidden), ('hidden_to_output', hidden, outputs)]:
        offset = r.pos
        weight_count = columns * (rows - 1)
        if weight_count + rows > end - offset:
            raise ValueError('Truncated legacy matrix')
        r.pos += weight_count
        scales = r.array('B', rows)
        if scales[0] != 1 or min(scales[1:]) <= 0:
            raise ValueError('Invalid row scales')
        matrices.append(dict(name=name, offset=offset, columns=columns, rows=rows - 1,
                             scales_offset=offset + weight_count, bytes=weight_count + rows,
                             sha256=hashlib.sha256(r.data[offset:r.pos]).hexdigest()))
    if r.pos != end:
        raise ValueError('Unconsumed legacy network bytes')
    return dict(offset=start, end=end, kind=kind, inputs=inputs, hidden=hidden,
                outputs=outputs, exponent=exponent, matrices=matrices)


def pairs(r):
    if not r.flag():
        return None
    count = r.read('H')
    return dict(first=r.array('H', count), second=r.array('H', count))


def submodel(r):
    start = r.pos
    mapping = pairs(r)
    flags = r.array('B', 2)
    scale_mode = r.read('B')
    scale = r.read('f') if scale_mode == 0 else None
    networks = [network(r) if r.flag() else None for _ in range(3)]
    return dict(offset=start, end=r.pos, pairs=mapping, flags=flags,
                scale_mode=scale_mode, scale=scale, networks=networks)


def global_tail(r, active_models):
    result = dict(offset=r.pos)
    result['fields_78_88'] = r.read('BIBII')
    result['field_98_groups'] = [r.array16() for _ in range(r.read('H'))]
    result['fields_a0_b4'] = r.array('I', 6)
    result['fields_b8_c0'] = r.read('BII')
    if r.flag():
        raise ValueError('Unsupported obsolete global object')
    count = r.read('h')
    if count < 0:
        raise ValueError('Invalid global class count')
    if count:
        offsets = r.array('H', count + 1)
        if offsets[0] != 0 or any(a > b for a, b in zip(offsets, offsets[1:])):
            raise ValueError('Invalid class table spans')
        labels = r.array('H', offsets[-1])
        result['field_c8_classes'] = [labels[a:b] for a, b in zip(offsets, offsets[1:])]
        result['field_e0'] = r.read('I')
    result['fields_e8_100'] = r.read('IHHIIIBI')
    if r.flag():
        raise ValueError('Unsupported global object')
    mode = r.read('I')
    result['field_108_mode'] = mode
    if mode:
        if mode not in (1, 2):
            raise ValueError('Unsupported global score mode')
        classes, storage = r.read('HH')
        if storage != 0x100:
            raise ValueError('Unsupported global score storage')
        while r.pos % 4:
            if r.read('B') != 255:
                raise ValueError('Invalid global table alignment')
        raw = r.array('I', classes * active_models * (2 if mode == 1 else 1))
        floats = [struct.unpack('<f', struct.pack('<I', value))[0] for value in raw]
        if not all(math.isfinite(value) for value in floats):
            raise ValueError('Non-finite score calibration')
        result['field_118_table'] = dict(classes=classes, models=active_models, values=raw,
             layout='model-major, then class',
             calibration='sigmoid((score - center) * slope)' if mode == 1 else 'tanh(score * slope / 2)',
             coefficients=floats)
    count = r.read('I')
    result['field_128_count'] = count
    if count:
        if count > 65536:
            raise ValueError('Global byte table too large')
        result['fields_12c_134'] = r.read('III')
        result['field_138_bytes'] = r.array('B', count)
    result['fields_120_124'] = r.read('II')
    if r.flag():
        raise ValueError('Unported trailing global object')
    result['end'] = r.pos
    return result


def configuration(data, node):
    if node['kind'] != 0x21d or node['version'] != 0x110:
        raise ValueError('Only legacy type21d/version110 is supported')
    r = Reader(data, node['offset'] + 16, node['end'])
    result = dict(**node)
    result['prefix'] = r.array('B', 2) + [r.read('H')]
    result['initial_network'] = network(r) if r.flag() else None
    if result['initial_network'] is not None:
        result['initial_metadata'] = dict(fields_48_50=r.array('f', 3), fields_54_56=r.read('BBH'),
                                          field_58=r.read('f'), fields_5c_65=r.array('B', 4))
    if r.flag():
        result['field_10_configuration'] = r.read('Hfffffffff')
    profiles = []
    for _ in range(r.read('B')):
        profile = dict(offset=r.pos)
        if r.flag():
            profile['prefix'] = r.read('ffBf')
        # The packed feature-configuration fields are integers/bit masks, not floats.
        profile['fields_14_44'] = r.read('HIHHHHHIIIIIII')
        profile['field_48_ids'] = r.array16()
        count = r.read('H')
        profile['fields_60_68'] = [r.array('H', count), r.array('H', count)]
        count = r.read('H')
        profile['fields_78_80'] = [r.array('H', count), r.array('H', count)]
        profile['fields_88_9e'] = r.read('BBHffHH')
        if r.flag():
            raise ValueError('Unsupported nested profile object')
        profile['field_a8_pairs'] = pairs(r)
        profile['submodels'] = [submodel(r) for _ in range(7)]
        profile['end'] = r.pos
        profiles.append(profile)
    result['profiles'] = profiles
    result['global_tail'] = global_tail(r, sum(bool(s['flags'][0]) for p in profiles for s in p['submodels']))
    # Never silently interpret these legacy classes as a modern CTC schema.
    result['parsed_prefix_end'] = r.pos
    result['remaining_bytes'] = r.end - r.pos
    result['remaining_sha256'] = hashlib.sha256(data[r.pos:r.end]).hexdigest()
    if r.pos != r.end:
        raise ValueError('Unconsumed legacy configuration bytes')
    return result


def inspect(data):
    if len(data) > MAX_RESOURCE:
        raise ValueError('Resource exceeds inspection limit')
    root = header(data, 0, len(data))
    if root['end'] != len(data):
        raise ValueError('Trailing resource bytes')
    configs = []
    offset = 0
    while True:
        offset = data.find(MAGIC, offset)
        if offset < 0:
            break
        try:
            node = header(data, offset, len(data))
        except ValueError:
            offset += 1
            continue
        if node['kind'] == 0x21d:
            configs.append(configuration(data, node))
        offset += 1
    if not configs:
        raise ValueError('No supported legacy configuration')
    return dict(bytes=len(data), sha256=hashlib.sha256(data).hexdigest(),
                configurations=configs, recognition_ready=False,
                missing=['multi-stroke and baseline/raster fixed-vector extraction',
                         'complete classifier ensemble and virtual-label semantics',
                         'segmentation and language decoder'])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('resource', type=Path)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    result = inspect(args.resource.read_bytes())
    output = json.dumps(result, indent=2, ensure_ascii=False, allow_nan=False)
    if args.output:
        args.output.write_text(output + '\n', encoding='utf-8')
    else:
        print(output)


if __name__ == '__main__':
    main()
