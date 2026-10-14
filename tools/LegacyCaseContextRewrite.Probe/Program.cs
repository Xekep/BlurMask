using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("case-data publication-data controls");
using var cs=File.OpenRead(args[0]);var data=LegacyNativeCaseData.Load(cs);
using var ps=File.OpenRead(args[1]);var publication=LegacyNativeTextPublicationData.Load(ps);
using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));int rows=0,refusals=0;
foreach(var item in json.RootElement.EnumerateArray())
{
 var a=item.GetProperty("alphabet");var alphabet=LegacyInkAlphabet.Load(Convert.FromBase64String(a.GetProperty("payload").GetString()!),a.GetProperty("version").GetUInt16());
 var prep=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(alphabet,data,publication),publication);
 foreach(var row in item.GetProperty("paths").EnumerateArray())
 {
  var contexts=new LegacySymbolContextFactory();ulong root=contexts.ResolveInitial(60000).Identity;
  ulong parent=contexts.ResolveChild(root,60001).Identity;contexts.LinkCreatedContext(parent,root);
  if(row.GetProperty("root").GetBoolean())parent=0;
  ushort[] symbols=row.GetProperty("symbols").EnumerateArray().Select(v=>v.GetUInt16()).ToArray();ulong leaf=parent;
  foreach(ushort symbol in symbols){ulong previous=leaf;var next=leaf==0?contexts.ResolveInitial(symbol):contexts.ResolveChild(leaf,symbol);leaf=next.Identity;if(next.Created && previous!=0)contexts.LinkCreatedContext(leaf,previous);}
  var rewrite=new LegacyNativeCaseContextRewrite(prep,contexts);int before=contexts.NodeCount;
  if(row.GetProperty("refusal").GetBoolean())
  {
   try{rewrite.Rewrite(parent,leaf,row.GetProperty("mode").GetInt32(),row.GetProperty("alternate").GetBoolean());throw new Exception("Uninitialized native row accepted.");}catch(NotSupportedException){}
   if(contexts.NodeCount!=before)throw new Exception("Unsupported rewrite inserted nodes.");refusals++;continue;
  }
  ulong result=rewrite.Rewrite(parent,leaf,row.GetProperty("mode").GetInt32(),row.GetProperty("alternate").GetBoolean());
  var expected=row.GetProperty("expected");
  if(expected.ValueKind==JsonValueKind.Null)
  {if(result!=0 || contexts.NodeCount!=before)throw new Exception("Native32-ancestor bound differs.");}
  else
  {
   var actual=new List<ushort>();ulong cursor=result;
   while(cursor!=parent){var node=contexts.ReadNode(cursor);actual.Add(node.Symbol);cursor=node.ParentLink;if(actual.Count>32)throw new Exception("Rebuilt parent chain broken.");}
   actual.Reverse();if(!actual.ToArray().AsSpan().SequenceEqual(expected.EnumerateArray().Select(v=>v.GetUInt16()).ToArray()))throw new Exception("Suffix symbol remapping differs.");
   int stable=contexts.NodeCount;
   if(rewrite.Rewrite(parent,leaf,row.GetProperty("mode").GetInt32(),row.GetProperty("alternate").GetBoolean())!=result || contexts.NodeCount!=stable)throw new Exception("Repeated lookup changed identity/tree.");
  }
  int snapshot=contexts.NodeCount;
  try{rewrite.Rewrite(parent,leaf,0,false,new(true));throw new Exception("Canceled accepted.");}catch(OperationCanceledException){}
  if(contexts.NodeCount!=snapshot)throw new Exception("Canceled insertion.");
  try{rewrite.Rewrite(parent,leaf,3);throw new Exception("Outside form accepted.");}catch(ArgumentOutOfRangeException){}
  ulong foreign=new LegacySymbolContextFactory().ResolveInitial(1).Identity;
  try{rewrite.Rewrite(foreign,leaf,0);throw new Exception("Foreign context accepted.");}catch(ArgumentException){}
  rows++;
 }
}
Console.WriteLine($"PASS {rows} owned context paths and {refusals} uninitialized-row refusals; exact oldest-first symbols, root/nonroot IDs,32-ancestor bound, foreign-owner and cancellation guards. Full wrapper query remains separate.");
