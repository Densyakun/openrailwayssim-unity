using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップを選択する画面
/// </summary>
public class SelectMapPanel : GamePanel, ScrollController.Listener
{

    public static string selectedMap; // 最後に選択されたマップ
    private static bool openMap = false;
    public Button addMapButton;
    public Button selectMapButton;
    public Button deleteMapButton;
    public Text selectButtonText;
    public Text mapNameText;
    public Text createdText;
    public Text updatedText;
    public ScrollController sc;
    string[] mapList = new string[0];

    void OnEnable()
    {
        sc.listeners.Add(this);
        reloadContents();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            !Main.INSTANCE.addMapPanel.gameObject.activeInHierarchy &&
            !Main.INSTANCE.unsupportedMapPanel.gameObject.activeInHierarchy &&
            !Main.INSTANCE.deleteMapPanel.gameObject.activeInHierarchy)
        {
            show(false);
        }
    }

    new public void show(bool show)
    {
        base.show(show);
        if (!show)
            resetSetting();
    }

    public void show(bool show, string selectText)
    {
        this.show(show);
        selectButtonText.text = selectText;
    }

    public void show(bool show, string selectText, bool addable)
    {
        this.show(show);
        selectButtonText.text = selectText;
        setMapAddable(addable);
    }

    private static void resetSetting()
    {
        openMap = false;
    }

    public void setOpenMap()
    {
        openMap = true;
    }

    void a()
    {
        bool interactable = sc.n != -1;
        deleteMapButton.interactable = selectMapButton.interactable = interactable;
        mapNameText.text = interactable ? mapList[sc.n] : "";
        var info = interactable ? MapManager.loadMapInfo(mapList[sc.n]) : null;
        createdText.text = interactable ? "Created: " + info.created.ToString() : "";
        updatedText.text = interactable ? "Updated: " + info.updated.ToString() : "";
    }

    public void reloadContents()
    {
        a();
        mapList = MapManager.getMapList();
        sc.setContents(mapList);
    }

    public void setMapAddable(bool addable)
    {
        addMapButton.interactable = addable;
    }

    void ScrollController.Listener.Select(ScrollController sc)
    {
        a();
    }

    public void OKButton()
    {
        selectedMap = sc.n == -1 ? null : mapList[sc.n];
        sc.n = -1;
        a();

        if (openMap)
        {
            show(false);
            Main.INSTANCE.StartCoroutine(Main.INSTANCE.openMap(SelectMapPanel.selectedMap));
        }
    }

    public void DeleteButton()
    {
        Main.INSTANCE.deleteMapPanel.show(true);
    }

    public void Delete()
    {
        selectedMap = null;
        if (MapManager.deleteMap(mapList[sc.n]))
        {
            sc.n = -1;
            reloadContents();
        }
    }
}
