"""Rebuild the tiny offline single-symbol classifier; training dependencies are not shipped.
Data cache: .artifacts/ai-data. Model: src/BlurMask/Assets/AiPen.bin (plain float32 weights).
CoMNIST letters: CC BY-SA 4.0, Gregory Vial; BHMSDS: MIT, Wojciech Blachowski.
Additional mathematical signs use original vector drawings below, not a downloaded font.
"""
import argparse, hashlib, io, json, math, random, struct, tarfile, time, zipfile
from pathlib import Path
from urllib.request import urlretrieve
import numpy as np
from PIL import Image, ImageDraw
import torch
from torch import nn
from torch.nn import functional as F

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / '.artifacts/ai-data'
COMMON = dict(zip('АВЕКМНОРСТХ', 'ABEKMHOPCTX'))
def label_of(value): return COMMON.get(value.upper(), value.upper())
def normalize(image):
    if image.mode == 'RGBA':
        base=Image.new('RGBA',image.size,'white'); base.alpha_composite(image); image=base
    array=255-np.array(image.convert('L'),dtype=np.uint8)
    ys,xs=np.nonzero(array>45)
    if len(xs)<3: return None
    crop=Image.fromarray(array[ys.min():ys.max()+1,xs.min():xs.max()+1])
    ratio=24/max(crop.size); size=tuple(max(1,round(n*ratio)) for n in crop.size)
    crop=crop.resize(size,Image.Resampling.LANCZOS)
    result=np.zeros((32,32),np.uint8); x=(32-size[0])//2; y=(32-size[1])//2
    result[y:y+size[1],x:x+size[0]]=np.array(crop)
    return result

# Independent stroke paths, allowing pen-width, point jitter and rotation augmentation.
def arc(cx,cy,rx,ry,a,b,n=32):
    return [(cx+rx*math.cos(t),cy+ry*math.sin(t)) for t in np.linspace(a,b,n)]
VECTORS={
 '=':[[(4,10),(28,10)],[(4,22),(28,22)]],
 '×':[[(5,5),(27,27)],[(27,5),(5,27)]],
 '÷':[[(3,16),(29,16)],arc(16,5,1,1,0,2*math.pi),arc(16,27,1,1,0,2*math.pi)],
 '±':[[(4,12),(28,12)],[(16,2),(16,22)],[(4,29),(28,29)]],
 '<':[[(27,3),(5,16),(27,29)]], '>':[[(5,3),(27,16),(5,29)]],
 '≤':[[(27,2),(5,13),(27,24)],[(5,29),(27,29)]],
 '≥':[[(5,2),(27,13),(5,24)],[(5,29),(27,29)]],
 '≠':[[(4,10),(28,10)],[(4,22),(28,22)],[(23,2),(9,30)]],
 '√':[[(2,18),(8,26),(14,5),(30,5)]],
 '∞':[[(16+13*math.sin(t),16+8*math.sin(2*t)) for t in np.linspace(0,2*math.pi,65)]],
 'π':[[(2,8),(30,8)],[(10,8),(9,27),(5,29)],[(24,8),(24,27),(28,29)]],
 '∑':[[(28,3),(5,3),(18,16),(5,29),(28,29)]],
 '∫':[arc(22,6,6,4,math.pi*.95,math.pi*1.85),[(16,6),(14,26)],arc(8,26,6,4,0,math.pi*.9)],
 '%':[[(26,3),(6,29)],arc(7,7,4,5,0,2*math.pi),arc(25,25,4,5,0,2*math.pi)],
 '(':[arc(23,16,13,14,math.pi*.6,math.pi*1.4)],
 ')':[arc(9,16,13,14,-math.pi*.4,math.pi*.4)],
 '[':[[(23,2),(8,2),(8,30),(23,30)]], ']':[[(9,2),(24,2),(24,30),(9,30)]],
 '{':[[(25,2),(16,2),(14,5),(14,11),(10,16),(14,21),(14,27),(16,30),(25,30)]],
 '}':[[(7,2),(16,2),(18,5),(18,11),(22,16),(18,21),(18,27),(16,30),(7,30)]],
 '^':[[(4,23),(16,7),(28,23)]],
 '*':[[(16,2),(16,30)],[(4,7),(28,25)],[(28,7),(4,25)]],
}
def vector_image(paths,rng):
    im=Image.new('L',(128,128),255); draw=ImageDraw.Draw(im)
    angle=rng.uniform(-.14,.14); sx=rng.uniform(.85,1.15); sy=rng.uniform(.85,1.15)
    width=rng.uniform(1.0,3.0)*4
    for path in paths:
        coords=[]
        for x,y in path:
            x=(x-16)*sx; y=(y-16)*sy
            coords.append(((x*math.cos(angle)-y*math.sin(angle)+16+rng.uniform(-.5,.5))*4,
                           (x*math.sin(angle)+y*math.cos(angle)+16+rng.uniform(-.5,.5))*4))
        draw.line(coords,fill=0,width=round(width),joint='curve')
        for x,y in (coords[0],coords[-1]): draw.ellipse((x-width/2,y-width/2,x+width/2,y+width/2),fill=0)
    return normalize(im)

