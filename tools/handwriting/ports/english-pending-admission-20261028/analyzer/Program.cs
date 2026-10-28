using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
var root=args[0];var output=args[1];var trees=Directory.GetFiles(root,"*.cs").Order().Select(p=>CSharpSyntaxTree.ParseText(File.ReadAllText(p),new CSharpParseOptions(LanguageVersion.Preview).WithPreprocessorSymbols(args.Length>3?new[]{"BLURMASK_PUBLICATION_DIAGNOSTICS"}:Array.Empty<string>()),p)).ToList();
if(args.Length>3)foreach(var path in Directory.GetFiles(@"C:/Users/Xekep/Documents/GitHub/BlurMask/tools/LegacyGlobalTextPublication.ScoreDiagnostic/Source","*.cs"))trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(path),new CSharpParseOptions(LanguageVersion.Preview).WithPreprocessorSymbols("BLURMASK_PUBLICATION_DIAGNOSTICS"),path));
trees.Add(CSharpSyntaxTree.ParseText("global using System;global using System.Collections.Generic;global using System.IO;global using System.Linq;global using System.Net.Http;global using System.Threading;global using System.Threading.Tasks;",new CSharpParseOptions(LanguageVersion.Preview).WithPreprocessorSymbols(args.Length>3?new[]{"BLURMASK_PUBLICATION_DIAGNOSTICS"}:Array.Empty<string>())));
var refs=Directory.GetFiles(@"C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/10.0.12/ref/net10.0","*.dll").Select(p=>MetadataReference.CreateFromFile(p));
var c=CSharpCompilation.Create("Review",trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,allowUnsafe:true,nullableContextOptions:NullableContextOptions.Enable));
var errors=c.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).Select(d=>d.ToString()).ToArray();
if(args.Length>2){File.WriteAllText(output,JsonSerializer.Serialize(new{SourceFiles=trees.Count-1,Errors=errors},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine($"Sources={trees.Count-1} Errors={errors.Length}");return;}
var edges=new SortedDictionary<string,SortedDictionary<string,SortedSet<string>>>();
foreach(var tree in trees.Where(t=>t.FilePath!="")){
 var from=Path.GetFileName(tree.FilePath);var destinations=new SortedDictionary<string,SortedSet<string>>();var model=c.GetSemanticModel(tree);
 foreach(var node in tree.GetRoot().DescendantNodes().Where(n=>n is IdentifierNameSyntax or GenericNameSyntax)){
  var symbol=model.GetSymbolInfo(node).Symbol;if(symbol is null or INamespaceSymbol)continue;
  foreach(var loc in symbol.OriginalDefinition.DeclaringSyntaxReferences){var to=Path.GetFileName(loc.SyntaxTree.FilePath);if(to==from||to=="")continue;if(!destinations.TryGetValue(to,out var names))destinations[to]=names=new();names.Add(symbol.ToDisplayString());}
 }
 edges[from]=destinations;
}
File.WriteAllText(output,JsonSerializer.Serialize(new{SourceFiles=trees.Count-1,Errors=errors,Edges=edges},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"Sources={trees.Count-1} Errors={errors.Length} SourceEdges={edges.Sum(x=>x.Value.Count)}");foreach(var error in errors.Take(5))Console.WriteLine(error);





