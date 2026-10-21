"""Independent raw alphabet, descriptor/720 ownership and NumPy IRLS audit.

Expected text is neither input to the runtime nor an accuracy assertion.
"""
import base64, importlib.util, json, math, pathlib, struct, sys
import numpy as np

ak=pathlib.Path(sys.argv[1]).read_bytes()
root=json.loads(pathlib.Path(sys.argv[2]).read_bytes())
p=6219;header=struct.unpack_from('<8H',ak,p);p+=16;count=header[-1]
assert count==192
n=struct.unpack_from('<H',ak,p)[0];p+=2
for increment in (32,64,1):
    table=struct.unpack_from('<'+'H'*n,ak,p);p+=2*n;n=[v for v in table if v!=65535][-1]+increment
spans=struct.unpack_from('<'+'H'*(n+1),ak,p);p+=2*(n+1)
slots=[struct.unpack_from('<II',ak,p+i*8) for i in range(spans[-1]+1)];p+=8*(spans[-1]+1)
strings=ak[p:p+slots[-1][0]];p+=slots[-1][0]
texts={label:strings[off:slots[i+1][0]].decode('utf8') for i,(off,label) in enumerate(slots[:-1])}
labels={s:i for i,s in texts.items()};reverse=struct.unpack_from('<'+'H'*count,ak,p)
virtual={i for i,v in enumerate(reverse) if v==65535};assert virtual=={140,178,179}
enabled=set(range(count))-virtual
if any(labels.get(v,-1) in enabled for v in ['-','_','\U000f0002','\U000f0003','\u30fc','\u4e00']):enabled.add(header[1])
if header[2]!=65535 and labels.get('\U000f0001',-1) in enabled:enabled.add(header[2])
enabled.update(v for i,v in enumerate(header[:7]) if i in (3,0,4,5,6) and v!=65535)
mask=bytes(int(i in enabled) for i in range(count))

def load(name,folder):
    path=pathlib.Path(__file__).parents[1]/folder/'reference_controls.py'
    spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
