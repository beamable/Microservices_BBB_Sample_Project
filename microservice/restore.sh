# Restore all projects
dotnet restore ../client/Packages/com.disruptorbeam.engine/Common/beamable.common.csproj
dotnet restore ../client/Packages/com.beamable.server/SharedRuntime/beamable.server.csproj

dotnet restore microservice.csproj
