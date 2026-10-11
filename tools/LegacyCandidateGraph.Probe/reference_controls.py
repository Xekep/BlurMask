"""Original exhaustive stable-move/partition oracle, not native orchestration."""
import base64,itertools,json,random,struct,sys
words=[p for length in range(1,5) for p in itertools.product(range(1,4),repeat=length)]
prefixes=[()]+words;states={w:i for i,w in enumerate(prefixes)}
def packed(values,width):
    bits=sum(v<<(width*i) for i,v in enumerate(values))
    return struct.pack('<IIB',0x01000000,len(values),width)+bits.to_bytes(((len(values)*width+31)//32)*4,'little')
payload=struct.pack('<IIIII',0x2b4,0x01000000,0,len(prefixes),4)
payload+=packed([0]+[4|w[-1] for w in words],3)+bytes([8])+packed([3 if len(p)<4 else 0 for p in prefixes],2)+struct.pack('<I',len(words))
lexicon=struct.pack('<II',0x0101fe90,len(payload))+payload
def cost(group,label):return ((sum((i+1)**2 for i in group)+len(group)*7)*label*13)%127-40
def bias(state):return (state*31)%107-53
rng=random.Random(0x1a3b30);cases=[]
for _ in range(192):
    count=rng.randint(2,8); selected=rng.randrange(count-1); u=selected+1
    cap=rng.randint(1,5); total=count+1; words=set()
    for shift in range(min(2,u)+1):
        path=list(range(total));path.pop(u);path.insert(u-shift,u)
        def partition(offset,prefix):
            if offset==total:words.add('|'.join(prefix));return
            for size in range(1,min(cap,total-offset)+1):
                partition(offset+size,prefix+[','.join(map(str,sorted(path[offset:offset+size])))])
        partition(0,[])
    best={}
    for partition in words:
        groups=[list(map(int,g.split(','))) for g in partition.split('|')]
        if len(groups)>4:continue
        for labels in itertools.product(range(1,4),repeat=len(groups)):
            acoustic=sum(cost(group,label) for group,label in zip(groups,labels));total=acoustic+bias(states[labels])
            if total<=180 and (labels not in best or acoustic<best[labels][0]):best[labels]=(acoustic,total)
    expected=[dict(labels=list(labels),acoustic=value[0],total=value[1],state=states[labels]) for labels,value in sorted(best.items(),key=lambda pair:(pair[1][1],states[pair[0]]))]
    cases.append(dict(count=count,selected=selected,cap=cap,partitions=sorted(words),expected=expected))
with open(sys.argv[1],'w',encoding='utf-8') as stream:json.dump(dict(lexicon=base64.b64encode(lexicon).decode(),cases=cases),stream,separators=(',',':'))
print('Wrote',len(cases),'independent exhaustive candidate graph references')
