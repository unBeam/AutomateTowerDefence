using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dialogues.Configs;
using Dialogues.Domain;

namespace Dialogues.Runtime
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
    }

    public interface IDialogueRepository
    {
        DialogueNodeAsset GetNode(DialogueNodeKey key);
        DialogueNodeAsset GetNode(string scriptKey, string nodeId);
        DialogueNodeAsset GetStartNode(string scriptKey);
        IEnumerable<DialogueNodeAsset> GetScriptNodes(string scriptKey);
    }

    public interface IScriptSettingsRepository
    {
        ScriptSettingEntry Get(string scriptKey);
    }

    public interface IRelationshipService
    {
        int GetLevel(string characterName);
        int GetRP(string characterName);
        float GetProgress01(string characterName);
        void AddRP(string characterName, int amount);
        IObservableRelationshipStream Stream { get; }
    }
    
    public interface IObservableRelationshipStream
    {
        IObservable<RelationshipChangedEvent> Changed { get; }
        IObservable<RelationshipLevelUpEvent> LevelUp { get; }
    }
    
    public struct RelationshipChangedEvent
    {
        public string CharacterName;
        public int Level;
        public int RP;
    }

    public struct RelationshipLevelUpEvent
    {
        public string CharacterName;
        public int NewLevel;
    }

    public interface IGiftService
    {
        bool CanGive(string giftId, string characterName, string characterType);
        bool Give(string giftId, string characterName, string characterType);
    }

    public interface IMoodService
    {
        string GetMood(string characterName);
        void SetMood(string characterName, string mood);
    }

    public interface ICallbackDispatcher
    {
        UniTask DispatchAsync(string callback);
    }
}