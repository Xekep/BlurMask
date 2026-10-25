namespace InkRecognition;

//Internal synchronous accounting only. No source/model/request authority and
//no retention by the pure helper. Existing public paths pass no implementation.
internal interface ILegacyDirectionalStructuralDebit
{
    void BeforeGrid(int points,int cells,CancellationToken token);
    void BeforeInterval(int count,CancellationToken token);
}
