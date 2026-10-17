"""Raw PE literal/name order and signed64 INPUT-tag binding; no vendor execution."""
import argparse
import hashlib
import json
import re
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_REG_R8, X86_REG_RIP, X86_OP_MEM, X86_OP_REG

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('output', type=Path)
args = parser.parse_args()
t, e = Binary('MyScriptText.dll'), Binary('MyScriptEngine.dll')
assert hashlib.sha256(t.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(e.data).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
md = Cs(CS_ARCH_X86, CS_MODE_64); md.detail = True
start, end = 0x18002d8c0, 0x18002de07
raw = t.data[t.off(start):t.off(end)]
latest_name, pending_registers, names = None, set(), []
rows = list(md.disasm(raw, start))
for instruction in rows:
    operands = instruction.operands
    if instruction.mnemonic == 'lea' and operands[0].reg == X86_REG_R8 and operands[1].type == X86_OP_MEM and operands[1].mem.base == X86_REG_RIP:
        latest_name = instruction.address + instruction.size + operands[1].mem.disp
    if instruction.mnemonic == 'mov' and len(operands) == 2 and operands[0].type == X86_OP_REG and operands[1].type == X86_OP_MEM and operands[1].mem.disp == 0x18:
        pending_registers.add(operands[0].reg)
    if instruction.mnemonic == 'call' and operands[0].type == X86_OP_REG and operands[0].reg in pending_registers:
        assert latest_name is not None
        value = t.data[t.off(latest_name):].split(b'\0', 1)[0].decode('ascii')
        names.append(dict(call=hex(instruction.address), address=hex(latest_name), value=value))
        pending_registers.clear()
assert len(names) == 35
source = (root / 'src/InkRecognition/LegacyLibraryTextProcessingContext.cs').read_text(encoding='utf-8')
initializer = source.split('internal static readonly string[] ImportNames = [', 1)[1].split('];', 1)[0]
assert re.findall(r'"([^"]+)"', initializer) == [n['value'] for n in names]
assert (0x18002de02, 'call', '0x18001c460') in [(i.address, i.mnemonic, i.op_str) for i in rows]
assert e.qwords(0x180227080 + 0x128, 1) == (0x1800eda40,)
assert e.qwords(0x180227080 + 0x1b8, 1) == (0x1800ef340,)
snapshot_raw = t.data[t.off(0x180050660):t.off(0x180050718)]
snapshot_rows = [(i.address, i.mnemonic, i.op_str) for i in md.disasm(snapshot_raw, 0x180050660)]
for pin in [(0x1800506fe, 'mov', 'qword ptr [rdi + 0x18], rax'),
            (0x180050702, 'cmp', 'dword ptr [rbx + 0xd8], r12d'),
            (0x180050709, 'jne', '0x180050718'),
            (0x18005070b, 'mov', 'al, 1'), (0x180050717, 'ret', '')]:
    assert pin in snapshot_rows
input_raw = e.data[e.off(0x1800ef340):e.off(0x1800ef435)]
input_rows = [(i.mnemonic, i.op_str) for i in md.disasm(input_raw, 0x1800ef340)]
assert ('call', '0x1800ea360') in input_rows and ('call', '0x1800eaaf0') in input_rows
tag_raw = e.data[e.off(0x1800eda40):e.off(0x1800eda9f)]
tag_rows = [(i.address, i.mnemonic, i.op_str) for i in md.disasm(tag_raw, 0x1800eda40)]
for pin in [(0x1800eda53, 'mov', 'rax, qword ptr [rsp + 0xa0]'),
            (0x1800eda63, 'mov', 'qword ptr [rsp + 0x40], rax'),
            (0x1800eda6d, 'mov', 'byte ptr [rsp + 0x38], 4'),
            (0x1800eda88, 'call', '0x1800ed830')]:
    assert pin in tag_rows
args.output.write_bytes((json.dumps(dict(vendorExecution=False,
    names=names, namePreludeSha256=hashlib.sha256(raw).hexdigest(),
    freshInputChildSha256=hashlib.sha256(input_raw).hexdigest(),
    emptyTextSnapshotSha256=hashlib.sha256(snapshot_raw).hexdigest(),
    signed64TagWriterSha256=hashlib.sha256(tag_raw).hexdigest(),
    pins=12, bindings=2, scope='Input-only pre-seed BCD/name owner; no SDK page or full049370'), indent=2) + '\n').encode())
print('PASS 35 native ordered name calls / input child append / signed64 INPUT-tag / empty2DB return / 12 pins / 2 bindings.')
