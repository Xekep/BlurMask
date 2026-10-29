# Repeat original DOM kernel controls

Pure owned-DOM handler/pre-resolution/scalar controls:46 assertions, no NN/runtime inputs. Run managed with `dotnet run --project tools/LegacyMathRepeatOriginalDom.Probe -c Release`; publish the same project with `-r <RID> -p:PublishAot=true` for native parity. The friend assembly name is shared; keep output directories separate.

Program is the exact reviewed/tested source with five explicit MathFieldNode qualifications. These controls do not demonstrate the original tree mutation, journal, receiver or successor path.
