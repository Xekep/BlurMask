using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishTextPublication.Probe")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal sealed record GraphTextCommandData(long SelectionId,string Name,int CommandIdentity,int PreviousCommandIdentity,byte OperationCode,byte Enabled,int Category,ulong[] SourcePairs);
    internal sealed record GraphTextPublicationData(long SelectionId,long NextSelectionId,ulong[] NullSelectedPairs,int NextWrapperLineOffset,
        byte[] SourceDescriptor88,byte[] SourceGuide40,byte[] SourceLines56,byte[] SourceLineKnown56,
        byte[] DocumentChoice32,byte[] DocumentChoiceKnown32,byte[] WordLink24,byte[] WordLinkKnown24,
        byte[][] WordChoiceKnown48,byte[][][] CharacterLinkKnown24,GraphTextCommandData[] PendingCommands,
        string[] Trace,bool CharacterContextAliveAtRegistration,string Scope);
    internal sealed record GraphTextPublicationResult(GraphWordPublicationResult Words,GraphTextPublicationData Text);
    private bool graphTextPublicationRequested;
    private GraphTextPublicationData? graphTextPublicationData;
    //Pending native TEXT membership only. No Unicode flattening/document apply,
    //source coverage025E40, public transcription, quality or admission is issued.
    internal GraphTextPublicationResult RunGraphFirstTextPublication(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);ArgumentNullException.ThrowIfNull(normalization);
        if(graphTextPublicationRequested)throw new InvalidOperationException("One same-owner TEXT registration required.");
        graphTextPublicationRequested=true;
        var words=RunGraphFirstWordPublication(normalization,token,observer,captureSourceInputs);
        return new(words,graphTextPublicationData??throw new InvalidOperationException("No completed private TEXT registration."));
    }

    internal sealed partial class GraphWordPublicationContext
    {
        private LegacyEnglish119FinalizedSingleSource.GraphTextLineFrame? textLineFrame;
        private LegacyNativeInkPositionRange[]? textKey;
        private LegacyNativeSourceLinePublication? textLines;
        private GraphTextSelectionReceipt? currentText;
        private int textPhase; //0 unprepared,1 early NULL key,2 lines,3 registration,4 complete.
        private void PrepareTextInputs(CancellationToken token)
        {
            //Constructor-time source-only preparation: parent has not escaped or
            //been installed yet. No callback occurs; explicit stage/source checks
            //replace Require's installed-parent predicate at this ONE point.
            owner.RequireGraphPublication(Stage,token);
            if(!owner.graphTextPublicationRequested||textPhase!=0||!ReferenceEquals(Selections.Document,Document))
                throw new InvalidOperationException("One genuine fresh TEXT input preparation required.");
            int n=Document.ObjectCount;if(n is<1 or>256)throw new NotSupportedException("Bounded whole original selected inputs required.");
            Reserve(checked(16384+n*1024L),checked(32768+n*2048L),checked(8192+n*512L));
            textLineFrame=Stage.Lease.Root.Knowledge.Owner.Owner.ReadGraphTextLineFrame(Stage,token);
            var selected=Document.CopySelectedInputs();if(selected.Length!=n)throw new InvalidDataException("Whole same-document selection required.");
            LegacyNativeInkPositionRange[] key=[];
            for(int i=0;i<selected.Length;i++)
            {
                token.ThrowIfCancellationRequested();var input=selected[i];
                if(input.IsKnownText||input.SourceIndex!=(uint)i||input.PointCount!=Document.Strokes[i].Points.Count)
                    throw new NotSupportedException("Actual fresh full original selected input rows required.");
                //NULL05AFA0 enumerates selected40 rows with NO overrides. This
                //is NOT map.ProjectNormalReferences(nonnullEmptyArray).
                var projected=LegacyNativeTextSourceRangeProjection.Project(input);
                if(projected.First.Mode==4)continue;
                long k=key.Length+1L;
                Reserve(checked(2048+128*k),checked(4096+256*k+48*k*k),checked(2048+128*k*k));
                key=operations.Union(key,[projected],token);
            }
            textKey=key;textPhase=1;trace.Add("NULL-selected-inputs-prepared");
        }
        internal GraphTextPublicationData CompleteText(CancellationToken token)
        {
            Require(token);
            var characters=owner.graphCharacterContext??throw new InvalidOperationException("Actual live CHAR context required.");
            characters.RequireCompleted(token);
            if(!finished||textPhase!=1||textKey is null||textLineFrame is null||currentText is not null||
                !ReferenceEquals(characters.Document,Document)||!ReferenceEquals(characters.Selections,Selections))
                throw new InvalidOperationException("Same finalized WORD and live CHAR transaction required.");
            textLineFrame.Require(Stage,token);
            //Fresh one-alternative wrapper offset0 and zero existing lines.
            //LineId and fields come from the exact finalized source218 frame;
            //sampler baseline/height0/4 or glyph bounds never substitute.
            Reserve(checked(32768+Count*128L+TotalReferences*128L+textKey.Length*128L),
                checked(65536+Count*256L+TotalReferences*256L+textKey.Length*256L),checked(16384+TotalReferences*512L));
            var input=textLineFrame.Input(Stage,token);var guide=textLineFrame.Guide(Stage,token);
            textLines=LegacyNativeSourceLinePublication.Append([],0,[input],[guide],textLineFrame.Rotation(Stage,token),token);
            textPhase=2;trace.Add("source-lines021AB0");
            currentText=GraphTextSelectionReceipt.Issue(this,token);textPhase=3;
            var registered=Selections.CreateEnglishText(currentText,token);
            if(registered is null)throw new NotSupportedException("TEXT registration refused; native pending/counter prefix retained.");
            characters.RequireCompleted(token);Require(token);textLineFrame.Require(Stage,token);
            textPhase=4;trace.Add("TEXT-registered");
            var choice=new byte[32];BinaryPrimitives.WriteUInt32LittleEndian(choice.AsSpan(8),output[SelectedOrdinal].Flags&1023);
            BinaryPrimitives.WriteInt32LittleEndian(choice.AsSpan(24),1);BinaryPrimitives.WriteInt32LittleEndian(choice.AsSpan(28),1);
            var wordLink=new byte[24];BinaryPrimitives.WriteInt32LittleEndian(wordLink.AsSpan(16),SelectedOrdinal);
            //All rows start zero; actual pointer stores are UNKNOWN, represented
            //by typed source/node sidecars and excluded from these known masks.
            var wordChoiceKnown=output.Select(_=>KnownWithPointer(48,16)).ToArray();
            var charLinkKnown=output.Select(c=>c.Links.Select(_=>KnownWithPointer(24,8)).ToArray()).ToArray();
            byte[] lines=LegacyFreshMainSourceLinePublication.Encode(textLines.Lines);var lineKnown=new byte[lines.Length];lineKnown.AsSpan().Fill(255);
            var commands=Selections.CopyPendingSelections().Select(x=>new GraphTextCommandData(x.SelectionId,x.Name,x.CommandIdentity,x.PreviousCommandIdentity,x.OperationCode,x.Enabled,x.Category,Pairs(x.CopyRanges()))).ToArray();
            return new(registered.SelectionId,Selections.NextSelectionId,Pairs(textKey),textLines.NextWrapperLineOffset,textLineFrame.CopyDescriptor(Stage,token),textLineFrame.CopyGuide(Stage,token),
                lines,lineKnown,choice,KnownWithPointer(32,16),wordLink,KnownWordLink24(),wordChoiceKnown,charLinkKnown,commands,trace.ToArray(),true,
                "SAME_EN_ALL275_WORD_LINES_NULL_SELECTED_PENDING_TEXT_NO_DOCUMENT_APPLY");
        }
        internal LegacyNativeInkPositionRange[] IssueTextKey(CancellationToken token)
        {
            Require(token);owner.graphCharacterContext!.RequireCompleted(token);
            if(!finished||textPhase!=2||currentText is not null||textKey is null||textLines is null)
                throw new InvalidOperationException("Actual source-lines prefix required before TEXT receipt.");
            Reserve(checked(4096+textKey.Length*32L),checked(8192+textKey.Length*64L),checked(4096+textKey.Length*64L));return textKey.ToArray();
        }
        internal void RequireTextReceipt(GraphTextSelectionReceipt actual,LegacyOwnedFreshTextSourceContext selections,CancellationToken token)
        {
            Require(token);owner.graphCharacterContext!.RequireCompleted(token);
            if(!finished||textPhase!=3||!ReferenceEquals(currentText,actual)||!ReferenceEquals(actual.Owner,this)||
                !ReferenceEquals(selections,Selections)||!ReferenceEquals(selections.Document,Document)||!MatchesGraphSelectionBinding(selections,token))
                throw new InvalidOperationException("Exact current English raw-root TEXT receipt/selection context required.");
        }
    }
    internal sealed class GraphTextSelectionReceipt
    {
        internal GraphWordPublicationContext Owner{get;}
        private readonly LegacyNativeInkPositionRange[] key;
        private GraphTextSelectionReceipt(GraphWordPublicationContext parent,LegacyNativeInkPositionRange[] actual){Owner=parent;key=actual;}
        internal static GraphTextSelectionReceipt Issue(GraphWordPublicationContext parent,CancellationToken token)=>new(parent,parent.IssueTextKey(token));
        internal LegacyNativeInkPositionRange[] CopyKey(LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {Owner.RequireTextReceipt(this,actual,token);return key.ToArray();}
    }
    //Pure numeric1692B0 source-guide offsets, never an owning frame issuer.
    internal static LegacyNativeSourceLineGuide ReadGraphTextGuide(ReadOnlySpan<byte> g)
    {
        if(g.Length!=40)throw new ArgumentException("Complete actual guide40 required.");
        return new(BinaryPrimitives.ReadSingleLittleEndian(g[8..]),BinaryPrimitives.ReadSingleLittleEndian(g[12..]),
            BinaryPrimitives.ReadSingleLittleEndian(g[16..]),BinaryPrimitives.ReadSingleLittleEndian(g[20..]),BinaryPrimitives.ReadSingleLittleEndian(g[24..]));
    }
    //Native02930F/316/329/34D stack link writes only0..20;21..23 remain unknown.
    internal static byte[] KnownWordLink24()
    {var b=KnownWithPointer(24,8);b.AsSpan(21,3).Clear();return b;}
    internal static byte[] KnownWithPointer(int count,int pointer)
    {if(!((count==24&&pointer==8)||(count==32&&pointer==16)||(count==48&&pointer==16)))throw new ArgumentOutOfRangeException();var b=new byte[count];b.AsSpan().Fill(255);b.AsSpan(pointer,8).Clear();return b;}
}

