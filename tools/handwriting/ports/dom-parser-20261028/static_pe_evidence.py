"""Read PE section mapping, service pointers, boolean instructions and literals."""
import hashlib
import struct
import sys
from pathlib import Path
import capstone

data = Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(data).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
pe = struct.unpack_from('<I', data, 60)[0]
count = struct.unpack_from('<H', data, pe + 6)[0]
extent = struct.unpack_from('<H', data, pe + 20)[0]
optional = pe + 24
base = struct.unpack_from('<Q', data, optional + 24)[0]

def offset(rva):
    for index in range(count):
        _, start, size, raw = struct.unpack_from('<IIII', data, optional + extent + index * 40 + 8)
        if start <= rva < start + size:
            return raw + rva - start
    raise ValueError(hex(rva))

for slot in [0x88, 0xa8, 0xb0, 0xb8, 0xc0]:
    pointer = struct.unpack_from('<Q', data, offset(0x20f290 + slot))[0]
    print(f'table+{slot:02X}: {pointer:016X}')
for address, size in [(0x1dcf80, 5), (0x1dcf88, 6)]:
    start = offset(address)
    print(f'literal {base + address:016X}: {data[start:start + size]!r}')
decoder = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
for instruction in decoder.disasm(data[offset(0x8ae00):offset(0x8ae67)], base + 0x8ae00):
    print(f'{instruction.address:016X}\t{instruction.bytes.hex()}\t{instruction.mnemonic} {instruction.op_str}')
