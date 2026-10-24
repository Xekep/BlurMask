using InkRecognition;
using System.Text.Json;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("Fresh prepared-only report required.");
int cases=0,guards=0,matchedSelectionRetained=0,appends=0;
for(int originals=1;originals<=3;originals++)
for(int current=1;current<=3;current++)
for(int earlier=0;earlier<=2;earlier++)
for(int mask=0;mask<(1<<(originals*current));mask++)
for(int selected=0;selected<current;selected++)
{
    bool Eq(int row,int first)=>(mask&(1<<(row*originals+first)))!=0;
    //Independent declarative append set: compare only with the original
    //first owner. Prior appended payload slots are not equality candidates.
    int[] unmatched=Enumerable.Range(0,current).Where(row=>!Enumerable.Range(0,originals).Any(first=>Eq(row,first))).ToArray();
    int expectedSelected=selected;
    foreach(int ordinal in unmatched)
        if(ordinal==expectedSelected)expectedSelected=originals+earlier+Array.IndexOf(unmatched,ordinal);
    if(expectedSelected>=originals+earlier+unmatched.Length)
    {
        Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(current,originals,originals+earlier,false,selected,Eq,default),typeof(NotSupportedException));
        continue;
    }
    int checks=0;
    var plan=LegacyPreparedGlobalWordPayloadReuse.Build(current,originals,originals+earlier,false,selected,
        (row,first)=>{checks++;if(first>=originals)throw new Exception("Compared with appended row.");return Eq(row,first);},default);
    if(!plan.AppendOrdinals.SequenceEqual(unmatched)||plan.ResultChoiceCount!=originals+earlier+unmatched.Length||
        plan.SelectedChoiceIndex!=expectedSelected||checks>current*originals)
        throw new Exception("Native append/selected-slot chronology differs.");
    for(int row=0;row<current;row++)
    {
        int[] matches=Enumerable.Range(0,originals).Where(first=>Eq(row,first)).ToArray();
        int want=matches.Length!=0?matches[0]:originals+earlier+Array.IndexOf(unmatched,row);
        if(plan.ChoiceMap[row]!=want)throw new Exception("Value map differs.");
    }
    if(unmatched.Length==0&&plan.SelectedChoiceIndex==selected)matchedSelectionRetained++;
    appends+=unmatched.Length;cases++;
}
for(int n=1;n<=12;n++)for(int selected=0;selected<n;selected++)
{
    var plan=LegacyPreparedGlobalWordPayloadReuse.Build(n,n,n+1,true,selected,(_,_)=>throw new Exception("Identical native span compared."),default);
    if(plan.SelectedChoiceIndex!=selected||plan.ResultChoiceCount!=n+1||plan.AppendOrdinals.Length!=0||!plan.ChoiceMap.SequenceEqual(Enumerable.Range(0,n)))throw new Exception("Identical-span path differs.");cases++;
}
void Refuse(Action action,Type type){try{action();}catch(Exception e){if(e.GetType()==type){guards++;return;}throw;}throw new Exception("Missing prepared refusal.");}
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(1,1,1,false,0,(_,_)=>false,new CancellationToken(true)),typeof(OperationCanceledException));
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(0,1,1,false,0,(_,_)=>false,default),typeof(ArgumentException));
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(1,0,1,false,0,(_,_)=>false,default),typeof(ArgumentException));
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(1,2,1,false,0,(_,_)=>false,default),typeof(ArgumentException));
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(1,1,1,false,1,(_,_)=>false,default),typeof(ArgumentException));
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(2,1,1,true,0,(_,_)=>false,default),typeof(InvalidOperationException));
Refuse(()=>LegacyPreparedGlobalWordPayloadReuse.Build(1,1,4095,false,0,(_,_)=>false,default),typeof(NotSupportedException));
int consumedControls=0;
for(int bit=0;bit<320;bit++)
{
    byte[] before=new byte[40],after=new byte[40];after[bit/8]=(byte)(1<<(bit%8));
    bool consumed=bit is>=32 and<=34||bit==49||bit is>=64 and<80||bit is>=256 and<320;
    if(LegacyPreparedGlobalWordPayloadReuse.SameEqualityInputs(before,after)==consumed)
        throw new Exception("Native equality consumed-field readset differs.");
    consumedControls++;
}
File.WriteAllText(args[0],JsonSerializer.Serialize(new{Scope="Prepared value-only0283B0 reuse controls; no actual source/WORD issuer/models/NN",Cases=cases,Guards=guards,ConsumedFieldControls=consumedControls,MatchedSelectionRetained=matchedSelectionRetained,AppendEvents=appends,ActualRuns=0},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS{cases} /{guards}");