public sealed partial class LegacyEnglish119FinalizedSingleSource
{
    //A NEW private-source companion. Neither public CopyGuide data nor caller
    //geometry can issue this frame. Original descriptor/guide remain immutable.
    internal GraphTextLineFrame ReadGraphTextLineFrame(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
        =>GraphTextLineFrame.Issue(this,stage,token);
    internal sealed class GraphTextLineFrame
    {
        private readonly LegacyEnglish119FinalizedSingleSource source;
        private readonly LegacyEnglishCaseNode0Completion.GraphLanguageScope stage;
        private readonly byte[] d,g;
        private GraphTextLineFrame(LegacyEnglish119FinalizedSingleSource source,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage)
        {this.source=source;this.stage=stage;d=source.descriptor.ToArray();g=source.guide.ToArray();}
        internal static GraphTextLineFrame Issue(LegacyEnglish119FinalizedSingleSource source,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
        {
            stage.Owner.RequireGraphPublication(stage,token);
            if(!ReferenceEquals(stage.Lease.Root.Knowledge.Owner.Owner,source)||!ReferenceEquals(stage.Lease.Root.Source,source.MainWire.Source))
                throw new InvalidOperationException("Exact live English finalized-source/model frame required.");
            lock(source.gate)
            {
                token.ThrowIfCancellationRequested();if(!source.complete||source.faulted||!source.localCandidatesReserved||source.descriptor.Length!=88||source.guide.Length!=40)
                    throw new InvalidOperationException("Completed immutable finalized descriptor88/guide40 required.");
                if((source.descriptor[0]&15)!=0||BinaryPrimitives.ReadInt32LittleEndian(source.descriptor.AsSpan(16))!=0)
                    throw new NotSupportedException("Actual single assigned ordinary line required.");
                return new(source,stage);
            }
        }
        internal void Require(LegacyEnglishCaseNode0Completion.GraphLanguageScope actual,CancellationToken token)
        {
            actual.Owner.RequireGraphPublication(actual,token);
            if(!ReferenceEquals(stage,actual)||!ReferenceEquals(actual.Lease.Root.Knowledge.Owner.Owner,source)||!source.IsComplete||source.IsFaulted)
                throw new InvalidOperationException("Same live finalized-source frame required.");
        }
        internal LegacyNativeSourceLineInput Input(LegacyEnglishCaseNode0Completion.GraphLanguageScope actual,CancellationToken token)
        {Require(actual,token);return new(d[0],BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(16)));}
        internal LegacyNativeSourceLineGuide? Guide(LegacyEnglishCaseNode0Completion.GraphLanguageScope actual,CancellationToken token)
        {Require(actual,token);return LegacyEnglishCaseNode0Completion.ReadGraphTextGuide(g);}
        internal float Rotation(LegacyEnglishCaseNode0Completion.GraphLanguageScope actual,CancellationToken token)
        {Require(actual,token);return 0f;} //Fresh typed parent has no property/rotation ingress; conditional default-native domain only.
        internal byte[] CopyDescriptor(LegacyEnglishCaseNode0Completion.GraphLanguageScope actual,CancellationToken token){Require(actual,token);return d.ToArray();}
        internal byte[] CopyGuide(LegacyEnglishCaseNode0Completion.GraphLanguageScope actual,CancellationToken token){Require(actual,token);return g.ToArray();}
    }
}
public sealed partial class LegacyOwnedFreshTextSourceContext
{
    internal LegacyOwnedFreshTextSelection? CreateEnglishText(LegacyEnglishCaseNode0Completion.GraphTextSelectionReceipt actual,CancellationToken token)
    {
        actual.Owner.RequireTextReceipt(actual,this,token);var key=actual.CopyKey(this,token);
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(textRegistrationEntered)throw new InvalidOperationException("This transaction already entered TEXT registration.");
            textRegistrationEntered=true;long output=-1;return CreateCore(key,2,ref output,0,token);
        }
    }
}
