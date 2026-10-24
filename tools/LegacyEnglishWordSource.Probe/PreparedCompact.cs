using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
internal static class PreparedCompact
{
 internal static void Run(string akPath,string priorPath,string output)
 {
  if(File.Exists(output))throw new IOException("Fresh prepared output required");
  byte[] Read(string p,int max,string sha){if(new FileInfo(p).Length>max)throw new InvalidDataException("Bounded data");var b=File.ReadAllBytes(p);if(Convert.ToHexStringLower(SHA256.HashData(b))!=sha)throw new InvalidDataException("Pinned prepared DATA");return b;}
  byte[] ak=Read(akPath,1024*1024,"ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657");
  byte[] prior=Read(priorPath,4*1024*1024,"bc5a6fd66742ed9e2395b0c7872a58168e67d8b1615c80146563dd46da3abdfc");using var json=JsonDocument.Parse(prior);var compact=json.RootElement.GetProperty("Compact");
  var graph=compact.GetProperty("Graph").EnumerateArray().Select(n=>n.EnumerateArray().Select(a=>(Start:a.GetProperty("Start").GetInt32(),End:a.GetProperty("End").GetInt32())).ToArray()).ToArray();
  int count=compact.GetProperty("Records").GetInt32();byte[] used=compact.GetProperty("Storage").GetBytesFromBase64().AsSpan(0,count*40).ToArray();int[] starts=compact.GetProperty("Starts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
  var al=LegacySource247WireLayout.Read(ak).Alphabet;ushort Label(string s)=>al.TryGetLabel(s,out ushort l)?l:ushort.MaxValue;
  int First(int n,ushort a)=>graph[n][a].Start;int Last(int n,ushort a)=>graph[n][a].End;
  int calls=0;var walker=new LegacyGlobalSpanAlternativeWalkKernel(used,count,graph.Length,Label("\u0003"),Label("\u200d"),Label("\U000f0001"),BinaryPrimitives.ReadUInt16LittleEndian(al.CopySerializedHeader().AsSpan(6)),Label("\u00ad"),new byte[64],First,Last,(request,cursor,ct)=>
  {calls++;var r=LegacyEnglishCaseGraphDelimiterBoundary.Read(request,cursor,graph.Length,(n,a)=>{_=graph[n][a];},ct);return new(r.RemainingCursor,r.Symbols.Length,r.NativeFlag0C,0);});
  int first=0,last=graph[^1].Max(a=>a.End);var dry=new List<object>();
  var scan=LegacyPreparedMainWordAugmentationCandidates.Scan(used,count,starts,first,last,false,First,Last,(i,m)=>{var r=walker.Run(i,m,false,65535,[],default);dry.Add(r);return new(r.Status,r.LastSourceKey);});
  var emitted=scan.CompactIndices.Select(i=>walker.Run(i,0,true,65534,new byte[72],default)).ToArray();
  if(scan.CompactIndices.Length==0||emitted.Any(r=>r.Status is 1 or -1||r.ResidualReferences.Length!=0))throw new InvalidDataException("Prior actual DATA branch requires unsupported work");
  foreach(int i in scan.CompactIndices)if(BinaryPrimitives.ReadInt32LittleEndian(used.AsSpan(i*40+36))!=-1)throw new InvalidDataException("Reached auxiliary payload");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Scope="PREPARED_PINNED_COMPACT_DATA_ONLY_NO_LIVE_EN_OWNER_NO_NN",NN=0,First=first,Last=last,Count=count,Starts=starts,Compact=used,Graph=graph.Select(n=>n.Select(a=>new[]{a.Start,a.End}).ToArray()).ToArray(),Dry=dry,Scan=scan,Emitted=emitted,DelimiterCalls=calls}));
  Console.WriteLine($"PASS prior-DATA scan {scan.CompactIndices.Length} actual-bit8 candidates/{emitted.Sum(r=>r.Rows72.Length/72)} prepared rows, NN0; not a live owner run");
 }
}
