"""Independent membership-partition/whole720 reference, not runtime code translation.
Selected tuple ordering, raw field layout and inclusive memberships are explicit.
"""
import base64,decimal,functools,math,struct

def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def point(bits):return [struct.unpack('<f',struct.pack('<i',v))[0] for v in bits]
def signed(value,bits):return (value^(1<<(bits-1)))-(1<<(bits-1))
def decode(ref):return signed(ref&0xffffff,24),signed((ref>>24)&65535,16),signed((ref>>40)&65535,16)
def key(ref):return ref&0x010000ffffffffff

def project(points,mapping,ranges):
    projected=[];members=[]
    for g,s,p,h,t,q in ranges:
        assert g==h and 0<=s<=t<len(mapping) and 0<=p<len(points[mapping[s]]) and 0<=q<len(points[mapping[t]]) and (s!=t or p<=q)
        piece=[]
        for physical in range(s,t+1):
            target=mapping[physical];first=p if physical==s else 0;last=q if physical==t else len(points[target])-1
            members.extend(points[target][first:last+1])
            if piece and target!=piece[-1][0]+1:
                projected.append([g,piece[0][0],piece[0][1],h,piece[-1][0],piece[-1][2]]);piece=[]
            piece.append((target,first,last))
        projected.append([g,piece[0][0],piece[0][1],h,piece[-1][0],piece[-1][2]])
    return projected,members

@functools.lru_cache(maxsize=4096)
def pow75(raw4):
    value=struct.unpack('<f',raw4)[0];assert math.isfinite(value) and value>=0
    if value==0:return 0.0
    x=decimal.Decimal.from_float(value)
    with decimal.localcontext() as c:
        c.prec=160;y=(x*x*x).sqrt().sqrt();candidate=struct.unpack('<I',struct.pack('<f',float(y)))[0]
        options=range(max(0,candidate-2),min(0x7f7fffff,candidate+2)+1)
        exact=lambda b:decimal.Decimal.from_float(struct.unpack('<f',struct.pack('<I',b))[0])
        nearest=min(options,key=lambda b:(abs(exact(b)-y),b&1));c.prec=220;y2=(x*x*x).sqrt().sqrt()
        assert nearest==min(options,key=lambda b:(abs(exact(b)-y2),b&1))
    expected=struct.unpack('<f',struct.pack('<I',nearest))[0]
    assert struct.pack('<f',math.pow(float(value),.75))==struct.pack('<f',expected)
    return expected

def produce(profile,bits,mapping,ranges,encoded,selected):
    assert profile['SourceScoreExponent']==.75
    points=[[point(p) for p in s] for s in bits]
    assert all(0<=m<len(points) and (i==0 or m>mapping[i-1]) for i,m in enumerate(mapping))
    raw=base64.b64decode(encoded,validate=True);assert len(raw)%40==0 and 0<=selected<len(raw)//40
    alts=[raw[i:i+40] for i in range(0,len(raw),40)]
    label=lambda row:struct.unpack_from('<H',row,8)[0]
    choices=[alts[selected]];seen={label(alts[selected])}
    for row in alts:
        if len(choices)==20:break
        if label(row) not in seen:seen.add(label(row));choices.append(row)
    projected,members=project(points,mapping,ranges);assert members
    out=bytearray(720);struct.pack_into('<qi',out,8,len(projected),-1)
    struct.pack_into('<ffff',out,24,min(x for x,y in members),max(x for x,y in members),min(y for x,y in members),max(y for x,y in members))
    for i in range(20):
        if i>=len(choices):struct.pack_into('<H',out,40+i*2,65535);struct.pack_into('<i',out,240+i*4,0x4000000);continue
        row=choices[i];symbol=label(row);struct.pack_into('<H',out,40+i*2,symbol)
        probability=struct.unpack_from('<f',row,12)[0]
        if i==0:struct.pack_into('<f',out,80+i*8,pow75(bytes(row[12:16])))
        else:out[80+i*8:84+i*8]=row[12:16]
        out[84+i*8:88+i*8]=row[16:20];out[240+i*4:244+i*4]=row[20:24]
        if symbol==65535:continue
        assert 0<=symbol<len(profile['StartMap'])
        start=profile['StartMap'][symbol]
        if start==-1:continue
        proto=profile['Prototypes'][start];lower=f(-1-f(proto[0]*2));upper=f(f(proto[1]*2)+lower)
        struct.pack_into('<f',out,560+i*8,lower);struct.pack_into('<f',out,400+i*8,upper)
        channels=1 if profile['EndMap'][symbol]==-1 else 2;struct.pack_into('<i',out,320+i*4,channels)
        if channels==2:struct.pack_into('<f',out,564+i*8,lower);struct.pack_into('<f',out,404+i*8,upper)
    return bytes(out),projected,len(choices),len(members)

def overwrite_flat(profile):
    row=bytearray(40);struct.pack_into('<H',row,8,profile['UnderscoreLabel']);struct.pack_into('<ff',row,12,1.,1.);return base64.b64encode(row).decode()
