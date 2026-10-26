using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

//Tools-only allocator INPUT witness. No library hook, source/model/query
//authority or recomputation. PDB/source cohort authentication is ROOT's task.
internal sealed class GlobalPublicationInputCapture
{
    internal sealed record Allocation(string Kind,int Node,ulong Identity,byte[] Bytes);
    private readonly string[] resources;
    private Frame? current;
    internal GlobalPublicationInputCapture(byte[] ak,byte[] lk,string casePath,string publicationPath)
    {
        static byte[] Table(string path)
        {if(new FileInfo(path).Length>131072)throw new InvalidDataException("Bounded immutable Unicode source required.");return File.ReadAllBytes(path);}
        static string Hash(byte[] value)=>Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
        resources=[Hash(ak),Hash(lk),Hash(Table(casePath)),Hash(Table(publicationPath))];
    }
    internal Frame Begin()
    {
        if(current is not null)throw new InvalidOperationException("Nested allocator-input observation is refused.");
        var value=new Frame(this);current=value;return value;
    }
    internal void Observe(LegacyMainDirectStorageRequest request,ulong identity,ReadOnlySpan<byte> bytes)
    {
        var frame=current;if(frame is null)return;
        if(request.Kind is not(LegacyMainDirectStorageKind.PublicationLanguageHeap or LegacyMainDirectStorageKind.PublicationCandidateHeap))return;
        int expected=request.Kind==LegacyMainDirectStorageKind.PublicationLanguageHeap?440:1176;
        if(request.EndingNode is <0 or >=240||request.RequiredBytes!=expected||bytes.Length!=expected||identity==0)
            throw new InvalidDataException("Closed GLOBAL2 allocator-input shape required.");
        //1Mi logical capture ceiling and480 input records per bounded240 nodes.
        //Precharge record+array/source copy before creating a detached payload.
        long charge=checked(128L+24+bytes.Length);
        if(frame.Rows.Count>=480||charge>1048576-frame.Payload||2L*bytes.Length>1048576-frame.Copies)
            throw new NotSupportedException("Allocator INPUT witness bound exhausted.");
        frame.Payload+=charge;frame.Copies+=2L*bytes.Length;
        string kind=request.Kind==LegacyMainDirectStorageKind.PublicationLanguageHeap?"PublicationLanguageHeap":"PublicationCandidateHeap";
        frame.Rows.Add(new(kind,request.EndingNode,identity,bytes.ToArray()));
    }
    internal sealed class Frame(GlobalPublicationInputCapture owner):IDisposable
    {
        internal readonly List<Allocation> Rows=new(480);
        internal long Payload=8192,Copies;
        private bool ended;
        internal Allocation[] Complete()
        {
            if(ended||!ReferenceEquals(owner.current,this))throw new InvalidOperationException("Actual active allocator INPUT scope required.");
            ended=true;owner.current=null;return Rows.ToArray();
        }
        public void Dispose(){if(ended)return;ended=true;if(ReferenceEquals(owner.current,this))owner.current=null;}
    }
    internal void Write(Utf8JsonWriter writer,IEnumerable<Allocation> values,int alternatives)
    {
        if(alternatives!=2)throw new NotSupportedException("Only authenticated GLOBAL2402=2 oracle contract.");
        writer.WriteString("publicationOracleMode","global-language-207cb0-v1");writer.WriteNumber("publicationOracleInputVersion",1);
        writer.WriteNumber("globalPublicationAlternatives",alternatives);
        writer.WriteStartArray("publicationResourceSHA256");foreach(string value in resources)writer.WriteStringValue(value);writer.WriteEndArray();
        WriteInputs(writer,"publicationAllocatorInputs",values);
    }
    internal static void WriteInputs(Utf8JsonWriter writer,string name,IEnumerable<Allocation> values)
    {
        writer.WriteStartArray(name);
        foreach(var value in values)
        {writer.WriteStartObject();writer.WriteString("Kind",value.Kind);writer.WriteNumber("Node",value.Node);writer.WriteNumber("Identity",value.Identity);writer.WriteBase64String("Bytes",value.Bytes);writer.WriteEndObject();}
        writer.WriteEndArray();
    }
}
