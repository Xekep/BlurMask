"""PE-only proof of helper68 normalization, variant72 fields and04F170 consumers."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary('MyScriptEngine.dll');f=functions('MyScriptEngine.dll')
assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert b.qwords(0x180252188,1)[0]==0x1801dd740 and b.qwords(0x1801dd740+0x20,1)[0]==0x1801dd7a0
assert b.qwords(0x1801dd7a0+0x60,1)[0]==0x180048190 and b.qwords(0x1801dd7a0+0x68,1)[0]==0x180048300
init=f['180031910'];at=init.index('+ 0x68) =');assert 'PTR_DAT_180252188' in init[init.rfind('puVar7 = PTR',0,at):at]
assert 'FUN_180048190' in f['180048300'] and 'FUN_18004aa10' in f['180048300']
decomposition=f['180048190'];assert 'FUN_18004ba10' in decomposition and 'FUN_18004b870' in decomposition
case=f['18004f170'];assert 'pcVar16 = (char *)(lVar9 + 4)' in case and 'pcVar16 = pcVar16 + 8' in case
for token in ['piVar12 = local_144','piVar12 = piVar12 + 0x12','(&local_178)[lVar9 * 9]','(&lStack_168)[lVar9 * 9]','local_3b8 = &local_398','0x22 < iVar8 - 200U']:
 assert token in case,token
#V68 andlaterV60 BOTHreadRAWuppercase local328=[RBP-50], whereas
#theirnormalized/decomposed output isseparatescratch=[RSP+68].
assert b.data[b.off(0x18004fb9e):b.off(0x18004fb9e)+8]==bytes.fromhex('488d55b04c8b45c0')
assert b.data[b.off(0x18004fcce):b.off(0x18004fcce)+8]==bytes.fromhex('488d55b04c8b45c0')
assert b.data[b.off(0x18004fbb7):b.off(0x18004fbb7)+4]==bytes.fromhex('4c8b5068')
assert b.data[b.off(0x18004fce7):b.off(0x18004fce7)+4]==bytes.fromhex('4c8b5060')
variant=f['18005f4c0']
for token in ['param_2 + uVar11 * 0x48','param_2 + 8 + uVar11 * 0x48','param_2 + 0x10 + uVar11 * 0x48','param_2 + 0x18 + uVar11 * 0x48','param_2 + 0x20 + uVar11 * 0x48','param_2 + 0x28 + uVar11 * 0x48','param_2 + 0x30 + uVar11 * 0x48','param_2 + 0x34 + uVar11 * 0x48']:
 assert token in variant,token
#Defaultgrapheme iterator constructor sets tailoring=true, not bare scalar count.
assert b.data[b.off(0x18005fac7):b.off(0x18005fac7)+4]==bytes.fromhex('c6474001')
assert b.qwords(0x180202a70+0x48,1)[0]==0x18005fa80
assert b.qwords(0x180202a70+0x50,1)[0]==0x18005fae0
assert b.qwords(0x180202a70,1)[0]==0x18005ebb0
out={'global68':'031910 PTR252188 ->descriptor1DD740+20 ->runtime1DD7A0',
 'normalization':'V60=048190 canonicaldecomposition/ordering;V68=048300 calls048190 then04AA10 composition',
 'variant72':'lowerptr00/count08,upperptr10/count18,titleptr20/count28,language30,context34',
 'case04F170':'writeslowerandupperonly;bothNFC;RAWuppercasewriterNFD(notnormalizedresult),removeCCC200..234inclusive,thenNFC',
 'poststream':'exactlyoneTAILOREDgrapheme,thenAL.V28 lookup andoptionalrecursion;notportedbythisscalarstage',
 'scope':'validdecodedUnicodescalarstreams/defaultservices;no customcharset,mutablelabelinsertionorrecursiveformclaims;vendorExecutionfalse'}
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf8');print('PASS PE-only helper68V60/V68,72-byte casevariants,04F170 lower/upper/CCCfilter andtailoredgraphemeboundary proof')
