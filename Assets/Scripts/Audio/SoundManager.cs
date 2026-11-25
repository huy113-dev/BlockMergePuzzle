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
        private AudioClip _iceCrackClip;
        private AudioClip _victoryClip;
        private AudioClip _timeOutClip;
        private AudioClip _heartBreakClip;
        private AudioClip _reviveClip;
        private AudioClip _coinTickClip;
        private AudioClip _freezeTimeClip;
        private AudioClip _magnetPullClip;
        private AudioClip _wandCastClip;

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
            _iceCrackClip = Resources.Load<AudioClip>("Sound/icecrak")
                ?? Resources.Load<AudioClip>("Sound/ice_crack");

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
            if (_iceCrackClip == null)
            {
                _iceCrackClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/icecrak.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sound/icecrak.wav");
            }
#endif

            EnsureClipLoaded(_moveBlockClip);
            EnsureClipLoaded(_mergeBlockClip);
            EnsureClipLoaded(_vanishClip);
            EnsureClipLoaded(_iceCrackClip);
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
            _victoryClip = GenerateVictoryFanfare();
            _timeOutClip = GenerateTimeOutTone();
            _heartBreakClip = GenerateHeartBreakTone();
            _reviveClip = GenerateReviveTone();
            _coinTickClip = GenerateCoinTickTone();
            _freezeTimeClip = GenerateFreezeTimeTone();
            _magnetPullClip = GenerateMagnetPullTone();
            _wandCastClip = GenerateWandCastTone();
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

        public void PlayVictory()
        {
            PlayClip(_victoryClip, 0.85f, 1.0f);
        }

        public void PlayTimeOut()
        {
            PlayClip(_timeOutClip, 0.85f, 1.0f);
        }

        public void PlayHeartBreak()
        {
            if (!SoundEnabled) return;
            if (_iceCrackClip == null) LoadAudioClips();
            if (_iceCrackClip != null) PlayClip(_iceCrackClip, 0.55f, 0.85f);
            PlayClip(_heartBreakClip, 0.85f, 1.0f);
        }

        public void PlayRevive()
        {
            PlayClip(_reviveClip, 0.85f, 1.0f);
        }

        public void PlayCoinTick(float pitch = 1f)
        {
            PlayClip(_coinTickClip, 0.52f, pitch);
        }

        public void PlayFreezeTime()
        {
            if (_freezeTimeClip == null) _freezeTimeClip = GenerateFreezeTimeTone();
            PlayClip(_freezeTimeClip, 0.85f, 1.0f);
        }

        public void PlayMagnetPull()
        {
            if (_magnetPullClip == null) _magnetPullClip = GenerateMagnetPullTone();
            PlayClip(_magnetPullClip, 0.85f, 1.0f);
        }

        public void PlayWandCast()
        {
            if (_wandCastClip == null) _wandCastClip = GenerateWandCastTone();
            PlayClip(_wandCastClip, 0.85f, 1.0f);
        }

        public void PlayButton()
        {
            PlayClip(_buttonClip, 0.5f, 1.0f);
        }

        public void PlayPickup()
        {
            PlayClip(_moveBlockClip != null ? _moveBlockClip : _buttonClip, 0.70f, Random.Range(1.15f, 1.25f));
        }

        public void PlayIceCrack(float volume = 1.0f, float pitch = 1.0f)
        {
            if (_iceCrackClip == null)
            {
                LoadAudioClips();
            }
            PlayClip(_iceCrackClip != null ? _iceCrackClip : _mergeBaseClip, volume, pitch);
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

        private AudioClip GenerateVictoryFanfare()
        {
            float duration = 0.78f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            float[] starts = { 0.00f, 0.11f, 0.22f, 0.34f };

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float sample = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float nt = t - starts[n];
                    if (nt < 0f) continue;
                    float decay = n == notes.Length - 1 ? 4.5f : 8.5f;
                    float env = Mathf.Min(1f, nt / 0.012f) * Mathf.Exp(-nt * decay);
                    float wave = Mathf.Sin(2f * Mathf.PI * notes[n] * nt)
                               + Mathf.Sin(4f * Mathf.PI * notes[n] * nt) * 0.32f;
                    sample += wave * env * 0.28f;
                }
                data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("VictoryFanfare", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateTimeOutTone()
        {
            float duration = 0.56f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];
            float[] notes = { 587.33f, 440.00f }; // D5 -> A4 cosmic alert chime
            float[] starts = { 0.00f, 0.18f };

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float sample = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float nt = t - starts[n];
                    if (nt < 0f) continue;
                    float env = Mathf.Min(1f, nt / 0.015f) * Mathf.Exp(-nt * 6.5f);
                    float wave = Mathf.Sin(2f * Mathf.PI * notes[n] * nt)
                               + Mathf.Sin(3f * Mathf.PI * notes[n] * nt) * 0.25f;
                    sample += wave * env * 0.36f;
                }
                data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("TimeOutTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateHeartBreakTone()
        {
            float duration = 0.65f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = (float)i / samplesCount;
                float freq1 = Mathf.Lerp(340f, 155f, progress);
                float freq2 = Mathf.Lerp(405f, 185f, progress); // minor third descent
                float env = Mathf.Min(1f, t / 0.012f) * Mathf.Exp(-t * 4.5f);
                float shatter = Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.55f
                              + Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.40f;
                data[i] = Mathf.Clamp(shatter * env * 0.55f, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("HeartBreakTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateReviveTone()
        {
            float duration = 0.55f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];
            float[] notes = { 659.25f, 830.61f, 987.77f, 1318.51f }; // E5, G#5, B5, E6
            float[] starts = { 0.00f, 0.08f, 0.16f, 0.25f };

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float sample = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float nt = t - starts[n];
                    if (nt < 0f) continue;
                    float decay = n == notes.Length - 1 ? 5.5f : 9.0f;
                    float env = Mathf.Min(1f, nt / 0.010f) * Mathf.Exp(-nt * decay);
                    float wave = Mathf.Sin(2f * Mathf.PI * notes[n] * nt)
                               + Mathf.Sin(4f * Mathf.PI * notes[n] * nt) * 0.28f;
                    sample += wave * env * 0.28f;
                }
                data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("ReviveTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateCoinTickTone()
        {
            float duration = 0.11f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float sweepT = Mathf.Clamp01(t / 0.035f);
                float baseFreq = Mathf.Lerp(1318.51f, 1567.98f, sweepT); // E6 -> G6
                float env = Mathf.Min(1f, t / 0.004f) * Mathf.Exp(-t * 28f);
                float wave = Mathf.Sin(2f * Mathf.PI * baseFreq * t)
                           + Mathf.Sin(4f * Mathf.PI * baseFreq * t) * 0.35f; // + E7->G7 harmonic
                data[i] = Mathf.Clamp(wave * env * 0.42f, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("CoinTickTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateFreezeTimeTone()
        {
            float duration = 0.72f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            // Ascending crystalline frost chime: C6, E6, G6, B6, D7
            float[] notes = { 1046.50f, 1318.51f, 1567.98f, 1975.53f, 2349.32f };
            float[] starts = { 0.00f, 0.06f, 0.13f, 0.21f, 0.30f };

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float sample = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    float nt = t - starts[n];
                    if (nt < 0f) continue;
                    float decay = n == notes.Length - 1 ? 5.2f : 8.5f;
                    float env = Mathf.Min(1f, nt / 0.008f) * Mathf.Exp(-nt * decay);
                    float glass = Mathf.Sin(2f * Mathf.PI * notes[n] * nt) * 0.65f
                                + Mathf.Sin(4f * Mathf.PI * notes[n] * nt) * 0.25f
                                + Mathf.Sin(6f * Mathf.PI * notes[n] * nt) * 0.10f;
                    sample += glass * env * 0.24f;
                }

                // Subtle icy shimmer tail
                float shimmerEnv = Mathf.Exp(-t * 4.5f) * Mathf.Min(1f, t / 0.04f);
                float shimmer = Mathf.Sin(2f * Mathf.PI * 3135.96f * t) * 0.08f
                              + Mathf.Sin(2f * Mathf.PI * 4186.01f * t) * 0.06f;
                sample += shimmer * shimmerEnv;

                data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("FreezeTimeTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateMagnetPullTone()
        {
            float duration = 0.34f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float norm = (float)i / samplesCount;
                float freq = Mathf.Lerp(320f, 880f, norm * norm);
                float env = Mathf.Min(1f, t / 0.015f) * Mathf.Exp(-t * 5.2f);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f
                           + Mathf.Sin(2f * Mathf.PI * freq * 2.5f * t) * 0.25f;
                data[i] = Mathf.Clamp(wave * env * 0.55f, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("MagnetPullTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateWandCastTone()
        {
            float duration = 0.68f;
            int sampleRate = 44100;
            int samplesCount = (int)(duration * sampleRate);
            float[] data = new float[samplesCount];

            // Ascending magical harp/chime glissando: A5, C#6, E6, G#6, B6, E7
            float[] notes = { 880.00f, 1108.73f, 1318.51f, 1661.22f, 1975.53f, 2637.02f };
            float[] starts = { 0.00f, 0.055f, 0.11f, 0.17f, 0.23f, 0.30f };

            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float sample = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    float nt = t - starts[n];
                    if (nt < 0f) continue;
                    float decay = n == notes.Length - 1 ? 5.0f : 8.8f;
                    float env = Mathf.Min(1f, nt / 0.007f) * Mathf.Exp(-nt * decay);
                    float chime = Mathf.Sin(2f * Mathf.PI * notes[n] * nt) * 0.65f
                                + Mathf.Sin(4f * Mathf.PI * notes[n] * nt) * 0.25f
                                + Mathf.Sin(6f * Mathf.PI * notes[n] * nt) * 0.10f;
                    sample += chime * env * 0.22f;
                }

                float shimmerEnv = Mathf.Exp(-Mathf.Max(0f, t - 0.18f) * 4.8f) * Mathf.Min(1f, t / 0.05f);
                float shimmer = Mathf.Sin(2f * Mathf.PI * 3520.00f * t) * 0.07f
                              + Mathf.Sin(2f * Mathf.PI * 4434.92f * t) * 0.05f;
                sample += shimmer * shimmerEnv;

                data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("WandCastTone", samplesCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
