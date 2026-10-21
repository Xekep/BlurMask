"""Literal static BC280 landmark reference; source coordinates only, no labels.
Unlike genericB6D20, both point2 axis comparisons use point1.
"""
def reference(strokes):
 if not 1<=len(strokes)<=15 or any(not s for s in strokes)or sum(map(len,strokes))>4096:raise ValueError('Bounded nonempty actual2B1 geometry required')
 flags=[];ends=[]
 for points in strokes:
  current=[0]*len(points)
  for axis,minimum,maximum in [(0,1,2),(1,16,8)]:
   values=[p[axis]for p in points];direction=0
   for a,b in zip(values,values[1:]):
    if a!=b:
     direction=1 if b>a else -1;current[0]|=minimum if direction>0 else maximum;break
   previous=values[1]if len(points)>1 else values[0]
   for i in range(2,len(values)):
    now=1 if values[i]>previous else -1 if values[i]<previous else 0
    if now:
     if direction==-now:current[i-1]|=minimum if now>0 else maximum
     direction=now
    previous=values[i]
   if direction:current[-1]|=maximum if direction>0 else minimum
  current[0]|=64;current[-1]|=128;flags.extend(current);ends.append(len(flags))
 return dict(strokes=strokes,flags=flags,ends=ends)
