"""Static, opt-in PE/resource audit. Does not load or execute any vendor DLL."""
import argparse
import hashlib
import json
import struct
from pathlib import Path

TEXT_SHA = '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
AK_SHA = '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'

def require(ok, message):
    if not ok:
        raise ValueError(message)

class PeBytes:
    """Original bounded PE32+ section mapper; no external binary-analysis module."""
    def __init__(self, data):
        self.data = data
        require(len(data) >= 64 and data[:2] == b'MZ', 'Missing DOS header')
        pe = self.number(0x3c, '<I')
        require(self.take(pe, 4) == b'PE\0\0', 'Missing PE signature')
        require(self.number(pe + 4, '<H') == 0x8664, 'Expected x64 PE')
        sections = self.number(pe + 6, '<H')
        optional_size = self.number(pe + 20, '<H')
        require(1 <= sections <= 96 and optional_size >= 112, 'Invalid PE dimensions')
        optional = pe + 24
        self.take(optional, optional_size)
        require(self.number(optional, '<H') == 0x20b, 'Expected PE32+')
        self.base = self.number(optional + 24, '<Q')
        start = optional + optional_size
        self.sections = []
        for i in range(sections):
            _, rva, size, offset = struct.unpack('<IIII', self.take(start + i * 40 + 8, 16))
            self.take(offset, size)
            self.sections.append((rva, size, offset))

    def take(self, offset, count):
        require(offset >= 0 and count >= 0 and offset <= len(self.data) - count, 'Truncated binary field')
        return self.data[offset:offset + count]

    def number(self, offset, fmt):
        return struct.unpack(fmt, self.take(offset, struct.calcsize(fmt)))[0]

    def read(self, address, count):
        rva = address - self.base
        for section_rva, size, offset in self.sections:
            delta = rva - section_rva
            if 0 <= delta <= size - count:
                return self.take(offset + delta, count)
        raise ValueError(f'VA0x{address:x} outside initialized section bytes')

    def qword(self, address):
        return struct.unpack('<Q', self.read(address, 8))[0]

def node(data, offset, kind, version):
    require(0 <= offset <= len(data) - 16, 'Truncated resource node')
    signature, codec, size, actual_kind, compression, actual_version = struct.unpack_from('<HHI IHH', data, offset)
    require((signature, codec, actual_kind, compression, actual_version) == (0xfe90, 0x101, kind, 0, version), 'Unexpected nested resource header')
    require(size >= 8 and offset + size + 8 <= len(data), 'Invalid nested resource size')
    return dict(offset=offset, kind=hex(kind), version=hex(version), total_bytes=size + 8, end=offset + size + 8)

def audit(text, acoustic):
    require(hashlib.sha256(text).hexdigest() == TEXT_SHA, 'Expected pinned MyScriptText bytes; no version guessing')
    require(hashlib.sha256(acoustic).hexdigest() == AK_SHA, 'Expected pinned Russian acoustic resource')
    pe = PeBytes(text)
    tables = [
        (0x2b3, 0x180274aa0, 0x180274b68, 0x180274af0, {0x18: 0x18000cab0}),
        (0x222, 0x1802de8a0, 0x1802de5d8, 0x1802de610, {0x60: 0x180151ce0}),
        (0x247, 0x1802e5ba0, 0x1802e5b68, 0x1802e5a20, {0x90: 0x180196490, 0x98: 0x1801d6330}),
        (0x21d, 0x1802e3c80, 0x1802e3c40, 0x1802e4180, {0x28: 0x1801c0ce0}),
        (0x240, 0x1802e7c60, 0x1802e7ab8, 0x1802e7cb0, {0xa8: 0x180206720})]
    metadata = []
    for kind, descriptor, information, vtable, slots in tables:
        require(pe.qword(descriptor) == 3 and pe.qword(descriptor + 8) & 0xffff == kind and
                pe.qword(descriptor + 16) == 0xffffffff, 'Concrete class descriptor mismatch')
        require(pe.qword(descriptor + 24) == information and pe.qword(information + 8) == vtable, 'Class metadata/vtable mismatch')
        for slot, target in slots.items():
            require(pe.qword(vtable + slot) == target, 'Vtable slot target mismatch')
        metadata.append(dict(kind=hex(kind), descriptor=hex(descriptor), vtable=hex(vtable),
                             slots={hex(slot): hex(target) for slot, target in slots.items()}))
    accessors = [
        (0x18000cab0, '4c894260b0014d85c0740449895018c3', 'container+60/controller+18 attachment'),
        (0x1801c0ce0, '4c898290010000c3', 'ordinary+190 controller assignment'),
        (0x1801d6330, '488b8240010000c3', 'profile+140 acoustic resource'),
        (0x180196490, '488b8238010000c3', 'profile+138 source graph resource')]
    for address, expected, _ in accessors:
        require(pe.read(address, len(bytes.fromhex(expected))) == bytes.fromhex(expected), 'Accessor machine bytes mismatch')
    outer = node(acoustic, 0, 0x247, 0x10d)
    require(outer['end'] == len(acoustic) and struct.unpack_from('<I', acoustic, 5233)[0] == 0x135,
            'Observed source-profile kind mismatch')
    require(acoustic[188156] == 1, 'Primary source-resource triplet disabled')
    sampler = node(acoustic, 188157, 0x297, 0x106)
    graph = node(acoustic, sampler['end'], 0x21b, 0x10a)
    cache = node(acoustic, graph['end'], 0x21d, 0x110)
    require((graph['offset'], cache['offset'], cache['end']) == (189379, 189734, 572573), 'Observed nested-resource placement mismatch')
    return dict(text_sha256=TEXT_SHA, acoustic_sha256=AK_SHA, static_only=True,
                concrete_classes=metadata,
                accessor_bytes=[dict(address=hex(a), bytes=b, role=role) for a, b, role in accessors],
                source_profile=outer, source_kind='0x135', resource_triplet=[sampler, graph, cache],
                scope='Prepared attachment only. Factory allocation, source refresh, decorators and OCR accuracy are not tested.')

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--text-dll', required=True, type=Path)
    parser.add_argument('--acoustic-res', required=True, type=Path)
    parser.add_argument('--out', required=True, type=Path)
    args = parser.parse_args()
    report = audit(args.text_dll.read_bytes(), args.acoustic_res.read_bytes())
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print('PASS static pinned PE:5 class bindings,4 machine-byte accessors,actual RU sampler/graph/cache resource triplet')

if __name__ == '__main__':
    main()