transfer=load('owned_input_transfer','LegacyFirstResultLine.Probe')
fit=load('owned_input_fit','LegacyCharacterLineFit.Probe');f=fit.f
dec=base64.b64decode
guides_checked=rows_checked=cells_checked=0
supported=[];unsupported=[]
for sample in root['inputs']:
    before=list(map(dec,sample['descriptorsBeforeFirst']))
    ordinary=[v for v in before if struct.unpack_from('<H',v)[0]&15!=3]
    assert len(ordinary)==sample['submissions']
    if not sample['supported']:
        assert 'single-word path' in sample['refusal'] or '24A empty-result' in sample['refusal']
        unsupported.append(sample['id']);continue
    supported.append(sample['id']);published=list(map(dec,sample['source720']))
    descriptors=list(map(dec,sample['descriptors']));guides=list(map(dec,sample['guides']))
    assert len(guides)==len(ordinary)==len(sample['first'])
    firstrow=0;firstphysical=0;ordinal=0
    for before_d,desc in zip(before,descriptors):
        kind=struct.unpack_from('<H',desc)[0]&15
        assert kind==struct.unpack_from('<H',before_d)[0]&15
        if kind==3:
            assert struct.unpack_from('<i',desc,16)[0]==-1
            continue
        line=struct.unpack_from('<i',desc,16)[0]
        start,end=struct.unpack_from('<ii',desc,52);a,z=struct.unpack_from('<ii',desc,36)
        assert line==ordinal and start==firstrow and start<end<=len(published) and a==firstphysical and a<=z
        assert before_d[36:44]==desc[36:44]
        actual=sample['first'][ordinal]
        assert bytes.fromhex(actual['mask'])==mask
        assert actual['wordBoundary'][0] and not any(actual['wordBoundary'][1:])
        assert [struct.unpack_from('<H',r,40)[0] for r in published[start:end]]==actual['labels']
        for primary,merged in zip(map(dec,actual['primary98']),map(dec,actual['b8'])):
            assert len(primary)==len(merged)==200
            for at in range(0,200,40):
                pcost=struct.unpack_from('<i',primary,at)[0];mcost=struct.unpack_from('<i',merged,at)[0]
                if mcost<0x4000000:
                    assert pcost<0x4000000 and primary[at+12:at+18]==merged[at+12:at+18]
                    cells_checked+=1
        items=[]
        for row in published[start:end]:
            assert len(row)==720
            b=transfer.transfer(row,sample['markers']);n=struct.unpack_from('<I',b,4)[0];candidates=[]
            for i in range(n):
                weight,coefficient=struct.unpack_from('<ff',b,24+i*8);k=struct.unpack_from('<i',b,184+i*4)[0]
                top,atop=struct.unpack_from('<ff',b,264+i*8);bottom,abottom=struct.unpack_from('<ff',b,424+i*8)
                candidates.append([weight,coefficient,k,top,atop,bottom,abottom])
            items.append(dict(known=bool(b[0]),clip=bool(b[1]),marker=bool(b[2]),bounds=list(struct.unpack_from('<4f',b,8)),candidates=candidates))
            rows_checked+=1
        profile=sample['fitProfile'];expected=fit.run(dict(items=items,profile=profile,force=False,incoming=float(np.finfo(np.float64).max)))['output']
        points=[pt for st in sample['parentPoints'][a:z+1] for pt in st]
        miny=min(q[1] for q in points);maxy=max(q[1] for q in points);fraction=f(f(maxy-miny)*f(.05))
        if fraction>expected[1]:expected=[f(f(maxy+miny)*f(.5)),max(fraction,profile[4]),0]
        guide=guides[line];baseline,height=struct.unpack_from('<ff',guide,12);slope=struct.unpack_from('<f',guide,8)[0]
        assert np.allclose([baseline,height,slope],expected,rtol=2e-5,atol=2e-4)
        assert height>0 and struct.unpack_from('<ii',guide,32)==(-1,-2147483648)
        cosine=f(1/math.sqrt(float(f(slope*slope))+1));sine=f(math.sqrt(float(f(1-f(cosine*cosine)))))
        if slope>0:sine=-sine
        cx=f(f(max(q[0] for q in points)+min(q[0] for q in points))*f(.5));cy=f(f(cx*slope)+baseline)
        tx=f(f(f(cy*sine)-f(cx*cosine))+cx);ty=f(f(f(-sine*cx)-f(cy*cosine))+cy)
        b=f(f(f(baseline*cosine)+f(sine*0))+ty);h=f(f(f(f(f(baseline+height)*cosine)+f(sine*0))+ty)-b)
        assert np.allclose(struct.unpack_from('<ff',guide),[b,max(h,profile[4])],atol=2e-4)
        current=[pt for st in sample['currentPoints'][a:z+1] for pt in st]
        for original,now in zip(points,current):
            assert np.allclose(now,[f(f(f(cosine*original[0])-f(sine*original[1]))+tx),f(f(f(sine*original[0])+f(cosine*original[1]))+ty)],atol=2e-4)
        guides_checked+=1;ordinal+=1;firstrow=end;firstphysical=z+1
    assert firstrow==len(published) and firstphysical==len(sample['smoothedCounts'])
    assert all(n>0 for n in sample['smoothedCounts'])
assert supported==['oneMIR','twoRowsReverse','twoWordsOffset230','twoWordsOffset300']
assert unsupported==['twoWordsOffset450','twoWordsOffset700','ownEnglishHIHI']
assert (root['passed'],root['unsupported'],root['multipleInputs'],root['guards'])==(4,3,1,6)
assert (root['duplicateRawEndThird'],root['duplicateRetainedCount'])==(3,2)
assert (root['identicalEndingThird'],root['identicalRetainedCount'])==(3,1)
assert root['identicalClassifiedGroups']==1
assert guides_checked==5 and rows_checked==14
print(f'PASS independent raw192 AL masks/{cells_checked} primary-B8 cells/{rows_checked} source720 rows/{guides_checked} NumPy IRLS guides +deskew; 3 actual multiword scope refusals, no text accuracy assertion')
