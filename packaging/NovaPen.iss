; Build through build-installer.ps1. AppId must remain stable across upgrades.
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef NumericVersion
  #define NumericVersion "1.0.0.0"
#endif
#ifndef PublishDir
  #error PublishDir must point to the Windows x64 Native AOT publish directory.
#endif
#ifndef InstallerOutputDir
  #define InstallerOutputDir "..\.artifacts\installers"
#endif
#ifndef SolverDir
  #error SolverDir must contain the verified Giac CLI, dependencies and licenses.
#endif
#define InstallerName "NovaPen-" + AppVersion + "-win-x64-Setup"

[Setup]
AppId={{A78C0733-F377-4CC3-91D0-D2621D4532CC}
AppName=NovaPen
AppVersion={#AppVersion}
AppPublisher=Xekep
AppPublisherURL=https://github.com/Xekep/NovaPen
AppSupportURL=https://github.com/Xekep/NovaPen/issues
AppUpdatesURL=https://github.com/Xekep/NovaPen/releases
VersionInfoVersion={#NumericVersion}
VersionInfoDescription=NovaPen Windows installer
DefaultDirName={localappdata}\Programs\NovaPen
DefaultGroupName=NovaPen
PrivilegesRequired=lowest
MinVersion=10.0
SetupArchitecture=x64
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppMutex=Xekep.BlurMask.SingleInstance.v1
SetupMutex=Xekep.BlurMask.Setup.v1
CloseApplications=no
RestartApplications=no
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
SetupIconFile=..\src\NovaPen\Assets\NovaPen.ico
UninstallDisplayIcon={app}\NovaPen.exe
OutputDir={#InstallerOutputDir}
OutputBaseFilename={#InstallerName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "{cm:StartupTask}"; Flags: checkedonce

[CustomMessages]
english.StartupTask=Start NovaPen when I sign in to Windows
french.StartupTask=Lancer NovaPen à l’ouverture de session Windows
german.StartupTask=NovaPen bei der Windows-Anmeldung starten
russian.StartupTask=Запускать NovaPen при входе в Windows

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb,runtimes\win-x64\solver\*"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SolverDir}\*"; DestDir: "{app}\runtimes\win-x64\solver"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[InstallDelete]
; Exact legacy branding artifacts only; never remove user directories or data.
Type: files; Name: "{app}\BlurMask.exe"
Type: files; Name: "{autoprograms}\BlurMask.lnk"
Type: files; Name: "{autodesktop}\BlurMask.lnk"
; Upgrades from the old dynamic build must not keep unused graphics dependencies.
Type: files; Name: "{app}\av_libglesv2.dll"
Type: files; Name: "{app}\libSkiaSharp.dll"
Type: files; Name: "{app}\libHarfBuzzSharp.dll"
Type: files; Name: "{app}\libSkiaSharp.pdb"
Type: files; Name: "{app}\libHarfBuzzSharp.pdb"

[Icons]
Name: "{autoprograms}\NovaPen"; Filename: "{app}\NovaPen.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NovaPen.exe"; AppUserModelID: "Xekep.NovaPen"
Name: "{autodesktop}\NovaPen"; Filename: "{app}\NovaPen.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Registry]
; Retire only the previous startup value, including when startup is deselected.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "BlurMask"; Flags: deletevalue
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "NovaPen"; ValueData: """{app}\NovaPen.exe"" --minimized"; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\NovaPen.exe"; Description: "{cm:LaunchProgram,NovaPen}"; Flags: nowait postinstall skipifsilent

; Existing settings/canvases remain under LocalAppData/BlurMask and are preserved on uninstall.
