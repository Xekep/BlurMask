"""Fetch redistributable OFL fonts from a pinned Google Fonts revision."""
import hashlib, json, urllib.request
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
REVISION='bd8f81ddb5c74d5c8897b36ad88b440266245103'
FONTS=[('inter','Inter[opsz,wght].ttf','Inter'),('manrope','Manrope[wght].ttf','Manrope'),
       ('sourceserif4','SourceSerif4[opsz,wght].ttf','SourceSerif4'),('stixtwomath','STIXTwoMath-Regular.ttf','STIXTwoMath'),
       ('stixtwotext','STIXTwoText[wght].ttf','STIXTwoText'),('stixtwotext','STIXTwoText-Italic[wght].ttf','STIXTwoTextItalic')]
directory=ROOT/'src/NovaPen/Assets/Fonts'; directory.mkdir(parents=True,exist_ok=True)
reports=[]
for family,filename,local in FONTS:
    base=f'https://raw.githubusercontent.com/google/fonts/{REVISION}/ofl/{family}/'
    data=urllib.request.urlopen(base+urllib.parse.quote(filename)).read()
    (directory/(local+'.ttf')).write_bytes(data)
    license=urllib.request.urlopen(base+'OFL.txt').read().decode('utf-8')
    license=('\n'.join(line.rstrip() for line in license.splitlines())+'\n').encode('utf-8')
    (ROOT/'docs/licenses'/('LICENSE-FONT-'+local+'.txt')).write_bytes(license)
    reports.append({'font':local,'source':base+urllib.parse.quote(filename),'sha256':hashlib.sha256(data).hexdigest(),'bytes':len(data),'license':'SIL-OFL-1.1'})
(ROOT/'docs/ai-fonts.json').write_text(json.dumps(reports,indent=2)+'\n')
print(json.dumps(reports))
