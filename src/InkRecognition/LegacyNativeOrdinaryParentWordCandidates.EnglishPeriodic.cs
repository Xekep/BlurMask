using System.Buffers.Binary;
namespace InkRecognition;
public sealed partial class LegacyNativeOrdinaryParentWordCandidates
{
    internal LegacyNativeOrdinaryWordStartupResult ProcessPeriodicPage(ReadOnlySpan<byte> queryRecords32,
        ushort endingNode, byte sourceSpan, int previousEndingNode, ushort previousRowOrdinal,
        ReadOnlySpan<byte> storedParent88, ushort inputFlags, LegacyNativeWordSymbolQualifier qualifier,
        IReadOnlyDictionary<byte, uint> preparedSourceHeaders,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getOwnedGroupCosts,
        LegacyEnglishCaseNode0Completion.GraphPeriodicRows periodicRows,
        LegacyNativeWordHeapRouter router, float secondaryScale, bool knowledgeF5,
        bool suppressHeaderFlag22f7, byte permission2309, bool suppressPreviousContext2609,
        bool frontendSuppressionEnabled, Func<int, ushort, bool>? isSuppressedE8,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Ordinary parent page calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed parent page requires a fresh producer.");
            ArgumentNullException.ThrowIfNull(qualifier);
            ArgumentNullException.ThrowIfNull(preparedSourceHeaders);
            ArgumentNullException.ThrowIfNull(getOwnedGroupCosts);
            ArgumentNullException.ThrowIfNull(periodicRows);
            periodicRows.RequirePage(queryRecords32,storedParent88,previousEndingNode,previousRowOrdinal,graph,contexts,alphabet,cancellationToken);
            ArgumentNullException.ThrowIfNull(router);
            cancellationToken.ThrowIfCancellationRequested();
            if (storedParent88.Length != 88 || queryRecords32.Length % 32 != 0 || queryRecords32.Length > 512 * 32 ||
                endingNode >= graph.NodeCount || previousEndingNode < 0 || sourceSpan > 14 ||
                endingNode - sourceSpan - 1 != previousEndingNode || qualifier.AcousticLabelCount != alphabet.LabelCount ||
                !float.IsFinite(secondaryScale))
                throw new ArgumentException("Actual bounded adjacent source span, complete parent88 and query page required.");
            if (frontendSuppressionEnabled && isSuppressedE8 is null)
                throw new ArgumentException("Enabled frontend suppression needs its actual graphE8 predicate.");
            var parent = storedParent88;
            var queryBytes = queryRecords32;
            bool softHyphenParent = (parent[6] & 1) != 0;
            if (softHyphenParent && (globalPathGuard is null || knowledgeMarkerA0 != 0))
                throw new NotSupportedException("Parent header6.bit0 selects kind7 and requires its full native source/context path guard.");
            byte parentSourceId = parent[4];
            if (parentSourceId == 0 || !preparedSourceHeaders.TryGetValue(parentSourceId, out uint parentHeader))
                throw new InvalidDataException("Actual parent source ID has no prepared header.");
            int parentAuxiliary30 = BinaryPrimitives.ReadInt32LittleEndian(parent.Slice(0x30));
            int parentAuxiliary34 = BinaryPrimitives.ReadInt32LittleEndian(parent.Slice(0x34));
            int anchorNode = BinaryPrimitives.ReadInt32LittleEndian(parent.Slice(0x38));
            ushort anchorArc = BinaryPrimitives.ReadUInt16LittleEndian(parent.Slice(0x3c));
            var arcs=graph.RequireSameSource().GetNode(previousEndingNode)[BinaryPrimitives.ReadUInt16LittleEndian(parent.Slice(0x1c))].Successors;
            periodicRows.ChargePage(queryBytes.Length,arcs.Count);
            int visited = 0, qualified = 0, calls = 0, accepted = 0;
            for (int offset = 0; offset < queryBytes.Length; offset += 32)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = queryBytes.Slice(offset, 32);
                visited++;
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                byte length = row[22], flags = row[23];
                if ((flags & 8) != 0 && parentAuxiliary30 != -1 || symbol == loop && parentAuxiliary30 == -1) continue;
                byte kind = (byte)(flags & 7);
                if (kind is not (0 or 2) || !LegacyNativeOrdinaryWordSymbols.Supports(alphabet, symbol))
                    throw new NotSupportedException("Nonvirtual ordinary kind0/2 source query required; special/type7/category policies remain unported.");
                ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(row);
                byte sourceId = (byte)(tag >> 32);
                if (sourceId == 0 || !preparedSourceHeaders.TryGetValue(sourceId, out uint header))
                    throw new InvalidDataException("Actual current source ID has no prepared header.");
                if (knowledgeF5 && (header >> 9 & 15) == 2) continue;
                ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                if (context == 0 || contexts.ReadNode(context).Symbol != symbol)
                    throw new InvalidDataException("Actual parent-query context symbol differs from its current label.");
                LegacyNativeOrdinaryWordSymbols.CheckQuoteContext(alphabet, symbol, contexts, context);
                if (!qualifier.Qualifies(symbol, length, context, contexts, cancellationToken)) continue;
                qualified++;
                for (int variant = 0; variant < LegacyNativeOrdinaryWordSymbols.VariantCount(alphabet, symbol, inputFlags); variant++)
                {
                    int category = categoryMap.GetSymbolCompositeCategory(symbol, endBoundary: variant != 0);
                    if (category < 0)
                    {
                        if (variant == 0) throw new NotSupportedException("Missing initial composite mapping has no bounded native geometry lookup.");
                        break;
                    }
                    if (category > 63) throw new NotSupportedException("Native composite category requires six bits.");
                    for(int successor=0;successor<arcs.Count;successor++)
                    {
                        var next=LegacyEnglishPeriodicSuccessorScan.Read(arcs,successor,periodicRows,cancellationToken);if(next.NodeOffset-1!=sourceSpan)continue;int ordinal=next.ArcIndex;
                        cancellationToken.ThrowIfCancellationRequested();
                        ushort arc = checked((ushort)ordinal);
                        periodicRows.ChargeRequest();
                        var request = LegacyNativeWordParentRequestFactory.Create(endingNode, sourceSpan, previousEndingNode,
                            parent, arc, symbol, category, tag, length, flags, header, parentHeader, permission2309,
                            categories, cell, BinaryPrimitives.ReadInt32LittleEndian(row[16..]), residue, cancellationToken);
                        calls++;
                        if (!geometry.Calculate118(request, cancellationToken) || !geometry.Validate140(request, cancellationToken) ||
                            !scorer.Score(request, getOwnedGroupCosts, frontendSuppressionEnabled, isSuppressedE8, cancellationToken)) continue;
                        //1fcf40: for kind!=7 and low request3c==0, neither the
                        //source boundary exception nor symbolic control-root guard applies.
                        if (request.ReadByte(28) == 7)
                        {
                            if (globalPathGuard is null || knowledgeMarkerA0 is null)
                                throw new NotSupportedException("Owned kind7 source and actual knowledge policy required.");
                            if (!globalPathGuard.AllowsSoftHyphenContinuation(request, context,
                                knowledgeMarkerA0 != 0, cancellationToken)) continue;
                        }
                        else if (request.ReadByte(28) is not (0 or 2))
                            throw new NotSupportedException("Native ordinary path guard requires kind0/2 or owned fresh kind7.");
                        else if(request.ReadByte(60)!=0)
                        {
                            if(globalPathGuard is null)throw new NotSupportedException("Native ordinary path guard requires the proved unmarked kind0/2 branch.");
                            if(!globalPathGuard.AllowsMarked(request,cancellationToken))continue;
                        }
                        int start = unchecked((int)graph.GetNode(endingNode)[ordinal].StartContextId);
                        bool flagged = !suppressHeaderFlag22f7 &&
                            (kind == 0 && (parent[5] & 0x80) != 0 || alphabet.IsSymbolFlagged(symbol));
                        var fields = new LegacyInitialWordRecordFields(tag, context, cell.Value, request.ReadInt32(0x30), symbol,
                            endingNode, arc, arc, checked((byte)category), previousRowOrdinal, unchecked((sbyte)sourceSpan), 0,
                            flagged ? (byte)1 : (byte)0, 0, 0, request.ReadByte(28), unchecked((byte)((flags >> 5 & 3) + length)),
                            0, 0, (flags & 8) != 0 ? start : parentAuxiliary30, kind == 2 ? start : parentAuxiliary34,
                            anchorNode, anchorArc);
                        if(sourceId!=1||secondaryScale!=0||suppressPreviousContext2609)throw new NotSupportedException("Same ordinary source1/scale0/previous-history route required.");
                        var allocation = router.TryInsertPeriodicInitialRecord(fields,periodicRows,cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (allocation.Accepted) accepted++;
                    }
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(true, visited, qualified, calls, accepted);

        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

}

// Pure pre-read scan primitive; no SOURCE/WORD authority is issued here.
internal interface ILegacyEnglishPeriodicSuccessorScanBudget
{void BeforeSuccessorRead(CancellationToken token);}
internal static class LegacyEnglishPeriodicSuccessorScan
{
    // Eight logical units for the two-int32 value for one successor edge read/test, independent of emission.
    internal const long VisitsPerRead=8;
    internal static LegacyCursiveCandidateSuccessor Read(IReadOnlyList<LegacyCursiveCandidateSuccessor> successors,int ordinal,ILegacyEnglishPeriodicSuccessorScanBudget budget,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();budget.BeforeSuccessorRead(token);
        token.ThrowIfCancellationRequested();return successors[ordinal];
    }
}
