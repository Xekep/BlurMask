"""Read-only01B4E0/019990 source AVL and Ink081870 raw comparison proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64

root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
t=Binary();ink=Binary('MyScriptInk.dll');f=functions()
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
assert ink.qwords(0x1800e5c90+0x20,1)[0]==0x1800e5ac0
assert ink.qwords(0x1800e5ac0+0xf8,1)[0]==0x180081870
md=Cs(CS_ARCH_X86,CS_MODE_64)
def dis(b,start,end):return [(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
comparison=dis(ink,0x180081870,0x180081915)
pins=[('cmp','qword ptr [rdx], r9'),('jb','0x180081900'),('ja','0x1800818eb'),
 ('cmp','r8, rcx'),('add','rdx, 0x10'),('cmp','r11d, ebx'),('seta','sil')]
for pin in pins:assert pin in comparison,pin
#These branches are unsigned. Native mode/fraction bytes remain part of the
#raw64-bit key; comparison performs no range sort, merge or canonicalization.
assert not any(mn in ['jl','jg','jle','jge']for mn,_ in comparison)
node=f['18001b4e0'];rotate=f['180019990'];ctor=f['18001c010'];allocate=f['18001b0e0']
for text in ['+ 0xf8','FUN_18001b0e0','FUN_180019990','*param_4 = 0','*param_4 = 1',
 'puVar8[7] = uVar10','puVar8[8] = uVar5','puVar8[0x10] = param_2[2]','puVar8[0x11] = 0']:
    assert text in node,text
assert '*(char *)(puVar15 + 2)' in node and '(iVar7 != -2) && (iVar7 != 2)' in node
for text in ['plVar3[1] = (longlong)param_3','plVar2[1] = (longlong)param_3',
             '*plVar2 = (longlong)param_3','return plVar2']:
    assert text in rotate,text
for offset in ['0x48','0x50','0x58','0x60','0x68','0x70']:
    assert f'*(undefined8 *)(param_2 + {offset}) = 0' in ctor
#Fresh insertion-only allocation has28 records of144 bytes per4032-byte page.
#Portable identities are not the address-sorted native page descriptors.
assert '0x90' in allocate and '0x1d' in allocate
assert '(param_1,1,0xfc0)' in f['18001aa80']
assert 'FUN_18001b4e0(param_1,param_2 + 0x48,pcVar18 + 8,local_b4)' in f['180024f30']
regions=[]
for b,start,end in [(ink,0x180081870,0x180081915),(t,0x18001b4e0,0x18001b920),
    (t,0x180019990,0x180019ac0),(t,0x18001b0e0,0x18001b2b0)]:
    regions.append(dict(binary=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()))
report=dict(regions=regions,unsignedComparatorPins=len(pins),freshCache='01C010 zero48..70',
    key='ordered unsigned64 first/last pairs, then unsigned count; duplicates retained',
    node='01B4E0 left0/right8/balance10/key38/count40/previous80/next88',
    allocation='fresh insertion-only; no address ordering,free/recycle,or allocatorfailure replay simulated',
    scope='same original raw document actualSourcePositions;024D40/textreusecaller stillseparate',noVendorExecution=True)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS4 PE regions/7unsignedcomparison pins,01B4E0/019990 AVL+insertionlist,actualC21F8 andfreshcache binding; staticonly.')
