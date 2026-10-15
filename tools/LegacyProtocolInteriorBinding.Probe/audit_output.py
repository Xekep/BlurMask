"""Independent216BC0 request/scratch bridge for proven1AAE70 AC2 branch."""
from pathlib import Path
import base64,json,struct,sys
if len(sys.argv)!=2:raise SystemExit('capture.json')
p=json.loads(Path(sys.argv[1]).read_bytes());emissions=0
for c in p['cases']:
 r=base64.b64decode(c['row']);q=bytearray(base64.b64decode(c['residue']))
 i=lambda at:struct.unpack_from('<i',r,at)[0]
 u=lambda at:struct.unpack_from('<H',r,at)[0]
 def w32(at,v):struct.pack_into('<I',q,at,v&0xffffffff)
 def w16(at,v):struct.pack_into('<H',q,at,v&65535)
 assert u(58)==0xfffb and r[60]==3 and i(32)==-1
 w32(0,-1);w16(4,65535);q[8:14]=r[24:30];q[16:22]=r[12:18];w16(22,65535)
 current=i(0)+(u(56)==65535);w32(24,current);q[28:30]=r[4:6];q[30:32]=r[56:58]
 q[32:38]=r[36:42];w32(40,1);w32(44,2);w32(48,c['b0']);w32(52,0);w32(56,0);q[60]=1
 assert q==base64.b64decode(c['request'])
 emit=c['cursor']>0 and i(24)!=-1 and current!=p['nodeCount'];emissions+=emit
 labels=[c['seed']*17+j*31 for j in range(256)]
 if emit:labels[c['cursor']-1]=0xfffb
 assert base64.b64decode(c['labels'])==struct.pack('<256H',*labels)
 assert (c['remaining'],c['score0C'],c['score10'],c['cost'],c['component'],c['external'],c['publication'])==(c['cursor']-emit,0x3f800000,0x3f800000,0,0,0,0)
assert len(p['cases'])==p['controls']==1024 and emissions==p['emitted']==192 and p['guards']==5
print('PASS independent1024 fullrequest64/fullscratch512/scorecursor bridges,192 actualFFFB AC2 emissions and endpoint/cursor earlyreturns; preparedpacketinputs only.')
