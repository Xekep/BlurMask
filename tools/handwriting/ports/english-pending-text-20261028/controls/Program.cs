using System.Reflection;
using System.Text;
using System.Text.Json;
using InkRecognition;
using System.Buffers.Binary;
static class Program
{
    sealed class Budget(int stop):ILegacyEnglishCharacterReuseVisitBudget
    {
        internal readonly List<string> Trace=[];
        void Event(string value){Trace.Add(value);if(Trace.Count-1==stop)throw new InvalidDataException("pre-read numerical fixture stop");}
        public void BeforeEmission(CancellationToken t)=>Event("E");
        public void BeforeIntern(int b,int e,CancellationToken t)=>Event($"I:{b}:{e}");
        public void BeforeIdentityProbe(CancellationToken t)=>Event("P");
        public void BeforePublication(int b,int e,CancellationToken t)=>Event($"U:{b}:{e}");
    }
    sealed class CancelBudget(int cut,CancellationTokenSource stop):ILegacyEnglishCharacterReuseVisitBudget
    {
        internal readonly List<string> Trace=[];
        void Event(string value){Trace.Add(value);if(Trace.Count-1==cut){stop.Cancel();stop.Token.ThrowIfCancellationRequested();}}
        public void BeforeEmission(CancellationToken t)=>Event("E");
        public void BeforeIntern(int b,int e,CancellationToken t)=>Event($"I:{b}:{e}");
        public void BeforeIdentityProbe(CancellationToken t)=>Event("P");
        public void BeforePublication(int b,int e,CancellationToken t)=>Event($"U:{b}:{e}");
    }
    static void Main(string[] args)
    {
        using var charges=JsonDocument.Parse(File.ReadAllBytes(args[0]));
        using(var output=new StreamWriter(args[3],false,new UTF8Encoding(false)))foreach(var row in charges.RootElement.EnumerateArray())
        {
            string op=row.GetProperty("op").GetString()!;int a=row.GetProperty("a").GetInt32(),b=row.GetProperty("b").GetInt32();
            long value=op switch{"range-call"=>GraphCharacterVisitCharge.RangeCall(a,b),"snapshot"=>GraphCharacterVisitCharge.Snapshot(a,b,26,1,true),"marker-flags"=>GraphCharacterVisitCharge.MarkerFlags(a),"selection"=>GraphCharacterVisitCharge.Selection(a),"no-probe-ranges"=>GraphCharacterVisitCharge.NoProbeRanges(a,b/100000,1,b%100000),"emission"=>GraphCharacterVisitCharge.Emission(a),"intern"=>GraphCharacterVisitCharge.Intern(a,b),"publication"=>GraphCharacterVisitCharge.Publication(a,b),_=>throw new InvalidDataException()};
            output.WriteLine(JsonSerializer.Serialize(new{op,a,b,value}));
        }
        using var pf=File.OpenRead(Path.Combine(args[2],"text-unicode/legacy-text-publication-data.json"));var properties=LegacyNativeTextPublicationData.Load(pf);
        using var nf=File.OpenRead(Path.Combine(args[2],"text-unicode/legacy-normalization-data.json"));var normalization=LegacyNativeUnicodeNormalizationData.Load(nf);
        using var fixtures=JsonDocument.Parse(File.ReadAllBytes(args[1]));int caseIndex=0;
        using var captured=new StreamWriter(args[4],false,new UTF8Encoding(false));
        foreach(var fixture in fixtures.RootElement.EnumerateArray())
        {
            var inputs=fixture.GetProperty("inputs").EnumerateArray().Select(x=>x.GetString()!).ToArray();int selected=fixture.GetProperty("marker").GetInt32();
            foreach(var stopNode in fixture.GetProperty("stops").EnumerateArray())
            {
                int stop=stopNode.GetInt32();using var pool=new LegacyNativeUtf8StringPool();var initial=new List<LegacyPreparedTextGlyphReuseRecord>();
                foreach(var value in fixture.GetProperty("initial").EnumerateArray())
                {byte[] bytes=Encoding.UTF8.GetBytes(value.GetString()!);var row=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(row.AsSpan(32),(uint)bytes.Length);initial.Add(new(row,pool.Intern(bytes)));}
                var reuse=LegacyPreparedMainTextGlyphReuse.PrepareEnglishNumeric(pool,properties,normalization,initial,0);var budget=new Budget(stop);int readCount=0;
                Func<int,LegacyPreparedGlyphReuseInput> read=i=>{readCount++;return new(0,Encoding.UTF8.GetBytes(inputs[i]));};
                var method=typeof(LegacyPreparedMainTextGlyphReuse).GetMethod("Process",BindingFlags.Instance|BindingFlags.NonPublic)!;
                var result=(LegacyPreparedTextGlyphReuseResult)method.Invoke(reuse,[inputs.Length,selected,read,default(CancellationToken),budget])!;
                var records=reuse.CopyRecords().Select(x=>Encoding.UTF8.GetString(x.Text!.ReadBytes())).ToArray();
                captured.WriteLine(JsonSerializer.Serialize(new{@case=caseIndex,stop,succeeded=result.Succeeded,marker=result.AdjustedMarkerIndex,original=result.OriginalRecordCount,visited=result.VisitedInputCount,appended=result.AppendedRecordCount,records,strings=pool.Count,readCount,trace=budget.Trace}));
            }
            caseIndex++;
        }
        caseIndex=0;using var cancellations=new StreamWriter(args[5],false,new UTF8Encoding(false));
        foreach(var fixture in fixtures.RootElement.EnumerateArray())
        {
            var inputs=fixture.GetProperty("inputs").EnumerateArray().Select(x=>x.GetString()!).ToArray();int selected=fixture.GetProperty("marker").GetInt32();
            foreach(var stopNode in fixture.GetProperty("stops").EnumerateArray())
            {
                int cut=stopNode.GetInt32();if(cut<0)continue;
                using var pool=new LegacyNativeUtf8StringPool();using var stop=new CancellationTokenSource();
                var initial=new List<LegacyPreparedTextGlyphReuseRecord>();
                foreach(var value in fixture.GetProperty("initial").EnumerateArray())
                {byte[] bytes=Encoding.UTF8.GetBytes(value.GetString()!);var row=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(row.AsSpan(32),(uint)bytes.Length);initial.Add(new(row,pool.Intern(bytes)));}
                var reuse=LegacyPreparedMainTextGlyphReuse.PrepareEnglishNumeric(pool,properties,normalization,initial,0);var budget=new CancelBudget(cut,stop);int readCount=0;
                Func<int,LegacyPreparedGlyphReuseInput> read=i=>{readCount++;return new(0,Encoding.UTF8.GetBytes(inputs[i]));};
                var method=typeof(LegacyPreparedMainTextGlyphReuse).GetMethod("Process",BindingFlags.Instance|BindingFlags.NonPublic)!;
                bool canceled=false,tokenMatches=false,poisoned=false;
                try{method.Invoke(reuse,[inputs.Length,selected,read,stop.Token,budget]);}
                catch(TargetInvocationException error)when(error.InnerException is OperationCanceledException cancel){canceled=true;tokenMatches=cancel.CancellationToken==stop.Token;}
                try{reuse.ApplyPrepared(Array.Empty<LegacyPreparedGlyphReuseInput>(),0);}
                catch(InvalidOperationException){poisoned=true;}
                var records=reuse.CopyRecords().Select(x=>Encoding.UTF8.GetString(x.Text!.ReadBytes())).ToArray();
                cancellations.WriteLine(JsonSerializer.Serialize(new{@case=caseIndex,stop=cut,canceled,tokenMatches,poisoned,records,strings=pool.Count,readCount,trace=budget.Trace}));
            }
            caseIndex++;
        }
        Console.WriteLine("Exact candidate numeric charge model and real reuse Process completed; no real owner/source authority.");
    }
}
