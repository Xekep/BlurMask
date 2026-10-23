"""Strict independent B8 source720/source-membership/chronology oracle.
Actual current permutation and scale are typed upstream INGRESS, not re-run lineNN;
raw point ownership/metadata, flat/filter/projection and every720 byte are replayed.
B7 data must match a separately range/numeric-audited B7 receipt, never promotesJSON.
"""
import argparse,base64,copy,hashlib,json,math,struct,sys
from pathlib import Path
sys.dont_write_bytecode=True;sys.path.insert(0,str(Path(__file__).resolve().parent))
from raw_profile import read_profile
from reference_rows import f,point,decode,key,produce,overwrite_flat
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--input',type=Path,required=True);p.add_argument('--b7',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--report',type=Path,required=True);p.add_argument('--tamper',action='store_true');a=p.parse_args()
def read(path):return json.loads(path.read_bytes(),parse_int=lambda x:-0.0 if x=='-0' else int(x))
def signedbits(x):return struct.unpack('<i',struct.pack('<f',f(x)))[0]
def bits(strokes):return [[[signedbits(x),signedbits(y)] for x,y in s] for s in strokes]
def flatten(x):return [v for s in x for v in s]
def normalized(strokes):return [[point(p) for p in s] for s in strokes]
def samepoint(p,q):return not any(abs(x)+abs(y)<abs(f(x-y))*8388608 for x,y in zip(p,q,strict=True))
def flat(stroke,scale):
 xs=[p[0] for p in stroke];ys=[p[1] for p in stroke];w=f(max(xs)-min(xs));h=f(max(ys)-min(ys))
 return w>3*scale and f(h/w)<.02

def integer_tree(value):
 return all(integer_tree(x) for x in value) if isinstance(value,list) else type(value) is int

def parent_source(actual,source):
 assert all(integer_tree(actual[k]) for k in ('RawBits','RetainedBits','CurrentBits','OriginalPhysical','Chronology','EndingChronology'))
 raw=source.get('Raw',source.get('strokes'));expectedbits=bits(raw);assert actual['RawBits']==expectedbits
 numeric=normalized(expectedbits);kept=[];metadata=[];endings=[];index=0
 for si,s in enumerate(numeric):
  indices=[i for i,p in enumerate(s) if i==0 or not samepoint(p,s[i-1])]
  kept.append([expectedbits[si][i] for i in indices]);rr=[];ee=[]
  for j,pi in enumerate(indices):
   rr.append([0,si,pi,index,index,0]);ee.append([0,si,indices[j+1]-1 if j+1<len(indices) else len(s)-1,index,index,0]);index+=1
  metadata.append(rr);endings.append(ee)
 assert actual['RetainedBits']==kept
 permutation=actual['OriginalPhysical'];assert sorted(permutation)==list(range(len(kept)))
 #This actual retained physical order is an ingress from the already audited
 #typed A owner. We prove every coordinate/key belongs to its raw physical ID;
 #do not claim to recompute the upstream neural line-order decision here.
 assert actual['CurrentBits']==[kept[i] for i in permutation]
 assert actual['Chronology']==[x for i in permutation for x in metadata[i]]
 assert actual['EndingChronology']==[x for i in permutation for x in endings[i]]
 assert len(actual['CurrentBits'])<=256 and sum(map(len,actual['CurrentBits']))<=8192
 assert math.isfinite(actual['ScalePrimary']) and actual['ScalePrimary']>=0
 return normalized(actual['CurrentBits'])

def hash_b7(children):
 h=hashlib.sha256()
 def u(n):h.update(struct.pack('<Q',n&0xffffffffffffffff))
 for child in children:
  u(child['Descriptor']);u(len(child['Owners']))
  for owner in child['Owners']:
   records=base64.b64decode(owner['Records'],validate=True);u(owner['Key']);u(owner['PackedCount']);u(len(records));h.update(records)
  u(len(child['Ranges']))
  for ranges in child['Ranges']:
   u(ranges['Key']);u(len(ranges['Rows']))
   for row in ranges['Rows']:
    for v in row:u(v)
  u(len(child['Words']))
  for word in child['Words']:
   data=base64.b64decode(word['Storage'],validate=True);assert len(data)==120;h.update(data);u(len(word['References']))
   for ref in word['References']:u(ref)
 return h.hexdigest()

def hash_parent(parent):
 h=hashlib.sha256()
 def u(n):h.update(struct.pack('<Q',n&0xffffffffffffffff))
 u(len(parent['CurrentBits']))
 for stroke in parent['CurrentBits']:
  u(len(stroke))
  for x,y in stroke:u(x);u(y)
 u(len(parent['OriginalPhysical']))
 for i in parent['OriginalPhysical']:u(i)
 for row in parent['Chronology']:
  for v in row:u(v)
 u(len(parent['Descriptors']))
 for d in parent['Descriptors']:h.update(base64.b64decode(d,validate=True))
 h.update(struct.pack('<dd',parent['ScalePrimary'],parent['ScaleSecondary']));u(signedbits(parent['Resolution']))
 return h.hexdigest()

