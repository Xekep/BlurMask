"""Actual FULL selected249/source binding metadata. No SDK/NN execution."""
from pathlib import Path
import argparse,ast,hashlib,struct,json
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def defs(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));g={'__file__':str(path)};exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(path),'exec'),g);return g
wire=defs(a.repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py');rows=[]
for name,expected,end in [('en_US-ak-cur.res','355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927',373961),('en_US-ak-cur.lite.res','ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657',182191)]:
 d=(a.repo/'models/en-US'/name).read_bytes();assert hashlib.sha256(d).hexdigest()==expected
 selected=next(c for c in wire['source'](d,0)[3]if c[3]==0x249);assert selected[1]==8827 and selected[2]==end and selected[4]==0x100
 stop,kind,count,_=wire['source'](d,selected[1]+16);assert kind==0x135 and count==217 and stop==end-4 and struct.unpack_from('<f',d,stop)[0]==.75
 rows.append(dict(path=name,resourceSha256=expected,selectedOffset=8827,selectedEnd=end,selectedKind=0x249,selectedVersion=0x100,actualOriginalSource=[8843,stop],actualSourceKind=kind,actualSourceFingerprint=hashlib.sha256(d[8843:stop]).hexdigest(),alphabet=count,loaded249Exponent=.75))
assert rows[0]['actualSourceFingerprint']!=rows[1]['actualSourceFingerprint']
report=dict(status='PASS',full=rows[0],liteDistinctControl=rows[1],nativeConditionalBinding=dict(successfulSelectedInstantiation='155EF0 creates actual247 and writes1E0; returns success only if nonnull',getter='registered158DE0 returns1E0',selectedCaller='150D00 checks selected249 getter nonnull, clones selected processor, passes original247 at pair+8',pairWriter='registered158DF0 writes16bytes to1D0..1DF, so1D8 gets actual original247',container='156630:14=(1D8?1:20),16=1,18=1,11=0',consumer='1EF070 stores14/16/18 toA0/A2/A4 and11 to88'),selectedPolicy=dict(A0=1,A2=1,A4=1,aliasFold88=0),unboundPreparedControl=dict(A0=20,A2=1,A4=1,aliasFold88=0),vendorExecuted=False,targetsRead=False,scope='Raw FULL metadata plus separately native-pinned conditional successful initializer; not actual native processor/ABI execution')
a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
