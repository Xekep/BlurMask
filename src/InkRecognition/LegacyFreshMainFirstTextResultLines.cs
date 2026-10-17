using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Actual first TEXT rows to descriptor34/38 and native24B. No empty
/// first result is replaced:24B leaves its unassigned descriptor for later24A.</summary>
public static class LegacyFreshMainFirstTextResultLines
{
    public static LegacyFreshMainFirstResultLineSet Apply(LegacyFreshMainFirstResultLines source,
        IReadOnlyList<LegacyMainMaterializedFirstTextSource> actualResults,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(actualResults);token.ThrowIfCancellationRequested();
        if(actualResults.Count!=source.FirstSubmissions.Count||actualResults.Select(r=>r.DescriptorOrdinal).Distinct().Count()!=actualResults.Count)
            throw new ArgumentException("One owned first TEXT source per ordinary descriptor required.");
        var descriptors=source.Input.CopyDescriptors();var rows=new List<LegacyFirstResultSourceRecord>();
        foreach(var submission in source.FirstSubmissions)
        {
            token.ThrowIfCancellationRequested();var result=actualResults.SingleOrDefault(r=>r.DescriptorOrdinal==submission.DescriptorOrdinal)
                ??throw new ArgumentException("Missing actual first TEXT submission.");
            if(!ReferenceEquals(result.SourceOwner,source)||!ReferenceEquals(result.Text.Result.Factory,submission.Factory))
                throw new ArgumentException("Foreign first TEXT source owner.");
            if(result.Rows.Count>4096-rows.Count)throw new ArgumentException("Total bounded source720 storage required.");
            descriptors[submission.DescriptorOrdinal]=LegacyFirstResultSourceRecord.PublishDescriptorInterval(descriptors[submission.DescriptorOrdinal],rows.Count,result.Rows.Count,token);
            rows.AddRange(result.Rows);
        }
        var owned=rows.Select(r=>new LegacyMainOwnedFirstResultLineRow(source.Controller,r)).ToArray();
        var guides=Array.Empty<ReadOnlyMemory<byte>>();var outputs=new List<LegacyMainFirstResultLineOutput>();
        var points=source.Controller.Source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var transformed=rows.Select(LegacyFirstResultLineRow.FromFirstResult).ToArray();
        foreach(var submission in source.FirstSubmissions)
        {
            token.ThrowIfCancellationRequested();int d=submission.DescriptorOrdinal;
            var output=source.Controller.ProcessDescriptor(descriptors[d],owned,guides,new byte[40],token);
            descriptors[d]=output.CopyDescriptor().ToArray();guides=output.CopyGuides().Select(g=>new ReadOnlyMemory<byte>(g)).ToArray();
            int first=I(descriptors[d],52),end=I(descriptors[d],56);
            if(first==end)
            {
                if(output.NewLineFit is not null||output.Deskew is not null||I(descriptors[d],16)!=-1)
                    throw new InvalidDataException("Native24B empty descriptor skip changed.");
            }
            else
            {
                if(output.Deskew is not {} deskew||output.NewLineFit is null)throw new InvalidDataException("Actual nonempty first TEXT did not publish24B geometry.");
                var current=deskew.CopyPhysicalPoints();foreach(int physical in submission.CurrentParentPhysicalIndices.Span)points[physical]=current[physical];
                for(int r=first;r<end;r++)transformed[r]=deskew.Rows[r];
            }
            outputs.Add(output);
        }
        token.ThrowIfCancellationRequested();return new(source,descriptors,guides.Select(g=>g.ToArray()).ToArray(),points,rows.ToArray(),transformed,outputs.ToArray());
    }
    private static int I(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}
