using System;
using System.IO;
using JinYanMaoYing;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class WindowsBuild
{
    public static void Build()
    {
        string[] scenes = {
            "Assets/Scenes/封面.unity",
            "Assets/Scenes/故宫地图.unity",
            "Assets/Scenes/午门追逐.unity",
            "Assets/Scenes/保和殿水管.unity"
        };
        foreach (string path in scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int objects = 0;
            int images = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                objects += root.GetComponentsInChildren<Transform>(true).Length;
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                {
                    if (image.sprite != null) images++;
                }
            }
            if (objects < 8 || images < 1) throw new Exception("场景内容不足: " + path);
            Debug.Log("REMAKE_SCENE_CHECK " + path + " objects=" + objects + " spriteImages=" + images);
        }
        Scene runnerScene = EditorSceneManager.OpenScene(scenes[2], OpenSceneMode.Single);
        WuMenRunner runner = UnityEngine.Object.FindObjectOfType<WuMenRunner>();
        if (runner == null || runner.cat == null || runner.playArea == null || runner.coinSprite == null ||
            runner.lionSprite == null || runner.lanternSprite == null || runner.beastSprites == null || runner.beastSprites.Length != 3 ||
            runner.runFrames == null || runner.runFrames.Length != 3 || runner.backgrounds == null || runner.backgrounds.Length != 2 ||
            runner.introClip == null || runner.missionPrompt == null || runner.settingsPanel == null)
            throw new Exception("午门关卡缺少必需的玩法或图片引用");
        foreach (Sprite beast in runner.beastSprites) if (beast == null) throw new Exception("脊兽图片引用丢失");
        foreach (Sprite frame in runner.runFrames) if (frame == null) throw new Exception("黑猫跑动帧丢失");

        EditorSceneManager.OpenScene(scenes[3], OpenSceneMode.Single);
        BaoHeDianPipes pipes = UnityEngine.Object.FindObjectOfType<BaoHeDianPipes>();
        if (pipes == null || pipes.tiles == null || pipes.tiles.Length != 25 || pipes.timerText == null)
            throw new Exception("保和殿水管关卡配置不完整");
        foreach (Button tile in pipes.tiles)
        {
            if (tile == null) throw new Exception("保和殿水管按钮丢失");
            bool hasSprite = false;
            foreach (Image image in tile.GetComponentsInChildren<Image>(true))
                if (image.sprite != null) hasSprite = true;
            if (!hasSprite) throw new Exception("保和殿水管图片引用丢失: " + tile.name);
        }

        string output = Path.GetFullPath("Builds/Windows/锦檐猫影：筑梦人.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows 构建失败: " + report.summary.result);
        Debug.Log("REMAKE_WINDOWS_BUILD_READY " + output + " bytes=" + report.summary.totalSize);
    }
}
