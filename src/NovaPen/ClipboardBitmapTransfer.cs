using Avalonia.Input;
using Avalonia.Media.Imaging;

namespace NovaPen;

// Clipboard backends can request pixels after SetDataAsync returns. The transfer,
// rather than the click handler, owns the bitmap until the clipboard releases it.
internal sealed class ClipboardBitmapTransfer : IDataTransfer, IAsyncDataTransfer
{
    private Bitmap? _bitmap;
    private readonly DataTransferItem[] _items;
    public IReadOnlyList<DataFormat> Formats { get; } = [DataFormat.Bitmap];
    IReadOnlyList<IDataTransferItem> IDataTransfer.Items => _items;
    IReadOnlyList<IAsyncDataTransferItem> IAsyncDataTransfer.Items => _items;

    public ClipboardBitmapTransfer(byte[] png)
    {
        using var stream = new MemoryStream(png, writable: false);
        _bitmap = new Bitmap(stream);
        _items = [DataTransferItem.Create(DataFormat.Bitmap, _bitmap)];
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _bitmap, null)?.Dispose();
    }
}
