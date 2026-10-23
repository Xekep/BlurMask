from pathlib import Path
import bisect,collections,copy,hashlib,json,runpy,struct,sys
R=Path(__file__).resolve().parents[2]
models=Path(sys.argv[1])
D=Path(sys.argv[3]);assert not D.exists();D.mkdir(parents=True)
actual=Path(sys.argv[2]);q=json.loads(actual.read_bytes())
sys.argv=['reference_actual',str(models),str(D/'raw-language.json')]
g=runpy.run_path(str(R/'tools/LegacyEnglishLkSparse101.Probe/reference_actual.py'),run_name='__main__')
a=g['arrays'];raw=g['raws'];cl=g['classCompiled'];bk=g['backoffCompiled'];costs={};backs={}
for first in range(1024):
    costs[(first,)]=cl[0][raw['C0.0'][first]];backs[(first,)]=bk[0][raw['C8.0'][first]]
    for j in range(a['B8.0'][first],a['B8.0'][first+1]):
        pair=(first,a['B0.1'][j]);costs[pair]=cl[1][raw['C0.1'][j]];backs[pair]=bk[1][raw['C8.1'][j]]
        for k in range(a['B8.1'][j],a['B8.1'][j+1]):costs[pair+(a['B0.2'][k],)]=cl[2][raw['C0.2'][k]]

def validate(c,index):
    x=c['input'];n=x['HistoryCount'];assert n in (0,1,2)
    h=[x['History0'],x['History1']][:n];classes=[v&0xffffff for v in h];tags=[v>>24&15 for v in h]
    if x['ClearFirstTag'] and tags:tags[0]=0
    current=a['88'][x['Word']];seq=tuple(classes)+(current,);base=0
    for at in range(len(seq)):
        suffix=seq[at:]
        if suffix in costs:base+=costs[suffix];matched=len(suffix);break
        if suffix[:-1] in backs:base+=backs[suffix[:-1]]
    base+=g['wordCompiled'][raw['98.wordCodes'][x['Word']]]
    if x['Boundary']:base+=g['compiled']['boundary94C']
    prefix=[]
    if x['Category']:
        for tag in tags:
            if tag==0:break
            prefix.append(tag)
    category=x['Category']
    if not x['SuppressCategory']:
        if len(prefix)==2:base+=g['caseCost'](g['case3'][prefix[0]*169+prefix[1]*13+category])
        elif len(prefix)==1:base+=g['caseCost'](g['case2'][prefix[0]*13+category])
        else:base+=(-2 if x['ClearFirstTag'] else 1) if category==3 else 2 if category in (5,6) else 3 if category in (10,11,12) else 0
    retained=(classes+[current])[-2:]
    tagsout=prefix if x['SuppressCategory'] else (prefix+[category])[-2:]
    retained=[v|((tagsout[i] if i<len(tagsout) else 0)<<24) for i,v in enumerate(retained)]
    before=bytes((i*37+index*11)&255 for i in range(40));assert c['before']==before.hex().upper()
    out=bytearray(before);struct.pack_into('<ii',out,0,base,matched);out[8]=len(retained)
    for i,v in enumerate(retained):struct.pack_into('<I',out,12+i*4,v)
    struct.pack_into('<ii',out,24,-1,x['Word'])
    mask=0xff0001ff|(15<<12)|((15<<16) if len(retained)==2 else 0)
    assert c['cost']==base and c['matched']==matched and c['count']==len(retained) and c['mask']==mask
    assert bytes.fromhex(c['after'])==out
    return matched

assert q['actualNetworkForwards']==0 and q['guards']==9 and q['caseCount']==len(q['cases'])==57096
orders=collections.Counter(validate(c,i) for i,c in enumerate(q['cases']))
assert q['matchedCounts']==[0,orders[1],orders[2],orders[3]]
negatives=[]
for name,change in [
    ('cost',lambda c:c.__setitem__('cost',c['cost']+1)),
    ('matched',lambda c:c.__setitem__('matched',c['matched']+1)),
    ('history-count',lambda c:c.__setitem__('count',c['count']+1)),
    ('mask',lambda c:c.__setitem__('mask',c['mask']^1)),
    ('category',lambda c:c['input'].__setitem__('Category',3)),
    ('allocator-residue',lambda c:c.__setitem__('before','FF'+c['before'][2:])),
    ('record',lambda c:c.__setitem__('after','FF'+c['after'][2:]))]:
    c=copy.deepcopy(q['cases'][0]);change(c)
    try:validate(c,0)
    except AssertionError:negatives.append(name)
    else:raise AssertionError('Tamper accepted: '+name)
report=dict(cases=57096,guards=9,matchedOrders=dict(orders),negativeControls=negatives,
    fullRecord40BytesExact=True,knownWriteMaskExact=True,actualNetworkForwards=0,
    captureSha256=hashlib.sha256(actual.read_bytes()).hexdigest(),
    scope='Pure known-word/tagged-history/order3 numerical records; no live source/candidate authority or handwriting quality claim')
(D/'report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(json.dumps(report,indent=2))
