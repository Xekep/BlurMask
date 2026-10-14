"""Independent packed-row pointer walk, preserving original acoustic parent."""
import json,struct,sys
walks=0;skipped=0;rootstops=0
for d in json.load(open(sys.argv[1],encoding='utf-8')):
    rows=[bytes.fromhex(r) for r in d['rows']]
    for original,alternative,node,ordinal,count,originalcost,ancost in d['walks']:
        n=original;a=alternative;c=0
        assert struct.unpack_from('<i',rows[n],a*40)[0]==originalcost
        while True:
            row=rows[n];p=a*40;symbol=struct.unpack_from('<H',row,p+12)[0]
            span=(struct.unpack_from('<H',row,p+14)[0]>>1)&7;parent=n-span-1
            if symbol not in (192,180,181,182) or parent==-1:break
            a=row[p+9];n=parent;c+=1
        assert (n,a,c)==(node,ordinal,count)
        assert struct.unpack_from('<i',rows[n],a*40)[0]==ancost
        walks+=1;skipped+=c;rootstops+=n==0 and symbol in (192,180,181,182)
assert skipped and rootstops
print(f'PASS independent first ancestor packed-row audit: {walks} walks/{skipped} skipped/{rootstops} rootstops, original costs preserved.')
