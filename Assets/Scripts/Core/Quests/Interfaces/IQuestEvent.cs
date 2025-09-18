namespace DefaultNamespace.Quests
{
    public interface IQuestEvent
    {
        string Key { get; } 
        int Amount { get; }
    }
}