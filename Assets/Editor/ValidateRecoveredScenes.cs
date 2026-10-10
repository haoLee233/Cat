using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public static class ValidateRecoveredScenes
{
    public static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("scene,gameObjects,activeObjects,missingScripts,spriteRenderers,emptySpriteRenderers,uiImages,emptyUiImages,videoPlayers,emptyVideoPlayers,animators,emptyAnimatorControllers,emptyAnimationSpriteFrames");
        for (var i = 0; i < 13; i++)
        {
            var path = $"Assets/Scenes/level{i}.unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var gameObjects = 0;
            var missingScripts = 0;
            var activeObjects = 0;
            var spriteRenderers = 0;
            var emptySpriteRenderers = 0;
            var uiImages = 0;
            var emptyUiImages = 0;
            var videoPlayers = 0;
            var emptyVideoPlayers = 0;
            var animators = 0;
            var emptyAnimatorControllers = 0;
            var emptyAnimationSpriteFrames = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var obj in root.GetComponentsInChildren<Transform>(true))
                {
                    gameObjects++;
                    if (obj.gameObject.activeSelf) activeObjects++;
                    missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj.gameObject);
                    foreach (var renderer in obj.GetComponents<SpriteRenderer>())
                    {
                        spriteRenderers++;
                        if (renderer.sprite == null) emptySpriteRenderers++;
                    }
                    foreach (var image in obj.GetComponents<Image>())
                    {
                        uiImages++;
                        if (image.sprite == null) emptyUiImages++;
                    }
                    foreach (var player in obj.GetComponents<VideoPlayer>())
                    {
                        videoPlayers++;
                        if (player.clip == null) emptyVideoPlayers++;
                    }
                    foreach (var animator in obj.GetComponents<Animator>())
                    {
                        animators++;
                        if (animator.runtimeAnimatorController == null)
                        {
                            emptyAnimatorControllers++;
                            continue;
                        }
                        foreach (var clip in animator.runtimeAnimatorController.animationClips)
                        {
                            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                            {
                                foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                                {
                                    if (key.value == null) emptyAnimationSpriteFrames++;
                                }
                            }
                        }
                    }
                }
            }
            report.AppendLine($"{i},{gameObjects},{activeObjects},{missingScripts},{spriteRenderers},{emptySpriteRenderers},{uiImages},{emptyUiImages},{videoPlayers},{emptyVideoPlayers},{animators},{emptyAnimatorControllers},{emptyAnimationSpriteFrames}");
            Debug.Log($"RECOVERED_SCENE_{i}: GO={gameObjects} missingScripts={missingScripts} emptySprites={emptySpriteRenderers} emptyImages={emptyUiImages}");
        }
        var output = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "SceneValidation.csv");
        File.WriteAllText(output, report.ToString(), new UTF8Encoding(true));
        Debug.Log("RECOVERED_SCENE_VALIDATION_COMPLETE " + output);
    }
}
