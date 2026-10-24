"""Pure DATA/source-guard replay of ONE already captured owning CHAR; no NN."""
import argparse,ast,base64,copy,hashlib,json,struct,types
from pathlib import Path
HERE=Path(__file__).resolve().parent
PRIOR='2b18399df0bd06513a0652c5c3fe506eeef51536257cce0719fbaf6ca6764f3b'
GATE='c5051194569749bdd7ae2ca23be94771a554f5ec491e2e7b4cb231c591f89f39'
SCOPE='ACTUAL_OWNING_ENGLISH_CHAR_SOURCE_PROCESSING_REUSE_DATA_NO_TEXT_ADMISSION'
def sha(b):return hashlib.sha256(b).hexdigest()
def loadmodule(path,name):
 m=types.ModuleType(name);m.__file__=str(path);exec(compile(path.read_bytes().decode('utf-8-sig'),str(path),'exec'),m.__dict__);return m

def functions(path,scope):
 t=ast.parse(path.read_bytes().decode('utf-8-sig'));exec(compile(ast.Module(body=[n for n in t.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),str(path),'exec'),scope)

def setup(a):
 priorraw=a.prior_geometry.read_bytes();assert sha(priorraw)==PRIOR;prior=json.loads(priorraw)
 geom=loadmodule(HERE/'prior/LegacyEnglishGlyphUtf8.ActualEntry/audit_geometry.py','pinned_geometry')
 b,e,w,g,p,r=geom.load(a.prior_utf8,a.top,a.source,a.compact,a.raw,a.models)
 wantgeom=geom.expected_geometry(b,e,w,g,p,r);geom.audit(prior,b,wantgeom,g)
 gate=a.source_gate.read_bytes();assert sha(gate)==GATE and b'I(descriptor,16)!=0'in gate
 rangescope={'MASK':2**64-1};functions(HERE/'reference_range_operations.py',rangescope)
 assert sha((HERE/'reference_range_operations.py').read_bytes())=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
 charscope={'g':rangescope,'copy':copy};functions(HERE/'audit_prepared.py',charscope)
 glyphs=wantgeom['Glyphs'];assert len(glyphs)==1 and len(w['GlyphChoices'])==1 # exact actual scope; no multi-ref claim
 pair=lambda flat:[flat[i:i+2]for i in range(0,len(flat),2)]
 t={'Id':'actual','Counts':wantgeom['OriginalStrokePointCounts'],'Original':pair(wantgeom['OriginalRoot']),
    'Ranges':[pair(v['WordClippedPairs'])for v in glyphs],'Positive':[True for _ in glyphs],'Joiner':[v['OriginalJoiner']for v in glyphs]}
 steps=charscope['process'](t);prefix=[];final=[];commands=[];nextid=len(r['Raw'])
 for i,(v,c,u,step)in enumerate(zip(glyphs,w['GlyphChoices'],w['GlyphUtf8'],steps['Steps'],strict=True)):
  assert step['Inserted']and step['Ordinary'] # actual one fresh ordinary source; no actual21760 reuse
  encoded=base64.b64decode(c['Source26'],validate=True);assert len(encoded)>0 and len(encoded)%26==0;n=len(encoded)//26;visits=0
  for q in range(n):
   group,first,_,lastgroup,last,_=struct.unpack_from('<6i',encoded,q*24)
   assert group==lastgroup==0 and 0<=first<=last<len(r['Raw']);visits+=last-first+1
  assert visits>0;priorvalue=0 # exact fresh raw document has no KnownText input
  records=[];masks=[];texts=[];aggregate=0
  owner=base64.b64decode(c['Owner40'],validate=True);selection=base64.b64decode(c['Selected4'],validate=True)
  assert len(selection)%4==0;selected=list(struct.unpack('<'+'i'*(len(selection)//4),selection))
  for chosen in selected:
   row=owner[chosen*40:(chosen+1)*40];assert len(row)==40 and struct.unpack_from('<H',row,4)[0]&7==0
   symbol=struct.unpack_from('<H',row,8)[0];text=e['s']['al']['mapping'][symbol]
   assert len(text)==1 and('a'<=text<='z'or'A'<=text<='Z')
   cp=ord(text);script=next((v for a,z,v in e['publication']['scriptRanges']if a<=cp<=z),0)
   ccc=next((v for a,z,v in e['normalization']['combiningRanges']if a<=cp<=z),-1);assert script==2 and ccc==0
   utf8=text.encode();flags=struct.unpack_from('<H',row,6)[0]&0x3ff
   literal={bytes.fromhex(x[0]):x[1]for x in e['publication']['literalRows']}
   if literal.get(utf8,-1)>=0:flags|=0x10000
   record=bytearray(40);struct.pack_into('<I',record,8,flags);struct.pack_into('<I',record,32,len(utf8))
   mask=bytearray(40);mask[8:12]=b'\xff'*4;mask[32:36]=b'\xff'*4
   records.append(base64.b64encode(record).decode());masks.append(base64.b64encode(mask).decode());texts.append(base64.b64encode(utf8).decode());aggregate|=flags
  assert texts==u['Utf8']and len(texts)==len(u['Flags'])
  key=step['Key'];flags=charscope['flags']({'Ordinary':True,'Prior':priorvalue,'Aggregate':aggregate,'Key':key})
  flat=[x for k in key for x in k]
  common={'SourceNode':step['Node'],'SourcePairs':flat,'SelectionId':nextid,'LineId':0,'Flags':flags,'Prior':priorvalue,'PriorVisits':visits}
  # Line0 is not a guessed category/default: finalized SAME owner rejects descriptor16!=0.
  prefix.append({'Reference':v['Reference'],**{'SourceNode':step['Node'],'Inserted':True,'Ordinary':True},**{k:x for k,x in common.items()if k!='SourceNode'},'Marker':u['Marker'],'Records40':records,'RecordKnown40':masks,'Utf8':texts})
  final.append({**common,'AssignedDefaults':{'Field08':-1,'Field40':-1,'Field00':0,'Field1C':0,'Field64':0,'Field48':-1},'Written':127,'Records40':records,'RecordKnown40':masks,'Utf8':texts})
  commands.append({'SelectionId':nextid,'Name':'CHAR','CommandIdentity':len(commands)+1,'PreviousCommandIdentity':len(commands),'SourcePairs':flat});nextid+=1
 chars={'PerReferencePrefixes':prefix,'FinalNodes':final,'CharacterTreeRoot':steps['Root'],'CharacterTree':steps['Tree'],'PendingCommands':commands}
 want=copy.deepcopy(prior);want.update(Scope=SCOPE,CHARCreated=True,TEXTCreated=False,Characters=chars,PriorGeometrySha=PRIOR,PriorCanonicalFieldsMatched=True,ParentPreparationMayRunLineModels=True)
 return want,g.strict

def audit(x,want,strict):strict(x,want)
def tamper(x,want,strict):
 changes=[('scope',lambda v:v.update(Scope='TEXT')),('meta bool',lambda v:v.update(OriginalIndex=True)),('meta float',lambda v:v.update(OriginalIndex=4.0)),('metrics',lambda v:v.update(Metrics={})),('vector',lambda v:v.update(Vector10=[])),('geometry',lambda v:v['Geometry'].update(WordClip=[])),('TEXT marker',lambda v:v.update(TEXTCreated=True)),('CHAR marker',lambda v:v.update(CHARCreated=1)),('prior marker',lambda v:v.update(PriorCanonicalFieldsMatched=False))]
 for name,field,value in [('key','SourcePairs',[]),('node','SourceNode',True),('insert','Inserted',False),('ordinary','Ordinary',False),('selection','SelectionId',0),('line','LineId',1),('flags','Flags',1),('prior','Prior',1),('visits','PriorVisits',0),('marker','Marker',1),('rows','Records40',[]),('known','RecordKnown40',[]),('utf8','Utf8',[])]:
  changes.append((name,lambda v,f=field,x=value:v['Characters']['PerReferencePrefixes'][0].update({f:x})))
 changes.extend([('default',lambda v:v['Characters']['FinalNodes'][0]['AssignedDefaults'].update(Field40=0)),('default type',lambda v:v['Characters']['FinalNodes'][0]['AssignedDefaults'].update(Field00=False)),('write bits',lambda v:v['Characters']['FinalNodes'][0].update(Written=119)),('final prefix',lambda v:v['Characters']['FinalNodes'][0].update(Records40=[])),('final key',lambda v:v['Characters']['FinalNodes'][0].update(SourcePairs=[])),('root',lambda v:v['Characters'].update(CharacterTreeRoot=0)),('tree cycle',lambda v:v['Characters']['CharacterTree'][0].update(Left=1)),('command type',lambda v:v['Characters']['PendingCommands'][0].update(Name='WORD')),('command link',lambda v:v['Characters']['PendingCommands'][0].update(PreviousCommandIdentity=1)),('command id',lambda v:v['Characters']['PendingCommands'][0].update(SelectionId=0)),('command key',lambda v:v['Characters']['PendingCommands'][0].update(SourcePairs=[])),('extra command',lambda v:v['Characters']['PendingCommands'].append(copy.deepcopy(v['Characters']['PendingCommands'][0]))),('omitted command',lambda v:v['Characters'].update(PendingCommands=[]))])
 def byte(v,channel,field,offset):
  target=v['Characters'][channel][0];b=bytearray(base64.b64decode(target[field][0],validate=True));b[offset]^=255;target[field][0]=base64.b64encode(b).decode()
 changes.extend([('unknown residue',lambda v:byte(v,'PerReferencePrefixes','Records40',0)),('bytecount',lambda v:byte(v,'PerReferencePrefixes','Records40',32)),('known pointer promotion',lambda v:byte(v,'PerReferencePrefixes','RecordKnown40',16)),('known flag omitted',lambda v:byte(v,'FinalNodes','RecordKnown40',8))])
 names=[]
 for name,mutate in changes:
  bad=copy.deepcopy(x);mutate(bad)
  try:audit(bad,want,strict)
  except(AssertionError,TypeError,KeyError,ValueError):names.append(name)
  else:raise AssertionError('Mutation accepted: '+name)
 return names

def main():
 p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('prior_geometry',type=Path);p.add_argument('prior_utf8',type=Path);p.add_argument('top',type=Path);p.add_argument('source',type=Path);p.add_argument('compact',type=Path);p.add_argument('raw',type=Path);p.add_argument('models',type=Path);p.add_argument('source_gate',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
 assert not a.report.exists()and a.capture.stat().st_size<=32*1024*1024
 x=json.loads(a.capture.read_bytes());want,strict=setup(a);audit(x,want,strict)
 r={'Status':'PASS_FIXED_OWNING_CHAR_DATA_SOURCE_GUARD_JOIN','ActualSha256':sha(a.capture.read_bytes()),'WholePriorCanonicalMatched':True,'References':1,'FreshNodes':1,'ActualReusedReferences':0,'PerReferenceRecords40Bytes':80,'FinalRecords40Bytes':80,'KnownMaskBytes':160,'SourcePairs':1,'PendingCommands':1,'UTF8BytesPerSnapshot':2,'LineIdProof':'Authenticated finalized-single-source descriptor16==0 guard; descriptor bytes are not separately exported here','NN':False,'TEXTOrAdmission':False,'NotIndependentlyVerified':['live owner identity/epoch/cancellation/budget guards and exact source count/fill call chronology: SOURCE review, not detached DATA','actual21760 reuse/cache hit/multi-reference behavior: this capture has one fresh key, prepared evidence remains separate','native pointer contents/allocator residue: only flags8..11/bytecount32..35 known; other library-template zero bytes are NOT promoted','pending command OperationCode/Enabled/Category not exported; no external document commit claimed','general Unicode/cached/external/page/prior/multiWORD/public TEXT/app/quality/admission; actual oracle scopes single ASCII Latin glyph alternatives','CLR/RSS/CPU and exact remaining cumulative-ledger counters not exported']}
 if a.tamper:r['TamperNames']=tamper(x,want,strict);r['TamperRefusals']=len(r['TamperNames'])
 a.report.write_bytes((json.dumps(r,indent=2)+'\n').encode());print(json.dumps(r))
if __name__=='__main__':main()
