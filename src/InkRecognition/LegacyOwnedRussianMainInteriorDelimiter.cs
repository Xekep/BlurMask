using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainInteriorDelimiter.Probe")]
namespace InkRecognition;

/// <summary>Ordinary assigned-source kind1 branch of21B.V130/1AAE70.
/// Geometry is read from the same actual sampled source. The request64 remains
/// an explicit prepared218560 input, not a reconstructed winning-row packet.</summary>
public sealed class LegacyOwnedRussianMainInteriorDelimiter
{
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly ushort colon,semicolon,exclamation,question,hash,at,hyphen,slash;
    public LegacyOwnedRussianMainSourceSymbolCallbacks Callbacks {get;}
    public LegacyOwnedRussianMainSourceGeometry Geometry {get;}
    private LegacyOwnedRussianMainInteriorDelimiter(LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        LegacyOwnedRussianMainSourceGeometry geometry,LegacyCursiveCategoryProfile categories)
    {
        Callbacks=callbacks;Geometry=geometry;this.categories=categories;
        var alphabet=callbacks.Owner.Model.AcousticAlphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        colon=Label(":");semicolon=Label(";");exclamation=Label("!");question=Label("?");
        hash=Label("#");at=Label("@");hyphen=Label("-");slash=Label("/");
    }

