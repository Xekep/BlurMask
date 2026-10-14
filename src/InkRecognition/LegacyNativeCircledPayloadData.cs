using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace InkRecognition;

/// <summary>Complete pinned0491A0 non-ASCII literal payload decisions; ASCII arithmetic is recovered separately below.</summary>
public sealed class LegacyNativeCircledPayloadData
{
    public const string ExpectedTableSha256 = "a162c735d60ca0c0d0581cc40a4e3882e1de805de205fb18cfa8d50f983e45cc";
    private readonly Dictionary<string,int> _map;
    private LegacyNativeCircledPayloadData(Dictionary<string,int> map) => _map = map;
    public static LegacyNativeCircledPayloadData Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream); using var bytes = new MemoryStream(); byte[] buffer=new byte[4096]; int read;
        while((read=stream.Read(buffer))!=0){if(bytes.Length+read>65536)throw new InvalidDataException("Circled data budget exceeded.");bytes.Write(buffer,0,read);}
        using var json=JsonDocument.Parse(bytes.ToArray());var root=json.RootElement;
        Require("format","legacy-native-circled-payload-data-v1");
        Require("engineSha256","d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c");Require("tableSha256",ExpectedTableSha256);
        var rows=root.GetProperty("literalPayloads");if(rows.GetArrayLength()!=158)throw new InvalidDataException("Expected158 literal paths.");
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);Hash(158);var map=new Dictionary<string,int>(StringComparer.Ordinal);
        var targets=new HashSet<int>();string? previous=null;
        foreach(var row in rows.EnumerateArray())
        {
            if(row.GetArrayLength()!=2)throw new InvalidDataException("Invalid circle row.");
            string text=row[0].GetString()!;byte[] raw=Convert.FromHexString(text);int cp=row[1].GetInt32();
            if(raw.Length is not (3 or 6 or 12 or 15) || text!=Convert.ToHexString(raw).ToLowerInvariant() ||
                (previous is not null&&string.CompareOrdinal(previous,text)>=0) ||
                !(cp is >=0x3260 and <=0x327e or >=0x3280 and <=0x32b0 or >=0x32d0 and <=0x32fe))
                throw new InvalidDataException("Invalid ordered native circle payload.");
            Hash(raw.Length);hash.AppendData(raw);Hash(cp);map.Add(Convert.ToHexString(raw),cp);targets.Add(cp);previous=text;
        }
        if(targets.Count!=127 || Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant()!=ExpectedTableSha256)
            throw new InvalidDataException("Circled numeric checksum differs.");
        return new(map);
        void Require(string field,string expected){if(root.GetProperty(field).GetString()!=expected)throw new InvalidDataException("Circle provenance differs.");}
        void Hash(int value){Span<byte>b=stackalloc byte[4];BinaryPrimitives.WriteInt32LittleEndian(b,value);hash.AppendData(b);}
    }

    /// <summary>Complete literal/arithmetic classifier after canonical scratch. Unrecognized payloads returnfalse, not a guessed Unicode compatibility conversion.</summary>
    public bool TryReadCircledCodePoint(ReadOnlySpan<byte> actualCanonicalPayload, out int codePoint)
    {
        codePoint=-1;
        if(actualCanonicalPayload.Length==1)
        {
            int c=actualCanonicalPayload[0];
            if(c is >= '1' and <= '9')codePoint=c+0x242f;
            else if(c is >= 'A' and <= 'Z')codePoint=c+0x2475;
            else if(c is >= 'a' and <= 'z')codePoint=c+0x246f;
            else if(c=='0')codePoint=0x24ea;
            return codePoint>=0;
        }
        if(actualCanonicalPayload.Length==2)
        {
            int first=actualCanonicalPayload[0],second=actualCanonicalPayload[1];
            if(first=='1' && second is >= '0' and <= '9')codePoint=second+0x2439;
            else if(first=='2' && second is >= '1' and <= '9')codePoint=second+0x3220;
            else if(first=='3' && second is >= '0' and <= '5')codePoint=second+0x322a;
            else if(first=='3' && second is >= '6' and <= '9')codePoint=second+0x327b;
            else if(first=='4' && second is >= '0' and <= '9')codePoint=second+0x3285;
            else if(first=='2' && second=='0')codePoint=0x2473;
            else if(first=='5' && second=='0')codePoint=0x32bf;
            return codePoint>=0;
        }
        if(actualCanonicalPayload.Length is 3 or 6 or 12 or 15 && _map.TryGetValue(Convert.ToHexString(actualCanonicalPayload),out int mapped))
        { codePoint=mapped;return true; }
        return false;
    }
}
