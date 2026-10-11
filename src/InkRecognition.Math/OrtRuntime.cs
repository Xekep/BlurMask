using System.Runtime.InteropServices;
using System.Text;

namespace InkRecognition.Math;

// The pinned public ORT C API is append-only. Slots below were checked against
// microsoft/onnxruntime v1.23.2 include/onnxruntime/core/session/onnxruntime_c_api.h.
// Direct unmanaged function pointers keep this bridge compatible with Native AOT.
internal sealed unsafe class OrtRuntime : IDisposable
{
    private nint _library;
    private readonly nint* _api;
    private nint _env, _memory;
    public string Version { get; }

    internal nint Slot(int index) => _api[index];

    public OrtRuntime(string? libraryPath)
    {
        var name = OperatingSystem.IsWindows() ? "onnxruntime.dll" : OperatingSystem.IsMacOS() ? "libonnxruntime.dylib" : "libonnxruntime.so";
        var bundled = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier, "native", name);
        var selected = libraryPath ?? (File.Exists(bundled) ? bundled : name);
        _library = OperatingSystem.IsWindows() && Path.IsPathFullyQualified(selected)
            ? NativeLibrary.Load(selected, typeof(OrtRuntime).Assembly, DllImportSearchPath.UseDllDirectoryForDependencies | DllImportSearchPath.SafeDirectories)
            : NativeLibrary.Load(selected);
        try
        {
            var getBase = (delegate* unmanaged<nint>)NativeLibrary.GetExport(_library, "OrtGetApiBase");
            var apiBase = (nint*)getBase();
            _api = (nint*)((delegate* unmanaged<uint, nint>)apiBase[0])(23);
            if (_api == null) throw new NotSupportedException("Math recognition requires ONNX Runtime C API23 (version1.23.2 or compatible). ");
            Version = Marshal.PtrToStringUTF8(((delegate* unmanaged<nint>)apiBase[1])()) ?? "unknown";
            using var logName = new Utf8("InkRecognition.Math");
            nint env = 0, memory = 0;
            Check(((delegate* unmanaged<int, nint, nint*, nint>)Slot(3))(3, logName.Pointer, &env));
            _env = env;
            Check(((delegate* unmanaged<int, int, nint*, nint>)Slot(69))(1, 0, &memory));
            _memory = memory;
        }
        catch { Dispose(); throw; }
    }

    public OrtSession CreateSession(ReadOnlyMemory<byte> model, string[] inputs, string[] outputs, int threads = 2)
    {
        nint options = 0, session = 0;
        try
        {
            Check(((delegate* unmanaged<nint*, nint>)Slot(10))(&options));
            Check(((delegate* unmanaged<nint, int, nint>)Slot(24))(options, threads));
            Check(((delegate* unmanaged<nint, int, nint>)Slot(25))(options, 1));
            Check(((delegate* unmanaged<nint, int, nint>)Slot(23))(options, 99));
            fixed (byte* bytes = model.Span)
                Check(((delegate* unmanaged<nint, void*, nuint, nint, nint*, nint>)Slot(8))(_env, bytes, (nuint)model.Length, options, &session));
            return new(this, session, inputs, outputs);
        }
        catch { Release(95, session); throw; }
        finally { Release(100, options); }
    }

    public OrtTensor Tensor(float[] data, params long[] shape) => CreateTensor(data, 1, sizeof(float), shape);
    public OrtTensor Tensor(long[] data, params long[] shape) => CreateTensor(data, 7, sizeof(long), shape);

    private OrtTensor CreateTensor(Array data, int elementType, int elementBytes, long[] shape)
    {
        long count = 1;
        foreach (var dimension in shape)
        {
            if (dimension < 0) throw new ArgumentException("Negative tensor dimension.");
            count = checked(count * dimension);
        }
        if (count != data.LongLength) throw new ArgumentException("Tensor shape does not match data.");
        var pin = GCHandle.Alloc(data, GCHandleType.Pinned);
        nint value = 0;
        try
        {
            fixed (long* dimensions = shape)
                Check(((delegate* unmanaged<nint, nint, nuint, long*, nuint, int, nint*, nint>)Slot(49))
                    (_memory, pin.AddrOfPinnedObject(), checked((nuint)(count * elementBytes)), dimensions, (nuint)shape.Length, elementType, &value));
            return new(this, value, pin);
        }
        catch { Release(96, value); pin.Free(); throw; }
    }

    internal void Check(nint status)
    {
        if (status == 0) return;
        try
        {
            var message = Marshal.PtrToStringUTF8(((delegate* unmanaged<nint, nint>)Slot(2))(status)) ?? "ONNX Runtime failed.";
            throw new InvalidOperationException(message);
        }
        finally { Release(93, status); }
    }

    internal void Release(int slot, nint pointer)
    {
        if (pointer != 0 && _api != null) ((delegate* unmanaged<nint, void>)Slot(slot))(pointer);
    }

    public OrtRun CreateRun(CancellationToken token) => new(this, token);

    public void Dispose()
    {
        Release(94, _memory); _memory = 0;
        Release(92, _env); _env = 0;
        if (_library != 0) { NativeLibrary.Free(_library); _library = 0; }
    }

    internal sealed class Utf8 : IDisposable
    {
        public nint Pointer { get; private set; }
        public Utf8(string text) => Pointer = Marshal.StringToCoTaskMemUTF8(text);
        public void Dispose() { if (Pointer != 0) Marshal.FreeCoTaskMem(Pointer); Pointer = 0; }
    }
}

