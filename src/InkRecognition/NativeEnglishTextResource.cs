using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
[assembly:InternalsVisibleTo("NativeEnglishResource.Probe")]
namespace InkRecognition;

/// <summary>Loads the pinned full English resource into the existing portable
/// BLSTM/CTC recognizer, without Python or an SDK DLL. This does not implement
/// the separate legacy119 MAIN dictionary controller or bilingual selection.</summary>
public static class NativeEnglishTextResource
{
    public const string SourceSha256="355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927";
    public static StrokeTextRecognizer Load(ReadOnlySpan<byte> resource,CancellationToken token=default)
    {using var stream=new MemoryStream(CreatePortableSnapshot(resource,token),false);token.ThrowIfCancellationRequested();var model=StrokeTextRecognizer.Load(stream);token.ThrowIfCancellationRequested();return model;}
    internal static byte[] CreatePortableSnapshot(ReadOnlySpan<byte> resource,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();Require(resource.Length==3783049,"Complete full English resource required.");
        byte[] data=resource.ToArray();Require(Convert.ToHexStringLower(SHA256.HashData(data))==SourceSha256,"Unsupported English resource fingerprint.");token.ThrowIfCancellationRequested();
        // Resolve the actual delegated source child, rather than interpreting
        // header-like byte sequences inside unrelated model/tensor payloads.
        var source=LegacySource247WireLayout.Read(data,token:token);
        Require(source.Offset==0&&source.End==data.Length&&source.SourceKind==0x135&&
            source.Alphabet.LabelCount==217&&source.Fingerprint.Equals(SourceSha256,StringComparison.OrdinalIgnoreCase),
            "Complete actual English source owner required.");
        var alphabet=source.Alphabet;
        var children=source.Children.Where(c=>c.Destination=="247.148").ToArray();
        Require(children.Length==1,"Exactly one delegated English adapter required.");
        var child=children[0];
        Require(child.Kind==0x21e&&child.Version==0x103,"Actual text adapter required.");
        var node=Header(data,child.Offset);
        Require(node.End==child.End&&node.Flags==0,"Owned complete English adapter required.");
        var r=new Cursor(data,node.Offset+16,node.End);
        Require(r.Int()==1&&r.Int()==0&&r.Int()==4,"One actual13-channel English feature scheme required.");
        var wrapper=Header(data,r.Position);
        Require(wrapper.Kind==0x53&&wrapper.Version==0x100&&wrapper.Flags==0&&wrapper.End<=node.End,
            "Owned English network wrapper required.");
        // These nine bytes are a pinned opaque wrapper prefix, not a generic
        // wrapper execution policy or retained-graph/V120 proof.
        const int wrapperPrefixLength=9;
        Require(wrapper.End-wrapper.Offset>=16+wrapperPrefixLength+16&&
            data.AsSpan(wrapper.Offset+16,wrapperPrefixLength).SequenceEqual(new byte[]{1,1,0,0,0,1,0,0,0}),
            "Actual opaque English wrapper prefix required.");
        var net=Header(data,wrapper.Offset+16+wrapperPrefixLength);
        Require(net.Kind==0x5f&&net.Flags==0&&net.Version==0x106&&net.End==wrapper.End,
            "One complete nested actual106 network required.");
        r.Position=wrapper.End;
        Require(r.Byte()==1,"Normalized feature channels required.");var means=r.Floats(13);var scales=r.Floats(13);
        int blank=r.Int(),scoreKind=r.Int(),representation=r.Int();float scoreScale=r.Float();int unused=r.Int();float spacing=r.Float();
        int minimum=r.Int(1000),transform=r.Int(),property14=r.Int();int property20=r.Byte(),link=r.Byte();float property1c=r.Float();int labelCount=r.Short();
        Require(labelCount==181&&blank==181&&scoreKind==0&&representation==0&&unused==0&&transform==0&&property14==0&&property20==0&&link==0&&property1c==float.MaxValue&&spacing>0&&minimum is>=1 and<=8,"Unsupported English feature/scoring profile.");
        var labels=new ushort[labelCount];for(int i=0;i<labels.Length;i++)labels[i]=r.Short();Require(r.Position==r.End,"Unconsumed text adapter.");
        var p=new Profile(wrapper,means,scales,spacing,minimum,labels);
        // Reuse the authenticated data-only selected106 owner; conversion remains here.
        var selected=LegacySelectedEnglish106WireLayout.Read(source,child,token);
        var layers=selected.Layers;var tensors=selected.Tensors;
        int payloadLength=selected.PayloadBytes;
        byte[] payload=selected.CopyPayload(token);
        Require(payloadLength<=8*1024*1024-16384,"Bounded portable body reservation required.");
        using var body=new MemoryStream(checked(payloadLength+16384));using(var writer=new BinaryWriter(body,Encoding.UTF8,true))
        {
            void I(params int[] v){foreach(int value in v)writer.Write(value);}void Text(string value){byte[] utf8=Encoding.UTF8.GetBytes(value);I(utf8.Length);writer.Write(utf8);}
            I(13,182,181,3,181);Text("en-US");foreach(float value in p.Means)writer.Write(value);foreach(float value in p.Scales)writer.Write(value);writer.Write(p.Spacing);I(p.Minimum,0);
            for(int i=1;i<4;i++)I(layers[i].Width,1,layers[i-1].Code);I(layers[layers.Count-1].Code);
            foreach(ushort label in p.Labels){token.ThrowIfCancellationRequested();if(!alphabet.TryGetText(label,out string text)){Require(label==178,"Unknown output label.");text="";}Text(text);}
            I(tensors.Count);foreach(var t in tensors){Text(t.Name);I(t.Kind,t.Offset,t.Bytes,t.Rows,t.Columns,t.Input,t.Code);}I(payloadLength);writer.Write(payload);
        }
        Require(body.Length<=8*1024*1024,"Bounded portable payload required.");
        var bytesBody=body.GetBuffer().AsSpan(0,checked((int)body.Length));var result=new byte[checked(bytesBody.Length+48)];
        "IKRPACK1"u8.CopyTo(result);BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(8),1);BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(12),bytesBody.Length);
        SHA256.HashData(bytesBody,result.AsSpan(16,32));bytesBody.CopyTo(result.AsSpan(48));token.ThrowIfCancellationRequested();return result;
    }
    private static Node Header(byte[] data,int offset)
    {
        Require(offset>=0&&offset<=data.Length-16,"Bounded resource header required.");var s=data.AsSpan(offset);uint length=BinaryPrimitives.ReadUInt32LittleEndian(s[4..]);Require(BinaryPrimitives.ReadUInt32LittleEndian(s)==0x0101fe90&&length>=8&&length<=(uint)(data.Length-offset-8),"Invalid resource envelope.");
        return new(offset,checked(offset+8+(int)length),BinaryPrimitives.ReadUInt32LittleEndian(s[8..]),BinaryPrimitives.ReadUInt16LittleEndian(s[12..]),BinaryPrimitives.ReadUInt16LittleEndian(s[14..]));
    }
    private static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
    private sealed record Node(int Offset,int End,uint Kind,ushort Flags,ushort Version);
    private sealed record Profile(Node Wrapper,float[] Means,float[] Scales,float Spacing,int Minimum,ushort[] Labels);
    private sealed class Cursor(byte[] data,int start,int end)
    {
        public int Position{get;set;}=start;public int End=>end;
        public ReadOnlySpan<byte> Take(int count){Require(count>=0&&Position>=0&&Position<=end-count,"Truncated bounded native field.");var result=data.AsSpan(Position,count);Position+=count;return result;}
        public int Int(int max=int.MaxValue){uint value=BinaryPrimitives.ReadUInt32LittleEndian(Take(4));Require(value<=(uint)max,"Native count/value exceeds supported range.");return(int)value;}
        public ushort Short()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));public byte Byte()=>Take(1)[0];
        public float Float(){float value=BinaryPrimitives.ReadSingleLittleEndian(Take(4));Require(float.IsFinite(value),"Finite native float required.");return value;}
        public float[] Floats(int count){var result=new float[count];for(int i=0;i<count;i++)result[i]=Float();return result;}
        public void Zero()=>Require(Int()==0,"Unsupported native reserved field.");
    }
}
