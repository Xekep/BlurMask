"""Static first B0/B8 context row+column ancestor and identity proof."""
import argparse,hashlib,json,pathlib,re,sys
p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f';s=functions()['1801ef070']
walks=[s[m.start():s.index('local_320 =',m.start())] for m in re.finditer(r'while\( true \)',s)]
assert len(walks)==2
for w in walks:
    for field in ('0xe2','0xe6','0xe0','0xea','0xec','0xee'):assert field in w
    assert '>> 1' in w and '& 7' in w and '0xffffffff' in w
    assert '+ 10 + lVar28 * 0x28' in w and '+ 9 + lVar28 * 0x28' in w
    assert w.index('0xffffffff')<w.index('+ 10 + lVar28')
#Blank current uses its primary98 previous-alternative, rather than all prior
#primary rows, in both independently allocated context tables.
assert "if ((uVar47 != *(ushort *)(param_2 + 0xe2)) ||" in s
assert '(local_274 ==' in s and '(local_264 ==' in s
assert 'local_2b0 == 0' in s
assert 'uStack_20c = CONCAT31(uStack_20c._1_3_,5)' in s
#B0 has E2-only identity sharing; B8 extends that guard to E4.
guards=[s[m.start():m.start()+900] for m in re.finditer(re.escape('if (uVar47 == *(ushort *)(param_2 + 0xe2)) {'),s)]
assert sum('local_308 = *(longlong **)' in g and 'param_2 + 0xb0' in g for g in guards)==1
both=s.index('if ((uVar47 == *(ushort *)(param_2 + 0xe2)) ||',s.index('param_2 + 0xb8'))
assert 'uVar47 == *(ushort *)(param_2 + 0xe4)' in s[both:both+150]
assert 'local_308 = *(longlong **)' in s[both:both+900]
assert 'uVar47 != *(ushort *)(param_2 + 0xe6)' in s[s.index('while( true )',s.index('while( true )')+1):]
#Reverse callback order belongs to the context passes, not primary1EE3B0.
for start in (s.index('while( true )'),s.index('while( true )',s.index('while( true )')+1)):
    tail=s[start:];assert tail.index('+ 0x140')<tail.index('+ 0x118')
assert 'local_2a0 = uVar47' in s
report={'ancestor':'B0/B8 traverse their OWN contexttables using packedspan,byte9 primaryrow,byteA contextalternative; rootstopbefore255; no primary-only ancestry substitution',
 'owners':'Originalcontextparent keeps F0/F8 arc/node and accumulated costs; geometriccontextancestor supplies sampler bounds/category/symbol',
 'blank':'CurrentE2 restricts original primary-row selection to98.previous9; selectedsourceB8 additionally blocks consecutiveblank whenenabled; E2/shadda cannot useB8kind2',
 'identity':'B0kind0 E2 sharesoriginalidentity;B8kind0 E2/E4 sharesoriginalidentity; regularextends, B0kind2resets, B8kind2E0false insertsFFFB',
 'scope':'Ownedfreshfirst zero-trait E0false/aliasfold0; synthetic costoverride INTERNALfriendonly, actualpublic entry owns modelproducer andcostquery'}
out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report)+'\n',encoding='utf-8');print(json.dumps(report))
