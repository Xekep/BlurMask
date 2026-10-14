"""Static3A0 prototype-index audit. Reads pinned binaries; never executes vendor code."""
import argparse,hashlib,json,pathlib,struct,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
sys.path.insert(0,str(root/'tools/handwriting'))
from inspect_text_features import Binary,functions
from audit_legacy_character_properties import PE,ENGINE_SHA
import capstone
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();init=f['1801b0760'];score=f['1801bcbd0'];insert=f['18009e020']
# param2 is an eight-byte pointer: +74 means byte3A0, +52 means290 output width,
# +2D means168 symbol count, +1A/+1B/+1C meansD0/D8/E0 group arrays.
assert 'undefined8 *param_2' in init and 'param_2 + 0x74' in init
assert '0.0 < *(float *)(param_2[0x1c]' in init
assert '(uVar22 < *(ushort *)(param_2 + 0x52))' in init
assert '(*(ushort *)(param_2 + 0x2d) <= uVar22)' in init
assert '(uVar25 == 0xffff) && (!bVar5)' in init
assert 'if (local_138 != 0)' in init and '*(ushort *)(lVar15 + 0x14) = uVar25' in init
assert '*param_4 = 0;' in insert and '*param_4 = 1;' in insert
assert 'if (*(ushort *)((longlong)param_2 + 0x1d6) < *(ushort *)(param_2 + 0x52))' in init
assert '*(undefined2 *)((longlong)param_2 + 0x1d6) = 0xffff' in init
assert '*(short *)(param_2 + 0x2d) + 1' in init and '*(short *)(param_2 + 0x2d) + 2' in init
symbols=[(0x1802e9b80,'-'),(0x1802e9cbc,'\uff0d'),(0x1802e9bac,'_'),
 (0x1802e9ce4,'\uff3f'),(0x1802e9bc0,'\u00ad'),(0x1802eb700,'\u30fc'),
 (0x1802eb704,'\u4e00'),(0x1802eb368,'\U000f0002'),(0x1802eb370,'\U000f0003')]
for address,text in symbols:
 raw=text.encode('utf8');assert b.data[b.off(address):b.off(address)+len(raw)+1]==raw+b'\0'
assert '*(undefined8 **)(param_2 + 0x3a0)' in score
assert '*(ushort *)((longlong)puVar2 + 0x12)' in score and '*(ushort *)((longlong)puVar2 + 0x14)' in score
assert 'uVar1 == *(ushort *)(param_2 + 0x1e4)' in score and '+ 0xc)' in score
assert '+ 0x90)' in score and '+ 0x60)' in score
assert '+ 0x80)' in score and '+ 0x88)' in score
assert 'pcVar5 = *(code **)(*plVar8 + 0xd8)' in score and '(uVar9 & 0xffff) + 1' in score
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
instructions=list(cs.disasm(b.data[b.off(0x1800ebd30):b.off(0x1800ebd4b)],0x1800ebd30))
assert [i.mnemonic for i in instructions]==['mov','movsxd','movd','cvtdq2ps','mulss','ret']
assert instructions[2].op_str=='xmm0, dword ptr [rax + rcx*4]'
engine_data=(root/'research/myscript/sdk/build/x64/MyScriptEngine.dll').read_bytes()
assert hashlib.sha256(engine_data).hexdigest()==ENGINE_SHA
engine=PE(engine_data)
assert engine.read(0x1801de1b0+0x80,'Q')[0]==0x180052d40
assert engine.read(0x1801de1b0+0x88,'Q')[0]==0x180052d50
resource=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert struct.unpack_from('<8H',resource,6219)==(179,140,65535,178,182,180,181,192)
report=dict(textSha256=hashlib.sha256(b.data).hexdigest(),engineSha256=ENGINE_SHA,
 producer='1B0760 ordered positive C8/D0/D8/E0 groups -> insert-only09E020 tree3A0, first valid NN member wins; targets never recursively resolved',
 ordinaryPrototypes=[text for _,text in symbols],prolongedSoundGuard='1D6 cleared when30FC label<NNwidth before prototype insertion',
 consumer='1BCBD0 key12/target14 lookup before D8; output label unchanged; closed no-prototype uses graph21B90 range-vector/source21860 firstphysical enclosure probability; wide uses alphabet80/88',
 neuralBounds='0EBD30 directly reads buffer[label+1], no bounds branch; portable unresolved labels refuse rather than invent0',
 actualFirstHeader=[179,140,65535,178,182,180,181,192],
 actualFirstRemap={'185':140,'186':140,'183':140,'187':140,'188':140,'193':134,'194':129},
 actualScope='Verified first6120 reader requires zero knowledge groups and no wide pairs; soft hyphen183 is outsideNN180, not an internal output. No Cyrillic/Latin confusable substitution.')
target=pathlib.Path(a.output);target.parent.mkdir(parents=True,exist_ok=True)
target.write_text(json.dumps(report,separators=(',',':'),ensure_ascii=True)+'\n');print(json.dumps(report,ensure_ascii=True))
