"""04F170 poststream lookup and recursive consistency, default stored-readable AL."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary('MyScriptEngine.dll');f=functions('MyScriptEngine.dll');assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert [b.qwords(0x1801de1b0+off,1)[0] for off in (0x28,0x30,0x38)]==[0x1800527e0,0x180052a80,0x180052650]
#Readable AL V38: source reverse slotFFFF marks virtual, independent oftext.
assert b.data[b.off(0x18005265f):b.off(0x180052667)].hex()=='66423914490f94c0'
lookup=f['1800527e0'];assert 'uVar11 = 0x1505' in lookup and 'uVar11 * 0x21 ^' in lookup and 'uVar5 = *(undefined2 *)(lVar8 + uVar15 * 8 + 4)' in lookup
gate=f['180050400'];assert '*param_2 + 0x38' in gate and 'iVar2 != 0x18' in gate and 'iVar2 != 0' in gate
case=f['18004f170']
for token in ['local_380 == sVar4','local_380 == sVar1','(short)local_398 != *psVar17','local_398._2_2_ != psVar17[1]','psVar17[1] = -1','psVar17[2] = -1','*psVar17 = -1']:
 assert token in case,token
assert b.data[b.off(0x18004ffc1):b.off(0x18004ffc6)].hex()=='c644242800'
data=json.loads((root/'models/text-unicode/legacy-case-data.json').read_text(encoding='utf8'))
assert [0x1c4,0x1c4] in data['alternatingRanges'] and [0x1c4,2] in data['lowerSingles']
out={'readableAL':'1DE1B0 V28=0527E0 exactUTF8hashlookup,V30=052A80 copytext,V38=052650 reverseFFFFvirtual',
 'gate':'050400 decodedtextcase alternatives andscript!=24,independentstoredrelationmaps',
 'clusters':'04F170 requiresoneentiredefaulttailoredcluster beforelookup;no insertionofmissingforms',
 'recursion':'wheninput==lower,upperinverseLower mustequal lower elseclearupper+stripped;wheninput==upper,lowerinverseUppermismatch clearsloweronly',
 'nativequirk':'alternating01C4..01C4 lower01C5 overrides lowerSingle01C4+2;01C6inversecheckcaninvalidateupper',
 'scope':'ownedreadableAL successfuldefaultservice branch;mutableALinsertion/failureABI unchanged;vendorExecutionfalse'}
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf8');print('PASS static readableAL lookup/virtual gate,04F170 recursive consistency andnative01C4 precedence facts')
