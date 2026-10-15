"""Pinned-PE audit of later-word actual query/source70/history dispatch. No DLL execution."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f=functions();pins={
0x180204d7b:'4d8b4530', #source30, actual query tag and ending symbol
0x180204d8b:'4c8bce',0x180204d91:'664489642420',0x180204d9a:'4c8b5070',
0x180204da5:'0f1038', #whole source70 token in XMM7
0x180205bcd:'0f11bd88010000',0x180205c00:'4c8d8d70010000',
0x180205bf6:'889594010000', #ONLY boundary byte24 overwritten in history request
0x180205cb9:'0f11bdd8010000',0x180205ca0:'4c8d8dc0010000',
0x1802061d4:'0f11bdd8010000', #fallback token also keeps metadata category
0x1801003e4:'458b4e20',0x1801003ec:'e8fff8ffff',
0x180100db0:'458b4d20',0x180100dbf:'e8fcf0ffff',
0x180094059:'0f1101', #ordinary metadata starts zero; case supplies category
0x180203987:'440fb7b60a230000',0x180203993:'488b86f0230000',
0x1802039b0:'66443bf1',0x180203c59:'6641ffc6', #context+C enumerates [230A,23F0)
}
for address,expected in pins.items():assert b.data[b.off(address):b.off(address)+len(bytes.fromhex(expected))].hex()==expected,hex(address)
assert b.qwords(0x1802de610+0xe8,1)[0]==0x180152280
assert b.qwords(0x1802de610+0x280,1)[0]==0x180155710
assert b.qwords(0x1802e1ff0+0x1c8,1)[0]==0x18005a400
assert b.data[b.off(0x18005a400):b.off(0x18005a400)+3].hex()=='32c0c3'
main=f['180204290'];boundary=f['180152280']
assert 'FUN_180203690(local_320,param_2,local_305,0)' in main
assert 'local_128 = CONCAT44(uVar49,uVar48)' in main
assert 'local_128._5_3_' in main and '0xffffff01ffffffff' in main
assert 'local_314 = iVar13 - 1;' in main and 'if (local_314 == 0xffffffff)' in main
assert '*(undefined4 *)(lVar3 + 0x10)' in boundary
assert '0x68' in boundary and '0x78' in boundary
report=dict(textSha256=hashlib.sha256(b.data).hexdigest(),instructionPins=len(pins),vtablePins=3,
    actualQuery='204D7B/204D8B/204D91: source30.V70(full query tag, query symbol, boundary0)',
    category='205BCD history request20=source70+8;205BF6 only replaces boundary24;100090 reads20 directly',
    scope='Prepared actual query32 and later rows; owning case model/span/context lease, no scheduler/winner or positive RU V48 claim',
    ordinaryFrontend='222.V280 ->155710 ->ordinary21B.V1C8 ->05A400 false',
    descriptorLine='152280 graph68 group ->graph78 input descriptor ->218.V10 ->descriptor INT32+10')
if len(sys.argv)!=2:raise ValueError('report-output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,separators=(',',':'))+'\n')
print('PASS',len(pins),'instruction pins +3 vtable bindings; actualquery metadata/category/history/source-line proof; no vendor execution')
