"""Independent raw-resource mode1 calibration controls; no engine execution."""
import hashlib,json,math,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('acoustic.res output.json')
data=Path(sys.argv[1]).read_bytes()
assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<IHH',data,566408)==(1,192,256)
rows=[];endpointRows=[]
for family in [0,1,2,3,4,255,4294967295]:
    for symbol in list(range(192))+[192,193,194,65533,65535]:
        center,slope=struct.unpack_from('<ff',data,566416+8*(family*192+symbol)) if family<4 and symbol<192 else (0.,.575)
        for probability in [0,1,31,127,1024,4096,8191,8192,16384,24576,32766,32767]:
            # Quantized EXP multiplication is exact in binary32 here. Native
            # widens the stored float coefficients before its double exp.
            result=1/(1+math.exp((probability/32768-center)*-slope))
            bits=struct.unpack('<I',struct.pack('<f',result))[0]
            rows.append([symbol,family,probability,bits])
        #Additive endpoint channel; the original16548 rows/order are unchanged.
        endpoint=1/(1+math.exp((1.0-center)*-slope))
        endpointRows.append([symbol,family,32768,struct.unpack('<I',struct.pack('<f',endpoint))[0]])
out=Path(sys.argv[2]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_bytes((json.dumps(dict(rows=rows,endpointRows=endpointRows),separators=(',',':'))+'\n').encode())
print(f'PASS generated unchanged {len(rows)} independent full192-symbol/four-family and native fallback references plus separate {len(endpointRows)} endpoint32768 references')
