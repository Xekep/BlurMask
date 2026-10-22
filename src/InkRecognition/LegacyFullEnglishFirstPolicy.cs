using System.Buffers.Binary;
namespace InkRecognition;

//Actual full model data and fresh selected249/23F initializer lineage only.
internal sealed class LegacyFullEnglishFirstPolicy
{
    internal LegacyFirstAcoustic119WireModel Model {get;}
    internal LegacySampler297WireProfile Sampler {get;}
    internal LegacyCursiveCategoryProfile Categories {get;}
    internal byte[] Header {get;}
    internal int GeometryCount=>Categories.Prototypes.Length;
    internal float F(string n)=>BinaryPrimitives.ReadSingleLittleEndian(B(n));
    internal int I(string n)=>BinaryPrimitives.ReadInt32LittleEndian(B(n));
    internal byte[] B(string n)=>Sampler.Fields.Single(f=>f.Destination==n).CopyBytes();
    //Fresh actual23F:150D00 packet5=1, packet0A=0 regardless F5;1A6980/18D920.
    internal bool FirstSampler210=>true;
    internal bool Graph1DC=>false;
    internal float CostScale=>1;
    internal float SourceGap=>F("6C"); //16A130 source9C=-1; no24D/29B source writes.
    internal LegacyFullEnglishFirstPolicy(LegacyFullEnglishFirstController owner,LegacyFullEnglishFirstAcousticCache cache,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Model=owner.Parent.Model;Sampler=cache.Owner.Sampler;
        if(!ReferenceEquals(cache.Model,Model)||!ReferenceEquals(Sampler.Source,Model.Source)||Sampler.Child.Version!=0x108||
            Model.Source.SourceKind!=0x135||Model.Source.Offset!=Model.SelectedProcessor.Offset+16||Model.Source.End!=Model.SelectedProcessor.End-4||
            B("50")[0]!=1||B("10C")[0]!=1||B("134.present")[0]!=0||B("1D0.present")[0]!=0||F("60")!=0||
            BinaryPrimitives.ReadSingleLittleEndian(Model.Fields.Single(f=>f.Owner=="21D"&&f.Destination=="100").CopyBytes())!=0)
            throw new NotSupportedException("Same actual full fresh249 ordinary FIRST108 profile required.");
        Categories=Sampler.ReadCategories(token);Header=Model.Source.Alphabet.CopySerializedHeader();
        if(Categories.BaseEntryCount!=217||Categories.Prototypes.Length!=24||Categories.CompositeCategories.Length!=42)
            throw new NotSupportedException("Actual full FIRST category domain required.");
        foreach(string n in new[]{"54","58","5C","68","6C","70","74","108"})if(!float.IsFinite(F(n)))throw new InvalidDataException("Finite FIRST geometry profile required.");
        if(F("54")<0||F("5C")<0||F("108")<0||I("118")!=0x4000000||I("120")!=0x4000000||I("124")!=0x4000000)
            throw new NotSupportedException("Actual ordinary FIRST geometry caps required.");
    }
}
