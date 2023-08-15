
@echo off

echo ====================== VERSION ======================
set Version=
for /F "tokens=1,2 delims= " %%a in (ReleaseNotes.md) do (
    if NOT defined Version set Version=%%b
)
echo Version: %Version%

dotnet publish -r win-x64   -c Release -f net6.0 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:AssemblyVersion=%Version% -p:Version=%Version% -p:VersionPrefix=%Version% 
dotnet publish -r linux-x64 -c Release -f net6.0 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:AssemblyVersion=%Version% -p:Version=%Version% -p:VersionPrefix=%Version%

ECHO "D" | XCopy /S /Q /Y /F "GitState\bin\Release\net6.0\win-x64\publish\GitState.exe"   "packages\GitState.exe"
ECHO "D" | XCopy /S /Q /Y /F "GitState\bin\Release\net6.0\linux-x64\publish\GitState"     "packages\GitState"

ECHO "D" | XCopy /S /Q /Y /F "GitState\GitState.cfg"  "packages\GitState.cfg"
