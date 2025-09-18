using DefaultNamespace.Quests;
using UnityEngine;

public class ButtonClickEvent: IQuestEvent
{
    public string Key { get; }
    public int Amount { get; }

    public ButtonClickEvent(string key ,int amount = 1)
    {
        Debug.Log($"{key}.clicked");
        Key = $"{key}.clicked";
        Amount = amount;
    }
}