"""Independent recursive height-based AVL oracle over every permutation of up to7 keys."""
import argparse,itertools,json,random
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args()
class Node:
 def __init__(self,key):self.k=key;self.l=self.r=None;self.h=1

def height(n):return n.h if n else 0

def update(n):n.h=1+max(height(n.l),height(n.r));return n

def left(n):v=n.r;n.r=v.l;v.l=n;update(n);return update(v)
def right(n):v=n.l;n.l=v.r;v.r=n;update(n);return update(v)
def add(n,k):
 if n is None:return Node(k)
 if k<n.k:n.l=add(n.l,k)
 elif k>n.k:n.r=add(n.r,k)
 else:return n
 update(n);b=height(n.r)-height(n.l)
 if b==2:
  if height(n.r.l)>height(n.r.r):n.r=right(n.r)
  return left(n)
 if b==-2:
  if height(n.l.r)>height(n.l.l):n.l=left(n.l)
  return right(n)
 return n

def case(seq):
 root=None;unique=set()
 for k in seq:root=add(root,k);unique.add(k)
 fields=[]
 def visit(n):
  if n:visit(n.l);fields.append([n.k,n.l.k if n.l else -1,n.r.k if n.r else -1,height(n.r)-height(n.l)]);visit(n.r)
 visit(root);return dict(sequence=seq,root=root.k if root else -1,nodes=fields)
v=[0,1,255,32768,65531,65533,65535];out=[case([])]
for n in range(1,8):
 for permutation in itertools.permutations(v[:n]):
  seq=list(permutation);out.append(case(seq+seq[:1]+seq[-1:]))
r=random.Random(0x0918d0)
for _ in range(256):out.append(case([r.randrange(65536) if i%3 else r.choice(v) for i in range(64)]))
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(out,separators=(',',':')),encoding='utf8');print(f'Generated {len(out)} independent AVL references; exhaustive permutations through7 keys plus unsigned/duplicate/random controls')
