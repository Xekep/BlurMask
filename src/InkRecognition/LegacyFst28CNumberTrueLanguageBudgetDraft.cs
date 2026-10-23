namespace InkRecognition;
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    // SOURCE PROPOSAL ONLY. These are logical protocol/cardinality bounds,
    // never CLR/RSS/CPU/latency limits or accepted executable reservations.
    private readonly record struct WholeBudgetProposal(long AllocationPayload,long PrivateStrongPeak,long Work);
    private static WholeBudgetProposal DeriveCombinedProposal()
    {
        const long k=572763;
        long sourceC=8*88+8*128*32+2000*240+2000*64+72+128*32+57+8+125*20+250*51;
        long numericStorage=2*(250*128+2*250*80+157*88+125*80+157*80);
        long extra=k+32+104+54+256+sourceC+numericStorage+32000+192+1836+2000+32000+4000+120+108+
            4000+(124*128+31*88)+155*64+36+332+512;
        long allocations=55626+754206+extra;
        long strong=55626+754206+numericStorage+120+108+55935+332+256+3008;
        long Sort(long n,long width,long swap)=>(2*n*n+2*n)*64+n*n*swap+2*n*width+4000;
        long work=226858+16264293+3*k+412595010+250*51*4+6*51712+5*51712+(5*512*8+3*36)+(36*30+12+36+250)*30*64+36*51*4+
            Sort(250,128,768)+249*8192+375*5376+250*2*256+125*(48+996+4)+Sort(124,128,768)+123*8192+
            124*5376+32*708+Sort(31,88,1008)+30*1280+250*256+156*256+8*128+25*16+256+3200+2*extra;
        // Exact arithmetic crosscheck pins SOURCE proposal, not readiness.
        if(allocations!=2366404L||strong!=1086343L||work!=514403670L)
            throw new InvalidDataException("SOURCE cardinality derivation differs.");
        return new(allocations,strong,work);
    }
}
