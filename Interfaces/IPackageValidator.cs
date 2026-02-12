
namespace EGM.Core.Interfaces
{
    public interface IPackageValidator
    {
        Version ValidateAndExtractVersion(string packagePath, Version currentVersion);
    }
}
