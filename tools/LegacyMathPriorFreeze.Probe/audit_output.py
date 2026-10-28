"""Independent observed selected-DOM scan and prepared freeze-row controls.
Never restores owning authority from the diagnostic capture.
"""
from pathlib import Path
import base64,json,sys
d=json.loads(Path(sys.argv[1]).read_bytes());assert len(d['Actual'])==6 and d['OwnerControls']==89
if len(sys.argv)>3:
 assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes(), 'Managed/native capture differs'
for case in d['Actual']:
 p=case['Prior'];f=case['Freeze'];nodes=p['SelectedField'];children={}
 for n in nodes:children.setdefault(n['Parent'],[]).append(n)
 field=next(n for n in nodes if n['Parent']==0 and n['Name']=='mathField' and n['Kind']==3)
 result=next(n for n in children[field['Id']] if n['Kind']==3 and n['Name']=='mathResult')
 sequence=next(n for n in children[result['Id']] if n['Kind']==3 and n['Name']=='sequence')
 symbols=[n for n in children.get(sequence['Id'],[]) if n['Kind']!=0]
 frozen=[]
 for n in symbols:
  attributes={a['Name']:a for a in children.get(n['Id'],[]) if a['Kind']==0}
  if 'tag' in attributes and 'freeze' in attributes:frozen.append(n)
 assert not frozen # Actual producer has no freeze attribute; not a nonempty claim.
 assert f['Complete'] and not f['Failed'] and not f['Released'] and f['Rows']==[]
 assert f['SequenceChildren']==len(symbols)>0
 textbytes=0
 for n in symbols:
  first=next((x for x in children.get(n['Id'],[]) if x['Kind']!=0),None)
  textbytes+=len(((first or {}).get('Text') or '').encode())
 maximum=max(len(r['Ranges']) for r in p['Registry'])
 assert f['Reservation']==8192+len(symbols)*512+textbytes*4+len(symbols)*maximum*16
expected={
 'absent-tag':(True,0,None),'absent-freeze':(True,0,None),
 'sentinel-minus1':(False,0,None),'missing-selection':(False,0,None),
 'missing-first-child':(False,0,None),'unsupported-first-child':(False,0,None),
 'null-string-pointer':(False,0,None),'heap-string':(True,1,'é'),
 'inline15':(True,1,'abcdefghijklmno'),'empty-string3':(True,1,''),
 'embedded-nul':(True,1,'a\0b'),'clone-refusal':(False,0,None),
 'copied-prefix':(False,1,'x'),'max-is-not-sentinel':(True,1,'x')}
assert len(d['Prepared'])==len(expected)
for c in d['Prepared']:
 success,count,text=expected[c['Name']];assert c['Succeeded']==success and len(c['Rows'])==count
 assert c['Trace'][:2]==['tag:0','freeze:0']
 if c['Name']=='sentinel-minus1':assert c['Trace']==['tag:0','freeze:0','number:0','child:0']
 if c['Name']=='missing-selection':assert c['Trace'][-2:]==['selection:5','child:0']
 for row in c['Rows']:
  assert row['Flag']==1 and base64.b64decode(row['Text'])==text.encode()
  assert [(r['First']['Value'],r['Last']['Value'])for r in row['Ranges']]==[(3,0x10003)]
print(f"PASS6 actual live-selected prior scans -> zero freeze rows; {d['OwnerControls']} owning controls;14 separate native-branch prepared controls.")
