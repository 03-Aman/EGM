namespace EGM.Core.Infrastructure
{
    public static class AppPaths
    {
        public static string DataDirectory
        {
            get
            {
                string basePath = Directory.GetCurrentDirectory();

                string projectRoot = Path.GetFullPath(
                    Path.Combine(basePath, "..", "..", ".."));

                string dataPath = Path.Combine(projectRoot, "Data");

                Directory.CreateDirectory(dataPath);

                return dataPath;
            }
        }
    }
}
