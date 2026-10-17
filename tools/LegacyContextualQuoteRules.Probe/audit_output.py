"""Independent neighbour-category truth table for enabled21F320 decisions.

No models, labels, source graph or application acceptance are manufactured.
Numeric labels here are explicitly prepared branch controls.
"""
import json,sys
from pathlib import Path
d=json.loads(Path(sys.argv[1]).read_bytes());count=0
for previous,current,nxt,category,carry,arc,kind,label,after in d['cases']:
    previous_control=65529<=previous<=65532;next_control=65529<=nxt<=65532
    #Rows: previous category1; previous ordinary; previous control.
    #Columns: next ordinary/control. Values: opening/closing/unchanged.
    truth=(('open','open'),('close','close'),('open','keep'))
    row=0 if previous<100 and category==1 else 2 if previous_control else 1
    action=truth[row][int(next_control)]
    expected=current;expected_carry=carry
    if current in (10,11) and arc>=0 and kind==0:
        expected={'open':22 if current==10 else 20,
                  'close':23 if current==10 else 21,'keep':current}[action]
        if action=='close' and carry and expected==21:expected=20;expected_carry=False
    expected_carry=expected_carry or expected==24
    assert(label,after)==(expected,expected_carry),(previous,current,nxt,category,carry,arc,kind,label,after)
    count+=1
assert count==25344,count
assert d['checks']>=196608
assert len(d['cursorCases'])==64
for case in d['cursorCases']:
    expected=[(reference,word,index)for word,refs in enumerate(case['words'])for index,reference in enumerate(refs)]
    assert [tuple(read[:3])for read in case['reads']]==expected
print('PASS independent contextual-quote truth table',count,'cases; full ushort leading/trailing wrap guards')
print('PASS64 prepared cross-WORD cursor/vector traversals with empty-word boundaries')
