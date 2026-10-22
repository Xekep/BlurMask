"""Streaming independent actual FULL A8/B0/B8, every37-int callback and fresh AVL/page oracle.
No labels/NN/vendor execution. Stdlib only; processes one case at a time.
"""
from pathlib import Path
import argparse,ast,base64,hashlib,json,math,struct,re,copy
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--b3',type=Path,required=True);p.add_argument('--inputs',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);args=p.parse_args();root=args.repo;models=args.models
def definitions(path,names=None):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.pack('<f',x)
INF=0x4000000;d=(models/'en-US/en_US-ak-cur.res').read_bytes();assert hashlib.sha256(d).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
wire=definitions(root/'tools/LegacySource247Wire.Probe/audit_output.py');sampler,error=wire['sampler'](d,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']};F=lambda name:struct.unpack('<f',fields[name])[0];I=lambda name:struct.unpack('<i',fields[name])[0]
assert(F('6C'),F('70'),F('74'),F('108'),F('54'),F('58'),F('5C'),F('60'))==(1.5,-.5,f(.4),3.,1.,.5,1.5,0.)and fields['10C']==b'\1'
at=sampler['categoryOffset']+4
def ids():
 global at
 n=d[at];at=(at+4)&~3;v=list(struct.unpack_from('<'+'i'*n,d,at));at+=n*4;return v
first=ids();fm=list(struct.unpack_from('<220b',d,at));at+=220;geometry=ids();sm=list(struct.unpack_from('<220b',d,at));at+=220;em=list(struct.unpack_from('<220b',d,at));at+=220
prototypes=[struct.unpack_from('<fffB',d,at+i*13)for i in range(len(geometry))]
composites=[];start=[];end=[]
for a,b,c in zip(fm,sm,em):
 maps=[]
 for x,y in((a,b),(a,c),(first.index(0),b),(first.index(0),c)):
  if x==-1 or y==-1:maps.append(-1);continue
  item=(first[x],y)
  if item not in composites:composites.append(item)
  maps.append(composites.index(item))
 start.append(maps[0]);end.append(maps[1])
assert len(composites)==42 and composites[start[217]]==(6,7)
header,labels=definitions(root/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:373957])
assert header==[181,178,65535,180,184,182,183,217]
bypass={labels[t]for t in('\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008')if t in labels}|set(header[4:7]);bypass.discard(65535)
assert bypass=={179,182,183,184,189,190,211}
assert hashlib.sha256(d[8843:373957]).hexdigest()=='511b3f4e562cdd1a15661b7f0426255bc7d8675b76cd444e1f58c78adf2d380c'
table=[f(v)for v in json.loads((models/'ru-RU/legacy-separation-profile.json').read_bytes())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
roundFloat=definitions(root/'tools/LegacyCursiveSeparation.Probe/reference_controls.py',{'native_round_float'})['native_round_float']
def logcost(x,nearest=True):
 x=f(x);v=0 if x>4 else table[int(f(x+4)*255+.5)>>3]if x>=-4 else x;v=f(-v*8192)
 return int(roundFloat(v))if nearest else int(f(v+(.5 if v>0 else -.5)))
margins=[v for r in range(6)for v in struct.unpack('<6f',fields[f'{0x78+r*24:X}.matrixRow'])]
def environment(row):
 nodes=row['Nodes'];points=row['Cut'];height=f(row['Height']);boxes={};whole={};future={};prefix={};join={};blank={};matrix={}
 for g,ranges in row['GroupRanges'].items():
  ps=[p for r in ranges for p in points[r['StrokeIndex']][r['StartPoint']:r['EndPointExclusive']]]
  boxes[g]=[min(f(p[0])for p in ps),min(f(p[1])for p in ps),max(f(p[0])for p in ps),max(f(p[1])for p in ps)];whole[g]=all(r['StartPoint']==0 for r in ranges)
 for n in reversed(range(len(nodes))):
  for a,arc in enumerate(nodes[n]):
   low=boxes[arc['Group']][0]
   if n!=arc['Span']and n+1<len(nodes):
    for b,nextArc in enumerate(nodes[n+1]):
     if nextArc['Span']==0 and a in nextArc['Predecessors']:low=min(low,future[n+1,b])
   future[n,a]=low
 for n,arcs in enumerate(nodes):
  for a,arc in enumerate(arcs):
   box=boxes[arc['Group']];parent=n-arc['Span']-1
   if parent<0:prefix[n,a]=box[2];join[n,a]=blank[n,a]=INF;matrix[n,a]=[INF]*36
   else:
    prior=max((prefix[parent,p]for p in arc['Predecessors']),default=box[2]);prefix[n,a]=max(box[2],prior);gap=f(f(future[n,a]-prior)/height)
    join[n,a]=min(logcost(f(f(gap-F('6C'))*-F('108'))),I('120'))
    raw=f(f(f(f(box[2]-prior)/height)-F('70'))*-F('108'));blank[n,a]=min(logcost(raw),INF)
    matrix[n,a]=[]
    for i,margin in enumerate(margins):
     cost=logcost(f(f(gap-margin)*F('108')),False);matrix[n,a].append(cost-1 if i//6==5 else cost if i%6==5 else min(cost,I('124')))
   state=next(s for s in row['States']if(s['Node'],s['Arc'])==(n,a));b=state['Bounds']
   assert boxes[arc['Group']]==[f(b[k])for k in('MinimumX','MinimumY','MaximumX','MaximumY')]
   assert(bits(state['Future']),bits(state['Prefix']),state['Join'],state['Blank'],state['Matrix'])==(bits(future[n,a]),bits(prefix[n,a]),join[n,a],blank[n,a],matrix[n,a]),('state',n,a)
 def correct(stage,n,a,s,c,kind,gn,garc,gs,gc,incoming,on,oarc):
  if kind==6:return incoming,True
  if stage==140:
   delta=0
   if s!=209 and gs!=209:
    cp=prototypes[composites[c][1]];pp=prototypes[composites[gc][1]];current=boxes[nodes[n][a]['Group']];previous=boxes[nodes[gn][garc]['Group']];inverse=f(1/height)
    #Native1ACFA0 ordinary140 unions SOURCE with original ENDING(kind0).
    if (on,oarc)!=(gn,garc):
     ending=boxes[nodes[on][oarc]['Group']];previous=[min(previous[0],ending[0]),min(previous[1],ending[1]),max(previous[2],ending[2]),max(previous[3],ending[3])]
    bottom=f(cp[0]-pp[0]);top=f(bottom-f(cp[1]-pp[1]));residual=f(abs(f(f(f(current[1]-previous[1])*inverse)-top))+abs(f(f(f(current[3]-previous[3])*inverse)-bottom)))
    value=f(min(f(f(residual*F('54'))-F('58')),F('5C'))*8192);delta=int(f(value+(.5 if value>0 else -.5)))
   return incoming+delta,incoming+delta<INF
  if kind==5:
   possible=whole[nodes[n][a]['Group']]and(n+1==len(nodes)or whole[nodes[n+1][0]['Group']]);value=incoming+blank[n,a]if possible else INF;return value,possible and value<INF
  xor=(s==209)!=(gs==209)
  if kind==0 and xor or kind==2 and not whole[nodes[n][a]['Group']]:return INF,False
  if kind==2 and xor:return incoming,True
  delta=join[n,a]if kind==0 else matrix[n,a][composites[c][0]+composites[gc][0]*6]
  current=boxes[nodes[n][a]['Group']];previous=boxes[nodes[gn][garc]['Group']];displacement=f(max(f(current[0]-previous[0]),f(current[2]-previous[2]))/height)
  translate=f(f(displacement-F('74'))*F('108'))if displacement<=F('74')else 0
  value=incoming+delta-int(translate)*8192;return value,value<INF
 return correct
def replay(row,requests,trace):
 correct=environment(row);nodes=row['Nodes'];rows=[[]for _ in nodes];raw=[bytearray(200)for _ in nodes]
 for b in raw:
  for i in range(5):struct.pack_into('<i',b,i*40,INF)
 ti=0;stream=hashlib.sha256();witness={x['Index']:x['Data']for x in trace['Witnesses']}
 def call(stage,n,a,s,c,span,kind,on,oa,o,gn,ga,g,skipped,incoming):
  nonlocal ti
  cost,ok=correct(stage,n,a,s,c,kind,gn,g[3],g[5],g[7],incoming,on,o[3])
  expected=[n,a,s,c,span,kind,on,oa,o[3],o[5],o[7],o[4],o[2],o[0],o[1],int(o[6]),gn,ga,g[3],g[5],g[7],g[4],g[2],g[0],g[1],int(g[6]),skipped,incoming,cost,int(ok),stage]
  stream.update(struct.pack('<31i',*expected))
  if ti in witness:assert witness[ti]==expected,('ordered witness',ti,witness[ti],expected)
  ti+=1;return cost,ok
 for n,proposals in enumerate(requests):
  for proposal in proposals:
   a,span,s,c,cost,likelihood=[proposal[k]for k in('ArcOrdinal','Span','Symbol','CompositeCategory','GroupCost','LikelihoodByte')]
   if cost>=INF:continue
   parent=n-span-1
   if parent<0:
    if s==211 or -1 not in nodes[n][a]['Predecessors']:continue
    result=[cost,cost,255,a,span,s,True,c,likelihood]
   else:
    best=INF;result=None
    for oa,o in enumerate(rows[parent]):
     if o[3]not in nodes[n][a]['Predecessors']:continue
     gn,ga,g,skipped=parent,oa,o,0
     while g[5]in(217,65535,184,182,183):
      previous=gn-g[4]-1
      if previous==-1:break
      ga=g[2];gn=previous;g=rows[gn][ga];skipped+=1
     def correction(kind):
      value,ok=call(118,n,a,s,c,span,kind,parent,oa,o,gn,ga,g,skipped,0)
      if not ok:return INF
      value,ok=call(140,n,a,s,c,span,kind,parent,oa,o,gn,ga,g,skipped,value);return value if ok else INF
     first=second=INF
     if s not in(184,182,183)and o[5]not in(184,182,183)and(s==209)==(o[5]==209):first=correction(5 if s==217 else 6 if s==211 else 0)
     if s not in(217,65535):second=correction(2)
     delta=min(first,second)
     if delta>=INF or o[0]+delta>=INF:continue
     value=o[0]+delta+cost
     if value<best:best=value;result=[value,o[1]+cost,oa,a,span,s,second<first,c,likelihood]
    if result is None:continue
   dest=len(rows[n])
   while dest and rows[n][dest-1][0]>result[0]:dest-=1
   if dest>=5:continue
   count=min(5,len(rows[n])+1)
   for j in range(count-1,dest,-1):raw[n][j*40:(j+1)*40]=raw[n][(j-1)*40:j*40]
   off=dest*40;struct.pack_into('<ii',raw[n],off,result[0],result[1]);raw[n][off+9]=result[2];raw[n][off+10]=0
   struct.pack_into('<HHH',raw[n],off+12,result[5],int(result[6])|(result[4]<<1)|(result[7]<<4),result[3]);struct.pack_into('<Q',raw[n],off+24,0);raw[n][off+32]&=224
   rows[n].insert(dest,result);del rows[n][5:]
  for j,r in enumerate(rows[n]):raw[n][j*40+8]=next(x['LikelihoodByte']for x in proposals if x['ArcOrdinal']==r[3]and x['Symbol']==r[5])
 assert ti==trace['CallbackCount']and stream.hexdigest()==trace['CallbackSha256'],(ti,trace['CallbackCount'],stream.hexdigest(),trace['CallbackSha256'])
 return rows,[dict(Count=len(rows[n]),Bytes=base64.b64encode(b).decode())for n,b in enumerate(raw)]


def filehash(path):
 h=hashlib.sha256()
 with path.open('rb')as f:
  for b in iter(lambda:f.read(1048576),b''):h.update(b)
 return h.hexdigest()
class Cases:
 def __init__(self,path):
  self.file=path.open(encoding='utf-8-sig');self.buffer='';self.decoder=json.JSONDecoder();self.trailer=None
  while True:
   self.buffer+=self.file.read(65536);m=re.search(r'"Actual"\s*:\s*\[',self.buffer)
   if m:break
   assert len(self.buffer)<1048576,'bounded capture header'
  self.header=json.loads(self.buffer[:m.start()].rstrip().rstrip(',')+'}');self.buffer=self.buffer[m.end():]
 def __iter__(self):return self
 def __next__(self):
  while True:
   self.buffer=self.buffer.lstrip(' ,\r\n\t')
   if self.buffer.startswith(']'):
    self.trailer=json.loads('{'+(self.buffer[1:]+self.file.read()).lstrip().lstrip(','));self.file.close();raise StopIteration
   try:item,end=self.decoder.raw_decode(self.buffer)
   except json.JSONDecodeError:
    chunk=self.file.read(65536);assert chunk,'truncated capture';self.buffer+=chunk;assert len(self.buffer)<=64*1048576,'diagnostic case bound';continue
   self.buffer=self.buffer[end:];return item
class FreshPool:

    def __init__(self):
        self.nodes = {}
        self.next = 1
        self.root = 0
        self.used = []
        self.capacity = 0
        self.cursor = 0

    def height(self, node):
        return 0 if not node else 1 + max(self.height(self.nodes[node]['l']), self.height(self.nodes[node]['r']))

    def balance(self, node):
        if node:
            self.nodes[node]['balance'] = self.height(self.nodes[node]['r']) - self.height(self.nodes[node]['l'])

    def rotate_left(self, node):
        right = self.nodes[node]['r']
        self.nodes[node]['r'] = self.nodes[right]['l']
        self.nodes[right]['l'] = node
        self.balance(node)
        self.balance(right)
        return right

    def rotate_right(self, node):
        left = self.nodes[node]['l']
        self.nodes[node]['l'] = self.nodes[left]['r']
        self.nodes[left]['r'] = node
        self.balance(node)
        self.balance(left)
        return left

    def alloc(self, owner, key):
        page = -1
        if self.cursor < len(self.used) and self.used[self.cursor] < 102:
            page = self.cursor
        else:
            for n in range(len(self.used) - 1, -1, -1):
                if self.used[n] < 102:
                    page = n
                    break
        if page < 0:
            page = len(self.used)
            need = page + 1
            if need > self.capacity:
                self.capacity = max(need, int(self.capacity * 1.5))
            self.used.append(0)
        self.cursor = page
        slot = self.used[page]
        self.used[page] += 1
        node = self.next
        self.next += 1
        self.nodes[node] = dict(id=node, parent=0, child=0, l=0, r=0, key=key, balance=0, page=page, slot=slot, owner=owner)
        return node

    def insert(self, node, owner, key):
        if not node:
            leaf = self.alloc(owner, key)
            return (leaf, leaf, True)
        record = self.nodes[node]
        if key == record['key']:
            return (node, node, False)
        side = 'l' if key < record['key'] else 'r'
        record[side], leaf, created = self.insert(record[side], owner, key)
        self.balance(node)
        if record['balance'] > 1:
            right = record['r']
            if self.nodes[right]['balance'] < 0:
                record['r'] = self.rotate_right(right)
            node = self.rotate_left(node)
        elif record['balance'] < -1:
            left = record['l']
            if self.nodes[left]['balance'] > 0:
                record['l'] = self.rotate_left(left)
            node = self.rotate_right(node)
        return (node, leaf, created)

    def resolve(self, parent, key):
        if parent:
            if parent not in self.nodes:
                raise AssertionError("Foreign reference context")
            root, leaf, created = self.insert(self.nodes[parent]['child'], parent, key)
            self.nodes[parent]['child'] = root
        else:
            root, leaf, created = self.insert(self.root, 0, key)
            self.root = root
        return (leaf, created)
def lookup(group, symbol):
    cost = group['Fallback'] if symbol == 217 else next((e['Cost'] for e in group['Entries'][:20] if e['Symbol'] == symbol), INF)
    return cost if cost >= INF or symbol in g['bypass'] else cost + group['GeometryCost']
def raw(value):
    return bytearray(base64.b64decode(value))
def decode(record):
    b = raw(record['Bytes'])
    out = []
    for i in range(record['Count']):
        p = i * 40
        total, acoustic = struct.unpack_from('<ii', b, p)
        symbol, packed, arc = struct.unpack_from('<HHH', b, p + 12)
        out.append(dict(total=total, acoustic=acoustic, previous=b[p + 9], column=b[p + 10], arc=arc, span=packed >> 1 & 7, symbol=symbol, path=bool(packed & 1), category=packed >> 4 & 63, likelihood=b[p + 8], source=bytes(b[p:p + 40])))
    return out
def context_replay(source, current, captured, preparedCosts=None):
    correct = g['environment'](source)
    nodes = source['Nodes']
    rows = [decode(r) for r in current]
    trace = captured;stream=hashlib.sha256();witnesses={x['Index']:x['Data']for x in captured['ContextWitnesses']};costIndex=0
    ti = 0
    made = set()
    factory=FreshPool();mapping={};actual=captured['Contexts'];assert all(row[0]>0 for row in actual)
    physical=sorted(actual,key=lambda x:(x[7],x[8]));assert actual==physical and len({row[0]for row in actual})==len(actual)
    opaque={row[0]:i+1 for i,row in enumerate(actual)};opaque[0]=0
    def resolve(sequence):
        made.add(sequence);parent=0 if len(sequence)==1 else mapping[sequence[:-1]]
        identity,created=factory.resolve(parent,sequence[-1])
        if created and parent:factory.nodes[identity]['parent']=parent
        if sequence in mapping:assert mapping[sequence]==identity
        mapping[sequence]=identity;return identity
    tables = [[], []]
    stores = [[], []]
    groups = {v['Id']: v for v in preparedCosts['Groups']} if isinstance(preparedCosts, dict) else None

    def glyph(n, r):
        if groups is None:
            return preparedCosts[n]
        nonlocal costIndex
        value=lookup(groups[nodes[n][r['arc']]['Group']],r['symbol'])
        expected=[n,r['arc'],r['symbol'],r['category'],composites[r['category']][1],65535,1,0,value]
        assert costIndex<len(captured['ContextCosts'])and captured['ContextCosts'][costIndex]==expected,('ordered class cost',costIndex,captured['ContextCosts'][costIndex]if costIndex<len(captured['ContextCosts'])else None,expected)
        costIndex+=1;return value

    def inspect(final, n, ri, r, kind, parent, pri, ai, o, an, ar, aa, anc, skipped, stage, incoming):
        nonlocal ti
        cost, ok = correct(stage, n, r['arc'], r['symbol'], r['category'], kind, an, anc['arc'], anc['symbol'], anc['category'], incoming, parent, o['arc'])
        expected = [stage, final, n, ri, r['arc'], r['symbol'], r['category'], r['span'], kind, parent, pri, ai, o['arc'], o['symbol'], o['category'], o['span'], o['previous'], o['column'], o['total'], o['acoustic'], int(o['path']), an, ar, aa, anc['arc'], anc['symbol'], anc['category'], anc['span'], anc['previous'], anc['column'], anc['total'], anc['acoustic'], int(anc['path']), skipped, incoming, cost, int(ok)]
        stream.update(struct.pack('<37i',*expected))
        if ti in witnesses:assert witnesses[ti]==expected,('ordered context witness',ti,witnesses[ti],expected)
        ti += 1
        return (cost, ok)
    for n, currentRows in enumerate(rows):
        for final in (0, 1):
            target = [[] for _ in range(5)]
            storage = [bytearray(40) for _ in range(5)]
            for b in storage:
                struct.pack_into('<i', b, 0, INF)
            for ri, r in enumerate(currentRows):
                parent = n - r['span'] - 1
                if parent == -1:
                    identity = resolve((r['symbol'],))
                    b = bytearray(r['source'])
                    struct.pack_into('<Q', b, 24, identity)
                    storage[ri] = b
                    target[ri] = [dict(r, seq=(r['symbol'],))]
                    continue
                cost = glyph(n, r)
                if cost >= INF:
                    continue
                for pri in range(len(rows[parent])):
                    if r['symbol'] == 217 and pri != r['previous']:
                        continue
                    for ai, o in enumerate(tables[final][parent][pri]):
                        an, ar, aa, anc, skipped = (parent, pri, ai, o, 0)
                        while anc['symbol'] in (217, 65535, 184, 182, 183):
                            previous = an - anc['span'] - 1
                            if previous == -1:
                                break
                            aa = anc['column']
                            ar = anc['previous']
                            an = previous
                            anc = tables[final][an][ar][aa]
                            skipped += 1
                        if o['arc'] not in nodes[n][r['arc']]['Predecessors']:
                            continue
                        kinds = (0, 2) if final else (2,) if r['path'] else (0,)
                        for kind in kinds:
                            if kind == 2 and r['symbol'] in (217, 65535):
                                continue
                            rk = 5 if kind == 0 and r['symbol'] == 217 else 6 if kind == 0 and r['symbol'] == 211 else kind
                            value, ok = inspect(final, n, ri, r, rk, parent, pri, ai, o, an, ar, aa, anc, skipped, 140, o['total'] + cost)
                            if not ok:
                                continue
                            value, ok = inspect(final, n, ri, r, rk, parent, pri, ai, o, an, ar, aa, anc, skipped, 118, value)
                            if not ok or value >= INF:
                                continue
                            if kind == 0 and (r['symbol'] == 217 or (final and r['symbol'] == 211)):
                                sequence = o['seq']
                                identity = mapping[sequence]
                            elif not final and kind == 2:
                                sequence = (r['symbol'],)
                                identity = resolve(sequence)
                            else:
                                sequence = o['seq']
                                if final and kind == 2:
                                    sequence = sequence + (65531,)
                                    resolve(sequence)
                                sequence = sequence + (r['symbol'],)
                                identity = resolve(sequence)
                            e = dict(r, total=value, acoustic=o['acoustic'] + cost, previous=pri, column=ai, path=kind == 2, likelihood=0, seq=sequence)
                            if target[ri] and target[ri][0]['total'] <= value:
                                continue
                            target[ri] = [e]
                            b = storage[ri]
                            struct.pack_into('<ii', b, 0, e['total'], e['acoustic'])
                            b[9] = pri
                            b[10] = ai
                            struct.pack_into('<HHH', b, 12, r['symbol'], struct.unpack_from('<H', b, 14)[0] & 64512 | r['span'] << 1 | int(kind == 2) | r['category'] << 4, r['arc'])
                            struct.pack_into('<Q', b, 24, identity)
                            b[32] &= 224
            tables[final].append(target)
            stores[final].append([dict(Count=len(target[ri]), Bytes=base64.b64encode(storage[ri]).decode()) for ri in range(5)])
    assert ti==captured['ContextCallbackCount']and stream.hexdigest()==captured['ContextCallbackSha256'],('ordered37-int stream',ti,stream.hexdigest(),captured['ContextCallbackSha256'])
    assert costIndex==len(captured['ContextCosts'])
    assert made==set(mapping)and len(factory.nodes)==len(actual)==captured['ContextCount']
    expectedNodes=[]
    for n in sorted(factory.nodes.values(),key=lambda x:(x['page'],x['slot'])):expectedNodes.append([n['id'],n['parent'],n['child'],n['l'],n['r'],n['key'],n['balance'],n['page'],n['slot']])
    normalized=[[opaque[x]for x in row[:5]]+row[5:]for row in actual];assert normalized==expectedNodes,('fresh AVL/page nodes',normalized,expectedNodes)
    assert opaque[captured['ContextRoot']]==factory.root and captured['ContextPageCount']==len(factory.used)and captured['ContextPages']==factory.used and captured['ContextPageCapacity']==factory.capacity and captured['ContextCurrentPage']==factory.cursor
    def normalizeRows(value):
        result=[]
        for node in value:
            out=[]
            for row in node:
                b=bytearray(base64.b64decode(row['Bytes']));assert len(b)==40
                rawid=struct.unpack_from('<Q',b,24)[0];assert rawid in opaque;struct.pack_into('<Q',b,24,opaque[rawid]);out.append(dict(Count=row['Count'],Bytes=base64.b64encode(b).decode()))
            result.append(out)
        return result
    assert stores[0]==normalizeRows(captured['B0'])and stores[1]==normalizeRows(captured['B8']),'full normalized B0/B8 storage'
    return sum(len(r)for t in tables for n in t for r in n),ti,len(factory.nodes),costIndex
def shortlist(source, captured, costRow):
    groups = {v['Id']: v for v in costRow['Groups']}
    for n, record in enumerate(captured['Primary']):
        rows = decode(record)
        seen = set()
        owners = []
        counts = [0] * 5
        storage = bytearray(200)
        for ri, r in enumerate(rows):
            arc = r['arc']
            if arc in seen:
                continue
            seen.add(arc)
            index = len(owners)
            owners.append(ri)
            group = groups[source['Nodes'][n][arc]['Group']]
            entries = group['Entries']
            values = [f(math.exp(f(-f(e['Cost']) / 8192))) for e in entries[:20]]
            total = 0.0
            for value in values:
                total = f(total + value)
            weights = [1.0] if len(entries) == 1 else [f(v / total) for v in values]
            for entry, weight in zip(entries, weights):
                if entry['Symbol'] in (217, 211):
                    continue
                off = index * 40
                struct.pack_into('<ii', storage, off, r['total'], r['acoustic'])
                storage[off + 8] = max(1, min(255, int(f(weight * 255))))
                struct.pack_into('<H', storage, off + 12, entry['Symbol'])
                struct.pack_into('<H', storage, off + 16, arc)
                counts[index] = 1
                break
        for ri, count in enumerate(counts):
            if count == 0:
                struct.pack_into('<i', storage, ri * 40, INF)
        assert captured['A8'][n] == dict(Groups=len(owners), PrimaryOrdinals=owners, Counts=counts, Bytes=base64.b64encode(storage).decode()), ('full A8', n)

g=dict(environment=environment,bypass=bypass);upstream=Cases(args.b3);output=Cases(args.capture)
assert upstream.header['TargetsRead']is False and output.header['TargetsRead']is False
assert filehash(args.b3)in{'84ce82204df3574f897438abed14cd29192c623ebd646d0e10dc61010420d6a7','c82d59d4a854a181d16f142a83ff9a9ca135c957961a441d676940fd9dfb9a07'}
inputs=json.loads(args.inputs.read_bytes());inputs=inputs if isinstance(inputs,list)else inputs['samples'];actual=children=records=callbacks=contexts=classcosts=mutations=a8nodes=0
def reservation(prior):
 G=A=N=E=K=V=0
 for c in prior['Children']:
  nodes=c['Nodes'];counts=[x['Count']for x in c['Primary']];cn=len(nodes);ca=sum(map(len,nodes));D=P=T=R=k=0
  G+=len(c['Groups']);A+=ca;N+=cn
  for n,node in enumerate(nodes):
   roots=0
   for a in node:
    E+=len(a['Successors']);D=max(D,len(a['Successors']));P=max(P,len(a['Pred']))
    if a['Span']==n and -1 in a['Pred']:E+=1;roots+=1
   D=max(D,roots)
   for entry in decode(c['Primary'][n]):
    R+=1;parent=n-entry['span']-1
    if parent==-1:k+=2
    else:T+=counts[parent];k+=4*counts[parent]
  K+=k;V+=1024*cn+512*ca+320*R+2*T*(cn+D+32)+6*T*(P+128)+128*k
 assert G<=A<=4096 and N<=4096 and E<=1000000 and K<=32768 and V<=128*1048576
 return dict(B4Peak=4*1048576+4096*A+4096*N+512*G+16*E+320*K,B4Work=4*1048576+24576*A+16384*N+1024*G+24*E+1536*K,B4Visits=V,B4Resolves=K)
for item,prior,row in zip(inputs,upstream,output):
 actual+=1;expectedReservation=reservation(prior);assert all(row[k]==v for k,v in expectedReservation.items()),('B4 reservation',row['Name'],expectedReservation);assert sum(c['ContextCount']for c in row['Children'])<=expectedReservation['B4Resolves'];assert row['Name']==prior['Name']==item.get('Name',item.get('id'))and row['Status']=='AcceptedContext'and prior['Status']=='AcceptedPrimary'
 for key in ['NetworkCount','LogicalAdditional','LogicalCombined','B3Peak','B3Work','B3Visits']:assert row[key]==prior[key],('upstream count/reservation changed',key)
 assert len(row['Children'])==len(prior['Children'])
 for c,b in zip(row['Children'],prior['Children']):
  for key,value in b.items():assert c[key]==value,('B3 golden changed',row['Name'],key)
  source=dict(Nodes=[[dict(Group=a['Group'],Span=a['Span'],Predecessors=a['Pred'])for a in node]for node in b['Nodes']],Cut=b['Raw'],Height=b['Height'],GroupRanges={g['Group']:[dict(StrokeIndex=s,StartPoint=a,EndPointExclusive=z)for s,a,z in g['Stored']]for g in b['Groups']},States=[])
  for n,node in enumerate(b['Nodes']):
   for a,x in enumerate(node):source['States'].append(dict(Node=n,Arc=a,Bounds=dict(zip(('MinimumX','MinimumY','MaximumX','MaximumY'),x['Bounds'])),Future=x['Future'],Prefix=x['Prefix'],Join=x['Join'],Blank=x['Blank'],Matrix=x['Matrix']))
  groups=dict(Groups=[dict(Id=g['Group'],Fallback=g['Fallback'],GeometryCost=g['Geometry'],Entries=[dict(Symbol=s,Cost=cost,NormalizedLikelihood=w)for s,cost,w,_ in g['Entries']])for g in b['Groups']])
  a8nodes+=len(b['Nodes']);shortlist(source,c,groups);v=context_replay(source,b['Primary'],c,groups);records+=v[0];callbacks+=v[1];contexts+=v[2];classcosts+=v[3];children+=1
  if mutations==0 and c['ContextCosts']:
   for field in [4,5,6,7]:
    saved=c['ContextCosts'][0][field];c['ContextCosts'][0][field]=saved^1
    try:context_replay(source,b['Primary'],c,groups)
    except AssertionError as error:
     assert error.args and isinstance(error.args[0],tuple)and error.args[0][0]=='ordered class cost';mutations+=1
    else:raise AssertionError('Missed mutated prototype/ordinal/flags')
    finally:c['ContextCosts'][0][field]=saved
assert actual==len(inputs)
#Exhaust both readers to require exact denominator and parse root trailers.
assert next(upstream,None)is None and next(output,None)is None
assert output.trailer['Accepted']==actual and output.trailer['Refused']==0 and output.header['Guards']>=36 and output.header['Cancellations']>=7 and mutations==4
report=dict(status='PASS',actual=actual,accepted=actual,refused=0,children=children,a8Nodes=a8nodes,contextRecords=records,ordered37IntCallbacks=callbacks,symbolicNodes=contexts,orderedClassCosts=classcosts,flatCostArgumentMutationsRejected=mutations,guards=output.header['Guards'],cancellations=output.header['Cancellations'],captureSha256=filehash(args.capture),scope='Independent full raw sampler/B3 goldens/A8/B0/B8 and fresh height-AVL/pages; no borrowed captured path authority, no WORD',vendorExecuted=False,targetsRead=False);args.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
