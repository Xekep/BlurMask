"""Independent UINT32 count oracle with raw actual24D/29B profile fields."""
import hashlib,json,pathlib,struct,sys
import numpy as np
raw=pathlib.Path(sys.argv[1]).read_bytes();data=json.loads(pathlib.Path(sys.argv[2]).read_text(encoding='utf-8-sig'))
assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<I',raw,188012+8)[0]==0x24d and struct.unpack_from('<I',raw,188050+8)[0]==0x29b
lo,hi=raw[188012+16:188012+18];glo,ghi=raw[188050+40:188050+42];zero_bits=struct.unpack_from('<i',raw,188050+42)[0]
assert data['profile']==[lo,hi,glo,ghi,zero_bits]
def blend_bits(value,minimum,maximum):
    #Native zero-extends the UINT32 register to64bits before CVTSI2SS.
    x=np.float32(np.float32(np.float32(value)-np.float32(minimum))/np.float32(maximum-minimum))
    return struct.unpack('<i',struct.pack('<f',min(1,max(0,float(x)))))[0]
def check(item):
    counts=item['counts'] if item.get('kind',0) in (0,6) and item.get('line',0)>=0 else []
    total=sum(n-1 for n in counts)&0xffffffff
    qualified=sum((n-1)&0xffffffff for n in counts if ((n-1)&0xffffffff)>glo)&0xffffffff
    produced=sum(max(n-1,0) for n in counts)
    produced_qualified=sum(max(n-1,0) for n in counts if ((n-1)&0xffffffff)>glo)
    branch=0 if total==0 else 2 if max(total,qualified)>4096 else 3 if (total,qualified)!=(produced,produced_qualified) else 1
    expected=dict(shearCount=total,total=total,qualified=qualified,produced=produced,producedQualified=produced_qualified,
        empty=counts.count(0),shearBlendBits=blend_bits(total,lo,hi),gapBlendBits=blend_bits(qualified,glo,ghi),branch=branch)
    for key,value in expected.items():assert item[key]==value,(key,item[key],value,item['counts'])
    if branch==0:assert item['zeroGapBits']==zero_bits
    else:assert 'zeroGapBits' not in item
    return branch
distribution=[0]*4
for item in data['prepared']:
    branch=check(item)
    if 'kind' not in item:distribution[branch]+=1
for item in data['actual']:
    assert all(n>0 for n in item['counts']);assert check(item) in (0,1)
assert len(data['actual'])==7 and len(data['prepared'])==data['preparedCount']==7386
assert distribution==[data['zero'],data['bounded'],data['oversized'],data['underCapacity']]
assert data['guards']==distribution[2]+distribution[3]+19
#Explicit witnesses prevent a future "skip empty" or signed-count substitution.
find=lambda ns:next(x for x in data['prepared'] if x['counts']==ns and 'kind' not in x)
assert find([0])['total']==0xffffffff and find([0])['shearBlendBits']==0x3f800000 and find([0])['branch']==2
assert find([0,2])['total']==0 and find([0,2])['qualified']==0xffffffff and find([0,2])['branch']==0
assert find([0,12])['total']==10 and find([0,12])['produced']==11 and find([0,12])['branch']==3
print('PASS independent7386 UINT32 descriptor states, raw24D/29B thresholds, zero-before-allocation/oversize/undercapacity,7 actual owning inputs')
