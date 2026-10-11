#!/usr/bin/env python3
"""Independent synthetic legacy-format tests; optional local resource coverage."""
import importlib.util
from pathlib import Path
import struct
import sys
import unittest

spec = importlib.util.spec_from_file_location('legacy_ink', Path(__file__).with_name('inspect-legacy-ink.py'))
legacy = importlib.util.module_from_spec(spec)
spec.loader.exec_module(legacy)


def pack(fmt, *values):
    return struct.pack('<' + fmt, *values)


def object_(kind, version, body):
    return pack('4sIIHH', legacy.MAGIC, len(body) + 8, kind, 0, version) + body


def network():
    first = b'\x01\xfe\x03\x04\x05\x06' + b'\x01\x02\x03'
    second = b'\x01\x02\x03\x04\x05\x06' + b'\x01\x04\x05'
    body = pack('BHIIIIIH', 0x12, 31, 3, 3, 0, 0, 3, 10) + first + second
    return pack('HHI', 0xfea0, 0x106, len(body)) + body


def configuration():
    # One profile, seven slots; first slot carries one optional class-pair and network.
    prefix = pack('BBHBBB', 255, 255, 100, 0, 0, 1)
    profile = pack('BHIHHHHHIIIIIII', 0, 4113, 63, 30, 16, 8, 10, 0, *([0] * 7))
    profile += pack('HHHBBHIIHHBB', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    slot = pack('BHHHBBB', 1, 1, 62, 7, 1, 1, 2) + b'\x00\x01' + network() + b'\x00'
    empty_slot = pack('BBBBfBBB', 0, 0, 0, 0, 0.0, 0, 0, 0)
    tail = pack('BIBIIH', 0, 0, 0, 0, 0, 0) + pack('IIIIII', *([0] * 6))
    tail += pack('BII Bh IHHIIIBI BII IIB', 0, 0, 0, 0, 0,
                 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    return object_(0x21d, 0x110, prefix + profile + slot + empty_slot * 6 + tail)


class LegacyFormatChecks(unittest.TestCase):
    def test_calibration_float_layout_and_alignment(self):
        prefix = pack('BIBIIH', 0, 0, 0, 0, 0, 0) + pack('IIIIII', *([0] * 6))
        prefix += pack('BII Bh IHHIIIBI B I HH', 0, 0, 0, 0, 0,
                       0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0x100)
        # Start the tail at a nonzero absolute offset to exercise native alignment.
        data = b'abc' + prefix
        padding = bytes([255]) * (-len(data) % 4)
        data += padding + pack('ffffffff', -2, .5, 3, .25, -4, 1, 5, 2)
        data += pack('IIIB', 0, 0, 0, 0)
        tail = legacy.global_tail(legacy.Reader(data, 3), 2)
        table = tail['field_118_table']
        self.assertEqual(tail['end'], len(data))
        self.assertEqual((table['classes'], table['models']), (2, 2))
        self.assertEqual(table['coefficients'], [-2, .5, 3, .25, -4, 1, 5, 2])
        self.assertEqual(table['layout'], 'model-major, then class')
        if padding:
            bad = bytearray(data); bad[3 + len(prefix)] = 0
            with self.assertRaises(ValueError): legacy.global_tail(legacy.Reader(bad, 3), 2)
        bad = bytearray(data); start = 3 + len(prefix) + len(padding)
        bad[start:start + 4] = pack('f', float('nan'))
        with self.assertRaises(ValueError): legacy.global_tail(legacy.Reader(bad, 3), 2)

    def test_network_signed_storage_and_exact_coverage(self):
        data = network()
        record = legacy.network(legacy.Reader(data))
        self.assertEqual((record['inputs'], record['hidden'], record['outputs']), (3, 3, 3))
        self.assertEqual(record['end'], len(data))
        self.assertEqual(data[record['matrices'][0]['offset'] + 1], 254)

    def test_configuration_slots_pair_table_and_coverage(self):
        data = configuration()
        result = legacy.inspect(data)
        config = result['configurations'][0]
        self.assertEqual(config['remaining_bytes'], 0)
        self.assertFalse(result['recognition_ready'])
        slot = config['profiles'][0]['submodels'][0]
        self.assertEqual(slot['pairs'], dict(first=[62], second=[7]))
        self.assertIsNotNone(slot['networks'][1])
        self.assertIsNone(slot['networks'][0])
        self.assertEqual(len(config['profiles'][0]['submodels']), 7)

    def test_truncation_and_unsupported_profiles_fail_closed(self):
        data = network()
        for stop in [0, 7, 32, len(data) - 1]:
            with self.assertRaises(ValueError):
                legacy.network(legacy.Reader(data[:stop]))
        for offset in [0, 2, 8, 19, 23, 31, 39]:
            changed = bytearray(data)
            changed[offset] ^= 255
            with self.assertRaises(ValueError):
                legacy.network(legacy.Reader(changed))
        data = configuration()
        with self.assertRaises(ValueError):
            legacy.inspect(data + b'\x00')
        changed = bytearray(data)
        changed[14:16] = pack('H', 0x111)
        with self.assertRaises(ValueError):
            legacy.inspect(changed)


if __name__ == '__main__':
    if len(sys.argv) == 2 and not sys.argv[1].startswith('-'):
        result = legacy.inspect(Path(sys.argv.pop()).read_bytes())
        configs = result['configurations']
        models = [n for c in configs for n in [c['initial_network']] if n]
        models += [n for c in configs for p in c['profiles'] for s in p['submodels'] for n in s['networks'] if n]
        assert all(c['remaining_bytes'] == 0 for c in configs)
        print(f'PASS local resource: {len(configs)} complete configurations, {len(models)} exact networks; recognition remains unavailable.')
    unittest.main()
