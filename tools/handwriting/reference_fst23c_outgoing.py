"""Independent literal float32 MAIN135 cost table and13A.V80 outgoing primitive.

This is not28C preparation, label mapping, or whole-string recognition.
"""
import struct
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
def frombits(v):return struct.unpack('<f',struct.pack('<I',v))[0]
def actual_main135_costs(weight_bits=0x3f800000):
 weight=frombits(weight_bits);multiplier=f32(1.0*weight)
 bias=f32(multiplier*frombits(0x3fc00000));scale=f32(multiplier*frombits(0x3e19999a))
 middle=f32(-bias/scale);costs=[0]*256
 costs[0]=int(f32(f32(-frombits(0xc1980000))*-8192.0))
 def interpolated(t,left,right):
  value=f32(f32(f32(1.0-t)*left)+f32(t*right))
  return int(f32(f32(f32(value*scale)+bias)*-8192.0))
 for signed in range(-127,0):
  costs[signed+128]=interpolated(f32((signed+127)/127.0),frombits(0xc0400000),middle)
 costs[128]=0
 for signed in range(1,128):
  costs[signed+128]=interpolated(f32(signed/127.0),middle,frombits(0xc1880000))
 return costs
def outgoing(descriptors,words,state,costs):
 start,end=descriptors[state]['field0'],descriptors[state+1]['field0'];rows=[]
 for ordinal,target in enumerate(words[start:end]):
  row=descriptors[target];raw=row['runtimeByte6'];signed=raw if raw<128 else raw-256
  rows.append(dict(ordinal=ordinal,targetState=target,labelId=row['runtimeLabel16'],cost=0 if costs is None else costs[signed+128],targetByte6Signed=signed,targetByte7=row['runtimeByte7']))
 return dict(state=state,outputFlag=0,rows=rows)
