using InkRecognition;
using System.Text.Json;
static class Program
{
    static void Check(bool value){if(!value)throw new InvalidDataException("fixture mismatch");}
    static void Emit(string op,int cycle,int identity,int result,int probes)=>Console.WriteLine(JsonSerializer.Serialize(new{op,cycle,identity,result,probes}));
    sealed class Meter:ILegacyEnglishPeriodicCaseWrapperProbeBudget
    {internal int Probes,ThrowAt=-1;public void BeforeProbe(){if(Probes==ThrowAt)throw new OperationCanceledException();Probes++;}}
    static void Main()
    {
        var buckets=new List<LegacyPreparedDictionaryQuery>[256];for(int i=0;i<256;i++)buckets[i]=new();
        for(int i=0;i<20481;i++){var q=new LegacyPreparedDictionaryQuery();buckets[System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(q)&255].Add(q);}
        var chosen=buckets.First(b=>b.Count>=81).Take(81).ToArray();
        var index=new LegacyEnglishPeriodicRequestMembershipIndex();var requests=new LegacyPreparedDictionaryQuery?[80];var meter=new Meter();
        for(int cycle=0;cycle<2;cycle++)
        {
            for(int count=0;count<80;count++)
            {meter.Probes=0;int found=index.Find(chosen[count],requests,count,out int free,meter);Emit("insert",cycle,count,found,meter.Probes);Check(found==-1);requests[count]=chosen[count];index.Publish(free,count);}
            for(int ordinal=79;ordinal>=0;ordinal--)
            {meter.Probes=0;int found=index.Find(chosen[ordinal],requests,80,out _,meter);Emit("hit",cycle,ordinal,found,meter.Probes);chosen[ordinal].Node++;chosen[ordinal].Skip++;}
            meter.Probes=0;int absent=index.Find(chosen[80],requests,80,out _,meter);Emit("miss",cycle,80,absent,meter.Probes);
            for(int stop=0;stop<81;stop++)
            {meter.Probes=0;meter.ThrowAt=stop;bool refused=false;try{index.Find(chosen[80],requests,80,out _,meter);}catch(OperationCanceledException){refused=true;}Check(refused);Emit("interrupt",cycle,80,-2,meter.Probes);}
            meter.ThrowAt=-1;index.Clear();Array.Clear(requests);meter.Probes=0;Emit("reset",cycle,80,index.Find(chosen[80],requests,0,out _,meter),meter.Probes);
        }
        var scope=new SyntheticScope();var candidate=new LegacyPreparedDictionaryQuery();
        foreach(var pair in new[]{("same-absent",(object)scope),("foreign-absent",new object())})
        {candidate.PeriodicQuery=pair.Item2;bool refused=false;try{scope.Enter(candidate);}catch(InvalidOperationException){refused=true;}Check(refused&&scope.Count==0);Emit(pair.Item1,-1,0,-3,0);}
        candidate.PeriodicQuery=null;Emit("first",-1,0,scope.Enter(candidate),0);Emit("same-hit",-1,0,scope.Enter(candidate),0);
        candidate.PeriodicQuery=new object();Emit("foreign-hit",-1,0,scope.Enter(candidate),0);
        candidate.PeriodicQuery=scope;long copies=scope.Copies,visits=scope.Visits;scope.End();Check(candidate.PeriodicQuery is null&&scope.Copies==copies&&scope.Visits==visits);Emit("end",-1,0,scope.Count,0);
        Emit("reenroll",-1,0,scope.Enter(candidate),0);scope.End();scope.End();Check(candidate.PeriodicQuery is null);Emit("end-empty",-1,0,scope.Count,0);
    }
}
