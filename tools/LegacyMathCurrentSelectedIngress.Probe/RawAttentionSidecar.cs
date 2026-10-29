using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;

// Probe diagnostics only. Reflection reads two exact private fields of the
// actual completed owner; tensors still leave through the EXISTING guarded
// history copy. No field is written, no snapshot enters a recognizer, and no
// supplied tensor/array can become an engine owner.
internal sealed class RawAttentionSidecar
{
    private readonly string output,runtime;
    private readonly RawAttentionInputPin[] inputs;
    private readonly List<RawAttentionCase> cases=[];
    private readonly List<RawAttentionCase> fresh=[];
    private RawAttentionSidecar(string output,string runtime,RawAttentionInputPin[] inputs)
    {this.output=output;this.runtime=runtime;this.inputs=inputs;}
    internal static RawAttentionSidecar? CreateIfEnabled(string output,string[] inputs,string runtime)
    {
        if(Environment.GetEnvironmentVariable("NOVAPEN_MATH_RAW_ATTENTION")!="1")return null;
        if(!BitConverter.IsLittleEndian)throw new NotSupportedException("These pinned Windows/Linux/macOS captures require little-endian float bytes.");
        var pins=inputs.Select((path,index)=>new RawAttentionInputPin(index,Path.GetFileName(path),new FileInfo(path).Length,HashFile(path))).ToArray();
        return new(output+".attention.json",runtime,pins);
    }
    private static string HashFile(string path)
    {using var stream=File.OpenRead(path);return Convert.ToHexStringLower(SHA256.HashData(stream));}

    // Observe the fresh inference Pipeline.Create ALREADY ran. Do not run a
    // second loop, infer encoder inputs from outputs, or supply hypotheses.
    internal void CaptureFresh(int index,bool reversed,Math2Mode0RawOnlySource actual)
    {
        if(index is<0 or>5||fresh.Any(c=>c.Case==index))throw new InvalidOperationException("One actual fresh history per case required.");
        var loop=actual.Loop;var before=loop.CopyRawAttentionStatistics();
        var frames=actual.ReadHistoryForDiagnostic().CopyFramesForInspection();
        int calls=loop.SuccessfulDecoderCalls,width=loop.BeamWidth;
        if(!loop.IsFinishedRecording||loop.IsFailed||!before.Complete||before.Failed||before.Disposed||
            frames.Length!=calls||before.RawFrameCount!=calls||before.RawFrameHashes.Length!=calls)
            throw new InvalidDataException("Actual fresh recording is not complete.");
        var retained=new RawAttentionFrame[frames.Length];
        for(int i=0;i<frames.Length;i++)
        {
            var frame=frames[i];
            if(frame.Segments<1||frame.Beams<1||frame.Beams>width||frame.Values.Length!=checked(frame.Segments*frame.Beams))
                throw new InvalidDataException("Actual fresh frame dimensions differ.");
            string sha=Convert.ToHexStringLower(SHA256.HashData(MemoryMarshal.AsBytes(frame.Values.AsSpan())));
            if(sha!=before.RawFrameHashes[i])throw new InvalidDataException("Fresh float bytes differ from actual retained history SHA.");
            var bits=new uint[frame.Values.Length];for(int j=0;j<bits.Length;j++)bits[j]=BitConverter.SingleToUInt32Bits(frame.Values[j]);
            retained[i]=new(i,frame.Segments,frame.Beams,sha,bits);
        }
        var after=loop.CopyRawAttentionStatistics();
        if(!after.Complete||after.Failed||after.Disposed||after.RawFrameCount!=calls||loop.SuccessfulDecoderCalls!=calls||
            !after.RawFrameHashes.SequenceEqual(before.RawFrameHashes))throw new InvalidDataException("Fresh history changed during observation.");
        fresh.Add(new("Fresh",index,width,reversed,actual.InputFrameCount,calls,retained));
        Write(false,null);
    }

