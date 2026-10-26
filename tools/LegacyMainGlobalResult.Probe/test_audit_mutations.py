"""Negative captures for independent membership/order/fullquery checks.
AK LK valid_capture OUTPUT_DIR. Writes evidence only to explicit OUTPUT_DIR.
"""
import copy,hashlib,json,subprocess,sys
from pathlib import Path
if len(sys.argv)!=5:raise SystemExit('AK LK CAPTURE OUTPUT_DIR')
ak,lk,capture=map(Path,sys.argv[1:4]);out=Path(sys.argv[4]);out.mkdir(parents=True,exist_ok=True)
data=json.loads(capture.read_bytes());auditor=Path(__file__).with_name('audit_output.py');reports=[]
def sample():return copy.deepcopy(data)
def reject(name,value,marker):
 path=out/(name+'.json');path.write_text(json.dumps(value,separators=(',',':'))+'\n',encoding='utf-8')
 result=subprocess.run([sys.executable,str(auditor),str(ak),str(lk),str(path)],capture_output=True,text=True)
 diagnostic=result.stdout+result.stderr;(out/(name+'.log')).write_text(diagnostic,encoding='utf-8')
 assert result.returncode!=0 and marker in diagnostic,(name,result.returncode,diagnostic)
 reports.append(dict(name=name,capture=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),exitCode=result.returncode,rejectedAt=marker))
#The raw AL header fixes standalone mixed output178; only it admits32768.
for name,index,value in [('ordinary-probability-overflow',0,32768),('special-probability-overflow',178,32769),('ordinary-probability-negative',0,-1),('special-probability-negative',178,-1),('boolean-probability',0,False)]:
 x=sample();x[0]['groups'][0]['probabilities'][0][index]=value;reject(name,x,'family-probability-domain')
x=sample();x[0]['groups'][0]['candidates'].pop();reject('candidate-delete',x,'candidate-membership/order')
x=sample();v=x[0]['groups'][0]['candidates'];v[0],v[1]=v[1],v[0];reject('candidate-swap',x,'candidate-membership/order')
x=sample();g=x[0]['groups'][0];present={r[0]for r in g['candidates']};symbol=next(i for i in range(192)if i not in present)
g['candidates'][0][:2]=[symbol,g['finalCosts'][symbol]];reject('candidate-membership',x,'candidate-membership/order')
x=sample();raw=bytearray.fromhex(x[0]['queryRows']);raw[16]^=1;x[0]['queryRows']=raw.hex();reject('query-scalar-corruption',x,'prepared-retained-map-query')
x=sample();x[0]['queryRows']=x[0]['queryRows'][64:];reject('query-row-delete',x,'prepared-retained-map-query')
#Try every currently permitted map position until an emitted root edge changes.
#This mutation preserves raw-map dimensions and valid symbol domain, exposing
#membership checking rather than only malformed-data checks.
for index,value in enumerate(data[0]['retainedWorkingMap']):
 if value==65535:continue
 x=sample();x[0]['retainedWorkingMap'][index]=65535
 path=out/'map-search.json';path.write_text(json.dumps(x,separators=(',',':'))+'\n',encoding='utf-8')
 result=subprocess.run([sys.executable,str(auditor),str(ak),str(lk),str(path)],capture_output=True,text=True)
 if result.returncode and'prepared-retained-map-query'in result.stderr:
  reject('retained-map-membership',x,'prepared-retained-map-query');break
else:raise AssertionError('No actual retained-map membership witness')
(out/'report.json').write_text(json.dumps(dict(status='PASS',negativeControls=reports,productionChanged=False),indent=2)+'\n',encoding='utf-8')
print('PASS11 independent negative captures: probability domains, candidate delete/swap/membership, full query scalar/delete and retained-map membership; no production edits')
