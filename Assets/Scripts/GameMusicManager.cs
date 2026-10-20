using UnityEngine;
using UnityEngine.Video;

namespace JinYanMaoYing
{
    public sealed class GameMusicManager : MonoBehaviour
    {
        private static GameMusicManager instance;
        private AudioSource musicSource;
        private bool pausedForCutscene;
        private float nextVideoScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreatePersistentMusic()
        {
            if (instance != null) return;
            GameObject musicObject = new GameObject("全局背景音乐");
            instance = musicObject.AddComponent<GameMusicManager>();
            DontDestroyOnLoad(musicObject);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            AudioClip music = Resources.Load<AudioClip>("Audio/GaoShanLiuShui_Game_BGM");
            if (music == null)
            {
                Debug.LogError("缺少全局背景音乐 Resources/Audio/GaoShanLiuShui_Game_BGM");
                return;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.34f;
            musicSource.Play();
        }

        private void Update()
        {
            if (musicSource == null || Time.unscaledTime < nextVideoScan) return;
            nextVideoScan = Time.unscaledTime + 0.08f;

            bool cutscenePlaying = false;
            VideoPlayer[] players = FindObjectsOfType<VideoPlayer>(true);
            foreach (VideoPlayer player in players)
            {
                if (player != null && player.isPlaying)
                {
                    cutscenePlaying = true;
                    break;
                }
            }

            if (cutscenePlaying == pausedForCutscene) return;
            pausedForCutscene = cutscenePlaying;
            if (pausedForCutscene) musicSource.Pause();
            else musicSource.UnPause();
        }
    }
}
