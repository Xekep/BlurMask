"""Independent native ordinary range oracle: raw metadata, ceil/remainder,
recursive path search, native stable bubble ordering and reverse merge.
No C#/SDK load, no transcription targets. Expected maps are actual upstream
inputs; source265 fitting itself is not reimplemented or claimed here.
"""
import argparse
import base64
import copy
import hashlib
import json
import struct
from pathlib import Path


def f(x): return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x): return struct.unpack('<i',struct.pack('<f',x))[0]


def metadata(s):
    raw=[[[f(x),f(y)] for x,y in stroke] for stroke in s['Raw']]
    kept=[]; identities=[]; endings=[];keys=[];index=0
    #Native167627/167655 SUBSS rounds BEFORE CVTPS2PD and MULSD;
    #do not silently keep the subtraction at Python binary64 precision.
    def different(a,b): return abs(a)+abs(b)<abs(f(a-b))*8388608
    for si,stroke in enumerate(raw):
        indices=[i for i,p in enumerate(stroke) if i==0 or any(different(a,b) for a,b in zip(p,stroke[i-1],strict=True))]
        kept.append(indices);row=[];end=[]
        for j,pi in enumerate(indices):
            row.append([0,si,pi,index,index,0]);keys.append([0,si,pi])
            end.append([0,si,indices[j+1]-1 if j+1<len(indices) else len(stroke)-1,index,index,0]);index+=1
        identities.append(row);endings.append(end)
    assert kept==s['RetainedIndices'],'retained raw duplicate metadata'
    assert [[[bits(x) for x in raw[si][pi]] for pi in ids] for si,ids in enumerate(kept)]==[[[bits(x) for x in p] for p in stroke] for stroke in s['Retained']]
    assert identities==s['OriginalIdentities'] and endings==s['OriginalEndingIdentities'],'raw key/index/ending identity'
    source=[v for row in identities for v in row];end_lookup={row[4]:row for stroke in endings for row in stroke}
    if 'PreCutMap200' in s:
        assert s['Factor']==200 and len(s['CutOriginalIndices'])==sum(s['Counts'])
        #Actual upstream265 point keys compose through incoming factor1 cut IDs;
        #not a geometrical interpolation or a guessed raw ordinal map.
        assert all(0<=i<len(s['PreCutMap200']) for i in s['CutOriginalIndices'])
        assert s['Map']==[s['PreCutMap200'][i] for i in s['CutOriginalIndices']],'actual factor1->200 map composition'
    pointmap=s['Map'];factor=s['Factor'];assert factor>0 and (not pointmap or len(pointmap)==sum(s['Counts']))
    resolved=[];global_index=0
    for count in s['Counts']:
        row=[]
        for _ in range(count):
            ticks=pointmap[global_index] if pointmap else global_index;whole,rem=divmod(ticks,factor) if pointmap else (ticks,0)
            at=whole+(rem!=0);assert 0<=at<len(source)
            p=source[at].copy();p[3]=at;p[5]=(factor-rem)&255 if rem else 0;row.append(p);global_index+=1
        resolved.append(row)
    assert resolved==s['Resolved'],'actual prepared ceil/remainder/key map'
    return resolved,end_lookup,keys


def project(s,path,prepared_metadata=None):
    resolved,ends,keys=metadata(s) if prepared_metadata is None else prepared_metadata;rows=[]
    assert 1<=len(path)<=8
    for stroke,start,end in path:
        assert 0<=stroke<len(resolved) and 0<=start<end<=len(resolved[stroke])
        first=resolved[stroke][start];last_start=resolved[stroke][end-1];last=ends[last_start[4]].copy();last[5]=last_start[5]
        rows.append(dict(first=first,last=last,last_start=last_start,tail=end==s['Counts'][stroke]))
    #Literal native comparator order, deliberately different from C# OrderBy.
    def order(r): return (*r['first'][:3],-r['first'][5],*r['last'][:3],-r['last'][5])
    for stop in range(len(rows)-1,0,-1):
        for j in range(stop):
            if order(rows[j])>order(rows[j+1]):rows[j],rows[j+1]=rows[j+1],rows[j]
    merges=0
    for i in range(len(rows)-1,0,-1):
        prev,cur=rows[i-1],rows[i];last_start=prev['last_start'];first=cur['first']
        equal=first[:3]==last_start[:3] and first[5]==last_start[5]
        adjacent=prev['tail'] and last_start[4]+1<len(keys) and first[:3]==keys[last_start[4]+1]
        if equal or adjacent:
            prev.update(last=cur['last'],last_start=cur['last_start'],tail=cur['tail']);rows.pop(i);merges+=1
    return [r['first'][:3]+(r['last'] if r['tail'] else r['last_start'])[:3] for r in rows],merges


