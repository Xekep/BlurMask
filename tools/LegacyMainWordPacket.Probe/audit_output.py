"""Independent21FE30 byte stores over explicit prepared packet/WORD inputs."""
import json,struct,sys
from pathlib import Path
p=json.loads(Path(sys.argv[1]).read_bytes())
fields=[(0,16,4),(4,24,4),(8,28,4),(12,48,2),(14,50,2),(16,53,1),(17,54,1),
 (20,60,4),(28,68,4),(32,72,4),(36,76,4),(40,80,4),(44,55,1)]
for row in p['rows']:
 packet=bytes.fromhex(row['packet']);word=bytearray.fromhex(row['word'])
 for a,b,n in fields:word[b:b+n]=packet[a:a+n]
 assert row['result']==word.hex().upper()
 assert row['previous']==struct.unpack_from('<H',packet,46)[0]
 assert [row['first'],row['last']]==list(struct.unpack_from('<ii',packet,48))
 assert [row['hint38'],row['hint3c']]==list(struct.unpack_from('<II',packet,56))
assert len(p['rows'])==256 and p['guards']==2
print(f'PASS independent256 prepared64/120-byte21FE30 packets, all13 field stores and untouched bytes; actualsourceFormat={p["sourceFormat22F9"]}; not outerpublication')
