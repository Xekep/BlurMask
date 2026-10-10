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
#define InstallerName "BlurMask-" + AppVersion + "-win-x64-Setup"

[Setup]
AppId={{A78C0733-F377-4CC3-91D0-D2621D4532CC}
AppName=BlurMask
AppVersion={#AppVersion}
AppPublisher=Xekep
AppPublisherURL=https://github.com/Xekep/BlurMask
AppSupportURL=https://github.com/Xekep/BlurMask/issues
AppUpdatesURL=https://github.com/Xekep/BlurMask/releases
VersionInfoVersion={#NumericVersion}
VersionInfoDescription=BlurMask Windows installer
DefaultDirName={localappdata}\Programs\BlurMask
DefaultGroupName=BlurMask
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
SetupIconFile=..\Assets\BlurMask.ico
UninstallDisplayIcon={app}\BlurMask.exe
OutputDir={#InstallerOutputDir}
OutputBaseFilename={#InstallerName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "{cm:StartupTask}"; Flags: unchecked

[CustomMessages]
english.StartupTask=Start BlurMask when I sign in to Windows
russian.StartupTask=Запускать BlurMask при входе в Windows

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\BlurMask"; Filename: "{app}\BlurMask.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\BlurMask"; Filename: "{app}\BlurMask.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "BlurMask"; ValueData: """{app}\BlurMask.exe"""; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\BlurMask.exe"; Description: "{cm:LaunchProgram,BlurMask}"; Flags: nowait postinstall skipifsilent

; Local canvases in LocalAppData/BlurMask are deliberately preserved on uninstall.
