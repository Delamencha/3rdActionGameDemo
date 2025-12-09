using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Hit Effect Data")]
public class HitEffectData : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("用于在调试时区分不同受击效果数据，可选。")]
    public string EffectId;

    [Header("VFX")]
    [Tooltip("通用受击特效（火花、血雾等）。")]
    public GameObject HitVfxPrefab;

    [Tooltip("命中特效生成在受击者位置时的偏移量。")]
    public Vector3 HitVfxOffset = Vector3.zero;

    [Header("SFX")]
    [Tooltip("受击时播放的音效。")]
    public AudioClip HitSfx;
}


