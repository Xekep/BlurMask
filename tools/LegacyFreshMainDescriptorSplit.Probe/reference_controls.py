"""Original byte-exact fresh15B260 controls over independent actual NN groups."""
import json,struct,sys
from pathlib import Path

def i32(b,at,value):struct.pack_into('<i',b,at,value)
def descriptor(case):
    b=bytearray(88);struct.pack_into('<H',b,0,16);i32(b,4,-2147483648)
    for at in [8,12,16,20,24,32]:i32(b,at,-1)
    i32(b,40,len(case['strokes'])-1);struct.pack_into('<ff',b,44,*case['strokes'][0][0]);b[72]=1;b[76:84]=bytes([255])*8
    return b

def prepare(case):
    groups=case['expected']['groups'];b=descriptor(case);descriptors=[];order=[]
    if len(groups)==1:
        i32(b,24,0x2000002);i32(b,8,len(case['clean'][0]) if len(case['strokes'])==1 else -1)
        descriptors=[b];order=list(range(len(case['strokes'])))
    else:
        for g,ids in enumerate(groups):
            i32(b,24,0x2000003);struct.pack_into('<H',b,0,16 if g==0 else 0)
            i32(b,36,len(order));order+=sorted(ids);i32(b,40,len(order)-1)
            i32(b,4,-2147483648);i32(b,8,-1);b[72]=1;b[76:84]=bytes([255])*8;descriptors.append(b[:])
            if g+1<len(groups):
                separator=bytearray(88);struct.pack_into('<H',separator,0,3)
                i32(separator,24,0x2000003);i32(separator,32,-1);i32(separator,36,-1);struct.pack_into('<f',separator,40,.5)
                for at in [16,20,8]:i32(separator,at,-1)
                i32(separator,4,-2147483648);descriptors.append(separator);b=b[:]
    starts=[];total=0
    for stroke in case['clean']:starts.append(total);total+=len(stroke)
    ticks=[200*(starts[i]+p) for i in order for p in range(len(case['clean'][i]))]
    return dict(order=order,descriptors=[b.hex() for b in descriptors],ticks=ticks)

def main():
    if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('Supply group-controls.json output-split.json')
    data=json.loads(Path(sys.argv[1]).read_bytes())
    for case in data['cases']:case['split']=prepare(case)
    Path(sys.argv[2]).write_bytes((json.dumps(data,separators=(',',':'))+'\n').encode('utf8'))
    print(f'Wrote {len(data["cases"])} independent fresh88-byte descriptors/physical maps/factor200 controls.')
if __name__=='__main__':main()
