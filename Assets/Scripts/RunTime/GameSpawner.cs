using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GameSpawner : Singleton<GameSpawner>, IPointerClickHandler
{
    //point to settings

    public SliderBoxAIPlayer AIPlayerSliderBoxPrefab;
    public SliderBoxMatchSetting MatchSettingSliderBoxPrefab;

    private SliderBoxAIPlayer _sliderBoxWhite;
    private SliderBoxAIPlayer _sliderBoxBlack;
    private SliderBoxMatchSetting _sliderMatchSettings;

    public MatchSettingsSO[] AllMatchStyles;
    public AIPlayer[] AllAIStyles;

    private MatchSettingsSO _chosenMatchStyle;

    private AIPlayer _whitePlayer;
    private AIPlayer _blackPlayer;

    public MainGame MainGamePrefab;
    private MainGame _mainGameInstance;


    public override void Awake()
    {
      
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        SpawnInterface();
        base.Awake();

    }

 
    private void SpawnInterface()
    {

        if (_sliderBoxWhite == null)
        {

            _sliderBoxWhite = transform.InstantiateChild(AIPlayerSliderBoxPrefab);
            _sliderBoxWhite.transform.localPosition = Vector3.forward*10f;
            _sliderBoxWhite.Init(new List<AIPlayer>(AllAIStyles));

            _sliderBoxBlack = transform.InstantiateChild(AIPlayerSliderBoxPrefab);
            _sliderBoxBlack.transform.localPosition = Vector3.forward*-10f;
            _sliderBoxBlack.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);
            _sliderBoxBlack.Init(new List<AIPlayer>(AllAIStyles));

            _sliderMatchSettings = transform.InstantiateChild(MatchSettingSliderBoxPrefab);
            _sliderMatchSettings.Init(new List<MatchSettingsSO>(AllMatchStyles));

        }
        //got to make a "go" button.
    }


    public void SpawnGame()
    {

        _whitePlayer = _sliderBoxWhite.GetCurrentlySelectedObject().SelectedItem;
        _blackPlayer = _sliderBoxBlack.GetCurrentlySelectedObject().SelectedItem;

        _chosenMatchStyle = _sliderMatchSettings.GetCurrentlySelectedObject().SelectedItem;//todo: slider for mode.

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
