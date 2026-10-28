using Cb8OwnerProposal;

var service = new Services();
int count = 0;
void Check(byte tag, byte[] payload, long expected, long fallback = 1234)
{
    if (Document031E20.Read(tag, payload, fallback, service) != expected)
        throw new InvalidDataException($"031E20 fixture {count}");
    count++;
}
Check(4, BitConverter.GetBytes(long.MinValue), long.MinValue);
Check(9, BitConverter.GetBytes(ulong.MaxValue), -1);
Check(7, BitConverter.GetBytes(int.MinValue), int.MinValue);
Check(8, BitConverter.GetBytes(uint.MaxValue), 4294967295);
Check(6, [255], 1); Check(6, [0], 0);
Check(5, BitConverter.GetBytes(-2.5), -3);
Check(5, BitConverter.GetBytes(2.5), 3);
Check(5, BitConverter.GetBytes(double.NaN), long.MinValue);
Check(5, BitConverter.GetBytes(double.PositiveInfinity), long.MinValue);
Check(10, BitConverter.GetBytes(-2.5f), -3);
Check(10, BitConverter.GetBytes(2147483648f), int.MinValue);
Check(10, BitConverter.GetBytes(float.NaN), int.MinValue);
Check(3, [], long.MaxValue, long.MaxValue);
Check(0, [], -1, -1);
if (service.Errors != 2 || service.Conversions != 0 || service.Parses != 0) throw new InvalidDataException("Typed/default dispatch");
service.End = 2; service.Value = 42; Check(1, [52, 50], 42);
service.End = 1; Check(2, [52, 50], 1234);
service.End = null; Check(2, [52, 50], -1, -1);
service.End = 0; Check(1, [52, 50], long.MaxValue, long.MaxValue);
if (service.Errors != 2 || service.Conversions != 4 || service.Parses != 4) throw new InvalidDataException("String dispatch");
Console.WriteLine($"031E20 proposed kernel: {count} independent branch controls passed; no vendor execution.");

sealed class Services : INumericAttributeServices
{
    internal int Errors, Conversions, Parses;
    internal int? End;
    internal long Value;
    public ReadOnlyMemory<byte> StringBytes(byte tag, ReadOnlyMemory<byte> payload) { Conversions++; return payload; }
    public int? ParseInt64(ReadOnlyMemory<byte> text, out long value) { Parses++; value = Value; return End; }
    public void UnsupportedValue(uint error) { if (error != 0x02000000) throw new InvalidDataException(); Errors++; }
}
