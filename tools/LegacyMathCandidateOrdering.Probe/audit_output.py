"""Separate literal036810 transcription over full candidate byte records."""
import base64
import collections
import hashlib
import json
import struct
import sys
from pathlib import Path

if len(sys.argv)!=3:
    raise SystemExit('capture.json raw-math2.res')
capture=json.loads(Path(sys.argv[1]).read_bytes())
assert capture['caseCount']==181 and len(capture['cases'])==181 and capture['guards']==35
costs=[0xff800000,0xff7fffff,0xbf800000,0x80000000,0,0x3c23d70a,
       0x3f000000,0x3f800000,0x7f7fffff,0x7f800000,0x7fc00000,0x7fa00001]

def make(count,seed,equal):
    result=[]
    random=(seed*7919+17)&0xffffffff
    for i in range(count):
        row=bytearray((seed*23+i*37+j*19)&255 for j in range(168))
        random^=(random<<13)&0xffffffff
        random^=random>>17
        random^=(random<<5)&0xffffffff
        struct.pack_into('<II',row,0,0x3f800000 if equal else costs[random%len(costs)],i)
        result.append(bytes(row))
    return result

def reorder(rows,first,last):
    #Inclusive native pointer difference; short-range branch threshold0xD20.
    if (last-first)*168<0xd20:
        for current in range(first+1,last+1):
            saved=rows[current]
            key=struct.unpack_from('<f',saved)[0]
            previous=current-1
            while previous>=first and key<struct.unpack_from('<f',rows[previous])[0]:
                rows[previous+1]=rows[previous]
                previous-=1
            rows[previous+1]=saved
        return
    pivot=struct.unpack_from('<f',rows[first+(last-first)//2])[0]
    left,right=first,last
    while True:
        while struct.unpack_from('<f',rows[left])[0]<pivot:
            left+=1
        while pivot<struct.unpack_from('<f',rows[right])[0]:
            right-=1
        if right<left:
            break
        rows[left],rows[right]=rows[right],rows[left]
        left+=1
        right-=1
        if left>right:
            break
    if first<right:
        reorder(rows,first,right)
    if left<last:
        reorder(rows,left,last)

verified=0
for case in capture['cases']:
    rows=make(case['count'],case['seed'],case['equal'])
    initial=b''.join(rows)
    reorder(rows,0,len(rows)-1)
    final=b''.join(rows)
    if 'input' in case:
        assert base64.b64decode(case['input'],validate=True)==initial
        assert base64.b64decode(case['output'],validate=True)==final
    else:
        assert hashlib.sha256(initial).hexdigest()==case['inputSha256']
        assert hashlib.sha256(final).hexdigest()==case['outputSha256']
    assert collections.Counter(rows)==collections.Counter(initial[i:i+168] for i in range(0,len(initial),168))
    if case['equal'] and case['count'] in (20,21,42):
        identities=[struct.unpack_from('<I',r,4)[0] for r in rows]
        expected=list(range(20)) if len(rows)==20 else list(range(20,-1,-1)) if len(rows)==21 else list(range(21,42))+list(range(21))
        assert identities==expected
    verified+=len(final)
print(f'PASS181 independent candidate-ordering controls/{verified} full bytes incl65536 records; tie20/21/42 manual goldens and NaN payloads, prepared only')

#Independent algebraic no-LM score: in the portable integer domain n+6 is
#exact in float32. The two divisions are rounded separately; using a single
#double cost*6/(n+6) would give different output bytes on some controls.
f32=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
completed=[];discriminators=0
random=0x4ed92a31
assert len(capture['completionScores'])==4096
for n,row in enumerate(capture['completionScores']):
 random^=(random<<13)&0xffffffff;random^=random>>17;random^=(random<<5)&0xffffffff
 bits=random&0x7fffffff
 if bits&0x7f800000==0x7f800000:bits&=0x7f7fffff
 if n<4:bits=[0x80000000,0,1,0x7f7fffff][n]
 prefix=0 if n==0 else 1_000_000 if n==1 else random%1_000_001
 assert row['sample']==n and row['prefix']==prefix and row['costBits']==bits
 before=base64.b64decode(row['input'],validate=True);after=base64.b64decode(row['output'],validate=True)
 assert before==make(1,n,False)[0]
 cost=struct.unpack('<f',struct.pack('<I',bits))[0]
 expected=struct.pack('<f',cost/f32((row['prefix']+6)/6))
 assert len(before)==168 and after==expected+before[4:]
 single=struct.pack('<f',cost*6/(row['prefix']+6));discriminators+=single!=expected
 completed.append(after)
assert discriminators>100
reorder(completed,0,len(completed)-1)
assert base64.b64decode(capture['completedSorted'],validate=True)==b''.join(completed)
print(f'PASS4096 independent native-default completion scores/{discriminators} double-fold discriminators and complete score-to-sort records; prepared no-LM only')

#Independent array-index replay of the48-byte max heap. Compare the entire
#capacity after each helper return (zeroed row) and explicit caller payload.
assert len(capture['heaps'])==64
assert {(c['capacity'],c['seed'])for c in capture['heaps']}=={(n,s)for n in (0,1,2,3,4,8,16,32)for s in range(8)}
heap_offers=0
for case in capture['heaps']:
 capacity,seed=case['capacity'],case['seed']
 assert capacity in (0,1,2,3,4,8,16,32) and 0<=seed<8
 rows=[bytes((seed*43+i*48*17+j*17)&255 for j in range(48)) for i in range(capacity)]
 count=0;random=(seed*7919+17)&0xffffffff
 cost_at=lambda i:struct.unpack_from('<f',rows[i])[0]
 assert len(case['offers'])==64
 for n,row in enumerate(case['offers']):
  random^=(random<<13)&0xffffffff;random^=random>>17;random^=(random<<5)&0xffffffff
  bits=0x3f800000 if seed==7 else costs[random%len(costs)]
  value=struct.unpack('<f',struct.pack('<I',bits))[0];slot=-1
  if capacity and not(count==capacity and value>=cost_at(0)):
   if count<capacity:
    slot=count;count+=1
    while slot and not(cost_at((slot-1)//2)>=value):
     rows[slot]=rows[(slot-1)//2];slot=(slot-1)//2
   else:
    slot=0
    while slot*2<count-1:
     left=slot*2+1;child=left
     if left+1<count and cost_at(left+1)>=cost_at(left):child=left+1
     if value>=cost_at(child):break
     rows[slot]=rows[child];slot=child
   rows[slot]=struct.pack('<I',bits)+bytes(44)
  assert row['costBits']==bits and row['slot']==slot and row['count']==count
  assert base64.b64decode(row['zeroed'],validate=True)==b''.join(rows)
  if slot>=0:rows[slot]=rows[slot][:4]+bytes((seed*23+n*37+j*19)&255 for j in range(44))
  assert base64.b64decode(row['filled'],validate=True)==b''.join(rows)
  heap_offers+=1
 #Stable equal insertion followed by strict full-heap rejection is manual.
 if seed==7:
  assert count==capacity
  for i in range(capacity):assert rows[i][4:]==bytes((seed*23+i*37+j*19)&255 for j in range(44))
assert heap_offers==4096
print('PASS4096 prepared48-byte heap offers/full capacity before-after payload; equal and unordered float branches; no native beam integration')

def sort48(rows,lo,hi):
 if not rows:return
 while True:
  i,j=lo,hi;pivot=struct.unpack_from('<f',rows[lo+(hi-lo)//2])[0]
  while True:
   while pivot>struct.unpack_from('<f',rows[i])[0]:i+=1
   while struct.unpack_from('<f',rows[j])[0]>pivot:j-=1
   if i>j:break
   rows[i],rows[j]=rows[j],rows[i];i+=1;j-=1
   if i>j:break
  if lo<j:sort48(rows,lo,j)
  lo=i
  if lo>=hi:return

assert len(capture['rawSorts'])==264
assert {(c['count'],c['seed'])for c in capture['rawSorts']}=={(n,s)for n in range(33)for s in range(8)}
for case in capture['rawSorts']:
 count,seed=case['count'],case['seed'];rows=[];random=(seed*7919+17)&0xffffffff
 for i in range(count):
  random^=(random<<13)&0xffffffff;random^=random>>17;random^=(random<<5)&0xffffffff
  bits=0x3f800000 if seed==7 else costs[random%len(costs)]
  rows.append(struct.pack('<I',bits)+bytes((seed*23+i*37+j*19)&255 for j in range(4,48)))
 assert base64.b64decode(case['input'],validate=True)==b''.join(rows)
 before=list(rows);sort48(rows,0,len(rows)-1)
 assert base64.b64decode(case['output'],validate=True)==b''.join(rows)
 if seed==7 and count==2:assert rows==before[::-1]
 if seed==7 and count==4:assert rows==before[2:]+before[:2]

assert len(capture['unrestricted'])==120
assert collections.Counter((c['width'],c['active'],c['seed'])for c in capture['unrestricted'])==collections.Counter((w,a,s)for w in (1,2,4,8,16)for a in (1,max(1,w//2),w)for s in range(8))
for case in capture['unrestricted']:
 width,active,seed=case['width'],case['active'],case['seed'];capacity=width*2;rows=[]
 random=(seed*7919+17)&0xffffffff
 info=bytes((seed*43+j*17)&255 for j in range(active*8));conditional=bytes((seed*23+j*19)&255 for j in range(active*4))
 for beam in range(active):
  for symbol in range(297):
   random^=(random<<13)&0xffffffff;random^=random>>17;random^=(random<<5)&0xffffffff
   bits=0x3f800000 if seed==7 else costs[random%len(costs)]
   value=struct.unpack('<f',struct.pack('<I',bits))[0];cost=lambda i:struct.unpack_from('<f',rows[i])[0]
   if len(rows)==capacity and value>=cost(0):continue
   if len(rows)<capacity:
    slot=len(rows);rows.append(bytes(48))
    while slot and not(cost((slot-1)//2)>=value):rows[slot]=rows[(slot-1)//2];slot=(slot-1)//2
   else:
    slot=0
    while 2*slot<len(rows)-1:
     child=2*slot+1
     if child+1<len(rows) and cost(child+1)>=cost(child):child+=1
     if value>=cost(child):break
     rows[slot]=rows[child];slot=child
   row=bytearray(48);struct.pack_into('<I',row,0,bits);struct.pack_into('<Q',row,8,symbol)
   row[16:24]=info[beam*8:beam*8+8];row[24:28]=conditional[beam*4:beam*4+4]
   struct.pack_into('<Qi',row,32,0xffffffffffffffff,beam);rows[slot]=bytes(row)
 assert len(rows)==2*width and base64.b64decode(case['heap'],validate=True)==b''.join(rows)
 sort48(rows,0,len(rows)-1)
 assert base64.b64decode(case['sorted'],validate=True)==b''.join(rows)
print('PASS264 independent raw48 sort controls/tie2+4 manual vectors and120 unrestricted prepared297-class cost-row -> full heap -> sorted record joins')

#Data-only raw AL reader independently derives the source profile predicate.
import runpy
reader=runpy.run_path(str(Path(__file__).resolve().parents[1]/'export-ink-math-model.py'))
raw=Path(sys.argv[2]).read_bytes();assert hashlib.sha256(raw).hexdigest()=='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_,_,_,end=reader['header'](raw,90);rr=reader['Reader'](raw,106,end);header=rr.read('H',8);n=rr.read('H')
for inc in (32,64,1):
 table=rr.read('H',n);n=next(v+inc for v in reversed(table)if v!=65535)
spans=rr.read('H',n+1);entries=[rr.read('II')for _ in range(spans[-1]+1)];text=rr.bytes(entries[-1][0])
labels={label&65535:text[offset:entries[j+1][0]].decode('utf8')for j,(offset,label)in enumerate(entries[:-1])}
inv={s:i for i,s in labels.items()};whitespace=set(range(9,14))|{32,160,8232,8233}
ordinary=[i for i,s in labels.items()if len(s)==1 and ord(s)not in whitespace and i not in(inv['\u2402'],inv['\u2403'])]
slots=[min(ordinary),inv['\u221a'],max(ordinary),inv['<fracbar>'],inv['<slantedfracbar>'],65535,inv['\u22c5'],inv['<vopbar>']]
assert header==[65535]*7+[299] and slots==capture['observableSlots']==[2,190,261,268,276,65535,242,296]
observable=lambda i:i!=slots[5]and(slots[0]<=i<=slots[2]or i in[slots[k]for k in(3,4,6,7)if slots[k]!=65535])
mask=bytearray(8192)
for i in range(65536):
 if observable(i):mask[i//8]|=1<<(i%8)
assert base64.b64decode(capture['observableBits'],validate=True)==mask

patterns=[[],[262,263],[262,2,263],[268],[190],[296],[1,0]]
delta_bits=[0xbf800000,0,0x3f000000,0x447a0000,0x7fc00000,0x7f800000,0xff800000]
assert len(capture['eosHandoffs'])==980
assert {(c['width'],c['seed'],c['pattern'],c['deltaBits'])for c in capture['eosHandoffs']}=={(w,s,p,d)for w in(1,2,4,8,16)for s in range(4)for p in range(7)for d in delta_bits}
completed=0;any_mixed=0
for case in capture['eosHandoffs']:
 width,seed,pattern=case['width'],case['seed'],case['pattern'];rows=[];capacity=width*2
 #Reconstruct actual raw caller records through independent heap replay.
 for beam in range(width):
  for symbol in range(297):
   value=f32(1+((symbol*17+beam*29+seed*13)%100)/16)
   if symbol==1:value=f32(beam*.125+seed*.25)
   if symbol==5:value=f32(.5+beam*.25)
   at=lambda i:struct.unpack_from('<f',rows[i])[0]
   if len(rows)==capacity and value>=at(0):continue
   if len(rows)<capacity:
    slot=len(rows);rows.append(bytes(48))
    while slot and not(at((slot-1)//2)>=value):rows[slot]=rows[(slot-1)//2];slot=(slot-1)//2
   else:
    slot=0
    while slot*2<len(rows)-1:
     child=slot*2+1
     if child+1<len(rows)and at(child+1)>=at(child):child+=1
     if value>=at(child):break
     rows[slot]=rows[child];slot=child
   row=bytearray(48);struct.pack_into('<f',row,0,value);struct.pack_into('<Q',row,8,symbol);struct.pack_into('<Qi',row,32,0xffffffffffffffff,beam);rows[slot]=bytes(row)
 sort48(rows,0,len(rows)-1);delta=struct.unpack('<f',struct.pack('<I',case['deltaBits']))[0]
 if delta<1000:
  threshold=f32(struct.unpack_from('<f',rows[0])[0]+delta)
  for i in range(1,len(rows)):
   if struct.unpack_from('<f',rows[i])[0]>=threshold:rows[i]=struct.pack('<I',0x7f7fffff)+rows[i][4:]
 assert base64.b64decode(case['pruned'],validate=True)==b''.join(rows)
 expected=[]
 for rank,row in enumerate(rows):
  bits=struct.unpack_from('<I',row)[0]
  if struct.unpack_from('<Q',row,8)[0]!=1 or bits==0x7f7fffff:continue
  beam=struct.unpack_from('<i',row,40)[0];prefix=patterns[(pattern+beam)%7]
  if rank<width and any(map(observable,prefix)):
   cost=struct.unpack_from('<f',row)[0];score=f32(cost/f32((len(prefix)+6)/6))
   expected.append(dict(rank=rank,beam=beam,costBits=bits,scoreBits=struct.unpack('<I',struct.pack('<f',score))[0],prefix=prefix));completed+=1
   any_mixed+=not all(map(observable,prefix))
  rows[rank]=struct.pack('<I',0x7f7fffff)+row[4:]
 assert case['scores']==expected
 assert base64.b64decode(case['eos'],validate=True)==b''.join(rows)
 sort48(rows,0,len(rows)-1)
 assert base64.b64decode(case['resorted'],validate=True)==b''.join(rows)
 active=1
 while active<width and struct.unpack_from('<I',rows[active])[0]!=0x7f7fffff:active+=1
 assert case['active']==active
 if width==1 and seed==0 and case['deltaBits']==0xbf800000:
  assert active==1 and struct.unpack_from('<Q',rows[0],8)[0]==5 and all(struct.unpack_from('<I',x)[0]==0x7f7fffff for x in rows)
assert completed>0 and any_mixed>0
print(f'PASS actual299-label EF3 profile/all65536 predicates;980 prepared delta/EOS handoffs/{completed} completed numeric scores/{any_mixed} mixed-prefix ANY discriminators; no beam or acceptance claim')
