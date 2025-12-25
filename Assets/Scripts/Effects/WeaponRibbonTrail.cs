using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime ribbon mesh trail driven by two transforms (base/tip).
/// Produces a quad strip between base and tip over time (ideal for slash arcs).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WeaponRibbonTrail : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private Transform basePoint;
    [SerializeField] private Transform tipPoint;

    [Header("Sampling")]
    [Tooltip("Minimum distance (meters) before a new segment is sampled.")]
    [Min(0f)]
    [SerializeField] private float minSegmentDistance = 0.02f;

    [Tooltip("Upper bound on how many sampled segments are kept (safety).")]
    [Min(4)]
    [SerializeField] private int maxSegments = 64;

    [Header("Lifetime")]
    [Tooltip("How long a sampled segment remains visible (seconds).")]
    [Min(0.01f)]
    [SerializeField] private float lifetime = 0.18f;

    [Tooltip("When emission stops, keep updating until all segments expire, then destroy this GameObject.")]
    [SerializeField] private bool destroyWhenFinished = true;

    private struct Segment
    {
        public Vector3 basePos;
        public Vector3 tipPos;
        public float time;
    }

    private readonly List<Segment> segments = new List<Segment>(128);
    private Mesh mesh;
    private MeshFilter meshFilter;
    private bool emitting = true;
    private bool hasSampledOnce;
    private Vector3 lastBase;
    private Vector3 lastTip;

    public void SetPoints(Transform baseTr, Transform tipTr)
    {
        basePoint = baseTr;
        tipPoint = tipTr;
        ResetTrail();
    }

    public void SetLifetime(float seconds)
    {
        lifetime = Mathf.Max(0.01f, seconds);
    }

    public void StopEmitting()
    {
        emitting = false;
    }

    public void ResetTrail()
    {
        segments.Clear();
        hasSampledOnce = false;
        emitting = true;
        UpdateMesh(); // clear visual immediately
    }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh { name = "WeaponRibbonTrailMesh" };
        mesh.MarkDynamic();
        meshFilter.sharedMesh = mesh;
    }

    private void LateUpdate()
    {
        float now = Time.time;

        // Remove expired segments first
        CullExpired(now);

        if (emitting)
        {
            SampleIfNeeded(now);
        }

        UpdateMesh();

        if (!emitting && destroyWhenFinished && segments.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    private void SampleIfNeeded(float now)
    {
        if (basePoint == null || tipPoint == null) return;

        Vector3 b = basePoint.position;
        Vector3 t = tipPoint.position;

        if (!hasSampledOnce)
        {
            hasSampledOnce = true;
            lastBase = b;
            lastTip = t;
            AddSegment(b, t, now);
            return;
        }

        float db = (b - lastBase).sqrMagnitude;
        float dt = (t - lastTip).sqrMagnitude;
        float minSqr = minSegmentDistance * minSegmentDistance;

        if (db >= minSqr || dt >= minSqr)
        {
            lastBase = b;
            lastTip = t;
            AddSegment(b, t, now);
        }
    }

    private void AddSegment(Vector3 b, Vector3 t, float now)
    {
        segments.Add(new Segment { basePos = b, tipPos = t, time = now });

        // Safety: if too many, drop oldest
        if (segments.Count > maxSegments)
        {
            int remove = segments.Count - maxSegments;
            segments.RemoveRange(0, remove);
        }
    }

    private void CullExpired(float now)
    {
        if (segments.Count == 0) return;

        float cutoff = now - lifetime;
        int firstAlive = 0;
        while (firstAlive < segments.Count && segments[firstAlive].time < cutoff)
        {
            firstAlive++;
        }
        if (firstAlive > 0)
        {
            segments.RemoveRange(0, firstAlive);
        }
    }

    private void UpdateMesh()
    {
        if (mesh == null) return;

        int count = segments.Count;
        if (count < 2)
        {
            mesh.Clear(false);
            return;
        }

        int vCount = count * 2;
        int triCount = (count - 1) * 2; // two triangles per segment

        var vertices = new Vector3[vCount];
        var uvs = new Vector2[vCount];
        var colors = new Color32[vCount];
        var triangles = new int[triCount * 3];

        float now = Time.time;

        // Build vertices (oldest -> newest). X along length, Y across width (0=base, 1=tip)
        for (int i = 0; i < count; i++)
        {
            Segment s = segments[i];

            float x = (count <= 1) ? 0f : (i / (float)(count - 1));
            float age01 = lifetime <= 0.0001f ? 1f : Mathf.Clamp01((now - s.time) / lifetime);
            byte a = (byte)Mathf.RoundToInt((1f - age01) * 255f);

            int vi = i * 2;
            vertices[vi + 0] = transform.InverseTransformPoint(s.basePos);
            vertices[vi + 1] = transform.InverseTransformPoint(s.tipPos);

            uvs[vi + 0] = new Vector2(x, 0f);
            uvs[vi + 1] = new Vector2(x, 1f);

            // Vertex alpha drives fade; RGB can be used as tint if needed later.
            colors[vi + 0] = new Color32(255, 255, 255, a);
            colors[vi + 1] = new Color32(255, 255, 255, a);
        }

        // Build triangle strip (quad strip)
        int ti = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int v0 = i * 2;
            int v1 = v0 + 1;
            int v2 = v0 + 2;
            int v3 = v0 + 3;

            // (v0,v2,v1) and (v2,v3,v1)
            triangles[ti++] = v0;
            triangles[ti++] = v2;
            triangles[ti++] = v1;

            triangles[ti++] = v2;
            triangles[ti++] = v3;
            triangles[ti++] = v1;
        }

        mesh.Clear(false);
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}