    public static LegacyOwnedRussianMainInteriorDelimiter Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,LegacyOwnedRussianMainSourceGeometry geometry,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(callbacks);ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(callbacks.Owner,geometry.Owner)||!ReferenceEquals(callbacks.Graph,geometry.Graph))
            throw new ArgumentException("One actual sampled source and confidence owner required.");
        //150D00 sets91 from containerC4, sourcekind137, orF0009. The accepted
        //fresh DIRECT/case owners have no external property/reuse overrides.
        var owner=callbacks.Owner;
        if(owner.Model.SourceKind!=0x135||owner.Model.AcousticAlphabet.TryGetLabel("\U000f0009",out _)||
            owner.CaseDictionaryOwner is {} c&&c.IgnoreCaseC4)
            throw new NotSupportedException("Fresh ordinary135 without C4/F0009 shortcut required.");
        //The geometry factory already pins the same complete resource. Prove
        //this separately supplied category buffer is that same snapshot too.
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(resource)),
            "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978"))
            throw new ArgumentException("The same main resource snapshot is required.");
        var selected=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0,token);
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource,189379);
        if(span.NativeSecondByte!=1||selected.SamplerNodeOffset!=188157||selected.Categories.BaseEntryCount!=owner.Model.AcousticAlphabet.LabelCount)
            throw new NotSupportedException("Actual main21B9=1 and version106 sampler/category owner required.");
        token.ThrowIfCancellationRequested();return new(callbacks,geometry,selected.Categories);
    }

    public LegacyOwnedMainDelimiterResult ReadDelimiter(ReadOnlySpan<byte> preparedRequest64,int cursor,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(preparedRequest64.Length!=64||cursor is <0 or >256)throw new ArgumentException("Complete request64 and bounded scratch cursor required.");
        byte[] request=preparedRequest64.ToArray();
        int I(int offset)=>BinaryPrimitives.ReadInt32LittleEndian(request.AsSpan(offset));
        ushort U(int offset)=>BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(offset));
        if(I(0x28)!=1)throw new NotSupportedException("This extension supports only kind1/FFFB.");
        int previous=I(8),current=I(0x18);ushort previousArc=U(12),currentArc=U(0x1c);
        if(previous!=-1)_=Geometry.ReadKind(previous,previousArc,token);
        if(current!=Geometry.Graph.NodeCount)_=Geometry.ReadKind(current,currentArc,token);
        if(cursor==0||previous==-1||current==Geometry.Graph.NodeCount)return new(Callbacks,cursor,[]);
        //Source descriptor flags0 makes request2C==2's recognized-input test
        //false, so this branch emits without reading category or geometry.
        bool emit;
        if(I(0x2c)==2)emit=true;
        else
        {
            int line=Geometry.ReadLine(current,currentArc,token);
            if(line!=Geometry.ReadLine(previous,previousArc,token))throw new NotSupportedException("One assigned source line required.");
            float height=Geometry.ReadGuide(line,token).Height;
            ushort symbol=U(0x1e),prior=U(0x16);
            int first=FirstId(symbol),last=prior==65535?0:FirstId(prior);
            int policy=Policies[first+last*7];
            if(symbol==hash||symbol==at||symbol==hyphen||prior==hyphen||symbol==slash||prior==slash)policy=5;
            else if(policy==4)
            {policy=2;if(I(0x2c)==1&&(symbol==colon||symbol==semicolon||symbol==exclamation||symbol==question))policy=1;}
            uint start=Geometry.Graph.GetNode(previous)[previousArc].StartContextId;
            bool special=I(0x34)==10||prior==hyphen&&start==0;
            if(special&&(I(0x38)==10||!(prior==hyphen&&start!=0)))emit=true;
            else
            {
                if(!special&&policy==3)policy=start!=0?2:1;
                var a=Geometry.ReadBounds(current,currentArc,token);var b=Geometry.ReadBounds(previous,previousArc,token);
                float gap=Gap(a,b),sourceGap=BitConverter.Int32BitsToSingle(I(0x30));
                if(!float.IsFinite(sourceGap))throw new NotSupportedException("Finite prepared source-gap scalar required.");
                float before=float.MaxValue,after=float.MaxValue;
                if(policy==5&&EvaluatePolicy(5,height,sourceGap,gap,before,after,false,false))
                {
                    if((prior==hyphen||prior==slash)&&I(0)!=-1)before=Gap(b,Geometry.ReadBounds(I(0),U(4),token));
                    else if((symbol==hyphen||symbol==slash)&&I(0x20)!=-1)after=Gap(a,Geometry.ReadBounds(I(0x20),U(0x24),token));
                }
                emit=EvaluatePolicy(policy,height,sourceGap,gap,before,after,prior==hyphen||prior==slash,symbol==hyphen||symbol==slash);
            }
        }
        if(!emit&&(Geometry.Graph.GetNode(current)[currentArc].Flags&8)!=0)emit=true;
        token.ThrowIfCancellationRequested();return emit?new(Callbacks,cursor-1,[0xfffb]):new(Callbacks,cursor,[]);
    }

    private int FirstId(ushort symbol)
    {
        if(symbol>=categories.FirstMap.Length||categories.FirstMap.Span[symbol]<0)
            throw new NotSupportedException("Actual mapped sampler symbol required.");
        int first=categories.FirstIds.Span[categories.FirstMap.Span[symbol]];
        if((uint)first>=7)throw new InvalidDataException("Actual seven-category delimiter policy required.");return first;
    }
    private static float Gap(LegacyNativeResidualBounds current,LegacyNativeResidualBounds prior)=>
        current.MinimumX<=prior.MinimumX?prior.MinimumX-current.MaximumX:current.MinimumX-prior.MaximumX;

    internal static bool EvaluatePolicy(int policy,float height,float sourceGap,float gap,float before,float after,bool priorDash,bool currentDash)
    {
        float ordinary=sourceGap<=0?height*2.95f:Math.Max(height*2.35f,sourceGap*1.87f);
        if(policy==2)return ordinary<gap;
        float punctuation=sourceGap<=0?height*.29f:Math.Min(sourceGap*.55f,sourceGap*.12f);
        if(policy==1)return punctuation<=gap;
        if(policy!=5)return true;
        float threshold=(punctuation+ordinary)*.5f;
        bool accepted=threshold<gap;
        if(priorDash&&accepted)return threshold<before||threshold*1.2f<gap;
        if(currentDash&&accepted)return threshold<after||threshold*1.2f<gap;
        return accepted;
    }
    private static readonly int[] Policies=[0,1,4,2,4,1,0, 2,2,2,2,2,0,0, 1,1,2,1,2,0,0,
        3,1,2,2,2,0,0, 1,1,2,1,2,0,0, 2,0,0,0,0,0,0, 0,0,0,0,0,0,0];
}
