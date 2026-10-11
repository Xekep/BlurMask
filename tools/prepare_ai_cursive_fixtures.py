"""Small real Russian cursive smoke examples, source School Notebooks RU (MIT).
Requires .artifacts/ai-data/school-test.json and school-page.jpg, downloaded separately.
This is a functionality check, not an unseen-writer quality benchmark.
"""
import json, struct
from pathlib import Path
import numpy as np
from PIL import Image
import torch
from export_ai_handwriting import load

ROOT=Path(__file__).resolve().parents[1]
CACHE=ROOT/'.artifacts/ai-data'
model,charset,_=load()
from strhub.data.utils import Tokenizer
tokenizer=Tokenizer(charset)
annotations=json.loads((CACHE/'school-test.json').read_text(encoding='utf8'))
page=Image.open(CACHE/'school-page.jpg')
selected=[a for a in annotations['annotations'] if a['image_id']==annotations['images'][0]['id']][:5]
results=[]
with (ROOT/'tools/ai-cursive-fixtures.bin').open('wb') as f:
    f.write(struct.pack('<i',len(selected)))
    for i,a in enumerate(selected):
        points=np.array(a['segmentation'][0]).reshape(-1,2)
        crop=page.crop((int(points[:,0].min())-3,int(points[:,1].min())-3,int(points[:,0].max())+4,int(points[:,1].max())+4))
        gray=np.array(crop.convert('L').resize((512,48),Image.Resampling.BILINEAR),dtype=np.float32)/127.5-1
        x=torch.from_numpy(gray).reshape(1,1,48,512).repeat(1,3,1,1)
        with torch.no_grad(): logits=model(tokenizer,x); strings,scores=tokenizer.decode(logits.softmax(-1))
        # The reference decoder is checked separately against annotation so no incorrect example is silently accepted.
        label=a['attributes']['translation']; result=strings[0]
        results.append({'annotation':label,'recognized':result,'matches':label==result,'image':'2013.jpg','region':i})
        encoded=result.encode('utf8'); f.write(struct.pack('<i',len(encoded))); f.write(encoded); f.write(gray.astype('<f4').tobytes())
        crop.save(CACHE/f'cursive-{i}.png')
(ROOT/'docs/ai-cursive-smoke.json').write_text(json.dumps(results,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print(json.dumps(results,ensure_ascii=True))
