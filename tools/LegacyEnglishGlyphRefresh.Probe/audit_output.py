import json,struct,sys
from pathlib import Path

def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def row(descriptor,symbol,score=1,neural=-1,cost=0,end=False):
 b=bytearray(40);struct.pack_into('<iHHH',b,0,descriptor,8 if end else 0,0,symbol);struct.pack_into('<ffi',b,12,f(score),f(neural),cost);b[32:40]=b'\xff'*8;return b
expected=bytes(row(77,3)+row(0,1,1,.125,1011,True)+row(1,2,f(128/255),.25,1002)+row(3,1,f(200/255),.125,1001))
calls=['resolve:1','score:1','cost:1:1','resolve:2','score:2','cost:2:0','resolve:1','score:1','cost:1:0']
def audit(x):
 assert set(x)=={'Scope','Visited','Calls','Owner','Selected','Marker','ExactCancel','BadRange','RepeatedExact','MaximumOwnerCount','MaximumSelection','MaximumMarker','MissingOwnerRefused'}
 assert x['Scope']=='PREPARED_GENERIC_REFRESH_SELECTION_NOT_ACTUAL_EN_OWNER'
 assert type(x['Visited']) is int and x['Visited']==3
 assert x['Calls']==calls+calls
 assert x['RepeatedExact'] is True
 assert bytes.fromhex(x['Owner'])==expected
 assert bytes.fromhex(x['Selected'])==struct.pack('<iii',0,1,2)
 assert type(x['Marker']) is int and x['Marker']==0
 assert x['ExactCancel'] is True and x['BadRange'] is True
 assert type(x['MaximumOwnerCount']) is int and x['MaximumOwnerCount']==2040
 assert bytes.fromhex(x['MaximumSelection'])==b''.join(struct.pack('<i',v)for v in range(2039,-1,-1))
 assert type(x['MaximumMarker']) is int and x['MaximumMarker']==2039
 assert x['MissingOwnerRefused'] is True
x=json.loads(Path(sys.argv[1]).read_text());audit(x)
count=0
for name in ['Owner','Selected','Calls','Marker','Visited','ExactCancel','BadRange','RepeatedExact','MaximumOwnerCount','MaximumSelection','MaximumMarker','MissingOwnerRefused']:
 y=json.loads(json.dumps(x))
 if name in ['Owner','Selected','MaximumSelection']:y[name]='00'+y[name][2:]
 elif name=='Calls':y[name]=list(reversed(y[name]))
 elif name in ['Marker','Visited','MaximumOwnerCount','MaximumMarker']:y[name]=False
 else:y[name]=False
 if name in ['Owner','Selected','MaximumSelection'] and y[name]==x[name]:
  y[name]='FF'+y[name][2:]
 try:audit(y)
 except (AssertionError,TypeError,ValueError):count+=1
 else:raise AssertionError('Tamper accepted: '+name)
print(json.dumps({'Scope':'GENERIC_PREPARED_ONLY','FullOwnerBytes':len(expected),'SelectionBytes':12,'OrderedCallbacks':18,'TamperRefusals':count,'MaximumOwnerCount':2040,'MaximumSelectionBytes':8160,'MaximumInsertionCopyBytes':2*2040*2039}))
