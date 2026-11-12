using UnityEngine;

namespace BlockMergePuzzle.Audio
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindAnyObjectByType<SoundManager>();
#else
                    _instance = FindObjectOfType<SoundManager>();
#endif
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SoundManager");
                        _instance = go.AddComponent<SoundManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private AudioSource _effectsSource;
        private AudioClip _moveBlockClip;
        private AudioClip _mergeBlockClip;
        private AudioClip _dropClip;
        private AudioClip _mergeBaseClip;
        private AudioClip _gameOverClip;
        private AudioClip _buttonClip;
        private AudioClip _errorClip;
        private AudioClip _escapeClip;
        private AudioClip _vanishClip;

        // Combo merge settings: max 4 levels, 1.5s timeout reset
        private int _mergeComboCount = 0;
        private float _lastMergeTime = -999f;
        private const float MergeComboTimeout = 1.5f;
        private const int MaxMergeCombo = 4;

        public int CurrentMergeCombo => (Time.time - _lastMergeTime <= MergeComboTimeout) ? _mergeComboCount : 0;

        public bool SoundEnabled { get; set; } = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            SoundEnabled = PlayerPrefs.GetInt("BMP_SoundEnabled", 1) == 1;
            EnsureAudioSourceAndListener();
            LoadAudioClips();
            GenerateProceduralAudio();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void EnsureAudioSourceAndListener()
        {
            if (_effectsSource == null)
            {
                _effectsSource = GetComponent<AudioSource>();
                if (_effectsSource == null)
                {
                    _effectsSource = gameObject.AddComponent<AudioSource>();
                }
            }
            _effectsSource.playOnAwake = false;
            _effectsSource.loop = false;
            _effectsSource.mute = false;
            _effectsSource.volume = 1f;
            _effectsSource.spatialBlend = 0f; // Force 2D audio so distance from camera never mutes sound
            _effectsSource.bypassEffects = true;
            _effectsSource.bypassListenerEffects = true;
            _effectsSource.bypassReverbZones = true;

            AudioListener.volume = 1f;
            AudioListener.pause = false;

#if UNITY_2023_1_OR_NEWER
            AudioListener listener = FindAnyObjectByType<AudioListener>();
#else
            AudioListener listener = FindObjectOfType<AudioListener>();
#endif
            if (listener == null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    gameObject.AddComponent<AudioListener>();
                }
            }
            else if (!listener.enabled)
            {
                listener.enabled = true;
            }
        }

        private void LoadAudioClips()
        {
            _moveBlockClip = Resources.Load<AudioClip>("Sound/block_move")
                ?? Resources.Load<AudioClip>("Sound/moveblock");
            _mergeBlockClip = Resources.Load<AudioClip>("Sound/merge_block")
                ?? Resources.Load<AudioClip>("Sound/mergeblock");
            _vanishClip = Resources.Load<AudioClip>("Sound/block_vanish")
                ?? Resources.Load<AudioClip>("Sound/Block_Vanish");

#if UNITY_EDITOR
            if (_moveBlockClip == null)
            {
                _moveBlockClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sound/block_move.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/moveblock.wav");
            }
            if (_mergeBlockClip == null)
            {
                _mergeBlockClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sound/merge_block.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/mergeblock.wav");
            }
            if (_vanishClip == null)
            {
                _vanishClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sound/block_vanish.mp3")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Block_Vanish.mp3");
            }