def replay(out,raw_inputs,baseline,profile):
 assert set(out)=={'TargetsRead','Scope','GuardMode','Actual','Accepted','Refused'} and type(out['GuardMode']) is bool
 assert 1<=len(raw_inputs)<=10000
 assert type(out['Accepted']) is int and type(out['Refused']) is int and out['Accepted']==len(raw_inputs) and out['Refused']==0
 assert out['TargetsRead'] is False and out['Accepted']+out['Refused']==len(out['Actual'])==len(raw_inputs)
 assert len({x['Name'] for x in out['Actual']})==len(out['Actual'])
 old={x['Name']:x for x in baseline['Actual']};assert len(old)==len(baseline['Actual']) and baseline['TargetsRead'] is False
 stats=dict(Parents=0,Children=0,Rows=0,Fragments=0,PointVisits=0,Flat=0,RawChronologyPoints=0)
 for case,source in zip(out['Actual'],raw_inputs,strict=True):
  assert set(source)<= {'Name','Raw','Resolution','id','strokes'}
  assert all(len(p)==2 for stroke in source.get('Raw',source.get('strokes')) for p in stroke)
  allowed={'Name','Status','NetworkBefore','ExportPeak','ExportCopy','ParentBefore','Parent','Variants','ParentAfter','NetworkAfter'}
  if out['GuardMode']:allowed|={'GuardProofs','GuardPrefixes','CategoryMutation'}
  assert set(case)==allowed
  name=source.get('Name',source.get('id'));assert isinstance(name,str);assert case['Name']==name and case['Status']=='Accepted';b7=old[name]
  assert b7['Status']=='Accepted' and case['NetworkBefore']==case['NetworkAfter']==b7['NetworkBefore']==b7['NetworkAfter']
  parent=case['Parent'];assert set(parent)=={'SourceFingerprint','Resolution','Exponent','ScalePrimary','ScaleSecondary','RawBits','RetainedBits','CurrentBits','OriginalPhysical','Chronology','EndingChronology','Descriptors'}
  assert parent['SourceFingerprint'].lower()==profile['SourceFingerprint'] and parent['Exponent']==profile['SourceScoreExponent']==.75
  assert signedbits(parent['Resolution'])==signedbits(source.get('Resolution',0))
  assert case['ParentBefore']==case['ParentAfter']==hash_parent(parent)
  current=parent_source(parent,source);descriptors=[base64.b64decode(x,validate=True) for x in parent['Descriptors']]
  assert all(len(d)==88 for d in descriptors) and len(descriptors)<=511
  assert [v['Residue'] for v in case['Variants']]==[0,1]
  assert (case['ExportPeak'],case['ExportCopy'])==(17825792,65929216)
  for variant,prior in zip(case['Variants'],b7['Variants'],strict=True):
   assert set(variant)=={'Residue','UpstreamBefore','B7Children','RowsCount','Fragments','PointVisits','Peak','CopyWork','VisitWork','Rows','DescriptorsAfter','UpstreamAfter'}
   assert all(type(variant[k]) is int for k in ('Residue','RowsCount','Fragments','PointVisits','Peak','CopyWork','VisitWork'))
   assert variant['UpstreamBefore']==variant['UpstreamAfter']==hash_b7(variant['B7Children']);expecteddesc=[bytearray(x) for x in descriptors]
   assert len(variant['B7Children'])==len(prior['Children']) and [c['Descriptor'] for c in variant['B7Children']]==[c['Descriptor'] for c in prior['Children']]
   expectedrows=[];fragments=visits=0
   for child,previous in zip(variant['B7Children'],prior['Children'],strict=True):
    assert set(child)=={'Descriptor','SoleFlat','Map','RawBits','Owners','Ranges','Words'} and type(child['SoleFlat']) is bool
    assert all(set(o)=={'Key','PackedCount','Records'} and type(o['Key']) is int and type(o['PackedCount']) is int for o in child['Owners'])
    assert all(set(r)=={'Key','Rows'} and type(r['Key']) is int and integer_tree(r['Rows']) for r in child['Ranges']) and all(set(w)=={'Storage','References'} and integer_tree(w['References']) for w in child['Words'])
    assert {k:child[k] for k in ('Descriptor','Owners','Ranges','Words')}=={k:previous[k] for k in ('Descriptor','Owners','Ranges','Words')}
    d=descriptors[child['Descriptor']];first,last=struct.unpack_from('<ii',d,36);assert (d[0]&15)==0 and 0<=first<=last<len(current)
    retained=[]
    for si in range(first,last+1):
     if flat(current[si],parent['ScalePrimary']) and (retained or si!=last):continue
     retained.append(si)
    assert child['Map']==retained and child['RawBits']==[parent['CurrentBits'][i] for i in retained]
    sole=first==last and flat(current[first],parent['ScalePrimary']);assert child['SoleFlat']==sole
    owners={n['Key']:base64.b64decode(n['Records'],validate=True) for n in child['Owners']};assert len(owners)==len(child['Owners'])
    ranges={r['Key']:r['Rows'] for r in child['Ranges']};assert len(ranges)==len(child['Ranges'])
    refs=[];seen=set()
    for wi,word in enumerate(child['Words']):
     for ri,reference in enumerate(word['References']):
      _,arc,selected=decode(reference);k=key(reference);assert k in owners and 0<=selected<len(owners[k])//40
      if arc<0:continue
      assert k not in seen;seen.add(k);refs.append((wi,ri,reference,k,selected))
    assert refs and len(refs)<=4096;begin=len(expectedrows)
    for at,(wi,ri,reference,k,selected) in enumerate(refs):
     isflat=sole and at==len(refs)-1 and profile['UnderscoreLabel']!=65535
     encoded=overwrite_flat(profile) if isflat else base64.b64encode(owners[k]).decode();selected=0 if isflat else selected
     result,projection,count,pv=produce(profile,parent['CurrentBits'],retained,ranges[k],encoded,selected)
     expectedrows.append(dict(Descriptor=child['Descriptor'],Word=wi,Ordinal=ri,Reference=reference,Flat=isflat,Storage=base64.b64encode(result).decode(),LabelCount=count,ChildRanges=ranges[k],Projected=projection));fragments+=len(projection);visits+=pv
     stats['Flat']+=int(isflat)
    struct.pack_into('<ii',expecteddesc[child['Descriptor']],52,begin,len(expectedrows));stats['Children']+=1
   assert all(set(row)=={'Descriptor','Word','Ordinal','Reference','Flat','Storage','LabelCount','ChildRanges','Projected'} and type(row['Flat']) is bool and all(type(row[k]) is int for k in ('Descriptor','Word','Ordinal','Reference','LabelCount')) for row in variant['Rows'])
   assert variant['Rows']==expectedrows and variant['RowsCount']==len(expectedrows)<=4096
   assert variant['Fragments']==fragments<=65536 and variant['PointVisits']==visits<=1048576
   assert variant['DescriptorsAfter']==[base64.b64encode(x).decode() for x in expecteddesc]
   assert (variant['Peak'],variant['CopyWork'],variant['VisitWork'])==(31391744,63569920,54722560)
   stats['Rows']+=len(expectedrows);stats['Fragments']+=fragments;stats['PointVisits']+=visits
  stats['Parents']+=1;stats['RawChronologyPoints']+=sum(map(len,current))
 return stats

