"""Independent request64 and pending-first protocol216BC0 oracle.
Actual72 provenance is separated from explicitly prepared rows/packet scalars.
"""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture')
owners=json.loads(Path(sys.argv[1]).read_bytes());assert len(owners)==8
u16=lambda b,p:struct.unpack_from('<H',b,p)[0]
i32=lambda b,p:struct.unpack_from('<i',b,p)[0]
u64=lambda b,p:struct.unpack_from('<Q',b,p)[0]
actual=prepared=writes=cases=underflow=0
for owner,p in enumerate(owners):
    assert p['owner']==owner and p['headers']==[0,0xc0a]
    compact=base64.b64decode(p['compact']);actual_rows=base64.b64decode(p['actualRows72']);assert len(actual_rows)%72==0
    for c in p['cases']:
        r=base64.b64decode(c['row72']);q=bytearray((i*17+23)&255 for i in range(64));assert len(r)==72 and r[60]==3
        if c['actualRow']:assert r in [actual_rows[i:i+72]for i in range(0,len(actual_rows),72)];actual+=1
        else:prepared+=1
        def row(index):assert 0<=index<len(compact)//40;return compact[index*40:index*40+40]
        def w32(at,value):struct.pack_into('<I',q,at,value&0xffffffff)
        def w16(at,value):struct.pack_into('<H',q,at,value&65535)
        neighbor=i32(r,32);previous=-1;previous_arc=65535;source_class=0
        if neighbor!=-1:
            adjacent=row(neighbor);source=adjacent[4];assert source<len(p['headers']);source_class=(p['headers'][source]>>9)&15
            if u16(adjacent,28)==140:
                index=i32(adjacent,16)
                if index!=-1:
                    predecessor=row(index)
                    if(u16(predecessor,28)+7)&65535<4:
                        index=i32(predecessor,16)
                        if index!=-1:predecessor=row(index)
                    if index!=-1:
                        key=u64(predecessor,8);previous=key&0xffffff;previous=previous-0x1000000 if previous&0x800000 else previous;previous_arc=(key>>24)&65535
        w32(0,previous);w16(4,previous_arc);w32(8,i32(r,24));w16(12,u16(r,28));w32(16,i32(r,12));w16(20,u16(r,16));w16(22,65535 if neighbor==-1 else u16(row(neighbor),28))
        current=i32(r,0)+(u16(r,56)==65535);arc=u16(r,4);w32(24,current);w16(28,arc);w16(30,u16(r,56));w32(32,i32(r,36));w16(36,u16(r,40))
        kind={0xfffc:0,0xfffb:1,0xfffa:2}[u16(r,58)];w32(40,kind);w32(44,c['ac']);w32(48,c['b0']);w32(52,source_class);w32(56,0);q[60]=1
        assert c['succeeded'] and base64.b64decode(c['request64'])==q
        cursor=c['cursor'];symbols=[];remaining=cursor
        metadata=[]
        if current!=p['nodeCount']:
            flags,context=p['graph'][current][arc]
            if context!=65535:metadata=p['contexts'][context];assert all(v==-7 for v in metadata)
        if cursor:
            if metadata:symbols=[65529]*min(cursor,len(metadata));remaining=cursor-len(metadata)
            elif i32(q,8)==-1 or current==p['nodeCount']:pass
            elif kind==0:symbols=[65532];remaining-=1
            elif kind==2:
                if flags&8:symbols=[65531];remaining-=1
            elif kind==1:
                assert c['ac']==2;symbols=[65531];remaining-=1
        expected=[0x8000+i for i in range(256)]
        for ordinal,symbol in enumerate(symbols):expected[cursor-ordinal-1]=symbol
        assert c['labels']==expected
        assert(c['remaining'],c['component'],c['external'],c['score0C'],c['score10'],c['cost14'],c['flags06'])==(remaining,0,0,0x3f800000,0x3f800000,0,0)
        writes+=len(symbols);cases+=1
    if owner==0:
        assert len(p['preparedPendingCursorControls'])==24
        for cursor,count,remaining,written,refused,observed in p['preparedPendingCursorControls']:
            target=cursor-count if cursor else 0;expected_writes=min(cursor,count)
            assert(remaining,written,refused)==(target,expected_writes,target<0)
            expected=[0x8000+i for i in range(256)]
            for ordinal in range(expected_writes):expected[cursor-ordinal-1]=65529
            assert observed==expected;underflow+=target<0
assert(actual,prepared,cases)==(32,7572,7604) and underflow>0
print(f'PASS independent {actual} actual-backtrack-row controls/{prepared} explicit prepared protocol rows/{writes} native source writes;24 separate prepared pending controls/{underflow} safe negative finalization refusals. Packet scalars prepared.')
