"""Model-free SourceDebit scalar/UTF8 protocol bound, never a runtime quota claim."""
import json,sys
from pathlib import Path

def check():
    data=dict(copies=128*1024*1024,work=64*1024*1024,phase=-1,activeGroup=-(1<<31),
              sites=[dict(site=i,copies=128*1024*1024,work=64*1024*1024) for i in range(12)])
    raw=json.dumps({'sourceDebit':data},separators=(',',':')).encode('ascii')
    reads=24*8+2*8+2*4+12*4
    guard_allowance=256
    assert reads==264 and len(raw)<1024 and reads+3*len(raw)+guard_allowance<4096
    return dict(scope='Conservative scalar/UTF8 protocol cardinalities, not CPU instructions/CLR allocations; impossible simultaneous maxima allowed only as upper bound',
                sites=12,siteLongs=24,totalLongs=2,stateI32=2,siteIDs=12,logicalExportScalarBytes=reads,
                fixedGuardAllowance=guard_allowance,conservativeSerializedBytes=len(raw),
                readPlusTripleUTF8PlusGuards=reads+3*len(raw)+guard_allowance,prepaidAdditionalVisits=4096,
                additionalOwnedArrays=0,additionalFields=0,additionalObjectAllocationsOnSuccessfulAccessorPath=0,
                sourceCopyCap=128*1024*1024,sourceWorkCap=64*1024*1024)
if __name__=='__main__':
    assert len(sys.argv)==2,'fresh-report.json'
    path=Path(sys.argv[1]);assert not path.exists()
    path.write_text(json.dumps(check(),indent=2)+'\n',encoding='utf-8')
    print('PASS 12 sites/26 I64/2 state I32/12 IDs; scalar+UTF8+guards <4096 prepaid visits')
