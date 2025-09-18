using DefaultNamespace.Quests;

public class DropMoneyEvent : IQuestEvent
{
    public string Key => "drop.money";
    public int Amount { get; }

    public DropMoneyEvent(int amount = 1)
    {
        Amount = amount;
    }
}
