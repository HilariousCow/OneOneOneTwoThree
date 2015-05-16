using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GameSpawner : Singleton<GameSpawner>, IPointerClickHandler
{
    //point to settings

    public SliderBox SliderBoxPrefab;
    private SliderBox _sliderBoxWhite;
    private SliderBox _sliderBoxBlack;

    public MatchSettingsSO[] AllMatchStyles;
    public AIPlayer[] AllAIStyles;

    private MatchSettingsSO _chosenMatchStyle;

    private AIPlayer _whitePlayer;
    private AIPlayer _blackPlayer;

    public MainGame MainGamePrefab;
    private MainGame _mainGameInstance;


    public void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        SpawnInterface();

    }

    private void SpawnInterface()
    {
     


        _sliderBoxWhite = transform.InstantiateChild(SliderBoxPrefab);
        _sliderBoxWhite.transform.localPosition = Vector3.forward * 10f;
        _sliderBoxWhite.Init(new List<AIPlayer>(AllAIStyles));





        _sliderBoxBlack = transform.InstantiateChild(SliderBoxPrefab);
        _sliderBoxBlack.transform.localPosition = Vector3.forward * -10f;
        _sliderBoxBlack.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);
        _sliderBoxBlack.Init(new List<AIPlayer>(AllAIStyles));


        //got to make a "go" button.
    }


    public void SpawnGame()
    {

        _whitePlayer = _sliderBoxWhite.GetCurrentlySelectedObject().AiPlayerPrefab;
        _blackPlayer = _sliderBoxBlack.GetCurrentlySelectedObject().AiPlayerPrefab;
        
        _chosenMatchStyle = AllMatchStyles[0];//todo: slider for mode.

        _mainGameInstance = (Instantiate(MainGamePrefab.gameObject) as GameObject).GetComponent<MainGame>();

        _mainGameInstance.Init(_chosenMatchStyle, _whitePlayer, _blackPlayer);

        
    }


    

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        SpawnGame();
    }

    public void TurnOn()
    {
        gameObject.SetActive(true);
    }
}
