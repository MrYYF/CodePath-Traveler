using System;

/// <summary>
/// 打听指令，用于存储打听消息的内容信息
/// </summary>
public class InquireAction : ActionBase
{
    [Header("打听消息数据列表")]
    [SerializeField] public List<InquireActionData> inquireActionDatas = new();

}

[Serializable]
public class InquireActionData {
    [Header("消息显示信息")]
    public string title;
    public string personName;
    public string message;
    public Sprite portraitOverride;
}
