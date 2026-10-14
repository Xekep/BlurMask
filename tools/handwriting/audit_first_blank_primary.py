"""Static first regular+blank producing-kernel input/dispatch proof."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--resource',required=True);p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
assert b.qwords(0x1802de610+0xf0,1)[0]==0x1801522f0 and b.qwords(0x1802de610+0xf8,1)[0]==0x180152320
for address in ('1801522f0','180152320'):
    s=f[address];assert '*(undefined4 *)(param_3 + 8)' in s and '*(undefined2 *)(param_3 + 0x10)' in s and '*(undefined1 *)(param_3 + 0x20)' in s
outer=f['1801ef070'];inner=f['1801ee3b0'];start=outer.index('if ((*(short *)((longlong)local_240 + 0xe)')
blank=outer[start:outer.index('LAB_1801efae1:',start)]
assert '(int)uVar35 < (int)local_280' in blank and '*(ushort *)(param_2 + 0xe2)' in blank
assert '*(longlong *)(local_1d8 + 0x50)' in blank and 'local_318 = 0xffff' in blank
assert 'local_2e0 = 0' in blank and 'FUN_1801ee3b0' in blank
assert '*plVar3 + 0x1c0' in inner and 'local_c24' in inner
query=f['1801c13a0'];assert '(short *)' in query and '0xffff' in query
#Native fallback owner and its top20 exception are separately covered by the
#ranked-cache port; here pin the concrete blank request's label/FFFF ordinal.
assert 'local_2b0 = (**(code **)(*plVar22 + 0x60))' in outer
assert b.qwords(0x1802de610+0x18,1)[0]==0x180150780 and b.qwords(0x1802e5a20+0x60,1)[0]==0x1801d6290
assert 'movzx eax, byte ptr [rdx + 0xb8]' in asm(0x1801d6290,0x1801d6298)
reader=f['1801d4950'];assert reader.index('param_2 + 0xb8,1')<reader.index('param_2 + 0xb9,1')<reader.index('param_2 + 0xba,2')<reader.index('param_2 + 0xbc,2')
assert '0xb8' not in f['1801d60d0']
raw=pathlib.Path(a.resource).read_bytes();assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978' and raw[11375]==0
assert "uStack_bec = CONCAT31(uStack_bec._1_3_,5)" in inner
assert 'param_10 == *(short *)(param_2 + 0xe2)' in inner and 'local_c30 = 0x4000000' in inner
assert '*(int *)' in inner and 'local_c34 = local_c34 + local_c24' in inner
report={'fallback':'1EF070 appends blankE2 aftereacharc regular requests whenheaderE present/span<node;StartMap50 only andshortlistFFFF;1EE3B0 frontend1C0 ->1C13A0 ranked fallback+geometry query',
 'compatibility':'1522F0/152320 use originalPreviousEndNode+8,originalArc+10,currentSpan+20; geometric ancestor fields+4/+E remain distinct',
 'consecutive':'1EF070 plVar22=frontend18/150780 selected247;V60=1D6290 readsB8;actualreader1D4950 placesB8 atParametersOffset−6=11375/value0;1D60D0doesnotmutateB8',
 'scope':'Freshfirst zero-trait same-descriptor ordinary0/2/5/6; protocol194 remains explicit; actual controls retain0blankrows sofullblankcontextnotclaimed'}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps(report))
