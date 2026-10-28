[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishPostTextCoverage.ActualEntry")]
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishPostTextCoverage.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphPostTextChoiceData(int Ordinal,int[] CharacterNodes,ulong[] InitialPairs,ulong[] RemainingPairs);
    internal sealed record GraphPostTextCoverageData(int SelectedOrdinal,int LineCount,int CheckedChoices,int CheckedLinks,
        ulong[] WordUnionPairs,GraphPostTextChoiceData[] Choices,long NextSelectionId,int PendingCount,
        bool DocumentApplied,string Scope,string RemainingFrontier);
    internal sealed record GraphPostTextCoverageResult(GraphTextPublicationResult Publication,GraphPostTextCoverageData Coverage);
    private bool graphPostTextCoverageRequested;
    private GraphPostTextCoverageData? graphPostTextCoverageData;

    //025E40/027670/0248D0/01C740 ordinary one-line branch. This is not a
    //selected-document issuer: the original INPUT/C20 bootstrap is absent.
    internal GraphPostTextCoverageResult RunGraphFirstPostTextCoverage(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);ArgumentNullException.ThrowIfNull(normalization);
        if(graphPostTextCoverageRequested)throw new InvalidOperationException("One live post-TEXT pass required.");
        graphPostTextCoverageRequested=true;
        var publication=RunGraphFirstTextPublication(normalization,token,observer,captureSourceInputs);
        return new(publication,graphPostTextCoverageData??throw new InvalidOperationException("No completed live source coverage."));
    }

    internal sealed partial class GraphWordPublicationContext
    {
        private record PostTextLink(GraphCharacterPayload Payload,int Marker);
        // Carry-only CURRENT input provenance. Canonical adjusted Marker remains separate.
        private sealed record RichPostTextLink(GraphCharacterPayload Payload,int Marker,ulong CurrentSourceReference,
            uint SelectedInputOrdinal,ushort SelectedGlyphOrdinal):PostTextLink(Payload,Marker);
        private sealed record PostTextChoice(LegacyNativeInkPositionRange[] Ordinary,PostTextLink[] Links);
        private PostTextChoice?[]? postTextChoices;
        private List<PostTextLink>? postTextLinks;
        private bool postTextFinished;

        private void PreparePostTextState(CancellationToken token)
        {
            owner.RequireGraphPublication(Stage,token);
            if(!owner.graphTextPublicationRequested||postTextChoices is not null||!MatchesGraphSelectionBindingAtPreparation())
                throw new InvalidOperationException("Fresh raw-document pending-TEXT preparation required.");
            //Array slots, one holder/choice, list+final link arrays, link holders,
            //and final diagnostic arrays. Logical payload, not CLR/RSS bytes.
            Reserve(checked(16384+Count*256L+TotalReferences*256L),
                checked(32768+Count*512L+TotalReferences*512L),checked(8192+TotalReferences*256L));
            postTextChoices=new PostTextChoice?[Count];
        }
        private void BeginPostTextChoice(CancellationToken token)
        {
            Require(token);
            if(postTextChoices is null||postTextChoices[current] is not null||postTextLinks is not null)
                throw new InvalidOperationException("Next private post-TEXT choice required.");
            postTextLinks=new(CurrentReferences(token).Count);
        }
        private void RetainPostTextLink(GraphCharacterSource source,GraphCharacterPayload payload,int marker,byte[] currentSelectionRows,CancellationToken token)
        {
            Require(token);
            if(postTextLinks is null||links is null||postTextLinks.Count+1!=links.Count||
                !ReferenceEquals(payload.Context,owner.graphCharacterContext)||!ReferenceEquals(payload.Context.Document,Document)||
                !ReferenceEquals(payload.Context.Selections,Selections)||!payload.Complete)
                throw new InvalidOperationException("Exact just-appended live CHAR required.");
            if(owner.graphPeriodicActive&&owner.graphOriginalSelectedRequested&&owner.graphTextPublicationRequested)
            {
                // BEFORE any new source/selection reads or derived-record allocation.
                Reserve(128,256,1024);
                if(!ReferenceEquals(source.Context,payload.Context)||source.Index!=postTextLinks.Count)
                    throw new InvalidOperationException("Exact CURRENT source/link position required.");
                uint input=source.Selection.SelectedMarker??throw new InvalidOperationException("Actual selected input position required.");
                if(currentSelectionRows.Length%4!=0||input>=(uint)(currentSelectionRows.Length/4))
                    throw new InvalidDataException("Same CURRENT selected input row extent required.");
                ushort ordinal=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(currentSelectionRows.AsSpan(checked((int)input*4),2));
                postTextLinks.Add(new RichPostTextLink(payload,marker,source.Reference,input,ordinal));
            }
            else postTextLinks.Add(new(payload,marker));
        }
        private void SealPostTextChoice(CancellationToken token)
        {
            Require(token);
            if(postTextChoices is null||postTextLinks is null||postTextLinks.Count!=CurrentReferences(token).Count)
                throw new InvalidOperationException("Whole current live CHAR vector required.");
            //ordinary is private and replaced, never mutated, at the next Open.
            //The public word result receives Pairs(ordinary), a separate array.
            postTextChoices[current]=new(ordinary,postTextLinks.ToArray());postTextLinks=null;
        }
        private void ReservePostTextRangeCall(int rows,int cuts)
        {
            if(rows<0||cuts<0||rows>65536-cuts)
                throw new NotSupportedException("Actual interval extent exceeds source-operation domain.");
            long n=checked((long)rows+cuts);
            //Copy(target), List backing and growth/Split, final ToArray, key
            //clone and exported pair arrays. Each cut can add at most one row.
            //The n*cuts terms cover repeated searches/shifts, not CPU cycles.
            Reserve(checked(2048+128*n),checked(4096+256*n+32*n*cuts),checked(4096+512*n*(cuts+1L)));
        }
        private void RequirePostTextPayload(PostTextLink link,GraphCharacterContext context,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();var p=link.Payload;
            if(!ReferenceEquals(p.Context,context)||!ReferenceEquals(context.Document,Document)||
                !ReferenceEquals(context.Selections,Selections)||!ReferenceEquals(p.Node.Owner,context.Interner)||
                !ReferenceEquals(p.Node.Owner.Document,Document)||!p.Complete||p.LineId!=0||
                p.AssignedDefaults?.Field40!=-1||(p.Written&GraphCharacterWritten.EmptyPriorVector)==0||
                (uint)link.Marker>=(uint)p.Reuse.RecordCount)
                throw new NotSupportedException("Actual fresh same-document CHAR line0/empty prior vector required.");
        }
        internal GraphPostTextCoverageData CompletePostTextCoverage(CancellationToken token)
        {
            Require(token);var context=owner.graphCharacterContext??throw new InvalidOperationException("Live CHAR context required.");
            context.RequireCompleted(token);
            if(postTextFinished||!finished||textPhase!=4||textKey is null||textLines is null||textLineFrame is null||
                postTextChoices is null||postTextLinks is not null||head.Next is not null||!MatchesGraphSelectionBinding(Selections,token)||
                textLines.Lines.Length!=1||textLines.NextWrapperLineOffset!=1)
                throw new NotSupportedException("One completed raw pending-TEXT/one WORD/one line domain required.");
            textLineFrame.Require(Stage,token);
            var lineInput=textLineFrame.Input(Stage,token);
            if((lineInput.Flags&15)!=0||lineInput.SourceLineId!=0)
                throw new NotSupportedException("Actual ordinary finalized source line0 required.");
            if((flags&0x200000)!=0)throw new NotSupportedException("Unregistered WORD post-TEXT branch is not connected.");
            //Precharge the full validation traversal BEFORE reading link vectors.
            Reserve(checked(8192+Count*128L+TotalReferences*32L),checked(16384+Count*256L+TotalReferences*64L),
                checked(8192+Count*128L+TotalReferences*512L));
            long pendingNext=Selections.NextSelectionId;
            var pendingBefore=Selections.CopyPendingSelections();
            if(pendingBefore.Count<1||pendingBefore.Count>TotalReferences+2||pendingBefore[^1].Name!="TEXT")
                throw new InvalidOperationException("Exact completed CHAR/WORD/TEXT pending prefix required.");
            int checkedLinks=0;
            for(int c=0;c<Count;c++)
            {
                token.ThrowIfCancellationRequested();var choice=postTextChoices[c]??throw new InvalidOperationException("Missing live choice.");
                if(choice.Links.Length!=span.Words[c].FlatReferences.Count||choice.Links.Length==0)
                    throw new InvalidOperationException("Exact retained native link vector required.");
                foreach(var link in choice.Links){RequirePostTextPayload(link,context,token);checkedLinks++;}
            }
            //025E40: selected last CHAR line0, all alternative last lines0,
            //and adjacent lines equal: no025940/024A90; trim target is one.
            //027670: exactly one WORD link returns before next-WORD inspection.
            //026090 is excluded only in this fresh no-property 22A=false domain
            //(native proof dependency), not by guessing the outer vector count.
            if(checkedLinks!=TotalReferences)throw new InvalidOperationException("Whole275 live coverage required.");
            ReservePostTextRangeCall(0,originalKey.Length);
            var union=operations.Union([],originalKey,token);
            ReservePostTextRangeCall(union.Length,textKey.Length);
            if(!LegacyOwnedRawInkRangeOperations.Equal(union,textKey))
                throw new NotSupportedException("Unequal WORD/TEXT source union requires native0238B0.");
            var data=new GraphPostTextChoiceData[Count];
            //Native01C740 visits choices in reverse, CHAR links forward.
            for(int c=Count-1;c>=0;c--)
            {
                token.ThrowIfCancellationRequested();var choice=postTextChoices[c]!;
                var start=choice.Ordinary.Length==0?originalKey:choice.Ordinary;
                ReservePostTextRangeCall(start.Length,0);var remaining=start.ToArray();
                var initial=Pairs(start);var nodes=new int[choice.Links.Length];
                for(int i=0;i<choice.Links.Length;i++)
                {
                    var link=choice.Links[i];RequirePostTextPayload(link,context,token);
                    int cuts=link.Payload.RetainedRangeCount;ReservePostTextRangeCall(remaining.Length,cuts);
                    var key=link.Payload.CopyRetainedRanges();
                    if(key.Length!=cuts)throw new InvalidOperationException("Private CHAR key changed.");
                    remaining=operations.Subtract(remaining,key,token);nodes[i]=link.Payload.Node.Identity;
                }
                if(remaining.Length!=0&&Count>1)
                    throw new NotSupportedException("Residual choice removal requires native024A90 transaction handling.");
                ReservePostTextRangeCall(remaining.Length,0);data[c]=new(c,nodes,initial,Pairs(remaining));
            }
            Require(token);context.RequireCompleted(token);textLineFrame.Require(Stage,token);
            var pendingAfter=Selections.CopyPendingSelections();
            if(Selections.NextSelectionId!=pendingNext||pendingBefore.Count!=pendingAfter.Count)
                throw new InvalidOperationException("Post-TEXT checks must preserve the pending transaction.");
            for(int i=0;i<pendingBefore.Count;i++)
            {
                token.ThrowIfCancellationRequested();
                if(!ReferenceEquals(pendingBefore[i],pendingAfter[i]))
                    throw new InvalidOperationException("Pending command identity changed during coverage.");
            }
            //No source command is appended and no public DATA can become an issuer.
            postTextFinished=true;
            return new(SelectedOrdinal,1,Count,checkedLinks,Pairs(union),data,Selections.NextSelectionId,
                pendingAfter.Count,false,"SAME_EN_LIVE_POST_TEXT_025E40_027670_0248D0_01C740_ORDINARY_LINE0",
                owner.graphOriginalSelectedRequested?"Original-selected bootstrap retained;01FD40/020000 and document apply remain unissued.":
                "Original-selected INPUT/C20 bootstrap,01FD40/020000 and document apply remain unissued.");
        }
    }
}
