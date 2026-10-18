using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyEnglish119Primary.Probe")]
namespace InkRecognition;

/// <summary>Internal shared numeric1EE3B0 primary kernel. Public owning
/// producers supply their own verified source, requests and corrections.</summary>
internal static class LegacyFirstPrimaryRecurrenceKernel
{
    internal static LegacyFirstOrdinaryAcousticRecurrence Run(
        LegacyFirstOrdinaryGlyphRequest[][] requests,int width,ushort blank,ushort replacement,ushort closed,
        ushort shadda,ushort newline,ushort space,ushort nbsp,bool excludeConsecutiveBlank,
        LegacyOrdinaryCandidateConnections connections,
        Func<LegacyFirstAncestorTransitionRequest,int,CancellationToken,LegacyFirstOrdinaryTransitionResult> correct118,
        Func<LegacyFirstAncestorTransitionRequest,int,CancellationToken,LegacyFirstOrdinaryTransitionResult> correct140,
        Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult>? observer,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();var rows=new LegacyFirstAcousticRecordTable[requests.Length];
        for(int n=0;n<rows.Length;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var target=rows[n]=new(width);
            foreach(var g in requests[n])
            {
                cancellationToken.ThrowIfCancellationRequested();if(g.GroupCost>=0x4000000)continue;int parent=n-g.Span-1;
                if(parent==-1)
                {
                    if(g.Symbol==closed)continue;
                    if(connections.GetSuccessorOrdinals(-1,ushort.MaxValue,g.Span,cancellationToken).Span.Contains((int)g.ArcOrdinal))
                        target.Insert(new(g.GroupCost,g.GroupCost,255,0,g.ArcOrdinal,g.Span,g.Symbol,true,0,g.CompositeCategory,0),cancellationToken);
                    continue;
                }
                var prior=rows[parent];int best=0x4000000,bestAlternative=-1;byte bestType=0;
                for(int a=0;a<prior.Count;a++)
                {
                    cancellationToken.ThrowIfCancellationRequested();var original=prior.Get(a);if(original.TotalCost>=0x4000000)break;
                    if(excludeConsecutiveBlank&&g.Symbol==blank&&original.Symbol==blank)continue;
                    //F0/F8 use original parent+arc even after the geometric walk.
                    if(!connections.GetSuccessorOrdinals(parent,original.ArcOrdinal,g.Span,cancellationToken).Span.Contains((int)g.ArcOrdinal))continue;
                    int an=parent,aa=a,skipped=0;var ancestor=original;
                    while(ancestor.Symbol==blank||ancestor.Symbol==shadda||ancestor.Symbol==newline||ancestor.Symbol==space||ancestor.Symbol==nbsp)
                    {
                        cancellationToken.ThrowIfCancellationRequested();int previous=an-ancestor.Span-1;if(previous==-1)break;
                        aa=ancestor.PreviousAlternative;an=previous;ancestor=rows[an].Get(aa);skipped++;
                    }
                    var owners=new LegacyFirstGeometricAncestor(parent,a,original,an,aa,ancestor,skipped);
                    int first=0x4000000,second=0x4000000;
                    if(g.Symbol!=newline&&g.Symbol!=space&&g.Symbol!=nbsp&&original.Symbol!=newline&&original.Symbol!=space&&original.Symbol!=nbsp&&
                        (g.Symbol==replacement)==(original.Symbol==replacement))
                        first=Cost(g.Symbol==blank?(byte)5:g.Symbol==closed?(byte)6:(byte)0);
                    if(g.Symbol!=blank&&g.Symbol!=shadda)second=Cost(2);
                    int delta=Math.Min(first,second);if(delta>=0x4000000)continue;
                    int subtotal=checked(original.TotalCost+delta);if(subtotal>=0x4000000)continue;
                    int value=checked(subtotal+g.GroupCost);if(value<best){best=value;bestAlternative=a;bestType=second<first?(byte)1:(byte)0;}
                    int Cost(byte kind)
                    {
                        var request=new LegacyFirstAncestorTransitionRequest(n,g.ArcOrdinal,g.Symbol,g.CompositeCategory,g.Span,kind,owners);
                        var a118=correct118(request,0,cancellationToken);Observe(118,request,0,a118);
                        if(!a118.Accepted)return 0x4000000;
                        var a140=correct140(request,a118.Cost,cancellationToken);Observe(140,request,a118.Cost,a140);return a140.Accepted?a140.Cost:0x4000000;
                    }
                }
                if(bestAlternative>=0)
                {
                    var original=prior.Get(bestAlternative);
                    target.Insert(new(best,checked(original.AcousticCost+g.GroupCost),(byte)bestAlternative,0,g.ArcOrdinal,g.Span,g.Symbol,bestType==1,0,g.CompositeCategory,0),cancellationToken);
                }
            }
            for(int a=0;a<target.Count;a++)
            {
                var r=target.Get(a);target.SetLikelihoodByte(a,requests[n].First(g=>g.ArcOrdinal==r.ArcOrdinal&&g.Symbol==r.Symbol).LikelihoodByte);
            }
        }
        return LegacyFirstOrdinaryAcousticRecurrence.FromOwnedRows(rows,width,cancellationToken);

        void Observe(int stage,LegacyFirstAncestorTransitionRequest request,int incoming,LegacyFirstOrdinaryTransitionResult result)
        {cancellationToken.ThrowIfCancellationRequested();observer?.Invoke(stage,request,incoming,result);cancellationToken.ThrowIfCancellationRequested();}
    }
}
