"""Independent raw/C51/PAGE projection of the selected worker contract."""
import argparse,base64,json,struct
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('capture',type=Path);a=p.parse_args()
data=json.loads(a.capture.read_text());total=0;prefixes=0;all_registry=0
b64=lambda b:base64.b64encode(b).decode()
uri='http://www.myscript.com/ns/myscript/page'
class Stop(Exception):pass
for stream in data['cases']:
 counts=stream['pointCounts'];n=len(counts);names=stream['ids'];chosen=stream['chosen'];bad=stream['fault']
 assert len(names)==1+n%16 and 0<=chosen<len(names)
 assert names==['F'*n]+['field_'+str(i)for i in range(1,len(names))]
 assert counts==[1+(n+i)%8 for i in range(n)]
 # Derive source raw little-endian float channels from the fixture's physical
 # producer, not a model string, selected registry or runtime raw export.
 expected_raw=[]
 for i,count in enumerate(counts):
  b=bytearray()
  for j in range(count):
   x=i*4+j*.125;y=n-i*.5+j
   b.extend(struct.pack('<f',x));b.extend(bytes.fromhex('00000080')if i%2==0 and j==0 else struct.pack('<f',y))
  expected_raw.append(b64(b))
 assert stream['originalRaw']==expected_raw
 first=(255<<8|3)if counts[0]==1 else 3
 last=((n-1)<<32)|((counts[-1]-1)<<16)|(255<<8)|5
 ranges=b64(struct.pack('<QQ',first,last))
 nodes=[];entries=[];trace=[];raw=[];attempt=0;next_id=0;maximum=-(1<<63);root=field=input_node=0
 def step(name):
  global attempt
  ordinal=attempt;attempt+=1;trace.append('try:'+name)
  if ordinal==bad:
   if stream['role']=='preparedFailure':trace.append('refused:'+name)
   raise Stop()
 def node(kind,name,origin=0):
  step('node:'+str(kind)+':'+name)
  value={'id':len(nodes)+1,'kind':kind,'name':name,'origin':origin,'tag':3 if kind==0 else 0,'value':'','uri':None,'parent':0,'children':[],'attributes':[]}
  nodes.append(value);return value
 def edge(parent,child,attribute=False):
  child['parent']=parent['id'];parent['attributes'if attribute else'children'].append(child['id'])
  trace.append(('attribute:'if attribute else'append:')+str(parent['id'])+':'+str(child['id']))
 try:
  step('C20:create-history0')
  for i in range(n):step('raw-clone:'+str(i));raw.append(expected_raw[i])
  step('query:all-mode2');step('query:remove-empty')
  #06D2C0 matching query + C57 signed in-order IDs, with09EE20 explicit IDs.
  for registry_id in range(len(names)):
   step('C22:create:'+str(registry_id));step('C51:register:'+str(registry_id))
   entries.append({'id':registry_id,'category':100,'name':'INPUT','ranges':ranges})
   maximum=max(maximum,registry_id);next_id=max(next_id,registry_id+1)
  step('C51:copy-original-next');next_id=len(names)
  r=node(4,'');root=r['id'];page=node(3,'page');ns=node(2,'');ns['uri']=uri;edge(page,ns)
  version=node(0,'version');version['tag']=7;version['value']=b64(struct.pack('<i',5));edge(page,version,True)
  content=node(3,'content');layout=node(3,'layout');edge(page,content);edge(page,layout);edge(r,page)
  #Original1243C0 has6 nodes. Each11D3C0/addItems addsfield/id/input/tag.
  #0EBE40 copies actual chosen field attributes BEFORE its input child.
  f=node(3,'textField',7+4*chosen);attr=node(0,'id',8+4*chosen)
  if len(names[chosen])>=16:step('clone:heap-value')
  attr['tag']=1 if len(names[chosen])>=16 else 2;attr['value']=b64(names[chosen].encode());edge(f,attr,True)
  ink=node(3,'input',9+4*chosen);tag=node(0,'tag',10+4*chosen);tag['tag']=4;tag['value']=b64(struct.pack('<q',chosen));edge(ink,tag,True);edge(f,ink)
  field=f['id'];edge(content,f);input_node=ink['id'];step('BCD:create-selected-field')
  trace.append('BCC:context-setter-false-ignored')
  succeeded=True
 except Stop:
  succeeded=False;prefixes+=1
 expected={'succeeded':succeeded,'faulted':not succeeded,'attempts':attempt,'next':next_id,'maximum':maximum,
  'c51':b64(struct.pack('<qqB',next_id,maximum,0)),'root':root,'selectedField':field,'selectedInput':input_node,
  'registry':entries,'nodes':nodes,'selectedRaw':raw,'trace':trace}
 for key,value in expected.items():assert stream[key]==value,(n,chosen,stream['role'],bad,key,stream[key],value)
 if succeeded:
  total+=1
  if len(entries)>1 and chosen==0:all_registry+=1
 assert (stream['role']=='actual')==succeeded
assert total==272 and all_registry==30,(total,all_registry)
assert len(data['cases'])==data['streams'] and data['completed']==272 and data['guards']>1000
print(f'PASS independent {total} actual selected clones/{prefixes} fault or cancellation prefixes/{all_registry} selected0-allIDs controls; full DOM/raw/C51 bytes')
