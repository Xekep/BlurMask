using System.Text;
namespace InkRecognition.Math;

public enum ExperimentalMathAdmission { Uncalibrated }
public sealed class ExperimentalMathResult
{
    public IReadOnlyList<ExperimentalMathToken> Tokens {get;}
    public IReadOnlyList<ExperimentalMathSyntaxNode> Syntax {get;}
    public string? LaTex {get;}
    public string? UnsupportedReason {get;}
    public float? Confidence=>null;
    public ExperimentalMathAdmission Admission=>ExperimentalMathAdmission.Uncalibrated;
    public int ScoreBits {get;}
    public int Frames {get;}
    public string Policy {get;}
    public long PayloadReservation {get;}
    public bool HasCalculationRequest=>LaTex?.TrimEnd().EndsWith('=')==true;
    internal ExperimentalMathResult(ExperimentalMathToken[] tokens,ExperimentalMathSyntaxNode[] nodes,string? latex,string? unsupported,int score,int frames,string policy,long reservation)
    {Tokens=Array.AsReadOnly(tokens);Syntax=Array.AsReadOnly(nodes);LaTex=latex;UnsupportedReason=unsupported;ScoreBits=score;Frames=frames;Policy=policy;PayloadReservation=reservation;}
}
internal enum ExperimentalMathStage { BeforeInputCopy,BeforeInference,AfterInference,BeforeResultCopy,AfterToken,BeforeSyntax,AfterSyntax,Complete }

