namespace InkRecognition;

/// <summary>Owned wrapper292 context suffix remapping232F10 using a prepared stored form table.</summary>
public sealed class LegacyNativeCaseContextRewrite
{
    public LegacyNativeCasePreparation Preparation { get; }
    public LegacySymbolContextFactory Contexts { get; }
    public LegacyNativeCaseContextRewrite(LegacyNativeCasePreparation preparation,LegacySymbolContextFactory contexts)
    {ArgumentNullException.ThrowIfNull(preparation);ArgumentNullException.ThrowIfNull(contexts);Preparation=preparation;Contexts=contexts;}

    /// <summary>Walk at most32 source ancestors, then rebuild oldest-first in the same container.
    /// A longer/unrelated path returns native null before insertion. Form ordinal is native byte offset0/2/4.</summary>
    public ulong Rewrite(ulong startParent,ulong leaf,int formOrdinal,bool alternate=false,CancellationToken cancellationToken=default)
    {
        if(formOrdinal is <0 or >2)throw new ArgumentOutOfRangeException(nameof(formOrdinal));
        cancellationToken.ThrowIfCancellationRequested();
        if(startParent!=0)_=Contexts.ReadNode(startParent);
        Span<ushort> suffix=stackalloc ushort[32];int count=0;
        while(leaf!=startParent)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(count==32 || leaf==0)return 0;
            var node=Contexts.ReadNode(leaf);suffix[count++]=node.Symbol;leaf=node.ParentLink;
        }
        //Validate every required stored row before changing the symbolic tree.
        //Native can leave holes uninitialized for other prepared alphabets;
        //those unsupported reads must not become an invented zero form.
        for(int i=0;i<count;i++)
        {
            ushort label=suffix[i];
            if(label<Preparation.Alphabet.LabelCount && Cased(label))
            {
                if(!Preparation.TryReadForms(label,alternate,out var forms))
                    throw new NotSupportedException("Cased source label has no owned initialized form row.");
                suffix[i]=formOrdinal switch {0=>forms.First,1=>forms.Second,_=>forms.Third};
            }
        }
        ulong current=startParent;
        for(int i=count-1;i>=0;i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next=current==0?Contexts.ResolveInitial(suffix[i],cancellationToken):Contexts.ResolveChild(current,suffix[i],cancellationToken);
            if(next.Identity!=0 && next.Created && current!=0)Contexts.LinkCreatedContext(next.Identity,current);
            current=next.Identity;
        }
        return current;
    }
    private bool Cased(ushort label)
    {
        var r=Preparation.Ranges;
        return label>=r.FirstStart && label<=r.FirstEnd || label>=r.SecondStart && label<=r.SecondEnd;
    }
}
