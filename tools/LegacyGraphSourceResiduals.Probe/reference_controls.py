"""Independent source tick->ceil-key/remainder and21B normal forward oracle."""
import argparse,base64,json,struct,subprocess,sys
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('path',type=Path);p.add_argument('--audit',action='store_true');a=p.parse_args()
if not a.audit:
    # Reuse committed original raw-coordinate inputs, not managed output or labels.
    subprocess.run([sys.executable,str(Path(__file__).resolve().parents[1]/'LegacyGraphSourceRanges.Probe/reference_controls.py'),str(a.path)],check=True)
    raise SystemExit(0)
queries=merges=nonzero=tail_differences=reordered=0
for case in json.loads(a.path.read_bytes()):
    nodes=case['nodes'];seeds=case['seeds'];keys=case['keys'];counts=case['counts'];factor=case['factor'];ticks=case['map'];raw_counts=case['originalCounts']
    starts=[];offset=0;metadata=[]
    for stroke,count in enumerate(counts):
        starts.append(offset);points=[]
        for point in range(count):
            tick=ticks[offset+point] if ticks else offset+point
            whole,remainder=divmod(tick,factor);index=whole+(remainder!=0);residual=(factor-remainder)&255 if remainder else 0
            key=keys[index];last=list(key)
            last[2]=keys[index+1][2]-1 if index+1<len(keys) and keys[index+1][1]==key[1] else raw_counts[key[1]]-1
            points.append(dict(key=key,last=last,index=index,residual=residual))
        metadata.append(points);offset+=count
    assert not ticks or len(ticks)==offset
    reordered+=any(points[0]['key'][1]!=physical for physical,points in enumerate(metadata))
    def visit(node,ordinal,target):
        group,start,end,children=nodes[node][ordinal]
        if target<end:return None
        if target==end:return [group]
        for child in children:
            child_path=visit(node+1,child,target)
            if child_path is not None:return [group]+child_path
        return None
    for query in case['queries']:
        path=None;first_node=query['first']>>7
        for ordinal,arc in enumerate(nodes[first_node]):
            if arc[1]==query['first']:
                path=visit(first_node,ordinal,query['last'])
                if path is not None:break
        assert path is not None;rows=[]
        for group in path:
            stroke,start,end=seeds[group];first=metadata[stroke][start];last=metadata[stroke][end-1];tail=end==counts[stroke]
            rows.append(dict(first=first,last=last,tail=tail));tail_differences+=tail and last['key']!=last['last']
        def order(row):return tuple(row['first']['key'])+(-row['first']['residual'],)+tuple(row['last']['last'])+(-row['last']['residual'],)
        # Independent stable bubble comparison; runtime uses an owned comparer.
        for end in range(len(rows)-1,0,-1):
            for i in range(end):
                if order(rows[i])>order(rows[i+1]):rows[i],rows[i+1]=rows[i+1],rows[i]
        for i in range(len(rows)-1,0,-1):
            previous,current=rows[i-1],rows[i];last=previous['last'];first=current['first']
            equal=first['key']==last['key'] and first['residual']==last['residual']
            adjacent=previous['tail'] and last['index']+1<len(keys) and first['key']==keys[last['index']+1]
            if equal or adjacent:
                previous['last']=current['last'];previous['tail']=current['tail'];rows.pop(i);merges+=1
        expected=[]
        for row in rows:
            first=row['first'];last=row['last'];end=last['last'] if row['tail'] else last['key']
            ref=struct.pack('<iiiiii',*first['key'],*end);residual=bytes([first['residual'],last['residual']])
            expected.append(dict(reference24=base64.b64encode(ref).decode(),residual2=base64.b64encode(residual).decode()))
            nonzero+=sum(x!=0 for x in residual)
        assert expected==query['ranges'],(queries,expected,query['ranges']);queries+=1
assert queries>1000 and merges>0 and nonzero>1000 and reordered>100 and tail_differences>0
print(f'PASS independent raw-tick/source-key oracle: {queries} source callbacks/{nonzero} nonzero endpoint residues/{merges} reverse merges/{reordered} reordered physical owners/{tail_differences} duplicate-tail differences; full24+2 bytes.')
