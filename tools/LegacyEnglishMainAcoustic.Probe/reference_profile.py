"""Independent raw actual EN MAIN119 feature profile/shape/scope reference; no SDK execution."""
from pathlib import Path
import argparse,struct,json,hashlib
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('output',type=Path);a=p.parse_args();d=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();digest=hashlib.sha256(d).hexdigest();assert digest=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
h=d[184221:184265];at=0;values={}
for name,fmt in [('14','H'),('18','I'),('1C','H'),('1E','H'),('20','H'),('22','H'),('28','H')]+[(f'{n:X}','I')for n in range(0x2c,0x45,4)]:values[name]=struct.unpack_from('<'+fmt,h,at)[0];at+=struct.calcsize(fmt)
assert at==44 and values=={'14':4369,'18':63,'1C':30,'1E':16,'20':8,'22':10,'28':4369,'2C':1974271,'30':511,'34':925695,'38':925695,'3C':255,'40':100,'44':10}
windows=struct.unpack_from('<H',d,184267)[0];cols,rows=struct.unpack_from('<HH',d,184271);assert(windows,cols,rows)==(16,4,4)
assert list(d[184281:184283])==[1,2]and d[184297:184299]==bytes(2)
shapes=[values['1C']*7+7+9,values['1E']*12+7+9,1+values['2C'].bit_count()+values['30'].bit_count()+windows*values['34'].bit_count(),1+values['2C'].bit_count()+values['30'].bit_count()+cols*rows*values['38'].bit_count()];assert shapes==[226,208,283,283]
header=struct.unpack_from('<8H',d,99);assert header==(181,178,65535,180,184,182,183,217)
assert d[545671]==0 and d[545658]==1 #actual1B95A0 count-mask no-op and span+1 power
assert d[545680]==0 and struct.unpack_from('<I',d,545681)[0]==0;meta=545685;modeMask=struct.unpack_from('<I',d,meta+13*4)[0];assert(modeMask,d[meta+128],meta+186)==(0x303,16,len(d))
r=dict(scope='STATIC RAW actual MAIN23A/2AF variant2-to2B1 profile and scope widths; not evaluated feature vectors/NN or winner',akSha256=digest,profileOffset=184221,profileFields=values,temporalWindows=windows,spatialGrid=[cols,rows],unusedUpperGrid=list(struct.unpack_from('<HH',d,184277)),variantBytes=list(d[184281:184283]),featureWidths=shapes,appendedContextCount=0,boundaryCount=2,networkInputWidths=[v+2 for v in shapes],alphabetHeader=list(header),freshMode13Flags=0x21703&~modeMask,pruneCandidates=d[meta+128]>13,powerCount='requested graph arc scope/span+1 (FC=1); distinct source identifiers separately filter masks',nativeExecuted=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(dict(profile='1111/1111 variant2',widths=r['networkInputWidths'],flags=hex(r['freshMode13Flags']),nativeExecuted=False)))
