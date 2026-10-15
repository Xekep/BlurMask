namespace InkRecognition;

public sealed partial class LegacyNativeFirstDictionaryWordCompletion
{
    private readonly CaseSpanBinding? caseSpan;

    /// <summary>Bounded204290 first-word consumer over the actual fresh292
    /// source and current owned glyph-span map. This does not run the complete
    /// case scheduler or produce a final decoded winner. All original first-word
    /// mode, root-anchor, protocol and geometry restrictions still apply.</summary>
    public static LegacyNativeFirstDictionaryWordCompletion ForFreshCaseSpan(
        LegacyFreshCaseMainSourcePolicy policy, LegacyOwnedMainWordSpan span,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(span);
        cancellationToken.ThrowIfCancellationRequested();
        var controller = policy.Controller;
        var owner = controller.Owner;
        if (!ReferenceEquals(span.Owner, owner) ||
            !ReferenceEquals(owner.CaseDictionaryOwner, policy.DictionaryOwner) ||
            !ReferenceEquals(owner.CommonModel, policy.Model) ||
            !ReferenceEquals(owner.SourceGraph, policy.Graph))
            throw new ArgumentException("Actual fresh case source/controller/span/model/ordinary graph owner required.");
        owner.ValidateSpan(span, cancellationToken);
        var configuration = controller.Configuration;
        if (configuration.Mode22f6 != 0 || configuration.Trace2368 != 0 ||
            !float.IsFinite(controller.SecondaryScaleEc) || controller.SecondaryScaleEc > 0 ||
            policy.HasAssociatedSource || policy.ConfiguredSourceCount != 1)
            throw new NotSupportedException("Fresh nontracing mode0/nonpositive ec one-source case policy required.");
        //204323 calls the same222.V280/155710 getter as093AB0. It forwards
        //this typed ordinary21B graph's V1C8/05A400, which returns false.
        //NoAssociated alone would not prove this frontend branch.
        return new(policy, span);
    }

    private LegacyNativeFirstDictionaryWordCompletion(LegacyFreshCaseMainSourcePolicy policy,
        LegacyOwnedMainWordSpan span)
    {
        var dictionary = policy.DictionaryOwner;
        alphabet = policy.Model.AcousticAlphabet;
        categories = policy.Model.CreateCursiveCategoryMapper(1);
        categoryPolicy = new(categories);
        language = dictionary.LanguageRecords;
        if (!language.IsForModel(policy.Model))
            throw new ArgumentException("Actual selected case dictionary language profiles required.");
        sourceId = 1;
        sourceHeader = dictionary.Descriptor.Flags;
        if ((sourceHeader & 0x1e00) != 0xc00 || (sourceHeader & 0x1c) <= 7)
            throw new NotSupportedException("Actual order2/C00 source descriptor required.");
        replacement = alphabet.TryGetLabel("\ufffd", out ushort label) ? label : ushort.MaxValue;
        caseSpan = new(policy, span);
    }

    private sealed class CaseSpanBinding(LegacyFreshCaseMainSourcePolicy policy, LegacyOwnedMainWordSpan span)
    {
        private LegacyOwnedMainWordSpanPreparation Owner => policy.Controller.Owner;

        public void Validate(int endingNode, bool knowledgeF5, bool suppressPrevious2609,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Owner.ValidateSpan(span, cancellationToken);
            if (endingNode != span.EndingNode || knowledgeF5 != policy.Controller.KnowledgeF5 ||
                suppressPrevious2609 != (policy.Controller.Configuration.SuppressPreviousContext2609 != 0))
                throw new ArgumentException("Completion node/knowledge/history flags must match the actual fresh source controller.");
        }

        public ReadOnlyMemory<byte> ReadTerminal(ulong fullTag, ushort endingSymbol,
            CancellationToken cancellationToken)
            => Owner.ReadWordTerminal(span, fullTag, endingSymbol, 0, cancellationToken);

        public ulong ResolveRestart(ulong fullTag, CancellationToken cancellationToken)
            => Owner.ResolveWordRestart(span, fullTag, cancellationToken);
    }
}
