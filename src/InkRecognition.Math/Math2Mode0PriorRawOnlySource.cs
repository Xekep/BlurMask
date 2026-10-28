namespace InkRecognition.Math;

internal enum Math2Mode0PriorNnStage { Entered, Decoded, Copied, Completed }
internal sealed record Math2Mode0PriorNnRow(int ScoreBits,long Frontier,int Parent,ushort[] Primary,ushort[] Secondary,
    uint[] Units,MathSegmentRun[] Runs,Math2Mode0PriorNnText[] Text,LegacyNativeInkPositionRange[][] Ranges);
internal sealed record Math2Mode0PriorNnText(uint Unit,int Identity,int ByteCount,byte[] Bytes);
internal sealed record Math2Mode0PriorNnSnapshot(bool Complete,bool Failed,bool Released,bool DecoderReleased,
    int Width,int Frames,int DecoderCalls,ulong[] Positions,int[] Separators,int[] Order,string[] RawHashes,Math2Mode0PriorNnRow? FirstCopy);

// Explicit new cold/single-F0 library request bound to a genuine current C23.
// It does not replay old output, enter configured grouping, or authorize edits.
internal sealed class Math2Mode0PriorRawOnlySource:IDisposable
{
    private readonly object gate=new();
    private readonly Math2Mode0PriorC23Preparation preparation;
    private Math2Mode0RawOnlySource? source;
    private Math2Mode0FirstResultCopy? copy;
    private readonly int width,frames;
    private readonly ulong[] positions;
    private readonly int[] separators,order;
    private int calls;
    private string[] hashes=[];
    private bool entered,complete,failed,released;
    private Math2Mode0PriorRawOnlySource(Math2Mode0PriorC23Preparation preparation,Math2Mode0RawOnlySource source)
    {
        this.preparation=preparation;this.source=source;width=source.Loop.BeamWidth;frames=source.InputFrameCount;
        var actual=source.ReadSourcePositions();positions=actual.CopyPositions();separators=actual.CopySeparators();order=actual.CopySourceOrder();
    }
    internal static Math2Mode0PriorRawOnlySource PrepareCold(OrtRuntime runtime,Math2Mode0ColdBundle bundle,OrtRun run,
        Math2Mode0PriorC23Preparation preparation,CancellationToken token=default)=>Prepare(runtime,bundle,run,preparation,null,token);
    internal static Math2Mode0PriorRawOnlySource PrepareDiagnostic(OrtRuntime runtime,Math2Mode0ColdBundle bundle,OrtRun run,
        Math2Mode0PriorC23Preparation preparation,int width,CancellationToken token=default)=>Prepare(runtime,bundle,run,preparation,width,token);
    private static Math2Mode0PriorRawOnlySource Prepare(OrtRuntime runtime,Math2Mode0ColdBundle bundle,OrtRun run,
        Math2Mode0PriorC23Preparation preparation,int? width,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(preparation);token.ThrowIfCancellationRequested();
        if(width is<1 or>16)throw new ArgumentOutOfRangeException(nameof(width));
        Math2Mode0PriorRawOnlySource? target=null;Math2Mode0RawOnlySource? raw=null;
        try{return preparation.WithNewNn(input=>
        {
            raw=Math2Mode0RawOnlySource.PrepareCurrentF0(runtime,bundle,run,input,width,token);
            target=new(preparation,raw);input.Commit(target);return target;
        },token);}
        catch{target?.Dispose();if(target is null)raw?.Dispose();if(target is not null)preparation.FailPreparedSource(target);throw;}
    }
    private T WithLive<T>(Func<T> consume,CancellationToken token=default)
        =>preparation.WithPreparedSource(this,()=>{lock(gate){ObjectDisposedException.ThrowIf(released,this);if(failed)throw new InvalidOperationException("Prior NN retains a failed prefix.");token.ThrowIfCancellationRequested();var result=consume();ObjectDisposedException.ThrowIf(released,this);if(failed)throw new InvalidOperationException("Prior NN failed during consumption.");return result;}},token);
    internal object DecoderIdentity=>WithLive(()=>source??throw new InvalidOperationException("Decoder released."));
    internal LegacyOwnedRawInkDocument SourceDocument=>WithLive(()=>source?.ReadSourcePositions().Document??copy?.Document??throw new InvalidOperationException());
    internal void RunToFirstCopy(CancellationToken token=default)=>RunDiagnostic(null,token);
    internal void RunDiagnostic(Action<Math2Mode0PriorNnStage>? observe,CancellationToken token=default)
    {
        bool started=false;
        try
        {
            WithLive(()=>
            {
                if(entered||source is null)throw new InvalidOperationException("Prior NN work is single-use.");entered=true;started=true;
                void Stage(Math2Mode0PriorNnStage stage){observe?.Invoke(stage);ObjectDisposedException.ThrowIf(released,this);preparation.CheckRunningSource(this);token.ThrowIfCancellationRequested();}
                Stage(Math2Mode0PriorNnStage.Entered);
                source.RunToInitialOrder(token);source.ApplyEf4(token);Stage(Math2Mode0PriorNnStage.Decoded);
                var output=source.OrderFinal(token).FormatFullCold(token).AttachSourcePositions(token);
                copy=Math2Mode0FirstResultCopy.Create(output,token);Stage(Math2Mode0PriorNnStage.Copied);
                calls=source.Loop.SuccessfulDecoderCalls;hashes=source.Loop.CopyRawAttentionStatistics().RawFrameHashes;
                if(calls<1||!ReferenceEquals(copy.Document,source.ReadSourcePositions().Document))throw new InvalidOperationException("Actual new NN and same-document first copy required.");
                preparation.MarkWorkComplete(this);complete=true;
                // Exercise and preserve failure of the OUTER owner recheck too.
                observe?.Invoke(Math2Mode0PriorNnStage.Completed);return 0;
            },token);
        }
        catch
        {
            if(started){lock(gate)failed=true;preparation.FailPreparedSource(this);}
            throw;
        }
    }
    internal T WithCompletedResult<T>(Func<Math2Mode0FirstResultCopy,T> consume,CancellationToken token=default)
        =>WithLive(()=>{if(!complete||copy is null)throw new InvalidOperationException("Actual completed prior NN first copy required.");copy.ValidateForOuterCopy(token);var result=consume(copy);copy.ValidateForOuterCopy(token);return result;},token);
    internal Math2Mode0PriorNnSnapshot CopySnapshot()
    {
        lock(gate)
        {
            Math2Mode0PriorNnRow? row=null;
            if(copy is not null&&!released)
            {
                var r=copy.CopyRecord();row=new(BitConverter.SingleToInt32Bits(r.Score),r.Frontier,r.Parent,r.Primary,r.Secondary,r.Units,r.Runs,
                    r.UnitRows.Select(t=>new Math2Mode0PriorNnText(t.Unit,t.Text.Identity,t.ByteCount,t.Text.ReadBytes())).ToArray(),r.SourceRanges);
            }
            return new(complete,failed,released,source is null,width,frames,calls,positions.ToArray(),separators.ToArray(),order.ToArray(),hashes.ToArray(),row);
        }
    }
    internal Math2Mode0PriorNnSnapshot CopyResult(CancellationToken token=default)=>WithLive(()=>{if(!complete)throw new InvalidOperationException("Prior NN incomplete.");return CopySnapshot();},token);
    internal void ReleaseDecoder()=>WithLive(()=>{if(!complete)throw new InvalidOperationException();source?.Dispose();source=null;return 0;});
    public void Dispose(){lock(gate){if(released)return;released=true;source?.Dispose();source=null;copy?.Dispose();}}
}

