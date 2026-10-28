using System.Security.Cryptography;
using System.Runtime.InteropServices;
using InkRecognition;
using InkRecognition.RawContent;
namespace NovaPen;

// Bundled fallback only; installed user packs continue through the old owner.
internal static class AiExperimentalRawContentModels
{
    internal const int PackBytes=5341832;
    internal const string PackSha="b3bfe175da6627d8f8281675b5d61aba979478444ccffa0b6eec976fce102d24";
    internal const string GraphSha="788aaed9ed4b4eb2f62f0806695f1353d92826889c2e6ce8f503eba56c9c8741";
    private static readonly object Gate=new();
    private static RawContentAnalyzer? owner;
    private sealed record Identity(long Size,DateTime Modified,long RuntimeSize,DateTime RuntimeModified);
    private static Identity? signature,failed;
    private static int releaseRequested;
    private static string Pack=>Path.Combine(AppContext.BaseDirectory,"models","raw-content2.ikrraw");
    private static string Runtime=>Path.Combine(AppContext.BaseDirectory,"runtimes",RuntimeInformation.RuntimeIdentifier,"native",
        OperatingSystem.IsWindows()?"onnxruntime.dll":OperatingSystem.IsMacOS()?"libonnxruntime.dylib":"libonnxruntime.so");
    static AiExperimentalRawContentModels()=>AppDomain.CurrentDomain.ProcessExit+=(_,_)=>Release();
    internal static RawContentResult? Analyze(IReadOnlyList<InkStroke> strokes,CancellationToken token)
    {
        lock(Gate)
        {
            token.ThrowIfCancellationRequested();bool loading=false;
            try
            {
                if(Volatile.Read(ref releaseRequested)!=0)DisposeLocked();
                var file=new FileInfo(Pack);var runtime=new FileInfo(Runtime);
                var current=file.Exists&&runtime.Exists&&runtime.Length is >0 and <=100000000?new Identity(file.Length,file.LastWriteTimeUtc,runtime.Length,runtime.LastWriteTimeUtc):null;
                if(current is null||current.Size!=PackBytes){DisposeLocked();return null;}
                if(signature!=current){DisposeLocked();signature=current;}
                if(failed==current)return null;
                if(owner is null)
                {
                    loading=true;var bytes=new byte[PackBytes];
                    try
                    {
                        using var source=new FileStream(Pack,FileMode.Open,FileAccess.Read,FileShare.Read);
                        if(source.Length!=bytes.Length)throw new InvalidDataException("Bundled raw-content extent differs.");
                        for(int at=0;at<bytes.Length;)
                        {token.ThrowIfCancellationRequested();int n=source.Read(bytes,at,Math.Min(65536,bytes.Length-at));if(n==0)throw new EndOfStreamException();at+=n;}
                        if(source.ReadByte()!=-1)throw new InvalidDataException("Bundled raw-content grew.");
                        ValidatePack(bytes,token);using var stream=new MemoryStream(bytes,false);var loaded=RawContentAnalyzer.Load(stream,Runtime);
                        try{token.ThrowIfCancellationRequested();owner=loaded;}catch{loaded.Dispose();throw;}
                    }
                    finally{CryptographicOperations.ZeroMemory(bytes);}
                }
                loading=false;var result=owner.Analyze(strokes,token);token.ThrowIfCancellationRequested();return result;
            }
            catch(OperationCanceledException){throw;}
            catch(Exception e)when(Recoverable(e))
            {if(loading){owner?.Dispose();owner=null;failed=signature;}System.Diagnostics.Trace.WriteLine($"Experimental auto classifier: {e.Message}");return null;}
            finally{if(Volatile.Read(ref releaseRequested)!=0)DisposeLocked();}
        }
    }
    // Hash the owned complete snapshot before parsing/loading. Both fingerprints
    // are exact known assets, not acceptance of arbitrary self-checksummed packs.
    internal static void ValidatePack(ReadOnlySpan<byte> bytes,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(bytes.Length!=PackBytes||!SHA256.HashData(bytes).AsSpan().SequenceEqual(Convert.FromHexString(PackSha)))throw new InvalidDataException("Unverified bundled raw-content pack.");
        token.ThrowIfCancellationRequested();
        if(!SHA256.HashData(bytes[72..]).AsSpan().SequenceEqual(Convert.FromHexString(GraphSha)))throw new InvalidDataException("Unverified bundled raw-content graph.");
        token.ThrowIfCancellationRequested();
    }
    private static bool Recoverable(Exception e)=>e is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or DllNotFoundException or BadImageFormatException or EntryPointNotFoundException or NotSupportedException;
    private static void DisposeLocked(){owner?.Dispose();owner=null;signature=null;failed=null;Volatile.Write(ref releaseRequested,0);}
    internal static void Release()
    {
        Volatile.Write(ref releaseRequested,1);if(!Monitor.TryEnter(Gate))return;
        try{DisposeLocked();}finally{Monitor.Exit(Gate);}
    }
}
