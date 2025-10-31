using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackInfo
{
    public int index;
    public AttackData attack;
    
}

[CreateAssetMenu(menuName = "Combat/Combo Sequence")]
public class ComboSequenceSO : ScriptableObject
{
    //public List<AttackData> attacks = new List<AttackData>();

    public List<AttackInfo> comboInfo = new List<AttackInfo>();

    private Dictionary<int, AttackData> _dictionary;
    public Dictionary<int, AttackData> Attack_Dic
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
        _dictionary = new Dictionary<int, AttackData>();
        foreach (var pair in comboInfo)
        {
            if (!_dictionary.ContainsKey(pair.index))
                _dictionary[pair.index] = pair.attack;
        }
    }

    // 编辑器修改后重新初始化
    private void OnValidate()
    {
        InitializeDictionary();
    }

}


