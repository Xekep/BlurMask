"""Independent 13AF30 bit-packing oracle; no vendor code is executed."""
import base64,json,pathlib,random,struct,sys
r=random.Random(13030);cases=[]
counts=[0,1,2,8191,8192,8193,-1,-8192,-8193,2147483647,-2147483648]
symbols=[0,1,179,180,65528,65529,65530,65531,65532,65533,65535]
for symbol in symbols:
 for count in counts:
  for flag in [0,1,2,3,127,128,255]:
   for variant in [False,True]:
    row=bytearray(r.randbytes(40));struct.pack_into('<i',row,0,17)
    before=bytes(row);accepted=True
    if variant:kind,label=4,symbol
    elif count==0:kind,label=1,1
    elif 65529<=symbol<=65532:
     if count>=8192:accepted=False
     else:kind,label=1,((((flag%8)*8192+(count%8192))*4)%65536)+(symbol-1)%4
    else:kind,label=0,symbol
    if accepted:
     struct.pack_into('<H',row,4,(struct.unpack_from('<H',row,4)[0]//8)*8+kind)
     struct.pack_into('<H',row,8,label)
    cases.append(dict(symbol=symbol,count=count,flag=flag,variant=variant,accepted=accepted,before=base64.b64encode(before).decode(),after=base64.b64encode(row).decode()))
target=pathlib.Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True)
target.write_text(json.dumps(cases,separators=(',',':'))+'\n');print(f'{len(cases)} independent signed-count/flag/symbol cases')
