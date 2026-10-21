"""Independent actual full249/119 input metadata/tensor inventory, never target labels."""
from pathlib import Path
import argparse,ast,struct,base64,json,hashlib
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();path=a.repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py';ns={'struct':struct,'base64':base64};exec(compile(ast.Module(body=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(path),'exec'),ns);sha=lambda b:hashlib.sha256(b).hexdigest();results=[]
for name in ['en_US-ak-cur.res','en_US-ak-cur.lite.res']:
 d=(a.models/'en-US'/name).read_bytes();_,_,_,ch=ns['source'](d,0);selected=next(c for c in ch if c[3]==0x249);start=selected[1]+16;se,_,al,ch=ns['source'](d,start);_,at,end,_,_=next(c for c in ch if c[0]=='140');r=ns['Acoustic'](d,at,end);r.take('21D','envelope',1,16);r.u('08');r.u('09');target=r.u('0A',2);mult=r.f('0C');assert r.u('18.initialPresent')==r.u('10.dynamicPresent')==0 and r.u('68.profileCount')==1 and r.u('00.gridPresent',owner='profile0')==0
 r.u('14',2,'profile0');r.u('18',4,'profile0')
 for f in('1C','1E','20','22','28'):r.u(f,2,'profile0')
 for f in('2C','30','34','38','3C','40','44'):r.f(f,'profile0')
 for c,v,n in(('48','50',1),('58','60/68',2),('70','78/80',2)):r.lists(c,v,n)
 r.u('88',owner='profile0');r.u('89',owner='profile0');r.u('90',2,'profile0')
 for f in('94','98'):r.f(f,'profile0')
 for f in('9C','9E'):r.u(f,2,'profile0')
 assert r.u('optionalA0',owner='profile0')==r.u('optionalPair',owner='profile0')==0
 slots=[]
 for index in range(7):
  owner=f'slot{index}';assert r.u('conversionPresent',owner=owner)==0;first=r.u('first',owner=owner);second=r.u('second',owner=owner);mode=r.u('mode',owner=owner);weight=r.f('weight',owner)if mode==0 else None;nets=[]
  for position in range(3):
   present=r.u(f'network{position}.present',owner=owner)
   if not present:continue
   p=r.p;magic,v,size,architecture,kind,inputs,hidden,u1,u2,outputs,trailing=struct.unpack_from('<HHIBHIIIIIH',d,p);assert(magic,v,architecture,u1,u2,trailing)==(0xfea0,0x107,0x12,0,0,10);z=p+8+size;q=p+33;matrices=[]
   for cols,rows in[(inputs,hidden-1),(hidden,outputs-1)]:
    q=(q+15)//16*16;stride=(cols+15)//16*16;sc=q+stride*rows;assert d[sc]==1 and all(d[sc+1:sc+rows+1]);matrices.append({'input':cols,'rows':rows,'stride':stride,'weightsOffset':q,'scalesOffset':sc,'weightsSha256':sha(d[q:sc]),'scalesSha256':sha(d[sc:sc+rows+1])});q=sc+rows+1
   assert q==z;nets.append({'offset':p,'end':z,'version':v,'position':position,'kind':kind,'input':inputs,'hidden':hidden-1,'output':outputs-1,'matrices':matrices});r.take(owner,'net',1,z-p)
  slots.append({'index':index,'first':first,'second':second,'mode':mode,'weight':weight,'networks':nets})
 prefix=r.p;r.u('78');r.f('7C');r.u('80');r.f('88');r.f('84')
 for i in range(r.u('90',2)):r.lists(f'98[{i}].count',f'98[{i}]',1,'21D')
 for f in('A4','A8','AC','B0','B4'):r.f(f)
 r.u('B8');r.f('BC');r.f('C0');groups=r.u('C8',2)
 if groups:
  ends=struct.unpack('<'+'H'*(groups+1),r.take('21D','D0',2,groups+1));r.take('21D','D8',2,ends[-1]);[r.f(f'E0[{i}]')for i in range(groups)]
 r.u('E8',4);r.u('EC',2);r.u('EE',2)
 for f in('F0','F4','F8'):r.u(f,4)
 r.u('FC');r.f('100');assert r.u('108',4)==r.u('128',4)==r.u('148/150.present')==0;r.f('120');r.f('124');assert r.p==end
 results.append({'file':name,'resourceSha256':sha(d),'selected249':{'offset':selected[1],'end':selected[2]},'source247':{'offset':start,'end':se,'sha256':sha(d[start:se]),'alphabet':al},'exponent':struct.unpack_from('<f',d,se)[0],'acoustic119':{'offset':at,'end':end,'sha256':sha(d[at:end])},'normalizationTarget':target,'multiplier':mult,'slots':slots,'prefixEnd':prefix,'fields':r.fields,'positiveGroups':groups})
