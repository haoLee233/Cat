using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonClickSound : MonoBehaviour, IPointerDownHandler
    {
        private static AudioSource sharedSource;
        private static AudioClip clickClip;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button == null) button = GetComponent<Button>();
            if (button == null || !button.IsInteractable() || eventData.button != PointerEventData.InputButton.Left) return;
            if (clickClip == null) clickClip = Resources.Load<AudioClip>("Audio/UI_Click");
            if (clickClip == null) return;

            if (sharedSource == null)
            {
                GameObject audioObject = new GameObject("UI按键音效");
                Object.DontDestroyOnLoad(audioObject);
                sharedSource = audioObject.AddComponent<AudioSource>();
                sharedSource.playOnAwake = false;
                sharedSource.spatialBlend = 0f;
                sharedSource.volume = 0.8f;
            }

            sharedSource.PlayOneShot(clickClip);
        }
    }
}
