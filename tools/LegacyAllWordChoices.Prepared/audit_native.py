"""Pinned full024F30 body and literal flag registers; reads PE bytes only."""
import hashlib,json,pathlib,struct,sys
R,O=map(pathlib.Path,sys.argv[1:]);assert not O.exists()
b=(R/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes()
assert hashlib.sha256(b).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
nt=struct.unpack_from('<I',b,60)[0];n=struct.unpack_from('<H',b,nt+6)[0];opt=struct.unpack_from('<H',b,nt+20)[0]
sections=[struct.unpack_from('<4I',b,nt+24+opt+i*40+8)for i in range(n)]
def read(va,count):
    rva=va-0x180000000
    for vs,base,size,offset in sections:
        if base<=rva and rva+count<=base+size:return b[offset+rva-base:offset+rva-base+count]
    raise AssertionError('PE range')
body=read(0x180024f30,0x180025937-0x180024f30)
assert len(body)==2567 and body[-1]==0xc3
assert hashlib.sha256(body).hexdigest()=='704616d97def59b653a6d47f6d975772e84eaea6aa35877da97cf2482a45ebcd'
pins={0x18002580c:'458b442408',0x180025818:'418bd0',0x18002581f:'410fbaf014',0x18002582f:'0b54c808',0x180025833:'81e200001000',0x180025839:'4133d0',0x18002583f:'4189542408',0x18002574d:'ffc7',0x180025787:'41897c241c'}
for va,hexbytes in pins.items():assert read(va,len(bytes.fromhex(hexbytes)))==bytes.fromhex(hexbytes)
values=[0,1,8,0x100000,0x200000,0x300000,0xffffffff];literal=0;admitted=0
for prior in values:
    for defaults in values:
        for marker in values:
            r8=prior|defaults;edx=r8;r8&=0xffefffff;edx|=marker;edx&=0x100000;edx^=r8
            assert edx==(prior|defaults)|(marker&0x100000);literal+=1
    for marker in values:
        default_or=prior|marker
        assert default_or|(marker&0x100000)==prior|marker;admitted+=1
assert (0x100000|0)==0x100000
O.write_text(json.dumps(dict(scope='Full Ghidra body byte pin and literal register semantics; not transitive source/grammar closure',bodyBytes=2567,bodySHA256=hashlib.sha256(body).hexdigest(),literalControls=literal,admittedControls=admitted,bit20CannotClear=True,vendorExecution=False,modelExecutions=0),indent=2),encoding='utf-8')
print('PASS full2567-byte body/343 literal registers/49 admitted folds/0NN')
