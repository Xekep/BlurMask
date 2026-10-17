using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyPreparedNetwork107.Probe")]

namespace InkRecognition;

/// <summary>Prepared full-vector evaluator for one actual119-owned107 network.
/// Activation tables are explicitly prepared; no feature/context/source-driver
/// factory or default English classifier is implied by this endpoint.</summary>
public sealed class LegacyPreparedNetwork107Evaluator
{
    private readonly LegacyInkActivations activations;
    private readonly sbyte[][] weights;
    private readonly byte[][] scales;
    public LegacyFirstAcoustic119WireModel Model {get;}
    public LegacyInkNetwork107Wire Network {get;}
    public int Slot {get;}
    public int Quantizer {get;}
    public int SourceSpecialClassIndex {get;}
    private LegacyPreparedNetwork107Evaluator(LegacyFirstAcoustic119WireModel model,int slot,LegacyInkNetwork107Wire network,
        LegacyInkActivations activations,sbyte[][] weights,byte[][] scales,int quantizer,int special)
    {Model=model;Slot=slot;Network=network;this.activations=activations;this.weights=weights;this.scales=scales;Quantizer=quantizer;SourceSpecialClassIndex=special;}

    public static LegacyPreparedNetwork107Evaluator Prepare(LegacyFirstAcoustic119WireModel actualModel,int slot,
        LegacyInkActivations preparedActivations,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualModel);ArgumentNullException.ThrowIfNull(preparedActivations);token.ThrowIfCancellationRequested();
        if((uint)slot>=actualModel.Slots.Count)throw new ArgumentOutOfRangeException(nameof(slot));
        var entry=actualModel.Slots[slot];
        if(entry.Networks.Count!=1||entry.First||!entry.Second||entry.Mode!=2)
            throw new NotSupportedException("An actual second-only mode2 network slot is required.");
        var network=entry.Networks[0];
        if(!ReferenceEquals(network.Source,actualModel.Source))throw new ArgumentException("Network source ownership differs.");
        var weights=new sbyte[2][];var scales=new byte[2][];
        for(int layer=0;layer<2;layer++)
        {
            token.ThrowIfCancellationRequested();weights[layer]=network.CopyMatrixWeights(layer);scales[layer]=network.CopyMatrixScales(layer);
            var m=network.Matrices[layer];
            //The prepared API supplies meaningful columns, not uninitialized
            //native allocation padding. Actual retained107 padding weights are0.
            for(int row=0;row<m.Rows;row++)for(int col=m.Columns;col<m.Stride;col++)
                if(weights[layer][row*m.Stride+col]!=0)throw new NotSupportedException("Nonzero matrix padding requires explicit native padded-input ownership.");
        }
        int quantizer=BinaryPrimitives.ReadUInt16LittleEndian(network.CopyBytes().AsSpan(31));
        if(quantizer!=10)throw new NotSupportedException("Only the proved exponent10 quantizer is supported.");
        byte[] header=actualModel.Source.Alphabet.CopySerializedHeader();int special=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
        if(special==ushort.MaxValue)special=-1;
        return new(actualModel,slot,network,preparedActivations,weights,scales,quantizer,special);
    }

    public static LegacyPreparedNetwork107Evaluator Prepare(LegacyFirstAcoustic119WireModel actualModel,
        LegacyInkNetwork107Wire actualNetwork,LegacyInkActivations preparedActivations,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualModel);ArgumentNullException.ThrowIfNull(actualNetwork);token.ThrowIfCancellationRequested();
        int slot=-1;foreach(var entry in actualModel.Slots)if(entry.Networks.Any(n=>ReferenceEquals(n,actualNetwork)))slot=entry.Index;
        if(slot<0||!ReferenceEquals(actualNetwork.Source,actualModel.Source))throw new ArgumentException("Network is not owned by this selected119 model.");
        return Prepare(actualModel,slot,preparedActivations,token);
    }

    /// <summary>Only signed wrapping dots and hidden activation; no class conversion.</summary>
    public LegacyPreparedNetwork107Result EvaluatePreparedRaw(ReadOnlySpan<short> fullVector,CancellationToken token=default)
        =>Evaluate(fullVector,0,token);
    /// <summary>Explicit all-class EXP branch (native212.V50), irrespective of source special class.</summary>
    public LegacyPreparedNetwork107Result EvaluatePreparedExp(ReadOnlySpan<short> fullVector,CancellationToken token=default)
        =>Evaluate(fullVector,1,token);
    /// <summary>Unmasked1B86F0 with prepared param4=0: same-source AL+E selects
    /// mixed sigmoid/EXP180 on actual English data, or all-EXP whenFFFF.</summary>
    public LegacyPreparedNetwork107Result EvaluatePreparedSourceUnmasked(ReadOnlySpan<short> fullVector,CancellationToken token=default)
        =>Evaluate(fullVector,2,token);
    private LegacyPreparedNetwork107Result Evaluate(ReadOnlySpan<short> input,int mode,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(input.Length!=Network.InputWidth)throw new ArgumentException("Full prepared107 input vector required.",nameof(input));
        int special=mode==2?SourceSpecialClassIndex:-1;
        if(special>=Network.OutputWidth)throw new NotSupportedException("Source special class is outside this network's actual output domain.");
        short[] prepared=input.ToArray();var hiddenDots=new int[Network.HiddenWidth];var hidden=new short[Network.HiddenWidth+1];hidden[0]=(short)(1<<Quantizer);
        for(int row=0;row<hiddenDots.Length;row++)
        {
            if((row&15)==0)token.ThrowIfCancellationRequested();
            int dot=Dot(prepared,0,row);hiddenDots[row]=dot;
            //Native249DE0 negates a signed int before an arithmetic shift.
            //INT_MIN would escape the lookup's supported domain; do not repair it.
            if(dot==int.MinValue)throw new NotSupportedException("INT_MIN hidden activation is outside the bounded LUT domain.");
            hidden[row+1]=(short)activations.Hidden(dot);
        }
        var raw=new int[Network.OutputWidth];for(int row=0;row<raw.Length;row++)
        {if((row&15)==0)token.ThrowIfCancellationRequested();raw[row]=Dot(hidden,1,row);}
        if(mode==0)return new(this,hiddenDots,hidden,raw,null,0,-1);
        int[] ordinary=special<0?raw:raw.Where((_,i)=>i!=special).ToArray();
        var exp=LegacyClassScores.Convert(ordinary,token);int[] probabilities;
        if(special<0)probabilities=exp.Probability32768.ToArray();
        else
        {
            probabilities=new int[raw.Length];int p=0;
            for(int i=0;i<raw.Length;i++)if(i!=special)probabilities[i]=exp.Probability32768.Span[p++];
            probabilities[special]=ComputeSpecialProbability(raw[special]);
        }
        token.ThrowIfCancellationRequested();return new(this,hiddenDots,hidden,raw,probabilities,exp.StabilizationOffset,special);
    }
    private int Dot(ReadOnlySpan<short> input,int layer,int row)
    {
        int offset=row*Network.Matrices[layer].Stride,sum=0;
        unchecked{for(int i=0;i<input.Length;i++)sum+=input[i]*weights[layer][offset+i];return sum*scales[layer][row+1];}
    }
    //E9360 uses the un-stabilized special dot and float arithmetic before a
    //native double exp. Math.Exp is a portable transcendental adaptation:
    //control parity does not establish universal bit identity to that CRT.
    internal static int ComputeSpecialProbability(int raw)
    {float argument=ComputeSpecialArgument(raw);return argument>75f?0:(int)(32768d/(Math.Exp((double)argument)+1d)+.5d);}
    internal static float ComputeSpecialArgument(int raw)=>((float)raw*-2f)/1048576f;
}

