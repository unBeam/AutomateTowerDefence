using DefaultNamespace.Quests;
using UnityEngine;

public class ThiefCaughtEvent: IQuestEvent
{
    public string Key => "thief.caught";
    public int Amount { get; }

    public ThiefCaughtEvent(int amount = 1)
    {
        Amount = amount;
    }
}