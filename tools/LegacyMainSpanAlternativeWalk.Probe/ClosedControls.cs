using InkRecognition;
using System.Buffers.Binary;
static class ClosedControls
{
 internal static void Run(string path)
 {
  byte[] output=new byte[786432*3];int position=0;
  foreach(ushort closed in new ushort[]{191,65535})foreach(byte flags in new byte[]{0,16})foreach(int prior in new[]{0,1,0xff00})
   for(int symbol=0;symbol<65536;symbol++)
   {
    int state=prior;bool accepted=LegacyOwnedMainSpanAlternativeWalk.AdvanceClosedLabel((ushort)symbol,closed,flags,ref state);
    output[position++]=(byte)(accepted?1:0);BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(position),(ushort)state);position+=2;
   }
  if(position!=output.Length)throw new InvalidDataException("Closed numeric control count differs.");
  File.WriteAllBytes(path,output);Console.WriteLine("PASS786432 exhaustive prepared F0001/FFF6 carry transitions; not actual source candidates.");
 }
}
