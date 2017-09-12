using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections;

public class ExternalPackageManager
{
    public static ExportPackageOptions exportOptions = ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse;

    protected static PackagesJson packagesJson;

    private static string externalAssetDirectory = Path.Combine("Assets", "External");
    private static string externalAssetDirectoryPath = Path.Combine(Application.dataPath, "External");

    private static Queue<String> downloadQueue = null;
    private static int totalDownloads = 0;
    private static int downloadCount = 0;

    private static Action<Boolean> DownloadsComplete;
    private delegate void HandlePackageComplete(Boolean error);

    [MenuItem("Assets/External Package Manager/Export All")]
    static void ExportAll()
    {
        Debug.Log("EPM: ExportAll");
        parsePackagesJson();
        try
        {
            // Make sure build folder exists at project root (inferred from assets folder)
            string buildFolderPath =
                Path.Combine(Application.dataPath, Path.Combine("..", packagesJson.buildFolder));
            if (!Directory.Exists(buildFolderPath))
            {
                Directory.CreateDirectory(buildFolderPath);
            }

            // Export a package per export directory defined in packages.json
            foreach (string packageFolder in packagesJson.exports)
            {
                string packageFolderPath = Path.Combine(externalAssetDirectoryPath, packageFolder);
                ArrayList exportedPackages = new ArrayList();

                if (Directory.Exists(packageFolderPath))
                {
                    string packageBuildName = packageFolder + "-v" + packagesJson.version + ".unitypackage";
                    string packageBuildPath = Path.GetFullPath(Path.Combine(buildFolderPath, packageBuildName));

                    Debug.Log("EPM: Exporting assets from " + externalAssetDirectoryPath + " to package " + packageBuildPath);
                    AssetDatabase.ExportPackage(externalAssetDirectory, packageBuildPath, exportOptions);

                    exportedPackages.Add(packageBuildPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("External Package Manager",
                        "Unable to find package folder (" + packageFolderPath + ") for export.",
                        "OK");
                }

                if (exportedPackages.Count > 0)
                {
                    EditorUtility.DisplayDialog("External Package Manager",
                        "Exported the following packages: " +
                        string.Join("," + System.Environment.NewLine,
                            (string[])exportedPackages.ToArray(Type.GetType("System.String"))),
                        "OK");
                }

            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }

    [MenuItem("Assets/External Package Manager/Import All")]
    static void ImportAll()
    {
        // Download and import all packages from package.json
        Debug.Log("EPM: ImportAll");
        parsePackagesJson();

        downloadQueue = new Queue<string>();

        // Init queue
        packagesJson.dependencies.ForEach(downloadQueue.Enqueue);
        totalDownloads = downloadQueue.Count;
        downloadCount = 0;

        double startTime = EditorApplication.timeSinceStartup;

        DownloadsComplete = (error) =>
        {
            if (!error)
            {
                // Time it
                double totalTime = EditorApplication.timeSinceStartup - startTime;

                // Display complete dialog with elapsed time and downloaded bytes
                EditorUtility.DisplayDialog("External Package Manager",
                                            "Imported " + totalDownloads + " packages in " + Math.Floor(totalTime) + " seconds.",
                                            "OK");
            }
            DownloadsComplete = null;
        };

        // Kick off
        ImportPackage();
    }

    static void ImportPackage()
    {
        if (downloadQueue.Count > 0)
        {
            String url = downloadQueue.Dequeue();
            string tempFile = FileUtil.GetUniqueTempPathInProject();
            string dialogMessage = "Downloading " + Path.GetFileName(url) + " (" + (downloadCount + 1) + " of " + totalDownloads + ")";
            DownloadPackage(url, dialogMessage, tempFile, (Boolean error) =>
            {
                if (!error)
                {
                    // Import package
                    AssetDatabase.ImportPackage(tempFile, false);
                    FileUtil.DeleteFileOrDirectory(tempFile);
                    downloadCount++;
                    // Try to import the next package
                    ImportPackage();
                }
                else
                {
                    downloadQueue.Clear();
                    DownloadsComplete(true);
                }
            });
        }
        else
        {
            DownloadsComplete(false);
        }
    }

    private static void DownloadPackage(string dependencyUrl, string dialogMessage, string destFile, HandlePackageComplete callback)
    {
        Debug.Log("EPM: " + dialogMessage);
        float lastProgress = 0.0f;
        WWW www = new WWW(dependencyUrl);

        EditorApplication.CallbackFunction checkDownload = null;
        checkDownload = () =>
        {
            if (!www.isDone)
            {
                // Only update if this is the first update or we have made progress
                if (lastProgress < 0.01f || (www.progress - lastProgress > 0.01f))
                {
                    lastProgress = www.progress;
                    if (EditorUtility.DisplayCancelableProgressBar("External Package Manager", dialogMessage, www.progress))
                    {
                        Debug.Log("EPM: ImportAll cancelled by user.");
                        www.Dispose();

                        EditorUtility.ClearProgressBar();
                        EditorApplication.update -= checkDownload;
                        callback(true);
                    }
                }
            }
            else
            {
                EditorUtility.ClearProgressBar();

                if (!String.IsNullOrEmpty(www.error))
                {
                    EditorUtility.DisplayDialog("External Package Manager",
                            "Unable to download " + dependencyUrl + ": " + System.Environment.NewLine + www.error,
                            "OK");
                    // Remove ourselves
                    EditorApplication.update -= checkDownload;
                    callback(true);
                }
                else
                {
                    // Save package
                    File.WriteAllBytes(destFile, www.bytes);

                    // Remove ourselves
                    EditorApplication.update -= checkDownload;
                    callback(false);
                }
            }
        };

        // Check download each update cycle
        EditorApplication.update += checkDownload;
    }

    private static void parsePackagesJson()
    {
        string packagesPath = Path.Combine(Application.dataPath, "packages.json");

        Debug.Log("EPM: Loading packages file: " + packagesPath);

        try
        {
            if (File.Exists(packagesPath))
            {
                string dataAsJson = File.ReadAllText(packagesPath);
                packagesJson = JsonUtility.FromJson<PackagesJson>(dataAsJson);
            }
            else
            {
                EditorUtility.DisplayDialog("External Package Manager",
                    "Unable to find " + packagesPath,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    protected class PackagesJson
    {
        public string version;
        public string buildFolder = Path.Combine("Builds", "Packages"); // relative to project root
        public List<string> exports = new List<string>();
        public List<string> dependencies = new List<string>();
    }
}