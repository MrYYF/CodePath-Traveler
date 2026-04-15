
/// <summary>
/// 玩家交互器，负责检测玩家与可交互对象的碰撞，并调用相应的交互方法
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    private CharacterIdentity _characterIdentity;
    private InteractionBase _target; //被互动的对象

    private void Awake() {
        _characterIdentity = GetComponentInParent<CharacterIdentity>();
    }

    private void Update() {
        InputSystemController inastance = InputSystemController.Inastance;
        if(inastance != null && inastance.GetPlayerConfirmPressed() && _target != null) {
            _target.Interact(_characterIdentity.CharacterDefinitionSO as AllyDefinitionSO);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out InteractionBase interactable)) {
            _target = interactable;
            interactable.OnFocus(_characterIdentity.CharacterDefinitionSO as AllyDefinitionSO);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.TryGetComponent(out InteractionBase interactable)) {
            interactable.OnLoseFocus(_characterIdentity.CharacterDefinitionSO as AllyDefinitionSO);
            _target = null;
        }
    }
}
