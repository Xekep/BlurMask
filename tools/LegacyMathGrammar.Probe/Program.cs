using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("REPOSITORY CAPTURE");
byte[] grammar=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-grm-standard.res"));
byte[] acoustic=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var owner=LegacyMathGrammarResource.LoadActual(grammar,acoustic);
var guards=new List<string>();
void Guard(string name,Action action,Type expected)
{ try {action();throw new Exception("Missing refusal: "+name);}catch(Exception e){if(e.GetType()!=expected)throw;guards.Add(name);} }
void Bad(byte[] g,byte[] a,string name)=>Guard(name,()=>LegacyMathGrammarResource.LoadActual(g,a),typeof(InvalidDataException));
var changedGrammar=(byte[])grammar.Clone();changedGrammar[24]^=1;Bad(changedGrammar,acoustic,"grammar-fingerprint");
var changedAcoustic=(byte[])acoustic.Clone();changedAcoustic[1005996]^=1;Bad(grammar,changedAcoustic,"acoustic-fingerprint");
Guard("grammar-length",()=>LegacyMathGrammarResource.LoadActual(grammar.AsSpan(1),acoustic),typeof(ArgumentException));
Guard("acoustic-length",()=>LegacyMathGrammarResource.LoadActual(grammar,acoustic.AsSpan(1)),typeof(ArgumentException));
using var cts=new CancellationTokenSource();cts.Cancel();
void Cancel(string name,Action action)
{try{action();throw new Exception("Missing cancellation");}catch(OperationCanceledException e){if(e.CancellationToken!=cts.Token)throw;guards.Add(name);}}
Cancel("load-cancel-token",()=>LegacyMathGrammarResource.LoadActual(grammar,acoustic,cts.Token));
Cancel("write-cancel-token",()=>owner.WriteSerialized(cts.Token));
Guard("rules-immutable",()=>((IList<LegacyMathGrammarResource.Rule>)owner.Rules).Clear(),typeof(NotSupportedException));
var terminal=owner.Rules.SelectMany(r=>r.Children).First(n=>n.TerminalLabels.Count>0);
Guard("terminal-labels-immutable",()=>((IList<ushort>)terminal.TerminalLabels).Clear(),typeof(NotSupportedException));
byte[] serialized=owner.WriteSerialized();
if(!serialized.AsSpan().SequenceEqual(grammar))throw new Exception("Parsed writer differs from actual resource");
Array.Clear(grammar);Array.Clear(acoustic);
if(!owner.WriteSerialized().AsSpan().SequenceEqual(serialized))throw new Exception("Borrowed source retained");guards.Add("input-detached");
var copy=owner.WriteSerialized();Array.Clear(copy);
if(!owner.WriteSerialized().AsSpan().SequenceEqual(serialized))throw new Exception("Writer output retained");guards.Add("output-detached");
if(owner.TryGetTerminalText(256,out _))throw new Exception("Virtual alphabet label invented");guards.Add("virtual-label-has-no-text");
foreach(var child in owner.Rules.SelectMany(r=>r.Children))
for(int i=0;i<child.TerminalNames.Count;i++)
if(!owner.TryGetTerminalText(child.TerminalLabels[i],out var text)||text!=child.TerminalNames[i])throw new Exception("Terminal text mismatch");
guards.Add("all-terminal-labels-roundtrip");
if(File.Exists(args[1]))throw new IOException("Fresh capture required");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{grammarSHA256=Convert.ToHexString(SHA256.HashData(serialized)).ToLowerInvariant(),
 serializedHex=Convert.ToHexString(serialized).ToLowerInvariant(),owner.AlphabetCount,owner.HasCandidateAuthority,
 owner.TrailingScalar38,rules=owner.Rules,guards,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase})+"\n");
Console.WriteLine($"PASS {owner.Rules.Count} rules / {owner.Rules.Sum(r=>r.Children.Count)} children / {guards.Count} guards / 0 model executions");