/// <summary>Cold width1 raw Math2 experimental library adapter. Ranking scores
/// are uncalibrated; no native semantic tree, calculation or app admission.</summary>
public sealed class ExperimentalColdMathRecognizer:IDisposable
{
    public const long DefaultResultBudget=32L*1024*1024;
    private readonly SemaphoreSlim gate=new(1,1);
    private readonly OrtRuntime runtime;
    private readonly Math2Mode0ColdBundle bundle;
    private bool disposed;
    private ExperimentalColdMathRecognizer(OrtRuntime runtime,Math2Mode0ColdBundle bundle){this.runtime=runtime;this.bundle=bundle;}
    public static ExperimentalColdMathRecognizer Load(Stream pack,ReadOnlySpan<byte> rawResource,string? officialRuntimePath=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(pack);token.ThrowIfCancellationRequested();var runtime=new OrtRuntime(officialRuntimePath);
        try{return new(runtime,Math2Mode0ColdBundle.Read(runtime,pack,rawResource,token));}catch{runtime.Dispose();throw;}
    }
    public ExperimentalMathResult Recognize(IReadOnlyList<InkStroke> strokes,long resultBudget=DefaultResultBudget,CancellationToken token=default)
        =>RecognizeDiagnostic(strokes,resultBudget,null,token);
    internal ExperimentalMathResult RecognizeDiagnostic(IReadOnlyList<InkStroke> strokes,long budget,Action<ExperimentalMathStage,int>? observer,CancellationToken token)
    {
        gate.Wait(token);
        try
        {
            ObjectDisposedException.ThrowIf(disposed,this);CheckInput(strokes,budget,token);Observe(ExperimentalMathStage.BeforeInputCopy,-1);
            var document=LegacyOwnedRawInkDocument.Create(strokes,token);var view=LegacyFreshRawInkCurrentView.Prepare(document);
            using var run=runtime.CreateRun(token);
            using var field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula",token);Observe(ExperimentalMathStage.BeforeInference,-1);
            using var source=Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field,token);
            source.RunToInitialOrder(token);source.ApplyEf4(token);var formatted=source.OrderFinal(token).FormatFullCold(token);
            var attached=formatted.AttachSourcePositions(token);using var receipt=attached.CopySelectedMathField(field,token);
            Observe(ExperimentalMathStage.AfterInference,-1);
            return Detach(source,receipt,budget,observer,token);
        }
        finally{gate.Release();}
        void Observe(ExperimentalMathStage stage,int index){observer?.Invoke(stage,index);token.ThrowIfCancellationRequested();}
    }
    private static void CheckInput(IReadOnlyList<InkStroke> strokes,long budget,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(strokes);if(budget is<0 or>DefaultResultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        if(budget<16384)throw new NotSupportedException("Minimum detached result envelope unavailable before input copy.");
        if(strokes.Count is<1 or>256)throw new ArgumentException("One to256 original pen strokes required.");
        int count=0;foreach(var stroke in strokes)
        {
            token.ThrowIfCancellationRequested();var points=stroke?.Points??throw new ArgumentException("Null pen stroke.");
            if(points.Count<1||points.Count>4096-count)throw new NotSupportedException("At most4096 original source points.");count+=points.Count;
            foreach(var p in points)if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||MathF.Abs(p.X)>100000||MathF.Abs(p.Y)>100000)throw new ArgumentException("Finite bounded coordinates required.");
        }
    }
    internal static ExperimentalMathResult Detach(Math2Mode0RawOnlySource source,Math2Mode0SelectedMathFieldCopy receipt,long budget,Action<ExperimentalMathStage,int>? observer,CancellationToken token)
    {
        if(budget is<0 or>DefaultResultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        source.ReadSelectedMathField(receipt.Field);receipt.Validate(receipt.Copy,receipt.Field,token);
        string policy=source.Loop.UsesColdDefaults&&source.Loop.BeamWidth==1?"ColdDefault":"PreparedSearchDiagnostic";
        var result=receipt.Copy.WithSymbolRowsForOuterCopy((rows,ranges,sourceReservation)=>
        {
            long bytes=0,rangeCount=0;
            if(rows.Length>4096)throw new NotSupportedException("Bounded first-record token vector required.");
            for(int i=0;i<rows.Length;i++){token.ThrowIfCancellationRequested();if(rows[i].ByteCount<1||rows[i].ByteCount!=rows[i].Text.ByteCount)throw new InvalidDataException("Exact UTF8 interval required.");bytes=checked(bytes+rows[i].ByteCount);rangeCount=checked(rangeCount+ranges[i].Length);}
            int maxNodes=checked(rows.Length*2+1);
            long reservation=checked(sourceReservation+16384+rows.Length*2048L+bytes*16+rangeCount*128);
            if(reservation>budget||bytes>1024*1024||rangeCount>65536)throw new NotSupportedException("Whole detached copy/UTF8/ranges/AST envelope exceeds budget before copies.");
            Observe(ExperimentalMathStage.BeforeResultCopy,-1);
            var record=receipt.Copy.CopyRecord();var detached=new ExperimentalMathToken[rows.Length];
            for(int i=0;i<rows.Length;i++)
            {
                token.ThrowIfCancellationRequested();byte[] utf8=rows[i].Text.ReadBytes();string? text=null;
                int end=Array.IndexOf(utf8,(byte)0);if(end<0)end=utf8.Length;
                if(end==utf8.Length||end==utf8.Length-1)
                    try{text=new UTF8Encoding(false,true).GetString(utf8.AsSpan(0,end));}catch(DecoderFallbackException){/* Raw bytes remain evidence. */}
                detached[i]=new(record.Units[i],utf8,text,ranges[i].Select(r=>new ExperimentalMathRange(r.First.Value,r.Last.Value)).ToArray());
                Observe(ExperimentalMathStage.AfterToken,i);
            }
            Observe(ExperimentalMathStage.BeforeSyntax,-1);ExperimentalMathSyntaxNode[] nodes=[];string? latex=null,unsupported=null;
            try
            {nodes=ExperimentalMathSyntax.Parse(detached,maxNodes,token);latex=MathLatexFormatter.Format(detached.Select(t=>t.Text!).ToArray());if(latex is null)unsupported="Unsupported canonical math grammar.";}
            catch(FormatException e){unsupported=e.Message;}
            if(unsupported is not null)nodes=[];
            Observe(ExperimentalMathStage.AfterSyntax,-1);
            return new ExperimentalMathResult(detached,nodes,latex,unsupported,BitConverter.SingleToInt32Bits(record.Score),source.InputFrameCount,policy,reservation);
        },token);
        receipt.Validate(receipt.Copy,receipt.Field,token);Observe(ExperimentalMathStage.Complete,-1);return result;
        void Observe(ExperimentalMathStage stage,int index){observer?.Invoke(stage,index);token.ThrowIfCancellationRequested();}
    }
    public void Dispose()
    {
        gate.Wait();try{if(disposed)return;disposed=true;try{bundle.Dispose();}finally{runtime.Dispose();}}finally{gate.Release();}
    }
}
