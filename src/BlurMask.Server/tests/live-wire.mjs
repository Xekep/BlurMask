export const protocol = 'blurmask.live.v2';
const uuid = bytes => { const h=bytes.toString('hex'); return `${h.slice(0,8)}-${h.slice(8,12)}-${h.slice(12,16)}-${h.slice(16,20)}-${h.slice(20)}`; };
export function encodeDelta(frame) {
  const points=frame.mark.points, bytes=Buffer.alloc(42+points.length*16);
  bytes.set([66,77,2,1]); Buffer.from((frame.peer || '00000000-0000-0000-0000-000000000000').replaceAll('-',''),'hex').copy(bytes,4);
  Buffer.from(frame.mark.id.replaceAll('-',''),'hex').copy(bytes,20);
  bytes.writeInt32LE(frame.offset,36);bytes.writeUInt16LE(points.length,40);
  for(let i=0;i<points.length;i++){bytes.writeDoubleLE(points[i].x,42+i*16);bytes.writeDoubleLE(points[i].y,50+i*16);}
  return bytes;
}
export function decodeDelta(bytes, metadata) {
  if(bytes.length>=4&&bytes[0]===66&&bytes[1]===77&&bytes[2]===2&&bytes[3]===2) {
    if(bytes.length<24)throw new Error('Truncated saved patch');
    const length=bytes.readInt32LE(20);
    if(length<=0||length>bytes.length-24)throw new Error('Invalid saved metadata');
    const patch=JSON.parse(bytes.subarray(24,24+length).toString());let offset=24+length;
    for(const mark of [...patch.add,...patch.update]) {
      if(offset+2>bytes.length)throw new Error('Missing saved points');
      const count=bytes.readUInt16LE(offset);offset+=2;
      if(count<1||count>20000||offset+count*16>bytes.length)throw new Error('Invalid saved points');
      mark.points=Array.from({length:count},()=>{const point={x:bytes.readDoubleLE(offset),y:bytes.readDoubleLE(offset+8)};offset+=16;return point;});
    }
    if(offset!==bytes.length)throw new Error('Trailing saved bytes');
    return {type:'changed',baseRevision:Number(bytes.readBigInt64LE(4)),revision:Number(bytes.readBigInt64LE(12)),patch};
  }
  if(bytes.length<42||bytes[0]!==66||bytes[1]!==77||bytes[2]!==2||bytes[3]!==1)throw new Error('Invalid binary signature');
  const count=bytes.readUInt16LE(40), id=uuid(bytes.subarray(20,36)), mark=metadata.get(id);
  if(!mark||count<1||count>512||bytes.length!==42+count*16)throw new Error('Invalid binary length/style');
  const points=Array.from({length:count},(_,i)=>({x:bytes.readDoubleLE(42+i*16),y:bytes.readDoubleLE(50+i*16)}));
  return {type:'preview',peer:uuid(bytes.subarray(4,20)),offset:bytes.readInt32LE(36),mark:{...mark,points}};
}
