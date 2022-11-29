using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Asset", order = 0)]
    public class AudioAsset : ScriptableObject
    {
        [Title("Clip")]
        [SerializeField] 
        private AudioClip clip;

        [ShowInInspector] [ReadOnly] 
        private float originalClipDuration;
        
        [ShowInInspector] [ReadOnly] 
        private float usedClipDuration;

        [Title("Audio Settings")]
        [SerializeField] [MinMaxSlider(0, 1)] 
        private Vector2 normalizedTime = new (0, 1);

        [Title("", "Volume", bold: true)]
        [SerializeField] [ToggleLeft] 
        private bool useCurvedVolume;
        
        [SerializeField] [Range(0,1)] [HideIf("useCurvedVolume")] 
        private float volume = 1f;
        
        [SerializeField] [ShowIf("useCurvedVolume")] 
        private AnimationCurve volumeCurve = 
            AnimationCurve.Linear(0,1,1,1);

        [Title("", "Pitch")]
        [SerializeField] [ToggleLeft] 
        private bool useCurvedPitch;
        
        [SerializeField] [Range(-3,3)] [HideIf("useCurvedPitch")] 
        private float pitch = 1f;
        
        [SerializeField] [ShowIf("useCurvedPitch")] 
        private AnimationCurve pitchCurve = 
            AnimationCurve.Linear(0,1,1,1);

        [Title("","Loop")]
        [SerializeField] 
        private bool loopEnabled = false;

        private void OnValidate()
        {
            if (!clip) return;
            originalClipDuration = clip.length;

            usedClipDuration = originalClipDuration * (normalizedTime.y - normalizedTime.x);
        }

        [Button(ButtonSizes.Medium)]
        public void Play()
        {
            GameObject emptyGO = new GameObject("SoundSourceObject");
            AudioSource source = emptyGO.AddComponent<AudioSource>();
            
            SetAudioSourceSettings(source);
            source.Play();
            
            if (loopEnabled) return;
            Destroy(emptyGO, usedClipDuration);
        }

        private void SetAudioSourceSettings(AudioSource source)
        {
            source.clip = clip;
            source.loop = loopEnabled;
            source.time = originalClipDuration * normalizedTime.x;
            source.playOnAwake = false;
            
            SetPitch();
            SetVolume();

            void SetPitch()
            {
                if (useCurvedVolume)
                {
                    source.pitch = pitchCurve.Evaluate(0);

                    float time = 0f;

                    DOTween.To(() => time, value => time = value, 1f, usedClipDuration)
                        .SetEase(Ease.Linear)
                        .OnUpdate(() => source.pitch = pitchCurve.Evaluate(time))
                        .SetLink(source.gameObject);   
                }
                else
                {
                    source.pitch = pitch;
                }
            }

            void SetVolume()
            {
                if (useCurvedVolume)
                {
                    source.volume = volumeCurve.Evaluate(0);

                    float time = 0f;
                    DOTween.To(() => time, value => time = value, 1f, usedClipDuration)
                        .SetEase(Ease.Linear)
                        .OnUpdate(() => { source.volume = volumeCurve.Evaluate(time); })
                        .SetLink(source.gameObject);
                }
                else
                {
                    source.volume = volume;
                }
            }
        }
    }