"""Static owned first sampler kind5 cache/dispatch proof; no DLL loading."""
import argparse,hashlib,json,pathlib,struct,sys
p=argparse.ArgumentParser();p.add_argument('--resource',required=True);p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
s=f['1801941b0'];start=s.index('fVar28 = local_f8;',s.index('pfVar24[3] ='))
blank=s[start:s.index('pfVar24[4] = fVar28;',start)+len('pfVar24[4] = fVar28;')]
assert 'fVar28 = fStack_f0' in blank and '0x10c' in blank
assert '(fVar28 - fVar32) / fVar29 - *(float *)(param_2 + 0x70)' in blank
assert '-*(float *)(param_2 + 0x108)' in blank and 'FUN_18024d8a0(-fVar28 * 8192.0)' in blank
assert 'fVar30 < 67108864.0' in blank and 'pfVar24[4] = fVar28' in blank
s=f['180195c40'];start=s.index("case '\\x05':");case=s[start:s.index('break;',start)]
assert '0x108' in case and '0x10c' in case and case.count('0x188')==2
assert 'iVar10 + 1,0' in case and '0x10 + (ulonglong)*(ushort *)(param_3 + 3) * 0xb8' in case
assert 'return **(int **)(param_3 + 10) < param_3[0xd]' in case
s=f['1801a82a0'];assert '& 8' in s and '< 0' in s and 'pbVar3 == pbVar4' in s and '(*pbVar4 & 0xf) != 2' in s and 'return 0' in s
assert b.qwords(0x1802e1ff0+0x108,1)[0]==0x1801a82a0
raw=pathlib.Path(a.resource).read_bytes();assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<f',raw,12513+33)[0]==-.5 and raw[12513+189]==1
report={'cache10':'1941B0 chooses minX/maxX by10C, subtracts preceding prefixMaxX, dividesheight, subtracts70, multiplies-negative108; rounds float and caps before integer store',
 'firstActual':'sampler297/v106@11506 tail12513; threshold70=-.5,10C=1, usesmaxX',
 'kind5':'195C40 ordinary108 mustreturn0, then10C enforceswholephysicalstarts current and nextnode arc0; adds cache10 only and compareslimit',
 'scope':'Owned pruned firstsame-descriptor unsuppressed/noProtocol/sourceType0; previous geometric ancestor is caller supplied, not produced here',
 'unrestored':'Blank acoustic fallback/proposal insertion, ancestor traversal and complete first controller winner'}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps(report))
