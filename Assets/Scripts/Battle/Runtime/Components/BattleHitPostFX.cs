using UnityEngine.Rendering;

public class BattleHitPostFX : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Volume breakVolume;
    [SerializeField] private bool disableWhenIdle = true;

    [Header("Flash")]
    [SerializeField, Range(0f, 1f)] private float flashWeight = 1f;
    [SerializeField, Range(0f, 1f)] private float flashDuration = 0.06f;

    private Coroutine _playRoutine;

    private void Awake() {
        ResetVolume();
    }

    public void Play() {
        // 清空之前的协程
        if (_playRoutine != null) {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }
        ResetVolume();

        // 将breakvolume置为目标权重
        breakVolume.enabled = true;
        breakVolume.weight = flashWeight;

        // 保持一个极短时间后立即关闭，闪屏
        if(flashDuration <= 0f) {
            ResetVolume();
            return;
        }

        _playRoutine = StartCoroutine(FlashOnce());
    }

    private void ResetVolume() {
        breakVolume.weight = 0f;
        if (disableWhenIdle) {
            breakVolume.enabled = false;
        }
    }

    private IEnumerator FlashOnce() {
        yield return new WaitForSeconds(flashDuration);
        ResetVolume();
        _playRoutine = null;
    }
}
