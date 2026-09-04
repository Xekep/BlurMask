# Contributing

1. Use the SDK version from `global.json`.
2. Keep the app Native AOT compatible and trimming-safe.
3. Avoid adding reflection-heavy UI frameworks or XAML unless there is a concrete reason.
4. Run `dotnet build -c Release -p:PublishAot=false` before submitting changes.
5. On the target OS, run a Native AOT publish and the `--smoke-test` executable.

Changes that touch platform interop should be tested on the affected OS.
