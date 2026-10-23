"""Pure whole-parent B6 admission arithmetic; no production/vendor imports."""
import json
from pathlib import Path
C,T,G,P,V=256,4096,4096,8192,1048576
# Convex squares maximize by filling 256 cells in 16 children; empty slots cost4.
Q=16*258**2+240*2**2
assert Q==1065984
W=2*T;H=2*T+2*C
peak=(8<<20)+8192*T+2048*G+2048*C+128*P+1024*W
work=(16<<20)+32768*T+4096*G+4096*C+512*P+256*Q
visits=8192*T+1024*G+4096*C+64*Q+32*V
assert (peak,work,visits)==(60293120,445906944,140574720)
assert peak<=64<<20 and work<=512<<20 and visits<=256<<20
assert (2*256+2+9)//10*10==520<2040
def admitted(p,w,v,limits):return p<=limits[0] and w<=limits[1] and v<=limits[2]
assert admitted(peak,work,visits,(peak,work,visits))
for i in range(3):
 limits=[peak,work,visits];limits[i]-=1
 assert not admitted(peak,work,visits,limits)
r=dict(status='PASS',children=C,trace=T,selectedGroups=G,cutPoints=P,groupPointVisits=V,prospectiveWords=W,prospectiveRichAttempts=H,quadraticWork=Q,peakBytes=peak,copyWorkBytes=work,visits=visits,scope='Conservative checked library admission, not RSS; runtime still unimplemented')
Path(__file__).with_name('budget-reference.json').write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
