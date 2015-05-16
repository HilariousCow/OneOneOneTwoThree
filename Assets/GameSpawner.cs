using UnityEngine;
using System.Collections;

public class GameSpawner : MonoBehaviour
{
    //point to settings

    public MatchSettingsSO[] AllMatchStyles;
    public AIPlayer[] AllAIStyles;

    private MatchSettingsSO _chosenMatchStyle;

    private AIPlayer _whitePlayer;
    private AIPlayer _blackPlayer;

    public MainGame MainGamePrefab;
    private MainGame _mainGameInstance;

    public void SpawnGame()
    {
        _mainGameInstance = (Instantiate(MainGamePrefab.gameObject) as GameObject).GetComponent<MainGame>();

        _mainGameInstance.Init(_chosenMatchStyle, _whitePlayer, _blackPlayer);
    }
	
}
