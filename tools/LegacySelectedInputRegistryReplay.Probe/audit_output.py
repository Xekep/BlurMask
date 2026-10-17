"""Independent registry replay from captured actual pending C4D commands."""
import argparse,base64,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);a=p.parse_args()
d=json.loads(a.capture.read_text());selection_rows=ranges=prefixes=0
for item in d['actual']:
    next_id=item['initialNext'];maximum=item['initialMax'];expected=[];done=0;failed=False
    assert next_id==1 and maximum==item['inputId']==0
    assert len(base64.b64decode(item['inputRanges']))%16==0
    commands=item['commands'];ids={item['inputId']}
    for i,c in enumerate(commands):
        assert c['identity']==i+1 and c['next']==(i+2 if i+1<len(commands) else 0)
        if c['operation']==4:
            assert c['id']>=0 and c['id'] not in ids and c['category']==100
            ids.add(c['id']);assert c['name'] in ('CHAR','WORD','TEXT')
            assert len(base64.b64decode(c['ranges']))%16==0
        else:
            assert c['operation']==7 and i==len(commands)-1
            assert base64.b64decode(c['body56'])==struct.pack('<qq',c['new'],c['old'])+bytes(40)
            assert c['old']==item['initialNext']
    if not item['preentryClosed']:
        for i,c in enumerate(commands):
            if i in (item['fail'],item['cancel']):failed=True;break
            if c['operation']==4:
                # The raw request ID is preserved; INPUT/raw IDs are not substituted.
                expected.append(dict(command=c['identity'],id=c['id'],name=c['name'],category=c['category'],ranges=c['ranges']))
                if c['id']>=next_id:next_id=c['id']+1
                if c['id']>=maximum:maximum=c['id']
            elif next_id<c['new']:
                assert c['new']>maximum;next_id=c['new']
            done+=1
    s=item['state'];assert s['selections']==expected
    assert base64.b64decode(s['counter16'])==struct.pack('<qq',next_id,maximum)
    assert s['completed']==done and s['faulted']==failed
    assert s['success']==(not failed and not item['preentryClosed'])
    selection_rows+=len(expected);ranges+=sum(len(base64.b64decode(c['ranges']))//16 for c in expected)
    prefixes+=failed
assert len(d['actual'])==10 and prefixes==4
for initial,maximum,requested,after,newmaximum in d['numeric']:
    assert after==(requested+1 if requested>=initial else initial)
    assert newmaximum==max(maximum,requested)
assert len(d['numeric'])==4096 and d['guards']>=50
for next_id,maximum,old_value,new_value,result in d['counterControls']:
    assert old_value>=0 and new_value>=0 and next_id>maximum
    assert result==(max(maximum+1,new_value) if next_id<new_value else next_id)
assert len(d['counterControls'])==2048
print(f'PASS10 actual command streams/{selection_rows} retained selection rows/{ranges} raw16B ranges/{prefixes} entered refusal prefixes +4096 independent ID controls/{d["guards"]} guards.')
