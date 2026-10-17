using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>Successful actual21FE30 word -> raw glyph UTF8 and source keys.
/// This precedes0213A0 filtering/021000 string publication and any document
/// endpoint mapping. No expected transcription or supplied labels are read.</summary>
public sealed class LegacyFreshMainGlobalWordUtf8Projection
{
    private readonly byte[] utf8;
    public LegacyFreshMainGlobalGlyphExecutionResult Publication {get;}
    public IReadOnlyList<LegacyOwnedMainProjectedWordGlyph> Glyphs {get;}
    public string Text=>Encoding.UTF8.GetString(utf8);
    private LegacyFreshMainGlobalWordUtf8Projection(LegacyFreshMainGlobalGlyphExecutionResult publication,
        LegacyOwnedMainProjectedWordGlyph[] glyphs,byte[] utf8)
    {Publication=publication;Glyphs=Array.AsReadOnly(glyphs);this.utf8=utf8;}
    public byte[] CopyUtf8()=>utf8.ToArray();

    public static LegacyFreshMainGlobalWordUtf8Projection Project(LegacyFreshMainGlobalGlyphExecutionResult publication,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        var association=publication.Association;var session=publication.ExecutionSession;var ownership=session.Ownership;
        var pass=ownership.CompactOwner.Pass;pass.ValidateResultQueryLease(token);
        if(!ReferenceEquals(association.Session,session.Associations)||
            !association.Flush.BacktrackRows.CompactOwner.IsSameSelection(ownership.CompactOwner))
            throw new ArgumentException("Exact actual global execution/association/materialization required.");
        if(!publication.Succeeded||!association.Succeeded)
            throw new NotSupportedException("Only an actual successful same-owner word callback can be projected.");
        var word=publication.PublishedWord?.Word;
        if(word is null)return new(publication,[],[]); //Actual successful zero-span callback.
        if(!ReferenceEquals(word.Pool,ownership.GlyphPool)||
            !association.Flush.BacktrackRows.CompactOwner.IsSameSelection(ownership.CompactOwner))
            throw new ArgumentException("Published word, glyph pool and compact source must retain identical ownership.");
        byte[] wordStorage=word.CopyStorage();var references=word.FlatReferences;
        if(references.Count is <1 or >4096||BinaryPrimitives.ReadInt32LittleEndian(wordStorage.AsSpan(40))!=references.Count||
            BinaryPrimitives.ReadUInt64LittleEndian(wordStorage.AsSpan(8))!=references[^1])
            throw new InvalidDataException("Actual plain275 ordered reference vector and head/count required.");
        byte[] rows=association.CopyRows72();int[] starts=ownership.CompactOwner.CopyNodeStarts2478();
        var model=ownership.Geometry.Policy.Owner.Model;
        var sources=new Dictionary<(ulong Key,int Descriptor),List<int>>();
        for(int i=0;i<rows.Length/72;i++)
        {
            token.ThrowIfCancellationRequested();var row=rows.AsSpan(i*72,72);int node=BinaryPrimitives.ReadInt32LittleEndian(row);
            if((uint)node>=(uint)(starts.Length-1))throw new InvalidDataException("Associated source node escapes compact owner.");
            var source=(BinaryPrimitives.ReadUInt64LittleEndian(row[48..]),unchecked(BinaryPrimitives.ReadInt32LittleEndian(row[8..])-starts[node]));
            if(!sources.TryGetValue(source,out var indices))sources.Add(source,indices=[]);
            indices.Add(i);
        }
        var result=new List<LegacyOwnedMainProjectedWordGlyph>();using var bytes=new MemoryStream();int tokenWork=0;
        foreach(ulong reference in references)
        {
            token.ThrowIfCancellationRequested();byte[] record=ownership.GlyphPool.CopyRecord(reference,token);
            ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            int descriptorId=BinaryPrimitives.ReadInt32LittleEndian(record);bool? protocol=null;
            if(!sources.TryGetValue((key,descriptorId),out var matches))
                throw new NotSupportedException("Published reference has no actual associated-row producer in this flush.");
            foreach(int i in matches)
            {
                var row=rows.AsSpan(i*72,72);
                if(row[60] is not (0 or 3))throw new NotSupportedException("Additional glyph source kinds need their actual owner mapping.");
                bool kind=row[60]==3;
                if(protocol is not null&&protocol!=kind)throw new InvalidDataException("The same glyph descriptor ambiguously binds ordinary and protocol sources.");
                protocol=kind;
            }
            ulong? original=null;LegacyInkRange[] ranges=[];
            if(protocol==false)
            {
                original=session.Associations.SourceOverlay.ResolveBaseReference(key,token);
                foreach(int i in matches)
                {
                    var row=rows.AsSpan(i*72,72);int node=BinaryPrimitives.ReadInt32LittleEndian(row);ushort arc=BinaryPrimitives.ReadUInt16LittleEndian(row[4..]);
                    ulong baseKey=((uint)node&0xffffffUL)|((ulong)arc<<24);
                    if(original!=baseKey)throw new NotSupportedException("Merged source must retain this exact original scored graph group.");
                }
                ranges=session.Associations.SourceOverlay.CopySourceRanges(key,token);
            }
            //Protocol span keys are not ordinary graph arcs. Preserve them
            //verbatim rather than inventing an NN group or document point ID.
            var descriptor=new LegacyNativeGlyphSymbolDescriptor(BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4)),
                BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(8)));
            int kindCode=descriptor.KindFlags&7;int count=LegacyNativeGlyphUtf8Tokens.ReadTokenCount(descriptor);
            if(kindCode==4||kindCode==3&&count!=0)
                throw new NotSupportedException("Actual alternate/control-alphabet ownership is required for this nonordinary descriptor.");
            tokenWork=checked(tokenWork+count);
            if(tokenWork>1_048_576)throw new NotSupportedException("Portable word token budget exceeded.");
            byte[] text=LegacyNativeOrdinaryGlyphUtf8.Emit(descriptor,model.AcousticAlphabet,cancellationToken:token);
            if(text.Length>8_388_608-bytes.Length)throw new NotSupportedException("Portable word byte budget exceeded.");
            bytes.Write(text);result.Add(new(reference,key,protocol!.Value,original,descriptor,record,text,matches.ToArray(),ranges));
        }
        pass.ValidateResultQueryLease(token);
        token.ThrowIfCancellationRequested();return new(publication,result.ToArray(),bytes.ToArray());
    }
}
