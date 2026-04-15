

public abstract class CharacterDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public string ID;
    public string Name;
    public Sprite Portrait; // 人物立绘
    public Job Job; // 职业
}
