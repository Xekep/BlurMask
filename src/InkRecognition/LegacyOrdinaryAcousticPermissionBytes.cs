namespace InkRecognition;

// Detached scalar result, never a prepared source or execution authority.
internal sealed record LegacyOrdinaryAcousticPermissionBytes(bool Succeeded,ReadOnlyMemory<byte> Permissions);
