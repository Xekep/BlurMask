"""Generate only synthetic weights and independent reference outputs; no asset reads."""
import argparse, importlib.util, struct, hashlib, json
from pathlib import Path
import numpy as np

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', type=Path, default=Path(__file__).resolve().parent,
                    help='Output directory; default is tools next to this generator')
args = parser.parse_args()
output_directory = args.output_dir.resolve()
output_directory.mkdir(parents=True, exist_ok=True)
spec = importlib.util.spec_from_file_location('ink_reference', Path(__file__).with_name('ink-reference.py'))
reference = importlib.util.module_from_spec(spec)
spec.loader.exec_module(reference)
infer, align = reference.infer, reference.align
rng=np.random.default_rng(20261011)
payload=bytearray();tensors=[]

def matrix(name,rows,width,code,recurrent=False,gate='g'):
    cols=align(align(width+1),{1:8,2:16,3:32}[code]);start=len(payload)
    weights=np.zeros((rows,cols),dtype={1:'<f4',2:'<i2',3:'i1'}[code])
    if code==1:
        weights[:,:width]=rng.uniform(-.18,.18,(rows,width)).astype(np.float32)
        weights[:,width]=0 if recurrent else {'g':3.,'i':3.,'f':.5,'o':3.}[gate]
        scale=np.ones(rows,np.float32)
    elif code==2:
        weights[:,:width]=rng.integers(-300,301,(rows,width),dtype=np.int16)
        weights[:,width]=0 if recurrent else {'g':30000,'i':24000,'f':12000,'o':22000}[gate]
        if not recurrent and gate=='g':weights[:,:width+1]=30000
        scale=np.full(rows,.00004 if not recurrent else .0005,np.float32)
    else:
        weights[:,:width+1]=rng.integers(-24,25,(rows,width+1),dtype=np.int8)
        if recurrent:weights[:,width]=0
        if name=='output':weights[:,width]=np.array([-9,14,2,-3],dtype=np.int8)
        scale=np.linspace(.025,.04,rows,dtype=np.float32)
    payload.extend(weights.tobytes())
    meta=np.zeros((align(rows),2),np.float32)
    meta[:rows,0]=scale;meta[:rows,1]=weights.astype(np.int64).sum(axis=1).astype(np.float32)
    aux=np.zeros(8,np.float32);aux[0]=32767 if code==2 else 0
    if code!=1:payload.extend(meta.astype('<f4').tobytes());payload.extend(aux.astype('<f4').tobytes())
    tensors.append(dict(name=name,kind=1,offset=start,bytes=len(payload)-start,rows=rows,columns=cols,input_width=width,code=code))
    return dict(weights=weights,metadata=meta[:rows],aux=aux,width=width,code=code,name=name)

def peephole(name,width):
    start=len(payload);values=np.zeros(align(width),np.float32)
    values[:width]=rng.uniform(-.06,.06,width).astype(np.float32)
    payload.extend(values.astype('<f4').tobytes())
    tensors.append(dict(name=name,kind=2,offset=start,bytes=len(payload)-start,rows=width,columns=1,input_width=0,code=1))
    return values[:width]

specs=[(9,1),(2,2),(2,3)];layers=[];previous=13
for index,(width,code) in enumerate(specs,1):
    layer=dict(width=width,bidirectional=True)
    for group in ['input','recurrent']:
        for direction in ['forward','backward']:
            prefix=f'layer{index}.{direction}.{group}'
            layer[direction+'_'+group]={g:matrix(prefix+'.'+g,width,previous if group=='input' else width,code,group=='recurrent',g) for g in ['g','i','f','o']}
            if group=='recurrent':
                for g in ['i','f','o']:layer[direction+'_'+group]['p'+g]=peephole(prefix+'.p'+g,width)
    layers.append(layer);previous=width*2
output=matrix('output',4,previous,3)
network=dict(input_width=13,layers=layers,output=output)
body=bytearray()
def u32(*v):body.extend(struct.pack('<'+'I'*len(v),*v))
def text(s):b=s.encode('utf8');u32(len(b));body.extend(b)
u32(13,4,3,3,3);text('en-US')
body.extend(struct.pack('<26f',*([0.]*13+[1.]*13)));body.extend(struct.pack('<fII',.05,7,0))
for width,code in specs:u32(width,1,code)
u32(3)
for label in ['a','b',' ']:text(label)
u32(len(tensors))
for t in tensors:
    text(t['name']);u32(*(t[k] for k in ['kind','offset','bytes','rows','columns','input_width','code']))
u32(len(payload));body.extend(payload)
binary=b'IKRPACK1'+struct.pack('<II',1,len(body))+hashlib.sha256(body).digest()+body
(output_directory/'ink-reference-fixture.bin').write_bytes(binary)
features=rng.normal(0,.3,(4,13)).astype(np.float32)
features[0,0]=-.75;features[1,1]=.875;features[2,2]=0
probabilities,states=infer(network,features)
expected=dict(format='IKRPACK1 synthetic reference v1',origin='Entirely synthetic deterministic weights; no recognition resources were read.',
             seed=20261011,modelSha256=hashlib.sha256(binary).hexdigest(),frames=4,inputWidth=13,outputWidth=4,
             features=features.ravel().tolist(),probabilities=probabilities.ravel().tolist(),tolerance=.000001,
             layers=[dict(width=w,storage=c,bidirectional=True,values=s.ravel().tolist()) for (w,c),s in zip(specs,states)])
(output_directory/'ink-reference-expected.json').write_text(json.dumps(expected,indent=2)+'\n',encoding='utf8')
print(json.dumps({k:expected[k] for k in ['modelSha256','frames','inputWidth','outputWidth','probabilities']},indent=2))
print('bytes',len(binary),'tensors',len(tensors))
