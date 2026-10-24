import base64, copy, importlib.util, json, struct, sys
from pathlib import Path
B=Path(__file__).parent
sp=importlib.util.spec_from_file_location('audit',B/'audit_raw.py');a=importlib.util.module_from_spec(sp);sp.loader.exec_module(a);r=a.r

def word(ref=r.NIL,label=0):
    b=bytearray(120);struct.pack_into('<Q',b,8,ref);struct.pack_into('<f',b,16,0.5);return bytes(b)
def glyph(label,nxt=r.NIL,flags=0,kind=0):
    b=bytearray(40);struct.pack_into('<H',b,4,kind);struct.pack_into('<H',b,8,label);b[6]=flags;struct.pack_into('<Q',b,32,nxt);return bytes(b)
def before(l,rw,sa=b'\0'*4,sb=b'\0'*4,ignored=65535):return l+rw+sa+sb+struct.pack('<H',ignored)
def read(ref,row):return (6,struct.pack('<Q',ref)+row)
assert r.equality(before(word(),word()),[])==bytes([1])+bytes(8)
assert r.equality(before(word(1),word(2)),[read(1,glyph(3)),read(2,glyph(3))])[0]==1
assert r.equality(before(word(1),word(2)),[read(1,glyph(3)),read(2,glyph(4))])[0]==0
assert r.equality(before(word(1),word(2)),[read(1,glyph(3)),read(2,glyph(8,3,2)),read(1,glyph(3)),read(3,glyph(3))])[0]==1
ctl=struct.pack('<H',9)+' '.encode('utf-16-le')
assert r.equality(before(word(1),word(2)),[read(1,glyph(6,kind=3)),read(2,glyph(9)),(7,ctl)])[0]==1
assert r.equality(before(word(1),word(2),b'\x08\0\x01\x73',b'\x08\0\x01\x73'),[read(1,glyph(0,kind=3)),read(2,glyph(0,kind=3))])==bytes([1])+b'\x08\0\x01\x73'*2
try:r.equality(before(word(1),word(2)),[(7,ctl),read(1,glyph(6,kind=3)),read(2,glyph(9))]);raise RuntimeError('accepted reordered control')
except AssertionError:pass
# Three independent numeric ranking discriminators (no owner/capture authority).
def ranked_word(cost,key,flags=0):
    b=bytearray(word());struct.pack_into('<f',b,16,cost);struct.pack_into('<i',b,60,key);struct.pack_into('<H',b,50,flags);return bytes(b)
a0,a1=ranked_word(.8,1),ranked_word(.2,2)
def forbidden(a,b):raise AssertionError('unexpected comparison across unequal DWORD60')
x=r.ranking([a0,a1],[1,0],forbidden)
assert x['words']==[a1,a0] and x['mapping']==[1,0] and x['sources']==[1,0] and x['quota']==2
low,high,tie=ranked_word(.1,0),ranked_word(.7,0),ranked_word(.7,0)
x=r.ranking([low,high,tie],[0,2],lambda a,b:True)
assert x['words']==[high] and x['sources']==[1] and x['mapping']==[0,0,0] and x['releasedsources']==[0,2]
ws=[ranked_word(5-i,i,0x100 if i==2 else 0x200 if i==4 else 0) for i in range(5)]
x=r.ranking(ws,[0,1],forbidden)
assert x['words']==[ws[0],ws[1],ws[2],ws[4]] and x['quota']==4 and x['released']==[3]
# Model-free schema/route fixture. It is SYNTHETIC DATA, never a configured owner.
ev=[];candidate=-1;number=0
add=lambda k,arg,b:ev.append(dict(kind=k,argument=arg,raw=base64.b64encode(b).decode()))
def ints(k,v):add(k,candidate,struct.pack('<'+'i'*len(v),*v))
def snap(k,words,selected):
    add(k,len(words),struct.pack('<iiii',0,128,*selected))
    for w in words:add(19,candidate,w)
def equal(l,rw):
    global number
    number+=1;add(5,number,before(l,rw));add(8,number,bytes([1])+bytes(8))
w=word();initial=[w,w];compact=bytearray(80);compact[32]=compact[72]=8;struct.pack_into('<i',compact,36,-1);struct.pack_into('<i',compact,76,-1);add(16,-1,bytes(compact));ints(17,[0,2]);snap(18,initial,[0,1]);ints(2,[0,1])
for candidate in (0,1):
    add(1,candidate,b'');add(3,candidate,b'');add(4,candidate,w);equal(w,w);add(9,candidate,struct.pack('<iiii',0,0,candidate+1,0))
