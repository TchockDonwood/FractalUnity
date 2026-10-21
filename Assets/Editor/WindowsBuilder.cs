#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WindowsBuilder
{
    [MenuItem("Fractal/Build Windows x64")]
    public static void BuildWindows()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("Ошибка",
                "Сохраните сцену (File → Save As…) перед сборкой.", "OK");
            return;
        }

        var path = EditorUtility.SaveFilePanel(
            "Куда собрать сборку", "", "KochFractal.exe", "exe");
        if (string.IsNullOrEmpty(path)) return;

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { scene.path },
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("Готово",
                $"Сборка: {path}\nРазмер: {report.summary.totalSize / 1024 / 1024} МБ", "OK");
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            EditorUtility.DisplayDialog("Ошибка сборки",
                report.summary.result.ToString(), "OK");
        }
    }
}
#endif