using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Text;
namespace InkRecognition;

/// <summary>Detached ordered WORD output of a genuine completed source batch.
/// These values are not a WORD/TEXT/query issuer. No separator is invented.</summary>
public sealed class LegacyCompletedGlobalTextWords
{
    public readonly record struct PreparedLine(int Index,double OriginX,double OriginY,double Angle,
        double XHeight,long OwnerTag,double UnadjustedBaseline);
    public sealed class Word
    {
        private readonly LegacyNativeInkPositionRange[] ranges;
        public int SourceSelectedIndex{get;}
        public int ProcessedSelectedIndex{get;}
        public int SourceChoiceCount{get;}
        public int ProcessedChoiceCount{get;}
        public string SelectedUtf8{get;}
        /// <summary>Minimum actual glyph+16 value; uncalibrated, not probability.</summary>
        public float CandidateScore{get;}
        public bool MeetsExperimentalGlyphPolicy{get;}
        public PreparedLine Line{get;}
        internal Word(LegacyCompletedBatchSelectedWords.Word actual,PreparedLine line)
        {
            SourceSelectedIndex=actual.SourceSelectedIndex;ProcessedSelectedIndex=actual.PayloadSelectedIndex;
            SourceChoiceCount=actual.SourceChoiceCount;ProcessedChoiceCount=actual.ChoiceCount;
            ranges=actual.OriginalRanges;Line=line;var text=new StringBuilder();float score=1;bool admitted=true;
            foreach(var glyph in actual.Glyphs)
            {
                string value=Encoding.UTF8.GetString(glyph.Utf8);text.Append(value);
                float raw=BinaryPrimitives.ReadSingleLittleEndian(glyph.Record40.AsSpan(16));
                score=float.IsNaN(raw)?float.NaN:Math.Min(score,raw);
                admitted&=ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(glyph.Record40,value,out _);
            }
            SelectedUtf8=text.ToString();CandidateScore=score;MeetsExperimentalGlyphPolicy=admitted;
        }
        public LegacyNativeInkPositionRange[] CopyOriginalRanges()=>ranges.ToArray();
    }
    public ReadOnlyCollection<Word> Words{get;}
    public bool FullOriginalCoverage{get;}
    public bool MeetsExperimentalGlyphPolicy{get;}
    private readonly LegacyNativeInkPositionRange[] coverage;
    private LegacyCompletedGlobalTextWords(LegacyCompletedBatchSelectedWords actual,PreparedLine line)
    {
        var words=new Word[actual.Words.Length];bool admitted=true;
        for(int i=0;i<words.Length;i++){words[i]=new(actual.Words[i],line);admitted&=words[i].MeetsExperimentalGlyphPolicy;}
        Words=Array.AsReadOnly(words);coverage=actual.OriginalCoverage;FullOriginalCoverage=actual.FullCoverage;
        MeetsExperimentalGlyphPolicy=admitted;
    }
    /// <summary>Copies DATA from the exact private completed batch; cannot
    /// promote caller bytes, rows or ranges to a live source owner.</summary>
    public static LegacyCompletedGlobalTextWords Read(LegacyFreshMainGlobalWordTextPublication publication,
        LegacyFreshMainGlobalPlainRecognitionResult original,CancellationToken token=default)
    {
        var actual=LegacyCompletedBatchSelectedWords.Read(publication,original,token);
        var result=FromDetached(actual,publication.CopySourceLines56());
        token.ThrowIfCancellationRequested();publication.CompletedWordUseCount(token);return result;
    }
    internal static LegacyCompletedGlobalTextWords FromDetached(LegacyCompletedBatchSelectedWords actual,byte[] lines)
    {
        return new(actual,DecodePreparedLine(lines));
    }
    internal static PreparedLine DecodePreparedLine(byte[] lines)
    {
        if(lines.Length!=56)throw new NotSupportedException("Structured WORD output requires the validated ordinary line0.");
        var line=new PreparedLine(0,D(8),D(16),D(24),D(32),BinaryPrimitives.ReadInt64LittleEndian(lines.AsSpan(40)),D(48));
        if(!double.IsFinite(line.OriginX)||!double.IsFinite(line.OriginY)||!double.IsFinite(line.Angle)||
            !double.IsFinite(line.XHeight)||line.XHeight<=0||!double.IsFinite(line.UnadjustedBaseline))
            throw new InvalidDataException("Assigned finite native line geometry required.");
        return line;
        double D(int at)=>BinaryPrimitives.ReadDoubleLittleEndian(lines.AsSpan(at));
    }
    public LegacyNativeInkPositionRange[] CopyOriginalCoverage()=>coverage.ToArray();
}
