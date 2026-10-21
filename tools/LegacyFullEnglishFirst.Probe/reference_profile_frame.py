"""Raw full resource/frame branch reference; static only, no target text."""
from pathlib import Path
import argparse,ast,base64,hashlib,json,struct,math
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def definitions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and(names is None or n.name in names)];ns={'struct':struct,'base64':base64};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
wire=definitions(a.repo/'tools/LegacySource247Wire.Probe/audit_output.py');full=(a.models/'en-US/en_US-ak-cur.res').read_bytes();lite=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(full)=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927';fr,fh=wire['source'](full);lr,lh=wire['source'](lite)
selected=next(x for x in fr.children if x['destination']=='247.148');assert(selected['Offset'],selected['End'],selected['Kind'],selected['Version'])==(1774246,3782859,0x21e,0x103)
ns=definitions(a.repo/'research/myscript/scripts/inspect_text_features.py',{'Reader','parse_alphabet','parse_network_adapter'});alphabet=ns['parse_alphabet'](full);profile=ns['parse_network_adapter'](full,selected['Offset'],alphabet);profile.pop('output_mapping');assert(profile['width'],profile['feature_kind'],profile['link_strokes'],profile['transform_kind'],profile['property14'],profile['property20'],profile['min_points'])==(13,4,0,0,0,0,2)
fd={(v['owner'],v['destination']):v for v in fr.fields};ld={(v['owner'],v['destination']):v for v in lr.fields};same=[k for k in fd.keys()&ld.keys()if k[0]=='parametersC0'];assert all(fd[k]['raw']==ld[k]['raw']for k in same)
children=[]
for dst in ['parametersC0.30','parametersC0.38']:
 x=next(v for v in fr.children if v['destination']==dst);y=next(v for v in lr.children if v['destination']==dst);fb=full[x['Offset']:x['End']];lb=lite[y['Offset']:y['End']];children.append({'destination':dst,'fullSha256':sha(fb),'liteSha256':sha(lb),'byteEqual':fb==lb})
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def frame(b,h,sourceScale,second=None):
 b,h,sourceScale=f(b),f(h),f(sourceScale);raw=(b,h);clamped=h<1 and h<f(sourceScale*f(.05))
 if clamped:h=f(1)
 if second is not None:
  b2,h2=map(f,second);b=max(b,b2);h=f(b-min(f(raw[0]-raw[1]),f(b2-h2)))
 return {'Baseline':b,'Height':h,'FirstHeightFloorAppliedBeforeUnion':clamped,'TwoLineUnion':second is not None}
controls=[]
for b,h,scale,second in [(50,.7,20,None),(50,.9,10,None),(50,1,100,None),(0,0,1,None),(0,0,0,None),(50,.7,20,(50,.7)),(50,10,20,(60,5)),(100,40,0,(110,30))]:controls.append({'Input':[b,h,scale,second],'Output':frame(b,h,scale,second)})
assert controls[0]['Output']['Height']==1 and controls[1]['Output']['Height']==f(.9)and controls[5]['Output']['Height']<1 and controls[6]['Output']['Height']==20
fallback=[]
for y,e in [(100,40),(0,0),(-10,8)]:fallback.append({'InputYExtent':[y,e],'Baseline':f(f(e*f(.75))+f(y)),'Height':f(f(e)*f(.25))})
result={'scope':'Actual full root247?selected148 plain21E/profile plus conditional1B9910/1629C0 frame arithmetic. No clustering branch selection or whole bbox claim.','resourceSha256':sha(full),'root':fh,'selected':selected,'profile':profile,'equalParentParameterScalarFields':len(same),'actualProviderChildren':children,'preparedFrameControls':controls,'preparedFallbackRecordControls':fallback,'nativeExecuted':False,'targetsRead':False};a.output.write_text(json.dumps(result,indent=2)+'\n');print({'selected':selected,'preparedControls':len(controls),'fallbackControls':len(fallback),'parameterFields':len(same),'nativeExecuted':False})
