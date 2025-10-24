using UnityEngine;

[CreateAssetMenu(menuName = "Input/Input Weights")]
public class InputWeightsSO : ScriptableObject
{
    public int AttackWeight = 100;
    public int DodgeWeight = 90;
    public int JumpWeight = 80;
    public int TargetWeight = 70;
    public int RunWeight = 60;
    public int SkillWeight = 85;
    public int BlockWeight = 50;
}
