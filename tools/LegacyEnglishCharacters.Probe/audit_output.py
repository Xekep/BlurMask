"""Prepared components only; supplemental ledger fields never enter the old numeric oracle."""
import json,sys,subprocess
from pathlib import Path
assert len(sys.argv)==6
reuse,source,capture,report,growth=map(Path,sys.argv[1:])
assert all(not p.exists() for p in (report,growth))
x=json.loads(capture.read_bytes());assert type(x)is dict and 'RangeBudgets'in x and 'GrowthChecks'in x
# EXACT two-field projection; all old fields remain subject to the original strict oracle.
old={k:v for k,v in x.items()if k not in ('RangeBudgets','GrowthChecks')}
projected=report.with_name(report.stem+'-numeric-input.json');assert not projected.exists()
projected.write_bytes((json.dumps(old,separators=(',',':'))+'\n').encode())
base=Path(__file__).parent
subprocess.run([sys.executable,'-I','-B',str(base/'audit_prepared.py'),str(reuse),str(source),str(projected),str(report)],check=True)
subprocess.run([sys.executable,'-I','-B',str(base/'audit_growth.py'),str(capture),str(growth)],check=True)
