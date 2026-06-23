using TMPro;
using UnityEngine.UI;

public class InfoHUD : MonoBehaviour {
    #region 信息栏组件
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Slider expSlider;
    [SerializeField, Min(0.1f)] private float expTweenDuration = 0.9f;
    #endregion

    #region 运行时缓存
    private Coroutine _expRoutine;
    private int _startLevel;
    private int _startExp;
    #endregion

    public void SetInfo(string displayName, int level, int currentExp, int targetExp, float expProgress01, Sprite protrait) {
        _startLevel = level;
        _startExp = currentExp;

        int shownTargetExp = targetExp > 0 ? targetExp : 1;
        if (protrait != null) {
            characterImage.sprite = protrait;
        }
        nameText.text = displayName;
        levelText.text = "lv." + level.ToString();
        expText.text = $"{currentExp}/{shownTargetExp}";
        expSlider.minValue = 0;
        expSlider.maxValue = 1;
        expSlider.value = Mathf.Clamp01(expProgress01);
    }

    public void PlayExpGainAnimation(CharacterRuntimeData member, int gainedExp, float delay = 0f) {
        // 停止残留动画
        StopExpRoutine();

        // 判断有无获取经验值
        if (gainedExp <= 0) {
            return;
        }

        // 读取盟友成长配置
        AllyDefinitionSO allyDef = (AllyDefinitionSO)member.Definition;
        _expRoutine = StartCoroutine(CoPlayExpGainAnimation(member, allyDef, gainedExp, delay));
    }

    private void StopExpRoutine() {
        if (_expRoutine == null) {
            return;
        }

        StopCoroutine(_expRoutine);
        _expRoutine = null;
    }

    /// <summary>
    /// 播放经验增长动画
    /// </summary>
    /// <param name="member"></param>
    /// <param name="allyDef"></param>
    /// <param name="gainedExp"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator CoPlayExpGainAnimation(CharacterRuntimeData member, AllyDefinitionSO allyDef, int gainedExp, float delay = 0f) {
        if (delay > 0) {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0;
        int lastAppliedExp = -1;

        // 按持续事件逐帧推进经验显示
        while (elapsed < expTweenDuration) {
            elapsed += Time.deltaTime;
            // 每帧推进的经验数量
            int appliedExp = Mathf.RoundToInt(gainedExp * Mathf.Clamp01(elapsed / expTweenDuration));
            if (appliedExp != lastAppliedExp) {
                BuildExpPreview(allyDef, _startLevel, _startExp, appliedExp,
                    out int level, out int exp, out int targetExp, out float progress);
                levelText.text = $"lv.{level.ToString()}";
                expText.text = $"{exp}/{targetExp}";
                expSlider.value = progress;
                lastAppliedExp = appliedExp;
            }
            yield return null;
        }

        SetInfo(member.Definition.Name, member.Level, member.CurrentExp,
            member.GetExpRequiredToNextLevel(), member.GetExpProgress01(),
            member.Definition.Portrait);
        _expRoutine = null;
    }

    /// <summary>
    /// 构建经验预览
    /// </summary>
    /// <param name="allyDef"></param>
    /// <param name="startLevel"></param>
    /// <param name="startExp"></param>
    /// <param name="gainedExp"></param>
    /// <param name="level"></param>
    /// <param name="exp"></param>
    /// <param name="targetExp"></param>
    /// <param name="progress"></param>
    private void BuildExpPreview(AllyDefinitionSO allyDef, int startLevel, int startExp, int gainedExp,
        out int level, out int exp, out int targetExp, out float progress) {
        // 开始时的等级与经验为基准
        level = startLevel;
        exp = startExp;

        // 动画演出还未使用的经验值
        int remaining = gainedExp;

        // 只要有经验还未分配则持续推进
        while (remaining > 0) {
            // 获取升到下一级所需经验
            targetExp = allyDef.GetExpRequiredToNextLevel(level);

            // 满级状态
            if (targetExp == 0) {
                exp = 0;
                progress = 1;
                return;
            }

            // 计算当前等级升级所需经验
            int need = targetExp - exp;
            if (need <= 0) {
                level++;
                exp = 0;
                continue;
            }

            // 扣除经验值
            int take = Mathf.Min(need, remaining);
            exp += take;
            remaining -= take;

            // 如果可以升级
            if (exp >= targetExp) {
                level++;
                exp = 0;
            }
        }

        // 所有经验分配完
        targetExp = allyDef.GetExpRequiredToNextLevel(level);
        // 满级
        if (targetExp == 0) {
            progress = 1f;
            return;
        }

        progress = exp / (float)targetExp;
    }
}
