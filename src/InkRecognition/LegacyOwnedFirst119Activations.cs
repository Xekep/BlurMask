namespace InkRecognition;

/// <summary>Same-model portable reconstruction of the pinned native global
///48/128 activation constants. Does not construct a feature or source driver.</summary>
public sealed class LegacyOwnedFirst119Activations
{
    public LegacyFirstAcoustic119WireModel Model {get;}
    private readonly int[] hidden,output;
    private readonly LegacyInkActivations activations;
    private LegacyOwnedFirst119Activations(LegacyFirstAcoustic119WireModel model,int[] hidden,int[] output)
    {Model=model;this.hidden=hidden;this.output=output;activations=new(hidden,output);}
    public static LegacyOwnedFirst119Activations Prepare(LegacyFirstAcoustic119WireModel model,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(model);token.ThrowIfCancellationRequested();
        var hidden=new int[48];var output=new int[128];
        //Finite mathematical reconstructions checked against ALL native table
        //entries; not a claim that the native runtime generates its constants.
        for(int i=0;i<hidden.Length;i++){token.ThrowIfCancellationRequested();hidden[i]=(int)Math.Floor(1024*Math.Tanh(3.0*i/47));}
        for(int i=0;i<127;i++)output[i]=(int)Math.Floor(1024/(1+Math.Exp(-8.0*i/127)));
        output[^1]=1024;token.ThrowIfCancellationRequested();return new(model,hidden,output);
    }
    public int[] CopyHiddenTable()=>hidden.ToArray();
    public int[] CopyOutputTable()=>output.ToArray();
    public LegacyPreparedNetwork107Evaluator CreatePreparedEvaluator(int slot,CancellationToken token=default)
        =>LegacyPreparedNetwork107Evaluator.Prepare(Model,slot,activations,token);
}
