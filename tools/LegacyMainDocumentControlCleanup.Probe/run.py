import argparse,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('program',type=Path);p.add_argument('capture',type=Path)
p.add_argument('--selected',type=Path);p.add_argument('--indices',type=Path);a=p.parse_args()
assert (a.selected is None)==(a.indices is None),'Both current parent captures are required together'
models=['models/ru-RU/ru_RU-ak-cur.lite.res','models/ru-RU/ru_RU-lk-text.lite.res','models/ru-RU/legacy-engine-profile.json','models/ru-RU/legacy-separation-profile.json','models/text-unicode/legacy-case-data.json','models/text-unicode/legacy-text-publication-data.json','models/text-unicode/legacy-normalization-data.json','models/text-unicode/legacy-circled-payload-data.json','models/line-geometry/line-2e3-30.res','models/line-geometry/line-2e3-50.res']
program=a.program.resolve();capture=a.capture.resolve();command=['dotnet',str(program)]if program.suffix=='.dll'else[str(program)]
subprocess.run(command+models+[str(capture)],cwd=root,check=True)
parents=[]if a.selected is None else['--selected',str(a.selected.resolve()),'--indices',str(a.indices.resolve())]
subprocess.run([sys.executable,'-I','-B',str(Path(__file__).with_name('audit_output.py')),str(capture)]+parents,cwd=root,check=True)
