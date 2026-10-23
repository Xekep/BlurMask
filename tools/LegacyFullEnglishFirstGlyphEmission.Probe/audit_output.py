"""B7 actual-output oracle. No NN/vendor load or transcription labels.
Combines pinned raw full-model category semantics and independently verified
old A/B4/B6 receipts with native range projection, not C# B7 as authority.
"""
import argparse
import base64
import copy
import hashlib
import json
import struct
import subprocess
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import reference_emission as emission
import audit_ranges as ranges


def shape(row,children):
    assert row['Status']=='Accepted'
    assert len(row['Sources'])==children
    assert [x['Residue'] for x in row['Variants']]==[0,1]
    assert all(len(v['Children'])==children for v in row['Variants'])
    assert row['NetworkBefore']==row['NetworkAfter']


def schema_controls():
    good=dict(Status='Accepted',Sources=[{}],Variants=[dict(Residue=0,Children=[{}]),dict(Residue=1,Children=[{}])],NetworkBefore=4,NetworkAfter=4)
    shape(good,1);refused=[]
    edits=[('extra-source',lambda x:x['Sources'].append({})),('omitted-variant',lambda x:x['Variants'].pop()),
           ('empty-variants',lambda x:x['Variants'].clear()),('reordered-variant',lambda x:x['Variants'].reverse()),
           ('extra-child',lambda x:x['Variants'][0]['Children'].append({})),('omitted-child',lambda x:x['Variants'][1]['Children'].clear()),
           ('NN-replay',lambda x:x.__setitem__('NetworkAfter',8))]
    for name,edit in edits:
        bad=copy.deepcopy(good);edit(bad)
        try:shape(bad,1)
        except AssertionError:refused.append(name)
        else:raise AssertionError(('schema negative accepted',name))
    return refused


def audit(actual,oldA,old4,old6,contexts,profile):
    assert actual['Name']==oldA['Name']==old4['Name']==old6['Name']
    shape(actual,len(old6['Children']));assert len(oldA['Children'])==len(old4['Children'])==len(old6['Children'])
    expected,_=emission.parent(old4,old6,contexts,profile)
    counts=dict(Children=0,Owners=0,Ranges=0,RangeObservations=0,Attempts=0,Queries=0)
    source_rows=[];source_metadata=[]
    for source,cA,c4,c6 in zip(actual['Sources'],oldA['Children'],old4['Children'],old6['Children'],strict=True):
        desc=c6['Descriptor'];assert source['Descriptor']==cA['Descriptor']==c4['Descriptor']==desc
        assert source['NNEvaluated'] is False and source['Raw']==cA['Raw'] and source['ParentMap']==cA['Map']
        canonical=contexts[actual['Name'],desc]
        for key in ['Seeds','SeedNodes','Nodes']:assert source[key]==canonical[key],('same independently audited source contexts',key)
        source_metadata.append(ranges.metadata(source));source_rows.append(source)
    for variant_index,variant in enumerate(actual['Variants']):
        event=hashlib.sha256();events=0;stages=[];attempts=[];queries=[];range_obs=[]
        def emit(child,tag):
            nonlocal events
            events+=1;event.update(f'{child}:{tag}\n'.encode())
        def stage(child,name):stages.append([child,name]);emit(child,'s:'+name)
        stage(-1,'entry')
        for ci,(child,want,c6,source) in enumerate(zip(variant['Children'],expected['Children'],old6['Children'],source_rows,strict=True)):
            stage(ci,'child:entry');stage(ci,'child:cloned');old_variant=c6['Variants'][variant_index]
            assert old_variant['Residue']==variant_index and child['Descriptor']==want['Descriptor']
            assert child['OriginalOwnersAfter']==old_variant['Owners'],'B6 private pool changed'
            old_words=[dict(Storage=w['Storage'],References=w['References']) for w in old_variant['Words']]
            assert child['OriginalWordsAfter']==child['Words']==old_words,'WORD bytes/refs changed'
            pool=emission.clone(old_variant['Owners']);plans={p['encounter']:p for p in want['QueryPlans']};sidecars={};fresh=attempted=0
            refs=[ref for word in old_variant['Words'] for ref in word['References']]
            for encounter,ref in enumerate(refs):
                arc=(ref>>24)&65535
                if arc&32768:continue
                node=ref&0xffffff;key=ref&emission.MASK
                if key not in sidecars:
                    current=source['Nodes'][node][arc];first,last=current['Start'],current['End']
                    projected,_=ranges.project(source,ranges.path(source,first,last),source_metadata[ci]);assert 1<=len(projected)<=8
                    for phase in [0,1]:
                        #Expected actual callbacks count/fill EACH return the same
                        #independently derived rows, never B7's retained sidecar.
                        for ordinal,value in enumerate(projected):
                            range_obs.append([ci,key,phase,ordinal,len(projected),*value]);emit(ci,'r:'+str(phase))
                        stage(ci,'range:counted' if phase==0 else 'range:filled')
                    sidecars[key]=projected
                if encounter not in plans:continue
                p=plans[encounter];ordinal,existing=emission.append(pool[key],p['descriptor'],False,bytes(40));attempted+=1
                stage(ci,'glyph:interned')
                if not existing:
                    fresh+=1;stage(ci,'glyph:score');queries.append([ci,node,arc,p['symbol'],p['category'],p['prototype'],65535,1,0,p['cost'],ranges.bits(p['score'])]);emit(ci,'q')
                    emission.refresh(pool[key]['rows'][ordinal],p['symbol'],p['likelihood'],p['score'],p['cost'])
                attempts.append([ci,encounter,ref,key,p['descriptor'],ordinal,p['symbol'],existing]);emit(ci,'a');stage(ci,'glyph:complete')
            assert child['Owners']==emission.output_owners(pool)==want['Variants'][variant_index]['Owners'],'full40B/count/flags/opaque/ordinal mismatch'
            assert child['Attempts']==attempted and child['Fresh']==fresh and child['RangePasses']==2*len(sidecars)
            assert child['Ranges']==[dict(Key=key,Rows=value) for key,value in sidecars.items()]
            stage(ci,'child:materialized');stage(ci,'child:published')
            if variant_index==0:
                counts['Children']+=1;counts['Owners']+=len(pool);counts['Ranges']+=sum(map(len,sidecars.values()));counts['Attempts']+=attempted;counts['Queries']+=fresh
        assert variant['Attempts']==attempts and variant['Queries']==queries
        assert variant['RangeObservations']==range_obs and variant['Stages']==stages
        assert variant['EventCount']==events and variant['EventSha256']==event.hexdigest(),'actual score/query/interner/count/fill callback order'
        if variant_index==0:counts['RangeObservations']+=len(range_obs)
    return counts


