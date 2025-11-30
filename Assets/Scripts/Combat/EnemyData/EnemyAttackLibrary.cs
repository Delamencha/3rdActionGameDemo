using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyAttackInfo
{
    [HideInInspector] public string index;
    public EnemyAttakData attack;
}

[CreateAssetMenu(menuName = "Combat/Enemy Attack Library")]
public class EnemyAttackLibrary : ScriptableObject
{
    public List<EnemyAttackInfo> attackInfo = new List<EnemyAttackInfo>();

    private Dictionary<string, EnemyAttakData> _dictionary;
    public Dictionary<string, EnemyAttakData> Attack_Dic
    {
        get
        {
            if (_dictionary == null)
                InitializeDictionary();
            return _dictionary;
        }
    }

    private void InitializeDictionary()
    {
        _dictionary = new Dictionary<string, EnemyAttakData>();
        foreach (var pair in attackInfo)
        {
            if (pair.attack == null) continue;
            var key = pair.attack.AnimationName;
            if (string.IsNullOrEmpty(key)) continue;
            pair.index = key; // keep internal index synced with asset's AnimationName
            if (!_dictionary.ContainsKey(key))
                _dictionary[key] = pair.attack;
        }
    }

    private void OnValidate()
    {
        InitializeDictionary();
    }
}


