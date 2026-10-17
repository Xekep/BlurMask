"""Pinned PE-only global ordinary source geometry/pending1A0 evidence."""
import argparse,hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[(0x180168b90,0x180168bc0,'762d56f06279a92cce8b138235f31ced753c2031fe72da1ed39dccfba787f206'),
 (0x1801aae70,0x1801ac110,'7c297a0a0396465bc668d343423b7b9557d3194f62e7970c593e89d68ebf0345'),
 (0x1801a96d0,0x1801a97a0,'fdebcc613a124bb1a30177cf519d4cdf754c6e11770d497aa4763d15d0797575')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def pin(a,m,o):
    i=next(md.disasm(b.data[b.off(a):b.off(a)+15],a));assert(i.mnemonic,i.op_str)==(m,o),(hex(a),i.mnemonic,i.op_str)
pins=[(0x18000c92c,'mov','word ptr [rdi + 8], 0x10'),
 (0x1801514cf,'movzx','eax, byte ptr [rbx + 8]'),(0x1801514d3,'shr','al, 4'),
 (0x1801514d6,'and','al, 1'),(0x1801514dc,'mov','byte ptr [rbp - 0x48], al'),
 (0x1801a6bc2,'movzx','eax, byte ptr [rbp + 1]'),(0x1801a6bc9,'mov','byte ptr [rsi + 0x1d9], al'),
 (0x180168ba4,'mov','eax, r8d'),(0x180168ba7,'lea','rcx, [rax + rax*4]'),
 (0x180168bab,'mov','rax, qword ptr [r9 + 0x78]'),(0x180168baf,'lea','rax, [rax + rcx*8]'),
 (0x1801abcde,'mov','r8d, dword ptr [rax + 0x10]'),(0x1801abd11,'mov','r9, qword ptr [rax + 0x40]'),
 (0x1801abd15,'call','r9'),(0x1801a09fa,'mov','dword ptr [rsi + 0x1f0], 0'),
 (0x1801a182e,'mov','dword ptr [rsi + 0x1f0], 0')]
for a,m,o in pins:pin(a,m,o)
assert b.qwords(0x1802df340+0x40,1)[0]==0x180168b90
assert b.qwords(0x1802de610+0x1a0,1)[0]==0x180152da0
assert b.qwords(0x1802de610+0x1a8,1)[0]==0x180152dc0
assert b.qwords(0x1802e1ff0+0x130,1)[0]==0x1801aae70
body=f['1801aae70'];assert body.index('uVar24 + 0x18')<body.index('iVar6 = param_3[2]')
assert "sVar27 == -7"in body and '= 0xfff9;'in body and 'uVar32 = uVar32 - 1;'in body
assert "*(short *)(*(longlong *)(param_3 + 0x10) + 2 + uVar4 * 4) != -1"in f['1801a96d0']
assert '*(undefined4 *)(param_2 + 0x208) = 0;'in f['1801a7230']
assert f['18019f850'].count('*(undefined4 *)(param_2 + 0x1f0) = 0;')>=2
assert 'FUN_18019eb10' in f['18019f850']
report=dict(regions=[dict(start=hex(a),endExclusive=hex(z),sha256=h)for a,z,h in regions],pins=[f'{a:x} {m} {o}'for a,m,o in pins],dispatches=4,
 scope='Actual fresh NEW2B3 WORD8=10h, no external property/reuse. Config1 becomes graph1D9=1. Real kind3 controls-7 have highshortFFFF: flagsC/D0 andFFF9 before endpoint/geometry. Ordinary final nonempty allocation consumes pending fallback via19EB10/19F850 clears. Trailing/unconsumed fallback refuses.',
 guide='Source218V40 indexes actualLine R8D into40-byte guide table; caller explicitly passes descriptorI32+10 despite omitted decompiler arg. Raw guide0/4, not corrected C/10 or cross normalization union.',
 liveBounds='Global geometry retains same WordTransitions mutable group bbox sidecar; prepared actual-key kind6 controls separately verify live binding. No surrogate graph/physical IDs.',
 unsupported=['general kind1 geometry/category','custom/known input metadata','unconsumed trailing fallback','higher source overlays','glyph/text publication'],vendorExecution=False)
Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f'PASS3 pinned regions/{len(pins)} instructions/4 dispatches; pending-first/guide-line/fresh1D9 proof, no vendor execution')
