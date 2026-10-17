namespace InkRecognition;

//1BCBD0 arithmetic shared by the original concrete owner and actual global
//cache snapshots. Graph/model identity remains the responsibility of their
//private owning factories; this is not a public prepared-score replacement.
internal static class LegacyMainGlyphConfidenceKernel
{
    internal static LegacyMainGlyphConfidenceCandidate[] Evaluate(ReadOnlySpan<LegacyAcousticCacheCandidate> candidates,
        IReadOnlyList<ReadOnlyMemory<int>?> family,Func<float> actualPrimitiveProbability,
        LegacyMainConfidenceCalibration calibration,LegacyAcousticConfidenceLabelMap labels,ushort closed,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(family);ArgumentNullException.ThrowIfNull(actualPrimitiveProbability);token.ThrowIfCancellationRequested();
        if(candidates.Length>150)throw new InvalidDataException("Bounded actual candidate cache required.");
        var result=new LegacyMainGlyphConfidenceCandidate[candidates.Length];
        if(family.Count!=7||family.Take(4).Any(f=>f is null||f.Value.Length!=180)||family.Skip(4).Any(f=>f is not null))
            throw new InvalidDataException("Actual four180-output EXP families required.");
        for(int i=0;i<candidates.Length;i++)
        {
            token.ThrowIfCancellationRequested();ushort symbol=candidates[i].Symbol;float confidence=0;
            if(i<20)
            {
                if(symbol==closed)
                {
                    //Stored first physical range, not chronological order or
                    //the narrower whole-normalized-stroke primitive scope.
                    confidence=actualPrimitiveProbability();token.ThrowIfCancellationRequested();
                    if(!float.IsFinite(confidence))throw new InvalidDataException("Finite original source probability required.");
                }
                else
                {
                    if(!labels.TryResolveOutputLabel(symbol,out ushort output))
                        throw new NotSupportedException("Actual retained symbol requires an unimplemented confidence service.");
                    //Calibration indexes the original192-label domain. Only
                    //the neural lookup uses3A0; preserve family1+0+2+3 order.
                    float a=calibration.Calibrate(symbol,0,family[0]!.Value.Span[output],token);
                    float b=calibration.Calibrate(symbol,1,family[1]!.Value.Span[output],token);
                    float sum=b+a;
                    sum+=calibration.Calibrate(symbol,2,family[2]!.Value.Span[output],token);
                    sum+=calibration.Calibrate(symbol,3,family[3]!.Value.Span[output],token);
                    sum+=0f;sum+=0f;sum+=0f;sum+=0f;
                    confidence=sum/4;if(confidence>=1)confidence=1;
                }
            }
            result[i]=new(symbol,candidates[i].Cost,confidence);
        }
        token.ThrowIfCancellationRequested();return result;
    }
}
