"""Independent mixed20E6A0 heap dispatch/full-pool oracle.

Root/primary/reset stores are original independent field interpreters; general
geometry reuses the existing independently scripted byte interpreter. All
higher predecessor policies/callbacks are explicitly prepared, not observed
fresh inference. Two general rows separated by other branches test real local
request scratch carry and noncontiguous pool ordinals.
"""
import ast
import json
import random
import struct
import sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
old=ROOT/'tools/LegacyWordHigherArc.Probe/reference_controls.py'
source=old.read_text().split('cases=[generate(index)')[0]
source=source.replace('def generate(index):','def generate(index, residue_override=None):')
source=source.replace('residue = bytearray(RNG.randbytes(80))','residue = bytearray(RNG.randbytes(80) if residue_override is None else residue_override)')
scope={'__file__':str(old)}
exec(compile(source,str(old),'exec'),scope)
generate=scope['generate'];rng=scope['RNG'];compose=scope['compose']
randomizer=random.Random(0x2144f0)
IMP=0x4000000
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
wrap=lambda x:(x+0x80000000)%0x100000000-0x80000000
s8=lambda x:x-256 if x>=128 else x

def w16(b,o,x):struct.pack_into('<H',b,o,x&65535)
def w32(b,o,x):struct.pack_into('<I',b,o,x&0xffffffff)
def w64(b,o,x):struct.pack_into('<Q',b,o,x)

