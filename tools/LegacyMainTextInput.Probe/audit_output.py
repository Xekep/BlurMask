"""Independent selected glyph40 -> source720 and NumPy IRLS/guide audit."""
import base64,importlib.util,json,math,pathlib,struct,sys
import numpy as np
root=json.loads(pathlib.Path(sys.argv[1]).read_bytes());dec=base64.b64decode
def load(name,folder):
    p=pathlib.Path(__file__).parents[1]/folder/'reference_controls.py'
    spec=importlib.util.spec_from_file_location(name,p);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
transfer=load('text_input_transfer','LegacyFirstResultLine.Probe');fit=load('text_input_fit','LegacyCharacterLineFit.Probe');f=fit.f
rows_checked=guides_checked=0
for sample in root['inputs']:
    published=list(map(dec,sample['source720']));descriptors=list(map(dec,sample['descriptors']));guides=list(map(dec,sample['guides']))
    exponent=struct.unpack('<f',struct.pack('<i',sample['exponentBits']))[0];assert exponent==.75
    assert len(guides)==sample['submissions']==len(sample['first'])
    firstrow=firstphysical=ordinal=0
    for desc in descriptors:
        if struct.unpack_from('<H',desc)[0]&15==3:
            assert struct.unpack_from('<i',desc,16)[0]==-1
            continue
        line=struct.unpack_from('<i',desc,16)[0];start,end=struct.unpack_from('<ii',desc,52);a,z=struct.unpack_from('<ii',desc,36)
        assert line==ordinal and start==firstrow and start<end<=len(published) and a==firstphysical and z>=a
        actual=sample['first'][ordinal];glyphs=list(map(dec,actual['glyphs']));assert len(glyphs)==end-start
        items=[]
        for row,glyph in zip(published[start:end],glyphs):
            assert len(row)==720 and len(glyph)==40
            assert struct.unpack_from('<H',row,40)[0]==struct.unpack_from('<H',glyph,8)[0]
            confidence=struct.unpack_from('<f',glyph,12)[0];weight=struct.unpack_from('<f',row,80)[0]
            want=f(math.pow(confidence,exponent));assert abs(weight-want)<=max(abs(want)*2e-7,1e-8)
            assert row[84:88]==glyph[16:20] and row[240:244]==glyph[20:24]
            b=transfer.transfer(row,sample['markers']);n=struct.unpack_from('<I',b,4)[0];candidates=[]
            for i in range(n):
                weight,coefficient=struct.unpack_from('<ff',b,24+i*8);kind=struct.unpack_from('<i',b,184+i*4)[0]
                top,atop=struct.unpack_from('<ff',b,264+i*8);bottom,abottom=struct.unpack_from('<ff',b,424+i*8)
                candidates.append([weight,coefficient,kind,top,atop,bottom,abottom])
            items.append(dict(known=bool(b[0]),clip=bool(b[1]),marker=bool(b[2]),bounds=list(struct.unpack_from('<4f',b,8)),candidates=candidates));rows_checked+=1
        profile=sample['fitProfile'];expected=fit.run(dict(items=items,profile=profile,force=False,incoming=float(np.finfo(np.float64).max)))['output']
        points=[p for stroke in sample['parentPoints'][a:z+1] for p in stroke]
        fraction=f(f(max(p[1] for p in points)-min(p[1] for p in points))*f(.05))
        if fraction>expected[1]:expected=[f(f(max(p[1] for p in points)+min(p[1] for p in points))*f(.5)),max(fraction,profile[4]),0]
        guide=guides[line];baseline,height=struct.unpack_from('<ff',guide,12);slope=struct.unpack_from('<f',guide,8)[0]
        assert np.allclose([baseline,height,slope],expected,rtol=2e-5,atol=2e-4)
        assert height>0 and struct.unpack_from('<ii',guide,32)==(-1,-2147483648)
        cosine=f(1/math.sqrt(float(f(slope*slope))+1));sine=f(math.sqrt(float(f(1-f(cosine*cosine)))))
        if slope>0:sine=-sine
        cx=f(f(max(p[0] for p in points)+min(p[0] for p in points))*f(.5));cy=f(f(cx*slope)+baseline)
        tx=f(f(f(cy*sine)-f(cx*cosine))+cx);ty=f(f(f(-sine*cx)-f(cy*cosine))+cy)
        b=f(f(f(baseline*cosine)+f(sine*0))+ty);h=f(f(f(f(f(baseline+height)*cosine)+f(sine*0))+ty)-b)
        assert np.allclose(struct.unpack_from('<ff',guide),[b,max(h,profile[4])],atol=2e-4)
        current=[p for stroke in sample['currentPoints'][a:z+1] for p in stroke]
        for original,now in zip(points,current):assert np.allclose(now,[f(f(f(cosine*original[0])-f(sine*original[1]))+tx),f(f(f(sine*original[0])+f(cosine*original[1]))+ty)],atol=2e-4)
        firstrow=end;firstphysical=z+1;ordinal+=1;guides_checked+=1
    assert firstrow==len(published) and firstphysical==len(sample['smoothedCounts'])
    assert all(n>0 for n in sample['smoothedCounts'])
assert (rows_checked,guides_checked,root['multiwordInputs'],root['guards'])==(root['rows'],8,3,45)==(26,8,3,45)
print('PASS independent26 actual selected glyph40->720 label/score/exponent/cost fields;8 NumPy IRLS guides and deskew; genuine multiword/two-row input ownership')
