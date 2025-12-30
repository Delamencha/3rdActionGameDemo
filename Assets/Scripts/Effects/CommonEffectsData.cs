using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Common Effects Data")]
public class CommonEffectsData : ScriptableObject
{
    [Header("SFX")]
    [Tooltip("完美闪避时的音效。")]
    public AudioClip perfectDodgeSFX;

    [Tooltip("完美格挡时的音效。")]
    public AudioClip perfectBlockSFX;

    [Tooltip("普通格挡时的音效。")]
    public AudioClip blockSFX;
}


