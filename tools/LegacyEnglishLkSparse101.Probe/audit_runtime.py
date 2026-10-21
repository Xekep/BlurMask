"""Independent all-raw English sparse-data/numeric runtime oracle.
The raw reference receives no runtime output or expected handwritten labels.
"""
from pathlib import Path
import argparse,hashlib,json,subprocess,sys
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('capture',type=Path);p.add_argument('output',type=Path);p.add_argument('--native',type=Path);a=p.parse_args()
reference=a.output.with_name(a.output.name+'.raw-reference.json');subprocess.run([sys.executable,'-B',str(Path(__file__).with_name('reference_actual.py')),str(a.models),str(reference)],check=True)
x=json.loads(reference.read_bytes());c=json.loads(a.capture.read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest()
assert c['RetainedLanguageWireBytes']==x['end']-x['base']==1580304
assert c['LogicalCopyBytes']<=32*1024*1024 and c['LogicalCopyBytes']>=2*3300812+2*1580304
assert c['ReservedWordIndex']==260074 and c['StoredDefault268C']==-0x4000000
assert c['Fields']==[dict(Name=f['destination'],Offset=f['offset'],End=f['end'],Sha256=f['sha256'])for f in x['fields']]
keys={'WordClasses':'88','Order2Labels':'B0.1','Order3Labels':'B0.2','Order1Offsets':'B8.0','Order2Offsets':'B8.1'}
assert len(c['Packed'])==5
for z in c['Packed']:
 v=x['values'][keys[z['Field']]];assert(z['Count'],z['Width'],z['ValuesSha256'])==(v['count'],v['width'],v['valuesSha256'])
b=c['Compiled'];e=x['compiled']
for left,right in [('SourceKind','sourceKind'),('Profile','profile'),('SourceScale','sourceScale'),('LiteralPacketByte0','packetByte0'),('Context00','context00'),('Boundary94C','boundary94C'),('FinalUnigram94D','finalUnigram94D'),('WordCostsSha256','wordCodesSha256'),('ClassCostsSha256','classCodesSha256'),('BackoffCostsSha256','backoffCodesSha256'),('CaseRows','caseRows'),('CaseTriples','caseTriples')]:assert b[left]==e[right],left
#Utf8JsonWriter emits shortest binary32 decimals; compare their binary32 values.
import struct
for key,ref in [('CaseBias','caseBias'),('CaseMultiplier','caseMultiplier')]:assert struct.pack('<f',b[key])==struct.pack('<f',e[ref])
assert len(b['CaseRows'])==144 and len(b['CaseTriples'])==1728
assert c['Guards']==143 and c['PreparedControls']==21 and len(c['CanceledLoads'])==18 and all(not f['Published']for f in c['CanceledLoads'])
assert len(c['TamperedByteOffsets'])==14
if a.native:assert a.capture.read_bytes()==a.native.read_bytes()
r=dict(status='PASS independent actual English sparse fields+fresh numeric binding',fields=len(c['Fields']),packedValues=sum(z['Count']for z in c['Packed']),costEntries=1536,caseRows=144,caseTriples=1728,guards=c['Guards'],canceledLoads=18,preparedControls=c['PreparedControls'],captureSha256=sha(a.capture.read_bytes()),nativeExact=bool(a.native),nativeVendorExecuted=False,scope='No query/history/configured policy/masks/main inference or quality claim')
a.output.write_text(json.dumps(r,indent=2)+'\n');print('PASS independent all-raw sparse/numeric runtime oracle')
