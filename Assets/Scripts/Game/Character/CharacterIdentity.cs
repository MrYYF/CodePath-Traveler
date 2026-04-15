

/// <summary>
/// 角色身份类，用于存储角色的数据和身份信息
/// </summary>
public class CharacterIdentity : MonoBehaviour
{
    [SerializeField] private CharacterDefinitionSO _characterDefinitionSO;

    public CharacterDefinitionSO CharacterDefinitionSO => _characterDefinitionSO;

    public void SetCharacterDefinitionSO(CharacterDefinitionSO characterDefinitionSO)
    {
        _characterDefinitionSO = characterDefinitionSO;
    }

}
