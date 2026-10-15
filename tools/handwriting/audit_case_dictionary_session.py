"""Static selected292 metadata/restart bridge, not dynamic vendor execution."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise ValueError('output.json')
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert '+ 0x70))(local_18)' in f['180236d20'] and '*param_1 = uVar1' in f['180236d20']
# R9 retains the complete input source tag across child lookup/virtual70 call;
# outputU32 is separately stamped from input byte4.
assert b.data[b.off(0x180236d2a):b.off(0x180236d2a)+3]==bytes.fromhex('498bc1')
assert b.data[b.off(0x180236d79):b.off(0x180236d79)+2]==bytes.fromhex('891f')
assert '*(undefined1 *)(param_3 + 4) = 0' in f['180236b50'] and 'uVar3 >> 0x28 & 4' in f['180236b50']
assert '*(byte *)(param_3 + 5) & 4' in f['180233f70'] and '& 0x7c' in f['180233f70']
assert 'param_3 + 0x9030' in f['180234970'] and 'param_4 >> 0x3a' in f['180234970']
assert 'param_2 + 0x58' in f['180093f80'] and '0x1e00' in f['180093f80'] and '0x800' in f['180093f80']
out={'metadata':'236D20 passesunchangedtag+endingsymbol+boundarytochild70;outputU32=selectedsourceID',
 'restart':'236B50 clearsinputbyte4,233F70 copiescasebits58..62 onlyifinputheader05.bit2;stampoutputbyte4onlyifreturnedheader05.bit2',
 'refresh':'234840 orderedcasepropagation then093F80 restorespreserved58 beforefiltering50;backendsharednormal/aux',
 'scope':'onepreparedselected292dictionary;container90,C4,restartpair/sidecostflag explicit;controllerfactory/epochs/winner notclaimed'}
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,indent=2)+'\n',encoding='utf8')
print('PASS pinned selected-source metadata236D20/restart236B50->233F70 andpermissionrefresh scope; staticonly')
