using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f; // ★追加: デフォルトは1.0
    public AudioSource customSource;
}