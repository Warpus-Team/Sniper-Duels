using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardView : View
{
    [SerializeField] private Transform scoreBoardEntriesParent;
    [SerializeField] private ScoreBoardEntry scoreBoardEntryPrefab;

    private GameViewManager _gameViewManager;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void Start()
    {
        _gameViewManager = InstanceHandler.GetInstance<GameViewManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _gameViewManager.ShowView<ScoreBoardView>(false);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            _gameViewManager.HideView<ScoreBoardView>();
        }
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ScoreBoardView>();
    }

    public void SetData(Dictionary<PlayerID, ScoreManager.ScoreData> data) 
    {
        foreach (Transform child in scoreBoardEntriesParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var playerScore in data)
        {
            var entry = Instantiate(scoreBoardEntryPrefab, scoreBoardEntriesParent);
            entry.SetData(playerScore.Key.id.ToString(), playerScore.Value.Kills, playerScore.Value.Deaths);
        }
    }

    public override void OnShow()
    {
    }
    public override void OnHide()
    {
    }
}
   