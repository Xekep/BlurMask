"""Independent binary32 separation and literal mutable arc-deletion oracle."""
import copy
import json
import pathlib
import struct
import sys

def f(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]

def rounded(value):
    bits = struct.unpack('<I', struct.pack('<f', value))[0]
    magnitude = bits & 0x7fffffff
    if magnitude >= 0x4b000000:
        return value
    if magnitude < 0x3f800000:
        bits = (bits & 0x80000000) | (0x3f800000 if magnitude >= 0x3f000000 else 0)
    else:
        shift = 150 - (magnitude >> 23)
        bits = ((bits >> shift) + ((bits >> (shift - 1)) & 1)) << shift
    return struct.unpack('<f', struct.pack('<I', bits))[0]

data=json.loads(pathlib.Path(sys.argv[1]).read_bytes());assert data['expectedLabelsRead'] is False
assert data['inputSha256']=='bcd10f1a92a3abf6d78c5eb18787abf33ad291ab07660daae7468cf465dab285'
table=[f(x) for x in data['table']];assert len(table)==256
active=removed=groups=arcs=0
for case in data['samples']:
 costs=[]
 for ranges in case['groups']:
  boxes=[]
  for stroke,start,end in ranges:
   xs=[f(p[0]) for p in case['prepared'][stroke][start:end]];assert xs
   boxes.append((min(xs),max(xs)))
  parts=[]
  for boundary in range(1,len(boxes)):
   left,right=boxes[:boundary],boxes[boundary:]
   gap=max(f(min(x[0] for x in right)-max(x[1] for x in left)),f(min(x[0] for x in left)-max(x[1] for x in right)))
   x=f(f(f(gap/f(case['height']))-f(case['gap']))*f(-case['slope']))
   value=0 if x>4 else table[int(f(f(f(x+4)*255)+.5))>>3] if x>=-4 else x
   scaled=f(f(-value)*8192)
   parts.append(int(rounded(scaled)) if len(boxes)<9 else int(f(scaled+(.5 if scaled>0 else -.5))))
  total=0
  for cost in parts:total=total+cost if len(boxes)>=9 or f(f(total)+f(cost))<0x4000000 else 0x4000000
  costs.append(min(total,0x4000000));groups+=1
 graph=copy.deepcopy(case['before']);labels=[list(range(len(n))) for n in graph];local=None;nr=0;tail=[]
 for node in range(len(graph)):
  if node==len(graph)-1:local=copy.deepcopy(graph)
  for ordinal in range(len(graph[node])-1,-1,-1):
   arc=graph[node][ordinal];arcs+=1
   if arc[1]==0 or costs[arc[0]]<=case['threshold']:continue
   if node==len(graph)-1:tail.append((labels[node][ordinal],arc[0],arc[1],costs[arc[0]]))
   for other_node,others in enumerate(graph):
    for other in others:
     if other_node-other[1]-1==node:other[5]=[p-(p>ordinal) for p in other[5] if p!=ordinal]
     other[6]=[[off,index-(index>ordinal)] if other_node+off==node else [off,index] for off,index in other[6] if other_node+off!=node or index!=ordinal]
   del graph[node][ordinal];del labels[node][ordinal];nr+=1
 assert local==case['local'] and graph==case['full']==case['actual'] and labels==case['fullOrdinals'],case['id']
 assert nr==case['fullRemoved'] and nr-len(tail)==case['localRemoved']
 assert sorted(tail)==[(a['ordinal'],a['group'],a['span'],a['cost']) for a in case['tail']]
 for a in case['tail']:assert a['ranges']==case['groups'][a['group']] and len(a['ranges'])>1
 active+=bool(tail);removed+=len(tail)
assert (len(data['samples']),active,removed)==(100,36,95)
assert data['activeSamples']==active and data['removedTailArcs']==removed
report=dict(samples=100,active=active,tailRemoved=removed,groups=groups,arcVisits=arcs,fullGraphsAndAdjacency=True,expectedLabelsRead=False,scope='Recorded Latin geometry in Russian FIRST owner; no quality claim')
if len(sys.argv)>2:pathlib.Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n')
print('PASS',json.dumps(report))
