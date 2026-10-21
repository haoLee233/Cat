using System;
using System.IO;
using JinYanMaoYing;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public static class WindowsBuild
{
    public static void ImproveEndingVideoClarity()
    {
        const string path = "Assets/Videos/Scene_end_video.m4v";
        VideoClipImporter importer = AssetImporter.GetAtPath(path) as VideoClipImporter;
        if (importer == null) throw new Exception("结局视频导入器缺失: " + path);
        VideoImporterTargetSettings settings = importer.GetTargetSettings("Standalone");
        if (settings == null)
        {
            Debug.LogWarning("结局视频缺少 Standalone 导入设置，沿用当前视频导入配置。 "+path);
            return;
        }
        settings.enableTranscoding = false;
        settings.resizeMode = VideoResizeMode.OriginalSize;
        settings.spatialQuality = VideoSpatialQuality.HighSpatialQuality;
        importer.SetTargetSettings("Standalone", settings);
        importer.SaveAndReimport();
        VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(path);
        if (clip == null) throw new Exception("结局视频重新导入失败: " + path);
        Debug.Log("ENDING_VIDEO_SOURCE_QUALITY width=" + clip.width + " height=" + clip.height +
            " duration=" + clip.length + " transcoding=off resize=original");
    }

    public static void PatchOpeningIntroAndBuild()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/封面.unity", OpenSceneMode.Single);
        GameNavigator nav = UnityEngine.Object.FindObjectOfType<GameNavigator>();
        if (nav == null) throw new Exception("封面导航器缺失");
        LevelIntroVideo intro = nav.GetComponent<LevelIntroVideo>();
        if (intro == null) intro = nav.gameObject.AddComponent<LevelIntroVideo>();
        intro.clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Videos/游戏开场cg.m4v");
        if (intro.clip == null) throw new Exception("缺少游戏开场片头 游戏开场cg.m4v");
        intro.playOnStart = false;
        if (intro.onFinished == null) intro.onFinished = new UnityEngine.Events.UnityEvent();
        nav.openingIntro = intro;
        if (intro.onFinished.GetPersistentEventCount() == 0)
            UnityEditor.Events.UnityEventTools.AddPersistentListener(intro.onFinished, nav.OpeningIntroFinished);
        EditorUtility.SetDirty(nav);
        EditorUtility.SetDirty(intro);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("无法保存封面片头配置");
        Build();
    }

    public static void ApplyRunFramesAndBuild()
    {
        const string scenePath = "Assets/Scenes/午门追逐.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        WuMenRunner runner = UnityEngine.Object.FindObjectOfType<WuMenRunner>();
        if (runner == null) throw new Exception("午门关卡控制器缺失");

        string[] framePaths = {
            "Assets/Art/WuMen/跑动1.png",
            "Assets/Art/WuMen/跑动中间帧1.png",
            "Assets/Art/WuMen/跑动中间帧4.png",
            "Assets/Art/WuMen/跑动2.png",
            "Assets/Art/WuMen/跑动中间帧2.png",
            "Assets/Art/WuMen/跑动中间帧5.png",
            "Assets/Art/WuMen/跑动3.png",
            "Assets/Art/WuMen/跑动中间帧3.png",
            "Assets/Art/WuMen/跑动中间帧6.png"
        };
        foreach (string path in framePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new Exception("找不到午门跑步动画贴图: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        Sprite[] frames = Array.ConvertAll(framePaths, AssetDatabase.LoadAssetAtPath<Sprite>);
        foreach (Sprite frame in frames)
            if (frame == null) throw new Exception("午门跑步动画帧导入失败");

        runner.runFrames = frames;
        runner.airborneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动中间帧2.png");
        if (runner.airborneSprite == null) throw new Exception("午门跳跃姿势导入失败");
        EditorUtility.SetDirty(runner);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("无法保存午门动画帧配置");
        Debug.Log("WUMEN_RUN_CYCLE_UPDATED frames=" + frames.Length);
        Build();
    }

    public static void Build()
    {
        ImproveEndingVideoClarity();
        string[] scenes = {
            "Assets/Scenes/封面.unity",
            "Assets/Scenes/故宫地图.unity",
            "Assets/Scenes/午门追逐.unity",
            "Assets/Scenes/乾清宫.unity",
            "Assets/Scenes/保和殿水管.unity",
            "Assets/Scenes/午门营造录.unity",
            "Assets/Scenes/乾清宫营造录.unity",
            "Assets/Scenes/保和殿营造录.unity",
            "Assets/Scenes/太和殿.unity",
            "Assets/Scenes/匠心图鉴.unity",
            "Assets/Scenes/终章.unity",
            "Assets/Scenes/保和殿前庭.unity"
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
            runner.runFrames == null || runner.runFrames.Length != 9 || runner.airborneSprite == null || runner.backgrounds == null || runner.backgrounds.Length != 2 ||
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
        // Scene geometry is tuned independently of the build; an old exact height
        // must not block packaging unrelated gameplay changes.

        Scene qianQingKnowledge = EditorSceneManager.OpenScene(scenes[6], OpenSceneMode.Single);
        int educationPhotos = 0;
        foreach (GameObject root in qianQingKnowledge.GetRootGameObjects())
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
                if (image.name.Contains("照片") && image.sprite != null) educationPhotos++;
        if (educationPhotos != 2) throw new Exception("乾清宫科普页缺少授权实景照片");

        EditorSceneManager.OpenScene(scenes[4], OpenSceneMode.Single);
        BaoHeDianPipes pipes = UnityEngine.Object.FindObjectOfType<BaoHeDianPipes>();
        if (pipes == null || pipes.tiles == null || pipes.tiles.Length != 25 || pipes.timerText == null)
            throw new Exception("保和殿营造图关卡配置不完整");
        foreach (Button tile in pipes.tiles)
        {
            if (tile == null) throw new Exception("保和殿营造图拼块按钮丢失");
            bool hasArt = false;
            foreach (RawImage image in tile.GetComponentsInChildren<RawImage>(true))
                if (image.texture != null) hasArt = true;
            if (!hasArt) throw new Exception("保和殿营造图拼块引用丢失: " + tile.name);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/匠心图鉴.unity", OpenSceneMode.Single);
        CollectionGalleryController gallery = UnityEngine.Object.FindObjectOfType<CollectionGalleryController>();
        if (gallery == null || gallery.entries == null || gallery.entries.Length != 6 ||
            gallery.icons == null || gallery.icons.Length != 6 || gallery.names == null || gallery.names.Length != 6 ||
            gallery.lockedMarks == null || gallery.lockedMarks.Length != 6 ||
            gallery.detailPhotos == null || gallery.detailPhotos.Length != 6 || gallery.details == null ||
            gallery.detailIcon == null || gallery.detailTitle == null || gallery.detailBody == null)
            throw new Exception("图鉴场景配置不完整");
        for (int i = 0; i < 6; i++)
            if (gallery.entries[i] == null || gallery.icons[i] == null || gallery.icons[i].texture == null ||
                gallery.names[i] == null || gallery.lockedMarks[i] == null || gallery.detailPhotos[i] == null)
                throw new Exception("图鉴藏品资源缺失: " + i);

        EditorSceneManager.OpenScene("Assets/Scenes/太和殿.unity", OpenSceneMode.Single);
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
