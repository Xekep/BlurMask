"""Run a portable Managed/Native probe, then independently audit its output."""
import argparse,subprocess,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('program',type=Path);p.add_argument('capture',type=Path);a=p.parse_args()
command=(["dotnet",str(a.program)]if a.program.suffix.lower()=='.dll' else[str(a.program)])+[str(a.capture)]
subprocess.run(command,check=True)
subprocess.run([sys.executable,str(Path(__file__).with_name('audit_output.py')),str(a.capture)],check=True)
