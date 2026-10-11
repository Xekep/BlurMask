"""Original bit-ownership oracle for the native initial word prefix."""
import base64,json,random,sys
rng=random.Random(0x1f8d60);cases=[]
u8=lambda:rng.randrange(256)
u16=lambda:rng.randrange(65536)
i32=lambda:rng.randrange(-2**31,2**31)
for _ in range(512):
    fields=dict(SourceTag=rng.getrandbits(64),Auxiliary08=rng.getrandbits(64),
        CandidateCost10=i32(),IncrementalCost14=i32(),Label24=u16(),SourceNode18=u16(),
        PackedSourceArcIndex=u16(),SourceArcIndex1c=u16(),CompositeCategory=u8(),PreviousArcIndex1e=u16(),
        SourceSpan=rng.randrange(-128,128),Count26=u8(),HeaderFlag5=u8(),HeaderFlag6=u8(),
        RequestedSecondaryFlag=u8(),PackedKind27=u8(),SmallState21=u8(),StateBit6=u8(),StateBit7=u8(),
        Auxiliary30=i32(),Auxiliary34=i32(),AnchorEndingNode38=i32(),AnchorArcIndex3c=u16())
    scale=rng.choice([0.0,-0.0,1.0,-1.0,2.5]);bits=0
    def assign(offset,width,value):
        global bits
        mask=((1<<width)-1)<<offset;bits=(bits&~mask)|((value&((1<<width)-1))<<offset)
    assign(0,64,fields['SourceTag'])
    assign(41,1,(fields['SourceTag']>>41)&1 if fields['HeaderFlag6']==0 else 0)
    assign(47,1,fields['HeaderFlag5'])
    assign(48,1,fields['HeaderFlag6']);assign(49,1,0)
    assign(8*8,64,fields['Auxiliary08'])
    assign(0x10*8,32,fields['CandidateCost10']);assign(0x14*8,32,fields['IncrementalCost14'])
    assign(0x18*8,16,fields['SourceNode18']);assign(0x1a*8,6,fields['CompositeCategory']);assign(0x1a*8+6,10,fields['PackedSourceArcIndex'])
    assign(0x1c*8,16,fields['SourceArcIndex1c']);assign(0x1e*8,16,fields['PreviousArcIndex1e'])
    assign(0x20*8,8,fields['SourceSpan']);assign(0x21*8,6,fields['SmallState21'])
    assign(0x21*8+6,1,fields['StateBit6']);assign(0x21*8+7,1,fields['StateBit7'])
    assign(0x22*8,16,65535);assign(0x24*8,16,fields['Label24']);assign(0x26*8,8,fields['Count26'])
    assign(0x27*8,1,0 if scale==0 else fields['RequestedSecondaryFlag']);assign(0x27*8+1,3,fields['PackedKind27'])
    assign(0x28*8,64,2**64-1);assign(0x30*8,32,fields['Auxiliary30']);assign(0x34*8,32,fields['Auxiliary34'])
    assign(0x38*8,32,fields['AnchorEndingNode38']);assign(0x3c*8,16,fields['AnchorArcIndex3c']);assign(0x3e*8,16,65535)
    cases.append(dict(fields=fields,scale=scale,expected=base64.b64encode(bits.to_bytes(0x58,'little')).decode()))
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump(cases,stream,separators=(',',':'))
print('Wrote',len(cases),'independent word-prefix bit-ownership controls')
