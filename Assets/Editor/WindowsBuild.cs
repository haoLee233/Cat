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
            "Assets/Scenes/乾清宫.unity",
            "Assets/Scenes/保和殿水管.unity",
            "Assets/Scenes/午门营造录.unity",
            "Assets/Scenes/乾清宫营造录.unity",
            "Assets/Scenes/太和殿.unity",
            "Assets/Scenes/终章.unity"
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
            runner.introClip == null || runner.missionPrompt == null || runner.settingsPanel == null || runner.crouchSprite == null ||
            runner.rankImage == null || runner.scoreText == null || runner.rankExcellent == null || runner.rankFirst == null || runner.rankSecond == null ||
            runner.hudFrame == null || runner.hudFrame.sprite == null)
            throw new Exception("午门关卡缺少必需的玩法或图片引用");
        foreach (Sprite beast in runner.beastSprites) if (beast == null) throw new Exception("脊兽图片引用丢失");
        foreach (Sprite frame in runner.runFrames) if (frame == null) throw new Exception("黑猫跑动帧丢失");

        EditorSceneManager.OpenScene(scenes[3], OpenSceneMode.Single);
        QianQingExplorer explorer = UnityEngine.Object.FindObjectOfType<QianQingExplorer>();
        if (explorer == null || explorer.rooms == null || explorer.rooms.Length != 4 ||
            explorer.cat == null || explorer.mapNodes == null || explorer.mapNodes.Length != 4 ||
            explorer.quiz == null || explorer.quizAnswers == null || explorer.quizAnswers.Length != 3 ||
            explorer.introClip == null || explorer.introPlayer == null || explorer.videoOverlay == null || explorer.videoScreen == null)
            throw new Exception("乾清宫关卡配置不完整");
        for (int i = 0; i < explorer.rooms.Length; i++)
        {
            QianQingRoom room = explorer.rooms[i];
            if (room == null || (i < 3 && room.relic == null))
                throw new Exception("乾清宫第 " + i + " 区配置不完整：room=" + (room != null) +
                    " relic=" + (room != null && room.relic != null));
        }
        if (explorer.rooms[2].GroundAt(0f) != -353f || explorer.rooms[2].relic.anchoredPosition.y > -250f)
            throw new Exception("东侧月台行走面或日晷仍位于石栏杆上");

        Scene qianQingKnowledge = EditorSceneManager.OpenScene(scenes[6], OpenSceneMode.Single);
        int educationPhotos = 0;
        foreach (GameObject root in qianQingKnowledge.GetRootGameObjects())
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
                if (image.name.Contains("照片") && image.sprite != null) educationPhotos++;
        if (educationPhotos != 2) throw new Exception("乾清宫科普页缺少授权实景照片");

        EditorSceneManager.OpenScene(scenes[4], OpenSceneMode.Single);
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

        EditorSceneManager.OpenScene(scenes[7], OpenSceneMode.Single);
        TaiHeFinale finale = UnityEngine.Object.FindObjectOfType<TaiHeFinale>();
        if (finale == null || finale.parts == null || finale.parts.Length != 3 ||
            finale.instruction == null || finale.feedback == null || finale.completionPanel == null)
            throw new Exception("太和殿终章互动配置不完整");

        // A fresh output directory prevents stale scene files from an earlier build
        // from being mixed with a newly generated player.
        string output = Path.GetFullPath("Builds/WindowsFirstPlayable/JinYanMaoYing.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows 构建失败: " + report.summary.result);
        File.Copy(Path.GetFullPath("CREDITS.md"), Path.Combine(Path.GetDirectoryName(output), "CREDITS.md"), true);
        Debug.Log("REMAKE_WINDOWS_BUILD_READY " + output + " bytes=" + report.summary.totalSize);
    }
}
