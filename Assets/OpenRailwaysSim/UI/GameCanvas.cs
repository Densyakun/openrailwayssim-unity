using UnityEngine;

//GamePanelを管理するスクリプト
public class GameCanvas : MonoBehaviour
{
    public static Canvas canvas;
    public static TitlePanel titlePanel;
    public static SelectMapPanel selectMapPanel;
    public static SettingPanel settingPanel;
    public static AddMapPanel addMapPanel;
    public static LoadingMapPanel loadingMapPanel;
    public static PlayingPanel playingPanel;
    public static PausePanel pausePanel;
    public static TitleBackPanel titleBackPanel;
    public static UnsupportedMapPanel unsupportedMapPanel;
    public static DeleteMapPanel deleteMapPanel;
    public static ShapeSettingPanel shapeSettingPanel;
    public static CouplerSettingPanel couplerSettingPanel;
    public static RunPanel runPanel;
    public static MapPinSettingPanel mapPinSettingPanel;
    public static StructureSettingPanel structureSettingPanel;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        titlePanel = GetComponentInChildren<TitlePanel>(true);
        selectMapPanel = GetComponentInChildren<SelectMapPanel>(true);
        settingPanel = GetComponentInChildren<SettingPanel>(true);
        addMapPanel = GetComponentInChildren<AddMapPanel>(true);
        loadingMapPanel = GetComponentInChildren<LoadingMapPanel>(true);
        playingPanel = GetComponentInChildren<PlayingPanel>(true);
        pausePanel = GetComponentInChildren<PausePanel>(true);
        titleBackPanel = GetComponentInChildren<TitleBackPanel>(true);
        unsupportedMapPanel = GetComponentInChildren<UnsupportedMapPanel>(true);
        deleteMapPanel = GetComponentInChildren<DeleteMapPanel>(true);
        shapeSettingPanel = GetComponentInChildren<ShapeSettingPanel>(true);
        couplerSettingPanel = GetComponentInChildren<CouplerSettingPanel>(true);
        runPanel = GetComponentInChildren<RunPanel>(true);
        mapPinSettingPanel = GetComponentInChildren<MapPinSettingPanel>(true);
        structureSettingPanel = GetComponentInChildren<StructureSettingPanel>(true);
    }
}
