"""Synthetic SOURCE/numeric controls; no actual resources or model constructors."""
from pathlib import Path
import argparse,hashlib,json,struct,importlib.util,copy,ast
HERE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('global_reference_synthetic',HERE/'global_language_reference.py');g=importlib.util.module_from_spec(spec);spec.loader.exec_module(g)
passed=negative=0
def yes(value,label):
    global passed
    assert value,label;passed+=1
def reject(fn,label):
    global negative
    try:fn()
    except (AssertionError,ValueError,KeyError,TypeError,IndexError):negative+=1;return
    raise AssertionError('Synthetic corruption accepted '+label)
def genuine(before,after):yes(json.dumps(before,sort_keys=True)!=json.dumps(after,sort_keys=True),'typed genuine mutation')
def fake_model(word_cost=10,class_cost=20):
    m=object.__new__(g.RawLanguage);raw=bytearray(3503884+1024);raw[3503884+5]=7;m.raw=bytes(raw);m.classes=bytes([5,0]);m.words=[word_cost]*256;m.unigrams=[class_cost]*256;m.boundary=0
    return m
def fake_profile_resource():
    raw=bytearray(4565174);struct.pack_into('<5I',raw,2513213,2,3,439826,1024,1024)
    base=4565174-(2+16+16+0x2a4+0x2254)-128;struct.pack_into('<H',raw,base+128,0x200)
    struct.pack_into('<IIf',raw,607056,0,1,1.0)
    for offset,value in [(80,1.25),(40,.125),(16,.0625),(84,.75),(20,.25),(56,0.0)]:struct.pack_into('<f',raw,base+offset,value)
    for offset in [3502860,4553492]:struct.pack_into('<256i',raw,offset,*([0,8192,-8192,4096,-4096]+[0]*251))
    return raw
def allocation_inputs(nodes=2):
    return [{'Kind':kind,'Node':node,'Identity':1+node*2+k,'Bytes':__import__('base64').b64encode(bytes([37+node+k])*size).decode()} for node in range(nodes) for k,(kind,size) in enumerate([('PublicationLanguageHeap',440),('PublicationCandidateHeap',1176)])]
def fake_unicode():
    case={'format':'legacy-native-case-data-v1','engineSha256':g.ENGINE,'tableSha256':''};integers=[]
    for name,count,width in [('alternatingRanges',58,2),('lowerRanges',34,3),('lowerSingles',89,2),('upperRanges',35,3),('upperSingles',110,2),('titleSingles',8,2),('specialRows',130,9),('sequenceScalars',348,1)]:
        case[name]=[[0]*width for _ in range(count)];integers += [count]+[0]*(count*width)
    case_hash=hashlib.sha256(struct.pack('<'+'i'*len(integers),*integers)).hexdigest();case['tableSha256']=case_hash
    pub={'format':'legacy-native-text-publication-data-v1','engineSha256':g.ENGINE,'textSha256':g.TEXT,'tableSha256':''};payload=bytearray()
    for name,count,width in [('scriptRanges',716,3),('controlRanges',19,2),('extendRanges',225,2),('prependRanges',11,2),('joinerRanges',77,2)]:
        pub[name]=[[i*2,i*2+1]+([1] if width==3 else []) for i in range(count)]
        payload.extend(struct.pack('<i',count))
        for row in pub[name]:payload.extend(struct.pack('<'+'i'*width,*row))
    pub['literalRows']=[];payload.extend(struct.pack('<i',120))
    for i in range(120):
        raw=('L%03d'%i).encode();pub['literalRows'].append([raw.hex(),0,0,0,0,0]);payload.extend(struct.pack('<i',len(raw))+raw+bytes(20))
    pub_hash=hashlib.sha256(payload).hexdigest();pub['tableSha256']=pub_hash
    return case,pub,case_hash,pub_hash
