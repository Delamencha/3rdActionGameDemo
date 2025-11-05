using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class InputBuffer
{
    public bool IsActive { get; private set; }

    private int sequence;

    private struct Entry
    {
        public PlayerBufferedInputType Type;
        public int Weight;
        public int Sequence;
        public float Time;
    }

    private readonly List<Entry> entries = new List<Entry>();

    private const int DefaultCapacity = 5;
    private int capacity = DefaultCapacity;

    public void SetCapacity(int cap)
    {
        capacity = Mathf.Max(1, cap);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate(bool clear)
    {
        IsActive = false;
        if (clear)
        {
            entries.Clear();
        }
    }

    public void TryAdd(PlayerBufferedInputType type)
    {
        if (!IsActive) return;

        if (entries.Count >= capacity)
        {
            int oldestIdx = 0;
            for (int i = 1; i < entries.Count; i++)
            {
                if (entries[i].Sequence < entries[oldestIdx].Sequence)
                {
                    oldestIdx = i;
                }
            }
            entries.RemoveAt(oldestIdx);
        }
        entries.Add(new Entry
        {
            Type = type,

            Sequence = ++sequence,
            Time = Time.time
        });
		Debug.Log($"[InputBuffer] Added: type={type} | buffer={GetDebugContents()}");
    }


    public bool TryConsumeType(PlayerBufferedInputType type, float maxAgeSeconds)
    {
        if (entries.Count == 0) return false;

        int candidateIdx = -1;
        int bestSequence = int.MaxValue;
        float now = Time.time;

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e.Type != type) continue;
            if (now - e.Time > maxAgeSeconds) continue;

            if (e.Sequence < bestSequence)
            {
                bestSequence = e.Sequence;
                candidateIdx = i;
            }
        }

        if (candidateIdx < 0) return false;

        entries.RemoveAt(candidateIdx);
        return true;
    }

    public bool TryConsumeType(PlayerBufferedInputType type, float maxAgeSeconds, out string consumedInfo)
    {
        consumedInfo = string.Empty;
        if (entries.Count == 0) return false;

        int candidateIdx = -1;
        int bestSequence = int.MaxValue;
        float now = Time.time;

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e.Type != type) continue;
            if (now - e.Time > maxAgeSeconds) continue;

            if (e.Sequence < bestSequence)
            {
                bestSequence = e.Sequence;
                candidateIdx = i;
            }
        }

        if (candidateIdx < 0) return false;

        var c = entries[candidateIdx];
        float age = now - c.Time;
        consumedInfo = $"type={c.Type}, weight={c.Weight}, seq={c.Sequence}, age={age:0.00}s";
        entries.RemoveAt(candidateIdx);
        return true;
    }

    public string GetDebugContents()
    {
        if (entries.Count == 0) return "[]";
        StringBuilder sb = new StringBuilder();
        sb.Append('[');
        float now = Time.time;
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            sb.Append(i);
            sb.Append(':');
            sb.Append(e.Type);
            sb.Append("(w=");
            sb.Append(e.Weight);
            sb.Append(",seq=");
            sb.Append(e.Sequence);
            sb.Append(",age=");
            sb.Append((now - e.Time).ToString("0.00"));
            sb.Append(')');
            if (i < entries.Count - 1) sb.Append(", ");
        }
        sb.Append(']');
        return sb.ToString();
    }
}


