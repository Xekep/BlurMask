"""Independent fresh ordinary raw-kind policy and literal01FD40 metadata.
The source class comes from the proved sealed C49 constructor/clone lineage,
not point counts, source bounds, NN output or an expected recognized label.
"""
def lookup_needed(flags,selection,ranges):
 if flags&0x10600000:return False
 if flags&0x20000000:
  if selection!=-1:raise ValueError('Actually used prior V128 is outside fresh authority')
  return False
 if len(ranges)!=1:return False
 first,last=ranges[0]
 return first>>32==last>>32 and(first>>16&65535)==0 and(first>>8&255)==0 and(last>>16&65535)==3 and(last>>8&255)==255

def metadata(flags,selection,ranges,counts):
 if not lookup_needed(flags,selection,ranges):return flags,None
 source=ranges[0][0]>>32
 assert 0<=source<len(counts)
 concrete=0xc49 # actual library default typed factory, fixed by static proof.
 reported=0xc1e # actual094940 C49 switch destination, not concrete type.
 return(flags&0xfeffffff)|(0x01000000 if reported==0xc25 else 0),dict(source=source,concrete=concrete,reported=reported)
