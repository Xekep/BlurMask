"""Extract only existing child raw coordinates, no expected labels or NN."""
from pathlib import Path
import argparse,ast,json
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--a',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
t=ast.parse((a.repo/'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py').read_text(encoding='utf-8-sig'));g={};exec(compile(ast.Module(body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom))or isinstance(n,ast.ClassDef)and n.name=='Cases'],type_ignores=[]),'reader','exec'),g)
r=[dict(name=row['Name'],descriptor=c['Descriptor'],raw=c['Raw'])for row in g['Cases'](a.a)for c in row.get('Children',[])]
a.output.write_text(json.dumps(r,separators=(',',':'))+'\n');print(len(r))
