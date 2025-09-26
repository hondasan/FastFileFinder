@echo off
echo === Cleaning build artifacts... ===

rem プロジェクトフォルダの bin と obj を削除
rmdir /s /q FastFileFinder\bin
rmdir /s /q FastFileFinder\obj

rem .sln と同じ階層にある .vs を削除
rmdir /s /q .vs

echo Done. You can now upload safely to GitHub.
pause
