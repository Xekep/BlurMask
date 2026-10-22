namespace InkRecognition;

// Numerical view with typed origins only. Neither constructor nor a supplied
// mask/flag ingress is exposed. Pure numerical kernels confer no source authority.
internal sealed class LegacyEnglishMainAcousticInputs
{
    private readonly LegacyEnglishMainOnlySourceMasks? independent;
    private readonly LegacyEnglishFreshCaseMainSource? actualCase;
    private LegacyEnglishMainAcousticInputs(LegacyEnglishMainOnlySourceMasks owner){independent=owner;Knowledge=owner.Knowledge;}
    private LegacyEnglishMainAcousticInputs(LegacyEnglishFreshCaseMainSource owner){actualCase=owner;Knowledge=owner.Knowledge;}
    internal LegacyEnglish119MainKnowledge Knowledge{get;}
    internal LegacySource247WireLayout Source=>Knowledge.Source;
    internal int NeuralClassCount=>182;
    internal static LegacyEnglishMainAcousticInputs FromIndependent(LegacyEnglishMainOnlySourceMasks actual)
    {ArgumentNullException.ThrowIfNull(actual);var view=new LegacyEnglishMainAcousticInputs(actual);view.RequireCurrent();return view;}
    internal static LegacyEnglishMainAcousticInputs FromFreshCase(LegacyEnglishFreshCaseMainSource actual)
    {ArgumentNullException.ThrowIfNull(actual);actual.RequireCurrent();return new(actual);}
    internal byte[] CopyPrimaryMask(){RequireCurrent();return independent is{} old?old.CopyPrimaryMask():actualCase!.CopyPrimaryMask();}
    internal void RequireFreshCase(){if(actualCase is null)throw new InvalidOperationException("Actual fresh case source required.");}
    internal void RequireCurrent()
    {
        if(!Knowledge.Owner.IsComplete||Knowledge.Owner.IsFaulted||!ReferenceEquals(Knowledge.MainWire.Source,Source))
            throw new InvalidOperationException("Complete current actual MAIN owner required.");
        if(independent is{} old)
        {
            if(old.Policy!="independent-main13B-only/nonbypassed/no-user-restriction"||old.NeuralClassCount!=182||!ReferenceEquals(old.Source,Source))
                throw new ArgumentException("Exact independent MAIN mask policy required on every call.");
        }
        else actualCase!.RequireCurrent();
    }
}
