using InkRecognition;

int controls=0;
var profile=OwnProfile();
CheckProfile(profile);
if(args.Length>0)
{
    var bytes=File.ReadAllBytes(args[0]);
    foreach(int offset in new[]{11571,188222})
        CheckProfile(LegacyCursiveCategoryProfile.Load(bytes.AsSpan(offset),offset,out _));
    Console.WriteLine("Actual Russian parallel profiles: category selection agrees with their stored start maps and first-ID tables.");
}
foreach(bool mode in new[]{false,true})foreach(uint flags in new uint[]{0,4,8,12,16,20,24,28,0xffffffff})
foreach(int category in new[]{0,1,2,3,4,5,6})foreach(int start in new[]{0,1,11})foreach(int delta in new[]{-2,0,3})
{
    bool enabled=!mode && flags is not 0 and not 4;
    bool penalizedCategory=category==2 || category==4;
    int expected=enabled && penalizedCategory && (start==0 || delta!=0) ? 8192:0;
    Require(LegacyOrdinaryWordCategoryPolicy.GetPreviousCategoryPathPenalty(mode,flags,category,start,delta)==expected,"Ordinary predecessor correction differs.");
    controls++;
}
Reject<ArgumentOutOfRangeException>(()=>LegacyOrdinaryWordCategoryPolicy.GetPreviousCategoryPathPenalty(false,8,2,-1,0));
Console.WriteLine($"PASS ordinary word category: {controls} checks, explicit candidate/beam choice, low-six-bit masking, post-lookup category-one override, metadata separation, predecessor correction, missing/invalid rejection, cancellation.");

void CheckProfile(LegacyCursiveCategoryProfile p)
{
    var mapper=new LegacyCursiveCategoryMapper(p);
    var policy=new LegacyOrdinaryWordCategoryPolicy(mapper);
    for(int symbol=0;symbol<mapper.SymbolEntryCount;symbol++)
    {
        int expectedComposite=p.StartCompositeMap.Span[symbol];
        for(int flags=0;flags<256;flags+=17)
        {
            // Nonzero increment selects the resource symbol-start category,
            // including increments greater than one (not a boolean equality).
            foreach(byte increment in new byte[]{1,2,255})
            {
                if(expectedComposite<0)
                    Reject<InvalidDataException>(()=>policy.Resolve(0,(ushort)symbol,increment,(ushort)mapper.SymbolEntryCount,(byte)flags,ushort.MaxValue,false));
                else
                {
                    var selected=policy.Resolve(0,(ushort)symbol,increment,(ushort)mapper.SymbolEntryCount,(byte)flags,ushort.MaxValue,false);
                    Require(selected.CompositeCategory==expectedComposite && selected.WordCategory==p.CompositeCategories.Span[expectedComposite].FirstId && selected.UsedCandidateStartMap && !selected.ForcedCategoryOne && selected.MetadataFlags==(flags&7),"Start-map selection differs.");
                    var special=policy.Resolve(0,(ushort)symbol,increment,(ushort)mapper.SymbolEntryCount,(byte)flags,(ushort)symbol,false);
                    var forced=policy.Resolve(0,(ushort)symbol,increment,(ushort)mapper.SymbolEntryCount,(byte)flags,ushort.MaxValue,true);
                    Require(special.WordCategory==1 && forced.WordCategory==1 && special.CompositeCategory==expectedComposite && forced.ForcedCategoryOne,"Mapped category-one override differs.");
                }
                controls++;
            }
        }
    }
    for(int composite=0;composite<Math.Min(64,mapper.CompositeCategoryCount);composite++)
    foreach(int high in new[]{0,64,128,192})foreach(byte increment in new byte[]{0,1,255})
    {
        // At the exact limit, even a nonzero increment must use the beam.
        var selected=policy.Resolve((byte)(high|composite),7,increment,7,255,ushort.MaxValue,false);
        Require(selected.CompositeCategory==composite && selected.WordCategory==p.CompositeCategories.Span[composite].FirstId && !selected.UsedCandidateStartMap && selected.MetadataFlags==7,"Beam fallback or unsigned equality differs.");
        controls++;
    }
    // Zero increment must use the beam even for a valid below-limit symbol.
    Require(!policy.Resolve(0,0,0,(ushort)mapper.SymbolEntryCount,0,ushort.MaxValue,false).UsedCandidateStartMap,"Zero increment selected start map.");
    Reject<ArgumentOutOfRangeException>(()=>policy.Resolve(63,ushort.MaxValue,1,ushort.MaxValue,0,ushort.MaxValue,true));
    int missing=Array.IndexOf(p.StartCompositeMap.ToArray(),-1);
    if(missing>=0)Reject<InvalidDataException>(()=>policy.Resolve(0,(ushort)missing,1,(ushort)mapper.SymbolEntryCount,0,(ushort)missing,true));
    using var cts=new CancellationTokenSource();cts.Cancel();
    Reject<OperationCanceledException>(()=>policy.Resolve(0,0,0,0,0,0,true,cts.Token));
}

static LegacyCursiveCategoryProfile OwnProfile()
{
    using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    writer.Write((ushort)0x201);writer.Write((ushort)3);
    Ids([0,4,2]);writer.Write(new byte[]{0,1,2,255,0,1});
    Ids([11,21]);writer.Write(new byte[]{0,1,0,0,1,0});writer.Write(new byte[]{1,0,1,0,0,1});
    for(int i=0;i<2;i++){writer.Write((float)i);writer.Write(1f);writer.Write(2f);writer.Write((byte)0);}
    return LegacyCursiveCategoryProfile.Load(stream.ToArray(),0,out _);
    void Ids(int[] ids){writer.Write((byte)ids.Length);while(stream.Position%4!=0)writer.Write((byte)0);foreach(int id in ids)writer.Write(id);}
}
static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
static void Reject<T>(Action action)where T:Exception{try{action();throw new Exception("Unsupported input accepted.");}catch(T){}}
