"""Own serialized profiles and independently indexed two-dimensional categories."""
import base64,json,random,struct,sys
def make(rng,offset):
    base=rng.randint(1,192);n=base+3;ids=[0]+rng.sample(range(1,60),rng.randint(0,6));geometry_ids=rng.sample(range(50),rng.randint(1,12))
    first=[rng.randrange(-1,len(ids)) for _ in range(n)];start=[rng.randrange(-1,len(geometry_ids)) for _ in range(n)];end=[rng.randrange(-1,len(geometry_ids)) for _ in range(n)]
    prototypes=[[rng.randint(-8,8)/4,rng.randint(-8,8)/4,rng.randint(-8,8)/4,rng.randrange(2)] for _ in geometry_ids]
    data=bytearray(struct.pack('<HH',0x201,base))
    def write_ids(values):
        data.append(len(values))
        while (offset+len(data))%4:data.append(255)
        data.extend(struct.pack('<'+'i'*len(values),*values))
    write_ids(ids);data.extend(v&255 for v in first);write_ids(geometry_ids)
    data.extend(v&255 for v in start);data.extend(v&255 for v in end)
    for row in prototypes:data.extend(struct.pack('<fffB',*row))
    consumed=len(data);data.extend(b'following-sampler-config')
    table=[[-1 for _ in geometry_ids] for _ in ids];categories=[];maps=[[] for _ in range(4)]
    for i in range(n):
        for dest,(a,b) in zip(maps,[(first[i],start[i]),(first[i],end[i]),(0,start[i]),(0,end[i])]):
            if a==-1 or b==-1:dest.append(-1);continue
            if table[a][b]==-1:
                table[a][b]=len(categories);categories.append([ids[a],b])
            dest.append(table[a][b])
    return dict(bytes=base64.b64encode(data).decode(),offset=offset,consumed=consumed,ids=ids,geometry_ids=geometry_ids,maps=maps,categories=categories,prototypes=prototypes)
rng=random.Random(0x297010d80)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng,i%4) for i in range(256)],stream,separators=(',',':'))
print('Wrote256 independent category-profile controls')