#endif

            EnsureClipLoaded(_moveBlockClip);
            EnsureClipLoaded(_mergeBlockClip);
            EnsureClipLoaded(_vanishClip);
        }

        private static void EnsureClipLoaded(AudioClip clip)
        {
            if (clip != null && clip.loadState != AudioDataLoadState.Loaded)
            {
                clip.LoadAudioData();
            }
        }

        private void PlayClip(AudioClip clip, float volume, float pitch = 1f)
        {
            if (!SoundEnabled || clip == null) return;
            EnsureAudioSourceAndListener();
            EnsureClipLoaded(clip);
            _effectsSource.pitch = pitch;
            _effectsSource.PlayOneShot(clip, volume);
        }

        private void GenerateProceduralAudio()
        {
            _dropClip = GenerateTone(0.08f, 220f, 110f, 0.4f);
            _mergeBaseClip = GenerateBellTone(0.35f, 440f, 0.6f);
            _gameOverClip = GenerateGameOverTone();
            _buttonClip = GenerateTone(0.05f, 800f, 600f, 0.3f);
            _errorClip = GenerateErrorTone();
            _escapeClip = GenerateEscapeTone();
        }

        public void PlayDrop()
        {
            PlayClip(_moveBlockClip != null ? _moveBlockClip : _dropClip, 0.85f, Random.Range(0.92f, 1.02f));
        }

        public void ResetMergeCombo()
        {
            _mergeComboCount = 0;
            _lastMergeTime = -999f;
        }

        public void PlayMerge(int value = 1)
        {
            if (!SoundEnabled) return;

            float now = Time.time;
            if (now - _lastMergeTime <= MergeComboTimeout)
            {
                _mergeComboCount = Mathf.Min(_mergeComboCount + 1, MaxMergeCombo);
            }
            else
            {
                _mergeComboCount = 1;
            }
            _lastMergeTime = now;

            // 4 ascending volume steps: 0.40 -> 0.60 -> 0.80 -> 1.00
            float volume = Mathf.Lerp(0.40f, 1.00f, (_mergeComboCount - 1) / (float)(MaxMergeCombo - 1));

            if (_mergeBlockClip != null)
            {
                PlayClip(_mergeBlockClip, volume, 1.0f);
            }
            else
            {
                float[] pentatonic = { 1.0f, 1.189f, 1.335f, 1.498f, 1.682f, 2.0f, 2.378f };
                int index = Mathf.Clamp(value - 1, 0, pentatonic.Length - 1);
                PlayClip(_mergeBaseClip, volume, pentatonic[index]);
            }
        }

        public void PlayError()
        {
            PlayClip(_errorClip, 0.7f, 1.0f);
        }

        public void PlayEscape()
        {
            PlayVanish();
        }

        public void PlayVanish()
        {
            AudioClip clip = _vanishClip != null ? _vanishClip : _escapeClip;
            PlayClip(clip, 0.90f, 1.0f);
        }

        public void PlayGameOver()
        {
            PlayClip(_gameOverClip, 0.8f, 1.0f);
        }

        public void PlayButton()
        {
            PlayClip(_buttonClip, 0.5f, 1.0f);
        }

        public void PlayPickup()
        {
            PlayClip(_moveBlockClip != null ? _moveBlockClip : _buttonClip, 0.70f, Random.Range(1.15f, 1.25f));
        }

        private AudioClip GenerateTone(float duration, float startFreq, float endFreq, float volume)
        {
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / samplesCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float env = 1f - t;
                float sample = Mathf.Sin(2f * Mathf.PI * currentFreq * i / sampleRate) * env * volume;
                data[i] = sample;
            }

            AudioClip clip = AudioClip.Create("Tone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateErrorTone()
        {
            // Two-tone descending rejection buzz
            float duration = 0.24f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = t < 0.12f ? 280f : 190f;
                float env = 1f - (float)i / samplesCount;
                // Square-ish wave for buzz feeling
                float raw = Mathf.Sin(2f * Mathf.PI * freq * t);
                float buzz = Mathf.Sign(raw) * 0.45f * env;
                data[i] = buzz;
            }

            AudioClip clip = AudioClip.Create("ErrorTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateEscapeTone()
        {
            // Ascending resonant portal shimmer
            float duration = 0.45f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(380f, 920f, (float)i / samplesCount);
                float env = Mathf.Sin(Mathf.PI * (float)i / samplesCount);
                float s1 = Mathf.Sin(2f * Mathf.PI * freq * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.5f;
                data[i] = (s1 + s2) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("EscapeTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateBellTone(float duration, float freq, float volume)
        {
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 6f);
                float h1 = Mathf.Sin(2f * Mathf.PI * freq * t);
                float h2 = Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t) * 0.4f;
                float h3 = Mathf.Sin(2f * Mathf.PI * freq * 3.02f * t) * 0.2f;
                data[i] = (h1 + h2 + h3) * env * volume;
            }

            AudioClip clip = AudioClip.Create("Bell", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateGameOverTone()
        {
            float duration = 0.8f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(520f, 260f, (float)i / samplesCount);
                float env = 1f - (float)i / samplesCount;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("GameOver", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