def path(s,first,last):
    assert first<=last and 0<=(last>>7)-(first>>7)<=7
    nodes=s['SeedNodes'];visits=0
    def visit(n,ordinal):
        nonlocal visits
        visits+=1;assert visits<=1048576
        a=nodes[n][ordinal]
        if a['End']>last:return None
        if a['End']==last:return [a['Group']]
        for child in a['Succ']:
            p=visit(n+1,child)
            if p is not None:return [a['Group']]+p
        return None
    for i,a in enumerate(nodes[first>>7]):
        if a['Start']==first:
            found=visit(first>>7,i)
            if found is not None:
                assert len(found)<=8
                return [s['Seeds'][g]['Range'] for g in found]
    raise AssertionError('actual ordinary forward path unavailable')


def prepared(data):
    assert data['B7Executed'] is False and data['SDKExecuted'] is False and data['NNEvaluated'] is False
    assert len(data['ProjectionControls'])==5 and len(data['GraphQueries'])==6
    source=data['Source'];metadata(source);merges=0
    for q in data['ProjectionControls']:
        result,m=project(source,q['Path']);assert result==q['Ranges'];merges+=m
    for q in data['GraphQueries']:
        result,m=project(source,path(source,q['First'],q['Last']));assert result==q['Ranges'];merges+=m
    raw=base64.b64decode(data['Allocator']);node=data['PoolOwners'][0];out=base64.b64decode(node['Records']);assert node['Key']==0 and node['PackedCount']==32780
    row=bytearray(raw);struct.pack_into('<i',row,0,10);struct.pack_into('<H',row,4,struct.unpack_from('<H',row,4)[0]&0xfff0)
    struct.pack_into('<I',row,6,0xffff0000);struct.pack_into('<f',row,12,1.);struct.pack_into('<f',row,16,-1.);struct.pack_into('<i',row,20,0);row[24:32]=bytes(8);row[32:40]=bytes([255])*8
    assert out==bytes(row)*3 and out[10:12]==raw[10:12]
    assert merges>0
    precision=data['RawPrecisionControls'];assert len(precision)==10;rounded=0
    for item in precision:
        a=struct.unpack('<f',struct.pack('<i',item['ABits']))[0];b=struct.unpack('<f',struct.pack('<i',item['BBits']))[0]
        assert f(item['A'])==a and f(item['B'])==b and max(abs(a),abs(b))<=1000000
        delta=f(a-b);assert item['SubssBits']==bits(delta),'native-rounded subtraction bits'
        different=abs(a)+abs(b)<abs(delta)*8388608
        assert item['RetainedIndices']==([0,1] if different else [0])
        rounded+=delta!=a-b
    assert rounded>=2,'large opposite-sign scalar intermediate must discriminate binary32 from binary64'
    return dict(ProjectionCases=len(data['ProjectionControls']),GraphQueries=len(data['GraphQueries']),Merges=merges,GenericPoolRows=3,
                NativeSubssControls=len(precision),RoundedIntermediateDiscriminators=rounded)


def main():
    p=argparse.ArgumentParser();p.add_argument('--prepared',type=Path,required=True);p.add_argument('--output',type=Path,required=True);p.add_argument('--tamper-controls',action='store_true');a=p.parse_args()
    data=json.loads(a.prepared.read_bytes());counts=prepared(data);refused=[]
    if a.tamper_controls:
        edits=[('extra-raw-point',lambda d:d['Source']['Raw'][0].append([90,90])),
               ('wrong-remainder',lambda d:d['Source']['Resolved'][0][1].__setitem__(5,0)),
               ('wrong-pointmap',lambda d:d['Source']['Map'].__setitem__(1,0)),
               ('wrong-original-key',lambda d:d['Source']['OriginalIdentities'][0][1].__setitem__(2,1)),
               ('wrong-duplicate-tail',lambda d:d['Source']['OriginalEndingIdentities'][1][-1].__setitem__(2,1)),
               ('wrong-range',lambda d:d['ProjectionControls'][0]['Ranges'][0].__setitem__(2,99)),
               ('wrong-DFS-end',lambda d:d['GraphQueries'][0].__setitem__('Last',0x7fffffff)),
               ('extra-projection',lambda d:d['ProjectionControls'].append(copy.deepcopy(d['ProjectionControls'][0]))),
               ('omitted-DFS',lambda d:d['GraphQueries'].pop()),
               ('lost-dedup-state',lambda d:d['PoolOwners'][0].__setitem__('PackedCount',12))]
        for name,edit in edits:
            bad=copy.deepcopy(data);edit(bad)
            try:prepared(bad)
            except (AssertionError,IndexError,KeyError):refused.append(name)
            else:raise AssertionError(('tamper not rejected',name))
    report=dict(Status='PASS_PREPARED_UNCHANGED_GENERIC_ONLY',B7Executed=False,TargetsRead=False,**counts,TamperRefused=refused,InputSha256=hashlib.sha256(a.prepared.read_bytes()).hexdigest())
    a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))


if __name__=='__main__':main()
