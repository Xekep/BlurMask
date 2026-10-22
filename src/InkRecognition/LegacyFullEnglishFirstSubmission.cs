using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirst.Probe")]
namespace InkRecognition;

/// <summary>Exact pre-NN direct-child input and its own selected119 preparation.
/// Child coordinates are private; its owning ParentInput separately retains
/// actual157C60 parent maps. No flat text override, source720 append or
/// document publication is performed by this receipt.</summary>
public sealed class LegacyFullEnglishFirstSubmission
{
    public const int MaximumRawPoints=4096;
    public LegacyFirstAcoustic119WireModel Model {get;}
    public LegacyFullEnglishFirstPreprocessing Preparation {get;}
    private readonly InkStroke[] raw;
    private readonly int[] map;
    internal IReadOnlyList<InkStroke> Raw=>raw;
    internal IReadOnlyList<int> PhysicalMap=>map;
    private LegacyFullEnglishFirstSubmission(LegacyFirstAcoustic119WireModel model,InkStroke[] raw,
        LegacyFullEnglishFirstPreprocessing preparation)
    {Model=model;this.raw=raw;Preparation=preparation;map=Enumerable.Range(0,raw.Length).ToArray();}

    internal static LegacyFullEnglishFirstSubmission Prepare(LegacyFullEnglishFirstKnowledge knowledge,LegacyFullEnglishFirstBudget budget,
        IReadOnlyList<InkStroke> exactRawChildInput,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);var actualModel=knowledge.Model;ArgumentNullException.ThrowIfNull(exactRawChildInput);token.ThrowIfCancellationRequested();
        int physicalCount=exactRawChildInput.Count;
        if(physicalCount is <1 or >256)throw new ArgumentException("Bounded actual child physical input required.");
        //Preflight the complete aggregate before copying any point storage.
        int total=0;var inputs=new IReadOnlyList<InkPoint>[physicalCount];var counts=new int[inputs.Length];
        for(int s=0;s<inputs.Length;s++)
        {
            token.ThrowIfCancellationRequested();var points=exactRawChildInput[s]?.Points;
            int count=points?.Count??0;
            if(count is <1 or >4096||count>MaximumRawPoints-total)
                throw new ArgumentException("At most4096 total finite raw child points required.");
            total+=count;inputs[s]=points!;counts[s]=count;
            for(int p=0;p<count;p++){token.ThrowIfCancellationRequested();if(!Finite(points![p]))throw new ArgumentException("Finite binary32 child coordinates within ordinary ingress bounds required.");}
        }
        var raw=new InkStroke[inputs.Length];
        for(int s=0;s<raw.Length;s++)
        {
            token.ThrowIfCancellationRequested();var points=inputs[s];
            if(points.Count!=counts[s])throw new ArgumentException("Input point storage changed during bounded snapshot.");
            var copy=new InkPoint[counts[s]];
            for(int p=0;p<copy.Length;p++){token.ThrowIfCancellationRequested();copy[p]=points[p];if(!Finite(copy[p]))throw new ArgumentException("Finite actual snapshot coordinates required.");}
            raw[s]=new(Array.AsReadOnly(copy));
        }
        //Existing preparation keeps deduplicated raw metadata. This receipt
        //separately preserves exact pre-dedup coordinates addressed by that metadata.
        var prepared=LegacyFullEnglishFirstPreprocessing.Prepare(knowledge,budget,raw,token);
        token.ThrowIfCancellationRequested();return new(actualModel,raw,prepared);
    }
    private static bool Finite(InkPoint p)=>float.IsFinite(p.X)&&float.IsFinite(p.Y)&&MathF.Abs(p.X)<=1_000_000&&MathF.Abs(p.Y)<=1_000_000;

    public InkStroke[] CopyRawChildInput(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var copy=new InkStroke[raw.Length];
        for(int s=0;s<copy.Length;s++)
        {
            token.ThrowIfCancellationRequested();var points=new InkPoint[raw[s].Points.Count];
            for(int p=0;p<points.Length;p++){token.ThrowIfCancellationRequested();points[p]=raw[s].Points[p];}
            copy[s]=new(points);
        }
        return copy;
    }
}
