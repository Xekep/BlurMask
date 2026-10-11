"""Independent uint32 source-map interpreter and native-shaped signed key sort."""
import json,random,sys
rng=random.Random(168900);mask=0xffffffff

def compose(incoming,factor,previous,ratio):
 out=[]
 for tick in incoming:
  if not previous:out.append(tick if factor==ratio else ((ratio*tick)&mask)//factor)
  else:
   whole,rem=divmod(tick,factor)
   delta=((((previous[whole+1]-previous[whole])&mask)*rem)&mask)//factor if rem else 0
   out.append((previous[whole]+delta)&mask)
 return out

def resolve(global_index,point_map,ratio,key_map,keys):
 rembyte=0;original=global_index
 if point_map:
  whole,rem=divmod(point_map[global_index],ratio);original=whole+(rem!=0)
  rembyte=((ratio-rem)&255) if rem else 0
 keyindex=key_map[original]
 return [original,keyindex,rembyte,keys[keyindex]]

def sort(records):
 # Records retain native input ordinal; comparisons use only signed3-fieldkey.
 records=[(tuple(key),i) for i,key in enumerate(records)]
 def partition(first,last):
  while True:
   lo=first;hi=last;pivot=records[(first+last)//2][0]
   while True:
    while records[lo][0]<pivot:lo+=1
    while records[hi][0]>pivot:hi-=1
    if hi<lo:break
    records[lo],records[hi]=records[hi],records[lo];lo+=1;hi-=1
    if lo>hi:break
   if first<hi:partition(first,hi)
   if last<=lo:return
   first=lo
 partition(0,len(records)-1)
 return [i for key,i in records]

kernels=[]
for i in range(1024):
 factor=rng.choice([1,2,7,256,65535,0xffffffff]);ratio=rng.choice([1,256,65537,0xfffffffe])
 previous=[] if i%2 else [rng.randrange(0x100000000) for _ in range(rng.randrange(2,30))]
 if previous:
  incoming=[]
  for j in range(rng.randrange(1,40)):
   index=rng.randrange(len(previous));rem=0 if index==len(previous)-1 else rng.randrange(min(factor,1000))
   tick=index*factor+rem
   if tick>mask:tick=index
   incoming.append(tick)
 else:incoming=[rng.randrange(0x100000000) for _ in range(rng.randrange(1,40))]
 kernels.append(dict(incoming=incoming,factor=factor,previous=previous,ratio=ratio,expected=compose(incoming,factor,previous,ratio)))

sources=[]
for i in range(768):
 counts=[rng.randrange(1,12) for _ in range(rng.randrange(1,7))];total=sum(counts);original=rng.randrange(total,total+50);ratio=rng.choice([1,2,17,256,512])
 keys=[rng.choice([[rng.choice([-2147483648,-1,0,1,2147483647]),rng.randrange(-3,4),rng.randrange(-8,9)],[0,0,0]]) for _ in range(original)]
 key_map=[rng.randrange(original) for _ in range(original)]
 point_map=[] if i%3==0 else [rng.randrange((original-1)*ratio+1) for _ in range(total)]
 identities=[resolve(j,point_map,ratio,key_map,keys) for j in range(total)]
 ranges=[];range_keys=[];starts=[sum(counts[:j]) for j in range(len(counts))]
 for j in range(rng.randrange(1,70)):
  stroke=rng.randrange(len(counts));start=rng.randrange(counts[stroke]);end=rng.randrange(start+1,counts[stroke]+1)
  ranges.append([stroke,start,end]);range_keys.append(identities[starts[stroke]+start][3])
 # Exact index1 remap covers arbitrary prior maps and repeated insertion keys.
 incoming=[rng.randrange(total) for j in range(rng.randrange(1,70))]
 remapped=compose(incoming,1,point_map,ratio)
 sources.append(dict(counts=counts,keys=keys,keyMap=key_map,pointMap=point_map,ratio=ratio,identities=identities,ranges=ranges,order=sort(range_keys),incoming=incoming,remapped=remapped,remappedIdentities=[resolve(j,remapped,ratio,key_map,keys) for j in range(len(incoming))]))
json.dump(dict(kernels=kernels,sources=sources),open(sys.argv[1],'w'),separators=(',',':'))
print('Wrote',len(kernels),'uint32 map kernels and',len(sources),'point-key source/range controls')
