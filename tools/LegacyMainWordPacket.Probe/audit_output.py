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
assert len(p['rows'])==256 and p['guards']==6
def reset(word):
 word=bytearray(word)
 for offset,value,fmt in [(8,2**64-1,'Q'),(16,0x3f800000,'I'),(20,0x3f800000,'I'),
  (24,0,'I'),(28,0,'I'),(40,0,'I'),(48,65534,'I'),(53,0,'H'),(55,0,'B'),
  (60,2**32-1,'I'),(84,2**32-1,'I'),(96,0,'I'),(112,0,'I')]:
  struct.pack_into('<'+fmt,word,offset,value)
 return word
for row in p['ownedWords']:
 assert reset(bytes.fromhex(row['residue'])).hex().upper()==row['initial']
 applied=bytearray.fromhex(row['before']);packet=bytes.fromhex(row['packet'])
 for a,b,n in fields:applied[b:b+n]=packet[a:a+n]
 assert row['applied']==applied.hex().upper()
 assert row['reset']==reset(applied).hex().upper()
 assert bytes.fromhex(row['reset'])[68:84]==applied[68:84]
assert len(p['ownedWords'])==64
print(f'PASS independent256 prepared21FE30 packets/64 actual-main-pool words, allfield stores/reset/preserved metadata; actualsourceFormat={p["sourceFormat22F9"]}; not outerpublication')
