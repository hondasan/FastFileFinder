@echo off
echo === Cleaning build artifacts... ===

rem �v���W�F�N�g�t�H���_�� bin �� obj ���폜
rmdir /s /q FastFileFinder\bin
rmdir /s /q FastFileFinder\obj

rem .sln �Ɠ����K�w�ɂ��� .vs ���폜
rmdir /s /q .vs

echo Done. You can now upload safely to GitHub.
pause
