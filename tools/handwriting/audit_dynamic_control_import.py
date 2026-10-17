from pathlib import Path
import sys,hashlib,json
repo=Path(__file__).resolve().parents[2];sys.path.insert(0,str(repo/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
e=Binary('MyScriptEngine.dll');t=Binary();ef=functions('MyScriptEngine.dll');tf=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c';assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
for va,target in [(0x1801ddf10+0x18,0x180050f10),(0x1801ddf10+0x28,0x1800511b0),(0x1801ddf10+0x30,0x180051280),(0x1801ddc00+0x18,0x18004d950),(0x1801ddc00+0x28,0x18004de20),(0x1801ddc00+0x38,0x18004e110),(0x1801ddc00+0x40,0x18004e120)]:assert e.qwords(va,1)[0]==target
for a,terms in {'18004d950':['uVar12 = 0x1505','uVar12 * 0x21 ^ (uint)bVar2','(ulonglong)*(uint *)(param_3 + 0x10) + 0x14','plVar8[1] = 0','*param_4 = 1'], '18004de20':['uVar8 = 0x1505','uVar8 * 0x21 ^ (uint)bVar2','uVar8 >> 0x10 & 0xf','uVar8 >> 6 & 0x1f','uVar8 & 0x3f'],'180050f10':['local_78[0]','param_2 + 0x28','param_2 + 0x30','param_2 + 0x40','param_2 + 0x58','iVar4 == 10','iVar4 == 0x20','iVar4 == 0xa0'],'1800521f0':['param_3 & 0xf','+ 0x10','8,param_3']}.items():
 for term in terms:assert term in ef[a],(a,term)
assert 'lVar3 < param_3' in tf['1801399f0'];assert 'iVar4 * 0x28' in tf['18013b350'];assert 'FUN_180139dc0' in tf['18013b350']
assert '0x38' in tf['18013be80'] and '0x28' in tf['18013be80']
unit={i.address:(i.mnemonic,i.op_str) for i in md.disasm(t.data[t.off(0x180139dc0):t.off(0x18013a0b0)],0x180139dc0)}
assert unit[0x180139e10]==('mov','r9d, 1');assert unit[0x180139e16]==('lea','r8, [rip + 0x1b2a9f]');assert 0x180139e1d+0x1b2a9f==0x1802ec8bc
assert t.data[t.off(0x1802ec8bc):t.off(0x1802ec8bc)+2]==bytes.fromhex('c2a7')
ins={i.address:(i.mnemonic,i.op_str) for i in md.disasm(e.data[e.off(0x180050f10):e.off(0x180051140)],0x180050f10)}
pins={0x180050fad:('call','r10'),0x180050fbc:('cmp','byte ptr [rsp + 0x20], 0'),0x180050fd7:('call','0x1800521f0'),0x180050fde:('je','0x18005100b'),0x180051005:('mov','word ptr [rdi + 0x30], ax'),0x18005100b:('mov','ebx, 0xffff'),0x180051017:('movzx','r9d, bx'),0x180051021:('mov','r10, qword ptr [rax + 0x38]'),0x180051025:('call','r10')}
for va,expected in pins.items():assert ins[va]==expected,(hex(va),ins[va])
regions=[]
for b,lo,hi in [(e,0x18004d950,0x18004dbc0),(e,0x18004de20,0x18004e040),(e,0x18004e130,0x18004e270),(e,0x180050f10,0x180051140),(e,0x1800511b0,0x180051280),(e,0x180051280,0x180051380),(e,0x1800521f0,0x180052260),(t,0x1801399f0,0x180139c90),(t,0x180139dc0,0x18013a0b0),(t,0x18013b350,0x18013b5a0),(t,0x18013b5a0,0x18013b870),(t,0x18013be80,0x18013c1d0)]:
 regions.append(dict(binary=b.name,start=hex(lo),end=hex(hi),sha256=hashlib.sha256(b.data[b.off(lo):b.off(hi)]).hexdigest()))
report=dict(tableBindings=7,failureOrderPins=[dict(address=hex(a),mnemonic=v[0],operands=v[1]) for a,v in pins.items()],regions=regions,unitLiteral=dict(callLength=1,address='1802EC8BC',storedBytes='c2a7',actualPassedBytes='c2',meaning='Do not silently repair to valid U+00A7'),noVendorExecution=True,unresolved=['Actual postimportV50 primary-alphabet pointer/caller; converted IDs must not use acousticAL','Malformed general UTF8/other-encoding stream conversion beyond pinned valid-UTF8 or native unit literal scope'])
if len(sys.argv)!=2:raise SystemExit('report.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n');print('PASS7 exact Enginebindings/12 pinnedTextEngine regions/rawunitliteral2instructions; static only')
