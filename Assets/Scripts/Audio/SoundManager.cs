using UnityEngine;

namespace BlockMergePuzzle.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource _effectsSource;
        private AudioClip _dropClip;
        private AudioClip _mergeBaseClip;
        private AudioClip _gameOverClip;
        private AudioClip _buttonClip;
        private AudioClip _errorClip;
        private AudioClip _escapeClip;

        public bool SoundEnabled { get; set; } = true;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _effectsSource = gameObject.AddComponent<AudioSource>();
            _effectsSource.playOnAwake = false;

            GenerateProceduralAudio();
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
            if (!SoundEnabled || _effectsSource == null) return;
            _effectsSource.pitch = Random.Range(0.95f, 1.05f);
            _effectsSource.PlayOneShot(_dropClip, 0.5f);
        }

        public void PlayMerge(int value = 1)
        {
            if (!SoundEnabled || _effectsSource == null) return;

            // Musical pitch scale: Root, Minor 3rd, 4th, 5th, Octave...
            float[] pentatonic = { 1.0f, 1.189f, 1.335f, 1.498f, 1.682f, 2.0f, 2.378f };
            int index = Mathf.Clamp(value - 1, 0, pentatonic.Length - 1);
            _effectsSource.pitch = pentatonic[index];
            _effectsSource.PlayOneShot(_mergeBaseClip, 0.75f);
        }

        public void PlayError()
        {
            if (!SoundEnabled || _effectsSource == null) return;
            _effectsSource.pitch = 1.0f;
            _effectsSource.PlayOneShot(_errorClip, 0.7f);
        }

        public void PlayEscape()
        {
            if (!SoundEnabled || _effectsSource == null) return;
            _effectsSource.pitch = 1.0f;
            _effectsSource.PlayOneShot(_escapeClip, 0.85f);
        }

        public void PlayGameOver()
        {
            if (!SoundEnabled || _effectsSource == null) return;
            _effectsSource.pitch = 1.0f;
            _effectsSource.PlayOneShot(_gameOverClip, 0.8f);
        }

        public void PlayButton()
        {
            if (!SoundEnabled || _effectsSource == null) return;
            _effectsSource.pitch = 1.0f;
            _effectsSource.PlayOneShot(_buttonClip, 0.5f);
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
