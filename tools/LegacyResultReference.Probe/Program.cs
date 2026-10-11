using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("controls.json");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,updates=0;
foreach(var row in controls.RootElement.EnumerateArray())
{
 int[] nodes=row.GetProperty("counts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 int available=LegacyPreparedResultReferencePolicy.GetAvailableSourceNodeCount(nodes,row.GetProperty("cached").GetInt32());
 long old=row.GetProperty("old").GetInt64();
 long reference=LegacyPreparedResultReferencePolicy.UpdateReferenceCost(old,row.GetProperty("cost").GetInt32(),row.GetProperty("cumulative").GetInt32(),row.GetProperty("first").GetInt32()!=0,row.GetProperty("flag99").GetInt32()!=0,row.GetProperty("flagA8").GetInt32()!=0);
 Require(available==row.GetProperty("count").GetInt32()&&reference==row.GetProperty("reference").GetInt64(),"Native reference/count branch differs.");
 Require(LegacyPreparedResultReferencePolicy.GetSourceScoreMultiplier(row.GetProperty("kind").GetInt32())==row.GetProperty("multiplier").GetSingle(),"Explicit source-kind dispatch differs.");
 if(reference!=old)updates++;count++;
}
Require(LegacyPreparedResultReferencePolicy.GetAvailableSourceNodeCount([-1],3)==3,"Positive joined cache did not bypass source records.");
Reject(()=>LegacyPreparedResultReferencePolicy.GetAvailableSourceNodeCount([-1],0));
Reject(()=>LegacyPreparedResultReferencePolicy.GetAvailableSourceNodeCount([int.MaxValue,1],0));
Reject(()=>LegacyPreparedResultReferencePolicy.UpdateReferenceCost(0,3,int.MaxValue,true,false,false));
using(var cts=new CancellationTokenSource()){cts.Cancel();Canceled(()=>LegacyPreparedResultReferencePolicy.GetAvailableSourceNodeCount([1],2,cts.Token));Canceled(()=>LegacyPreparedResultReferencePolicy.UpdateReferenceCost(0,3,0,true,false,false,cts.Token));}
// Complete prepared result controller composition with caller-supplied graph
// metadata and terminal winner. Not a decoded-text acceptance policy.
int nodeCount=LegacyPreparedResultReferencePolicy.GetAvailableSourceNodeCount([2,3],0);
long bestReference=LegacyPreparedResultReferencePolicy.UpdateReferenceCost(0,55000,5000,true,false,false);
var best=LegacyPreparedResultScore.Evaluate(new(5000,50000,bestReference,nodeCount-1,false,LegacyPreparedResultReferencePolicy.GetSourceScoreMultiplier(0x135),10000));
var next=LegacyPreparedResultScore.Evaluate(new(5000,58000,bestReference,nodeCount-1,false,5,12000));
Require(best.NormalizedPathCost==0&&best.LogMass==0&&best.Mass==1&&next.NormalizedPathCost>0&&next.Mass<1,"Prepared controller did not subtract the explicit best reference.");
Require(LegacyPreparedResultReferencePolicy.UpdateReferenceCost(bestReference,63000,5000,false,false,false)==bestReference,"Later alternative changed first reference.");
Console.WriteLine($"PASS {count} independent controller/count cases ({updates} reference updates), source-kind dispatch, sentinel/flag guards, checked domain, cancellation and composed prepared result stage.");
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();throw new Exception("Invalid reference request accepted.");}catch(ArgumentException){}}
static void Canceled(Action action){try{action();throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}}