raw=read(a.input);raw=raw if isinstance(raw,list) else raw['samples'];baseline=read(a.b7);capture=read(a.capture);profile=read_profile(a.models/'en-US/en_US-ak-cur.res')
metrics=replay(capture,raw,baseline,profile);negatives=[]
if a.tamper:
 def refuses(name,mutate):
  changed=copy.deepcopy(capture);mutate(changed)
  try:replay(changed,raw,baseline,profile)
  except(AssertionError,ValueError,KeyError,IndexError,TypeError):negatives.append(name);return
  raise AssertionError('Undetected tamper '+name)
 c=lambda d:d['Actual'][0];v=lambda d:c(d)['Variants'][0];row=lambda d:v(d)['Rows'][0]
 refuses('extra-parent',lambda d:d['Actual'].append(copy.deepcopy(c(d))))
 refuses('missing-variant',lambda d:c(d)['Variants'].pop())
 refuses('extra-child',lambda d:v(d)['B7Children'].append(copy.deepcopy(v(d)['B7Children'][0])))
 refuses('missing-row',lambda d:v(d)['Rows'].pop())
 refuses('source-coordinate-bit',lambda d:c(d)['Parent']['CurrentBits'][0][0].__setitem__(0,c(d)['Parent']['CurrentBits'][0][0][0]^1))
 refuses('chronology-key',lambda d:c(d)['Parent']['Chronology'][0].__setitem__(2,999))
 refuses('selected-reference',lambda d:row(d).__setitem__('Reference',row(d)['Reference']^(1<<40)))
 refuses('720-byte',lambda d:row(d).__setitem__('Storage',base64.b64encode(bytes([1])+base64.b64decode(row(d)['Storage'])[1:]).decode()))
 refuses('range-count',lambda d:v(d).__setitem__('Fragments',v(d)['Fragments']+1))
 refuses('interval',lambda d:v(d)['DescriptorsAfter'].__setitem__(0,base64.b64encode(bytes(88)).decode()))
 refuses('NN-replay',lambda d:c(d).__setitem__('NetworkAfter',c(d)['NetworkAfter']+1))
 refuses('flat-flag',lambda d:row(d).__setitem__('Flat',not row(d)['Flat']))
report=dict(CaptureSHA256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),RawInputSHA256=hashlib.sha256(a.input.read_bytes()).hexdigest(),B7InputSHA256=hashlib.sha256(a.b7.read_bytes()).hexdigest(),Status='PASS_INDEPENDENT_FULL_SOURCE720',Metrics=metrics,TamperRefusals=negatives,SDKExecuted=False,NNEvaluated=False,TargetsRead=False,Scope='Actual typed upstream order/scale ingress, raw coordinate/key provenance and all720 replay; no lineNN reexecution/nativeABI/finalguide')
a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
