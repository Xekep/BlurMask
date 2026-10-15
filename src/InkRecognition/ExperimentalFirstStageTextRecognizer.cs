using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InkRecognition;

/// <summary>Opt-in executable adapter for the restored ordinary first stage.
/// It deliberately does not claim the unfinished main dictionary/text pass.
/// Unsupported paths leave ink untouched; there is no CTC or vendor fallback.</summary>
public sealed class ExperimentalFirstStageTextRecognizer : IStrokeTextRecognizer
{
    private readonly byte[] resource;
    private readonly LegacyInkActivations activations;
    private readonly LegacyCursiveSeparationTable separation;
    public string Language => "ru-RU";
    private ExperimentalFirstStageTextRecognizer(byte[] resource,LegacyInkActivations activations,LegacyCursiveSeparationTable separation)
    {this.resource=resource;this.activations=activations;this.separation=separation;}

    public static ExperimentalFirstStageTextRecognizer Load(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> engineProfile,ReadOnlySpan<byte> separationProfile)
    {
        if(acousticResource.Length!=572763||engineProfile.Length>1_048_576||separationProfile.Length>1_048_576||
            !Convert.ToHexString(SHA256.HashData(acousticResource)).Equals(
                "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978",StringComparison.Ordinal))
            throw new InvalidDataException("The experimental adapter requires its exact supported acoustic resource.");
        using var profile=JsonDocument.Parse(engineProfile.ToArray());
        var a=profile.RootElement.GetProperty("activations");
        var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(x=>x.GetInt32()).ToArray(),
            a.GetProperty("output").EnumerateArray().Select(x=>x.GetInt32()).ToArray());
        _=LegacyFirstAcousticModel.ReadSource(acousticResource,6120);
        return new(acousticResource.ToArray(),activations,LegacyCursiveSeparationTable.Load(separationProfile));
    }

    public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(strokes);cancellationToken.ThrowIfCancellationRequested();
        if(strokes.Count is <1 or >32)return Empty();
        int count=0;
        foreach(var stroke in strokes)
        {
            if(stroke?.Points is not{} points||points.Count==0||points.Count>1024-count)return Empty();
            count+=points.Count;
            foreach(var p in points)if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||Math.Abs(p.X)>100000||Math.Abs(p.Y)>100000)return Empty();
        }
        //UI inference already runs on a worker. Bound experimental CPU work
        //and honor caller cancellation without leaving partially replaced ink.
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(2));
        try
        {
            var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,strokes,activations,separation,timeout.Token);
            var result=owner.Run(timeout.Token).Materialized;
            if(result.PublishedWord is null||result.GlyphReferences.Count is <1 or >128)return Empty();
            var text=new StringBuilder();float score=1;
            foreach(ulong reference in result.GlyphReferences)
            {
                timeout.Token.ThrowIfCancellationRequested();byte[] row=result.Result.GlyphPool.CopyRecord(reference,timeout.Token);
                //Only actual ordinary single-component rows are supported.
                //Macro/control conversion and contextual casing are not guessed.
                if((BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4))&7)!=0)return Empty();
                ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8));
                if(!owner.Factory.InvocationPolicy.Binding.Alphabet.TryGetText(symbol,out string value)||
                    string.IsNullOrWhiteSpace(value)||value.Any(char.IsControl)||value.EnumerateRunes().Any(r=>
                        Rune.GetUnicodeCategory(r)==System.Globalization.UnicodeCategory.PrivateUse))return Empty();
                //Glyph10 is the selected symbol's actual neural score;
                //glyph0C is the independent primitive-likelihood byte/255.
                float candidate=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(16)));
                if(!float.IsFinite(candidate)||candidate<.5f||candidate>1)return Empty();
                score=Math.Min(score,candidate);text.Append(value);
            }
            timeout.Token.ThrowIfCancellationRequested();
            return new(text.ToString(),score,owner.Prepared.SourceRanges.Graph.NodeCount){ExperimentalCandidateScore=true};
        }
        catch(OperationCanceledException)when(!cancellationToken.IsCancellationRequested){return Empty();}
        catch(Exception e)when(e is NotSupportedException or ArgumentException or InvalidOperationException or InvalidDataException)
        {System.Diagnostics.Trace.WriteLine($"Experimental first stage: {e.Message}");return Empty();}
    }
    private static TextRecognition Empty()=>new("",0,0){ExperimentalCandidateScore=true};
}