cases=[];index=0
while len(cases)<256:
    at=index;index+=1
    if at%4==3 or at%19==0:continue
    before=rng.getstate();g=generate(at);after=rng.getstate()
    rng.setstate(before);g2=generate(at,bytes.fromhex(g['expectedRequest']));rng.setstate(after)
    assert g['paths']==g2['paths'] and g['records']==g2['records'] and g['scripts']==g2['scripts']
    pc,sc,node=g['pc'],g['sc'],g['node']
    if node>127:continue
    paths=g['paths']
    def ident(path):
        path=list(path)
        if path not in paths:paths.append(path)
        return paths.index(path)
    def joined(a,z,mapped):return ident(compose(paths[a],paths[z],mapped)[0])
    records=[bytearray.fromhex(x)[:88]for x in g['records']]
    #The primary-parent source08 owns a real symbolic node; this is unrelated
    #to a test string and does not change general geometry's category/flags.
    parent_token=[22,0xfff5,0xfffb,0xfff9,0xfffa][len(cases)%5]
    w64(records[0],8,ident([parent_token]))
    parent_primary=[]
    for a in range(len(records)):
        data=bytearray(randomizer.randbytes(pc*40))
        for k in range(pc):
            w32(data,k*40,randomizer.choice([-2147483648,-100,0,100,IMP-1,IMP]))
            w32(data,k*40+4,randomizer.randrange(-1000,1000))
            w64(data,k*40+8,ident([100+a,k]));w64(data,k*40+16,ident([200+a,k]))
        parent_primary.append(data)
    secondary_parents=[bytearray.fromhex(x)for x in g['pools']]
    slots=[];ordinals=[3,1,0,4,2];routes=[0,3,1,2,3]
    for n in range(5):
        slot=bytearray.fromhex(g['slot']);w32(slot,88,ordinals[n]);slots.append(slot)
    slots[0][32]=node;w16(slots[0],30,65535)
    w16(slots[2],30,0);w16(slots[2],24,node if len(cases)%2 else node+1)
    w16(slots[3],30,g['limit']);slots[3][4]=7;w16(slots[3],36,1)
    headers=g['headers'][:];headers[0]&=~28;headers[7]=(headers[7]&~28)|8
    reset_source=7 if len(cases)%2 else 0xffffffff
    primary=bytearray(randomizer.randbytes(6*pc*40));secondary=bytearray(randomizer.randbytes(6*sc*40))
    for pool in [primary,secondary]:
        for at40 in range(0,len(pool),40):w64(pool,at40+8,0);w64(pool,at40+16,0)
    for n in [1,4]:
        ordinal=ordinals[n]
        primary[ordinal*pc*40:(ordinal+1)*pc*40]=bytes.fromhex(g['primary'])[:pc*40]
        secondary[ordinal*sc*40:(ordinal+1)*sc*40]=bytes.fromhex(g['secondary'])[:sc*40]
    inactive=randomizer.randbytes(128)
    inp=dict(g,slots=(b''.join(slots)+inactive).hex(),active=5,primaryFull=primary.hex(),secondaryFull=secondary.hex(),
             records88=[x.hex()for x in records],primaryParents=[x.hex()for x in parent_primary],headers=headers,
             resetSource=reset_source,routes=routes,ordinals=ordinals,expectedCallbacks=g['callbacks']+g2['callbacks'])
    expected_heap=bytearray(b''.join(slots)+inactive)
    expected_p=bytearray(primary);expected_s=bytearray(secondary)
    for n,route in enumerate(routes):
        slot=slots[n];ordinal=ordinals[n];suffix=u64(slot,8)
        if route==3:
            general=g if n==1 else g2
            row=bytearray.fromhex(general['expectedSlot']);w32(row,88,ordinal)
            expected_heap[n*128:(n+1)*128]=row
            if pc>1:expected_p[ordinal*pc*40:(ordinal+1)*pc*40]=bytes.fromhex(general['expectedPrimary'])[:pc*40]
            expected_s[ordinal*sc*40:(ordinal+1)*sc*40]=bytes.fromhex(general['expectedSecondary'])[:sc*40]
            continue
        arc=u16(slot,30)
        parent=records[arc]if route else None
        reset=route==1 and parent_token in [0xfff5,0xfff9,0xfffa,0xfffb]
        append=route==1 and (u16(slot,24)==node or s8(slot[5])>=0)
        for count,dst,primary_kind in [(pc,expected_p,True),(sc,expected_s,False)]:
            if count==1:continue
            start=ordinal*count*40
            w16(dst,start+24,arc);dst[start+26]=slot[32]
            dst[start+31]=(dst[start+31]&(192 if primary_kind else 224))|(slot[33]&31)
            if route:dst[start+27]&=128
            if not primary_kind or route==0:dst[start:start+8]=slot[16:24];dst[start+30]=slot[38]
            if route==0:
                w64(dst,start+8,joined(0,suffix,True));w64(dst,start+16,joined(0,suffix,False))
                if primary_kind:dst[start+31]|=32
                for k in range(1,count):w32(dst,start+k*40,IMP);w32(dst,start+k*40+4,IMP);dst[start+k*40+30]=0
                continue
            delta=wrap(i32(slot,16)-i32(parent,16));increment=wrap(i32(slot,20)-i32(parent,20))
            source=parent_primary[arc]if primary_kind else secondary_parents[arc]
            if route==2 or primary_kind and reset:
                if primary_kind:
                    w32(dst,start,delta);w32(dst,start+4,increment)
                    w64(dst,start+8,joined(0,suffix,True));w64(dst,start+16,joined(0,suffix,False))
                    dst[start+30]=slot[38]
                    bit=((slot[5]&1)<<5)if route==2 and slot[4]==reset_source else 32
                    dst[start+31]=(dst[start+31]&~32)|bit
                else:
                    w64(dst,start+8,joined(u64(source,8),suffix,False));w64(dst,start+16,joined(u64(source,16),suffix,False))
                for k in range(1,count):w32(dst,start+k*40,IMP);w32(dst,start+k*40+4,IMP);dst[start+k*40+30]=0
                continue
            for k in range(count):
                off=start+k*40;source_off=k*40
                if not primary_kind and k==0:
                    mapped,raw=u64(source,8),u64(source,16)
                    if reset or append:mapped=joined(mapped,suffix,not reset);raw=joined(raw,suffix,False)
                elif i32(source,source_off)<IMP:
                    w32(dst,off,i32(source,source_off)+delta);w32(dst,off+4,i32(source,source_off+4)+increment)
                    w16(dst,off+24,arc);dst[off+26]=slot[32];dst[off+27]=(dst[off+27]&128)|k
                    dst[off+30]=slot[38]if primary_kind else 0;dst[off+31]=(dst[off+31]&(192 if primary_kind else 224))|(slot[33]&31)
                    mapped,raw=u64(source,source_off+8),u64(source,source_off+16)
                    if append:mapped=joined(mapped,suffix,True);raw=joined(raw,suffix,False)
                else:w32(dst,off,IMP);w32(dst,off+4,IMP);dst[off+30]=0;continue
                w64(dst,off+8,mapped);w64(dst,off+16,raw)
    inp.update(paths=paths,expectedHeap=expected_heap.hex(),expectedP=expected_p.hex(),expectedS=expected_s.hex(),
               secondAccepted=g2['accepted'],secondFiltered=g2['filtered'],secondMerged=g2['merged'])
    cases.append(inp)
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases),separators=(',',':'))+'\n')
print(f'generated{len(cases)} mixed five-slot/two-general streams with full inactive storage and local request carry')
