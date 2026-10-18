"""Prepared numerical requests only; not outputs of native119 scheduling."""
import json,random,sys
from pathlib import Path
r=random.Random(0x1191ee3b0)
rows=[dict(rootHeader=i%4==0,blank1=i%4 in(1,2),blank2=i%4==2,costs=[r.randrange(0,5000)for _ in range(4)])for i in range(512)]
Path(sys.argv[1]).write_text(json.dumps(rows)+'\n',encoding='utf-8')