full,lite=results
assert full['resourceSha256']=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'and lite['resourceSha256']=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
def val(m,owner,name):return base64.b64decode(next(f['Raw']for f in m['fields']if f['Owner']==owner and f['Destination']==name))
assert full['source247']['alphabet']==217 and full['normalizationTarget']==100 and full['multiplier']==1 and full['exponent']==.75
assert val(full,'profile0','14')==val(full,'profile0','28')==bytes.fromhex('1110')and val(full,'profile0','88')==b'\x01'and val(full,'profile0','89')==b'\x02'
assert val(full,'21D','08')==val(full,'21D','09')==b'\xff'
assert all((full['slots'][i]['first'],full['slots'][i]['second'],full['slots'][i]['mode'])==(0,1,2)for i in range(4))
ft=[(f['Owner'],f['Destination'],f['Raw'])for f in full['fields']if f['Offset']>=full['prefixEnd']];lt=[(f['Owner'],f['Destination'],f['Raw'])for f in lite['fields']if f['Offset']>=lite['prefixEnd']];assert ft==lt
assert [n['input']for s in full['slots']for n in s['networks']]==[219,201,276,276]and [n['input']for s in lite['slots']for n in s['networks']]==[219,201]
assert all(full['slots'][i]['networks'][0]['matrices'][j][key]==lite['slots'][i]['networks'][0]['matrices'][j][key]for i in(0,1)for j in(0,1)for key in ['weightsSha256','scalesSha256'])
wirePath=a.repo/'tools/LegacySource247Wire.Probe/audit_output.py';w={'struct':struct,'base64':base64};exec(compile(ast.Module(body=[n for n in ast.parse(wirePath.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(wirePath),'exec'),w)
for m in results:
 data=(a.models/'en-US'/m['file']).read_bytes();lo,hi=m['source247']['offset'],m['source247']['end'];source,head=w['source'](data[lo:hi]);field=lambda n:base64.b64decode(next(f['raw']for f in source.fields if f['owner']=='247'and f['destination']==n));mask=struct.unpack('<16I',field('16C'));limits=struct.unpack('<16H',field('1AC'));graph=next(c for c in source.children if c['destination']=='247.138');g=data[lo+graph['Offset']:lo+graph['End']];m['sourcePolicy']={'clearMaskByMode':mask,'modeLimits':limits,'1F0':list(field('1F0')),'mode13ClearMask':mask[13],'mode13KnowledgeFlags':0x21703&~mask[13],'mode13Limit':limits[13],'graphSha256':sha(g),'maximumAtoms':g[16],'nativeSecondByte':g[17],'reorder':g[306],'insertionShift':g[307]}
assert full['sourcePolicy']['insertionShift']==1 and lite['sourcePolicy']['insertionShift']==0 and full['sourcePolicy']['maximumAtoms']==7 and full['sourcePolicy']['mode13KnowledgeFlags']==0x21400 and lite['sourcePolicy']['mode13KnowledgeFlags']==0x21700
result={'scope':'Raw full and lite FIRST119 are distinct owners; only first2matrixbytes coincide, fulladdsactual1011/2B1 families without9baseline channels. No neural inference or target text.','models':results,'firstTwoWeightBytesEqual':True,'nativeExecuted':False,'targetsRead':False};a.output.write_text(json.dumps(result,indent=2)+'\n');print({'fullNets':4,'liteNets':2,'fullGroups':full['positiveGroups'],'liteGroups':lite['positiveGroups'],'firstTwoWeightBytesEqual':True})