    [DynamicDependency("source",typeof(Math2Mode0PriorRawOnlySource))]
    [DynamicDependency("history",typeof(Math2Mode0RawOnlySource))]
    internal void Capture(string route,int index,bool reversed,Math2Mode0PriorRawOnlySource actual)
    {
        if(route is not("Unchanged"or"Actual")||index is<0 or>5||cases.Any(c=>c.Route==route&&c.Case==index))
            throw new InvalidOperationException("Exactly one diagnostic history per existing route/case is required.");
        var before=actual.CopyResult();
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var source=typeof(Math2Mode0PriorRawOnlySource).GetField("source",flags)?.GetValue(actual)as Math2Mode0RawOnlySource
            ??throw new InvalidOperationException("Exact actual completed source field is unavailable.");
        var history=typeof(Math2Mode0RawOnlySource).GetField("history",flags)?.GetValue(source)as Math2Mode0RawAttentionHistory
            ??throw new InvalidOperationException("Exact actual source history field is unavailable.");
        // This call rechecks same-loop history, completed owner and disposal.
        var frames=history.CopyFramesForInspection();
        if(!before.Complete||before.Failed||before.Released||before.DecoderReleased||
            frames.Length!=before.DecoderCalls||frames.Length!=before.RawHashes.Length)
            throw new InvalidDataException("Completed actual frame/hash cohort required.");
        var retained=new RawAttentionFrame[frames.Length];
        for(int i=0;i<frames.Length;i++)
        {
            var frame=frames[i];
            if(frame.Segments<1||frame.Beams<1||frame.Beams>before.Width||frame.Values.Length!=checked(frame.Segments*frame.Beams))
                throw new InvalidDataException("Actual source frame dimensions differ.");
            string sha=Convert.ToHexStringLower(SHA256.HashData(MemoryMarshal.AsBytes(frame.Values.AsSpan())));
            if(sha!=before.RawHashes[i])throw new InvalidDataException("Actual float bytes differ from existing diagnostic SHA.");
            var bits=new uint[frame.Values.Length];for(int j=0;j<bits.Length;j++)bits[j]=BitConverter.SingleToUInt32Bits(frame.Values[j]);
            retained[i]=new(i,frame.Segments,frame.Beams,sha,bits);
        }
        var after=actual.CopyResult();
        if(after.Complete!=before.Complete||after.Failed||after.Released||after.DecoderReleased||
            after.DecoderCalls!=before.DecoderCalls||!after.RawHashes.SequenceEqual(before.RawHashes))
            throw new InvalidDataException("Actual outer owner changed during diagnostic copy.");
        cases.Add(new(route,index,before.Width,reversed,before.Frames,before.DecoderCalls,retained));
        Write(false,null); // Retain evidence while this actual decoder is still alive.
    }
    internal void Seal(string capture)
    {
        if(!fresh.Select(c=>c.Case).Order().SequenceEqual(Enumerable.Range(0,6)))
            throw new InvalidDataException("Incomplete actual fresh diagnostic coverage.");
        foreach(string route in new[]{"Unchanged","Actual"})
            if(!cases.Where(c=>c.Route==route).Select(c=>c.Case).Order().SequenceEqual(Enumerable.Range(0,6)))
                throw new InvalidDataException("Incomplete diagnostic route/case coverage.");
        Write(true,HashFile(capture));
    }
    private void Write(bool complete,string? captureHash)
    {
        var value=new RawAttentionEvidence(2,"PROBE_DIAGNOSTIC_ONLY_NO_ENGINE_INGRESS_OR_NUMERIC_TOLERANCE",runtime,
            RuntimeInformation.ProcessArchitecture.ToString(),RuntimeInformation.FrameworkDescription,BitConverter.IsLittleEndian,
            complete,captureHash,inputs,cases.ToArray(),fresh.ToArray());
        string temporary=output+".tmp";
        using(var file=File.Create(temporary))JsonSerializer.Serialize(file,value,AttentionJson.Default.RawAttentionEvidence);
        File.Move(temporary,output,true);
    }
}
internal sealed record RawAttentionInputPin(int Argument,string FileName,long Bytes,string Sha256);
internal sealed record RawAttentionFrame(int Frame,int Segments,int Beams,string Sha256,uint[] FloatBits);
internal sealed record RawAttentionCase(string Route,int Case,int Width,bool Reversed,int InputFrames,int DecoderCalls,RawAttentionFrame[] Frames);
internal sealed record RawAttentionEvidence(int Schema,string Scope,string Runtime,string Architecture,string Framework,bool LittleEndian,
    bool Complete,string? CaptureSha256,RawAttentionInputPin[] Inputs,RawAttentionCase[] Cases,RawAttentionCase[] Fresh);
[JsonSourceGenerationOptions][JsonSerializable(typeof(RawAttentionEvidence))]
internal partial class AttentionJson:JsonSerializerContext;
