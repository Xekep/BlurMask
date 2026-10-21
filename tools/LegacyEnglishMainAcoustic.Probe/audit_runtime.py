"""Portable raw-only N runtime proof wrapper. Does not load/execute vendor code."""
from pathlib import Path
import argparse,hashlib,json,subprocess,sys,tempfile
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();root=Path(__file__).resolve().parents[2]
a.output.parent.mkdir(parents=True,exist_ok=True)
with tempfile.TemporaryDirectory(prefix='blurm-main119-oracle-',dir=a.output.parent)as temporary:
 raw=Path(temporary)/'raw-main.json'
 subprocess.run([sys.executable,'-I','-B',str(root/'tools/LegacyEnglishMainMasks.Probe/reference_actual.py'),str(a.models),str(raw)],check=True)
 subprocess.run([sys.executable,'-I','-B',str(Path(__file__).with_name('audit_output.py')),str(a.models/'en-US/en_US-ak-cur.lite.res'),str(raw),str(a.capture)],check=True)
capture_bytes=a.capture.read_bytes();c=json.loads(capture_bytes);assert c['TargetsRead']is False and c['Guards']==39 and c['CalibrationGuards']==6 and len(c['PreparedCalibrationControls'])==4 and c['Cancellations']==12 and len(c['PreparedAssemblyControls'])==16
assert(len(c['Rows']),len(c['MultilineRefused']),c['GroupCount'],c['ArcCount'])in[(134,7,4066,4083),(100,0,11330,11472)]
r=dict(status='PASS',scope='Actual MAIN119 features/four107/ordinary acoustic costs only; no worddecoder, expected labels or vendor execution',inputSha256=c['InputSha256'],captureSha256=hashlib.sha256(capture_bytes).hexdigest(),owners=len(c['Rows']),multilineRefused=len(c['MultilineRefused']),groups=c['GroupCount'],arcs=c['ArcCount'],networks=c['GroupCount']*4,guards=c['Guards'],cancellations=c['Cancellations'],preparedControls=16,nativeExecuted=False,targetsRead=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r,indent=2)+'\n',encoding='utf8');print(json.dumps(r))
