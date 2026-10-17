namespace InkRecognition;

/// <summary>One actual120-byte275 WORD payload and its native ordinal. This
/// is not a279 published node and does not fabricate a selected candidate.</summary>
public sealed class LegacyOwnedMainSpanWordView
{
    public LegacyFreshMainWordSourceChoices Owner {get;}
    public int WordOrdinal {get;}
    internal LegacyNativePlainWordRecord Record {get;}
    public IReadOnlyList<ulong> FlatReferences=>Record.FlatReferences;
    internal LegacyOwnedMainSpanWordView(LegacyFreshMainWordSourceChoices owner,int ordinal,
        LegacyNativePlainWordRecord record)
    {Owner=owner;WordOrdinal=ordinal;Record=record;}
    public byte[] CopyStorage120()=>Record.CopyStorage();
    internal void Validate(CancellationToken token)=>Owner.ValidateWord(this,token);
}
