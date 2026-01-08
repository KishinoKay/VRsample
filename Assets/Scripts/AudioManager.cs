using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("登録リスト")]
    [SerializeField] private List<SoundData> bgmList;
    [SerializeField] private List<SoundData> seList;

    [Header("デフォルト Audio Source")]
    [SerializeField] private AudioSource defaultBgmSource;
    [SerializeField] private AudioSource defaultSeSource;

    private Dictionary<string, SoundData> bgmDict = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> seDict = new Dictionary<string, SoundData>();

    private AudioSource currentBgmSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary();
            currentBgmSource = defaultBgmSource;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        foreach (var sound in bgmList) bgmDict[sound.name] = sound;
        foreach (var sound in seList) seDict[sound.name] = sound;
    }

    // --- SE 再生 (2D) ---

    public void PlaySE(string name, float pitchScale = 1.0f)
    {
        if (seDict.TryGetValue(name, out SoundData sound))
        {
            AudioSource sourceToUse = sound.customSource != null ? sound.customSource : defaultSeSource;
            
            sourceToUse.spatialBlend = 0f; // 2Dサウンド
            sourceToUse.pitch = sound.pitch * pitchScale;

            sourceToUse.PlayOneShot(sound.clip, sound.volume);
        }
        else
        {
            Debug.LogWarning($"SE: {name} が見つかりません");
        }
    }

    public void PlaySE(AudioSource source, string name, float pitchScale = 1.0f)
    {
        if (seDict.TryGetValue(name, out SoundData sound))
        {
            source.pitch = sound.pitch * pitchScale;
            source.volume = sound.volume;
            source.clip = sound.clip;
            source.Play(); 
        }
        else
        {
            Debug.LogWarning($"SE: {name} が見つかりません");
        }
    }

    // --- SE 再生 (3D / 場所指定) ---

    // ★修正: 引数を大幅に追加しました
    // spatialBlend: 0=2D, 1=3D (デフォルト1)
    // minDistance: この距離までは最大音量 (デフォルト1m)
    // maxDistance: 音が聞こえなくなる距離 (デフォルト20m)
    public void PlaySE(string name, Vector3 position, float pitchScale = 1.0f, float spatialBlend = 1.0f, float minDistance = 1.0f, float maxDistance = 20.0f)
    {
        if (seDict.TryGetValue(name, out SoundData sound))
        {
            GameObject tempGO = new GameObject("TempAudio_" + name);
            tempGO.transform.position = position;

            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = sound.clip;
            tempSource.volume = sound.volume;
            
            // ピッチ
            tempSource.pitch = sound.pitch * pitchScale;

            // ★追加した設定を適用
            tempSource.spatialBlend = spatialBlend;
            tempSource.minDistance = minDistance;
            tempSource.maxDistance = maxDistance;
            
            // 減衰カーブ（対数）
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;

            tempSource.Play();

            float lifeTime = sound.clip.length / Mathf.Abs(tempSource.pitch);
            Destroy(tempGO, lifeTime);
        }
        else
        {
            Debug.LogWarning($"SE: {name} が見つかりません");
        }
    }

    // --- BGM 再生 ---

    public void PlayBGM(string name, float fadeDuration = 1.0f, float pitchScale = 1.0f)
    {
        if (bgmDict.TryGetValue(name, out SoundData sound))
        {
            AudioSource nextSource = sound.customSource != null ? sound.customSource : defaultBgmSource;

            if (currentBgmSource == nextSource && currentBgmSource.clip == sound.clip && currentBgmSource.isPlaying) return;

            StartCoroutine(FadeSwitchBGM(nextSource, sound, fadeDuration, pitchScale));
        }
        else
        {
            Debug.LogWarning($"BGM: {name} が見つかりません");
        }
    }

    private IEnumerator FadeSwitchBGM(AudioSource nextSource, SoundData nextSound, float duration, float pitchScale)
    {
        if (currentBgmSource != null && currentBgmSource.isPlaying)
        {
            float startVolume = currentBgmSource.volume;
            for (float t = 0; t < duration / 2; t += Time.deltaTime)
            {
                currentBgmSource.volume = Mathf.Lerp(startVolume, 0, t / (duration / 2));
                yield return null;
            }
            currentBgmSource.volume = 0;
            currentBgmSource.Stop();
        }

        currentBgmSource = nextSource;
        currentBgmSource.clip = nextSound.clip;
        currentBgmSource.pitch = nextSound.pitch * pitchScale;
        currentBgmSource.Play();

        for (float t = 0; t < duration / 2; t += Time.deltaTime)
        {
            currentBgmSource.volume = Mathf.Lerp(0, nextSound.volume, t / (duration / 2));
            yield return null;
        }
        currentBgmSource.volume = nextSound.volume;
    }

    public void StopBGM()
    {
        if (currentBgmSource != null)
        {
            currentBgmSource.Stop();
        }
    }
}