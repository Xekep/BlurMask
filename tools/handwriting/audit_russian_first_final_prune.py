"""Read-only actual FIRST frontend250?21B.A4?final mode0 proof. Never loads DLLs."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser();p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
sys.path.insert(0,str(a.native_repo/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[['180002530', 6442460464, 6442479460, 'b7451f1d540b8200d9f0a7cd6f4c9f4f78329aab68c4233712545d2fe3901ffb'], ['180156200', 6443852288, 6443853357, '527efc8b62dbafb15de32a47714f01c62d6b767947d6106f2b2bc53b351d53a0'], ['180156630', 6443853360, 6443854151, 'f813e7af04355d649a01b797c1cca9bae1fad8a5d9c98076639c1781a7f104ae'], ['180158970', 6443862384, 6443862954, '1699db05bd310fe7103cfd30899953df29e77c0638776a382fea85c202e6fb10'], ['180157c60', 6443859040, 6443859785, 'f0d2df52d695691775ff1301a214bf5e016547eb7001b74f0e784b5a86294dff'], ['180157c60', 6443859792, 6443862295, '9d3ca68e150fae5cee0f6ea7b39f2a6b9bf109f220225ecadc4a7ab0747439dc'], ['18000ed90', 6442511760, 6442513493, '7e6c85338339ea5ee8ec6bb5cb841a2b176695301e28995e54b20ece101baa89'], ['1801ec0a0', 6444466336, 6444466821, 'aaeaf0a7799f5b20e32a29117d9ac2de2fa9b121ae4c001d3a155ade938cb907'], ['1801ec330', 6444466992, 6444467351, '528d6b4d58ff89c766b861e7336cf46ed8bc14a9b607db81f4af805700392f47'], ['1801ec4a0', 6444467360, 6444467834, '2f90aa71ca60f31087675d13b8e2f481e40752303d9be8185ecc0e099951d9b1'], ['180151d40', 6443834688, 6443834813, '14c047033de2741e3a2c55d0cabf3d18cec35f4de7cfcc8e0c6b29cf0563c421'], ['180151dc0', 6443834816, 6443835225, '427b7e0f45c1ae2f934077273fe03e25e6302aa33824fcc6d207c4da79932117'], ['180150d00', 6443830528, 6443834250, '87dd5a9169436e46a8cc8c61965faa1936b9a53bca6b032053ad515eade3c000'], ['1801a61c0', 6444179904, 6444181873, '9e042b4664fe7135057a7042a5f13b4d6161483e40ff187097e8b2ab745d5494'], ['18019e8b0', 6444148912, 6444149509, '95c52a214ceacde65639c47308af7d87a7ec9adbe90d5ec9c981e65110a09a06'], ['1801a5b00', 6444178176, 6444179899, 'eac4a1200400114521f9cd91d7b5514bd2fde39f8d04faea7b3e18a6c8d933c3'], ['180194140', 6444106048, 6444106092, '90cd1e529e3df7582ecda89d60edecbe765e619d35c77a6fd8108b13564b2aff'], ['180193860', 6444103776, 6444106038, 'cfb9e8d8ee63e4b61fc790e34f9d39d51ff79998c38a573cdb16cdc8873859c0'], ['18019f850', 6444152912, 6444154648, 'ba80eb5c483bdb4ff78615a4e3f1a30ecdca24e063f1d570427c682aee54b848'], ['18019f850', 6444154656, 6444161096, '34eb9c5e7832c2720662be8cabec2f89be0b5795d3fcd5b2a991366ed7e1522d'], ['1801a5700', 6444177152, 6444177683, '7a68271f3a8949a91c99b6b8ebff3bcf27a9c56384a718e003d0f94dc8ecdf06'], ['180151ce0', 6443834592, 6443834673, '126716e201ba6058690be985edfe3a3b4fbf539016a455a4b79e6b7f3d01e966']]
leaves=[[6444214080, 6444214113, '83fa11741433c9488d05c237130081faa4000000480f45c1c3488d05b8361300c3'], [6443849520, 6443849658, '81fa210200007f6e746483c2e281fa8400000077564863c2488d15b1a8eaff0fb68402dc5715008b8c82bc5715004803caffe1488d05e6a01b00c3488d052ea01b00c3488d05069f1b00c3488d050e9e1b00c3488d05c69f1b00c3488d056e9f1b00c3488d05769e1b00c333c0c3488d05c3a01b00c333c9488d05b19e1b0081fa50020000480f45c1c3'], [6442502816, 6442502832, '4c894248b0014d85c0740449895008c3'], [6444115088, 6444115096, '488b8238010000c3'], [6444464080, 6444464115, '33c0448842084889421048894218488942208942284889423089423cb001c6423801c3']]
tables=[(6445025976, 6445026152), (6445026160, 6445026032), (6445026048, 6442502816), (6445026120, 6442511760), (6445459640, 6445458904), (6445458912, 6445458960), (6445458928, 6443849520), (6445659760, 6443834816), (6445659752, 6443834688), (6445495936, 6445495872), (6445495872, 6444466992), (6445495880, 6444467360), (6445495888, 6444467840), (6445488816, 6444115088), (6445473112, 6445473160), (6445473184, 6444214080), (6445473552, 6444179904), (6445470968, 6444106048), (6445459056, 6443834592), (6445495656, 6445495472), (6445495488, 6445495552), (6445495576, 6444464080), (6445495664, 6445495600), (6445495608, 6444466336)]
pins=[[6443862894, 'call', '0x180157c60'], [6443860050, 'mov', 'rbx, qword ptr [rsp + 0x250]'], [6443860537, 'mov', 'r8, qword ptr [rax + 0x250]'], [6443860637, 'mov', 'rbx, qword ptr [rsp + 0x250]'], [6443860910, 'mov', 'rax, qword ptr [rsp + 0x250]'], [6443861236, 'mov', 'rbx, qword ptr [rsp + 0x250]'], [6443861536, 'mov', 'r8, qword ptr [rax + 0x250]'], [6443861750, 'mov', 'rbx, qword ptr [rsp + 0x250]'], [6443861769, 'mov', 'rbx, qword ptr [rsp + 0x250]'], [6443862074, 'mov', 'rdi, qword ptr [rsp + 0x250]'], [6442512173, 'mov', 'r8d, 0x250'], [6442512314, 'mov', 'r8d, 0x250'], [6442512676, 'mov', 'r8d, 0x250'], [6442512723, 'mov', 'r8d, 0x250'], [6442512856, 'mov', 'r8d, 0x250'], [6442512903, 'mov', 'r8d, 0x250'], [6444467139, 'mov', 'r8d, 0x250'], [6444467221, 'mov', 'r8d, 0x250'], [6444467460, 'mov', 'r8d, 0x250'], [6443834721, 'mov', 'r8d, 0x250'], [6443835019, 'mov', 'r8d, 0x250'], [6444181450, 'call', '0x1801a5b00'], [6444181604, 'call', '0x18019e8b0'], [6444149008, 'cmp', 'dword ptr [rdi + 0x250], -1'], [6444149028, 'mov', 'dword ptr [rdi + 0x250], eax'], [6444149301, 'call', '0x1801a5b00'], [6444149348, 'call', '0x1801a5b00'], [6444106072, 'call', '0x180193860']]
md=Cs(CS_ARCH_X86,CS_MODE_64);ins={}
for fn,start,end,h in regions:
 raw=b.data[b.off(start):b.off(end-1)+1];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str) for i in md.disasm(raw,start)})
for start,end,code in leaves:assert b.data[b.off(start):b.off(end)]==bytes.fromhex(code)
for va,target in tables:assert b.qwords(va,1)[0]==target,(hex(va),hex(target))
for va,m,o in pins:assert ins[va]==(m,o)
f=functions()
#Whole Ghidra bodies hashed above. Text initialization installs its own24F
#service, not an inferred Engine scheduler. First registration is not execution.
assert '*(undefined8 *)(*(longlong *)(param_1 + 0x18) + 0x58) = *(undefined8 *)(puVar9 + 0x20)' in f['180002530']
assert '&DAT_1802e7660' in f['180002530']
run=f['18000ed90'];assert '(param_1,uVar10,0x250)' in run and '(param_1,uVar10,local_70,&local_38)' in run
assert '*(undefined8 *)(param_2 + 0x48)' in run
assert '(param_1,param_3,0x250)' in f['1801ec330'] and '(lVar4 + 8)' in f['1801ec330']
complete=f['180151dc0'];assert 'param_2 + 0x70' in complete and ',0xa4)' in complete
assert 'FUN_180150d00' in f['180151ce0']
assert '+ 0x90)' in f['180150d00'] and 'param_2 + 0x70' in f['180150d00']
assert 'FUN_18019e8b0(param_1,param_2)' in f['1801a61c0']
assert '*piVar6 + -1 + iVar5' in f['18019e8b0']
assert '*(undefined2 *)(lVar33 + 2 + lVar23) = 0xffff' in f['18019f850']
assert 'local_158._2_2_ = 0xffff' in f['18019f850']
assert 'if (puVar17[1] != 0xffff)' in f['1801a5b00']
assert 'iVar5 = 0;' in f['180193860'] and 'if (1 < uVar3)' in f['180193860']
resource=(a.native_repo/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(resource).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert resource[12727+16]==7 and list(resource[12727+306:12727+309])==[1,0,0]
assert struct.unpack_from('<i',resource,12727-214+202)[0]==20480
report=dict(staticOnly=True,fullFunctions=len(set(r[0] for r in regions)),completeGhidraRegions=len(regions),closedLeaves=len(leaves),tableBindings=len(tables),pins=len(pins),regions=regions,leaves=leaves,tables=tables,instructionPins=pins,scope='Actual RU FIRST one ordinary descriptor. Registration24F distinct from direct frontend250.V10 completion. No native DLL execution, no quality claim, no token/cross-source equivalence.')
a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS',report['fullFunctions'],len(regions),len(leaves),len(tables),len(pins))