internal sealed unsafe class OrtRun : IDisposable
{
    private readonly OrtRuntime _runtime;
    private readonly CancellationTokenRegistration _registration;
    public nint Pointer { get; private set; }
    public OrtRun(OrtRuntime runtime, CancellationToken token)
    {
        _runtime = runtime;
        nint options = 0;
        runtime.Check(((delegate* unmanaged<nint*, nint>)runtime.Slot(39))(&options));
        Pointer = options;
        _registration = token.Register(() =>
        {
            var status = ((delegate* unmanaged<nint, nint>)runtime.Slot(46))(Pointer);
            if (status != 0) runtime.Release(93, status);
        });
    }
    public void Dispose() { _registration.Dispose(); _runtime.Release(97, Pointer); Pointer = 0; }
}

internal sealed unsafe class OrtSession : IDisposable
{
    private readonly OrtRuntime _runtime;
    private readonly OrtRuntime.Utf8[] _inputs, _outputs;
    private nint _session;
    public OrtSession(OrtRuntime runtime, nint session, string[] inputs, string[] outputs)
    {
        _runtime = runtime; _session = session;
        _inputs = inputs.Select(x => new OrtRuntime.Utf8(x)).ToArray();
        _outputs = outputs.Select(x => new OrtRuntime.Utf8(x)).ToArray();
    }
    public OrtTensor[] Run(OrtRun options, params OrtTensor[] values)
    {
        if (values.Length != _inputs.Length) throw new ArgumentException("Invalid model input count.");
        var names = _inputs.Select(x => x.Pointer).ToArray();
        var handles = values.Select(x => x.Pointer).ToArray();
        var outputNames = _outputs.Select(x => x.Pointer).ToArray();
        var outputHandles = new nint[_outputs.Length];
        try
        {
            fixed (nint* inputNames = names, inputValues = handles, outputs = outputNames, results = outputHandles)
                _runtime.Check(((delegate* unmanaged<nint, nint, nint*, nint*, nuint, nint*, nuint, nint*, nint>)_runtime.Slot(9))
                    (_session, options.Pointer, inputNames, inputValues, (nuint)values.Length, outputs, (nuint)outputNames.Length, results));
            return outputHandles.Select(x => new OrtTensor(_runtime, x)).ToArray();
        }
        catch { foreach (var value in outputHandles) _runtime.Release(96, value); throw; }
    }
    public void Dispose()
    {
        _runtime.Release(95, _session); _session = 0;
        foreach (var name in _inputs.Concat(_outputs)) name.Dispose();
    }
}

internal sealed unsafe class OrtTensor : IDisposable
{
    private readonly OrtRuntime _runtime;
    private GCHandle _pin;
    public nint Pointer { get; private set; }
    public OrtTensor(OrtRuntime runtime, nint value, GCHandle pin = default) { _runtime = runtime; Pointer = value; _pin = pin; }
    private (int Type, long[] Shape, nint Data) Metadata()
    {
        nint info = 0, data = 0;
        _runtime.Check(((delegate* unmanaged<nint, nint*, nint>)_runtime.Slot(65))(Pointer, &info));
        try
        {
            int type = 0; nuint rank = 0;
            _runtime.Check(((delegate* unmanaged<nint, int*, nint>)_runtime.Slot(60))(info, &type));
            _runtime.Check(((delegate* unmanaged<nint, nuint*, nint>)_runtime.Slot(61))(info, &rank));
            if (rank > 8) throw new InvalidDataException("Unexpected tensor rank.");
            var shape = new long[(int)rank];
            fixed (long* dims = shape) _runtime.Check(((delegate* unmanaged<nint, long*, nuint, nint>)_runtime.Slot(62))(info, dims, rank));
            _runtime.Check(((delegate* unmanaged<nint, nint*, nint>)_runtime.Slot(51))(Pointer, &data));
            return (type, shape, data);
        }
        finally { _runtime.Release(99, info); }
    }
    private static int Size(long[] shape)
    {
        long size = 1;
        foreach (var dimension in shape) { if (dimension < 0) throw new InvalidDataException("Invalid output dimension."); size = checked(size * dimension); }
        if (size > 64 * 1024 * 1024) throw new InvalidDataException("Model output exceeds supported limit.");
        return (int)size;
    }
    public (float[] Values, long[] Shape) Floats()
    {
        var (type, shape, pointer) = Metadata();
        if (type != 1) throw new InvalidDataException("Expected float32 model output.");
        var data = new ReadOnlySpan<float>((void*)pointer, Size(shape)).ToArray();
        if (data.Any(x => !float.IsFinite(x))) throw new InvalidDataException("Nonfinite model output.");
        return (data, shape);
    }
    public void Dispose() { _runtime.Release(96, Pointer); Pointer = 0; if (_pin.IsAllocated) _pin.Free(); }
}
