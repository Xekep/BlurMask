"""Static native279 comparison/list lifetime and TOP-owned275 interning."""
import hashlib, json, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x1801421b0,0x180142630,'56d0b96f67bb175dc92528c5785b01ce1630267c3e2b9f8c8271945e6718d68b'),
    (0x1801415b0,0x180141622,'5e7cc43776f462523b6cff2a1851ce5eb0b6b26250a162379242c0ba5e53777f'),
    (0x180227859,0x180227958,'8f665e94ec1b0223756eed2d491f8f20caffb60a3d7f431b0b7466de7d3c187b'),
    (0x180141743,0x1801417c2,'af354006a9845ce051d434d4478faac6007bd72604f930dbfb9b621482ca58dd'),
    (0x18013bd40,0x18013be80,'5103c015c3c9c0f02ed73c2a954c02f3c807b05bd24bba887f8cffb66cd74dfc')]
for start,end,sha in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==sha, hex(start)
pins = [
    (0x1801421d4,'488b7a18'),              #279.Head18
    (0x180142206,'443b7528'),              #275.count28, before advancingword
    (0x18014220c,'488b7f20'),              #279.Next20
    (0x180142253,'4885ff'),                #end BEFORE skips
    (0x180142320,'837d3cff'),              #word3C conditionalFFFC
    (0x18014234c,'837e3cff'),
    (0x180142373,'bafaff0000'),            #FFFA
    (0x1801423b2,'0fb6542432'),            #scalaralternate
    (0x1801423ca,'410fb64506'),            #glyphbyte6
    (0x1801423d4,'a840'),                  #onlybit40
    (0x180142427,'837c0a6000'),            #auxcount60 unsignednonzero
    (0x18014242e,'837c0a5400'),            #auxindex54 signed>=0
    (0x18022787f,'4538a640250000'),        #optional21D7A0 frontier
    (0x1802278e1,'4c8b5028'),              #279.V28
    (0x1802278ea,'7527'),                  #equal =>destruction
    (0x18022793e,'66ff4f60'),              #activecount--
    (0x180227942,'41ffcf'),                #outputordinal compensation
    (0x1801415d0,'488b4320'),              #destructor traverses279nodes
    (0x1801415d7,'48894618'),              #clears279.Head progressively
    (0x1801415df,'488b5310'),              #freesnodeauxvector, notpooledWORD
    (0x180141773,'488bd7'),                #278 owner argument=TOP273 RDI
    (0x18014177e,'4c8b5028'),              #278.V28 returnsTOPsharedspan
    (0x18014179a,'488d5018'),              #sharedspan275vector18
    (0x1801417b9,'4c8b11'),                #275 interning call
]
for address,raw in pins:
    assert b.data[b.off(address):b.off(address)+len(raw)//2].hex()==raw,hex(address)
assert b.qwords(0x1802dcbd0+0x28,1)[0]==0x1801421b0
assert b.qwords(0x1802dcbc0,1)[0]==0x1801415b0
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=h) for s,e,h in regions],pins=len(pins),
    type279='V28=1421B0, destructor1415B0',noVendorExecution=True,
    covered='plain comparison, sameTOP interning and prepared postcallback append/duplicate discard',
    excluded=['all-active auxiliary vectors','21D7A0 detailed comparison','native second-terminal inference',
              '2257C0 exclusions/rescue','full2271F0 allocation/failure ordering','acceptance confidence'])
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS{len(regions)} native regions/{len(pins)} pins/two279 method bindings; static reads only.')
