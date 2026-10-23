"""STATIC native/raw proof for a proposed initial number request owner. No DLL execution."""
import argparse,ast,hashlib,json,runpy,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
repo=Path(__file__).resolve().parents[2];sha=lambda b:hashlib.sha256(b).hexdigest()
pins=json.loads(Path(__file__).with_name('linguistic-fst28c-number-live-query-pins.json').read_bytes())
def pe_reader(b):
 pe=struct.unpack_from('<I',b,60)[0];o=pe+24;sz=struct.unpack_from('<H',b,pe+20)[0];sections=[struct.unpack_from('<IIII',b,o+sz+i*40+8)for i in range(struct.unpack_from('<H',b,pe+6)[0])]
 def read(va,n):
  r=va-0x180000000;s,v,l,pos=next(x for x in sections if x[1]<=r<x[1]+x[2]);assert r+n<=v+l;return b[pos+r-v:pos+r-v+n]
 return read
native=(a.native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes();assert sha(native)==pins['nativeSha256'];read=pe_reader(native)
for row in pins['regions']:assert sha(read(int(row['start'],16),row['bytes']))==row['sha256']
for row in pins['spans']:assert read(int(row['start'],16),row['bytes']).hex()==row['hex']and sha(bytes.fromhex(row['hex']))==row['sha256']
e=pins['engineTypeAPI'];engine=(a.native_repo/'research/myscript/sdk/build/x64'/e['binary']).read_bytes();assert sha(engine)==e['sha256'];er=pe_reader(engine)
for row in e['tables']:assert er(int(row['at'],16),len(bytes.fromhex(row['bytes']))).hex()==row['bytes']
for row in pins['runtimeReviewedLF']+pins['independentDefinitionsLF']:assert sha((repo/row['path']).read_bytes().replace(b'\r\n',b'\n'))==row['sha256LF']
# Reuse ONLY independent definitions, never script capture processing or supplied values.
def defs(path,names):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));ns={'struct':struct};nodes=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names];assert len(nodes)==len(names);exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
alphabet=defs(repo/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py',{'alphabet'})['alphabet']
parse=defs(repo/'tools/LegacyFst13AOutgoing.Probe/audit_output.py',{'parse'})['parse']
avl=defs(repo/'tools/LegacySymbolContext.Probe/reference_controls.py',{'Node','height','update','left','right','add','case'})
lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IIIHH',lk,299000)==(0x101fe90,2572,0x13a,0,0x202)
assert struct.unpack_from('<I',lk,299096)[0]==257 and struct.unpack_from('<I',ak,5233)[0]==0x135 and ak[5237]==2
sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={v:k for k,v in acoustic.items()};mapped=[65535 if reverse[i]==65535 else 65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(sn)]
assert mapped==[65533,0,1,2,3,4,5,6,7,8,9,65531,141,139,138,185]
source_keys=[k for k in mapped if k not in(65535,65533)];assert len(source_keys)==len(set(source_keys))==15
outer=[65531,65529,65531,65532,65530];sequence=source_keys+outer;tree=avl['case'](sequence);before=avl['case'](source_keys)
assert len(tree['nodes'])==18 and len(before['nodes'])==15
# Symbolic node allocation identities follow first-insertion order; rotations do not reallocate.
allocation=list(dict.fromkeys(sequence));ids={k:i+1 for i,k in enumerate(allocation)};slots=[{'slot':i,'key':k,'nodeId':ids[k],'history':None,'secondary':None}for i,k in enumerate(source_keys)]
assert all(ids[x['key']]==x['nodeId']==x['slot']+1 for x in slots)
assert ids[outer[0]]==ids[outer[2]]==ids[mapped[11]]
assert set(allocation)-set(source_keys)=={65529,65530,65532}
# Literal reset writes, not fabricated values for untouched fields/buffers.
reset=[(0xc8,4,-1)]+[(off+i*0x50,n,-1)for i in range(7)for off,n in[(0x100,8),(0x110,8),(0x118,4)]]+[(0x330,8,-1),(0x340,8,-1),(0x348,4,0)]
assert len(reset)==25
for off,n,value in reset:
 needle=b'\xc7\x82'+struct.pack('<I',off)+struct.pack('<i',value)
 if n==8:needle=b'\x48'+needle
 assert needle in read(0x18023ded0,267),(off,n)
assert read(0x18010ba90,20).hex()=='488b8280000000418bc8488b40180fb644c807c3'
# R8 target ordinal is really supplied to V70 (Ghidra prototype omits it).
assert read(0x180240149,20).hex()=='448bc1894d97488b4d67488b024c8b487041ffd1'
states,targets,descriptors=parse(lk[299576:301580],299576)
costs=runpy.run_path(str(repo/'tools/handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']()
assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
def rows(s):
 return [[t,descriptors[t][1],costs[descriptors[t][2]+128],descriptors[t][2],descriptors[t][3]]for t in targets[descriptors[s][0]:descriptors[s+1][0]]]
root=rows(0);assert root==[[97,0,3454,51,0]];after_epsilon=rows(root[0][0]);assert [r[0:2]for r in after_epsilon]==[[65,0],[69,14],[70,15]];digits=rows(65);assert len(digits)==10 and sorted(r[1]for r in digits)==list(range(1,11))
request={'tag':8<<40,'headerNonNull':True,'activeTreeNonNull':True,'parent':None,'source':0,'start':0,'span':0,'flags':0,'skip':0,'take':128,'cachedOwner':None}
assert read(0x1801fd336,8).hex()=='41c6864d16000008'
# Bounded proposed protocol domains. These are not CLR heap/RSS or CPU instructions.
logical=4080+256+18*40+16*8+16*4+15*8+5*(2+8)+72+80+8*(80+12*(4+2+4+2))+2*16*2+256
work=20*(18*16+64)+8*80+80+3*16*2+15*8+16*15+18*(56+16)+5*10+512+64
negative={
 'omit_outer_controls':len(before['nodes'])!=18,
 'independent_space_node':len(allocation)+1!=18,
 'lose_slot_identity':ids[source_keys[0]]!=ids[source_keys[1]],
 'nul_is_not_missing_header':mapped[0]==65533 and request['headerNonNull'],
 'source_aux_not_empty':len(slots)>0 and all(x['history']is None and x['secondary']is None for x in slots),
 'target_not_cost_tag':root[0][0]!=root[0][3],
 'root_internal_not_digit':mapped[root[0][1]]==65533 and mapped[after_epsilon[0][1]]==65533 and all(0<=mapped[r[1]]<192 for r in digits),
 'query_tag_not_zero':request['tag']!=0,
 'reset_not_constructor_state0':reset[0][2]==-1,
 'untouched_counts_not_zeroed':all(off!=0xcc for off,n,v in reset),
 'flags_do_not_follow_v280':read(0x1800ca630,5).hex()=='488b4270c3',
 'space_shared_alias':ids[outer[0]]==ids[outer[2]]==ids[mapped[11]],
}
assert all(negative.values())
report={'scope':'STATIC only; proposed active context/first request binding, no query execution','regions':len(pins['regions']),'functions':len(set(r['function'].lower().removeprefix('0x')for r in pins['regions'])),'boundedSpans':len(pins['spans']),'runtimeLFBindings':len(pins['runtimeReviewedLF']),'preservedPrimary':mapped,'sourceSlots':slots,'outerKeys':outer,'activeAvl':tree,'activeAllocationKeys':allocation,'sourceOnlyAvl':before,'resetWrites':reset,'rootInternalRows':root,'afterEpsilonSourceRows':after_epsilon,'state65DigitSourceRows':digits,'initialRequest':request,'logicalByteEnvelope':logical,'workEnvelope':work,'negativeControls':negative,'queryImplemented':False,'newHeaderIdentityPermitted':False}
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC',len(pins['regions']),'regions',len(pins['spans']),'spans; source15/live18/25resetwrites/root97,3454,51; proposed B/W',logical,work)
