"""Literal 13F/102 -> 133/100 wire reference. No native execution or selection."""
import hashlib,json,struct,sys
from pathlib import Path

models,capture,out=map(Path,sys.argv[1:4])
sha=lambda data:hashlib.sha256(data).hexdigest()

def reference(data):
    def header(at):
        magic,length,kind,flags,version=struct.unpack_from('<IIIHH',data,at)
        assert magic==0x0101fe90 and length>=8 and flags==0
        end=at+8+length;assert end<=len(data)
        return end,kind,version
    end,kind,version=header(0);assert (end,kind,version)==(len(data),0x13f,0x102)
    # 133 fixed word10/name56/sentinel, optional4C, unconditional50.
    cursor=16+4+56
    sentinel,=struct.unpack_from('<i',data,cursor);cursor+=4
    if sentinel==-1:cursor+=4
    cursor+=4
    prefix=data[16:cursor];count,=struct.unpack_from('<I',data,cursor);cursor+=4
    rows=[]
    for ordinal in range(count):
        start=cursor;end,kind,version=header(start)
        cursor=((end+3)//4)*4 if ordinal!=count-1 else end
        assert cursor<=len(data)
        rows.append(dict(Ordinal=ordinal,Offset=start,End=end,Kind=kind,Version=version,
            PaddingLength=cursor-end,Sha256=sha(data[start:end]),PaddingHex=data[end:cursor].hex()))
    assert cursor==len(data)
    return dict(End=len(data),Version=0x102,Fingerprint=sha(data),
        Base133Length=len(prefix),Base133Sha256=sha(prefix),Children=rows)

def procedural(extended,count):
    data=bytearray(struct.pack('<IIIHH',0x0101fe90,0,0x13f,0,0x102))
    data+=struct.pack('<I',0x12345678)+bytes(range(56))+struct.pack('<i',-1 if extended else 0x1234567)
    if extended:data+=struct.pack('<I',0x76543210)
    data+=struct.pack('<II',0x99aabbcc,count)
    for i in range(count):
        data+=struct.pack('<IIIHH',0x0101fe90,9+i,0xf000+i,0,0x100+i)+bytes((0x40+j)&255 for j in range(i+1))
        if i+1<count:data+=b'\xff'*(-len(data)%4)
    struct.pack_into('<I',data,4,len(data)-8)
    return bytes(data)

actual=json.loads(capture.read_bytes());assert actual['TargetsRead']is False and actual['Guards']==39
inputs={name:(models/name).read_bytes()for name in ['ru-RU/ru_RU-lk-text.lite.res','en-US/en_US-lk-text.lite.res']}
inputs.update({f'procedural-{str(extended)}-{count}':procedural(extended,count)
    for extended in [False,True]for count in [0,1,3]})
inputs['procedural-False-4096']=procedural(False,4096)
assert [r['Name']for r in actual['Rows']]==list(inputs)
for row in actual['Rows']:
    expected=dict(Name=row['Name'],**reference(inputs[row['Name']]))
    assert row==expected,row['Name']
ru=actual['Rows'][0];en=actual['Rows'][1]
assert ru['Fingerprint']=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
assert len(ru['Children'])==22
assert [sum(c['Kind']==kind for c in ru['Children'])for kind in [0x13a,0x13b,0x13d]]==[19,2,1]
assert [c['Offset']for c in ru['Children']if c['Kind']==0x13b]==[606964,4575280]
assert [c['Offset']for c in ru['Children']if c['Kind']==0x13d]==[4575188]
assert sum(c['PaddingHex']=='ffff'for c in ru['Children'])==3
report=dict(status='PASS independent13F wire reference',layouts=len(inputs),guards=actual['Guards'],
    ruChildren=len(ru['Children']),enChildren=len(en['Children']),captureSha256=sha(capture.read_bytes()),
    targetsRead=False,nativeExecuted=False,sourceSelectionImplemented=False,recognitionClaim=False)
out.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
