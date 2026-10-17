"""Static real C81.V80 -> BCD/BEF/CBB -> CB7 context provenance; no SDK execution."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
d=Binary('MyScriptDocument.dll');e=Binary('MyScriptEngine.dll')
assert hashlib.sha256(d.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];count=0
def bind(b,table,slot,target):
 assert b.qwords(table+slot,1)==(target,);bindings.append(dict(dll=b.name,table=hex(table),slot=hex(slot),target=hex(target)))
def region(b,start,end,pins):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(i.address,i.mnemonic,i.op_str)for i in md.disasm(raw,start)]
 for pin in pins:assert pin in rows,(b.name,pin)
 count+=len(pins);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
bind(d,0x180209300,0x80,0x180113e30);bind(d,0x180208f90,0x80,0x180113e30)
bind(d,0x180209248,0x10,0x1801114c0)
bind(d,0x18020ad70,0x18,0x18020b2d8);bind(d,0x18020b2d8,8,0x18020b070)
bind(d,0x18020b2d8,0x10,0x18020b170);bind(d,0x18020b170,0x10,0x1801363a0)
bind(d,0x18020b070,0x20,0x18013bce0);bind(d,0x18020b070,0x28,0x18013c500)
bind(d,0x180208f90,0x178,0x180116970);bind(d,0x180208f90,0x180,0x180116990)
bind(e,0x1802273a0,0x200,0x18000c0a0)
bind(d,0x180209508,0x18,0x18011b310);bind(d,0x180208f70,0,0x180115000);bind(d,0x180209278,0,0x180115000)
region(d,0x180113e30,0x180113e99,[
 (0x180113e52,'cmp','qword ptr [rax + 0x30], rbx'),(0x180113e58,'call','0x1801101a0'),(0x180113e67,'call','0x1801106f0')])
region(d,0x1801106f0,0x1801108a3,[
 (0x18011070e,'mov','rax, qword ptr [rdx + 0x30]'),(0x180110712,'mov','qword ptr [rsp + 0x20], rax'),
 (0x180110724,'call','0x180110660'),(0x180110748,'mov','edx, 0xcbb'),(0x180110766,'mov','qword ptr [rsp + 0x28], rsi'),
 (0x180110797,'mov','edx, 0xcbb'),(0x1801107bb,'lea','r8, [rsp + 0x30]'),(0x1801107d3,'mov','qword ptr [rsp + 0x28], rax'),
 (0x1801107f9,'mov','edx, 0xbef'),(0x180110819,'lea','r8, [rsp + 0x20]'),(0x18011081e,'mov','r9d, 0x10'),
 (0x180110827,'call','rbx'),(0x180110863,'mov','qword ptr [rsp + 0x40], rax'),(0x180110868,'mov','edx, 0xbcd')])
region(d,0x180111a11,0x180111a69,[
 (0x180111a11,'mov','edx, 0xcb7'),(0x180111a16,'mov','qword ptr [rbp + 0x40], r14'),
 (0x180111a3e,'lea','r8, [rbp + 0x40]'),(0x180111a42,'mov','r9d, 8'),
 (0x180111a59,'mov','qword ptr [rcx + 0xb8], rax'),(0x180111a63,'je','0x180111b42')])
region(d,0x180111935,0x180111981,[
 (0x180111935,'mov','edx, 0x3d'),(0x180111962,'lea','r8, [rbp + 0x40]'),
 (0x18011197a,'mov','qword ptr [rcx + 0x88], rax')])
assert d.data[d.off(0x18020ca38):d.off(0x18020ca38)+5]==b'page\0'
region(d,0x1801363a0,0x180136606,[
 (0x1801363ee,'mov','qword ptr [rbx + 0x90], rax'),(0x180136420,'mov','byte ptr [rbx + 0x1b0], sil'),
 (0x180136427,'mov','dword ptr [rbx + 0x1b4], esi'),(0x180136564,'mov','qword ptr [rbx + 0x1c0], rsi'),
 (0x180136572,'mov','qword ptr [rbx + 0x1d0], rsi')])
region(d,0x180116970,0x1801169b0,[
 (0x180116974,'mov','rdx, qword ptr [rax + 0xb8]'),(0x18011697e,'mov','r10, qword ptr [rax + 0x20]'),
 (0x180116994,'mov','rdx, qword ptr [rax + 0xb8]'),(0x18011699e,'mov','r9, qword ptr [rax + 0x28]')])
region(d,0x18013bce0,0x18013bd89,[
 (0x18013bcfa,'mov','eax, dword ptr [rdx + 0x1b4]'),(0x18013bd19,'mov','byte ptr [rdx + 0x1b0], r10b'),
 (0x18013bd22,'mov','dword ptr [rdx + 0x1b4], eax'),(0x18013bd43,'call','0x18013bba0'),(0x18013bd5f,'call','0x18013b9c0')])
region(d,0x18013c500,0x18013c597,[
 (0x18013c521,'call','0x18013b530'),(0x18013c526,'sub','dword ptr [rbx + 0x1b4], 1'),
 (0x18013c532,'mov','byte ptr [rbx + 0x1b0], 0'),(0x18013c53c,'call','0x18013bd90'),(0x18013c54b,'call','0x18013bfd0'),
 (0x18013c558,'mov','dword ptr [rbx + 0x1c0], ecx'),(0x18013c55e,'mov','dword ptr [rbx + 0x1d0], ecx'),
 (0x18013c573,'mov','dword ptr [rbx + 0x1c0], ecx'),(0x18013c579,'mov','dword ptr [rbx + 0x1d0], ecx')])
region(e,0x18000c0a0,0x18000c0a5,[(0x18000c0a0,'mov','rax, qword ptr [rdx + 0x28]'),(0x18000c0a4,'ret','')])
region(d,0x18011b310,0x18011b370,[
 (0x18011b31a,'sub','edx, 0x61'),(0x18011b31f,'sub','edx, 0x47'),(0x18011b322,'je','0x18011b334'),
 (0x18011b334,'lea','rax, [rip + 0xedc35]')])
assert 0x18011b33b+0xedc35==0x180208f70
region(d,0x180115000,0x18011500c,[
 (0x180115000,'mov','rax, qword ptr [rdx + 0x10]'),(0x180115004,'mov','rax, qword ptr [rax + 0x88]'),(0x18011500b,'ret','')])
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=count,vendorExecution=False,
 scope='Real native page accessor/history context candidate; actual05F720 entry/request+8 binding and library acceptance remain unproved'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/{len(bindings)} bindings/{count} pins; real page accessor toCB7, no library acceptance.')
