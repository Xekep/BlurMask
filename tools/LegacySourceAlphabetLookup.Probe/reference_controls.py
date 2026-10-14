"""Original independent sparse/collision table fixtures for native byte lookup."""
import base64,json,struct,sys
from pathlib import Path
def hash_utf8(b):
    h=5381
    for c in b:h=((h*33)&0xffffffff)^c
    return h
def main(path):
    texts=['-','_','alpha','alphabet','a\0b','a\0c','русский','ёж','é','e\u0301','\U000f0002','\U000f0003','\u30fc','\u4e00','\ufffd']
    #Every native hash bucket has an explicitly initialized span. An original
    #extra key is chosen from a collision bucket to exercise unequal length.
    original=[s.encode()for s in texts];target=hash_utf8(b'alpha')&0xf07ff
    for n in range(300000):
        candidate=f'collision{n}'.encode()
        if hash_utf8(candidate)&0xf07ff==target:original.append(candidate);break
    labels={s:i for i,s in enumerate(original)};groups=[[]for _ in range(32768)]
    for value in original:
        h=hash_utf8(value);bucket=(((h>>16)&15)*32+((h>>6)&31))*64+(h&63);groups[bucket].append(value)
    spans=[0];ordered=[]
    for group in groups:ordered.extend(sorted(group));spans.append(len(ordered))
    b=bytearray(struct.pack('<8H',0,0,0,0,0,0,0,len(original)));b.extend(struct.pack('<H',16))
    b.extend(struct.pack('<16H',*(i*32 for i in range(16))));b.extend(struct.pack('<512H',*(i*64 for i in range(512))));b.extend(struct.pack('<32768H',*range(32768)));b.extend(struct.pack('<32769H',*spans))
    strings=bytearray();reverse=[0]*len(original)
    for index,key in enumerate(ordered):b.extend(struct.pack('<II',len(strings),labels[key]));strings.extend(key);reverse[labels[key]]=index
    b.extend(struct.pack('<II',len(strings),0));b.extend(strings);b.extend(struct.pack('<'+'H'*len(reverse),*reverse));b.extend(struct.pack('<BH',0,0))
    #Legacy alphabet parser requires positive per-entry spans; test empty input
    #without inserting an empty stored glyph. Remove it from the fixture.
    queries=original+[b'',b'unknown',b'alpha\0',b'Alpha',b'\xff',b'\x00',b'\xef\xbb\xbf']
    out=Path(path);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(dict(payload=base64.b64encode(b).decode(),queries=[dict(bytes=base64.b64encode(s).decode(),label=labels.get(s,65535))for s in queries]),separators=(',',':'))+'\n').encode());print(len(original),'own keys,',len(queries),'queries')
if __name__=='__main__':main(sys.argv[1])
