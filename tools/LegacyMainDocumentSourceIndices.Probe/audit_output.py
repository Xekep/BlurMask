"""Independent recursive height-based AVL and raw source-membership replay.

The runtime uses iterative stored-balance propagation; this oracle recomputes
heights and independently checks raw unsigned interval/vector keys, identities,
page creation/slots, duplicate memberships and latest equal-vector ownership.
No private research imports, expected text or vendor execution.
"""
import json,sys
from pathlib import Path

class Tree:
 def __init__(self,slots):self.root=None;self.nodes=[];self.slots=slots;self.rotations=[0,0,0,0]
 def height(self,n):return 0 if n is None else 1+max(self.height(n['l']),self.height(n['r']))
 def balance(self,n):return self.height(n['r'])-self.height(n['l'])
 def left(self,n):
  b=n['r'];n['r']=b['l'];b['l']=n;return b
 def right(self,n):
  b=n['l'];n['l']=b['r'];b['r']=n;return b
 def intern(self,key):
  found=None;inserted=False
  def add(n):
   nonlocal found,inserted
   if n is None:
    i=len(self.nodes)+1;found=dict(id=i,key=key,l=None,r=None,head=0,source=0);self.nodes.append(found);inserted=True;return found
   if key==n['key']:found=n;return n
   side='l' if key<n['key'] else 'r';n[side]=add(n[side]);b=self.balance(n)
   if b==-2:
    if self.balance(n['l'])>0:n['l']=self.left(n['l']);self.rotations[1]+=1
    else:self.rotations[0]+=1
    return self.right(n)
   if b==2:
    if self.balance(n['r'])<0:n['r']=self.right(n['r']);self.rotations[3]+=1
    else:self.rotations[2]+=1
    return self.left(n)
   return n
  self.root=add(self.root);return found,inserted
 def rows(self,vector):
  rows=[]
  for n in self.nodes:
   i=n['id'];b=self.balance(n);assert abs(b)<=1
   rows.append(dict(fields=[i,(i-1)//self.slots+1,(i-1)%self.slots,n['l']['id']if n['l']else 0,n['r']['id']if n['r']else 0,b,0 if vector else n['head'],n['source']if vector else 0],key=[list(r)for r in n['key']]))
  return rows

def replay(inputs):
 intervals=Tree(64);vectors=Tree(73);members=[];dirty=False
 for source,key in inputs:
  key=tuple(tuple(r)for r in key)
  for r in key:
   n,new=intervals.intern((r,));dirty|=new;i=len(members)+1
   members.append([i,(i-1)//256+1,(i-1)%256,source,n['head']]);n['head']=i
  n,_=vectors.intern(key);n['source']=source
 result=dict(intervalRoot=intervals.root['id']if intervals.root else 0,vectorRoot=vectors.root['id']if vectors.root else 0,dirty=dirty,added=len(inputs),intervals=intervals.rows(False),vectors=vectors.rows(True),members=members)
 return result,intervals.rotations,vectors.rotations

j=json.loads(Path(sys.argv[1]).read_bytes());rotations=[0]*4;source_count=0;nodes=0;members=0;order_discriminators=0;duplicate_members=0
assert len(j['samples'])==4 and len(j['prepared'])==40
for sample in j['samples']:
 assert sample['ready']and not sample['faulted']
 inputs=sample['inputs'];chars=[s for s in inputs if not s['word']]
 assert [s['id']for s in chars]==sample['charInsertion']
 if sample['charInsertion']!=sample['charInOrder']:order_discriminators+=1
 processed=[s for s in inputs if not s['flags']&0x200000];assert sample['processed']==processed
 for s in processed:
  assert s['header']in(0x20,0x30)
  if not s['word']:assert s['header']==0x20
 for word,name in [(False,'characters'),(True,'words')]:
  sequence=[(i+1,s['ranges'])for i,s in enumerate(processed)if s['word']==word]
  expected,_,_=replay(sequence);assert sample[name]==expected,(name,sample[name],expected)
 source_count+=len(processed)
for case in j['prepared']:
 expected,a,b=replay([(i+1,key)for i,key in enumerate(case['inputs'])]);assert case['index']==expected
 rotations=[x+y+z for x,y,z in zip(rotations,a,b)];nodes+=len(expected['intervals'])+len(expected['vectors']);members+=len(expected['members'])
 assert max(n['fields'][1]for n in expected['intervals'])>=2
 assert max(n['fields'][1]for n in expected['vectors'])>=2
 assert expected['members'][-1][1]>=2 #256-member page boundary
 duplicate_members+=len(expected['members'])-len(expected['intervals'])
assert all(rotations)and order_discriminators and duplicate_members>0
print(f'PASS independent4 actual owners/{source_count} payloads/{order_discriminators} insertion-order discriminators;40 prepared trees/{nodes} nodes/{members} memberships;rotations{rotations};raw duplicates and latest-key owners exact.')
