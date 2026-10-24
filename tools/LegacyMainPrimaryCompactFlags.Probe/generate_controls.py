"""Prepared flag controls only. No model or actual query authority."""
import itertools,json,sys
def cases():
 rows=[]
 for quota,selected,fallback,replies,header,prior in itertools.product(range(3),range(3),(False,True),itertools.product((False,True),repeat=2),range(0,0x2000,0x200),range(5)):
  previous=dict(Tag=0,Context=11 if prior==4 else 10,Auxiliary=7 if prior==3 else -1,Fallback=True,Bit3=prior in (1,3,4),Bit4=prior==2)
  current=dict(Tag=1<<32,Context=10,Auxiliary=-1,Fallback=fallback,Bit3=False,Bit4=False)
  rows.append(dict(Id=len(rows),Rows=[previous,current],GroupStart=0,Selected=selected,Quota=quota,Headers=[0,header],Replies=list(replies)))
 assert len(rows)==5760
 return rows
if __name__=='__main__':
 with open(sys.argv[1],'w',encoding='utf8') as f:json.dump(cases(),f,separators=(',',':'))