internal sealed partial class Math2Mode0RawOnlySource
{
    // Only an active new current-C23 claim can enter this named source route.
    // The old selected-field APIs still reject it (selectedField remains null).
    internal static Math2Mode0RawOnlySource PrepareCurrentF0(OrtRuntime runtime,Math2Mode0ColdBundle bundle,OrtRun run,
        Math2Mode0PriorC23Preparation.NnBorrow input,int? width,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(runtime);ArgumentNullException.ThrowIfNull(run);input.Check();token.ThrowIfCancellationRequested();
        var policy=Math2Mode0RawOnlyEf4Policy.Bind(bundle);input.ClaimRawSource();
        var prepared=Math2Mode0RawSourcePositions.PrepareFeatures(input.Document,token);
        prepared.Positions.IncludeCurrentOwner(input);prepared.Positions.ValidateFeatures(prepared.Features);
        var f=prepared.Features;
        if(f.Frames<1)throw new ArgumentException("Nonempty actual current raw features required.");
        using var source=runtime.Tensor(f.Values,f.Frames,1,26);
        using var downsampling=runtime.Tensor(Enumerable.Repeat(1L,f.Frames).ToArray(),f.Frames,1);
        using var ends=runtime.Tensor(f.StrokeEnds,1,f.StrokeEnds.Length);
        input.Check();token.ThrowIfCancellationRequested();
        var loop=width.HasValue?Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle,run,source,downsampling,ends,width.Value,1f,1000f,token):
            Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle,run,source,downsampling,ends,token);
        try{return new(loop,policy,f.Frames,f.StrokeEnds.ToArray(),prepared.Positions);}
        catch{loop.Dispose();throw;}
    }
}
internal sealed partial class Math2Mode0RawSourcePositions
{
    internal void IncludeCurrentOwner(Math2Mode0PriorC23Preparation.NnBorrow input)
    {input.Check();if(!ReferenceEquals(Document,input.Document))throw new ArgumentException("Actual current-selected document required.");PayloadReservation=checked(PayloadReservation+input.Reservation);}
}
