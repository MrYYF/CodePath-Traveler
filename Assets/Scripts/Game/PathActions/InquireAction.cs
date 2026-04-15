using System;

/// <summary>
/// 打听指令，用于存储打听消息的内容信息
/// </summary>
public class InquireAction : ActionBase
{
    [Header("打听消息数据列表")]
    [SerializeField] public List<InquireActionData> inquireActionDatas = new();

    public int PickRandomMessageIndex() => UnityEngine.Random.Range(0, inquireActionDatas.Count);

    public void GetInquireActionData(int index, out InquireActionData inquireActionData) => inquireActionData = inquireActionDatas[index];

    public override void TriggerAction(AllyDefinitionSO inteactor) {
        EventBus.Publish(new PanelRequestEvent(this));
    }
}

[Serializable]
public class InquireActionData {
    [Header("消息显示信息")]
    public string title;
    public string personName;
    public string message;
    public Sprite portraitOverride;
}
