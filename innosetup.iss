; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "IOCam"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "AXIPlus"
#define MyAppURL "https://www.axiplus.com/"
#define MyAppExeName "IOCam.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{9D21E6F8-0055-435D-80D7-655808ED4324}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=R:\IOCam\LICENSE
InfoBeforeFile=R:\IOCam\INSTALL.txt
;PrivilegesRequired=lowest
OutputDir=R:\IOCam\bin
OutputBaseFilename={#MyAppName}-{#MyAppVersion}
SetupIconFile=R:\IOCam\tooth.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostarticon"; Description: "{cm:AutoStartProgram,{#MyAppName}}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "R:\IOCam\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\AForge.Controls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\AForge.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\AForge.Imaging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\AForge.Math.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\AForge.Video.DirectShow.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\AForge.Video.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\IOCam.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "R:\IOCam\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{commonstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: autostarticon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

