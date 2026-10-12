"""Own independent descriptor-event oracle, no vendor binary/assets execution."""
import json,random,sys,struct
from pathlib import Path
r=random.Random(0x167210);cases=[]
for k in range(384):
 strokes=[];events=[];count=0;first=None
 for ordinal in range(k%12+1):
  x=r.randrange(-500,500);y=r.randrange(-500,500)
  points=[[x,y],[x,y],[x+1,y+3],[x+1,y+3],[x+5,y+8]]
  if k%3==0:points=[[x,y]]*4
  strokes.append(points)
  #Independent descriptor state machine: source8efalse andpreviouskind0 reuse;
  #raw append yields physical count and updates only descriptorend coordinate.
  if first is None:first=ordinal
  events.append(dict(first=first,last=ordinal,descriptor=0,inputStroke=ordinal));count+=1
 cases.append(dict(strokes=strokes,events=events))
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(cases,separators=(',',':')))
