namespace InkRecognition;

// Private-constructor join of completed SAME-source factories, not an alternate
// native source or a DATA-to-owner constructor. Original public APIs stay typed.
internal sealed class LegacyConfiguredSourceCompletionReceipt
{
    internal LegacyFreshMainConfiguredNoCandidateSourceCompletion? Legacy {get;}
    private readonly LegacyFreshMainConfiguredWordAugmentation? augmented;
    internal LegacyFreshMainConfiguredWordAugmentation? Augmented=>augmented;
    private LegacyConfiguredSourceCompletionReceipt(LegacyFreshMainConfiguredNoCandidateSourceCompletion old)
    {Legacy=old;}
    private LegacyConfiguredSourceCompletionReceipt(LegacyFreshMainConfiguredWordAugmentation actual)
    {augmented=actual;}
    internal static LegacyConfiguredSourceCompletionReceipt From(LegacyFreshMainConfiguredNoCandidateSourceCompletion actual,CancellationToken token)
    {ArgumentNullException.ThrowIfNull(actual);actual.RequireCompleted(token);return new(actual);}
    internal static LegacyConfiguredSourceCompletionReceipt From(LegacyFreshMainConfiguredWordAugmentation actual,CancellationToken token)
    {ArgumentNullException.ThrowIfNull(actual);actual.RequireCompleted(token);return new(actual);}
    internal LegacyOwnedMainWordSourceCollection Sources=>Legacy?.Sources??augmented!.Sources;
    internal void Require(CancellationToken token)
    {if(Legacy is not null)Legacy.RequireCompleted(token);else augmented!.RequireCompleted(token);}
    internal LegacyOwnedMainWordSourceCollection SourcesFor(LegacyOwnedMainCandidateSource candidate,CancellationToken token)
    {Require(token);return Legacy is not null?Legacy.SourcesFor(candidate,token):augmented!.SourcesFor(candidate,token);}
}
