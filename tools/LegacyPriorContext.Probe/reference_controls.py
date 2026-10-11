"""Own prior-filter reference uses set/range intersections, not binary search."""
import json, pathlib, random, sys
r = random.Random(0xca5f02); cases = []
for i in range(640):
    target = [r.randrange(0,100) for _ in range(r.randrange(1,17))]
    line, scope, scoped, first, boundary = r.randrange(5), r.randrange(4), bool(i&1), bool(i&2), i%11==0
    groups = []
    for j in range(r.randrange(33)):
        a = r.randrange(1,100); b = a+r.randrange(20)
        groups.append([r.randrange(5),r.randrange(4),a,b,j])
    expected = []
    for gl, gs, a, b, center in groups:
        if boundary or gl != line or (scoped and gs != scope):
            continue
        excluded = a in target if first else bool(set(target).intersection(range(a,b+1)))
        if not excluded:
            expected.append(center)
    cases.append(dict(target=target,line=line,scope=scope,scoped=scoped,firstOnly=first,boundary=boundary,groups=groups,expectedCenters=expected))
path=pathlib.Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps(cases,separators=(',',':')),encoding='utf8')
print(f'Wrote{len(cases)} own prior context selection cases.')
