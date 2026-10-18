"""Independent inclusive byte-pointer01ED50 ordering, preserving all16 bytes.

Prepared raw float words include unordered values. No DLL or model is executed.
"""
from pathlib import Path
import base64,json,math,struct,sys

if len(sys.argv) not in (2,3):raise SystemExit('capture.json [same-platform-other.json]')
data=json.loads(Path(sys.argv[1]).read_bytes())

def native_order(raw):
    memory=bytearray(raw)
    def mean(pointer):return struct.unpack_from('<f',memory,pointer+4)[0]
    def sort(begin,end):
        if (end-begin)//16 < 20:
            pointer=begin+16
            while pointer<=end:
                saved=memory[pointer:pointer+16];key=struct.unpack_from('<f',saved,4)[0]
                prior=pointer-16
                while prior>=begin and mean(prior)>key:
                    memory[prior+16:prior+32]=memory[prior:prior+16];prior-=16
                memory[prior+16:prior+32]=saved;pointer+=16
            return
        pivot=mean(begin+(((end-begin)//16)//2)*16)
        left,right=begin,end
        while True:
            while pivot>mean(left):left+=16
            while mean(right)>pivot:right-=16
            if left>right:break
            first=memory[left:left+16];second=memory[right:right+16]
            memory[left:left+16]=second;memory[right:right+16]=first
            left+=16;right-=16
            if left>right:break
        if begin<right:sort(begin,right)
        if left<end:sort(left,end)
    if memory:sort(0,len(memory)-16)
    return bytes(memory)

different=unordered=words=0
assert len(data['Cases'])==4224 and len(data['Sources'])==257 and data['Guards']==4
for case in data['Cases']:
    before=base64.b64decode(case['Before']);after=base64.b64decode(case['After'])
    assert len(before)%16==0 and len(before)<=4096
    expected=native_order(before)
    assert after==expected,(case['Family'],len(before)//16)
    records=[before[i:i+16]for i in range(0,len(before),16)];words+=len(records)
    assert sorted(records)==sorted(after[i:i+16]for i in range(0,len(after),16))
    means=[struct.unpack_from('<f',record,4)[0]for record in records]
    if all(math.isfinite(x)for x in means):
        stable=b''.join(sorted(records,key=lambda record:struct.unpack_from('<f',record,4)[0]))
        different+=stable!=after
    else:unordered+=1
    if case['Family']=='threshold:20:0':assert before==after
    if case['Family']=='threshold:21:0':assert after==b''.join(reversed(records))
    if case['Family']=='threshold:42:0':assert [struct.unpack_from('<I',after,i)[0]for i in range(0,len(after),16)]==list(range(21,42))+list(range(21))
for count,source in enumerate(data['Sources']):
    bits=source['MeanBits'];assert len(bits)==count
    raw=b''.join(struct.pack('<IIQ',i,word,0)for i,word in enumerate(bits))
    expected=native_order(raw)
    assert source['Order']==[struct.unpack_from('<I',expected,i)[0]for i in range(0,len(expected),16)]
assert different>0 and unordered>0
assert [f['Count']for f in data['Features']]==[2,19,20,21,22,41,42,43,256]
f32=lambda value:struct.unpack('<f',struct.pack('<f',value))[0]
bits=lambda value:struct.unpack('<I',struct.pack('<f',value))[0]
for feature in data['Features']:
    count=feature['Count'];scale=f32(3*(count-1));center=f32(f32(sum(i*3 for i in range(count)))/count)
    raw=b''.join(struct.pack('<IIQ',i,0,0)for i in range(count));ordered=native_order(raw)
    order=[struct.unpack_from('<I',ordered,i)[0]for i in range(0,len(ordered),16)]
    assert feature['ScaleBits']==bits(scale)and feature['Frames']==count
    assert feature['StrokeEnds']==list(range(1,count+1))and len(feature['Values'])==count*26
    for frame,original in enumerate(order):
        assert feature['Values'][frame*26:frame*26+4]==[bits(297),bits(0),bits(f32(f32(original*3-center)/scale)),bits(1)]
if len(sys.argv)==3:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[2]).read_bytes()
print(f'PASS4224 independent16B byte-pointer controls/{words} records/{unordered} nonfinite controls/{different} stable-sort counterexamples/257 source projections/9 actual feature inputs/4 guards')
