"""Independent byte-alphabet reader and run-domain cased range oracle."""
import base64,json,random,struct,sys
from pathlib import Path
if len(sys.argv)!=4 or Path(sys.argv[3]).suffix.lower()!='.json':raise ValueError('AK.res LK.res output.json')
def read(data):
 at=0
 def u(fmt):
  nonlocal at
  v=struct.unpack_from('<'+fmt,data,at)[0];at+=struct.calcsize('<'+fmt);return v
 header=[u('H') for _ in range(8)];count=header[-1];n=u('H')
 for inc in [32,64,1]:
  values=[u('H') for _ in range(n)];n=next(v for v in reversed(values) if v!=65535)+inc
 spans=[u('H') for _ in range(n+1)];entries=spans[-1]+1
 offsets=[];labels=[]
 for i in range(entries):offsets.append(u('I'));labels.append(u('I')&65535)
 strings=data[at:at+offsets[-1]];at+=offsets[-1]
 text={labels[i]:strings[offsets[i]:offsets[i+1]].decode('utf8') for i in range(entries-1)}
 reverse=[u('H') for _ in range(count)];bit=u('B')
 if bit:at+=(count+7)//8
 forms=u('H');at+=forms*6;maps=[];map_positions=[]
 if forms:
  flag=u('I')
  for m in range(3):
   if flag&(1<<m):
    map_positions.append(at);maps.append([u('b' if forms<128 else 'h') for _ in range(count)])
 return dict(count=count,end=at,forms=forms,maps=maps,positions=map_positions,text=text,reverse=reverse)
def expected(bits,enabled):
 if not enabled:return [65535]*4
 runs=[]
 for i,v in enumerate(bits):
  if v and (i==0 or not bits[i-1]):runs.append([i,i])
  elif v:runs[-1][1]=i
 if len(runs)>2:return None
 out=[65535]*4
 if runs:out[0]=runs[0][0];out[1]=runs[0][1] if runs[0][1]<len(bits)-1 else 65535
 if len(runs)==2:out[2]=runs[1][0];out[3]=runs[1][1] if runs[1][1]<len(bits)-1 else len(bits)
 return out
ak=Path(sys.argv[1]).read_bytes()[99:];lk=Path(sys.argv[2]).read_bytes()[607074:]
a=read(ak);l=read(lk);cases=[]
for name,raw,meta in [('actualAcoustic',ak,a),('actualLexical',lk,l)]:
 bits=[any(m[i]!=-1 for m in meta['maps']) for i in range(meta['count'])]
 cases.append(dict(name=name,payload=base64.b64encode(raw[:meta['end']]).decode(),version=259,enabled=bool(meta['forms']),expected=expected(bits,bool(meta['forms'])),storedBits=bits))
assert a['forms']<128 and len(a['positions'])==1
rng=random.Random(0x58200)
for n in range(512):
 raw=bytearray(ak[:a['end']]);count=a['count'];bits=[False]*count
 if n<8:
  selections=[[],list(range(count)),[0],[count-1],[0,2,4],[0,2],[count-2,count-1],[0,count-1]][n]
 else:selections=sorted(rng.sample(range(count),n%8))
 for i in selections:bits[i]=True
 at=a['positions'][0];raw[at:at+count]=bytes(0 if v else 255 for v in bits)
 enabled=n%3!=0;raw.append(1 if enabled else 0)
 cases.append(dict(name='preparedMap'+str(n),payload=base64.b64encode(raw).decode(),version=260,enabled=enabled,expected=expected(bits,enabled),storedBits=bits))
Path(sys.argv[3]).write_text(json.dumps(cases,separators=(',',':'))+'\n',encoding='utf8')
print(f'Generated{len(cases)} byte-alphabet controls, {sum(c["expected"] is None for c in cases)} native third-run refusals; actual dimensions {a["count"]}/{l["count"]}.')
