"""Native model62 SplitMix64/raw UUID order; seed values are prepared inputs."""
import base64,json,random,struct,sys
from pathlib import Path
mask=(1<<64)-1
def draw(state):
 values=[]
 for _ in range(2):
  state=(state+0x9e3779b97f4a7c15)&mask;x=state
  x=((x^(x>>30))*0xbf58476d1ce4e5b9)&mask
  x=((x^(x>>27))*0x94d049bb133111eb)&mask
  values.append(x^(x>>31))
 raw=bytearray(struct.pack('<QQ',*values));raw[6]=(raw[6]&15)|0x40;raw[8]=(raw[8]&63)|0x80
 return state,bytes(raw)
def format_raw(raw):return '-'.join(raw[a:b].hex()for a,b in [(0,4),(4,6),(6,8),(8,10),(10,16)])
def controls():
 r=random.Random(0x62);rows=[]
 for index in range(1024):
  seed=[0,mask,1,1<<63][index]if index<4 else r.getrandbits(64);state=seed;values=[]
  for _ in range(1+index%7):
   state,raw=draw(state);values.append(dict(raw=base64.b64encode(raw).decode(),text=format_raw(raw)))
  rows.append(dict(seed=seed,values=values,finalState=state))
 return dict(sequences=rows)
if __name__=='__main__':
 Path(sys.argv[1]).write_bytes((json.dumps(controls(),separators=(',',':'))+'\n').encode());print('Wrote1024 independent model62 prepared-seed sequences')
