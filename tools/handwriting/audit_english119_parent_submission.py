"""Pinned static fresh EN parent -> filtered selected119 child proposal.
Never executes a vendor binary, constructs a RU owner or performs recognition.
"""
import argparse,ast,base64,hashlib,json,struct,subprocess,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',required=True,type=Path);a=p.parse_args();root=a.native_repo.resolve();here=Path(__file__).resolve().parent
spec=json.loads((here/'english119-parent-pins.json').read_bytes());sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()==spec['dllSha256']=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);instructions={}
bodyRows=(here/'english119-parent-bodies.tsv').read_text().splitlines()[1:]
assert [[fn,int(s,16),int(e,16)+1,int(n)]for fn,s,e,n in (r.split('\t')for r in bodyRows)]==[[r['function'],r['start'],r['end'],r['end']-r['start']]for r in spec['regions']]
for row in spec['regions']:
 raw=b.data[b.off(row['start']):b.off(row['end'])];assert hashlib.sha256(raw).hexdigest()==row['sha256']
 instructions.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,row['start'])})
for at,mn,op in spec['pins']:assert instructions[at]==(mn,op),(hex(at),mn,op)
for row in spec['leaves']:
 raw=b.data[b.off(row['start']):b.off(row['end'])];assert hashlib.sha256(raw).hexdigest()==row['sha256']
 assert [[i.address,i.mnemonic,i.op_str]for i in md.disasm(raw,row['start'])]==row['instructions']
for table,slot,target in spec['bindings']:assert b.qwords(table+slot,1)[0]==target
metadata,words=spec['driverMetadata'];assert list(b.qwords(metadata,3))==words and words[:2]==[3,0x24800000249]
#Exact A0 interface dispatch, not an assumed main-vtable slot.
index=b.data[b.off(0x1801557dc)+(0xa0-0x1e)];assert index==1
assert struct.unpack_from('<I',b.data,b.off(0x1801557bc)+index*4)[0]==0x15576b
assert 0x180155772+struct.unpack_from('<i',b.data,b.off(0x18015576b)+3)[0]==0x18030f7a0
assert 0x180169631+struct.unpack_from('<i',b.data,b.off(0x18016962a)+3)[0]==0x1802df1d0
fs=functions()
for name,sha in spec['fullDecompilerTexts'].items():assert hashlib.sha256(fs[name].encode()).hexdigest()==sha
for name,needles in {
 '180163970':['param_2 + 0xb4) = 0','param_2 + 0x84) = 0','param_2 + 0xe0) = 0','param_2 + 0xe8) = 0'],
 '1801680d0':['param_2 + 0xb4','FUN_18015af40','FUN_18015b260','FUN_18015f050','FUN_1801629c0','param_2 + 0xd8'],
 '18015af40':['lVar5 + 0x10','25.4','lVar5 + 0xb0','lVar5 + 0xb8','FUN_180067aa0'],
 '18015a1c0':['0x2ab','0x2a9','lVar5 + 9','uVar9 = 0x2e3'],
 '180158970':['FUN_180156200','FUN_180157c60','0x34','0x38'],
 '180157c60':['param_3[0x16] * 3.0','0.02 <=','local_1e0','uVar6 + 1 == uVar23','FUN_180156950','FUN_180156e80','FUN_180157340','FUN_18016a280','FUN_18016a130'],
 '180156200':['DAT_1802e9bac','local_90 = 1','local_8c = 1','param_2 + 0x1e8'],
 '180156e80':['0x1e8','0x3f800000','0x4000000'],
 '18016aa00':['param_2 + 0x128','0x1c','param_2 + 0x118'],
 '1801533b0':['param_2 + 0x60','0xa0','param_1,lVar1,1'],
 '180164350':['param_2 + 200','FUN_18016a550','param_2 + 0xf8','param_2 + 0x108'],
 '18016a550':['param_3','FUN_18016a280','FUN_18016a130','param_2 + 4) = 0','param_2 + 0x1a) = 0'],
}.items():
 for needle in needles:assert needle in fs[name],(name,needle)
