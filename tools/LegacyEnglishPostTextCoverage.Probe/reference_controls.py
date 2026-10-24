import ast,copy,hashlib,json,sys
from pathlib import Path
D=Path(__file__).resolve().parent
f=D/'reference_ranges.py';assert hashlib.sha256(f.read_bytes()).hexdigest()=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
g={'MASK':2**64-1};tree=ast.parse(f.read_text(encoding='utf-8-sig'));exec(compile(ast.Module([n for n in tree.body if isinstance(n,ast.FunctionDef)],[]),str(f),'exec'),g)
cases=[]
for n in range(2,34):
 for cut in range(n-1):
  key=[[3,((n-1)<<16)|5]]
  chars=[[[3,(cut<<16)|5]],[[(cut<<16)|3,((n-1)<<16)|5]]]
  remaining=copy.deepcopy(key)
  for c in chars:remaining=g['subtract'](remaining,c,[n])
  assert remaining==[]
  partial=g['subtract'](key,chars[0],[n])
  cases.append(dict(Counts=[n],Original=key,Characters=chars,Union=g['union']([],key),Remaining=remaining,Partial=partial,
   MultipleChoiceRemovalRequired=bool(partial),SingleChoiceKept=True))
Path(sys.argv[1]).write_text(json.dumps(dict(Scope='PREPARED_C21_ONLY_NOT_EN_OWNER',Cases=cases),separators=(',',':'))+'\n',encoding='utf-8')
print('PASS prepared generator',len(cases))
