"""Prepared projection mutations only; never constructs an actual capture."""
import base64,copy

def negatives(want,validate):
 failures=[]
 def reject(name,change):
  x=copy.deepcopy(want);change(x)
  try:validate(x,want)
  except(AssertionError,KeyError,TypeError,ValueError):failures.append(name)
  else:raise AssertionError('Accepted mutated projection '+name)
 def byte(x,path,at):
  p=x
  for k in path[:-1]:p=p[k]
  raw=bytearray(base64.b64decode(p[path[-1]]));raw[at]^=1;p[path[-1]]=base64.b64encode(raw).decode()
 for name,channel,path,at in [
  ('owner descriptor','Owners',['Owners',0,'Records'],0),('owner symbol','Owners',['Owners',0,'Records'],8),
  ('owner Score0C','Owners',['Owners',0,'Records'],12),('owner vector10','Owners',['Owners',0,'Records'],16),
  ('owner Cost14','Owners',['Owners',0,'Records'],20),('owner opaque tail','Owners',['Owners',0,'Records'],39),
  ('choice owner bytes','GlyphChoices',['GlyphChoices',0,'Owner40'],12),('selection ordinal','GlyphChoices',['GlyphChoices',0,'Selected4'],0),
  ('source first coordinate','GlyphChoices',['GlyphChoices',0,'Source26'],0),('source tail residual','GlyphChoices',['GlyphChoices',0,'Source26'],-1),
  ('UTF8','GlyphUtf8',['GlyphUtf8',0,'Utf8',0],0),('UTF8 retained selection','GlyphUtf8',['GlyphUtf8',0,'RetainedSelection4'],0)]:
  reject(name,lambda x,path=path,at=at:byte(x,path,at))
 reject('first score bool',lambda x:x['GlyphChoices'][0].update(FirstScoreBits=True))
 reject('visited bool',lambda x:x['GlyphChoices'][0].update(Refreshed=True))
 reject('marker wrong',lambda x:x['GlyphChoices'][0].update(Marker=123))
 reject('reference wrong',lambda x:x['GlyphChoices'][0].update(Reference=0))
 reject('owner packed wrong',lambda x:x['Owners'][0].update(PackedCount=0))
 reject('UTF8 flag wrong',lambda x:x['GlyphUtf8'][0]['Flags'].__setitem__(0,123))
 reject('UTF8 marker wrong',lambda x:x['GlyphUtf8'][0].update(Marker=123))
 reject('extra owner',lambda x:x['Owners'].append(copy.deepcopy(x['Owners'][0])))
 reject('omitted choice',lambda x:x['GlyphChoices'].clear())
 reject('extra UTF8 ref',lambda x:x['GlyphUtf8'].append(copy.deepcopy(x['GlyphUtf8'][0])))
 reject('unknown channel',lambda x:x.update(Ignored=True))
 return len(failures)
