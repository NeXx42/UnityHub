using Models.Data;
using Models.Helpers;
using Models.Interfaces;

namespace Logic.Tagging;

public class CollectionHandler_Archive : CollectionHandler_Base
{
    public override string getConfirmationMessage => "This will remove cache folder for the project.\nThis will save space, however depending on the project size cause a very long launch time.";

    public override LoadRequest[] GetTransformations(ProjectInfo info)
    {
        return [
            new LoadRequest("Removing cache", DeleteCacheForProject),
            new LoadRequest("Update project", RederiveInfo),
        ];

        async Task DeleteCacheForProject(CancellationToken token)
        {
            string[] filesToDelete = Directory.GetFiles(info.directory);
            string[] foldersToDelete = Directory.GetDirectories(info.directory);

            foreach (string file in filesToDelete)
            {
                switch (Path.GetFileName(file).ToLower())
                {
                    case ".gitignore":
                    case ".gitattributes":
                        continue;
                }

                TryToDeleteFile(file);
            }

            foreach (string folder in foldersToDelete)
            {
                switch (Path.GetFileName(folder)?.ToLowerInvariant())
                {
                    case "assets":
                    case "packages":
                    case "projectsettings":
                        continue;
                }

                TryToDeleteFolder(folder);
            }

            void TryToDeleteFile(string file)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to delete {file} - {e.Message}");
                }
            }

            void TryToDeleteFolder(string dir)
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to delete {dir} - {e.Message}");
                }
            }
        }

        async Task RederiveInfo(CancellationToken token)
        {
            IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;
            await projectLogic.DeriveProjectInfo(info, true).WhenAllProgressive(token);
        }
    }
}