snap(10,initial,[0,1]);equal(w,w)
add(20,candidate,w)
row0=struct.pack('<iif',0,0,.5)+bytes([1,1,0,0,1,0,0,0]);row1=struct.pack('<iif',1,0,-r.MAX)+bytes([0,1,0,0,0,1,0,0])
add(21,candidate,row0);add(21,candidate,row1)
for k,v in ((22,[0,0]),(23,[1]),(24,[0]),(25,[1])):ints(k,v)
add(11,2,b'');snap(12,[w],[0,0]);norm=bytearray(w);struct.pack_into('<ff',norm,16,1.,1.);norm=bytes(norm)
add(13,candidate,norm);ints(14,[0]);snap(15,[norm,norm],[0,0])
# Independent storage accounting expected from schema + actual-copy call counts.
pc=hc=used=count=0;allocated=192;visits=5824;copied=0
for e in ev:
    length=len(base64.b64decode(e['raw']));need=used+length
    if need>pc:
        cap=min(32*1024*1024,max(need,max(4096,pc*2)));visits+=2*cap+used;allocated+=cap;copied+=used;pc=cap
    if count==hc:
        cap=min(32768,max(64,hc*2));visits+=32*cap+16*count;allocated+=16*cap;copied+=16*count;hc=cap
    visits+=4*length+144;copied+=length;used=need;count+=1
visits+=240*(2+2+2+1+2);allocated+=120*(2+2+2+1+2);copied+=120*(2+2+2+1+2)
debit=dict(copies=312,work=192,phase=2,activeGroup=-1,sites=[dict(site=i,copies=312 if i==0 else 0,work=192 if i==0 else 0) for i in range(12)])
data=dict(schema=1,complete=True,faulted=False,marked=True,diagnosticFailure=False,allocatedLogical=allocated,memoryVisits=visits,copyBytes=copied,pendingClearBytes=pc+hc*16,sourceDebit=debit,events=ev)
report=a.audit(data)
mutations=[('bool-schema',lambda d:d.__setitem__('schema',True)),('bool-ledger',lambda d:d.__setitem__('memoryVisits',True)),('mark',lambda d:d.__setitem__('marked',False)),('diagnostic-fault',lambda d:d.__setitem__('diagnosticFailure',True)),('unknown-field',lambda d:d.__setitem__('forged',1)),('extra-event',lambda d:d['events'].append(d['events'][-1])),('drop-event',lambda d:d['events'].pop(6)),('word-bit',lambda d:d['events'][3].__setitem__('raw',base64.b64encode(bytes([1])+w[1:]).decode())),('bool-argument',lambda d:d['events'][0].__setitem__('argument',False)),('bad-base64',lambda d:d['events'][0].__setitem__('raw','AA==\n')),('false-compare',lambda d:next(e for e in d['events'] if e['kind']==8).__setitem__('raw',base64.b64encode(bytes(9)).decode())),('copy-charge',lambda d:d.__setitem__('copyBytes',d['copyBytes']-1))]
mutations.extend([
 ('debit-total-bool',lambda d:d['sourceDebit'].__setitem__('copies',True)),
 ('debit-work-float',lambda d:d['sourceDebit']['sites'][0].__setitem__('work',192.0)),
 ('debit-site-bool',lambda d:d['sourceDebit']['sites'][0].__setitem__('site',False)),
 ('debit-wrong-phase',lambda d:d['sourceDebit'].__setitem__('phase',1)),
 ('debit-active-group',lambda d:d['sourceDebit'].__setitem__('activeGroup',0)),
 ('debit-missing-site',lambda d:d['sourceDebit']['sites'].pop()),
 ('debit-reordered-sites',lambda d:d['sourceDebit']['sites'].reverse()),
 ('debit-sum-copies',lambda d:d['sourceDebit'].__setitem__('copies',313)),
 ('debit-sum-work',lambda d:d['sourceDebit'].__setitem__('work',193)),
 ('debit-owner-site',lambda d:(d['sourceDebit'].__setitem__('copies',313),d['sourceDebit']['sites'][0].__setitem__('copies',313))),
 ('debit-neural-product',lambda d:(d['sourceDebit'].__setitem__('copies',313),d['sourceDebit']['sites'][10].__setitem__('copies',1))),
 ('debit-extra-selector',lambda d:d['sourceDebit'].__setitem__('selectedSite',10)),
 ('debit-neural-work-product',lambda d:(d['sourceDebit'].__setitem__('copies',312+14824),d['sourceDebit'].__setitem__('work',192+386625),d['sourceDebit']['sites'][10].update(copies=14824,work=386625))),
 ('debit-neural-ratio',lambda d:(d['sourceDebit'].__setitem__('copies',312+29648),d['sourceDebit'].__setitem__('work',192+386624),d['sourceDebit']['sites'][10].update(copies=29648,work=386624)))])
names=[]
for name,mutate in mutations:
    d=copy.deepcopy(data);mutate(d)
    try:a.audit(d)
    except (AssertionError,ValueError):names.append(name);continue
    raise AssertionError(('accepted',name))
out=Path(sys.argv[1]);assert len(sys.argv)==2 and not out.exists()
out.write_text(json.dumps(dict(syntheticOnly=True,owningExecutions=0,equalityControls=7,rankingControls=3,routeSchemaControl=report,negativeControls=names),indent=2)+'\n')
print('PASS7 equality/3 ranking pure controls + synthetic DATA route +26 negative controls; no owning execution')
