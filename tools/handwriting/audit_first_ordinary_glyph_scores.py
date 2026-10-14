"""Read-only default first confidence profile and scalar getter proof."""
import argparse, hashlib, json, re, sys
from pathlib import Path
from audit_legacy_character_properties import PE

p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2]
raw=(root/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pe=PE(raw)
assert pe.read(0x1802ba2c0+0xd8,'Q')[0]==0x1800ebd30
assert bytes(pe.read(0x1801b0873,'10B')).hex()=='0fb643198886a8010000'
assert pe.read(0x18013e92a+0x1ae926,'d')[0]==.575
assert pe.read(0x1800ebd4a+0x201356,'f')[0]==1/32768
source=(root/'research/myscript/output/MyScriptText.dll/decompiled.c').read_text()
def function(address):
    m=re.search(r'/\* Address 180'+address+r';.*?(?=/\* Address |\Z)',source,re.S)
    if not m:raise ValueError('Missing pinned native export:'+address)
    return m[0]
packet=function('150d00');assert '(char)(uVar20 >> 0xb)' in packet
profile=function('1c2940');assert 'param_2 + 0x108,4,1' in profile
assert '*(int *)(param_2 + 0x108) == 0' in profile and '*(undefined8 *)(param_2 + 0x110) = 0' in profile
confidence=function('1bcbd0')
assert 'local_f0 = 0x14' in confidence and 'pcVar5 = *(code **)(*plVar8 + 0xd8)' in confidence
assert 'fVar19 + fVar18 + fVar20 + fVar21 + fVar22 + fVar23 + fVar24' in confidence
assert '/ (float)uVar16' in confidence
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(dict(schemaVersion=1,sourceSha256=hashlib.sha256(raw).hexdigest(),
    probabilityGetter='2BA2C0.D8=0EBD30; quantizedEXP*float1/32768',
    freshIndividualChannels='150D00 flagsbit11 ->packet19 ->1B0873/77 runtime1A8',
    defaultCalibration='Mode108=0 clears tables110/118;13E922 reads DOUBLE0.575',
    aggregation='Retained top20, family1+family0 then remaining5, divide active-family count',
    noVendorExecution=True),indent=2),encoding='utf-8')
print('PASS pinned fresh flag, EXP getter, mode0 configuration, DOUBLE0.575 and ordered confidence aggregation; DLL was not executed.')
