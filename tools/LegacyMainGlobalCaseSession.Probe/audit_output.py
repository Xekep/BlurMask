"""Independent pool lifecycle, raw permissions/maps and case root-page oracle.

Query records are deliberately source-level root-page exercises at each D0 key;
they are not represented as requests produced by the full word scheduler.
"""
import ast,base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=6:raise ValueError('AK LK case-preparation-controls actual-global-acoustic-capture session-capture')
ak,lk,classification,acoustic,capture=map(Path,sys.argv[1:]);root=Path(__file__).resolve().parents[2]
classes={r[0]:r for r in json.loads(classification.read_text(encoding='utf8'))[0]['rows']}
tree=ast.parse((root/'tools/LegacyCaseQuery.Probe/reference_controls.py').read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='POLICIES' for t in n.targets)],type_ignores=[]),'pinned-case-policy','exec'),ns)
policy=ns['POLICIES'][512];transition=policy['rows'][0];alternate=policy['alternate'][0]
tree=ast.parse((root/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf8'));ns={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'raw-al-reader','exec'),ns)
rawAL=ak.read_bytes()[99:];a=ns['read'](rawAL);l=ns['read'](lk.read_bytes()[607074:]);labels={s:i for i,s in a['text'].items()}
control={'\0':65533,' ':65531,'\n':65529,'\u00a0':65532,'\ufffd':65535,'\U000f000c':65530}
primary=[65535 if l['reverse'][i]==65535 else control.get(l['text'][i],labels.get(l['text'][i],65535)) for i in range(l['count'])]
at=a['positions'][0]-4;triplets=[struct.unpack_from('<3H',rawAL,at-a['forms']*6+i*6) for i in range(a['forms'])]
header=struct.unpack_from('<8H',rawAL);reserved=list(header[4:7])+[labels.get(t,65535) for t in ['\U000f0000','\u200c','\u200d']]
cases=json.loads(capture.read_text());networks=json.loads(acoustic.read_text());assert len(cases)==len(networks)==8
spans=rows=computations=reuses=0
for case,nn in zip(cases,networks):
    raw=bytes.fromhex(case['raw']);assert len(raw)==132*32
    contexts={r[0]:r[1:] for r in case['contexts']};expectedkeys=[];pool=[];records={};totalcomputed=0
    assert case['epochs']==list(range(-1,case['nodeCount']))
    for node,arcs in enumerate(case['arcs']):
        extent=max((arc[1]+1 for arc in arcs),default=0)
        expectedkeys.extend((node,span)for span in range(extent))
    assert [(s['node'],s['span'])for s in case['spans']]==expectedkeys
    for scope in case['spans']:
        node,span=scope['node'],scope['span'];present={g for g,s in case['arcs'][node]}
        for item in pool:
            if item['span']==span and item['group']not in present:item['group']=-1
        selected=[g for g,s in case['arcs'][node]if s==span];count=0;reused=0
        for g in selected:
            if any(item['group']==g for item in pool):reused+=1
            else:
                vacant=next((item for item in pool if item['group']==-1),None)
                if vacant is None:vacant={};pool.append(vacant)
                vacant.update(group=g,span=span);records[g]=vacant;count+=1
        totalcomputed+=count;assert (scope['computed'],scope['reused'],scope['evaluations'])==(count,reused,totalcomputed)
        assert scope['costs']==[nn['groups'][g]['cacheCosts']for g in selected]
        p=bytearray(a['count']);eligible=bytearray(a['count']+3)
        for label in reserved:
            if label!=65535:p[label]=3
        for costs in scope['costs']:
            assert len(costs)==195
            for symbol,cost in enumerate(costs):
                if cost<0x4000000:eligible[symbol]=1
                if symbol<a['count']:p[symbol]|=(3 if cost<0x4000000 else 0)|(4 if cost<40000 else 0)
        assert base64.b64decode(scope['permissions'])==p and base64.b64decode(scope['eligibility'])==eligible
        for lower,upper,title in triplets:
            for src,dst in [(upper,lower),(title,lower),(title,upper),(lower,upper)]:
                if src!=65535 and dst!=65535:p[dst]|=p[src]
        assert scope['map']==[65535 if label<a['count'] and not p[label]&1 else label for label in primary]
        expected=[]
        for at in range(0,len(raw),32):
            row=raw[at:at+32];label=struct.unpack_from('<H',row,20)[0]
            if label<a['count'] and not p[label]&1:continue
            kind=classes[label][2 if alternate else 1];forms=classes[label][4 if alternate else 3];flags=row[5]&3;plan=[]
            def emit(column,form=-1,cost=0,force=False):
                if force or transition[column]!=255:plan.append((transition[column]&31,form,cost))
            if kind==1:
                emit(3);emit(6,1,1)
                if transition[6]!=255 and flags==3:emit(11,1,1,True)
            elif kind==2:
                emit(3);emit(7,1,1)
                if transition[7]!=255 and flags==3:emit(12,1,1,True)
                emit(8,2,1)
                if transition[8]!=255 and flags==3:emit(13,2,1,True)
            elif kind==3:emit(4);emit(10,0,8192)
            elif kind==4:emit(5);emit(9,2,1);emit(10,0,(1 if transition[9]!=255 else 0)+8192)
            else:plan=[((0 if kind==7 else transition[{5:0,6:1}.get(kind,2)])&31,-1,0)]
            for state,form,cost in plan:
                value=bytearray(row);value[4]=1;value[7]=(value[7]&3)|(state<<2);struct.pack_into('<i',value,16,struct.unpack_from('<i',row,16)[0]+cost)
                if form>=0:struct.pack_into('<H',value,20,forms[form])
                expected.append((value,form>=0))
        actual=bytes.fromhex(scope['records']);assert len(actual)==len(expected)*32
        for at,(value,rewrite) in enumerate(expected):
            row=actual[at*32:(at+1)*32];identity=struct.unpack_from('<Q',row,8)[0]
            assert contexts[identity]==[0,struct.unpack_from('<H',row,20)[0]]
            if rewrite:struct.pack_into('<Q',value,8,identity)
            assert value==row,(node,span,at)
        spans+=1;rows+=len(expected);computations+=count;reuses+=reused
print(f'PASS independent8 global owners/{spans} ordered D0 spans/{computations} computed/{reuses} reused; actualNN195-cache alignment, acoustic flags/one case propagation/maps/{rows} fulltag root records; no scheduler/winner claim')
