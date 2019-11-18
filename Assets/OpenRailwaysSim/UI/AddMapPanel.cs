using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップを追加する画面
/// </summary>
public class AddMapPanel : GamePanel
{

    public InputField mapnameInput;

    void OnEnable()
    {
        mapnameInput.text = MapManager.getRandomMapName();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void AddMap()
    {
        string mapname = mapnameInput.text.Trim();

        if (mapname.Length == 0)
        {
            // TODO 警告ダイアログ
        }
        else
        {
            var a = MapManager.getMapList();
            for (var b = 0; b < a.Length; b++)
                if (a[b].ToLower().Equals(mapname.ToLower()))
                {
                    // TODO ダイアログ
                    return;
                }

            MapManager.saveMap(new Map(mapname));
            show(false);
            Main.INSTANCE.selectMapPanel.show(false);
            Main.INSTANCE.StartCoroutine(Main.INSTANCE.openMap(mapname));
        }
    }
}
