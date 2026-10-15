"""Static native frontend1A0/1A8 bindings and bounded ordinary source policy."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();f=functions();cs=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
bindings=[(0x1802de610+0x1a0,0x180152da0),(0x1802e1ff0+0x130,0x1801aae70),
 (0x1802de610+0x1a8,0x180152dc0),(0x1802e1ff0+0x68,0x1801a77d0),
 (0x1802e1ff0+0x88,0x1801a78d0),(0x1802e1ff0+0xb0,0x1801a7b30)]
for slot,target in bindings:assert b.qwords(slot,1)[0]==target
blocks=[(0x1801aae70,0x1801ac0d5,'1e05e24372dfe33d1aca0d83a0e18c97615f464d052fb32368fc58db796b6228'),
 (0x180152dc0,0x180152e60,'1b99fe02f565600b2bdce0ea33165252f20206acc5a0576c71ffb1fc3dd8830b'),
 (0x1801a78d0,0x1801a7920,'4a11b66acbd1a2b06287fc3dddbc01ad9777ed5010abbe0bf74ae948df567e90'),
 (0x1801ad9d0,0x1801adf50,'75710418e20bd67d38d4b70c4b5514544f69edf9a5411f9f24644c6cf6f31c0a')]
for start,end,sha in blocks:assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==sha
def asm(start,end):return [(i.mnemonic,i.op_str) for i in cs.disasm(b.data[b.off(start):b.off(end)],start)]
assert asm(0x180152da0,0x180152db1)==[('mov','rdx, qword ptr [rdx + 0x70]'),('mov','rax, qword ptr [rdx]'),('mov','r10, qword ptr [rax + 0x130]'),('jmp','r10')]
fallback=f['180152dc0'];assert "if (cVar1 == '\\x01')" in fallback and 'return 0xffff;' in fallback
assert 'return *(undefined2 *)(lVar4 + 0x20);' in fallback
assert ('movzx','eax, byte ptr [rax + 7]') in asm(0x1801a78d0,0x1801a7920)
delimiter=f['1801aae70']
assert 'param_3[6] < *(int *)(param_2 + 0x220)' in delimiter and '-1 < (short)local_b0[6]' in delimiter
assert '*(undefined1 *)((longlong)param_4 + 0xc) = 0;' in delimiter and '*(undefined1 *)((longlong)param_4 + 0xd) = uVar9;' in delimiter
assert delimiter.index('if (iVar26 < 1)')<delimiter.index('iVar6 = param_3[2]')
assert 'if (iVar6 == -1) goto LAB_1801ac0a9' in delimiter and '(local_b0 == (uint *)0x0)) goto LAB_1801ac0a9' in delimiter
assert 'if (iVar14 == 0)' in delimiter and '= 0xfffc;' in delimiter
assert "*(char *)(param_2 + 0x159) == '\\0') goto LAB_1801ac085" in delimiter
assert '*(int *)(puVar16 + 8) != -1 || (*(int *)(puVar17 + 8) != -1)' in delimiter
assert asm(0x1801ac090,0x1801ac09c)==[('cmp','edi, dword ptr [rdx + 8]'),('jne','0x1801ac0a4'),('test','byte ptr [r14 + 6], 8'),('je','0x1801ac0a4')]
assert '*(undefined4 *)(param_2 + 0x208) = 0' in f['1801a7230']
assert 'uVar4 = 0xffffffff' in f['18019eb10']
assert 'uVar2 = *(undefined2 *)(param_5 + 0x18)' in f['1801a37b0']
reader=f['1801ad9d0'];assert 'param_2 + 0x159,1,1' in reader and 'param_2 + 0x15c,4,1' in reader
raw=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IHH',raw,189379+8)==(0x21b,0,0x10a)
assert struct.unpack_from('<I',raw,189379+4)==(347,)
assert raw[189729]==0 and struct.unpack_from('<f',raw,189730)==(0.,)
report=dict(bindings=len(bindings),blocks=[dict(start=hex(s),end=hex(e),sha256=h) for s,e,h in blocks],
 fallback='152DC0 graph68 ->88 type1 only ->primary source38+20; actual owned ordinarytype0 returnsFFFF',
 ordinaryMetadata='19EB10 FFFF ->arc18;1A37B0 left index preserved;fallback208=0',
 delimiter='cursor0/leading/trailing return0;interior kind0 emitsFFFC;assignedline kind2 +actual159zero ->nooutputthenrealcurrentarc6&8 fallbackFFFB',
 kind2Source='same exact sampled graph, ordinary descriptor0 withactualassigned line>=0; main21B189379+350=0',
 remaining='interior kind1/FFFB geometry and nonordinary/custom-token metadata; prepared requests are not winning rows',noVendorExecution=True)
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS static6bindings/4pinnedblocks, actual graph159 source, empty/FFFC/assigned-FFFA path andarcflag8fallback, FFFDtype0 sentinel')
