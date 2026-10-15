"""Static ordinary main confidence/candidate/channel ownership; never loads DLLs."""
import bisect,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();f=functions();cs=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
blocks=[(0x1801bcbd0,0x1801be19e,'6b67975a271c2d8fdb8192a3cf344204cf42cfbb0e89ab66048dbc4c256ff857'),
 (0x180152f50,0x180152f8a,'9bf0bca1cfbc07b656bc337cb6350c0d08b02f107f6b5a6f2be0be69b96e3ee0'),
 (0x18000c8c4,0x18000c8d2,'f6660bcf41aca7d13528c4ef51d7454cd94028446f016f43f752d46453d8440c'),
 (0x180274ba0,0x180275220,'af88a033bb95e941dca5f41e50dcfb00e50018c010f79e7457fc4005ebd93b38'),
 (0x18000eb50,0x18000eb96,'9a629bdc9af5107957494b88cf8a716799bccaf2eff70835def42fa34d595bce'),
 (0x18000eba0,0x18000eca0,'abc90fccdf1fd04b4f6ce5f2f4fe105c6130ced6869a0eda96c4613022f7c7ce')]
for start,end,sha in blocks:assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==sha
def asm(start,end):return [(i.mnemonic,i.op_str) for i in cs.disasm(b.data[b.off(start):b.off(end)],start)]
assert asm(0x18000c8c4,0x18000c8d2)==[('mov','qword ptr [rdi + 0x84], rax'),('mov','qword ptr [rdi + 0x98], rax')]
assert ('xor','eax, eax') in asm(0x18000c860,0x18000c8c4)
assert asm(0x180152f6b,0x180152f77)==[('movzx','eax, byte ptr [r10 + 0x88]'),('mov','byte ptr [rsp + 0x28], al')]
assert b.qwords(0x1802e4180+0x80,1)[0]==0x1801c1300
assert 'FUN_1801bcbd0(param_1,param_2,0,param_3,plVar17)' in f['1801be930']
assert '*(undefined4 *)(param_2 + 0x350) = 0xbf800000;' in f['1801b0330']
assert "if ((0.0 <= fVar15) && (*(char *)(param_2 + 0x1a8) == '\\0'))" in f['1801be1a0']
#Candidate24 field0 normalization in1BE530 is distinct from its vector10.
assert '*(float *)(uVar8 + lVar2) = (float)dVar9' in f['1801be530']
assert '+ 0x10)' not in f['1801be530']
score=f['1801bcbd0'];init=f['1801b0760']
assert 'local_f0 = 0x14;' in score and 'if (uVar13 < *(ushort *)(param_5 + 0x28))' in score
assert '*(undefined4 *)' in score and '= 0;' in score
assert 'fVar19 + fVar18 + fVar20 + fVar21 + fVar22 + fVar23 + fVar24 +' in score
assert '*(float *)(lVar3 + uVar14 * 4) / (float)uVar16' in score
assert '0x218)' in init and 'param_2[0x2e] = lVar15;' in init and 'param_2[0x2f] = lVar15;' in init
begin=score.index('(**(code **)(**(longlong **)(param_2 + 0x180) + 0x90))');end=score.index('\n    else {',begin)
closed=score[begin:end]
assert '(param_1,*(undefined8 *)(param_2 + 0x170),*local_e8[0])' in closed
assert '*(longlong *)(param_2 + 0x178) + 0x60' in closed and 'FUN_18013e8e0' not in closed
assert b.qwords(0x1802df340+0x60,1)[0]==0x180168c00
assert b.qwords(0x1802e1ff0+0x90,1)[0]==0x1801a7970
getter=asm(0x180168c00,0x180168c19)
assert ('imul','rcx, rax, 0x1c') in getter and ('movss','xmm0, dword ptr [rax + rcx + 0xc]') in getter
resource=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(resource).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<h',resource,566380)==(0,)
assert struct.unpack_from('<IHH',resource,566408)==(1,192,256)
report=dict(blocks=[dict(start=hex(s),end=hex(e),sha256=h) for s,e,h in blocks],
 source='same owned DIRECT or CASE dictionary scorer; ordinary1BE930 calls1BCBD0 flag0 (D8)',
 rank='min(candidateCount,20), later vector channels zero; original candidate label calibrates192x4,3A0 only indexes neural180',
 closed='graphV90 stored range firstphysical -> original218V60; raw float, no mean or clamp',
 channel88='fresh00C860 zero84..8B; only fresh/no-property/reuse factories accepted;52 registered properties copied by00EBA0',
 postprocessing='fresh21D350=-1 disables1BE1A0;1BE530 modifiescandidate24field0, not vector10',
 scope='single assigned main descriptor, one or multiple physical strokes; no winner/text/geometry publication; prepared numerical controls are separate',
 noVendorExecution=True)
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS static6 pinned blocks, fresh88/property boundaries, ordinaryD8/top20/mean, original-labelcalibration, closed original21860 and disabled post-pruning')
