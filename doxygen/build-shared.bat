@echo off
if not exist ".\build" mkdir ".\build"

echo Building shared documentation
doxygen Doxyfile