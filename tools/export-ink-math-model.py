"""Export a user-supplied Math2 v103 .res to private IKRMATH1 (stdlib only).

Does not download assets, load vendor DLLs or modify activation. Only the
observed ordinary-pen profile is accepted. Do not redistribute vendor weights.
"""
import argparse, hashlib, json, struct, zlib
from pathlib import Path

MAGIC=b'\x90\xfe\x01\x01'; MASK=(1<<64)-1
def header(data,pos=0):
    if pos<0 or pos+16>len(data):raise ValueError('Truncated container')
    magic,size,kind,flags,version=struct.unpack_from('<4sIIHH',data,pos)
    end=pos+8+size
    if magic!=MAGIC or size<8 or end>len(data):raise ValueError('Invalid container')
    return kind,flags,version,end

def tables(kind,version):
    state=kind^version;result=[]
    for _ in range(5):
        t=list(range(256));remaining=256
        for i in range(256):
            if remaining<=1:break
            if t[i]!=i:continue
            state=(state+0x9e3779b97f4a7c15)&MASK
            v=((state^(state>>30))*0xbf58476d1ce4e5b9)&MASK
            v=((v^(v>>27))*0x94d049bb133111eb)&MASK
            index=(v^(v>>31))%remaining
            if not index:remaining-=1;continue
            j=i
            while index:
                j+=1
                if t[j]==j:index-=1
            t[i],t[j]=j,i;remaining-=2
        result.append(t)
    return result

def unwrap(data):
    kind,flags,version,end=header(data)
    if (kind,version)!=(0xeed,0x103) or flags not in (0,3) or end!=len(data):raise ValueError('Unsupported Math2 resource')
    if not flags:return data
    t0,t1,t2,t3,t4=tables(kind,version);output=bytearray(len(data)-16)
    for i,value in enumerate(data[16:]):
        a=i&255;b=(i>>8)&255;c=(i>>16)&255
        x=t1[(t0[value]+a)&255];x=t2[(x+b)&255];x=t3[(x+c)&255]
        x=t3[t4[x]];x=t2[(x-c)&255];x=t1[(x-b)&255];output[i]=t0[(x-a)&255]
    stream=zlib.decompressobj(-15);plain=stream.decompress(output,64*1024*1024)
    if not stream.eof or stream.unused_data or stream.unconsumed_tail:raise ValueError('Invalid/oversized compressed Math2 payload')
    return MAGIC+struct.pack('<IIHH',len(plain)+8,kind,0,version)+plain

class Reader:
    def __init__(self,data,pos,end):self.data=data;self.pos=pos;self.end=end
    def read(self,fmt,count=1):
        size=struct.calcsize('<'+fmt)*count
        if count<0 or self.pos+size>self.end:raise ValueError('Truncated field')
        v=struct.unpack_from('<'+fmt*count,self.data,self.pos);self.pos+=size
        return v[0] if len(v)==1 else list(v)
    def bytes(self,size):
        if size<0 or self.pos+size>self.end:raise ValueError('Truncated bytes')
        v=self.data[self.pos:self.pos+size];self.pos+=size;return v

def parse(data):
    data=unwrap(data);kind,flags,version,end=header(data,90)
    if (kind,flags,version)!=(0x2b,0,0x104):raise ValueError('Unsupported alphabet layout')
    r=Reader(data,106,end);fields=r.read('H',8);count=fields[-1];n=r.read('H')
    if count!=299:raise ValueError('Unsupported alphabet count')
    for increment in (32,64,1):
        table=r.read('H',n);n=next(v+increment for v in reversed(table) if v!=65535)
    spans=r.read('H',n+1);entries=[r.read('II') for _ in range(spans[-1]+1)]
    strings=r.bytes(entries[-1][0]);r.read('H',count);mapping={}
    for j,(offset,label) in enumerate(entries[:-1]):mapping[label&65535]=strings[offset:entries[j+1][0]].decode('utf8')
    if r.read('B'):r.bytes((count+7)//8)
    triplets=r.read('H');r.read('H',triplets*3);extra=r.read('I') if triplets else 0
    for flag in (1,2,4):
        if extra&flag:r.read('B' if triplets<128 else 'H',count)
    r.read('B')
    if r.pos!=end:raise ValueError('Alphabet trailing data')
    r.end=len(data)
    if r.read('H')!=297:raise ValueError('Unsupported vocabulary size')
    kind,flags,version,end=header(data,r.pos)
    if (kind,flags,version)!=(0xc6d,0,0x102):raise ValueError('Unsupported pipeline')
    r.pos+=16;pipeline=json.loads(r.bytes(r.read('I')))
    expected={'input_processing_pipeline':{'preprocessor_array':[{'stroke_reorder_cogx':{}},{'scaler_group_extrema':{'quantil':0.5}},{'normalizer':{}},{'resampler':{'has_interstrokes':False,'resampling':0.4,'version':1}}],'features':{'scheme':['symbols','math7','knn6Prox']}}}
    if pipeline!=expected or r.pos!=end or r.read('I')!=0:raise ValueError('Unsupported feature/decoder profile')
    models=[]
    for i in range(4):
        kind=r.read('I')
        if i==3:
            if kind:raise ValueError('Unsupported fourth graph')
            continue
        k,f,v,e=header(data,r.pos)
        if kind!=0x51 or (k,f,v)!=(0x51,0,0x100):raise ValueError('Unsupported graph container')
        r.pos+=16;r.read('Q');size=r.read('Q');model=r.bytes(size)
        if r.pos!=e or model[4:8]!=b'ORTM':raise ValueError('Invalid graph')
        models.append(model)
    if r.pos!=len(data):raise ValueError('Trailing Math2 bytes')
    labels=[mapping[i] for i in range(297)]
    if labels[:2]!=['[bos]','[eos]']:raise ValueError('Unsupported special tokens')
    return labels,models

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('resource',type=Path);p.add_argument('--output',required=True,type=Path);a=p.parse_args()
    if a.resource.stat().st_size>64*1024*1024:raise ValueError('Resource too large')
    source=a.resource.read_bytes();labels,models=parse(source)
    body=bytearray(struct.pack('<IIIII',26,297,0,1,1))
    for label in labels:
        b=label.encode('utf8');body.extend(struct.pack('<I',len(b)));body.extend(b)
    body.extend(struct.pack('<I',3))
    for model in models:body.extend(struct.pack('<I',len(model)));body.extend(model)
    pack=b'IKRMATH1'+struct.pack('<II',1,len(body))+hashlib.sha256(body).digest()+body
    a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_bytes(pack)
    report=dict(source=str(a.resource),source_sha256=hashlib.sha256(source).hexdigest(),output=str(a.output),sha256=hashlib.sha256(pack).hexdigest(),bytes=len(pack),graphs=[dict(bytes=len(m),sha256=hashlib.sha256(m).hexdigest()) for m in models],profile='ordinary pen / symbols297+math7+knn6Prox / greedy')
    a.output.with_suffix('.provenance.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report,indent=2))

if __name__=='__main__':main()
