using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

class BuildScript
{
    private static string[] EnabledLevels()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled && !string.IsNullOrEmpty(scene.path))
            .Select(scene => scene.path).ToArray();
    }

    private static string basePath = "../build/";
    private static string programName = "SolariaTactics.exe";

    private static string buildPath(string build)
    {
        return string.Format("{0}{1}/", basePath, build);
    }

    public static void WinBuild()
    {
        Build("win", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    public static void WinDevBuild()
    {
        Build("dev", BuildTarget.StandaloneWindows64, BuildOptions.Development);
    }

    public static void MacBuild()
    {
        Build("osx", BuildTarget.StandaloneOSXIntel64, BuildOptions.None);
    }

    public static void MacDevBuild()
    {
        Build("osx-dev", BuildTarget.StandaloneOSXIntel64, BuildOptions.Development);
    }

    public static void BuildAll()
    {
        try
        {
            WinBuild();
            WinDevBuild();
            MacBuild();
            MacDevBuild();
        }
        catch (Exception e)
        {
            Debug.LogError("Error building: \n" + e);
            Console.WriteLine("Error Building: \n"+ e);
            throw e;
        }
    }

    private static void Build(string build, BuildTarget target, BuildOptions options)
    {
        var path = buildPath(build);
        FileUtil.DeleteFileOrDirectory(path);
        BuildPipeline.BuildPlayer(EnabledLevels(), path + "bin/" + programName, target, options);
        CopyAssets(path);
    }

    private static void CopyAssets(string path)
    {
        FileUtil.CopyFileOrDirectory("../cards", path + "cards");
        FileUtil.CopyFileOrDirectory("../maps", path + "maps");
    }

}
