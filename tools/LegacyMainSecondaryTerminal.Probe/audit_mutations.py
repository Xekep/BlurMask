"""ROOT-run capture-only mutations. No models/source-owner/native/C# execution.
Author delivered SOURCE only; did not execute auditor or these controls.
"""
import argparse,ast,base64,contextlib,copy,hashlib,io,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--capture',type=Path,required=True);p.add_argument('--auditor',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
raw=a.auditor.read_bytes();text=raw.decode('utf-8-sig').replace('\r\n','\n');expected_auditor_sha='a4c495d8b17c8b9e299d4d5ffc483ad7ffcaf72391195d4130c50f73678d2f4a';assert hashlib.sha256(text.encode()).hexdigest()==expected_auditor_sha,'Unreviewed auditor source'
source=ast.parse(text);reads=[n for n in source.body if isinstance(n,ast.Assign)and len(n.targets)==1 and isinstance(n.targets[0],ast.Name)and n.targets[0].id=='d'];assert len(reads)==1 and isinstance(reads[0].value,ast.Call)
# Only replace the file-I/O assignment by this immutable DATA test input; the full scan/checks stay exact.
reads[0].value=ast.Name(id='CAPTURE',ctx=ast.Load());ast.fix_missing_locations(source);code=compile(source,str(a.auditor),'exec')
with a.capture.open('rb')as f:capture_bytes=f.read(16*1024*1024+1)
assert len(capture_bytes)<=16*1024*1024;capture=json.loads(capture_bytes.decode('utf-8-sig'));results=[]
def replay(data):
 g={'CAPTURE':data,'__name__':'root_secondary_terminal_DATA_mutation','__file__':str(a.auditor)}
 with contextlib.redirect_stdout(io.StringIO())as out:exec(code,g,g)
 return out.getvalue()
base_output=replay(copy.deepcopy(capture));assert len(capture['choices'])>=2

def test(name,change):
 data=copy.deepcopy(capture);before=json.dumps(data,sort_keys=True,separators=(',',':'));change(data);assert json.dumps(data,sort_keys=True,separators=(',',':'))!=before,'Mutation did not change '+name
 try:replay(data)
 except AssertionError:results.append(dict(name=name,rejected=True));return
 raise AssertionError('Mutation accepted: '+name)
def byte_change(text,offset):
 b=bytearray(base64.b64decode(text,validate=True));assert 0<=offset<len(b);b[offset]^=1;return base64.b64encode(b).decode()
def matrix_offset(data,choice):return(choice['ordinal']*2+choice['secondaryOrdinal'])*40

test('missing-choice',lambda d:d['choices'].pop())
test('duplicate-choice',lambda d:d['choices'].append(copy.deepcopy(d['choices'][0])))
test('bad-choice-ordinal',lambda d:d['choices'][0].__setitem__('ordinal',d['sourceEnd']))
def coherent_exclusion(d):
 c=d['choices'][1];prior=d['choices'][0]['identity'];assert c['identity']!=prior
 b=bytearray(base64.b64decode(d['secondary'],validate=True));offset=matrix_offset(d,c);struct.pack_into('<Q',b,offset+8,prior);d['secondary']=base64.b64encode(b).decode()
 c['identity']=prior;c['alternative40']=base64.b64encode(b[offset:offset+40]).decode()
test('coherent-duplicate-exclusion-identity',coherent_exclusion)
test('bad-relative-cost',lambda d:d['choices'][0].__setitem__('relative',d['choices'][0]['relative']^1))
test('bad-total-cost',lambda d:d['choices'][0].__setitem__('total',d['choices'][0]['total']^1))
test('selected-full-row88-byte',lambda d:d['choices'][0].__setitem__('row88',byte_change(d['choices'][0]['row88'],87)))
test('selected-full-alternative40-byte',lambda d:d['choices'][0].__setitem__('alternative40',byte_change(d['choices'][0]['alternative40'],39)))
test('record-source-full-row-byte',lambda d:d.__setitem__('records',byte_change(d['records'],d['choices'][0]['ordinal']*88+87)))
test('matrix-source-cost-byte',lambda d:d.__setitem__('secondary',byte_change(d['secondary'],matrix_offset(d,d['choices'][0]))))
test('configured-width',lambda d:d.__setitem__('configured',3))
test('prepared-control-count',lambda d:d.__setitem__('controls',511))
test('guard-count',lambda d:d.__setitem__('guards',5))
test('cumulative-cost-input',lambda d:d.__setitem__('cumulative',d['cumulative']^1))
report=dict(scope='ROOT DATA-only terminal-audit controls; no upstreamNN/heap/native proof',captureSha256=hashlib.sha256(capture_bytes).hexdigest(),auditorLF_SHA256=expected_auditor_sha,baselineOutput=base_output.strip(),derivedActualChoices=len(capture['choices']),mutations=results,modelsRead=False,expectedTextRead=False,owningServiceExecuted=False,upstreamPopulationCausalityClaim=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(json.dumps(dict(choices=len(capture['choices']),mutationsRejected=len(results),scope='DATA-only')))
