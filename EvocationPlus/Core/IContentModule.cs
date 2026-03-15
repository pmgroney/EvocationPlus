using Kingmaker.Blueprints;

namespace EvocationPlus.Core
{
    public interface IContentModule
    {
        string Name { get; }
        void Install(LibraryScriptableObject library);
    }
}