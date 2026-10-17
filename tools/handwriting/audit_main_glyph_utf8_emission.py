"""Static fresh TOP control/alternate owner and virtual50 proof. PE reads only."""
import argparse
import hashlib
import json
import re
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('output',type=Path)
a=p.parse_args()
t=Binary(); e=Binary('MyScriptEngine.dll'); tf=functions(); ef=functions('MyScriptEngine.dll')
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
md=Cs(CS_ARCH_X86,CS_MODE_64)

def code(b,start,end):
    return [(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]

def terms(f,address,*pins):
    s=f[f'{address:x}']
    for pin in pins:assert pin in s,(hex(address),pin)

#Default module registration, not a guessed interpretation of object layout.
terms(tf,0x180002530,'puVar7 = &DAT_1802df150',
      '*(undefined8 *)(*(longlong *)(param_1 + 0x18) + 0x38) = *(undefined8 *)(puVar7 + 0x20)')
assert code(t,0x180002b83,0x180002b93)==[
    ('mov','rcx, qword ptr [rbx + 0x18]'),('lea','rsi, [rbx + 8]'),
    ('mov','rax, qword ptr [r14 + 0x20]'),('mov','qword ptr [rcx + 0x38], rax')]
assert t.qwords(0x1802df150+0x20,1)[0]==0x1802df0a0
assert t.qwords(0x1802df0a0+0x80,1)[0]==0x180169740
assert code(t,0x180169740,0x180169748)==[('mov','rax, qword ptr [rdx + 0x120]'),('ret','')]
assert t.qwords(0x1802de610+0x250,1)[0]==0x1801530f0
assert code(t,0x1801530f0,0x1801530f5)==[('mov','rax, qword ptr [rdx + 0x60]'),('ret','')]
terms(tf,0x180150d00,'param_1,0x219','*(longlong *)(param_2 + 0x60) = lVar13')
terms(tf,0x1802271f0,'+ 0x250','+ 0x38)','+ 0x80)',
      '*(undefined8 *)(param_3 + 0x20) = *(undefined8 *)(lVar11 + 0x170)',
      '*(undefined8 *)(param_3 + 0x30) = *(undefined8 *)(lVar11 + 0x180)')

#Fresh source secondary including its two alternate vectors. Raw points and
#selected first glyph records are different from source text insertion.
terms(tf,0x18016a130,'param_2[0x2e] = 0','param_2[0x2f] = 0','param_2[0x30] = 0','param_2[0x31] = 0')
terms(tf,0x180163970,'FUN_18016a130()','*(undefined8 **)(param_2 + 0x120) = puVar9')
direct_callers = sorted(k for k,v in tf.items() if k!='180159670' and 'FUN_180159670(' in v)
assert direct_callers==['180164ec0','180165d60']
#These are genuine external text-insertion interfaces, never members of the
#owned fresh raw -> selected first720 sequence. No absence-of-string guessing.
bindings=[(0x1802df240,0x180166cb0),(0x1802df250,0x180166d30),
          (0x1802df260,0x180166dc0),(0x1802df268,0x180166eb0),(0x1802df270,0x180164ec0)]
for slot,target in bindings:assert t.qwords(slot,1)[0]==target
for address in [0x180167210,0x180157c60,0x180156950,0x180157340,0x180157030]:
    text=tf[f'{address:x}']
    assert not any('FUN_'+x+'(' in text for x in ['180159670','180164ec0','180165d60','180166dc0'])
terms(tf,0x180157c60,'FUN_180156950','FUN_180157340')

#2A is a freshly empty UTF8 string-interning alphabet, not acousticAL. Its
#lookup path is non-inserting while mutable38=0; its28 tree root is NULL.
assert e.qwords(0x1801ddea0,1)[0]==0x180050a20
assert e.qwords(0x1801ddf10+0x28,1)[0]==0x1800511b0
terms(ef,0x180050a20,'*(undefined4 *)(param_2 + 0x30) = 0','*(undefined1 *)(param_2 + 0x38) = 0','param_1,0x28')
terms(ef,0x18004d6f0,'*(undefined8 *)(param_2 + 8) = 0')
terms(ef,0x1800511b0,'*(char *)(param_2 + 0x38)','FUN_180050f10','return 0xffff')
terms(tf,0x18013b870,'param_1,0x2a','*(longlong *)(param_2 + 0x18) = lVar3',
      '*(undefined8 *)(param_2 + 0x20) = 0','*(undefined8 *)(param_2 + 0x30) = 0')

#V30 imports/remaps text; V50 merely reads. The supported typed owner exposes
#neither V30 import nor external/prior text ingress.
assert t.qwords(0x1802dc740+0x30,1)[0]==0x18013be80
assert t.qwords(0x1802dc740+0x50,1)[0]==0x18013c920
terms(tf,0x18013be80,'FUN_18013b5a0','*(undefined1 *)(param_2 + 0x40) = 1')
terms(tf,0x180139dc0,'FUN_1801396f0','& 0xfffb | 3','| 2')
terms(tf,0x180139c90,'FUN_180018f70','*(undefined8 *)(param_4 + 0x18)')
terms(tf,0x18013c920,'FUN_18013c840','param_2 + 0x20')
assert ('lea','r9, [rsi + 0x20]') in code(t,0x18013c9b8,0x18013c9ed)
assert ('mov','r10, qword ptr [rax + 0x68]') in code(t,0x18013c9b8,0x18013c9ed)
assert ('cmp','eax, 2') in code(t,0x18013a183,0x18013a1c0)
terms(tf,0x18013a0b0,'*(uint *)(param_4 + 3) < 2','uVar2 < uVar6','+ 0xe0)','param_1,5')

regions=[]
for b,start,end in [(t,0x180002afa,0x180002bbe),(t,0x18013b870,0x18013b9e0),
 (t,0x18013c920,0x18013ca40),(t,0x180139c90,0x180139dc0),(t,0x18013a0b0,0x18013a670),
 (t,0x18016a130,0x18016a280),(t,0x180227388,0x1802273d0),
 (e,0x180050a20,0x180050ae0),(e,0x1800511b0,0x180051280),(e,0x18004d6f0,0x18004d730)]:
    regions.append(dict(binary=b.name,start=hex(start),end=hex(end),
        sha256=hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()))
report=dict(regions=regions,sourceGetter='219+120 exact169740 via002530 descriptor219 bootstrap',
    alternateCopy='source secondary170/180 -> TOP20/30 even when empty',
    actualControl='fresh2A, rootNULL/count0/mutable0; lookupFFFF without insertion',
    alternateEmptyFailure='13C920 passes NONNULL TOP+20; kind4 offsetcount<2 fails',
    externalAlternateInsertionCallers=direct_callers,
    noVendorExecution=True,scope='fresh raw no external/prior/import source; not273.V30 conversion or accepted text')
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS10 PE regions, exact219 attachment bootstrap/2A empty lifecycle/V50 nonnull-alternate proof, external text insertion separated; no vendor execution.')
