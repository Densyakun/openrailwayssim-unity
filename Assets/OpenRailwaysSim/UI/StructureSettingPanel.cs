using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ストラクチャの設定画面
/// </summary>
public class StructureSettingPanel : GamePanel
{

    public static string pathText_DEF = "パス";

    // settings by Inspector
    public Text pathText;
    public InputField pathInput;

    private string lastPath;

    void Update()
    {
        reflect();
    }

    public void load()
    {
        pathInput.text = lastPath = Main.editingStructure.path;
    }

    new public void show(bool show)
    {
        if (show)
        {
            pathText.text = pathText_DEF + ": ";

            load();
        }

        base.show(show);
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        Main.editingStructure.path = pathInput.text;
        Main.editingStructure.importFile();
        Main.editingStructure.reloadEntity();
    }

    public void save()
    {
        reflect();
        Main.playingmap.addObject(Main.editingStructure);
        Main.editingStructure = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        pathInput.text = lastPath;
        reflect();

        show(false);
    }
}
