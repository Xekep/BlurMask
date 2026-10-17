using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyContextualQuoteRules.Probe")]

namespace InkRecognition;

//Numeric decisions inside enabled21F320. The eventual owning caller must
//supply actual AL/controller labels, source graph kind and scalar reads across
//WORD boundaries. This kernel cannot enable the fresh F4=0 body itself.
internal readonly record struct LegacyContextualQuoteLabels(ushort Apostrophe,ushort DoubleQuote,
    ushort OpenDouble,ushort CloseDouble,ushort OpenSingle,ushort CloseSingle,ushort LowDouble)
{
    internal bool HasEnabledReplacement => CloseSingle!=ushort.MaxValue||
        OpenSingle!=ushort.MaxValue||CloseDouble!=ushort.MaxValue;
}

internal static class LegacyPreparedContextualQuoteRules
{
    private static bool Control(ushort label)=>unchecked((ushort)(label+7))<4;
    private static bool Ordinary(ushort label,LegacyContextualQuoteLabels labels)=>
        label==labels.Apostrophe||label==labels.DoubleQuote;
    private static ushort Open(ushort label,LegacyContextualQuoteLabels labels)=>
        label==labels.Apostrophe?labels.OpenSingle:labels.OpenDouble;
    private static ushort Close(ushort label,LegacyContextualQuoteLabels labels)=>
        label==labels.Apostrophe?labels.CloseSingle:labels.CloseDouble;
    private static bool Source(short arc,byte groupKind)=>arc>=0&&groupKind==0;

    internal static ushort Leading(ushort current,ushort next,short arc,byte groupKind,
        LegacyContextualQuoteLabels labels)=>Source(arc,groupKind)&&Ordinary(current,labels)&&!Control(next)
            ?Open(current,labels):current;

    internal static ushort Trailing(ushort previous,ushort current,short arc,byte groupKind,
        bool lowDoubleCarry,LegacyContextualQuoteLabels labels)
    {
        if(!Source(arc,groupKind)||!Ordinary(current,labels)||Control(previous))return current;
        ushort output=Close(current,labels);
        return lowDoubleCarry&&output==labels.CloseDouble?labels.OpenDouble:output;
    }

    //Previous is the retained, possibly rewritten scalar, not an acoustic
    //label reread. Category is used only while previous<actual AL labelcount.
    //When both neighbours are FFF9..FFFC native leaves the quote unchanged.
    internal static (ushort Label,bool Carry) Interior(ushort previous,ushort current,ushort next,
        short arc,byte groupKind,ushort alphabetCount,int previousCategory,
        bool lowDoubleCarry,LegacyContextualQuoteLabels labels)
    {
        ushort output=current;bool carry=lowDoubleCarry;
        if(Source(arc,groupKind)&&Ordinary(current,labels))
        {
            if(previous<alphabetCount&&previousCategory==1)output=Open(current,labels);
            else if(!Control(previous))
            {
                output=Close(current,labels);
                if(carry&&output==labels.CloseDouble){output=labels.OpenDouble;carry=false;}
            }
            else if(!Control(next))output=Open(current,labels);
        }
        return(output,carry||(labels.LowDouble!=ushort.MaxValue&&output==labels.LowDouble));
    }
}
