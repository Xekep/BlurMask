"""Read-only inventory of original class-24a v102 scalar geometry nodes; executes no DLL."""
import hashlib,json,math,struct,sys
from pathlib import Path
rows=[]
if len(sys.argv)<2:raise SystemExit('Supply one or more original acoustic .res files.')
for name in sys.argv[1:]:
    path=Path(name)
    if path.stat().st_size>32*1024*1024:raise ValueError('Resource exceeds 32 MiB.')
    data=path.read_bytes();sha=hashlib.sha256(data).hexdigest();cursor=0
    header=struct.pack('<IIIHH',0x0101fe90,28,0x24a,0,0x102)
    while True:
        offset=data.find(header,cursor)
        if offset<0:break
        if offset+36>len(data):raise ValueError('Truncated class-24a node.')
        first,second,endpoints,lo,hi,sparse,dense,reserved=struct.unpack_from('<ifBffBBB',data,offset+16)
        if not all(math.isfinite(x) for x in (second,lo,hi)) or endpoints>1 or sparse>dense:
            raise ValueError('Invalid class-24a scalar fields.')
        rows.append(dict(resource=str(path),sha256=sha,node_offset=offset,payload_offset=offset+16,
            type_id='0x24a',version='0x102',first_i32=first,second_float=second,include_endpoints=bool(endpoints),
            min_slope=lo,max_slope=hi,sparse_threshold=sparse,dense_threshold=dense,reserved=reserved))
        cursor=offset+36
print(json.dumps(rows,ensure_ascii=True,indent=2))
