@echo off
rem Makes a fast (Release) copy of SRD Desk in the "SRD Desk" folder next to the srd-helper repo (outside it),
rem then puts an "SRD Desk" shortcut on the Desktop and in the Start menu.
rem Run it again after changing the code. Close the app first, or the old copy can't be replaced.

cd /d "%~dp0"
set "APP=%~dp0..\..\SRD Desk"

dotnet publish -c Release -r win-x64 --self-contained false -o "%APP%"
if errorlevel 1 (
    echo.
    echo Build failed. See the errors above.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$app = (Resolve-Path '%APP%').Path;" ^
  "$exe = Join-Path $app 'SrdDesk.exe';" ^
  "$shell = New-Object -ComObject WScript.Shell;" ^
  "foreach ($folder in @([Environment]::GetFolderPath('Desktop'), [Environment]::GetFolderPath('Programs'))) {" ^
  "  $link = $shell.CreateShortcut((Join-Path $folder 'SRD Desk.lnk'));" ^
  "  $link.TargetPath = $exe; $link.WorkingDirectory = $app; $link.IconLocation = $exe + ',0'; $link.Save() }"

echo.
echo Done. Start SRD Desk from the Desktop or the Start menu.
pause
