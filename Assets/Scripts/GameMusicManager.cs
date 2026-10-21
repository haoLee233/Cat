using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace JinYanMaoYing
{
    public sealed class GameMusicManager : MonoBehaviour
    {
        private static GameMusicManager instance;
        private AudioSource musicSource;
        private AudioClip beforeCompletionMusic;
        private AudioClip afterCompletionMusic;
        private bool globalMusicPaused;
        private bool musicStarted;
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
            beforeCompletionMusic = Resources.Load<AudioClip>("Audio/游戏通关前的背景音乐");
            afterCompletionMusic = Resources.Load<AudioClip>("Audio/游戏完成后的背景音乐");
            if (beforeCompletionMusic == null)
            {
                Debug.LogError("缺少通关前背景音乐 Resources/Audio/游戏通关前的背景音乐");
                return;
            }
            if (afterCompletionMusic == null)
                Debug.LogWarning("缺少通关后背景音乐，将沿用通关前音乐");

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.34f;
            globalMusicPaused = IsGlobalMusicSuppressed();
            SetTrack(SelectProgressTrack());
        }

        private void Update()
        {
            if (musicSource == null) return;
            AudioClip progressTrack = SelectProgressTrack();
            if (progressTrack != null && progressTrack != musicSource.clip)
                SetTrack(progressTrack);

            if (Time.unscaledTime < nextVideoScan) return;
            nextVideoScan = Time.unscaledTime + 0.08f;
            SetGlobalMusicPaused(IsGlobalMusicSuppressed());
        }

        private AudioClip SelectProgressTrack()
        {
            return GameNavigator.TaiHeDianCleared && afterCompletionMusic != null
                ? afterCompletionMusic : beforeCompletionMusic;
        }

        private static bool IsGlobalMusicSuppressed()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == GameNavigator.WuMen || sceneName == GameNavigator.Collection)
                return true;
            VideoPlayer[] players = UnityEngine.Object.FindObjectsOfType<VideoPlayer>(true);
            foreach (VideoPlayer player in players)
                if (player != null && player.isPlaying) return true;
            return false;
        }

        private void SetTrack(AudioClip track)
        {
            if (track == null) return;
            musicSource.Stop();
            musicSource.clip = track;
            musicStarted = false;
            if (!globalMusicPaused)
            {
                musicSource.Play();
                musicStarted = true;
            }
        }

        private void SetGlobalMusicPaused(bool shouldPause)
        {
            if (shouldPause == globalMusicPaused) return;
            globalMusicPaused = shouldPause;
            if (shouldPause) musicSource.Pause();
            else if (musicStarted) musicSource.UnPause();
            else
            {
                musicSource.Play();
                musicStarted = true;
            }
        }
    }
}
