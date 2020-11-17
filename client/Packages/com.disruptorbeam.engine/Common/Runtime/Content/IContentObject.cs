namespace Beamable.Common.Content
{
   public interface IContentObject
   {
      string Id { get; }
      string Version { get; }

      void SetIdAndVersion(string id, string version);
   }
}