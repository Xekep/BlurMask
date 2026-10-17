"""Read-only pinned PE audit of the fresh configured2 result-loop branch."""
import hashlib, json, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180227570,0x180227750,'77296335d3c5e85e886cfb0d8dd4b0da783bf0496ec8bead4a76f9f0b2c46134'),
    (0x180227859,0x18022795d,'af21ef68d983df9f78d32e0c4d00c7f9c9b4078c5633b19b87bfaf8116e95efe'),
    (0x18000c860,0x18000c92c,'1dedd3f4d56fb9d0cadeda2b20488e85d966ec10a301514a078d0e6da558029e'),
    (0x18000d800,0x18000d890,'e4a1137d11b2a65d4dcbc778f1511759963ab10e17c376eb226a15a1ac71cc91'),
    (0x18021f337,0x18021f365,'90092af3b84961da8fed4ef74ff5a59237236e9499c3917e2dc65b9e0e2abf9e'),
    (0x180221935,0x1802219b2,'270a96cf41d9a5f1d2f65d5f8474ac1c52cd1173fad07b6463228c8e8577abaa'),
    (0x18020c49e,0x18020c4bd,'fdf5239ba0d87b06494c3990f7e13cb18dc538d6eec62b38c933455e53ac8e17'),
]
for a,z,h in regions: assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h,hex(a)
pins = [
    (0x180227570,'410fb79e04240000'), #actual native secondary limit
    (0x18022758a,'0f94c1'), #first output-index, not active vector count
    (0x180227593,'3b45a8'), #actual exclusion capacity
    (0x180227603,'e8b8e1ffff'),
    (0x18022760c,'81fa00000004'),
    (0x180227626,'81c100000004'),
    (0x180227634,'4585ff'),
    (0x18022763d,'4438a099000000'),
    (0x180227646,'4438a0a8000000'),
    (0x180227655,'48895778'),
    (0x18022787f,'4538a640250000'), #optionalAA equality preparation guard
    (0x1802278e5,'41ffd2'), #native279 plain equality
    (0x180227907,'41ffc7'), #normal next output index
    (0x180227937,'41ffd0'), #279 destructor, shared275 is not destroyed
    (0x18022793e,'66ff4f60'),
    (0x180227942,'41ffcf'), #duplicate restores the same output index
    (0x180227949,'41ffc7'),
    (0x180227953,'e918fcffff'),
    (0x180227958,'443bfb'), #no winner still advances below the limit
    (0x18000c8cb,'48898798000000'),
    (0x18000c8d9,'488987a4000000'),
    (0x18000c923,'668987f4000000'),
    (0x18000d83a,'66894318'), #the ONLY accepted property writes short18
    (0x18021f344,'80b8f400000000'),
    (0x18021f34b,'0f8488080000'),
    (0x180221935,'e896e6ffff'),
    (0x18022196f,'e85c330000'), #conditional repair is not merely source2300
    (0x18022198e,'e85de2ffff'), #quotes before F4 normalization
    (0x180221997,'4180ba9900000000'),
    (0x1802219ad,'e86ed9ffff'),
    (0x18020c49e,'4c8d059b040e00'),
    (0x18020c4ab,'e8c0cae0ff'),
    (0x18020c4b6,'668983fa240000'),
    (0x1802ec940,'e280ab00'), #U+202B, absent from the checked acoustic alphabet
]
for a,raw in pins: assert b.data[b.off(a):b.off(a)+len(raw)//2].hex()==raw,hex(a)
report = dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions], pins=len(pins),
    noVendorExecution=True, scope='fresh known-property18 configured2, source135/no-associated, EC0/no-rescue, ordinary callbacks, absent U+202B24FA, zero99/A8/F4/AA',
    excludes=['enabled224CD0 repair','enabled21F320','21D7A0 detailed equality','rescue','external6000 callbacks','app acceptance'])
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS {len(regions)} whole PE regions/{len(pins)} pins, native counters/reference/duplicate release and fresh property-only policy.')
