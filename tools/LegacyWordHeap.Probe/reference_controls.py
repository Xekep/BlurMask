"""Original functional path/permutation oracle plus mathematical best-K costs."""
import base64,json,random,struct,sys
rng=random.Random(0x1f75b0);cases=[]
def b64(value):return base64.b64encode(value).decode()
def cost(value):return struct.unpack_from('<i',value,0x10)[0]
for _ in range(256):
    capacity=rng.randrange(17);storage=[rng.randbytes(128) for _ in range(capacity)];initial=b''.join(storage)
    count=0;steps=[];all_costs=[]
    for index in range(48):
        value=rng.choice([rng.randrange(-80,81),rng.randrange(-80,81),0x4000000,2**31-1,-2**31])
        accepted=capacity!=0 and value<0x4000000 and (count<capacity or value<cost(storage[0]))
        evicted=None
        if value<0x4000000:all_costs.append(value)
        if accepted:
            old=storage[:]
            if count<capacity:
                def insertion_path(node):
                    if node==0:return [node]
                    parent=(node-1)//2
                    return [node]+insertion_path(parent) if value>cost(old[parent]) else [node]
                path=insertion_path(count);count+=1
                for child,parent in zip(path,path[1:]):storage[child]=old[parent]
                target=path[-1]
            else:
                evicted=b64(old[0])
                def reduction_path(node):
                    children=[child for child in (node*2+1,node*2+2) if child<count]
                    if not children:return [node]
                    child=max(children,key=lambda child:(cost(old[child]),child))
                    return [node]+reduction_path(child) if cost(old[child])>value else [node]
                path=reduction_path(0)
                for parent,child in zip(path,path[1:]):storage[parent]=old[child]
                target=path[-1]
            result=bytearray(120)+storage[target][120:]
            struct.pack_into('<i',result,0x10,value);struct.pack_into('<I',result,4,index)
            storage[target]=bytes(result)
        best=sorted(all_costs)[:capacity]
        assert sorted(cost(record) for record in storage[:count])==best
        steps.append(dict(cost=value,accepted=accepted,evicted=evicted,slots=[b64(record) for record in storage[:count]],best=best))
    cases.append(dict(capacity=capacity,initial=b64(initial),steps=steps))
tails=[]
for _ in range(256):
    slot=bytearray(rng.randbytes(128));node=rng.randrange(65);arc=rng.choice([65535,0,1,2])
    struct.pack_into('<i',slot,0x38,node);struct.pack_into('<H',slot,0x3c,arc)
    records={};calls=[];suppressed=bool(rng.randrange(3)==0);previous=-1;context=0
    def lookup(n,a):
        calls.append(f'{n}:{a}')
        return records.setdefault((n,a),rng.randbytes(88))
    if not suppressed:
        anchor=slot[:88] if arc==65535 else lookup(node,arc)
        span=struct.unpack_from('<b',anchor,0x20)[0];previous=node-span-1
        if previous>=0:
            previous_arc=struct.unpack_from('<H',anchor,0x1e)[0]
            context=struct.unpack_from('<Q',lookup(previous,previous_arc),0x28)[0]
    expected=slot[:];struct.pack_into('<Q',expected,0x58,2**64-1)
    struct.pack_into('<i',expected,0x68,previous);struct.pack_into('<Q',expected,0x70,context)
    tails.append(dict(slot=b64(slot),suppressed=suppressed,records={f'{n}:{a}':b64(v) for (n,a),v in records.items()},calls=calls,expected=b64(expected)))
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump(dict(heaps=cases,tails=tails),stream,separators=(',',':'))
print('Wrote',len(cases),'independent native heap streams/',sum(len(c['steps']) for c in cases),'allocations')
