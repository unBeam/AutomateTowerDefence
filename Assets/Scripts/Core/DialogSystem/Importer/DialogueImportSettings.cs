using UnityEngine;

[CreateAssetMenu(fileName = "DialogueImportSettings", menuName = "Dialogues/Import Settings")]
public class DialogueImportSettings : ScriptableObject
{
    public string SheetId;
    public string GidDialogues;
    public string GidScriptSettings;
    public string GidCharacters;
    public string GidGifts;
    public string GidRelationships;
}