"""Independent iterative balance/path AVL oracle for source-key interning."""
import argparse,itertools,json,random
from pathlib import Path

def simulate(keys):
    nodes=[None];root=0;answers=[];keymap={}
    def rotate(identity):
        x=nodes[identity]
        if x['balance']<0:
            left=x['left'];l=nodes[left]
            if l['balance']<=0:
                x['left']=l['right'];l['right']=identity
                x['balance']=-1 if l['balance']==0 else 0;l['balance']=1 if l['balance']==0 else 0
                return left
            middle=l['right'];m=nodes[middle];l['right']=m['left'];x['left']=m['right'];m['left']=left;m['right']=identity
            x['balance']=1 if m['balance']<0 else 0;l['balance']=-1 if m['balance']>0 else 0;m['balance']=0
            return middle
        right=x['right'];r=nodes[right]
        if r['balance']>=0:
            x['right']=r['left'];r['left']=identity
            x['balance']=1 if r['balance']==0 else 0;r['balance']=-1 if r['balance']==0 else 0
            return right
        middle=r['left'];m=nodes[middle];x['right']=m['left'];r['left']=m['right'];m['left']=identity;m['right']=right
        x['balance']=-1 if m['balance']>0 else 0;r['balance']=1 if m['balance']<0 else 0;m['balance']=0
        return middle
    for source in keys:
        key=tuple(tuple(pair)for pair in source)
        if key in keymap:answers.append([keymap[key],False]);continue
        path=[];current=root
        while current:
            direction='left' if key<nodes[current]['key'] else 'right';path.append((current,direction));current=nodes[current][direction]
        ident=len(nodes);keymap[key]=ident;nodes.append(dict(key=key,left=0,right=0,balance=0))
        if not path:root=ident
        else:nodes[path[-1][0]][path[-1][1]]=ident
        for at in range(len(path)-1,-1,-1):
            parent,direction=path[at];n=nodes[parent];n['balance']+=-1 if direction=='left' else 1
            if n['balance']==0:break
            if abs(n['balance'])==2:
                replacement=rotate(parent)
                if at:upper,side=path[at-1];nodes[upper][side]=replacement
                else:root=replacement
                break
        answers.append([ident,True])
    tree=[[i,n['left'],n['right'],n['balance'],i-1,i+1 if i+1<len(nodes)else 0]for i,n in enumerate(nodes)if i]
    return dict(root=root,answers=answers,tree=tree)

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args();rng=random.Random(0x1b4e0)
    streams=[];values=[[],[(0,0)],[(0,2**64-1)],[(2**63,2**63)],[(2**64-1,0)],[(2**64-1,2**64-1)]]
    for order in itertools.permutations(range(6)):
        keys=[values[i]for i in order];streams.append(dict(keys=keys,expected=simulate(keys)))
    for test in range(256):
        pool=[]
        for _ in range(48):pool.append([(rng.randrange(2**64),rng.randrange(2**64))for _ in range(rng.randrange(5))])
        pool.extend([[(1,2)],[(2,3)],[(1,2),(2,3)],[(2,3),(1,2)],[(1,2),(1,2)]])
        keys=[rng.choice(pool)for _ in range(128)];streams.append(dict(keys=keys,expected=simulate(keys)))
    comparisons=[]
    for _ in range(2048):
        first=[(rng.randrange(2**64),rng.randrange(2**64))for _ in range(rng.randrange(5))]
        second=first.copy() if rng.randrange(3)==0 else [(rng.randrange(2**64),rng.randrange(2**64))for _ in range(rng.randrange(5))]
        comparisons.append([first,second,(first>second)-(first<second)])
    a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_bytes((json.dumps(dict(streams=streams,comparisons=comparisons),separators=(',',':'))+'\n').encode())
    print(f'Generated {len(streams)} independent iterative AVL streams/{len(comparisons)} unsigned key comparisons.')

if __name__=='__main__':main()
