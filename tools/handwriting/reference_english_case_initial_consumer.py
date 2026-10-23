"""STATIC census of proved first-query rows, not an owning consumer/decoder.

No target input, vendor execution or promotion of historical pages to authority.
The raw alphabet reference is independent of the C# snapshots.
"""
from pathlib import Path
import argparse, collections, hashlib, json, runpy, struct, sys

p=argparse.ArgumentParser()
p.add_argument('--repo',type=Path,required=True)
p.add_argument('--models',type=Path,required=True)
p.add_argument('--capture',type=Path,required=True)
p.add_argument('--output',type=Path,required=True)
p.add_argument('--categories',type=Path,required=True)
p.add_argument('--geometry',type=Path,required=True)
a=p.parse_args()
sys.argv=['raw-case',str(a.models),str(a.output.with_name(a.output.stem+'-raw.json'))]
r=runpy.run_path(str(a.repo/'tools/LegacyEnglishCaseMain.Probe/reference_case_main.py'),run_name='__main__')
ac=r['ac']; text=ac['text']; reverse={v:k for k,v in text.items()}
special={reverse[x]:x for x in ["'",'"','\U000f0002','\U000f0003','\U000f0004','\U000f0008'] if x in reverse}
excluded={reverse[x]:x for x in ['\U000f0001','\u0651'] if x in reverse}
bypass={reverse[x] for x in ['\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008'] if x in reverse}
zwnj=reverse.get('\u200c',65535);apostrophe=reverse["'"]
def quote_rewrite(current,parent_symbol,parent_parent_symbol):
 return current==apostrophe and parent_symbol==zwnj and parent_parent_symbol==0xfffa
