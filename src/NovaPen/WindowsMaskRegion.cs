using System.Buffers;
using System.Runtime.InteropServices;

namespace NovaPen;

// Builds all sorted, disjoint tile rectangles in one GDI call. No HWND or desktop capture.
internal static partial class WindowsMaskRegion
{
    internal static unsafe nint Create(int plane, int columns, int rows, bool scramble)
    {
        var capacity = checked(8 + columns * rows * 4);
        var data = ArrayPool<int>.Shared.Rent(capacity);
        try
        {
            var length = Write(data.AsSpan(0, capacity), plane, columns, rows, scramble);
            fixed (int* buffer = data)
                return ExtCreateRegion(0, checked((uint)(length * sizeof(int))), buffer);
        }
        finally { ArrayPool<int>.Shared.Return(data); }
    }

    internal static int Write(Span<int> data, int plane, int columns, int rows, bool scramble)
    {
        var index = 8;
        var leftBound = int.MaxValue; var topBound = int.MaxValue;
        var rightBound = 0; var bottomBound = 0;
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                if (PlaneForCell(column, row, scramble) != plane) continue;
                var left = scramble ? column * 8 : column * 32 + 1;
                var top = scramble ? row * 8 : row * 32 + 1;
                var side = scramble ? 8 : 30;
                data[index++] = left; data[index++] = top;
                data[index++] = left + side; data[index++] = top + side;
                leftBound = Math.Min(leftBound, left); topBound = Math.Min(topBound, top);
                rightBound = Math.Max(rightBound, left + side); bottomBound = Math.Max(bottomBound, top + side);
            }
        data[0] = 32; data[1] = 1; // RGNDATAHEADER size / RDH_RECTANGLES
        data[2] = (index - 8) / 4; data[3] = (index - 8) * sizeof(int);
        data[4] = index == 8 ? 0 : leftBound; data[5] = index == 8 ? 0 : topBound;
        data[6] = rightBound; data[7] = bottomBound;
        return index;
    }

    internal static int PlaneForCell(int column, int row, bool scramble)
    {
        if (scramble) return (column & 1) + (row & 1) * 2;
        unchecked
        {
            uint hash = 2166136261;
            hash = (hash ^ (uint)(column + 17)) * 16777619;
            hash = (hash ^ (uint)(row + 31)) * 16777619;
            hash ^= hash >> 13; hash *= 0x5bd1e995; hash ^= hash >> 15;
            return (int)(hash % 4);
        }
    }

    [LibraryImport("gdi32.dll")]
    private static unsafe partial nint ExtCreateRegion(nint transform, uint bytes, int* data);
    [LibraryImport("gdi32.dll")]
    internal static partial int DeleteObject(nint obj);
}
