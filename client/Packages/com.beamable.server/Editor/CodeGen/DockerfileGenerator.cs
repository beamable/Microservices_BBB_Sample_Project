using System.IO;

namespace Beamable.Server.Editor.CodeGen
{
   public class DockerfileGenerator
   {
      public MicroserviceDescriptor Descriptor { get; }


      public const string BASE_IMAGE = "beamservice";
      public const string BASE_TAG = "latest"; // TODO this should be the version of the server package itself. SET AT JENKINS BUILD

      public DockerfileGenerator(MicroserviceDescriptor descriptor)
      {
         Descriptor = descriptor;
      }

      public string GetString()
      {
         var text = $@"FROM {BASE_IMAGE}:{BASE_TAG} AS build
WORKDIR /subsrc
COPY {Descriptor.ImageName}.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /subapp

WORKDIR /subapp
ENTRYPOINT [""dotnet"", ""{Descriptor.ImageName}.dll""] ";
         return text;
      }

      public void Generate(string filePath)
      {
         var content = GetString();
         File.WriteAllText(filePath, content);
      }

   }
}