/// <summary>Isolated numerical output. Class probabilities are absent on the raw
/// path. No canonical labels, candidate masks, confidence or winner are implied.</summary>
public sealed class LegacyPreparedNetwork107Result
{
    private readonly int[] hiddenDots,raw;
    private readonly short[] hidden;
    private readonly int[]? probabilities;
    public LegacyPreparedNetwork107Evaluator Owner {get;}
    public int StabilizationOffset {get;}
    public int SpecialClassIndex {get;}
    public bool HasClassProbabilities=>probabilities is not null;
    internal LegacyPreparedNetwork107Result(LegacyPreparedNetwork107Evaluator owner,int[] hiddenDots,short[] hidden,int[] raw,
        int[]? probabilities,int offset,int special)
    {Owner=owner;this.hiddenDots=hiddenDots;this.hidden=hidden;this.raw=raw;this.probabilities=probabilities;StabilizationOffset=offset;SpecialClassIndex=special;}
    public int[] CopyHiddenDots()=>hiddenDots.ToArray();
    public short[] CopyHiddenValues()=>hidden.ToArray();
    public int[] CopyOutputDots()=>raw.ToArray();
    public int[] CopyProbabilities()=>probabilities?.ToArray()??throw new InvalidOperationException("Raw evaluation has no class conversion.");
}
