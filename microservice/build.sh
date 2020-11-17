cd "${0%/*}"

# build latest copy of shared library.
# on windows, this won't work. We need a separate script for windows.
/usr/local/share/dotnet/dotnet publish ../client/Packages/com.beamable.common/ -c release -o ./lib
/usr/local/share/dotnet/dotnet publish ../client/Packages/com.beamable.server/SharedRuntime -c release -o ./lib

# build image
docker build -t beamservice .

# clean up lib folder
rm -r ./lib
