namespace InkRecognition;
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    // SOURCE PROPOSAL ONLY. These are logical protocol/cardinality bounds,
    // never CLR/RSS/CPU/latency limits or accepted executable reservations.
    private readonly record struct WholeBudgetProposal(long AllocationPayload,long PrivateStrongPeak,long Work);
    private static WholeBudgetProposal DeriveCombinedProposal()
    {
        const long k=572763;
        long sourceC=8*88+8*128*32+2000*240+2000*64+72+128*32+57+8+125*20;
        long numericStorage=2*(250*128+2*250*80+157*88+125*80+157*80);
        long extra=k+32+104+54+256+sourceC+numericStorage+32000+192+1836+2000+32000+4000+120+108+
            4000+(124*128+31*88)+155*64+36+332+512;
        long allocations=55626+754206+extra;
        long strong=55626+754206+numericStorage+120+108+55935+332+256+3008;
        long Sort(long n,long width,long swap)=>(2*n*n+2*n)*64+n*n*swap+2*n*width+4000;
        long work=226858+16264293+3*k+412595010+6*51712+5*51712+(5*512*8+3*36)+(36*30+12+36)*30*64+36*51*4+
            Sort(250,128,768)+249*8192+375*5376+250*2*256+125*(48+996+4)+Sort(124,128,768)+123*8192+
            124*5376+32*708+Sort(31,88,1008)+30*1280+250*256+156*256+8*128+25*16+256+3200+2*extra;
        // Shadow readset SOURCE successor: no deep validation removed.
        const long frozenPayload=6902,frozenMetadata=160,snapshotAlloc=7490;
        const long postCalls=6252,shadowChecks=19514;
        long addedB=frozenPayload+frozenMetadata+snapshotAlloc;
        long addedW=(postCalls+2)*51712+shadowChecks*(16*64+128)+
            30*30*64+30*51*4+16*64+98*128+299*128+frozenPayload*2+
            2000*12*32+250*30*16+250*(12*32+2*16*64+256)+2*addedB;
        allocations+=addedB;strong+=frozenPayload+frozenMetadata+12*16+12+51;work+=addedW;
        // ONE read-only original-parent getter only: <=125 outer probes plus
        // <=250 proof reads. Full top-level and mutation pre/post checks stay.
        // Scope16B + owning Numeric reference8B; no parent/frame/context copies.
        // Compact identity1024 + scope64 is a DRAFT helper envelope, separately
        // reviewable; old clean-map check frequency is unchanged and already in
        // shadowChecks above. No CPU/whole-source sufficiency is claimed.
        allocations+=24;strong+=24;
        // Retain inherited opaque full-work reservation: attribution of the
        // removed375 calls inside historical412595010 is not independently
        // closed. Do not subtract their nominal375*51712 from a whole upper.
        work+=375L*(1024+64)+80; // metadata initialization/one-shot close envelope.
        // Exact arithmetic crosscheck pins SOURCE proposal, not readiness.
        if(allocations!=2368230L||strong!=1093684L||work!=861860694L)
            throw new InvalidDataException("SOURCE cardinality derivation differs.");
        return new(allocations,strong,work);
    }
}
