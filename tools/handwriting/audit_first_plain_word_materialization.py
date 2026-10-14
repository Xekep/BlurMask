"""Pinned static plain1F42D0 rich WORD/glyph ownership and native resets; no vendor execution."""
import argparse,hashlib,json,pathlib,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
import capstone
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
assert b.qwords(0x1802de610+0x1c0,1)[0]==0x180152f90
assert b.qwords(0x1802de610+0x280,1)[0]==0x180155710
assert b.qwords(0x1802e1ff0+0x1c8,1)[0]==0x18005a400
init=asm(0x1801ede2f,0x1801ede41)
assert 'cmp qword ptr [rcx + 0x1c0], 0' in init and 'setne cl' in init and '[rsi + 0x28], cl' in init
forward=asm(0x180155710,0x180155721);assert '[rax + 0x1c8]' in forward
assert 'xor al, al' in asm(0x18005a400,0x18005a403)
word=f['1801f42d0'];reset=word.index('if (iVar21 == 0) {',word.index('local_320 = 0;',word.index('local_45c = uVar36')))
assert '(lVar24,local_338)' in word[reset:reset+230] and 'uVar27 = local_420' in word[reset:reset+310]
assert 'local_420 = 0xffffffffffffffff' in word[:reset]
assert 'if ((*(byte *)' in word and '& 1) == 0' in word
assert 'local_484 = local_47c' in word and 'local_44c = uVar36' in word and 'uVar36 = local_45c' in word
assert 'fVar42 = (float)(int)local_44c' in word and 'fVar42 / (float)(int)((local_438 + 1) * 0x2000)' in word
assert 'local_328 = -fVar39 * fVar42 * 0.00012207031' in word
assert 'local_320 = CONCAT44((float)(int)uVar26 * 0.00012207031' in word
assert '*(float *)(lVar17 + 0xc)' in word and 'param_2 + 0x98' in word and '(int)local_480 * 0x28) / 255.0' in word
assert '*(undefined4 *)(lVar17 + 0x10) = uVar40' in word and '*(undefined4 *)(lVar17 + 0x14) = uVar40' in word
assert '*(undefined4 *)(lVar19 + 0x1c) = uVar40' in word #13AC80 returns owner64, not glyph40.
normal=f['18000c860'];assert '*(undefined8 *)(param_2 + 0x84) = 0' in normal
clone=f['18013fe20'];assert '*puVar3 = param_4[2]' in clone and 'puVar3[1] = uVar11' in clone and 'uVar11 = param_4[3]' in clone
publisher=f['1801416b0'];assert 'local_148 = &local_124' in publisher and '(param_1,param_5,param_3,&local_120)' in publisher
assert 'local_120 = param_7' in publisher and 'local_124 = param_8' in publisher
score=f['1801c1300'];assert '(ulonglong)param_6 * 4' in score
vector=asm(0x180152f50,0x180152f8a);assert '[r10 + 0x88]' in vector
report=dict(bindings=3,cost14Enabled='1EDDE0 tests nonnull frontend1C0; ordinary222152F90 exists ->23F28=1',
 resets='Trailing count0 span check resets276 before glyphloop; root word publishes then resets276 before separate leading count0; neither endpoint enters selected wordrefs',
 wordScalars='WORD10 logmass from unadjusted terminalcost;14 ctor1;18 unadjusted terminalcost/(sourceNodes*8192);1C acoustic/8192;275 copies all120bytes',
 glyphScalars='GlyphC=primary98[row].likelihood8/255;10=ordinary vector(config88fresh0);14=enabled nativeclass+geometry integercost; owner64.1C separatefirstcandidatevector0',
 sourceSpanOverrides='Word40 first output local120 overwritten param7 ifnot−1; last output local124 overwritten param8 ifnot−1',
 scope='Fresh6C ordinary single word; no traits/aliases/primitive confidence/split words; no vendor execution')
target=pathlib.Path(a.output);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps(report))
