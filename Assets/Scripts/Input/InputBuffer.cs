using System.Collections.Generic;
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

    public void TryAdd(PlayerBufferedInputType type, int weight)
    {
        if (!IsActive) return;
        entries.Add(new Entry
        {
            Type = type,
            Weight = weight,
            Sequence = ++sequence,
            Time = Time.time
        });
    }

    public bool TryConsumeTop(out PlayerBufferedInputType type)
    {
        type = default;
        if (entries.Count == 0) return false;

        int bestIdx = 0;
        for (int i = 1; i < entries.Count; i++)
        {
            var a = entries[i];
            var b = entries[bestIdx];
            if (a.Weight > b.Weight || (a.Weight == b.Weight && a.Sequence > b.Sequence))
            {
                bestIdx = i;
            }
        }

        type = entries[bestIdx].Type;
        entries.RemoveAt(bestIdx);
        return true;
    }
}


