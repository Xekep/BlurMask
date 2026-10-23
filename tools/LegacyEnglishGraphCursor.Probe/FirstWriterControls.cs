using InkRecognition;
using System.Runtime.CompilerServices;
internal static class FirstWriterControls
{
 internal static object Run()
 {
  var cases=new List<object>();
  for(int n=0;n<64;n++){
   var slot=Enumerable.Range(0,128).Select(i=>(byte)(i*37+n*11)).ToArray();var before=slot.ToArray();
   var parent=Enumerable.Range(0,88).Select(i=>(byte)(i*13+n*7)).ToArray();var language=Enumerable.Range(0,20).Select(i=>(byte)(i*23+n*3)).ToArray();
   ulong tag=0x3900000100000000UL+(uint)n;int cost=new[]{-67108864,-1,0,1,67108863}[n%5],ordinal=n%125,previous=-1;
   Access.Write(null,slot,parent,language,tag,cost,ordinal,previous);
   if(!slot.AsSpan(120,8).SequenceEqual(before.AsSpan(120,8)))throw new Exception("Accepted FIRST touchedopaque tail.");
   cases.Add(new{Before=Convert.ToHexString(before),Parent=Convert.ToHexString(parent),Language=Convert.ToHexString(language),Tag=tag,Cost=cost,Ordinal=ordinal,Previous=previous,After=Convert.ToHexString(slot)});
  }
  return new{Scope="PREPARED shared native171-store FIRST arithmetic only; no source/owner/NN",Cases=cases};
 }
 private static class Access
 {
  [UnsafeAccessor(UnsafeAccessorKind.StaticMethod,Name="WriteAcceptedFirstFields")]
  internal static extern void Write(LegacyEnglishCaseNode0Completion? owner,Span<byte> slot,ReadOnlySpan<byte> parent,ReadOnlySpan<byte> language,ulong tag,int cost,int ordinal,int previous);
 }
}
