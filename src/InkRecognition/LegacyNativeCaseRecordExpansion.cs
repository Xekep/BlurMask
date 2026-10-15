using System.Buffers.Binary;
namespace InkRecognition;

public sealed record LegacyNativeCaseExpandedRecord(ReadOnlyMemory<byte> Record, int CostDelta);

/// <summary>233090 ordinary-frontend record32 expansion. The backend query/paging
/// and source-locale ownership are separate; no script-confusable substitution occurs.</summary>
public sealed class LegacyNativeCaseRecordExpansion
{
    public LegacyNativeCasePreparation Preparation {get;}
    public LegacyNativeCasePathPolicy Policy {get;}
    public LegacyNativeCaseContextRewrite Rewrite {get;}
    public LegacyNativeCaseRecordExpansion(LegacyNativeCasePreparation preparation,
        LegacyNativeCasePathPolicy policy,LegacySymbolContextFactory contexts)
    {
        ArgumentNullException.ThrowIfNull(preparation);ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(contexts);Preparation=preparation;Policy=policy;
        Rewrite=new(preparation,contexts);
    }

    /// <summary>Native pre-skip counting precedes Dutch special-state adjustment
    /// and context allocation. A skip falling inside one row's alternatives skips
    /// that entire row; this count must not be replaced with output-array slicing.</summary>
    public int CountForSkip(ReadOnlySpan<byte> row,int inputState)
    {
        Validate(row,inputState);
        byte kind=Classification(row,inputState);bool both=(row[5]&3)==3;
        bool Has(int col)=>Policy.ReadTransition(inputState,col)!=255;
        return kind switch
        {
            1 => (Has(3)?1:0)+(Has(6)?1+(both?1:0):0),
            2 => (Has(3)?1:0)+(Has(7)?1+(both?1:0):0)+(Has(8)?1+(both?1:0):0),
            3 => (Has(4)?1:0)+(Has(10)?1:0),
            4 => (Has(5)?1:0)+(Has(9)?1:0)+(Has(10)?1:0),
            _ => 1
        };
    }

    public IReadOnlyList<LegacyNativeCaseExpandedRecord> Expand(ReadOnlySpan<byte> row,
        int inputState,ulong contextContainer,ulong startParent,bool dutchLigatureMode,
        out int nextState,CancellationToken cancellationToken=default)
    {
        Validate(row,inputState);cancellationToken.ThrowIfCancellationRequested();
        if(contextContainer!=0 && contextContainer!=Rewrite.Contexts.ContainerIdentity)
            throw new ArgumentException("Case rewrite requires the same symbolic container.");
        byte[] original=row.ToArray();ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
        bool alternate=Policy.UseAlternateForms(inputState),both=(row[5]&3)==3;
        int state=inputState;
        if(dutchLigatureMode && symbol!=Preparation.SmallJ && state==Policy.ReadTransition(0,14))
            state=Policy.ReadTransition(0,6);
        Policy.ValidateState(state);nextState=state;
        bool specialI=dutchLigatureMode && (row[5]&1)!=0 && symbol==Preparation.SmallI;
        byte kind=Preparation.ReadClassification(symbol,alternate);
        var result=new List<LegacyNativeCaseExpandedRecord>(5);
        int costDelta=0;
        //233090 changes the high record byte with &3, unlike233FC0's &83.
        //Generated terminal state31 can be observable; a subsequent request for
        //that state must still fail table bounds instead of reading outside data.
        void Emit(int stateColumn,int form,int delta=0)
        {
            byte[] value=original.ToArray();byte selected=Policy.ReadTransition(state,stateColumn);
            value[7]=(byte)((value[7]&3)|((selected&31)<<2));
            if(form>=0)
            {
                if(!Preparation.TryReadForms(symbol,alternate,out var forms))
                    throw new NotSupportedException("Initialized stored form row is required by the native case branch.");
                ushort mapped=form switch {0=>forms.First,1=>forms.Second,_=>forms.Third};
                BinaryPrimitives.WriteUInt16LittleEndian(value.AsSpan(20),mapped);
                ulong leaf=BinaryPrimitives.ReadUInt64LittleEndian(original.AsSpan(8));
                ulong context=contextContainer==0?0:Rewrite.Rewrite(startParent,leaf,form,alternate,cancellationToken);
                BinaryPrimitives.WriteUInt64LittleEndian(value.AsSpan(8),context);
            }
            int adjustment=unchecked(costDelta+delta);
            BinaryPrimitives.WriteInt32LittleEndian(value.AsSpan(16),unchecked(BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(16))+adjustment));
            result.Add(new(value,adjustment));
        }
        bool Has(int col)=>Policy.ReadTransition(state,col)!=255;
        switch(kind)
        {
            case 1:
                if(Has(3))Emit(3,-1);
                int lower=specialI?14:6;
                if(Has(lower))
                {costDelta=1;Emit(lower,1);if(both)Emit(11,1);}
                break;
            case 2:
                if(Has(3))Emit(3,-1);
                costDelta=1;
                if(Has(7)){Emit(7,1);if(both)Emit(12,1);}
                if(Has(8)){Emit(8,2);if(both)Emit(13,2);}
                break;
            case 3:
                if(Has(4))Emit(4,-1);
                if(Has(10))Emit(10,0,state==0?8192:0);
                break;
            case 4:
                if(Has(5))Emit(5,-1);
                if(Has(9)){costDelta=1;Emit(9,2);}
                if(Has(10))Emit(10,0,state==0?8192:0);
                break;
            case 5:Emit(0,-1);break;
            case 6:Emit(1,-1);break;
            case 7:
                byte[] preserved=original.ToArray();preserved[7]=(byte)((preserved[7]&3)|(state<<2));
                result.Add(new(preserved,0));break;
            default:Emit(2,-1);break;
        }
        cancellationToken.ThrowIfCancellationRequested();return result.AsReadOnly();
    }
    private byte Classification(ReadOnlySpan<byte> row,int state)=>Preparation.ReadClassification(
        BinaryPrimitives.ReadUInt16LittleEndian(row[20..]),Policy.UseAlternateForms(state));
    private void Validate(ReadOnlySpan<byte> row,int state)
    {if(row.Length!=32)throw new ArgumentException("Native complete record32 required.");Policy.ValidateState(state);}
}
