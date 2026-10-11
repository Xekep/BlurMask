"""Export the pinned open PARSeq-S checkpoint as FP16 data for native C# inference.
No Python or ML runtime is required by the application. Run from the repo root.
Model license: CC BY-SA 4.0. Architecture: Darwin Bautista, Apache-2.0.
"""
import hashlib, json, struct, sys, urllib.request, zipfile
from pathlib import Path
import numpy as np
import torch
ROOT=Path(__file__).resolve().parents[1]
CACHE=ROOT/'.artifacts/ai-data'
SOURCE='1902db043c029a7e03a3818c616c06600af574be'
CACHE.mkdir(parents=True,exist_ok=True)
def download(url,path):
    if not path.exists():
        with urllib.request.urlopen(url) as response, path.open('wb') as output:
            import shutil
            shutil.copyfileobj(response,output)
source_path=CACHE/'parseq-source'/('parseq-'+SOURCE)
if not source_path.exists():
    archive=CACHE/'parseq-source.zip'
    download('https://codeload.github.com/baudm/parseq/zip/'+SOURCE,archive)
    with zipfile.ZipFile(archive) as source: source.extractall(CACHE/'parseq-source')
download('https://huggingface.co/Hukyl/parseq-s-cyrillic-handwritten/resolve/7ee349c4aa082a3ea6a694263b329ae613ce4dcd/best.pt',CACHE/'parseq-cyrillic.pt')
sys.path.insert(0,str(CACHE/'parseq-source'/('parseq-'+SOURCE)))
from strhub.models.parseq.model import PARSeq
from strhub.data.utils import Tokenizer

def load():
    payload=torch.load(CACHE/'parseq-cyrillic.pt',map_location='cpu',weights_only=True)
    c=payload['config']; charset=payload['charset']
    model=PARSeq(len(charset)+3,c['max_label_length'],(c['img_height'],c['img_width']),c['patch_size'],
      c['embed_dim'],c['enc_num_heads'],c['enc_mlp_ratio'],c['enc_depth'],c['dec_num_heads'],c['dec_mlp_ratio'],c['dec_depth'],True,1,.1)
    # Match the shipped half-precision data while computing in FP32.
    state={k:v.half().float() for k,v in payload['model_state'].items()}
    model.load_state_dict(state); model.eval(); torch.set_num_threads(3)
    return model,charset,state

def main():
    model,charset,state=load(); destination=ROOT/'src/BlurMask/Assets/AiHandwriting.bin'
    with destination.open('wb') as f:
        f.write(b'BMPSEQ01'); f.write(struct.pack('<i',len(charset)))
        f.write(struct.pack('<'+'i'*len(charset),*(ord(c) for c in charset)))
        f.write(struct.pack('<i',len(state)))
        for name,tensor in state.items():
            name=name.encode('ascii'); f.write(struct.pack('<i',len(name))); f.write(name)
            f.write(struct.pack('<i',tensor.numel())); f.write(tensor.numpy().astype('<f2').tobytes())
    report={'model':'Hukyl/parseq-s-cyrillic-handwritten','revision':'7ee349c4aa082a3ea6a694263b329ae613ce4dcd',
      'architecture_revision':SOURCE,'source_sha256':hashlib.sha256((CACHE/'parseq-cyrillic.pt').read_bytes()).hexdigest(),
      'format':'FP16 storage, FP32 inference','license':'CC-BY-SA-4.0',
      'bytes':destination.stat().st_size,'sha256':hashlib.sha256(destination.read_bytes()).hexdigest()}
    (ROOT/'docs/ai-handwriting-model.json').write_text(json.dumps(report,indent=2)+'\n')
    # A fixed sparse input verifies encoder and decoder numerics independently.
    image=torch.ones(1,3,48,512); image[:,:,8:40,100:106]=-1; image[:,:,8:14,100:160]=-1
    with torch.no_grad():
        memory=model.encode(image); tokens=torch.tensor([[len(charset)+1,129,140]])
        decoded=model.head(model.decode(tokens,memory))
    with (ROOT/'tools/ai-handwriting-fixture.bin').open('wb') as f:
        f.write(image[0,0].numpy().astype('<f4').tobytes()); f.write(memory.numpy().astype('<f4').tobytes())
        f.write(decoded.numpy().astype('<f4').tobytes())
    print(json.dumps(report))
    export_english()

def export_english():
    revision='a1526c3d63740e460153987f9aaf6b86aa199dc1'
    path=CACHE/'parseq-english.pt'
    download(f'https://huggingface.co/baudm/parseq-small/resolve/{revision}/pytorch_model.bin',path)
    charset='0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~'
    state={k:v.half().float() for k,v in torch.load(path,map_location='cpu',weights_only=True).items()}
    model=PARSeq(len(charset)+3,25,(32,128),(4,8),384,6,4,12,12,4,1,True,1,.1)
    model.load_state_dict(state); model.eval()
    destination=ROOT/'src/BlurMask/Assets/AiEnglish.bin'
    with destination.open('wb') as f:
        f.write(b'BMPSEQ01'); f.write(struct.pack('<i',len(charset))); f.write(struct.pack('<'+'i'*len(charset),*(ord(c) for c in charset)))
        f.write(struct.pack('<i',len(state)))
        for name,tensor in state.items():
            name=name.encode('ascii'); f.write(struct.pack('<i',len(name))); f.write(name)
            f.write(struct.pack('<i',tensor.numel())); f.write(tensor.numpy().astype('<f2').tobytes())
    image=torch.ones(1,3,32,128); image[:,:,4:28,20:24]=-1; image[:,:,4:8,20:45]=-1
    with torch.no_grad(): memory=model.encode(image); decoded=model.head(model.decode(torch.tensor([[95,18,15]]),memory))
    with (ROOT/'tools/ai-english-fixture.bin').open('wb') as f:
        f.write(image[0,0].numpy().astype('<f4').tobytes()); f.write(memory.numpy().astype('<f4').tobytes()); f.write(decoded.numpy().astype('<f4').tobytes())
    report={'model':'baudm/parseq-small','revision':revision,'architecture_revision':SOURCE,'license':'Apache-2.0',
        'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'sha256':hashlib.sha256(destination.read_bytes()).hexdigest(),
        'bytes':destination.stat().st_size,'input':[32,128],'scope':'Latin word recognition, scene text pretraining; cursive quality requires validation'}
    (ROOT/'docs/ai-english-model.json').write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report))
if __name__=='__main__': main()
