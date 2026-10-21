using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
[assembly:InternalsVisibleTo("ExperimentalGlobalText.Probe")]
namespace InkRecognition;

/// <summary>Opt-in host adapter for the actual Russian global dictionary path.
/// Only ordinary single-WORD output is admitted. Candidate scores remain
/// uncalibrated; unsupported/uncertain requests return no replacement.</summary>
public sealed class ExperimentalGlobalTextRecognizer : IStrokeTextRecognizer
{
    private readonly LegacyFreshMainRecognitionContext context;
    private readonly LegacyCursiveSeparationTable separation;
    private readonly LegacyNativeCaseData cases;
    private readonly LegacyNativeTextPublicationData publication;
    public string Language=>"ru-RU";
    private ExperimentalGlobalTextRecognizer(LegacyFreshMainRecognitionContext context,
        LegacyCursiveSeparationTable separation,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication)
    {this.context=context;this.separation=separation;this.cases=cases;this.publication=publication;}
    public static ExperimentalGlobalTextRecognizer Load(ReadOnlySpan<byte> acoustic,ReadOnlySpan<byte> linguistic,
        ReadOnlySpan<byte> engine,ReadOnlySpan<byte> line30,ReadOnlySpan<byte> line50,
        ReadOnlySpan<byte> separation,ReadOnlySpan<byte> cases,ReadOnlySpan<byte> publication,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(engine.Length is<1 or>1_048_576||separation.Length is<1 or>1_048_576||
            cases.Length is<1 or>8_388_608||publication.Length is<1 or>8_388_608)
            throw new InvalidDataException("Bounded complete global text resources required.");
        var context=LegacyFreshMainRecognitionContext.Load(acoustic,linguistic,engine,line30,line50,token);
        var table=LegacyCursiveSeparationTable.Load(separation);
        using var caseStream=new MemoryStream(cases.ToArray());using var publicationStream=new MemoryStream(publication.ToArray());
        var caseData=LegacyNativeCaseData.Load(caseStream);var textData=LegacyNativeTextPublicationData.Load(publicationStream);
        token.ThrowIfCancellationRequested();return new(context,table,caseData,textData);
    }
    public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes,CancellationToken cancellationToken=default)
        =>RecognizeCore(strokes,TimeSpan.FromSeconds(2),cancellationToken);
    //Fixed corpus diagnostics may isolate admission/algorithm output from the
    //host's wall-clock deadline. This is not a public host-policy override.
    internal TextRecognition RecognizeDiagnostic(IReadOnlyList<InkStroke> strokes,CancellationToken token=default)
        =>RecognizeCore(strokes,TimeSpan.FromSeconds(30),token);
    private TextRecognition RecognizeCore(IReadOnlyList<InkStroke> strokes,TimeSpan deadline,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(strokes);token.ThrowIfCancellationRequested();
        int count=strokes.Count;if(count is<1 or>32)return Empty();int points=0;
        var snapshot=new InkStroke[count];
        try
        {
        for(int s=0;s<count;s++)
        {
            token.ThrowIfCancellationRequested();
            if(strokes[s]?.Points is not{} values)return Empty();int length=values.Count;
            if(length<1||length>1024-points)return Empty();points+=length;
            var copy=new InkPoint[length];
            for(int i=0;i<length;i++)
            {
                token.ThrowIfCancellationRequested();var p=values[i];
                if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||Math.Abs(p.X)>100000||Math.Abs(p.Y)>100000)return Empty();copy[i]=p;
            }
            snapshot[s]=new(copy);
        }
        }
        catch(Exception e)when(e is ArgumentOutOfRangeException or IndexOutOfRangeException){return Empty();}
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(token);timeout.CancelAfter(deadline);
        try
        {
            var result=context.PrepareGlobal(snapshot,0,separation,cases,publication,timeout.Token)
                .Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],timeout.Token);
            if(!result.HasWinner||!result.PublicationCompleted||result.RawWordsInNativeHeadOrder.Count!=1)return Empty();
            var word=result.RawWordsInNativeHeadOrder[0];if(word.Glyphs.Count is<1 or>128)return Empty();
            var text=new StringBuilder();float score=1;
            foreach(var glyph in word.Glyphs)
            {
                timeout.Token.ThrowIfCancellationRequested();
                if(glyph.IsProtocolSource||glyph.OriginalGraphSourceKey is null||(glyph.Descriptor.KindFlags&7)!=0)return Empty();
                byte[] row=glyph.CopyRecord40();float candidate=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(16)));
                if(!float.IsFinite(candidate)||candidate<.5f||candidate>1)return Empty();
                string value=Encoding.UTF8.GetString(glyph.CopyUtf8());
                if(string.IsNullOrWhiteSpace(value)||value.Any(char.IsWhiteSpace)||value.EnumerateRunes().Any(r=>
                    Rune.GetUnicodeCategory(r) is UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.PrivateUse or UnicodeCategory.Surrogate))return Empty();
                if(value.Length>1024-text.Length)return Empty();text.Append(value);score=Math.Min(score,candidate);
            }
            timeout.Token.ThrowIfCancellationRequested();
            int nodes=word.Publication.ExecutionSession.Ownership.CompactOwner.CopyNodeStarts2478().Length-1;
            return new(text.ToString(),score,nodes){ExperimentalCandidateScore=true};
        }
        catch(OperationCanceledException)when(!token.IsCancellationRequested){return Empty();}
        catch(Exception e)when(e is NotSupportedException or ArgumentException or InvalidOperationException or InvalidDataException)
        {System.Diagnostics.Trace.WriteLine($"Experimental global text: {e.Message}");return Empty();}
    }
    private static TextRecognition Empty()=>new("",0,0){ExperimentalCandidateScore=true};
}
