namespace InkRecognition;
// Extracted real Enroll/End bodies, synthetic owner and ledger only.
internal sealed class SyntheticScope:ILegacyEnglishPeriodicCaseWrapperProbeBudget
{
    private readonly LegacyPreparedDictionaryQuery?[] requests=new LegacyPreparedDictionaryQuery?[80];
    private readonly LegacyEnglishPeriodicRequestMembershipIndex membershipIndex=new();
    private int count,state=1;
    internal long Copies,Visits;
    private void Require() { if(state!=1)throw new InvalidOperationException("synthetic inactive"); }
    private void Copy(long n){Require();Copies+=n;}
    private void Visit(long n){Require();Visits+=n;}
    public void BeforeProbe()=>Visits+=32;
    internal int Enter(LegacyPreparedDictionaryQuery q){state=1;Enroll(q);return count-1;}
    internal int Count=>count;
        private void Enroll(LegacyPreparedDictionaryQuery request)
        {
            Require();Visit(64);int found=membershipIndex.Find(request,requests,count,out int free,this);if(found>=0)return;
            if(count==requests.Length||request.PeriodicQuery is not null)throw new InvalidOperationException("Actual fixed query propagation extent exceeded or foreign active request.");
            Copy(28);requests[count]=request;membershipIndex.Publish(free,count);count++;request.PeriodicQuery=this;
        }
        internal void End()
        {
            // Cleanup must execute even after a quota exception. Its bounded writes were reserved by Issue/preparation.
            for(int i=0;i<count;i++){var q=requests[i]!;if(ReferenceEquals(q.PeriodicQuery,this))q.PeriodicQuery=null;requests[i]=null;}if(count!=0)membershipIndex.Clear();count=0;state=0;
        }
}
