using System.Text.Json;
using InkRecognition;
if(args.Length!=1)throw new ArgumentException("independent-controls.json");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));int controls=0,aliases=0,refused=0;
foreach(var sample in document.RootElement.EnumerateArray())
{
    int[] fields=sample.GetProperty("fields").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
    var parameters=new LegacyNativeWordRingParameters(fields[0],fields[1],fields[2],fields[3]);
    int distance=sample.GetProperty("distance").GetInt32();
    if(LegacyNativeWordRingCapacity.ResolveRetentionDistance(parameters)!=distance)throw new Exception("Native signed retention differs.");
    int requested=sample.GetProperty("requested").GetInt32();
    if(distance<=0){Reject(()=>LegacyNativeWordRingCapacity.Prepare(requested,parameters));refused++;}
    else
    {
        var plan=LegacyNativeWordRingCapacity.Prepare(requested,parameters);
        if(plan.LogicalCapacity!=sample.GetProperty("capacity").GetInt32()||plan.DistinctRowCount!=sample.GetProperty("physical").GetInt32()||plan.RetentionDistance!=distance)
            throw new Exception("Native rounded capacity or distinct row count differs.");
        int node=0;
        foreach(var identity in sample.GetProperty("identities").EnumerateArray())
        {if(plan.GetPhysicalRowIndex(node++)!=identity.GetInt32())throw new Exception("Native previous-ring alias identity differs.");aliases++;}
        Reject(()=>plan.GetPhysicalRowIndex(-1));Reject(()=>plan.GetPhysicalRowIndex(plan.LogicalCapacity));
    }
    controls++;
}
Reject(()=>LegacyNativeWordRingCapacity.Prepare(0,new(1,0,0,0)));
Reject(()=>LegacyNativeWordRingCapacity.Prepare(1_000_001,new(1,0,0,0)));
Reject(()=>default(LegacyNativeWordRingCapacityPlan).GetPhysicalRowIndex(0));
Console.WriteLine($"PASS {controls} independent signed ring/growth controls, {aliases} alias identities, {refused} unresolved/nonpositive refusals. Prepared scalar plan; not native allocation or final winner.");
static void Reject(Action action){try{action();}catch(ArgumentException){return;}throw new Exception("Invalid prepared ring extent accepted.");}
