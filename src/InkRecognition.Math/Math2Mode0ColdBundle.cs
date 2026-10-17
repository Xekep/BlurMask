using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleTo("LegacyMathColdBundle.Probe")]

namespace InkRecognition.Math;

/// <summary>Owning cold-only entry for the fingerprinted three-graph bundle.
/// Metadata policy is derived from the exact resource, never a caller boolean.
/// This is not the EEE search loop, final acceptance or an app recognizer.</summary>
internal sealed class Math2Mode0ColdBundle : IDisposable
{
    private readonly Math2Mode0TensorModels models;
    private readonly Math2Mode0PreparedPolicy policy;
    internal bool ForceNonobservablePositions => policy.ForceNonobservablePositions;
    private Math2Mode0ColdBundle(Math2Mode0TensorModels models, LegacyMath2ObservableTokens gate)
    {
        this.models = models;
        //Pinned cold045460 -> ordered raw3 metadata -> EF1packet20 -> EF130.
        //The third exact model sets true; no fourth model or warm child exists.
        policy = new(gate, preparedForceNonobservablePositions: true);
    }

    internal static Math2Mode0ColdBundle Read(OrtRuntime runtime, Stream exportedPack,
        ReadOnlySpan<byte> rawResource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(runtime); ArgumentNullException.ThrowIfNull(exportedPack);
        token.ThrowIfCancellationRequested();
        var gate = LegacyMath2ObservableTokens.BindBundledResource(rawResource, token);
        //Read owns its authenticated body/graph slices. Caller arrays cannot
        //change the validated label list or graph bytes after this point.
        var model = MathModel.Read(exportedPack);
        token.ThrowIfCancellationRequested();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[4];
        foreach (string label in model.Labels)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(label);
            BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
            hash.AppendData(length); hash.AppendData(bytes);
        }
        if (model.Labels.Length != 297 || Convert.ToHexStringLower(hash.GetHashAndReset()) !=
            "4366c61258cd74b6520f654898f94b846cf772b5fdcd975b6a95a1cdaf154dce")
            throw new NotSupportedException("The same exact Math2 decoder vocabulary is required.");
        //The model owner independently checks all three full graph hashes.
        return new(new Math2Mode0TensorModels(runtime, model), gate);
    }

    internal Math2Mode0TensorState PrepareState(OrtRun run, OrtTensor source,
        OrtTensor downsampling, OrtTensor strokeEnds, CancellationToken token = default)
    {
        using var operation = models.RetainOperation();
        using var encoder = models.RunEncoder(run, source, downsampling, strokeEnds, token);
        return Math2Mode0TensorState.Prepare(models, encoder, policy, token);
    }

    public void Dispose() => models.Dispose();
}
