using System.Text.Json;
internal static class JsonOutput
{
    internal static Dictionary<string,object?> D(params (string Key,object? Value)[] values)=>values.ToDictionary(x=>x.Key,x=>x.Value);
    internal static void Save(string path,object value){using var stream=File.Create(path);using var writer=new Utf8JsonWriter(stream);Write(writer,value);}
    private static void Write(Utf8JsonWriter w,object? value)
    {
        switch(value)
        {
            case null:w.WriteNullValue();break;
            case string s:w.WriteStringValue(s);break;
            case bool b:w.WriteBooleanValue(b);break;
            case int n:w.WriteNumberValue(n);break;
            case uint n:w.WriteNumberValue(n);break;
            case long n:w.WriteNumberValue(n);break;
            case ulong n:w.WriteNumberValue(n);break;
            case ushort n:w.WriteNumberValue(n);break;
            case byte n:w.WriteNumberValue(n);break;
            case Dictionary<string,object?> d:w.WriteStartObject();foreach(var item in d){w.WritePropertyName(item.Key);Write(w,item.Value);}w.WriteEndObject();break;
            case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
            default:throw new InvalidDataException("Only explicit static JSON scalars/collections admitted.");
        }
    }
}
