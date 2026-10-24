from pathlib import Path
import ast,types,json,hashlib,random,sys
HERE=Path(__file__).resolve().parent
ref=HERE/'reference_range_operations.py';raw=ref.read_bytes();assert hashlib.sha256(raw).hexdigest()=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
g={'MASK':2**64-1};tree=ast.parse(raw.decode('utf-8-sig'));exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),str(ref),'exec'),g)
root=[[3,0x3ff05]];cases=[]
def add(id,ranges,join=None,pos=None):cases.append(dict(Id=id,Counts=[4],Original=root,Ranges=ranges,Positive=pos or [True]*len(ranges),Joiner=join or [False]*len(ranges)))
add('equal keys',[root,root]);add('partial overlap',[[[3,0x20005]],[[0x10003,0x3ff05]]]);add('joiner',[root],[True]);add('inside point',[[[0x10003,0x10005]]]);add('outside root prepared only',[[[0x40003,0x40005]]]);add('negative prepared only',[root],pos=[False]);add('empty key',[[]]);add('two empty keys reuse',[[],[]]);add('nonoverlap',[[[3,5]],[[0x20003,0x20005]]]);add('AVL order',[[[i<<16|3,i<<16|5]]for i in [2,0,3,1,0]])
rng=random.Random(0x24d40);flags=[]
for i in range(128):flags.append(dict(Ordinary=bool(i&1),Prior=rng.randrange(256),Aggregate=rng.randrange(2**32),Key=rng.choice([root,[],[[3,5]],[[0x10003,0x1ff05]],[[0x10003,0x20005]]])))
Path(sys.argv[1]).write_bytes(json.dumps(dict(Cases=cases,Flags=flags),separators=(',',':')).encode())
