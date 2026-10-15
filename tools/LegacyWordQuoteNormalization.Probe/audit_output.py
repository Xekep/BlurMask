"""Independent scalar139C90 and quote21FBF0 oracle over explicit prepared owners."""
import json,struct,sys
from pathlib import Path
ak,scalar,words=map(Path,sys.argv[1:])
raw=ak.read_bytes();header=struct.unpack_from('<H',raw,105)[0]
assert header==178
def scalar_read(kind,label,s):
    s=bytearray(s)
    if kind==3:
        if not label&0x7ffc:return s
        low=label&3
        if low!=1:struct.pack_into('<H',s,0,{0:501,2:502,3:503}[low])
        s[2]=0;return s
    value=((label&3)-7)&65535 if kind==1 else label if kind in (0,2,4) else 65535
    struct.pack_into('<H',s,0,value);s[2]=int(kind==4)
    return s
actual=scalar.read_bytes();expected=bytearray()
for kind in range(8):
    for label in range(65536):expected.extend(scalar_read(kind,label,bytes.fromhex('c14ea573')))
assert actual==expected,'Scalar fields/residue differ'
p=json.loads(words.read_text());changed=0;word_count=0
def read(row,s):return scalar_read(struct.unpack_from('<H',row,4)[0]&7,struct.unpack_from('<H',row,8)[0],s)
for case in p['Cases']:
    leading=bytes.fromhex('c14ea573');trailing=bytes.fromhex('d25fb684');result=[]
    for word in case['Initial']:
        rows=[bytearray.fromhex(x) for x in word];word_count+=1
        for row in rows:
            leading=read(row,leading);label=struct.unpack_from('<H',leading)[0]
            if leading[2]==0 and label==header:continue
            if leading[2]==0 and label in (193,194):
                struct.pack_into('<H',row,8,{193:134,194:129}[label]);changed+=1
            break
        trailing=read(rows[-1],trailing)
        if trailing[2]==0 and struct.unpack_from('<H',trailing)[0]==193:
            struct.pack_into('<H',rows[-1],8,134);changed+=1
        result.append([r.hex().upper() for r in rows])
    assert result==case['Final']
    assert leading.hex().upper()==case['Leading'] and trailing.hex().upper()==case['Trailing']
assert p['Guards']==6 and len(p['Cases'])==272 and changed>20
print(f'PASS independent524288 scalar outputs/{len(actual)} bytes;{word_count} full linked words/{changed} quote writes, all untouched bytes and retained scratch; prepared data only')