def actual_tampers(actual,cA,c4,c6,contexts,profile):
    #Applied only after a real capture has passed; no fake actual output is
    #manufactured in compile-only review. No NN or production Run is invoked.
    edits=[('query-prototype',lambda x:x['Variants'][0]['Queries'][0].__setitem__(5,99)),
           ('query-ordinal',lambda x:x['Variants'][0]['Queries'][0].__setitem__(6,0)),
           ('query-primitive',lambda x:x['Variants'][0]['Queries'][0].__setitem__(8,1)),
           ('query-cost',lambda x:x['Variants'][0]['Queries'][0].__setitem__(9,-1)),
           ('lost-count-phase',lambda x:x['Variants'][0]['RangeObservations'].pop(0)),
           ('changed-fill-row',lambda x:x['Variants'][0]['RangeObservations'][-1].__setitem__(-1,-1)),
           ('source-raw-owner',lambda x:x['Sources'][0]['Raw'][0][0].__setitem__(0,123456)),
           ('prepared-map',lambda x:x['Sources'][0]['Map'].__setitem__(0,123456)),
           ('source-descriptor',lambda x:x['Sources'][0].__setitem__('Descriptor',-1)),
           ('source-parentmap',lambda x:x['Sources'][0]['ParentMap'].__setitem__(0,255)),
           ('callback-order',lambda x:x['Variants'][0].__setitem__('EventSha256','0'*64)),
           ('extra-variant',lambda x:x['Variants'].append(copy.deepcopy(x['Variants'][0]))),
           ('extra-child',lambda x:x['Variants'][0]['Children'].append(copy.deepcopy(x['Variants'][0]['Children'][0])))]
    assert actual['Variants'][0]['Queries'] and actual['Variants'][0]['RangeObservations']
    refused=[]
    for name,edit in edits:
        bad=copy.deepcopy(actual);edit(bad)
        try:audit(bad,cA,c4,c6,contexts,profile)
        except (AssertionError,KeyError,IndexError):refused.append(name)
        else:raise AssertionError(('actual tamper accepted',name))
    return refused


def main():
    p=argparse.ArgumentParser();p.add_argument('--schema-controls',action='store_true');p.add_argument('--output',type=Path,required=True)
    for name in ['repo','models','a','b4','b6','contexts','capture']:p.add_argument('--'+name,type=Path)
    a=p.parse_args()
    if a.schema_controls:
        out=dict(Status='PASS_PREPARED_SCHEMA_ONLY_NO_B7',Refused=schema_controls());a.output.write_text(json.dumps(out,indent=2)+'\n');print(json.dumps(out));return
    assert all(getattr(a,n) for n in ['repo','models','a','b4','b6','contexts','capture'])
    #This independently rechecks every source graph/seed ID against native
    #stable-interleaving DP; no frozen path/ID mapping is simply trusted.
    proof=a.output.with_name(a.output.stem+'-context-proof.json')
    subprocess.run([sys.executable,'-I',str(a.repo/'tools/LegacyFullEnglishFirstPlainText.Probe/reference_contexts.py'),
                    '--repo',str(a.repo),'--capture',str(a.contexts),'--b4',str(a.b4),'--output',str(proof)],check=True)
    Cases=emission.definitions(a.repo/'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py',{'Cases'})['Cases']
    ctx=json.loads(a.contexts.read_bytes());contexts={(r['Name'],r['Descriptor']):r for r in ctx};assert len(contexts)==len(ctx)
    profile=emission.raw_categories(a.models);old6=json.loads(a.b6.read_bytes());capture=json.loads(a.capture.read_bytes());assert capture['TargetsRead'] is False
    total={};cases=0;tamper_results=[]
    for actual,cA,c4,c6 in zip(capture['Actual'],Cases(a.a),Cases(a.b4),old6['Actual'],strict=True):
        values=audit(actual,cA,c4,c6,contexts,profile);cases+=1
        if not tamper_results and actual['Variants'][0]['Queries'] and actual['Variants'][0]['RangeObservations']:
            tamper_results=actual_tampers(actual,cA,c4,c6,contexts,profile)
        for key,value in values.items():total[key]=total.get(key,0)+value
    assert capture['Accepted']==cases and capture['Refused']==0 and total['Children']==len(contexts)
    report=dict(Status='PASS_ACTUAL_B7_RANGES_ROWS_QUERY_ORDER',TargetsRead=False,SDKExecuted=False,NNReplayed=False,Cases=cases,**total,
                SchemaNegatives=schema_controls(),ActualTamperRefused=tamper_results,InputSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),
                Scope='Actual ordinary source metadata/range composition, all40B and callbacks; no720/finalguide/accuracy. Upstream265 fitting remains separately owned.')
    a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))


if __name__=='__main__':main()
