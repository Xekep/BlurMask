using System.Buffers.Binary;
namespace InkRecognition;
// Pure mask arithmetic; source authority comes ONLY from the owning caller's
// canonical ordinary producer, never from these value arguments themselves.
internal static class LegacyEnglishCaseGraphPrimaryDefinedness
{
    internal static void Fill(int count,ReadOnlySpan<byte> alternatives,Span<byte> rowsMask,Span<byte> alternativesMask)
    {
        if((uint)count>125||alternatives.Length!=125*80||rowsMask.Length!=125*88||alternativesMask.Length!=125*80)
            throw new ArgumentException("Exact bounded125-row primary mask dimensions required.");
        rowsMask.Clear();alternativesMask.Clear();
        for(int ordinal=0;ordinal<125;ordinal++)
        {
            if(ordinal>=count)
            {
                // Only explicit inactive row cost16/increment20 stores occur.
                // NO inactive alternative record is inspected or made known,
                // especially managed-zero second40B with apparently finitecost0.
                rowsMask.Slice(ordinal*88+16,8).Fill(255);continue;
            }
            rowsMask.Slice(ordinal*88,88).Fill(255);
            for(int k=0;k<2;k++)
            {
                int at=ordinal*80+k*40;
                int known=BinaryPrimitives.ReadInt32LittleEndian(alternatives[at..])<0x4000000?24:4;
                alternativesMask.Slice(at,known).Fill(255);
            }
        }
    }
}
