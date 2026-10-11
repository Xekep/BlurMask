"""Synthetic connected/slanted English smoke samples (Caveat, SIL OFL 1.1).
These verify native inference; they are not an unseen-writer handwriting benchmark.
"""
import json, struct, urllib.request, urllib.parse
import numpy as np
import torch
from PIL import Image, ImageDraw, ImageFont
from export_ai_handwriting import CACHE, ROOT, PARSeq
revision='bd8f81ddb5c74d5c8897b36ad88b440266245103'
font_path=CACHE/'Caveat.ttf'
urllib.request.urlretrieve(f'https://raw.githubusercontent.com/google/fonts/{revision}/ofl/caveat/Caveat%5Bwght%5D.ttf',font_path)
urllib.request.urlretrieve(f'https://raw.githubusercontent.com/google/fonts/{revision}/ofl/caveat/OFL.txt',ROOT/'docs/licenses/LICENSE-FONT-Caveat.txt')
charset='0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~'
model=PARSeq(97,25,(32,128),(4,8),384,6,4,12,12,4,1,True,1,.1)
state={k:v.half().float() for k,v in torch.load(CACHE/'parseq-english.pt',map_location='cpu',weights_only=True).items()}
model.load_state_dict(state); model.eval(); torch.set_num_threads(3)
from strhub.data.utils import Tokenizer
tok=Tokenizer(charset); font=ImageFont.truetype(str(font_path),64)
words=['hello','world','English','drawing','test','12345','x+2=5']; reports=[]
with (ROOT/'tools/ai-english-words.bin').open('wb') as fixture:
    fixture.write(struct.pack('<i',len(words)))
    for word in words:
        b=font.getbbox(word); image=Image.new('L',(b[2]-b[0]+4,b[3]-b[1]+4),255)
        ImageDraw.Draw(image).text((2-b[0],2-b[1]),word,font=font,fill=0)
        gray=np.array(image.resize((128,32),Image.Resampling.BILINEAR),dtype=np.float32)/127.5-1
        with torch.no_grad(): logits=model(tok,torch.from_numpy(gray).reshape(1,1,32,128).repeat(1,3,1,1)); texts,_=tok.decode(logits.softmax(-1))
        if texts[0]!=word: raise RuntimeError((word,texts[0]))
        text=word.encode('utf8'); fixture.write(struct.pack('<i',len(text))); fixture.write(text); fixture.write(gray.astype('<f4').tobytes())
        reports.append({'annotation':word,'recognized':texts[0],'source':'Caveat font, synthetic smoke example','font_revision':revision})
(ROOT/'docs/ai-english-smoke.json').write_text(json.dumps(reports,indent=2)+'\n')
print(json.dumps(reports))
