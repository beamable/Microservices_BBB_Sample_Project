Rem  -------------------
Rem  Windows
Rem  -------------------

Rem  Build latest copy of shared library.
Rem  On windows, this won't work. We need a separate script for windows.
dotnet publish ..\\client\\Packages\\com.disruptorbeam.engine\\common\\ -c release -o .\\lib
dotnet publish ..\\client\\Packages\\com.beamable.server\\SharedRuntime -c release -o ./lib

Rem  Build image
docker build -t beamservice .

Rem  Clean up lib folder
@RD /S /Q ".\lib"
