using System;
using UnityEngine;
using Zenject;

public class TestQuestStarter : MonoBehaviour
{
    [Inject] private QuestManager _questManager;

    private void Start()
    {
        _questManager.StartQuestById("test");
    }
}
