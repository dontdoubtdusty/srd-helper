@echo off
rem Makes a fast (Release) copy of Kettle in the "Kettle" folder next to the srd-helper repo (outside it),
rem then puts a "Kettle" shortcut on the Desktop and in the Start menu.
rem Also removes the old "SRD Desk" shortcuts from before the rename.
rem Run it again after changing the code. Close the app first, or the old copy can't be replaced.

cd /d "%~dp0"
set "APP=%~dp0..\..\Kettle"

dotnet publish -c Release -r win-x64 --self-contained false -o "%APP%"
if errorlevel 1 (
    echo.
    echo Build failed. See the errors above.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$app = (Resolve-Path '%APP%').Path;" ^
  "$exe = Join-Path $app 'Kettle.exe';" ^
  "$shell = New-Object -ComObject WScript.Shell;" ^
  "foreach ($folder in @([Environment]::GetFolderPath('Desktop'), [Environment]::GetFolderPath('Programs'))) {" ^
  "  $old = Join-Path $folder 'SRD Desk.lnk'; if (Test-Path $old) { Remove-Item $old };" ^
  "  $link = $shell.CreateShortcut((Join-Path $folder 'Kettle.lnk'));" ^
  "  $link.TargetPath = $exe; $link.WorkingDirectory = $app; $link.IconLocation = $exe + ',0'; $link.Save() }"

echo.
echo Done. Start Kettle from the Desktop or the Start menu.
pause
