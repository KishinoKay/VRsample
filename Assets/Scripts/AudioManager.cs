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

    // --- SE 再生 ---

    // ★修正: pitchScale引数を追加 (デフォルト1.0)
    public void PlaySE(string name, float pitchScale = 1.0f)
    {
        if (seDict.TryGetValue(name, out SoundData sound))
        {
            AudioSource sourceToUse = sound.customSource != null ? sound.customSource : defaultSeSource;
            
            sourceToUse.spatialBlend = 0f;
            
            // ★追加: ピッチの設定 (データのピッチ × 引数の倍率)
            sourceToUse.pitch = sound.pitch * pitchScale;

            sourceToUse.PlayOneShot(sound.clip, sound.volume);
        }
        else
        {
            Debug.LogWarning($"SE: {name} が見つかりません");
        }
    }
    // AudioSourceを指定して鳴らすメソッド（修正版）
    public void PlaySE(AudioSource source, string name, float pitchScale = 1.0f)
    {
        if (seDict.TryGetValue(name, out SoundData sound))
        {
            source.pitch = sound.pitch * pitchScale;
            
            // ★変更点1: 音量をSource自体にセットする
            source.volume = sound.volume;
            
            // ★変更点2: クリップをセットして Play() で鳴らす
            // Play() は「再生」なので、前の音が残っていたらバツっと切って新しいのを流します。
            // これにより、音が重なってゴチャゴチャになるのを防げます。
            source.clip = sound.clip;
            source.Play(); 
        }
        else
        {
            Debug.LogWarning($"SE: {name} が見つかりません");
        }
    }
    // ★修正: pitchScale引数を追加
    public void PlaySE(string name, Vector3 position, float pitchScale = 1.0f)
    {
        if (seDict.TryGetValue(name, out SoundData sound))
        {
            GameObject tempGO = new GameObject("TempAudio_" + name);
            tempGO.transform.position = position;

            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = sound.clip;
            tempSource.volume = sound.volume;
            
            // ★追加: ピッチの設定
            tempSource.pitch = sound.pitch * pitchScale;

            tempSource.spatialBlend = 1.0f;
            tempSource.minDistance = 1.0f;
            tempSource.maxDistance = 20.0f;
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;

            tempSource.Play();

            // ピッチを変えると再生時間が変わるため、破壊までの時間も調整
            // (時間が短くなるなら早めに消す、長くなるなら待つ)
            float lifeTime = sound.clip.length / Mathf.Abs(tempSource.pitch);
            Destroy(tempGO, lifeTime);
        }
        else
        {
            Debug.LogWarning($"SE: {name} が見つかりません");
        }
    }

    // --- BGM 再生 ---

    // ★修正: pitchScale引数を追加
    public void PlayBGM(string name, float fadeDuration = 1.0f, float pitchScale = 1.0f)
    {
        if (bgmDict.TryGetValue(name, out SoundData sound))
        {
            AudioSource nextSource = sound.customSource != null ? sound.customSource : defaultBgmSource;

            // 同じ曲が流れているか確認
            if (currentBgmSource == nextSource && currentBgmSource.clip == sound.clip && currentBgmSource.isPlaying) return;

            // コルーチンにピッチ情報を渡す
            StartCoroutine(FadeSwitchBGM(nextSource, sound, fadeDuration, pitchScale));
        }
        else
        {
            Debug.LogWarning($"BGM: {name} が見つかりません");
        }
    }

    // ★修正: pitchScaleを受け取るように変更
    private IEnumerator FadeSwitchBGM(AudioSource nextSource, SoundData nextSound, float duration, float pitchScale)
    {
        // 1. フェードアウト
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

        // 2. ソース更新
        currentBgmSource = nextSource;

        // 3. 設定＆再生
        currentBgmSource.clip = nextSound.clip;
        
        // ★追加: ピッチ適用
        currentBgmSource.pitch = nextSound.pitch * pitchScale;
        
        currentBgmSource.Play();

        // 4. フェードイン
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