class Network(nn.Module):
    def __init__(self,classes):
        super().__init__()
        self.c1=nn.Conv2d(1,12,3); self.c2=nn.Conv2d(12,24,3)
        self.f1=nn.Linear(864,96); self.f2=nn.Linear(96,classes)
    def forward(self,x):
        x=F.max_pool2d(F.relu(self.c1(x)),2)
        x=F.max_pool2d(F.relu(self.c2(x)),2)
        x=F.relu(self.f1(x.flatten(1)))
        return self.f2(x)

def main():
    parser=argparse.ArgumentParser(); parser.add_argument('--epochs',type=int,default=14)
    parser.add_argument('--device',default='cpu',choices=['cpu','cuda']); args=parser.parse_args()
    torch.set_num_threads(4); torch.manual_seed(417); np.random.seed(417)
    rng=random.Random(417); CACHE.mkdir(parents=True,exist_ok=True)
    sources={
      'Cyrillic.zip':'https://raw.githubusercontent.com/GregVial/CoMNIST/3716a52d7b7571e71b347039b167ea3359d14e7e/images/Cyrillic.zip',
      'Latin.zip':'https://raw.githubusercontent.com/GregVial/CoMNIST/3716a52d7b7571e71b347039b167ea3359d14e7e/images/Latin.zip',
      'math.tar.gz':'https://codeload.github.com/wblachowski/bhmsds/tar.gz/07d208e41923b061e338c241e59daacdd2752d53'}
    for name,url in sources.items():
        if not (CACHE/name).exists(): urlretrieve(url,CACHE/name)
    cache=CACHE/'normalized-v1.npz'
    if cache.exists():
        data=np.load(cache); images=data['images']; names=data['names']; origin=data['origin']
    else:
        samples=[]; names=[]; origin=[]
        for name in ('Cyrillic.zip','Latin.zip'):
            with zipfile.ZipFile(CACHE/name) as z:
                for item in z.namelist():
                    if not item.endswith('.png'): continue
                    image=normalize(Image.open(io.BytesIO(z.read(item))))
                    if image is None: continue
                    samples.append(image); names.append(label_of(item.split('/')[1])); origin.append('letters')
        with tarfile.open(CACHE/'math.tar.gz') as archive:
            for item in archive.getmembers():
                if '/symbols/' not in item.name or not item.name.endswith('.png'): continue
                label=Path(item.name).name.rsplit('-',1)[0]
                label={'plus':'+','minus':'−','slash':'/','dot':'·'}.get(label,label_of(label))
                image=normalize(Image.open(archive.extractfile(item)))
                if image is None: continue
                samples.append(image); names.append(label); origin.append('basic-math')
        for name,paths in VECTORS.items():
            for _ in range(700):
                samples.append(vector_image(paths,rng)); names.append(name); origin.append('vector-math')
        images=np.stack(samples); names=np.array(names); origin=np.array(origin)
        np.savez_compressed(cache,images=images,names=names,origin=origin)
    labels=sorted(set(names)); label_ids={n:i for i,n in enumerate(labels)}
    rng=random.Random(417) # The train/test split is identical with and without the normalized cache.
    y=np.array([label_ids[n] for n in names]); train=[]; test=[]
    for label in range(len(labels)):
        ids=np.flatnonzero(y==label); rng.shuffle(ids); split=max(1,len(ids)//10)
        test.extend(ids[:split]); train.extend(ids[split:])
    # Sample-level split only; datasets do not expose reliable writer IDs.
    device=torch.device(args.device)
    if device.type=='cuda': torch.cuda.set_per_process_memory_fraction(.25)
    x=torch.from_numpy(images[:,None].astype(np.float32)/255).to(device)
    targets=torch.from_numpy(y).to(device); train=torch.tensor(train,device=device); test=torch.tensor(test,device=device)
    model=Network(len(labels)).to(device); optimizer=torch.optim.AdamW(model.parameters(),lr=.002,weight_decay=.001)
    print(json.dumps({'samples':len(images),'classes':len(labels),'device':str(device),'labels':labels},ensure_ascii=True),flush=True)
    best=0; best_state=None
    for epoch in range(args.epochs):
        start=time.monotonic(); model.train(); order=train[torch.randperm(len(train),device=device)]
        if epoch==args.epochs*2//3:
            for group in optimizer.param_groups: group['lr']=.0006
        for ids in order.split(256):
            batch=x[ids]
            theta=torch.zeros((len(ids),2,3),device=device)
            angle=(torch.rand(len(ids),device=device)-.5)*.22
            scale=.94+torch.rand(len(ids),device=device)*.12
            theta[:,0,0]=torch.cos(angle)*scale; theta[:,0,1]=-torch.sin(angle)*scale
            theta[:,1,0]=torch.sin(angle)*scale; theta[:,1,1]=torch.cos(angle)*scale
            theta[:,:,2]=(torch.rand((len(ids),2),device=device)-.5)*.1
            batch=F.grid_sample(batch,F.affine_grid(theta,batch.shape,align_corners=False),align_corners=False)
            optimizer.zero_grad(set_to_none=True); loss=F.cross_entropy(model(batch),targets[ids]); loss.backward(); optimizer.step()
            if device.type=='cuda': time.sleep(.008) # Keep the shared desktop GPU responsive.
        model.eval()
        with torch.no_grad(): prediction=torch.cat([model(x[ids]).argmax(1) for ids in test.split(512)])
        accuracy=(prediction==targets[test]).float().mean().item()
        print(json.dumps({'epoch':epoch+1,'accuracy':accuracy,'seconds':round(time.monotonic()-start,2)}),flush=True)
        if accuracy>best:
            best=accuracy; best_state={k:v.detach().cpu().clone() for k,v in model.state_dict().items()}
    model.load_state_dict(best_state); model.eval()
    with torch.no_grad(): logits=torch.cat([model(x[ids]) for ids in test.split(512)]); probabilities=logits.softmax(1); confidence,prediction=probabilities.max(1)
    actual=targets[test]; correct=prediction==actual; accepted=confidence>=.75
    report={'sample_split_seed':417,'classes':len(labels),'train_samples':len(train),'test_samples':len(test),
      'accuracy':best,'confidence_threshold':.75,'accepted_accuracy':correct[accepted].float().mean().item(),
      'accepted_fraction':accepted.float().mean().item(),'sources':sources,'source_sha256':{n:hashlib.sha256((CACHE/n).read_bytes()).hexdigest() for n in sources},
      'groups':{name:{'accuracy':correct[torch.tensor(origin[test.cpu().numpy()]==name,device=device)].float().mean().item(),
                        'samples':int(np.sum(origin[test.cpu().numpy()]==name))} for name in sorted(set(origin))}}
    destination=ROOT/'src/BlurMask/Assets/AiPen.bin'
    with destination.open('wb') as output:
        output.write(b'BMPEN001'); output.write(struct.pack('<4i',32,12,24,96)); output.write(struct.pack('<i',len(labels)))
        output.write(struct.pack('<'+'i'*len(labels),*(ord(n) for n in labels)))
        for layer in ('c1','c2','f1','f2'):
            for kind in ('weight','bias'): output.write(best_state[f'{layer}.{kind}'].numpy().astype('<f4').tobytes())
    report['model_bytes']=destination.stat().st_size; report['model_sha256']=hashlib.sha256(destination.read_bytes()).hexdigest()
    (ROOT/'docs/ai-pen-training.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
    # Portable fixed normalized examples verify C#/Python numerical agreement, not training accuracy.
    selected=[]
    test_ids=test.cpu().numpy(); pred=prediction.cpu().numpy(); ok=correct.cpu().numpy(); scores=confidence.cpu().numpy()
    for label in ('A','Ж','5','+','=','√','∞'):
        candidates=[i for i in range(len(test_ids)) if names[test_ids[i]]==label and ok[i] and scores[i]>.9]
        if candidates:
            i=candidates[0]; selected.append({'label':label,'pixels':images[test_ids[i]].flatten().tolist(),'logits':logits[i].detach().cpu().tolist()})
    (CACHE/'golden.json').write_text(json.dumps(selected,ensure_ascii=True),encoding='utf8')
    with (ROOT/'tools/ai-pen-fixtures.bin').open('wb') as output:
        output.write(struct.pack('<i',len(selected)))
        for example in selected:
            output.write(struct.pack('<i',ord(example['label']))); output.write(bytes(example['pixels']))
            output.write(struct.pack('<i',len(example['logits'])))
            output.write(struct.pack('<'+'f'*len(example['logits']),*example['logits']))
    print(json.dumps(report),flush=True)
if __name__=='__main__': main()
