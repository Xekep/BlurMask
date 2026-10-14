"""Static E4/zero-trait source proof. Reads bytes/C exports, never loads DLLs."""
import argparse,hashlib,json,pathlib,struct,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--resource',required=True);p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions,parse_alphabet
text=Binary();engine=Binary('MyScriptEngine.dll');f=functions();e=functions('MyScriptEngine.dll')
assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
def asm(binary,start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(binary.data[binary.off(start):binary.off(end)],start))
assert '*(undefined8 *)(param_2 + 0x88) = 0;' in e['180052260']
reader=e['180053f20'];assert 'FUN_1800573b0' in reader
build=e['1800573b0'];assert build.index('if (uVar6 == 0)')<build.index('param_2[0x11] = lVar10')
assert 'return 1;' in build[build.index('if (uVar6 == 0)'):build.index('param_2[0x11] = lVar10')]
scan=e['18004e280'];assert 'iVar4 != 5' in scan and 'iVar2 - 0x660U < 10' in scan and 'iVar2 - 0x6f0U < 10' in scan
lookup=asm(engine,0x18005f290,0x18005f2d7)
assert '18005f29a mov r9d, 0x2cc' in lookup and '18005f2ce xor eax, eax' in lookup
#Native virtual30 table target and actual716 ranges, all codepoints checked.
assert engine.qwords(0x180202a70+0x30,1)[0]==0x18005f290
ranges=[struct.unpack_from('<III',engine.data,engine.off(0x1801fb6d0)+12*i) for i in range(716)]
assert all(x<=y for x,y,k in ranges) and all(ranges[i][1]<ranges[i+1][0] for i in range(715))
raw=pathlib.Path(a.resource).read_bytes();alphabet=parse_alphabet(raw,6219)
#The generic private parser reads the104 final flag; this inline node is103.
payload=raw[6219:alphabet['end']-1];alhash=hashlib.sha256(payload).hexdigest()
assert alhash=='2997884bbaaa094929249d1af7e88f285f16ee0e5345ab9013455b66a18eb6a8'
codepoints=sorted({ord(c) for value in alphabet['mapping'].values() for c in value})
def script(cp):return next((k for start,end,k in ranges if start<=cp<=end),0)
assert all(script(cp)!=5 and not(0x660<=cp<=0x669 or 0x6f0<=cp<=0x6f9) for cp in codepoints)
assert alphabet['extra_flags']==1 and len(alphabet['triplets'])==59
v68=asm(engine,0x180052640,0x18005264d)
assert 'cmp qword ptr [rdx + 0x88], rax' in v68 and 'setne al' in v68
init=f['1801edde0'];assert '(code *)&LAB_1801eddb0' in init and 'iVar6 == 0' in init
zero=asm(text,0x1801eddb0,0x1801eddd7)
assert zero.count('mov byte ptr')==5 and zero.count(', 0')==5 and 'mov al, 1' in zero
#194170 publishes190/191true; first flag50 makes192true, not a guessed mode.
assert text.qwords(0x1802e1490+0x78,1)[0]==0x180194170
flags=asm(text,0x180194170,0x1801941a1);assert 'mov word ptr [r8], 0x101' in flags
#First23f subtype check produces graph-init byte5, copied to the sampler's
#init byte1 then210. This is distinct from serialized297+50/graph192.
assert '0x23f' in f['180150d00'] and "CONCAT15(cVar4 != '\\0',(undefined5)local_a8)" in f['180150d00']
assert 'local_37 = param_3[5]' in f['1801a6980']
assert '*(undefined1 *)(param_2 + 0x210) = *(undefined1 *)(param_3 + 1)' in f['18018d920']
assert text.qwords(0x1802e1490+0xa0,1)[0]==0x180195650
assert text.qwords(0x1802e1490+0xa8,1)[0]==0x180195c40
assert "*(char *)(param_2 + 0x210) == '\\0'" in f['180195650'] and f['180195650'].rstrip().endswith('}')
assert 'return true;' in f['180195650'] and "case '\\x06':" not in f['180195c40']
first=f['1801ef070'];assert 'uVar47 == *(ushort *)(param_2 + 0xe4)' in first
assert "if ((uVar47 == *(ushort *)(param_2 + 0xe2)) ||\n                                           (uVar47 == *(ushort *)(param_2 + 0xe4)))" in first
assert 'FUN_1801ee030' in first and 'local_310 = local_310 & 0xff00' in first
assert 'local_310 = CONCAT11(local_310._1_1_,1)' in first
report=dict(alphabetPayloadSha256=alhash,codepointCount=len(codepoints),rangeCount=716,
 derivedArabic88=False,storedRelatedTriplets=59,storedRelationMap78=True,
 zeroTraits='052260 NULL88 ->573B0 noArabic returns beforeallocation ->52640 false ->1EDDB0 five zero bytes',
 correction='1EF070 E4 onlykind0->6;194170 graph191 true;first210true makes195650 unchanged;195C40 no case6 unchanged',
 output='Kind6 changes callback request; 1EE030 pathbit still0. B0 extendsE4, B8 kind0 E2/E4 preservesprior18; kind2 keepsboundary identity',
 scope='Fresh audited AL103 E0false, no shadda/blank/ancestor-skipping/alias/protocol restoration; no language/wholewinner claim')
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,separators=(',',':'))+'\n',encoding='utf-8');print(json.dumps(report))
