using System;
using UnityEngine.UI;

/// <summary>
/// 控制场景转场的淡入淡出效果，支持面板淡入淡出和擦除两种样式。通过设置样式和持续时间覆盖值，可以灵活控制转场效果的表现。提供了简单的接口来触发淡入淡出，并在完成时调用回调函数。
/// </summary>
public class FadeController : Singleton<FadeController> {
    [SerializeField] private FadeStyle defaultFadeStyle = FadeStyle.PanelFade;
    [Header("Fade Panel")]
    [SerializeField] private Image fadePanelImage;
    [Header("Wipe Mask")]
    [SerializeField] private RawImage wipeRawImage;

    [SerializeField, Range(0.05f, 3f)] private float panelFadeDuration = 0.35f;
    [SerializeField, Range(0.05f, 3f)] private float wipeFadeDuration = 0.6f;

    private FadeStyle _currentFadeStyle;
    private Coroutine _fadeCoroutine;

    private float _nextFadeOutDurationOverride = -1;
    private float _nextFadeInDurationOverride = -1;

    private Material _wipeRuntimeMaterial;

    protected override void Awake() {
        base.Awake();
        _currentFadeStyle = defaultFadeStyle;

        fadePanelImage.enabled = false;
        SetPanelAlpha(0f);

        wipeRawImage.enabled = false;
        _wipeRuntimeMaterial = wipeRawImage.material;
        SetWipeProgress(0f);
    }

    /// <summary>
    /// 设置转场的样式，切换后将使用该样式进行后续的淡入淡出操作，直到再次调用 SetStyle 切换样式。
    /// </summary>
    /// <param name="fadeStyle"></param>
    public void SetStyle(FadeStyle fadeStyle) {
        _currentFadeStyle = fadeStyle;
    }

    /// <summary>
    /// 设置下次淡入和淡出的持续时间覆盖值，优先于默认持续时间（panelFadeDuration 或 wipeFadeDuration）使用，并在使用后重置为 -1。
    /// </summary>
    /// <param name="fadeOutDuration">淡出持续时间</param>
    /// <param name="fadeInDuration">淡入持续时间</param>
    public void SetNextFadeDurations(float fadeOutDuration, float fadeInDuration) {
        _nextFadeOutDurationOverride = fadeOutDuration > 0 ? fadeOutDuration : -1f;
        _nextFadeInDurationOverride = fadeInDuration > 0 ? fadeInDuration : -1f;
    }

    /// <summary>
    /// 淡出场景
    /// </summary>
    /// <param name="onDone">结束时的回调</param>
    public void FadeOut(Action onDone = null) {
        StartFade(ResolveDuration(true), 1f, onDone);
    }

    /// <summary>
    /// 淡入场景
    /// </summary>
    /// <param name="onDone">结束时的回调</param>
    public void FadeIn(Action onDone = null) {
        StartFade(ResolveDuration(false), 0f, onDone);
    }

    private void StartFade(float duration, float target, Action onDoen) {
        if (_fadeCoroutine != null) {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeRoutine(duration, target, onDoen));
    }

    /// <summary>
    /// 按指定持续时间将面板的 alpha 平滑插值到目标值，并在完成时调用回调。
    /// </summary>
    /// <remarks>在结束时将 alpha 强制为目标值并调用回调；当 duration ≤ 0 时立即完成。</remarks>
    /// <param name="duration">插值持续时间（秒）。</param>
    /// <param name="target">目标 alpha 值（通常 0 到 1）。</param>
    /// <param name="onDone">完成后调用的可选回调。</param>
    /// <returns>用于协程的 IEnumerator，可通过 StartCoroutine 运行。</returns>
    private IEnumerator FadeRoutine(float duration, float target, Action onDone) {
        fadePanelImage.enabled = _currentFadeStyle == FadeStyle.PanelFade;
        wipeRawImage.enabled = _currentFadeStyle == FadeStyle.WipeMask;

        float elapsedTime = 0f;
        float startValue = ReadCurrentValue();
        while (elapsedTime < duration) {
            float alpha = Mathf.Lerp(startValue, target, elapsedTime / duration);
            ApplyValue(alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ApplyValue(target);
        if (target < 0f) {
            fadePanelImage.enabled = false;
            wipeRawImage.enabled = false;
            SetPanelAlpha(0f);
            SetWipeProgress(0f);
        }

        onDone?.Invoke();
        _fadeCoroutine = null;
    }

    /// <summary>
    /// 设置面板的不透明度
    /// </summary>
    /// <param name="alpha">不透明度值（通常 0 到 1）</param>
    private void SetPanelAlpha(float alpha) {
        Color color = fadePanelImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadePanelImage.color = color;
    }

    /// <summary>
    /// 设置擦除的进度
    /// </summary>
    /// <param name="progress">进度值（通常 0 到 1）</param>
    private void SetWipeProgress(float progress) {
        _wipeRuntimeMaterial.SetFloat("_Progress", Mathf.Clamp01(progress));
    }

    /// <summary>
    /// 返回用于淡入或淡出的持续时长，优先使用对应的下一次覆盖值（若已设置并在使用后重置），否则根据当前 FadeStyle 返回面板或擦除的默认时长。
    /// </summary>
    /// <remarks>若 _nextFadeOutDurationOverride 或 _nextFadeInDurationOverride >= 0，则返回该覆盖值并将其重置为 -1；否则在
    /// _currentFadeStyle 为 FadeStyle.PanelFade 时返回 panelFadeDuration，否则返回 wipeFadeDuration。</remarks>
    /// <param name="isFadeOut">指示要解析的是淡出时长（true）还是淡入时长（false）。</param>
    /// <returns>以浮点数表示的持续时长（秒）。</returns>
    private float ResolveDuration(bool isFadeOut) {
        if (isFadeOut && _nextFadeOutDurationOverride >= 0) {
            float value = _nextFadeOutDurationOverride;
            _nextFadeOutDurationOverride = -1f;
            return value;
        }

        if (!isFadeOut && _nextFadeInDurationOverride >= 0) {
            float value = _nextFadeInDurationOverride;
            _nextFadeInDurationOverride = -1f;
            return value;
        }

        return _currentFadeStyle == FadeStyle.PanelFade ? panelFadeDuration : wipeFadeDuration;
    }

    /// <summary>
    /// 根据当前的 FadeStyle 读取面板的 alpha 或擦除的进度值，并返回该值（通常在 0 到 1 的范围内）。
    /// </summary>
    /// <returns>当前 alpha 或进度值</returns>
    private float ReadCurrentValue() {
        if (_currentFadeStyle == FadeStyle.PanelFade) {
            return fadePanelImage.color.a;
        }
        else {
            return _wipeRuntimeMaterial.GetFloat("_Progress");
        }
    }

    /// <summary>
    /// 根据当前的 FadeStyle 将面板的 alpha 或擦除的进度设置为指定值，确保值在 0 到 1 的范围内。
    /// </summary>
    /// <param name="value01">要设置的值（通常 0 到 1）</param>
    private void ApplyValue(float value01) {
        value01 = Mathf.Clamp01(value01);
        if (_currentFadeStyle == FadeStyle.PanelFade) {
            SetPanelAlpha(value01);
        }
        else {
            SetWipeProgress(value01);
        }
    }
}
