"""Restore and run an independent consumer from the release NuGet feed."""
import argparse
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET

parser = argparse.ArgumentParser()
parser.add_argument("feed", type=Path)
parser.add_argument("version")
args = parser.parse_args()
feed = args.feed.resolve()
for suffix in ("", ".Math", ".RawContent"):
    if not (feed / f"NovaPen.InkRecognition{suffix}.{args.version}.nupkg").is_file():
        raise ValueError("Missing independent recognition package")
with tempfile.TemporaryDirectory(prefix="ink-package-consumer-") as temporary:
    root = Path(temporary)
    project = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
    properties = ET.SubElement(project, "PropertyGroup")
    ET.SubElement(properties, "OutputType").text = "Exe"
    ET.SubElement(properties, "TargetFramework").text = "net10.0"
    ET.SubElement(properties, "ImplicitUsings").text = "enable"
    ET.SubElement(properties, "IsAotCompatible").text = "true"
    items = ET.SubElement(project, "ItemGroup")
    # Only the outer package is referenced: both transitive library dependencies
    # must resolve with the same version from this feed, not from source projects.
    ET.SubElement(items, "PackageReference", Include="NovaPen.InkRecognition.RawContent", Version=args.version)
    ET.ElementTree(project).write(root / "consumer.csproj", encoding="utf-8")
    config = ET.Element("configuration")
    sources = ET.SubElement(config, "packageSources")
    ET.SubElement(sources, "clear")
    ET.SubElement(sources, "add", key="local-release", value=str(feed))
    ET.SubElement(sources, "add", key="nuget.org", value="https://api.nuget.org/v3/index.json")
    ET.ElementTree(config).write(root / "NuGet.Config", encoding="utf-8")
    (root / "Program.cs").write_text('''using InkRecognition;
if (LegacyAcousticFixedLog.ProbabilityCost(32768) != 0) throw new Exception("Package execution failed.");
try { LegacyRussianModelBundle.Load([], [], []); throw new Exception("Invalid assets accepted."); }
catch (InvalidDataException) { }
Console.WriteLine("PASS independent package consumer: transitive local feed, library execution and resource guards.");
''', encoding="utf-8")
    subprocess.run(["dotnet", "run", "--project", str(root / "consumer.csproj"), "-c", "Release"], cwd=root, check=True)
