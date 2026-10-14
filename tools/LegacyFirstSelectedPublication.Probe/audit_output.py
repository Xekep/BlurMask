"""Independent outer-allocation/root-publication/scalar/reference oracle.
Prepared selected-path fixtures are not evidence that the NN/geometry selected
those blank paths: only the four non-synthetic captures are actual producers.
"""
import json,pathlib,struct,sys
captures=json.loads(pathlib.Path(sys.argv[1]).read_text(encoding='utf-8'))
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def i32(b,p):return struct.unpack_from('<i',b,p)[0]
def fl(b,p):return struct.unpack_from('<f',b,p)[0]
actual=synthetic=suppressed=unpublished_with_glyph=0
for c in captures:
    if c['synthetic']:synthetic+=1
    else:actual+=1
    hidden={c['blank'],c['closed']}
    emitted=[v for v in c['trace'] if c['configuration99'] or v[2] not in hidden]
    suppressed+=len(c['trace'])-len(emitted)
    root_allocates=c['configuration99'] or c['trace'][0][2] not in hidden
    assert c['hasWord']==bool(emitted and root_allocates)
    assert len(c['endpoints'])==(2 if root_allocates else 1)
    assert len(c['glyphs'])==len(emitted)
    unpublished_with_glyph+=bool(emitted and not c['hasWord'])
    for t,g in zip(emitted,c['glyphs']):
        node,arc,symbol,row,column,likelihood,end=t;b=bytes.fromhex(g['record'])
        assert len(b)==40 and g['key']==node|(arc<<24)
        assert i32(b,0)==row+column and u16(b,8)==symbol
        assert u16(b,4)&7==0 and u16(b,6)&0x40
        assert fl(b,12)==f32(likelihood/255)
    for h in c['endpoints']:
        b=bytes.fromhex(h);assert len(b)==40 and i32(b,0)==-2
        assert u16(b,4)&7==1 and u16(b,8)==1 and u16(b,6)&0x40
        assert fl(b,12)==1 and fl(b,16)==1
    multiplier=5 if c['sourceKind']==0x135 else 1
    #These diagnostics use the native unadjusted word-scalar formula. With no
    #published word they are NOT serialized to a nonexistent WORD275 owner.
    assert f32(c['wordScalar10'])==f32(f32(-multiplier*f32(c['selectedCost']))*f32(0.00012207031))
    assert f32(c['wordScalar18'])==f32(f32(c['selectedCost'])/f32(c['nodeCount']<<13))
    assert f32(c['wordScalar1c'])==f32(f32(c['acousticCost'])*f32(0.00012207031))
    if c['hasWord']:
        assert c['spanFirst']==c['trace'][-1][6] and c['spanLast']==emitted[-1][6]
        word=bytes.fromhex(c['word']);assert fl(word,16)==f32(c['wordScalar10']) and fl(word,20)==1
        assert fl(word,24)==f32(c['wordScalar18']) and fl(word,28)==f32(c['wordScalar1c'])
assert actual==4 and synthetic==24 and suppressed==95 and unpublished_with_glyph==4
real=[c for c in captures if not c['synthetic']]
assert [(c['selectedCost'],[t[2] for t in c['trace']]) for c in real]==[(41268,[22,70,25]),(12111,[134,191]),(14219,[24]),(29007,[18,18,18])]
assert [len(c['glyphs']) for c in real]==[3,1,1,3]
print(f'PASS independent publication oracle: {actual} actual +{synthetic} INTERNAL paths, {suppressed} skipped allocations, {unpublished_with_glyph} hidden-root/unpublished glyph pools.')