#Separately PREPARED structural witness ensures no blanket symbol-only false.
#The absent actual ZWNJ resolvesFFFF; this artificial path is NOT native-ready.
assert quote_rewrite(apostrophe,zwnj,0xfffa)
assert not quote_rewrite(apostrophe,0xfffa,None)
cap=json.loads(a.capture.read_bytes());assert cap['TargetsRead'] is False
category_bytes=a.categories.read_bytes()
assert hashlib.sha256(category_bytes).hexdigest()=='82ca0232e4005fc352f92c830b13bef5765b212619448ff49634ca14aa78502d'
categories=json.loads(category_bytes)
geometry={x['Name']:dict(x['Groups']) for x in json.loads(a.geometry.read_bytes())}
numeric=collections.Counter(); numeric_digest=hashlib.sha256()
wrap=lambda x:((x+2**31)%2**32)-2**31
owners=pages=records=root_leaves=grafted_leaves=terminal=0
flags=collections.Counter();states=collections.Counter();symbols=collections.Counter();special_rows=collections.Counter();excluded_rows=collections.Counter();lengths=collections.Counter()
quote_ancestry_false=0
digest=hashlib.sha256();by_owner=[]
for owner in cap['Rows']:
 if 'Pages' not in owner:continue
 owners+=1; nrows=0; nodes={x[0]:x for x in owner['Nodes']}
 fffa=next(x for x in nodes.values()if x[5]==0xfffa and x[1]==0)
 #Snapshot field order: id,parent,childrenRoot,left,right,symbol,balance,page,slot.
 assert len(nodes)==len(owner['Nodes'])
 skip=0
 for page in owner['Pages']:
  pages+=1;raw=bytes.fromhex(page['Records']);assert len(raw)==32*page['Count'] and page['Skip']==skip
  for offset in range(0,len(raw),32):
   row=raw[offset:offset+32];tag,context=struct.unpack_from('<QQ',row);base,symbol,length,flag=struct.unpack_from('<iHBB',row,16)
   assert (tag>>32)&255==1 and symbol<217 and context in nodes
   assert length==0 and flag==0
   leaf=nodes[context];assert leaf[5]==symbol and symbol<0xfff7
   if leaf[1]==0:root_leaves+=1
   else:
    prefix=nodes[leaf[1]];assert prefix[5]==0xfffa and prefix[1]==0
    grafted_leaves+=1
   assert row[24:]==bytes(8)
   flags[flag]+=1;lengths[length]+=1;states[(tag>>56)&255]+=1;symbols[symbol]+=1
   terminal+=bool((tag>>40)&2)
   if symbol in special:special_rows[special[symbol]]+=1
   if text[symbol] in ("'",'"'):
    #Actual grafted form has a root FFFA parent with NULL parent. Native
    #1FD240's apostrophe rewrite needs FFFA -> ZWNJ -> apostrophe, while
    #its other deep rewrite additionally requires frontend280=true.
    assert leaf[1]==0
    ancestor=nodes[fffa[1]][5] if fffa[1]else None
    assert not quote_rewrite(symbol,fffa[5],ancestor)
    quote_ancestry_false+=1
   if symbol in excluded:excluded_rows[excluded[symbol]]+=1
   #PREPARED numerical projection over observed rows only. It issues no
   #request/heap/source authority and is not a native execution comparison.
   if symbol not in excluded:
    if any(s['Costs'][symbol]<0x4000000 for s in owner['Selected']):
     numeric['qualifiedRows']+=1
     for variant in range(2):
      composite=categories['startComposite' if variant==0 else 'endComposite'][symbol]
      if composite<0:
       assert variant==1;numeric['absentAlternate']+=1;break
      index=categories['compositePairs'][composite][1];assert 0<=index<24
      for selected in owner['Selected']:
       numeric['scoringCalls']+=1;classcost=selected['Costs'][symbol]
       group_geometry=geometry[owner['Name']][selected['Group']]
       if classcost==0x4000000:total=0x4000000;accepted=False
       else:total=wrap(base+classcost+(0 if symbol in bypass else group_geometry));accepted=total<0x4000000
       frontend=wrap(total-base) if accepted else 0x4000000
       numeric['accepted' if accepted else 'rejected']+=1
       numeric_digest.update(struct.pack('<Q8i',tag,symbol,variant,composite,selected['Ordinal'],selected['Group'],total,frontend,int(accepted)))
    else:numeric['unqualifiedRows']+=1
   digest.update(row);records+=1;nrows+=1
  skip+=page['Count']
 assert skip==owner['ConsumedRows'] and owner['InitialTag']==0x0000080000000000
 by_owner.append({'name':owner['Name'],'rows':nrows,'pages':len(owner['Pages']),'selected':len(owner['Selected'])})
result={'status':'PASS STATIC census only','captureSha256':hashlib.sha256(a.capture.read_bytes()).hexdigest(),
 'targetsRead':False,'vendorExecuted':False,'queryAuthorityIssued':False,'wordSelectionClaim':False,
 'owners':owners,'pages':pages,'records':records,'rootLeaves':root_leaves,'existingFFFAChildren':grafted_leaves,
 'terminalTagRows':terminal,'flags':dict(sorted(flags.items())),'lengths':dict(sorted(lengths.items())),
 'states':dict(sorted(states.items())),'specialRows':dict(special_rows),'excludedRows':dict(excluded_rows),
 'quoteAncestryFalseActualRows':quote_ancestry_false,
 'preparedQuoteAncestryControls':2,
 'actualZwnjAbsent':zwnj==65535,'preparedQuoteControlsNativeReady':False,
 'preparedFrontendProjection':dict(numeric),'preparedFrontendDigest':numeric_digest.hexdigest(),
 'geometrySha256':hashlib.sha256(a.geometry.read_bytes()).hexdigest(),
 'categoriesSha256':hashlib.sha256(category_bytes).hexdigest(),
 'symbols':[{ 'symbol':s,'text':text[s],'rows':n} for s,n in sorted(symbols.items())],
 'orderedRowSha256':digest.hexdigest(),'ownerRows':by_owner}
a.output.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k not in ('symbols','ownerRows')},ensure_ascii=True))
