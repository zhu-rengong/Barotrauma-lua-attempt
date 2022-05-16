@echo off
if not exist ".\build" mkdir ".\build"
if not exist ".\build\baro-server" mkdir ".\build\baro-server"
if not exist ".\build\baro-client" mkdir ".\build\baro-client"

cd .\baro-server
echo Building server documentation
doxygen Doxyfile

cd ..\baro-client
echo Building client documentation
doxygen Doxyfile

cd ..
echo Building shared documentation
doxygen Doxyfile