def main():
    p=argparse.ArgumentParser();p.add_argument('output');a=p.parse_args();out=Path(a.output);assert not out.exists()
    #Hand-derived equal-cost insertion: strict comparison retains first root.
    native_residue=bytearray([91])*1176
    for slot in range(21):native_residue[slot*56+52:slot*56+56]=bytes([slot])*4
    heap=g.Heap56(2,native_residue)
    for ordinal,cost in enumerate([3,1,2,3]):
        slot=heap.allocate(cost);heap.fill(slot,ordinal,0,cost,1,101+ordinal,0,0,0,-1)
    yes([x['cost'] for x in heap.candidates()]==[3,3,2,1],'hand expected heap costs')
    yes([x['tag'] for x in heap.candidates()]==[101,104,103,102],'hand expected strict tie identities')
    yes([row[52] for row in heap.rows[1:5]]==[1,2,3,2],'full-row moved/retained residue hand expectation')
    snapshot=[bytes(x) for x in heap.rows];heap.reset();yes(heap.count==0 and snapshot==[bytes(x) for x in heap.rows],'ordinal count reset preserves all residue')
    slot=heap.allocate(1);heap.fill(slot,9,1,1,2,88,0,0,0,17);yes(heap.rows[slot][52:]==bytes([1])*4,'undefined tail remains allocator/moved residue')
    yes(heap.candidates()[0]['aux']==17,'auxiliary is explicit48 lane')
    full=g.Heap56(2,bytes(1176))
    for cost in range(20):slot=full.allocate(cost);full.fill(slot,cost,0,cost,0,cost,0,0,0,-1)
    saved=[bytes(x) for x in full.rows];yes(full.allocate(19)==0 and saved==[bytes(x) for x in full.rows],'full equal root rejection no mutation')
    slot=full.allocate(18);full.fill(slot,99,0,18,0,99,0,0,0,-1)
    yes(sorted(x['cost'] for x in full.candidates())==list(range(19))+[18],'full root eviction multiset')
    yes(full.allocate(g.IMP)==0,'IMP rejection')
    for x in [-2**31,-1,0,g.IMP-1]:
        h=g.Heap56(2,bytes(1176));yes(h.allocate(x)==1 and g.i32(h.rows[1],8)==x,'signed cost domain')
    yes(g.wrap(2**31-1+10)==-2147483639,'combined candidate modular arithmetic')
    tree_starts=[1,3,5,7,7,7,7,7];tree_labels=bytes([0,128,128,0,128,128,128])
    yes(g.preorder_terminals(tree_starts,tree_labels,8,{1,2,6},5)=={1:0,2:2,6:4},'hand DFS terminal preorder differs from numeric node order')
    reject(lambda:g.preorder_terminals([1,2,1],bytes([0,128]),8,{1},1),'malformed/cyclic tree extent')
    raw_profile=fake_profile_resource();old_lk_sha=g.LK_SHA;g.LK_SHA=g.sha(raw_profile)
    try:
        transformed=g.RawLanguage(bytes(raw_profile))
        yes(transformed.words[:5]==[-1024,9216,-11264,4096,-6144],'hand exact f32 word affine levels')
        yes(transformed.unigrams[:5]==[-512,9728,-10752,4608,-5632] and transformed.boundary==1024,'hand exact f32 unigram/boundary stages')
        bad=bytearray(raw_profile);struct.pack_into('<H',bad,4565174-(2+16+16+0x2a4+0x2254),0);g.LK_SHA=g.sha(bad);reject(lambda:g.RawLanguage(bytes(bad)),'unsupported raw TCM version')
        bad=bytearray(raw_profile);struct.pack_into('<i',bad,3502860,2**31-1);g.LK_SHA=g.sha(bad);reject(lambda:g.RawLanguage(bytes(bad)),'out-of-int32 synthetic affine result')
    finally:g.LK_SHA=old_lk_sha
    residue=bytes(range(40));token=bytearray(16);struct.pack_into('<iii',token,0,1,0,3)
    model=fake_model();record=model.evaluate(token,residue);expected=bytearray(residue);struct.pack_into('<iI',expected,0,31,1);expected[8]=1;struct.pack_into('<I',expected,12,5|(3<<24));struct.pack_into('<Ii',expected,24,0xffffffff,0)
    yes(record==expected,'complete native40 hand-written lanes and untouched residue')
    token[13]=1;yes(g.i32(model.evaluate(token,residue),0)==30,'suppress category empty history')
    for category,delta in [(0,0),(3,1),(5,2),(6,2),(10,3),(11,3),(12,3)]:
        token[13]=0;struct.pack_into('<i',token,8,category);yes(g.i32(model.evaluate(token,residue),0)==30+delta,'empty history category '+str(category))
    token[12]=1;reject(lambda:model.evaluate(token,residue),'unsupported nonzero boundary publication input');token[12]=0
    token[13]=2;reject(lambda:model.evaluate(token,residue),'bad category suppression');token[13]=0
    rejected=fake_model(g.IMP,0);yes(rejected.evaluate(token,residue) is None and residue==bytes(range(40)),'rejected page no residue rewrite')
    ref=object.__new__(g.GlobalReference);ref.text={10:'x'};ref.words={7:0};ref.language=fake_model(5,0);ref.residue=residue;ref.heap=g.Heap56(2,bytes(1176));ref.language_calls=ref.language_accepted=ref.heap_rejected=0
    item=dict(o=0,k=0,cost=g.IMP-1,inc=2,tag=(1<<41)|(1<<32)|7,ctx=0,key=0,end=0)
    yes(ref.offer(item,10) and ref.heap.count==0 and ref.heap_rejected==1,'accepted LM suppresses fallback despite heap rejection')
    yes(ref.residue!=residue and g.i32(ref.residue,24)==-1,'accepted LM retains slot1 residue and aux24')
    ref.language=fake_model(g.IMP,0);yes(not ref.offer(item,10),'rejected LM permits direct fallback')
    for state in range(32):
        tag=state<<58;yes(g.terminal_category(tag,',')==g.terminal_category(tag,'x'),'comma no tag40 uses state')
        yes(g.terminal_category(tag|(1<<40),',')==7 and g.terminal_category(tag,'?')==8,'literal8/9 overrides')
    yes(g.script_id([[0,4,1],[5,9,5]],7)==5 and g.script_id([[0,4,1]],9)==0,'raw script5 and absent ranges')
    reject(lambda:g.raw_json(b'{"a":1,"a":2}'),'duplicate raw JSON keys')
    case,pub,ch,ph=fake_unicode();old_ch,old_ph=g.CASE_NUMERIC,g.PUBLICATION_NUMERIC
    #The fake-domain checksum substitution exists only in this test module's
    #isolated namespace, never in shipped oracle source or an actual admission.
    g.CASE_NUMERIC,g.PUBLICATION_NUMERIC=ch,ph
    try:
        cr=json.dumps(case).encode();pr=json.dumps(pub).encode();yes(g.authenticate_unicode(cr,pr)==pub['scriptRanges'],'complete synthetic raw Unicode checksum parser')
        for key,mutate in [('casecell',lambda c,p:c['lowerSingles'][0].__setitem__(1,1)),('pubcell',lambda c,p:p['scriptRanges'][0].__setitem__(2,2)),('literal',lambda c,p:p['literalRows'][0].__setitem__(1,1)),('bool',lambda c,p:c['sequenceScalars'][0].__setitem__(0,True))]:
            c2,p2=copy.deepcopy(case),copy.deepcopy(pub);mutate(c2,p2);genuine([case,pub],[c2,p2]);reject(lambda c=c2,p=p2:g.authenticate_unicode(json.dumps(c).encode(),json.dumps(p).encode()),key)
    finally:g.CASE_NUMERIC,g.PUBLICATION_NUMERIC=old_ch,old_ph
    old_ak,old_lk=g.AK_SHA,g.LK_SHA;ak,lk=b'fakeAK',b'fakeLK';g.AK_SHA,g.LK_SHA=g.sha(ak),g.sha(lk)
    c={'publicationOracleMode':g.MODE,'publicationOracleInputVersion':1,'factory':0x292,'globalPublicationAlternatives':2,'publicationResourceSHA256':[g.sha(v) for v in [ak,lk,b'C',b'P']],'publicationAllocatorInputs':allocation_inputs(),'graph':[[],[]],'inputNodes':[{},{}],'nodes':[{},{}]}
    try:
        yes(len(g.admit_capture(c,ak,lk,b'C',b'P'))==4,'typed explicit-mode allocator input admission')
        mutations=[lambda d:d.pop('publicationOracleMode'),lambda d:d.update(publicationOracleMode='old'),lambda d:d.update(publicationOracleInputVersion=True),lambda d:d.update(globalPublicationAlternatives=3),lambda d:d['publicationResourceSHA256'].__setitem__(2,'0'*64),lambda d:d['publicationAllocatorInputs'].pop(),lambda d:d['publicationAllocatorInputs'][0].update(Node=1),lambda d:d['publicationAllocatorInputs'][0].update(Identity=True),lambda d:d['publicationAllocatorInputs'][1].update(Identity=1),lambda d:d['publicationAllocatorInputs'][0].update(Kind='PublicationCandidateHeap'),lambda d:d['publicationAllocatorInputs'][0].update(Bytes='AA=='),lambda d:d['nodes'].pop(),lambda d:d['inputNodes'].pop()]
        for i,mutate in enumerate(mutations):
            bad=copy.deepcopy(c);mutate(bad);genuine(c,bad);reject(lambda b=bad:g.admit_capture(b,ak,lk,b'C',b'P'),'contract '+str(i))
    finally:g.AK_SHA,g.LK_SHA=old_ak,old_lk
    #Old and new Append functions are isolated from their resource/CLI drivers.
    common=HERE/'audit_output.py';tree=ast.parse(common.read_text(encoding='utf-8'));funcs=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name in ('u16','w32','wrap','write','append')]
    ns={'struct':struct,'storage':bytearray([167])*160,'count':0,'N':192,'special_e2':65535,'closed':65534,'knowledge':178,'startmap':[255]*256}
    exec(compile(ast.Module(body=funcs,type_ignores=[]),'append-only-no-resource','exec'),ns)
    row=bytearray(88);row[26]=63
    ns['append']([(10,0,0),(11,0,0)],row,0,77,-1,20,10,65535,False,255,False,17)
    yes(g.i32(ns['storage'],36)==-1 and g.i32(ns['storage'],76)==17,'auxiliary only final compact lane')
    #Meaningful coherent wrong LM-wordID-for-aux corruption changes terminal lane.
    actual=bytes(ns['storage']);bad=bytearray(actual);struct.pack_into('<i',bad,76,0);yes(bytes(bad)!=actual,'coherent aux mutation genuinely changes bytes');reject(lambda:assert_equal(bytes(bad),actual),'wordID substituted for LM auxiliary')
    report={'Status':'PASS_SYNTHETIC_GLOBAL_RAW_REFERENCE','PositiveChecks':passed,'NegativeChecks':negative,'ActualResourcesRead':False,'RealModelConstructorsCalled':False,'RecognitionOrNativeOrCompilerExecuted':False}
    out.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
def assert_equal(a,b):assert a==b
if __name__=='__main__':main()
