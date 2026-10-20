"""Independent PREPARED export reduction of six actual Math publisher outputs.
This does not execute or attest a runtime outgoing-transfer implementation.
"""
from pathlib import Path
import argparse,copy,hashlib,json
p=argparse.ArgumentParser(description=__doc__);p.add_argument('capture',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
raw=a.capture.read_bytes()
assert hashlib.sha256(raw).hexdigest()=='574c580830e6f8c509422be486fdc5e0be1280a1d29e4b95dae5c174d959c51f'
capture=json.loads(raw)

def reduce(row):
    field=row['Evidence']['Field'];pub=row['Publication']
    assert pub['Complete'] and not pub['Failed'] and not pub['HeaderLive']
    old=field['SelectedNodes'];new=pub['Temporary']
    oldroot,=[n for n in old if n['Kind']==3 and n['Name']=='mathField']
    newroot,=[n for n in new if n['Kind']==3 and n['Name']=='mathField']
    assert newroot['Origin']==oldroot['Id']
    def children(nodes,n,attrs=False):return [v for v in nodes if v['Parent']==n['Id'] and ((v['Kind']==0)if attrs else(v['Kind']!=0))]
    def value(n,new=False):return (n['Kind'],n['Name'],n['ValueTag'],n.get('Text'if new else'StringValue'),n.get('Number'if new else'NumericValue'),n['NamespaceUri'],n['NamespacePrefix'])
    def same(oldnode,newnode):
        assert value(oldnode)==value(newnode,True) and newnode['Origin']==oldnode['Id']
        for attr in (True,False):
            left=children(old,oldnode,attr);right=children(new,newnode,attr)
            assert len(left)==len(right)
            for l,r in zip(left,right):same(l,r)
    left=children(old,oldroot);right=children(new,newroot)
    assert len(left)==1 and left[0]['Name']=='input'
    assert len(right)==2 and right[-1]['Name']=='mathResult'
    # Equal nonempty prefix, then one new child. Attributes agree byte-for-byte.
    same(left[0],right[0])
    oldattrs=children(old,oldroot,True);newattrs=children(new,newroot,True)
    assert len(oldattrs)==len(newattrs)==1
    same(oldattrs[0],newattrs[0])
    result=right[-1];payload=[]
    def clone(n,parent):
        identity=len(payload)+1
        payload.append(dict(Id=identity,Parent=parent,SourceTemporaryNode=n['Id'],Value=value(n,True)))
        for attr in (True,False):
            for c in children(new,n,attr):clone(c,identity)
    clone(result,0)
    # Native ordered set service walks signed keys in ascending order.
    registry=pub['Registry'];initial=field['Next'];assert field['Tag']==0 and initial==1
    unchanged,=[r for r in registry if r['Id']==field['Tag']]
    assert unchanged['Name']=='INPUT' and unchanged['Category']==102 and unchanged['Ranges']==field['SelectedRanges']
    additions=sorted((r for r in registry if r['Id']!=field['Tag']),key=lambda r:r['Id'])
    assert [r['Id']for r in additions]==list(range(initial,pub['RegistryNext']))
    assert all(r['Category']==102 and r['Name']in('MATH_SYMBOL','MATH_NODE')and r['Ranges']for r in additions)
    ids={r['Id']for r in additions}
    assert {n['Value'][4]for n in payload if n['Value'][1]=='tag'}==ids
    commands=[dict(Operation=4,Id=r['Id'],Name=r['Name'],Category=r['Category'],Ranges=r['Ranges'],DetachedC22=True,Auxiliary=None)for r in additions]
    assert pub['RegistryNext']>initial
    commands.append(dict(Operation=7,OldCounter20=initial,NewCounter18=pub['RegistryNext']))
    return dict(Policy=row['Policy'],Width=row['Width'],Reversed=row['Reversed'],SelectedField=oldroot['Id'],TemporaryField=newroot['Id'],DomRows=[dict(Operation=2,Path=[],Ordinal=1,Payload=payload)],SourceCommands=commands,PendingRow=dict(Before=-1,After=-1,ReverseDom=None),Scope='Prepared reduction, not a produced runtime transfer or native page acceptance')

results=[reduce(row)for row in capture['Actual']];assert len(results)==6
refusals=0
for mutate in ('input-tag','field-origin','new-id','raw-preservation'):
    row=copy.deepcopy(capture['Actual'][0])
    if mutate=='input-tag':next(n for n in row['Publication']['Temporary']if n['Name']=='tag'and n['Origin'])['Number']=99
    elif mutate=='field-origin':next(n for n in row['Publication']['Temporary']if n['Name']=='mathField')['Origin']=99
    elif mutate=='new-id':row['Publication']['Registry'][-1]['Id']=99
    else:row['Publication']['Registry'][0]['Ranges']=[]
    try:reduce(row)
    except AssertionError:refusals+=1
    else:raise AssertionError(mutate)
a.output.write_text(json.dumps(dict(ActualPublisherCases=6,PreparedReductions=results,RefusalControls=refusals,VendorExecution=False,RuntimeTransferImplemented=False),indent=2)+'\n',encoding='utf8')
print(f'PASS six actual-publisher prepared reductions/{refusals} provenance controls; no runtime transfer claim.')
