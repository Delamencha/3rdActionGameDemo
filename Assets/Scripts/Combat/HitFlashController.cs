using System.Collections;
using UnityEngine;

/// <summary>
/// 受击闪白 Fresnel 的驱动器：通过 MaterialPropertyBlock 设置 shader 参数 _HitFlash（0..1）。
/// 建议绑定到“叠加层”SkinnedMeshRenderer（HitFlashOverlay）。
/// </summary>
public class HitFlashController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer overlayRenderer;

    [Header("Timing (unscaled)")]
    [Min(0f)]
    [SerializeField] private float riseTime = 0.02f;

    [Min(0f)]
    [SerializeField] private float fallTime = 0.08f;

    private static readonly int HitFlashId = Shader.PropertyToID("_HitFlash");

    private MaterialPropertyBlock mpb;
    private Coroutine routine;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        SetFlash(0f);
    }

    public void Play()
    {
        if (overlayRenderer == null) return;

        if (routine != null)
        {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Rise 0 -> 1
        float t = 0f;
        if (riseTime <= 0f)
        {
            SetFlash(1f);
        }
        else
        {
            while (t < riseTime)
            {
                t += Time.unscaledDeltaTime;
                SetFlash(Mathf.Clamp01(t / riseTime));
                yield return null;
            }
            SetFlash(1f);
        }

        // Fall 1 -> 0
        t = 0f;
        if (fallTime <= 0f)
        {
            SetFlash(0f);
        }
        else
        {
            while (t < fallTime)
            {
                t += Time.unscaledDeltaTime;
                SetFlash(1f - Mathf.Clamp01(t / fallTime));
                yield return null;
            }
            SetFlash(0f);
        }

        routine = null;
    }

    private void SetFlash(float v)
    {
        if (overlayRenderer == null) return;

        overlayRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(HitFlashId, Mathf.Clamp01(v));
        overlayRenderer.SetPropertyBlock(mpb);
    }
}


