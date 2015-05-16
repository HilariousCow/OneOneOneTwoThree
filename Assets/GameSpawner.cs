using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GameSpawner : MonoBehaviour, IPointerClickHandler
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
        SpawnInterface();
    }

    private void SpawnInterface()
    {
     


        _sliderBoxWhite = transform.InstantiateChild(SliderBoxPrefab);
        _sliderBoxWhite.transform.localPosition = Vector3.forward * 10f;

        _sliderBoxBlack = transform.InstantiateChild(SliderBoxPrefab);
        _sliderBoxBlack.transform.localPosition = Vector3.forward * -10f;
        _sliderBoxBlack.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);


        _sliderBoxWhite.Init(new List<AIPlayer>(AllAIStyles));
        _sliderBoxBlack.Init(new List<AIPlayer>(AllAIStyles));


        //got to make a "go" button.
    }


    public void SpawnGame()
    {
        _mainGameInstance = (Instantiate(MainGamePrefab.gameObject) as GameObject).GetComponent<MainGame>();

        _mainGameInstance.Init(_chosenMatchStyle, _whitePlayer, _blackPlayer);

    }


    

    public void OnPointerClick(PointerEventData eventData)
    {
        SpawnGame();
    }


}
