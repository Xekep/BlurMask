using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalGlyphExecution.Probe")]
namespace InkRecognition;

internal sealed record LegacyMainPlainWordCallbackOutcome(bool Succeeded,int Span,
    LegacyNativePublishedWordNode? Published,byte[] CompletedWord);

//21FE30 success assignments and reset order. Actual factories bind the chain
//and publisher; explicit prepared callbacks are confined to internal controls.
internal static class LegacyMainPlainWordCallbackKernel
{
    internal static LegacyMainPlainWordCallbackOutcome Execute(LegacyNativeWordBuilder word,
        Func<bool> executeChain,Func<LegacyNativePublishedWordNode?> publish,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(word);ArgumentNullException.ThrowIfNull(executeChain);ArgumentNullException.ThrowIfNull(publish);
        token.ThrowIfCancellationRequested();bool succeeded=executeChain();int span=word.GetSymbolSpan(token);
        LegacyNativePublishedWordNode? published=null;
        if(span>0){if(succeeded)published=publish();succeeded=published is not null;}
        byte[] completed=word.CopyStorage(token);word.ResetPlain(token);
        return new(succeeded,span,published,completed);
    }
}
