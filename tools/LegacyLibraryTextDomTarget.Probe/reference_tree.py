"""Raw40/48/ref24/TEXT32/line56 tree oracle reused from committed DOM probe.
No recognition labels or runtime tree helpers enter this reconstruction.
"""
import base64,importlib.util,struct,math
from pathlib import Path
rp=Path(__file__).resolve().parents[1]/'LegacyMainSourceRangeUtf8.Probe/reference_controls.py'
spec=importlib.util.spec_from_file_location('range_reference',rp);ranges=importlib.util.module_from_spec(spec);spec.loader.exec_module(ranges)
b64=base64.b64decode;enc=lambda b:base64.b64encode(b).decode()

def lit(b):return b.split(b'\0',1)[0]

def value(b,tag=None):
 b=lit(b)if tag is None else b
 return dict(tag=(3 if not b else 2 if len(b)<16 else 1)if tag is None else tag,value=enc(b))

def attr(name,b,tag=None):return dict(kind=0,name=name,attributes=[],children=[],**value(b,tag))

def node(name,attributes=(),children=()):return dict(kind=3,name=name,tag=0,value='',attributes=list(attributes),children=list(children))

def number(name,n,fmt,tag):return attr(name,struct.pack(fmt,n),tag)

def range_bytes(s):
 rows=list(map(list,struct.iter_unpack('<QQ',b64(s['ranges16']))));v=b64(ranges.replay(rows)['utf8'])
 if s.get('flags',0)&0x400000:v=(b'{'if v[:1]==b'['else b'}')+v[1:]
 return v

def clean(n):return {k:([clean(x)for x in v]if k in('attributes','children')else v)for k,v in n.items()if k not in('id','nameId')}

def expected(s):
 chars=[];byid={x['id']:x for x in s['characters']}
 for c in s['characters']:
  ats=[attr('range',range_bytes(c))]
  if c['tag']!=-1:ats+=[number('tag',c['tag'],'<q',4)]
  if c['line']!=-1:ats+=[number('line',c['line'],'<i',7)]
  assert c['force']==-1 and c['freeze']==0
  children=[]
  for r in c['records40']:
   raw=b64(r['raw']);assert len(raw)==40 and struct.unpack_from('<I',raw,8)[0]&0xc00==0
   children.append(node('charCandidate',children=[dict(kind=1,name='',attributes=[],children=[],**value(b64(r['text'])))]))
  chars.append(node('charSegment',ats,children))
 word=s['word'];wordats=[attr('range',range_bytes(word))]
 if word['tag']!=-1:wordats+=[number('tag',word['tag'],'<q',4)]
 candidates=[]
 for row in word['rows48']:
  raw=b64(row['raw']);assert len(raw)==48 and struct.unpack_from('<I',raw,24)[0]==len(row['refs24'])
  children=[]
  for link in row['refs24']:
   raw=b64(link['raw']);assert len(raw)==24 and raw[20]==0
   children.append(node('char',[attr('ref',range_bytes(byid[link['charId']])),number('selected',struct.unpack_from('<i',raw,16)[0],'<i',7)]))
  candidates.append(node('wordCandidate',children=children))
 text=s['text'];textats=[number('tag',text['tag'],'<q',4),attr('range',range_bytes(text)),number('selected',text['selected'],'<i',7)];alternatives=[]
 assert text['selected']==0 and text['freeze']==0 and len(text['rows32'])==2
 for row in text['rows32']:
  raw=b64(row['raw']);ref=b64(row['ref24']);assert len(raw)==32 and len(ref)==24 and ref[20]==0 and struct.unpack_from('<I',raw,24)[0]==1
  alternatives.append(node('textCandidate',children=[node('word',[attr('ref',range_bytes(word)),number('selected',struct.unpack_from('<i',ref,16)[0],'<i',7)])]))
 lines=[];raw=b64(s['lines56']);assert len(raw)==56
 for offset in range(0,len(raw),56):
  x,y,angle,h,tag=struct.unpack_from('<ddddq',raw,offset+8);ats=[]
  if abs(x)>=2**-23 and abs(angle)>=2**-23:ats.append(number('x',x,'<d',5))
  ats +=[number('y',y,'<d',5),number('a',angle,'<d',5),number('h',h,'<d',5)]
  if tag>=0:ats.append(number('tagId',tag,'<q',4))
  lines.append(node('line',ats))
 return node('textResult',[attr('gen',b64(s['uuid36']))],[node('charSegmentList',children=chars),node('wordSegmentList',children=[node('wordSegment',wordats,candidates)]),node('text',textats,alternatives),node('lines',children=lines)])