assert '0xb4'not in fs['180167210']
assert 'param_3 + 0x2d' in fs['180157c60'] and 'param_2 + 0x2d) = 0' in fs['18016a130']
assert 'param_2 + 0x2d'not in fs['18016a550'] and '0x168'not in fs['180167210']
assert '*param_3 + 0x38'not in fs['18015f050']
assert not any('0x190'in s for s in fs['180076ad0'].splitlines()if '='in s)
at,literal=spec['underscoreLiteral'];assert b.data[b.off(at):b.off(at)+1].hex()==literal=='5f'
for model in spec['models']:
 data=(root/'models'/model['path']).read_bytes();assert hashlib.sha256(data).hexdigest()==model['sha256']
 assert b.data[b.off(model['address']):b.off(model['address'])+len(data)]==data
parser=root/'tools/LegacySource247Wire.Probe/audit_output.py';tree=ast.parse(parser.read_text(encoding='utf-8-sig'));ns={'struct':struct,'base64':base64}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(parser),'exec'),ns)
en=(root/'models/en-US/en_US-ak-cur.lite.res').read_bytes();ru=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(en).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
parent,layout=ns['source'](en);old,oldlayout=ns['source'](ru)
assert(layout['Version'],layout['Base134Version'],layout['SourceKind'],layout['CoordinateMode'],layout['alphabetCount'])==(0x110,0x300,0x135,2,217)
assert layout['ParametersOffset']==7959 and struct.unpack_from('<4f',en,7959)==tuple(struct.unpack('<f',struct.pack('<f',x))[0]for x in (.96,.85,3,10))
assert en[7975:7979]==bytes([0,1,1,1])and struct.unpack_from('<f',en,7979)[0]==.25 and en[8811]==1
def rawchild(data,reader,name):
 c=next(c for c in reader.children if c['destination']==name);return data[c['Offset']:c['End']],c
early,c=rawchild(en,parent,'parametersC0.30');ruEarly,_=rawchild(ru,old,'parametersC0.30')
assert early==ruEarly and(c['Offset'],c['End'],c['Kind'],c['Version'])==(7983,8118,0x2ab,0x101)
assert hashlib.sha256(early).hexdigest()=='319b7934037964b875a6f76739f4b336de0d36e2f7f449b991c8088fb3c3e38a'
assert struct.unpack_from('<IIIHH',en,7999)==(0x101fe90,71,0x2a9,0,0x100)and en[7999+16+9]==0
other,c=rawchild(en,parent,'parametersC0.38');ruOther,_=rawchild(ru,old,'parametersC0.38');assert other!=ruOther
assert(c['Offset'],c['End'],c['Kind'],c['Version'])==(8118,8811,0x2d0,0x100)
assert hashlib.sha256(other).hexdigest()=='1bf9e83966098a4e611ae2ed6a53a43c30e270b8ced586260ec6c1e0775fd7bd'
coordinates=lambda reader:[(f['destination'],f['raw'])for f in reader.fields if f['owner']=='coordinate88']
assert coordinates(parent)==coordinates(old)
labelscript=root/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py';t=ast.parse(labelscript.read_text(encoding='utf-8-sig'));scope={'struct':struct}
exec(compile(ast.Module(body=[n for n in t.body if isinstance(n,ast.FunctionDef)and n.name=='labels'],type_ignores=[]),str(labelscript),'exec'),scope)
header,labels=scope['labels'](en[8843:182187]);assert labels['_']==188 and labels['-']==187 and header[-1]==217
dependency=a.output.with_name(a.output.stem+'.selected-child.json')
subprocess.run([sys.executable,str(root/'tools/handwriting/audit_english119_source_join.py'),str(dependency),'--native-repo',str(root)],check=True)
report=dict(vendorExecution=False,scope='STATIC ONLY fresh EN parent proposal; no parent runtime; empty-child/associated/prior/page/property branches excluded',completeGhidraFunctions=len(set(r['function']for r in spec['regions'])),completeGhidraRegions=len(spec['regions']),instructionPins=len(spec['pins']),bindings=len(spec['bindings']),leafRegions=len(spec['leaves']),en2ABEqualsRuBytes=True,en2D0RetainedDifferentBytes=True,underscoreLabel=188,actualB0Producer='15AF40 actual EN source profile/samples/resolution',childDependencySha256=hashlib.sha256(dependency.read_bytes()).hexdigest(),oldSeamsProposed=['LegacyFreshMainDescriptorSplit.cs','LegacyFreshMainPhysicalReordering.cs'],unchangedDirectChildFiles=6)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(json.dumps(report))
