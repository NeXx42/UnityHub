using Models.Data;
using Models.Helpers;
using Models.Interfaces;

namespace Logic.Tagging;

public class CollectionHandler_Archive : CollectionHandler_Base
{
    public override string getConfirmationMessage => "This will remove cache folder for the project.\nThis will save space, however depending on the project size cause a very long launch time.";

    public override LoadRequest[] GetTransformations(ProjectInfo info)
    {
        IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;

        return [
            new LoadRequest("Removing cache", DeleteCacheForProject, true),
            ..projectLogic.DeriveProjectInfo(info, true)
        ];

        async Task DeleteCacheForProject(IProgress<float> progress, CancellationToken token)
        {
            progress.Report(0);

            string[] filesToDelete = Directory.GetFiles(info.directory);
            string[] foldersToDelete = Directory.GetDirectories(info.directory);

            float totalToDelete = filesToDelete.Length + foldersToDelete.Length;
            int deletedCount = 0;

            foreach (string file in filesToDelete)
            {
                switch (Path.GetFileName(file).ToLower())
                {
                    case ".gitignore":
                    case ".gitattributes":
                        continue;
                }

                TryToDeleteFile(file);
                IncrementProgress();
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
                IncrementProgress();
            }

            void TryToDeleteFile(string file)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    LoggingHelper.LogError($"Failed to delete {file} - {e.Message}");
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
                    LoggingHelper.LogError($"Failed to delete {dir} - {e.Message}");
                }
            }

            void IncrementProgress()
            {
                deletedCount++;
                progress.Report(deletedCount / totalToDelete);
            }
        }
    }
}
