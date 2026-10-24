import json,sys,copy
from pathlib import Path

def integer(x):
 assert type(x)is int
 return x

def audit(data):
 rows=data['RangeBudgets'];assert len(rows)==147
 seen=set()
 for row in rows:
  assert all(type(x)is int for x in row.values())
  a,b,r=row['A'],row['B'],row['R'];key=(a,b,r)
  assert a in(0,1,2,8,64,4096,65536)and b in(0,1,2,8,64,4096,65536)and r in(1,2,128)and key not in seen;seen.add(key)
  product=a*b;overlap=min(product,65536);intersection=min(product,65536+b);target=max(a,b);peak=min(target+overlap,65537)
  # Independent column ledger: union / intersect / possible subtraction.
  union_slots=5*b+4
  intersect_slots=2*b+5*intersection+4
  subtract_slots=2*target+5*peak+4
  slots=16+union_slots+intersect_slots+subtract_slots+12
  moves=16+(5*b+3*b*b)+(2*b+6*intersection)+(2*target+5*peak+overlap*peak)+24
  controls=1024+256*(1+b*(b+1)+a*(b+1)+overlap*(peak+1))
  keymax=max(1,a,min(a+overlap,65536));keys=32*keymax
  payload=16384+4096+16*(slots+keys);copies=32768+8192+32*(moves+keys)
  expected={'A':a,'B':b,'R':r,'Slots':slots,'Moves':moves,'Controls':controls,'KeyBound':keymax,'Probes':1,'Payload':payload,'Copies':copies,'Visits':copies+controls+4096+512*r*(keymax+1)}
  assert row==expected,(row,expected)
 assert data['GrowthChecks']==1024 and data['ActualOwnerExecuted']is False and data['NN']is False
 return len(rows)

if __name__=='__main__':
 data=json.loads(Path(sys.argv[1]).read_bytes());count=audit(data);neg=0
 for field in('Slots','Moves','Controls','KeyBound','Probes','Payload','Copies','Visits'):
  bad=copy.deepcopy(data);bad['RangeBudgets'][5][field]+=1
  try:audit(bad)
  except AssertionError:neg+=1
  else:raise Exception('Mutation accepted')
 for action in('missing','duplicate','bool','float'):
  bad=copy.deepcopy(data)
  if action=='missing':bad['RangeBudgets'].pop()
  elif action=='duplicate':bad['RangeBudgets'][1]=bad['RangeBudgets'][0]
  elif action=='bool':bad['RangeBudgets'][0]['A']=False
  else:bad['RangeBudgets'][0]['Payload']=float(bad['RangeBudgets'][0]['Payload'])
  try:audit(bad)
  except AssertionError:neg+=1
  else:raise Exception('Schema mutation accepted')
 report={'Status':'PASS_RANGE_ALLOCATION_CERTIFICATE_PREPARED_ONLY','Vectors':count,'ActualOperationGrowthCases':1024,'TamperRefusals':neg,'NN':False,'LiveCHARExecuted':False}
 out=Path(sys.argv[2]);assert not out.exists();out.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(report)
