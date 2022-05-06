if not exist "build" mkdir build
if not exist "build\baro-server" mkdir build\baro-server
if not exist "build\baro-client" mkdir build\baro-client

cd baro-client
doxygen Doxyfile.conf
cd ..

cd baro-server
doxygen Doxyfile.conf
cd ..

doxygen Doxyfile.conf