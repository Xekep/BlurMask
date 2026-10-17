"""Static-only proof for local pass + ordinary one-source final mode0."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[(0x1801a61c0,0x1801a6980,'5601d88e59901a9351625bc133a29baf174528c99f056ac71e1e8e06a47d8a4a'),
 (0x18019e8b0,0x18019eb10,'d5db4c7a1a16ce5d9186bd995225075e982f426c54ee5efef825454f1265c74f'),
 (0x1801a5b00,0x1801a61c0,'080fcf860b329bc36acc46680c17552da89cdfe95113f9636af4367ffe7b8972'),
 (0x180194140,0x18019416c,'90cd1e529e3df7582ecda89d60edecbe765e619d35c77a6fd8108b13564b2aff')]
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);ins={}
for a,z,h in blocks:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({v.address:(v.mnemonic,v.op_str) for v in md.disasm(raw,a)})
pins={0x18019ea23:('mov','byte ptr [rsp + 0x20], 1'),0x18019ea35:('call','0x1801a5b00'),
0x1801a5c24:('cmp','word ptr [rbx + 2], ax'),0x1801a5c28:('je','0x1801a616f'),
0x18019ea53:('mov','byte ptr [rsp + 0x20], 0'),0x18019ea5b:('mov','r9d, dword ptr [rsi]'),
0x18019ea5e:('dec','r9d'),0x18019ea61:('add','r9d, r15d'),0x18019ea64:('call','0x1801a5b00'),
0x1801a5bd0:('sub','ax, 1'),0x1801a5be6:('js','0x1801a61a3'),
0x180194146:('cmp','dword ptr [r8 + 0x14], 0'),0x180194158:('call','0x180193860'),
0x18019415d:('cmp','eax, dword ptr [rbx + 0x11c]'),0x180194163:('setg','al')}
for a,pin in pins.items():assert ins[a]==pin,(hex(a),ins.get(a),pin)
fs=functions();local=fs['1801a61c0'];join=fs['18019e8b0'];prune=fs['1801a5b00']
assert 'iVar3 + -1' in local and 'piVar24,uVar26,0' in local
assert '*piVar6 + -1 + iVar5' in join and "if (puVar17[1] != 0xffff)" in prune
report=dict(blocks=[dict(start=hex(a),endExclusive=hex(z),sha256=h) for a,z,h in blocks],pins=[f'{a:x} {m} {o}' for a,(m,o) in pins.items()],
 proof='One ordinary descriptor has secondaryFFFF on every group. Globalmode1 skips them; explicitfinalmode0 still runs atlocalCount-1. Local+final equals node-ascending all-node mode0 because ordinary geometry eligibility is independent of adjacency. No group-ID compaction inferred.',
 runtime='ExistingPrepare remainslocal. AdditivePrepareSingleDescriptor is called by actualFreshMainSingleLineSampler; actualC# source-reference tests verify ownership separately.',
 scope='Assignedordinary one-descriptor main; notcross-source geometry, globalgroupcompaction, NN quality or a finalwinner.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f'PASS static{len(pins)} final-prune/mode1/signedordinal pins in4 exactPEblocks;ordinarysingle-source equivalence')
