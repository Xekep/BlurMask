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
    private readonly byte[]? completedAcoustic;
    private readonly LegacyNativeUnicodeNormalizationData? normalization;
    private readonly LegacyNativeCircledPayloadData? circles;
    internal bool UsesCompletedPublication=>completedAcoustic is not null;
    public string Language=>"ru-RU";
    private ExperimentalGlobalTextRecognizer(LegacyFreshMainRecognitionContext context,
        LegacyCursiveSeparationTable separation,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,
        byte[]? acoustic=null,LegacyNativeUnicodeNormalizationData? normalization=null,LegacyNativeCircledPayloadData? circles=null)
    {this.context=context;this.separation=separation;this.cases=cases;this.publication=publication;
        completedAcoustic=acoustic;this.normalization=normalization;this.circles=circles;}
    /// <summary>Historical raw-WORD adapter; does not complete CHAR/WORD/TEXT publication.</summary>
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
    /// <summary>Opt-in actual completed source and selected-TOP TEXT route.
    /// Resource bytes authenticate the same native Unicode tables; no alternate fallback.</summary>
    public static ExperimentalGlobalTextRecognizer Load(ReadOnlySpan<byte> acoustic,ReadOnlySpan<byte> linguistic,
        ReadOnlySpan<byte> engine,ReadOnlySpan<byte> line30,ReadOnlySpan<byte> line50,
        ReadOnlySpan<byte> separation,ReadOnlySpan<byte> cases,ReadOnlySpan<byte> publication,
        ReadOnlySpan<byte> normalization,ReadOnlySpan<byte> circles,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(acoustic.Length!=572763||normalization.Length is<1 or>1_048_576||circles.Length is<1 or>1_048_576)
            throw new InvalidDataException("Complete bounded same acoustic and Unicode resources required.");
        using var ns=new MemoryStream(normalization.ToArray());using var cs=new MemoryStream(circles.ToArray());
        var n=LegacyNativeUnicodeNormalizationData.Load(ns);var c=LegacyNativeCircledPayloadData.Load(cs);
        byte[] ownedAcoustic=acoustic.ToArray();
        var raw=Load(ownedAcoustic,linguistic,engine,line30,line50,separation,cases,publication,token);
        token.ThrowIfCancellationRequested();return new(raw.context,raw.separation,raw.cases,raw.publication,ownedAcoustic,n,c);
    }
    public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes,CancellationToken cancellationToken=default)
        =>RecognizeCore(strokes,TimeSpan.FromSeconds(2),cancellationToken);
    //Fixed corpus diagnostics may isolate admission/algorithm output from the
    //host's wall-clock deadline. This is not a public host-policy override.
    internal TextRecognition RecognizeDiagnostic(IReadOnlyList<InkStroke> strokes,CancellationToken token=default)
        =>RecognizeCore(strokes,TimeSpan.FromSeconds(30),token);
    //Diagnostic data only; same public2s execution, no callbacks/live owner escape or second inference.
    internal TextRecognition RecognizeCompletedDiagnostic(IReadOnlyList<InkStroke> strokes,out ExperimentalCompletedTextDiagnostic diagnostic,CancellationToken token=default)
    {diagnostic=new();return RecognizeCore(strokes,TimeSpan.FromSeconds(2),token,diagnostic);}
    private TextRecognition RecognizeCore(IReadOnlyList<InkStroke> strokes,TimeSpan deadline,CancellationToken token,ExperimentalCompletedTextDiagnostic? diagnostic=null)
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
            if(UsesCompletedPublication)return RecognizeCompleted(snapshot,timeout.Token,diagnostic);
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
        catch(OperationCanceledException)when(!token.IsCancellationRequested){if(diagnostic is not null)diagnostic.Reason="deadline";return Empty();}
        catch(Exception e)when(e is NotSupportedException or ArgumentException or InvalidOperationException or InvalidDataException)
        {if(diagnostic is not null){diagnostic.Reason="unsupported-or-failed-stage";diagnostic.ExceptionType=e.GetType().FullName;diagnostic.ExceptionMessage=e.Message;}System.Diagnostics.Trace.WriteLine($"Experimental global text: {e.Message}");return Empty();}
    }
    //One source run and one completion claim. Unsupported Aug never retries as NoCandidate.
    private TextRecognition RecognizeCompleted(InkStroke[] snapshot,CancellationToken token,ExperimentalCompletedTextDiagnostic? diagnostic)
    {
        if(diagnostic is not null)diagnostic.Stage="bootstrap";
        var document=LegacyOwnedRawInkDocument.Create(snapshot,token);
        var bootstrap=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(document),["experimental"],token);
        bootstrap.Run(token);
        if(bootstrap.State.Inputs.Count!=1){if(diagnostic is not null)diagnostic.Reason="multiple-input-fields";return Empty();}
        var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(bootstrap,bootstrap.State.Inputs[0],token);selected.Run(token);
        using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(context,selected,0,separation,cases,publication,token);
        if(diagnostic is not null)diagnostic.Stage="source";
        if(diagnostic is not null)diagnostic.SourceInvocations++;
        var source=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],token);
        try
        {
        if(diagnostic is not null&&source.PublicationCompleted)diagnostic.CachedAcousticEvaluations=source.CachedMainGroupEvaluations;
        if(!source.HasWinner||!source.PublicationCompleted||source.Session is not{} session){if(diagnostic is not null)diagnostic.Reason="no-published-winner";return Empty();}
        var compact=source.Selection.Publication.CopyCompactStorage();int count=source.Selection.Publication.RecordCount2470;
        bool needsAugmentation=HasActiveAugmentation(compact,count,token);
        if(diagnostic is not null){diagnostic.Stage="completion";diagnostic.Route=needsAugmentation?"augmentation":"old-no-candidate";}
        LegacyFreshMainGlobalTextSourceWrapper wrapper;
        if(needsAugmentation)
        {
            var actual=LegacyFreshMainGlobalWordAugmentation.Prepare(completedAcoustic!,source,new byte[400],token);
            actual.Run(new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],
                new byte[4],new byte[4],new byte[4],new byte[4],token);
            wrapper=LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,actual,completedAcoustic!,publication,normalization!,circles!,token);
        }
        else
        {
            var actual=scope.CompleteSources(new byte[400],new byte[4],new byte[4],token);
            wrapper=LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,actual,completedAcoustic!,publication,normalization!,circles!,token);
        }
        if(diagnostic is not null)diagnostic.Stage="text";
        var parent=LegacyFreshMainGlobalWordTextPublication.Prepare(wrapper,token);
        parent.Publish(new byte[40],new byte[8160],token);
        var binding=wrapper.ActualCompletion;binding.Validate(token);
        if(!parent.IsComplete||parent.AlternativeCount!=1||!ReferenceEquals(binding.Source,source)||
            !ReferenceEquals(binding.Word.Word.Pool,session.Ownership.GlyphPool))return Empty();
        var originalWord=parent.OriginalWordSource.CopyOriginalKey();
        bool coversFull=HasFullOriginalCoverage(document,originalWord,token);
        if(diagnostic is not null)
        {
            diagnostic.Stage="coverage";diagnostic.FullCoverage=coversFull;
            diagnostic.WordOriginalRanges=originalWord;diagnostic.FullInputRanges=FullOriginalInput(document,token);
            diagnostic.SelectedTopCount=parent.AlternativeCount;diagnostic.Actual275PayloadCount=parent.ActualWordPayloadCount;
        }
        if(!coversFull){if(diagnostic is not null)diagnostic.Reason="partial-original-WORD-coverage";return Empty();}
        var refs=binding.Word.Word.FlatReferences;var links=parent.WordSource.CopyCharacterLinks();
        if(links.Count is<1 or>128||links.Count!=refs.Count)return Empty();
        var text=new StringBuilder();float score=1;
        for(int i=0;i<links.Count;i++)
        {
            token.ThrowIfCancellationRequested();var glyph=links[i].Choice;var processed=glyph.ProcessedSource;
            if(processed.Reference!=refs[i]||!ReferenceEquals(processed.Owner,wrapper)||!processed.IsOrdinarySource||
                !ReferenceEquals(processed.Document,document)||processed.CopyRanges(token).Length==0||
                !ReferenceEquals(processed.OriginalPositions.Source.Owner,session.Associations.Metadata))return Empty();
            //Actual CURRENT selected279 -> SAME live274, never stale raw-WORD or UTF8 scalar weight.
            byte[] row=session.Ownership.GlyphPool.CopyRecord(processed.Reference,token);
            byte[] utf8=glyph.CopySelectedUtf8(token);string value=Encoding.UTF8.GetString(utf8);
            if(diagnostic is not null)
            {
                diagnostic.Stage="glyph-host-policy";
                byte[] captured=utf8;
                if(utf8.Length>1024-diagnostic.CapturedUtf8Bytes){captured=[];diagnostic.Utf8CaptureTruncated=true;}
                diagnostic.CapturedUtf8Bytes+=captured.Length;diagnostic.Glyphs.Add(new(processed.Reference,row,captured));
            }
            if(!TryAdmitSelectedGlyph(row,value,out float candidate))
            {
                if(diagnostic is not null)
                {
                    float native=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(16)));
                    diagnostic.Reason=!float.IsFinite(native)||native<.5f||native>1?"glyph-score-outside-half-to-one":"glyph-kind-or-text-policy";
                }
                return Empty();
            }
            if(value.Length>1024-text.Length){if(diagnostic is not null)diagnostic.Reason="output-length";return Empty();}
            text.Append(value);score=Math.Min(score,candidate);
        }
        if(source.Selection.Publication.Policy.Owner.AcousticEvaluations!=source.CachedMainGroupEvaluations)
            throw new InvalidOperationException("TEXT publication must reuse the same acoustic evaluations.");
        token.ThrowIfCancellationRequested();int nodes=source.Selection.Publication.CopyNodeStarts2478().Length-1;
        if(diagnostic is not null){diagnostic.Stage="admitted";diagnostic.Reason="accepted";}
        return new(text.ToString(),score,nodes){ExperimentalCandidateScore=true};
        }
        finally
        {
            if(diagnostic is not null&&source.PublicationCompleted)
                diagnostic.CurrentAcousticEvaluations=source.Selection.Publication.Policy.Owner.AcousticEvaluations;
        }
    }
    private static LegacyNativeInkPositionRange[] FullOriginalInput(LegacyOwnedRawInkDocument document,CancellationToken token)
    {
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(document);LegacyNativeInkPositionRange[] full=[];
        foreach(var input in document.CopySelectedInputs())
        {token.ThrowIfCancellationRequested();full=operations.Union(full,[LegacyNativeTextSourceRangeProjection.Project(input)],token);}
        return full;
    }
    internal static bool HasFullOriginalCoverage(LegacyOwnedRawInkDocument document,ReadOnlySpan<LegacyNativeInkPositionRange> word,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var operations=LegacyOwnedRawInkRangeOperations.Prepare(document);
        var actual=operations.Union([],word,token);var full=FullOriginalInput(document,token);
        return full.Length>0&&LegacyOwnedRawInkRangeOperations.Equal(actual,full);
    }
    internal static bool HasActiveAugmentation(ReadOnlySpan<byte> compact,int count,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(count<1||count>compact.Length/40)throw new InvalidDataException("Complete bounded active compact extent required.");
        for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();if((compact[i*40+32]&8)!=0)return true;}return false;
    }
    //Host policy is unchanged: native glyph+16 supplies an uncalibrated candidate, not probability.
    internal static bool TryAdmitSelectedGlyph(ReadOnlySpan<byte> record,string value,out float candidate)
    {
        candidate=0;if(record.Length!=40||(BinaryPrimitives.ReadUInt16LittleEndian(record[4..])&7)!=0)return false;
        float score=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(record[16..]));
        if(!float.IsFinite(score)||score<.5f||score>1||string.IsNullOrWhiteSpace(value)||value.Any(char.IsWhiteSpace)||
            value.EnumerateRunes().Any(r=>Rune.GetUnicodeCategory(r) is UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.PrivateUse or UnicodeCategory.Surrogate))return false;
        candidate=score;return true;
    }
    private static TextRecognition Empty()=>new("",0,0){ExperimentalCandidateScore=true};
}

//Detached DATA only; internally created per diagnostic call, never a completion/query issuer.
internal sealed class ExperimentalCompletedTextDiagnostic
{
    internal string Stage="input";
    internal string? Route,Reason,ExceptionType,ExceptionMessage;
    internal int CapturedUtf8Bytes,SourceInvocations;
    internal int? CachedAcousticEvaluations,CurrentAcousticEvaluations;
    internal bool Utf8CaptureTruncated;
    internal bool FullCoverage;
    internal int SelectedTopCount,Actual275PayloadCount;
    internal LegacyNativeInkPositionRange[] WordOriginalRanges=[],FullInputRanges=[];
    internal List<ExperimentalCompletedGlyphDiagnostic> Glyphs=[];
}
internal sealed record ExperimentalCompletedGlyphDiagnostic(ulong Reference,byte[] ActualPoolRecord40,byte[] SelectedTextUtf8);
