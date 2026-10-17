"""Independent line-aware1941B0 folds and integer caches from owned raw inputs."""
import ast,hashlib,json,struct,sys
from pathlib import Path
if len(sys.argv)!=4:raise ValueError('AK separation-profile capture')
root=Path(__file__).resolve().parents[2];ns={}
tree=ast.parse((root/'tools/LegacyMainWordTransition.Probe/reference_controls.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef))],type_ignores=[]),'independent-cache-numerics','exec'),ns)
f=ns['f'];logcost=ns['logcost'];ak=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
at=189165;scalar=lambda offset:struct.unpack_from('<f',ak,at+offset)[0]
default,threshold,slope,gapScale,matrixScale=[scalar(o)for o in [29,33,185,190,194]]
matrix=[scalar(41+i*4)for i in range(36)];joinCap,matrixCap=struct.unpack_from('<ii',ak,at+206)
table=[f(v)for v in json.loads(Path(sys.argv[2]).read_text())['values']];cases=json.loads(Path(sys.argv[3]).read_text());arcs=discriminators=0
for case in cases:
    groups=case['groups'];points=[[[f(v)for v in p]for p in s]for s in case['points']];nodes=case['nodes']
    for group in groups:
        raw=[p for s,a,z in group['ranges']for p in points[s][a:z]]
        box=[min(p[0]for p in raw),min(p[1]for p in raw),max(p[0]for p in raw),max(p[1]for p in raw)]
        assert list(map(f,group['bounds']))==box
    def fold(filterLines):
        future=[[None]*len(row)for row in nodes];prefix=[[None]*len(row)for row in nodes];out=[]
        for n in range(len(nodes)-1,-1,-1):
            for a,arc in enumerate(nodes[n]):
                group=groups[arc['group']];low=f(group['bounds'][0])
                if n!=arc['span'] and n+1<len(nodes):
                    selected=[future[n+1][child]for child in arc['next']if not filterLines or groups[nodes[n+1][child]['group']]['line']==group['line']]
                    if selected:low=min(low,*selected)
                future[n][a]=low
        sourceGap=f(case['sourceGap']);gapDefault=f(sourceGap*gapScale)if sourceGap>=0 else default
        ms=1 if sourceGap<0 or matrix[0]==0 else f(f(sourceGap*matrixScale)/matrix[0])
        for n,row in enumerate(nodes):
            output=[]
            for a,arc in enumerate(row):
                group=groups[arc['group']];box=list(map(f,group['bounds']));height=f(group['height']);costs=[0x4000000]*3;m=[0x4000000]*36
                if n==arc['span']:high=box[2];prefix[n][a]=high
                else:
                    previous=n-arc['span']-1
                    selected=[prefix[previous][p]for p in arc['previous']if not filterLines or groups[nodes[previous][p]['group']]['line']==group['line']]
                    high=max(selected)if selected else box[2];prefix[n][a]=max(high,box[2]);gap=f(f(future[n][a]-high)/height)
                    costs[0]=min(joinCap,logcost(f(f(gap-gapDefault)*f(-slope)),table,True))
                    costs[1]=min(joinCap,logcost(f(f(gap-f(gapDefault*3))*f(-slope)),table,True))
                    blank=f(f(f(f(box[0]-high)/height)-threshold)*f(-slope))
                    costs[2]=min(0x4000000,logcost(blank,table,True))
                    for i in range(36):
                        v=logcost(f(f(gap-f(ms*matrix[i]))*slope),table,False)
                        m[i]=v-1 if i//6==5 else v if i%6==5 else min(v,matrixCap)
                output.append(([future[n][a],prefix[n][a]],costs,m))
            out.append(output)
        return out
    expected=fold(True);unfiltered=fold(False)
    for n,row in enumerate(nodes):
        for a,arc in enumerate(row):
            want=expected[n][a];assert(list(map(f,arc['fold'])),arc['costs'],arc['matrix'])==want,(n,a)
            discriminators+=want!=unfiltered[n][a];arcs+=1
assert len(cases)==8 and arcs==631 and discriminators>0
print(f'PASS independent8 raw global1941B0 owners/{arcs} full cache rows;{discriminators} genuine line-filter discriminators; primary heights/ordered selectors/36 matrix channels')
