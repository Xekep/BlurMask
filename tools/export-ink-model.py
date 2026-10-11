#!/usr/bin/env python3
"""Export a local English native recognition resource into IKRPACK1.

Pure Python, static parsing only. No SDK DLL is loaded. The user supplies their
own recognition asset; exported parameters are local data and must not be
committed or redistributed. This exporter contains no model parameters.

Format: 8-byte magic, u32 version, u32 body length, SHA256(body), then little-
endian body: dimensions, UTF8 language, feature normalization/resampling,
layers, UTF8 labels, tensor descriptors, original parameter bytes. All native padding and
quantization metadata remain byte-for-byte in the packed parameter payload.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import struct

MAGIC=b'\x90\xfe\x01\x01'
MAX_RESOURCE=64*1024*1024


class Reader:
    def __init__(self,data,start=0,end=None):
        self.data=data;self.pos=start;self.end=len(data) if end is None else end
        if not 0<=start<=self.end<=len(data):raise ValueError('Invalid reader bounds')
    def take(self,n):
        if n<0 or n>self.end-self.pos:raise ValueError('Truncated resource')
        result=self.data[self.pos:self.pos+n];self.pos+=n;return result
    def read(self,fmt):return struct.unpack('<'+fmt,self.take(struct.calcsize('<'+fmt)))[0]
    def u32(self,maximum=0xffffffff):
        value=self.read('I')
        if value>maximum:raise ValueError('Resource count exceeds supported bounds')
        return value
    def zero(self):
        if self.u32()!=0:raise ValueError('Unsupported reserved field')
    def array(self,fmt,count):return list(struct.unpack('<'+str(count)+fmt,self.take(struct.calcsize('<'+fmt)*count)))


def header(data,offset):
    if offset<0 or len(data)-offset<16:raise ValueError('Truncated object header')
    magic,length,kind,flags,version=struct.unpack_from('<4sIIHH',data,offset)
    end=offset+8+length
    if magic!=MAGIC or length<8 or end>len(data):raise ValueError('Invalid object header')
    return dict(offset=offset,end=end,kind=kind,flags=flags,version=version)


def nodes(data):
    root=header(data,0)
    if root['end']!=len(data) or root['flags']:raise ValueError('Expected complete, unencoded English resource')
    result=[];pos=0
    while (pos:=data.find(MAGIC,pos))>=0:
        try:result.append(header(data,pos))
        except ValueError:pass
        pos+=1
    return result


def alphabet(data):
    # These English profiles embed a compact alphabet marker after their common
    # metadata, not the16-byte bounded chunk used by the Math profiles.
    if data[91:99]!=bytes.fromhex('05012b0000000401'):
        raise ValueError('Unsupported English compact alphabet marker')
    r=Reader(data,99)
    fields=r.array('H',8);count=fields[-1]
    if count>65535:raise ValueError('Invalid alphabet count')
    size=r.read('H')
    for increment in [32,64,1]:
        table=r.array('H',size)
        used=[v for v in table if v!=65535]
        if not used:raise ValueError('Empty alphabet hash table')
        size=used[-1]+increment
        if size>1000000:raise ValueError('Alphabet table too large')
    spans=r.array('H',size+1)
    entries=[(r.u32(),r.u32()) for _ in range(spans[-1]+1)]
    strings=r.take(entries[-1][0]);r.array('H',count)
    mapping={}
    for index,(offset,label) in enumerate(entries[:-1]):
        stop=entries[index+1][0]
        if stop<offset or stop>len(strings):raise ValueError('Invalid alphabet string offsets')
        mapping[label&65535]=strings[offset:stop].decode('utf8')
    if r.read('B'):r.take((count+7)//8)
    triplets=r.read('H');r.take(triplets*6)
    extra=r.u32() if triplets else 0
    for bit in [1,2,4]:
        if extra&bit:r.take(count*(1 if triplets<128 else 2))
    r.read('B')
    return mapping


def adapter(data,node):
    if node['flags'] or node['version']!=0x103:raise ValueError('Unsupported text adapter')
    r=Reader(data,node['offset']+16,node['end'])
    if r.u32()!=1:raise ValueError('Only one feature scheme supported')
    mode=r.u32();feature_kind=r.u32()
    if mode!=0 or feature_kind!=4:return None
    network_wrapper=header(data,r.pos)
    if network_wrapper['kind']!=0x53 or network_wrapper['flags']:raise ValueError('Unsupported network wrapper')
    r.pos=network_wrapper['end']
    if r.read('B')!=1:raise ValueError('Expected normalized feature channels')
    mean=r.array('f',13);scale=r.array('f',13)
    if not all(math.isfinite(x) for x in mean+scale):raise ValueError('Invalid feature normalization')
    blank,score_kind,score_representation=[r.u32() for _ in range(3)]
    score_scale=r.read('f');unused=r.u32();spacing=r.read('f')
    min_points,transform,property14=[r.u32() for _ in range(3)]
    property20,link=r.read('B'),r.read('B');property1c=r.read('f')
    count=r.read('H');labels=r.array('H',count)
    if r.pos!=r.end or count!=181 or blank!=181 or score_kind or score_representation:
        raise ValueError('Unsupported text scoring/label profile')
    if transform or property14 or property20 or property1c!=struct.unpack('<f',bytes.fromhex('ffff7f7f'))[0] or unused or link:
        raise ValueError('Unsupported text preprocessing profile')
    if not math.isfinite(spacing) or spacing<=0 or min_points>1000:raise ValueError('Invalid resampling settings')
    return dict(mean=mean,scale=scale,spacing=spacing,min_points=min_points,link=link,blank=blank,
                labels=labels,wrapper=network_wrapper,score_scale=score_scale)


def align(value,multiple=8):return (value+multiple-1)//multiple*multiple


def network(data,node):
    if node['flags'] or node['version']!=0x106:raise ValueError('Unsupported native network')
    r=Reader(data,node['offset']+16,node['end'])
    if r.u32(2)!=1:raise ValueError('Expected one sequential network')
    count=r.u32(16);layers=[]
    for _ in range(count):
        kind,width,flags,code=[r.u32() for _ in range(4)]
        if flags or code not in [1,2,3] or width>2048:raise ValueError('Unsupported layer metadata')
        layers.append(dict(kind=kind,width=width,code=code))
    if count!=4 or layers[0]['kind']!=1 or layers[0]['width']!=13 or any(x['kind']!=8 for x in layers[1:]):
        raise ValueError('Expected English13-channel three-layer BLSTM')
    if r.u32()!=0:raise ValueError('Nonsequential edges unsupported')
    outwidth,outkind,outflags=[r.u32() for _ in range(3)]
    if outwidth!=182 or outkind!=1 or outflags:raise ValueError('Unsupported output')
    r.zero();extra_rows=r.u32();extra_columns=r.u32();r.zero()
    codes=[r.u32() for _ in range(count+1)];global_code=r.u32();r.zero()
    if extra_rows or extra_columns or codes!=[0,1,1,1,3] or global_code!=2:
        raise ValueError('Unsupported activation/lookup metadata')
    alignment=r.u32(1024)*4;payload_size=r.u32(MAX_RESOURCE)
    if alignment!=32:raise ValueError('Unsupported native alignment')
    if any(x!=255 for x in r.take((-r.pos)%alignment)):raise ValueError('Invalid alignment padding')
    payload=r.take(payload_size);tail_size=r.u32(MAX_RESOURCE);r.take(tail_size)
    if r.pos!=r.end:raise ValueError('Unconsumed network fields')
    tensors=[];cursor=0
    def matrix(name,rows,input_width,code):
        nonlocal cursor
        columns=align(align(input_width+1),{1:8,2:16,3:32}[code])
        size=rows*columns*{1:4,2:2,3:1}[code]
        if code!=1:size+=align(rows)*8+32
        tensors.append(dict(name=name,kind=1,offset=cursor,bytes=size,rows=rows,columns=columns,input_width=input_width,code=code))
        cursor+=size
    previous=13
    for index,layer in enumerate(layers[1:],1):
        width=layer['width'];code=layers[index-1]['code']
        for group in ['input','recurrent']:
            for direction in ['forward','backward']:
                for gate in ['g','i','f','o']:
                    matrix(f'layer{index}.{direction}.{group}.{gate}',width,previous if group=='input' else width,code)
                if group=='recurrent':
                    for gate in ['i','f','o']:
                        size=align(width)*4
                        tensors.append(dict(name=f'layer{index}.{direction}.recurrent.p{gate}',kind=2,
                            offset=cursor,bytes=size,rows=width,columns=1,input_width=0,code=1))
                        cursor+=size
        previous=width*2
    matrix('output',outwidth,previous,layers[-1]['code'])
    if cursor!=len(payload):raise ValueError('Tensor layout does not consume parameter payload')
    return dict(input_width=13,output_width=outwidth,layers=layers,output_code=layers[-1]['code'],
                tensors=tensors,payload=payload)


def pack(data):
    objects=nodes(data)
    mapping=alphabet(data)
    candidates=[adapter(data,x) for x in objects if x['kind']==0x21e]
    candidates=[x for x in candidates if x is not None]
    if len(candidates)!=1:raise ValueError('Expected one13-channel text adapter')
    profile=candidates[0];wrapper=profile['wrapper']
    nets=[x for x in objects if x['kind']==0x5f and wrapper['offset']<x['offset']<x['end']<=wrapper['end']]
    if len(nets)!=1:raise ValueError('Expected one native BLSTM in text adapter')
    model=network(data,nets[0]);body=bytearray()
    def u32(*values):body.extend(struct.pack('<'+'I'*len(values),*values))
    def string(value):
        text=value.encode('utf8');u32(len(text));body.extend(text)
    u32(model['input_width'],model['output_width'],profile['blank'],3,len(profile['labels']))
    string('en-US')
    body.extend(struct.pack('<26f',*(profile['mean']+profile['scale'])))
    body.extend(struct.pack('<fII',profile['spacing'],profile['min_points'],profile['link']))
    for index,layer in enumerate(model['layers'][1:],1):u32(layer['width'],1,model['layers'][index-1]['code'])
    u32(model['output_code'])
    for label in profile['labels']:
        if label not in mapping and label!=178:raise ValueError('Unknown output label')
        string(mapping.get(label,''))
    u32(len(model['tensors']))
    for tensor in model['tensors']:
        string(tensor['name'])
        u32(*(tensor[key] for key in ['kind','offset','bytes','rows','columns','input_width','code']))
    u32(len(model['payload']));body.extend(model['payload'])
    result=b'IKRPACK1'+struct.pack('<II',1,len(body))+hashlib.sha256(body).digest()+body
    return result,dict(input_width=13,output_width=182,blank_index=181,labels=181,
                       layers=[x['width'] for x in model['layers'][1:]],tensor_count=len(model['tensors']),
                       native_payload_bytes=len(model['payload']),native_payload_sha256=hashlib.sha256(model['payload']).hexdigest(),
                       packed_sha256=hashlib.sha256(result).hexdigest(),bytes=len(result),
                       feature_normalization='float32 (feature-mean)*scale',special_output178='empty UTF8 token')


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--resource',type=Path,required=True,help='Local en_US-ak-cur.res or en_US-ak-superimposed.res')
    p.add_argument('--output',type=Path,required=True,help='Local private output .ikrpack; keep outside Git')
    args=p.parse_args()
    if args.resource.stat().st_size>MAX_RESOURCE:raise ValueError('Resource exceeds64MiB bound')
    data=args.resource.read_bytes();binary,info=pack(data)
    args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_bytes(binary)
    info.update(source_file=str(args.resource),source_sha256=hashlib.sha256(data).hexdigest(),output=str(args.output))
    args.output.with_suffix('.manifest.json').write_text(json.dumps(info,indent=2),encoding='utf8')
    print(json.dumps(info,indent=2))


if __name__=='__main__':main()
