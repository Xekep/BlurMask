using System.Runtime.InteropServices;
using InkRecognition;
using InkRecognition.Math;
namespace NovaPen;

// Bundled explicit-manual owner only. Installed/user pack and auto are separate.
internal static class AiExperimentalMathModels
{
    private const int RawBytes=18704269;
    private const long MaximumPackBytes=64L*1024*1024+48;
    private static readonly object Gate=new();
    private static ExperimentalColdMathRecognizer? owner;
    private sealed record AssetIdentity(long Pack,long Raw,long RuntimeBytes,DateTime PackTime,DateTime RawTime,DateTime RuntimeTime);
    private static AssetIdentity? signature,failureIdentity;
    private static bool failed;
    private static int releaseRequested;
    private static string Models=>Path.Combine(AppContext.BaseDirectory,"models");
    private static string Pack=>Path.Combine(Models,"math2-mode0.ikrmath");
    private static string Raw=>Path.Combine(Models,"math2-unwrapped.res");
    private static string Runtime=>Path.Combine(AppContext.BaseDirectory,"runtimes",RuntimeInformation.RuntimeIdentifier,"native",
        OperatingSystem.IsWindows()?"onnxruntime.dll":OperatingSystem.IsMacOS()?"libonnxruntime.dylib":"libonnxruntime.so");
    static AiExperimentalMathModels()=>AppDomain.CurrentDomain.ProcessExit+=(_,_)=>Release();
    // Presence is not a confidence/quality claim; load validates the exact source,
    // all3 graph hashes, vocabulary and profile before inference can occur.
    internal static bool Available
    {
        get { try { var current=ReadSignature();return current is not null&&current!=Volatile.Read(ref failureIdentity); } catch(Exception e) when(Recoverable(e)) { return false; } }
    }
    private static AssetIdentity? ReadSignature()
    {
        var p=new FileInfo(Pack);var r=new FileInfo(Raw);var runtime=new FileInfo(Runtime);
        return p.Exists&&r.Exists&&runtime.Exists&&runtime.Length is >0 and <=100000000&&p.Length>=112&&p.Length<=MaximumPackBytes&&r.Length==RawBytes
            ?new(p.Length,r.Length,runtime.Length,p.LastWriteTimeUtc,r.LastWriteTimeUtc,runtime.LastWriteTimeUtc):null;
    }
    internal static AiExperimentalMathLayout.Candidate? Recognize(IReadOnlyList<InkStroke> strokes,CancellationToken token)
    {
        lock(Gate)
        {
            token.ThrowIfCancellationRequested();bool loading=false;
            try
            {
                if(Volatile.Read(ref releaseRequested)!=0)DisposeLocked();
                var current=ReadSignature();
                if(current is null){DisposeLocked();return null;}
                if(signature!=current){DisposeLocked();signature=current;}
                if(failed)return null;
                if(owner is null)
                {
                    loading=true;
                    owner=LoadBundled(token);
                }
                loading=false;var actual=owner.Recognize(strokes,token:token);token.ThrowIfCancellationRequested();
                if(actual.Confidence is not null||actual.Admission!=ExperimentalMathAdmission.Uncalibrated||actual.Policy!="ColdDefault")
                    throw new InvalidDataException("Unexpected experimental math admission policy.");
                return new(actual.LaTex,actual.UnsupportedReason,actual.Syntax.Count);
            }
            catch(OperationCanceledException){throw;}
            catch(Exception e) when(Recoverable(e))
            {if(loading){DisposeLocked();signature=ReadFailureSignature();Volatile.Write(ref failureIdentity,signature);failed=true;}System.Diagnostics.Trace.WriteLine($"Experimental formula: {e.Message}");return null;}
            finally{if(Volatile.Read(ref releaseRequested)!=0)DisposeLocked();}
        }
    }
    private static ExperimentalColdMathRecognizer LoadBundled(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        byte[] raw=new byte[RawBytes];
        try
        {
            using var source=new FileStream(Raw,FileMode.Open,FileAccess.Read,FileShare.Read);
            if(source.Length!=raw.Length)throw new InvalidDataException("Bundled Math2 source extent differs.");
            for(int at=0;at<raw.Length;)
            {token.ThrowIfCancellationRequested();int n=source.Read(raw,at,Math.Min(65536,raw.Length-at));if(n==0)throw new EndOfStreamException();at+=n;}
            if(source.ReadByte()!=-1)throw new InvalidDataException("Bundled Math2 source grew.");
            using var pack=new FileStream(Pack,FileMode.Open,FileAccess.Read,FileShare.Read);
            if(pack.Length<112||pack.Length>MaximumPackBytes)throw new InvalidDataException("Bundled Math2 pack extent differs.");
            var loaded=ExperimentalColdMathRecognizer.Load(pack,raw,Runtime,token);
            try{token.ThrowIfCancellationRequested();return loaded;}catch{loaded.Dispose();throw;}
        }
        finally{System.Security.Cryptography.CryptographicOperations.ZeroMemory(raw);}
    }
    // Explicit headless verification: loads actual source/3graphs and official
    // runtime, then disposes; no NN recognition, user files or GUI lifetime.
    internal static void VerifyBundled(CancellationToken token=default)
    {if(!Available)throw new FileNotFoundException("Bundled Math2 assets/runtime are unavailable.");using var verified=LoadBundled(token);Console.WriteLine("Bundled Math2: actual raw source, vocabulary and3graph hashes validated; uncalibrated, no inference.");}
    private static AssetIdentity? ReadFailureSignature()
    {try{return ReadSignature();}catch(Exception e)when(Recoverable(e)){return null;}}
    private static bool Recoverable(Exception e)=>e is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException
        or DllNotFoundException or BadImageFormatException or EntryPointNotFoundException or NotSupportedException or FormatException;
    private static void DisposeLocked()
    {owner?.Dispose();owner=null;signature=null;Volatile.Write(ref failureIdentity,null);failed=false;Volatile.Write(ref releaseRequested,0);}
    // UI toggles never wait for an inference lock. Its finally releases an active
    // owner; otherwise this call disposes immediately. Completed results detached.
    internal static void Release()
    {
        Volatile.Write(ref releaseRequested,1);
        if(!Monitor.TryEnter(Gate))return;
        try{DisposeLocked();}finally{Monitor.Exit(Gate);}
    }
}
