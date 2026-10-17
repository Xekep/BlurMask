"""Independent read-only21FE30 and first221840 publication contract audit.

No DLL loading or vendor execution. Runtime/WORD-byte controls are separate.
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_OP_REG, X86_REG_R12, X86_REG_R12D, X86_REG_R12W, X86_REG_R12B

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
f = functions()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x18021fe30, 0x18021ffc7, 'c830a2383ead1def950273f449335f6c9a1f161fe1c1d5ed86cb5a06a195c178'),
    (0x180221840, 0x180221a03, '5ac9e36d43eb5686a619a8631413bb850949cd277dafc24c6daf78ccfd172af6'),
    (0x1802188f0, 0x180218a2b, '7f3a9a401d16952302a2d935d87b15bcf5dff2dc1ac42797f214563a3b583b3c'),
]
for start, end, sha in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == sha, hex(start)

#Packet stores precede218560. The16-byte move preserves packet1C..2B
#exactly rather than interpreting allocator-provenance bytes as new scalars.
pins = [
    (0x18021fe6b, '894510'), (0x18021fe72, '894518'), (0x18021fe79, '89451c'),
    (0x18021fe81, '66894530'), (0x18021fe8a, '66894532'),
    (0x18021fe93, '884535'), (0x18021fe9b, '884536'),
    (0x18021fea2, '89453c'), (0x18021feb4, '0f114544'), (0x18021febd, '884537'),
    (0x18021fe59, '450fb65118'),           #Duplicate is callback-data LOW BYTE.
    (0x18021feaa, '4488542448'),           #Transport unchanged to218560.
    (0x18021feaf, 'c644244000'),           #Zero conversion/option stack argument.
    (0x18021fec0, '33c04889442438'),
    (0x18021fee4, 'e87786ffff'),           #218560 before the unconditional span query.
    (0x18021ff05, '4c8b4920'), (0x18021ff0c, '41ffd1'),
    (0x18021ff0f, '85c07e7a'),             #span<=0 retains chain-success, no publication.
    (0x18021ff13, '4180be6823000000'),     #Trace2368 selects packet+40 or null.
    (0x18021ff1b, '488d7740'), (0x18021ff24, '480f44f0'),
    (0x18021ff28, '4584e4745d'),           #Positive span also requires218560 success.
    (0x18021ff31, 'ba79020000'),           #Native279 publisher descriptor.
    (0x18021ff55, '4c8b5010'),
    (0x18021ff65, '8b473489442438'),       #Last boundary packet34.
    (0x18021ff6c, '8b473089442430'),       #First boundary packet30.
    (0x18021ff7d, '41ffd2'), (0x18021ff80, '4885c07405'),
    (0x18021ff8a, '4032f6'),               #Failed chain/publisher positive-span => false.
    (0x18021ff9c, '4c8b01'), (0x18021ffa2, '41ffd0'), #WORD virtual0 reset, all normal returns.
    (0x18022775d, '41b802000000'),
    (0x18022723e, '4533e4'), (0x180227330, '4533e4'), #First-phase R12 zero.
    (0x180227776, '4d39a600230000410f44c8'), #No associated2300 => TOP.AC2.
    (0x18022779a, '898fac000000'),
    (0x1802277a3, '4488642450'),           #First ordinary duplicate lowbyte0.
    (0x1802277b9, '4c896424484c89642440'), #First221840 WORD/publisher args are null.
    (0x1802277dd, 'e85ea0ffff'),
    (0x1802277ea, '4488642450'),           #Publication call retains duplicate lowbyte0.
    (0x18022784c, 'e8ef9fffff'),
    (0x1802218a2, '0f1145df'),             #Initial double sum/count zero.
    (0x1802218e0, '4d85f6'), (0x1802218e7, '480f45c1'), #Callback enabled only with publisher.
    (0x1802219b7, '85c07e1f'),
    (0x1802219c8, 'f20f5ec8660f5ac1'),     #double(sum/count) then single store.
    (0x1802219d0, 'f30f1186b0000000'),
    (0x1802219da, 'c786b000000000000000'), #Nonpositive count => TOP.B0 zero.
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address) + len(expected) // 2].hex() == expected, hex(address)

#Assert the actual conditional/control order, including the often missed
#failed-chain/zero-span case. Region hashes also cover every intervening edge.
body = f['18021fe30']
assert body.index('FUN_180218560(') < body.index('if (0 < iVar7)') < body.index('LAB_18021ff8d:')
assert "bVar11 = cVar6 != '\\0';" in body
assert "if (cVar6 != '\\0')" in body
caller = f['180221840']
assert 'if (param_9 != 0)' in caller and 'local_a0 = FUN_18021fe30;' in caller
assert 'if (0 < (int)uStack_50)' in caller
assert '*(float *)(param_3 + 0xb0) = (float)(local_58 / (double)(int)uStack_50);' in caller

md = Cs(CS_ARCH_X86, CS_MODE_64)
md.detail = True
instructions = list(md.disasm(b.data[b.off(0x18021fe30):b.off(0x18021ffc7)], 0x18021fe30))
assert instructions[-1].address == 0x18021ffc6 and instructions[-1].mnemonic == 'ret'
phase = list(md.disasm(b.data[b.off(0x180227330):b.off(0x18022784c)], 0x180227330))
writes = [ins.address for ins in phase if any(op.type == X86_OP_REG and op.access & 2 and
          op.reg in (X86_REG_R12, X86_REG_R12D, X86_REG_R12W, X86_REG_R12B) for op in ins.operands)]
assert writes == [0x180227330] #Nonvolatile R12 remains zero through both first-result calls.
report = dict(
    regions=[dict(start=hex(a), end=hex(z), sha256=h) for a, z, h in regions], exactPins=len(pins),
    order='packet stores ->218560 ->WORD.span ->positive-span successful279 publication ->WORD.reset',
    success='zero/nonpositive span preserves218560 success; positive span requires chain success and nonnull279 result',
    topAc='fresh associated2300null ->2',
    topB0='first221840 with publishernull: raw flush2188F0 double sum/count ->positive-count single mean, elsezero',
    duplicate='2271F0 first/result calls clear low callback byte;21FE30 transports that byte, not the full opaque qword',
    trace='native2368false passesnull details; bounded owner Create already enforcesTrace2368==0 transitively',
    limits=['trace/detailed publication', 'associated source', 'complete2271F0 alternatives/quote/result normalization',
            'unported glyph/protocol subtypes and nonempty genuine residual traces'], noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'PASS3 native regions/{len(pins)} instruction pins:21FE30 order, first221840 TOP.AC/B0 and low-byte duplicate; static reads only.')
