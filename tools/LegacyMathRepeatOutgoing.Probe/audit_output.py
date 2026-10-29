from pathlib import Path
import sys,json
from audit_repeat85 import audit as positive
from audit_failure_prefixes import audit as failures
if __name__=='__main__':
 path=Path(sys.argv[1]);report=dict(PASS=True,positive=positive(path,'Repeat'),failurePrefixes=failures(path));Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(json.dumps(report))
