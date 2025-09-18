using DefaultNamespace.Quests;

public class DropBoardEvent : IQuestEvent
{
    public string Key => "board.drop";
    public int Amount { get; }

    public DropBoardEvent(int amount = 1)
    {
        Amount = amount;
    }
}
