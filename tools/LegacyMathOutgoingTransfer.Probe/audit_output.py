"""Independent actual raw NN/C21/AL -> temporary publication -> owning outgoing
DOM payload and sorted detached source commands. No vendor execution.
"""
from pathlib import Path
import copy,sys
source=Path(__file__).resolve().parents[1]/'LegacyMathFreshPublisher.Probe/audit_output.py'
code=source.read_text(encoding='utf-8-sig')
exec(compile(code.split('max_error=0')[0],str(source),'exec'),globals())
# Re-run the existing independent numerical/source/typed-publication validation
# against these fresh NN outputs, stopping before its old-format fault controls.
exec(compile(code[code.index('max_error=0'):code.index('for width in(1,3,16):')],str(source),'exec'),globals())

def outgoing(evidence):
    _,pub=model(evidence);nodes=pub['Temporary'];field=evidence['Field']
    selected,=[n for n in field['SelectedNodes']if n['Kind']==3 and n['Name']=='mathField']
    temporary,=[n for n in nodes if n['Kind']==3 and n['Name']=='mathField']
    result,=[n for n in nodes if n['Parent']==temporary['Id']and n['Name']=='mathResult']
    original_children=[n for n in field['SelectedNodes']if n['Parent']==selected['Id']and n['Kind']!=0]
    assert len(original_children)==1 and original_children[0]['Name']=='input'
    value_bytes=sum(len((n[k]or'').encode())for n in nodes for k in('Name','Text','NamespaceUri','NamespacePrefix'))
    additions=sorted((r for r in pub['Registry']if r['Id']!=field['Tag']),key=lambda r:r['Id'])
    ranges=sum(len(r['Ranges'])for r in additions)
    planned=pub['Reservation']+8192+len(nodes)*768+value_bytes*8+ranges*256
    writer=[];commands=[];events=[]
    state=dict(Complete=False,Failed=False,Released=False,Reservation=planned,Phase='claimed',CleanupCount=0,
        DistinctRegistryOwners=True,FourthNameOwner=True,SelectedField=selected['Id'],Ordinal=len(original_children),
        Path=[],Writer=writer,Commands=commands,DomExported=False,SourceExported=False,HandedOff=False,
        Before=-1,After=-1,ReverseDomNull=True,DomOperation=0,PayloadRoot=0,PendingRows=0,SameHandoffPayloads=False)
    def observe(name,index=-1):state['Phase']=name;events.append((name,index,copy.deepcopy(state)))
    observe('Claimed')
    writer.append(dict(Id=1,Kind=4,Name='',Parent=0,Origin=0,ValueTag=0,Text=None,Number=None,NamespaceUri=None,NamespacePrefix=None));observe('WriterCreated')
    def clone(source,parent):
        identity=len(writer)+1
        writer.append(dict(source,Id=identity,Parent=parent,Origin=source['Id']));observe('PayloadNode',identity)
        for attributes in(True,False):
            for n in nodes:
                if n['Parent']==source['Id']and(n['Kind']==0)==attributes:clone(n,identity)
    payload_root=len(writer)+1;clone(result,0);state['DomExported']=True;state['DomOperation']=2;state['PayloadRoot']=payload_root;observe('DomExported')
    assert [r['Id']for r in additions]==list(range(field['Next'],pub['RegistryNext']))
    for r in additions:
        commands.append(dict(Operation=4,Id=r['Id'],Name=r['Name'],Category=r['Category'],Ranges=copy.deepcopy(r['Ranges']),OldCounter20=-1,NewCounter18=-1,DetachedC22=True));observe('SourceSelection',r['Id'])
    commands.append(dict(Operation=7,Id=-1,Name=None,Category=0,Ranges=[],OldCounter20=field['Next'],NewCounter18=pub['RegistryNext'],DetachedC22=False));observe('Counter')
    state['SourceExported']=True;observe('SourceExported');state['HandedOff']=True;state['PendingRows']=1;state['SameHandoffPayloads']=True;observe('Handoff');observe('Complete');state['Complete']=True
    return events,state

assert len(data['Actual'])==6
for row in data['Actual']:
    events,result=outgoing(row['Evidence']);assert row['Transfer']==result
events,result=outgoing(data['Actual'][0]['Evidence'])
assert data['Events']==[dict(Name=n,Index=i)for n,i,_ in events]
assert len(data['Failures'])==len(events)*2==64
for (name,index,prefix),pair in zip(events,[data['Failures'][i:i+2]for i in range(0,len(data['Failures']),2)],strict=True):
    for canceled,failure in zip((False,True),pair,strict=True):
        assert(failure['Stage'],failure['Index'],failure['Canceled'])==(name,index,canceled)
        assert failure['Before']==prefix
        assert failure['After']==dict(prefix,Failed=True)
assert(data['ConcurrentCompleted'],data['ConcurrentRefused'],data['Guards'])==(1,1,168)
if len(sys.argv)>=4:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes()
print(f'PASS 6 actual NN -> full typed temporary -> fourth DOM arena/field-relative row/sorted detached C22 commands/counter;64 independent prefixes/168 guards;feature error {max_error}; no original-page acceptance.')
