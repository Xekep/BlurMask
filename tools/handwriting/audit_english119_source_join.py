"""Static EN119 owned rich result -> source720 proposal audit. Never loads vendor code."""
import argparse,ast,base64,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args();root=a.native_repo.resolve()
spec=json.loads(Path(__file__).with_name('english119-source-join-pins.json').read_bytes())
bodyRows=Path(__file__).with_name('english119-source-join-bodies.tsv').read_text().splitlines()[1:]
assert [[f,int(s,16),int(e,16)+1,int(n)]for f,s,e,n in (r.split('\t')for r in bodyRows)]==[[r['function'],r['start'],r['end'],r['end']-r['start']]for r in spec['regions']]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary(spec['dll']);assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256']=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);instructions={}
for r in spec['regions']:
 raw=b.data[b.off(r['start']):b.off(r['end'])];assert hashlib.sha256(raw).hexdigest()==r['sha256']
 for x in md.disasm(raw,r['start']):instructions[x.address]=(x.mnemonic,x.op_str)
for at,mn,op in spec['pins']:assert instructions[at]==(mn,op),(hex(at),mn,op)
assert len([r for r in spec['regions']if r['function']=='180157c60'])==2
for table,slot,target in spec['bindings']:assert b.qwords(table+slot,1)[0]==target
at,literal=spec['literal'];assert b.data[b.off(at):b.off(at)+1].hex()==literal=='5f'
data=(root/spec['resource']).read_bytes();assert hashlib.sha256(data).hexdigest()==spec['resourceSha256']
manifest=json.loads((root/'models/manifest.json').read_bytes());assert any(x['path']=='en-US/en_US-ak-cur.lite.res' and x['sha256'].lower()==spec['resourceSha256'] for x in manifest['resources'])
parser=root/'tools/LegacySource247Wire.Probe/audit_output.py';tree=ast.parse(parser.read_text(encoding='utf-8-sig'));ns={'base64':base64,'struct':struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(parser),'exec'),ns)
parent,pv=ns['source'](data);assert pv['Version']==0x110 and pv['SourceKind']==0x135 and pv['CoordinateMode']==2
def child(reader,destination,expected):
 row=next(c for c in reader.children if c['destination']==destination);assert [row[k] for k in ('Offset','End','Kind','Version')]==expected;return row
child(parent,'parametersC0.58[0]',spec['selected249'])
off,end,kind,version=spec['source'];magic,length,k,flags,v=struct.unpack_from('<IIIHH',data,off);assert (magic,off+8+length,k,flags,v)==(0x101fe90,end,kind,0,version)
first,fv=ns['source'](data[off:end]);assert fv['alphabetCount']==217 and fv['Version']==0x110 and fv['SourceKind']==0x135
for destination,key in [('247.130','sampler'),('247.138','graph')]:
 expected=spec[key];child(first,destination,[expected[0]-off,expected[1]-off,*expected[2:]])
field=lambda reader,owner,dest:base64.b64decode(next(x['raw']for x in reader.fields if x['owner']==owner and x['destination']==dest))
assert all(field(first,'parametersC0',x)==b'\0'for x in ('20','21','legacyThirdPresence','22'))
assert field(first,'247','148.present')==b'\0'
ordered=[x['Kind']for x in first.children if x['destination'].startswith('parametersC0.58[')];assert ordered==[0x24a,0x24c,0x265]
sm=dict(zip(('Offset','End','Kind','Version'),spec['sampler']));sampler,error=ns['sampler'](data,sm,217);assert error is None
assert spec['exponent']==[end,0.75] and struct.unpack_from('<f',data,end)[0]==0.75
# Runtime provenance checks are narrow owner contracts, not proof of native ABI or full orchestration.
contracts={
 'LegacyOwnedEnglish119PlainText.cs':['internal LegacyOwnedEnglish119PlainResultFactory Factory','internal LegacyFirstPlainTextStorage Storage','internal IReadOnlyList<LegacyFirstPublishedTextWord> NativeWords','ReferenceEquals(word.Published.Word.Pool,Storage.GlyphPool)'],
 'LegacyOwnedEnglish119PlainResultFactory.cs':['Graph=>Policy.SourceRanges.Graph','Categories=>Policy.Categories'],
 'LegacyOwnedEnglish119FirstPolicy.cs':['candidates.Sampler.ReadCategories(token)'],
 'LegacyOwnedEnglish119Candidates.cs':['LegacyOrdinaryGraphSourceRanges','PreparedChronology'],
 'LegacyOwnedEnglishLiteFullPreprocessing.cs':['CopyOriginalStrokes','original.Strokes'],
 'LegacyOrdinarySourceInk.cs':['rawTotal>4096-raw.Count','ResolveEndingMetadata'],
 'LegacyFirstAcoustic119WireModel.cs':['SourceScoreExponent','SelectedProcessor'],
}
for name,needles in contracts.items():
 text=(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig')
 for needle in needles:assert needle in text,(name,needle)
result=dict(status='PASS',scope='static proposal only; no EN source720 runtime or full native parent orchestration',fullGhidraFunctions=len(set(r['function']for r in spec['regions'])),fullGhidraRegions=len(spec['regions']),instructionPins=len(spec['pins']),callbackBindings=len(spec['bindings']),selected249Exponent=.75,sourceVersion='110',samplerVersion='108',flatFallbackLiteral='_',contracts=sum(map(len,contracts.values())))
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(json.dumps(result))

