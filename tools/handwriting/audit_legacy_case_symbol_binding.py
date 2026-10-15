"""Pinned case symbol/permission chain into the preserved dictionary maps."""
import hashlib,json,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('output.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();assert 'FUN_180232c80' in f['180232ed0'] and '+ 0x30))' in f['180232ed0']
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
asm=[i.mnemonic+' '+i.op_str for i in md.disasm(b.data[b.off(0x180093af0):b.off(0x180093af0)+31],0x180093af0)]
assert any('[rax + 0x58]' in i for i in asm) and any('[r9 + rdx*2]' in i for i in asm)
assert '+ 0x68)' in f['180234840'] and 'puVar5 = puVar5 + 3' in f['180234840']
refresh=f['180093f80']
for term in ['param_2 + 0x58','param_2 + 0x50','bVar7 = 5','bVar7 = 1','0x1e00','0x800','uVar4 = 0xffff']:
 assert term in refresh,term
assert '0x4000000' in f['1801c01b0'] and '40000.0' in f['1801c01b0']
report={'enumeration':'2B7 preserved58 lexical map→093AF0→292232C80, no second map',
 'permissions':'1C01B0 succeeded→234840 stored triplets→inner68/093F80 restores58 and filters50; no callback on failure',
 'mappingInstructions':asm,'scope':'prepared source symbol/permission phases, not completed live source factory/epoch/winner'}
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS pinned case->preserved58 enumeration and permission->inner working50 binding; PE-only.')
