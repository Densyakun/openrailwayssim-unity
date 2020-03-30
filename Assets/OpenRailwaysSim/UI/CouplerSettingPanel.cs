using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 連結器の設定画面
/// </summary>
public class CouplerSettingPanel : GamePanel
{

    public static string isFrontText_DEF = "前面";
    public Text isFrontText;
    public Toggle isFrontToggle;

    private bool lastIsFront;

    [NonSerialized]
    public bool isNew;

    void Update()
    {
        reflect();
    }

    public void load()
    {
        isFrontToggle.isOn = (lastIsFront = Main.editingCoupler.isFront);
    }

    new public void show(bool show)
    {
        if (show)
        {
            isFrontText.text = isFrontText_DEF + ": ";

            load();
        }

        base.show(show);
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        Main.editingCoupler.isFront = isFrontToggle.isOn;
        Main.editingCoupler.reloadEntity();
    }

    public void save()
    {
        reflect();
        if (isNew)
            Main.playingmap.addObject(Main.editingCoupler);
        Main.editingCoupler = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        if (isNew)
            Main.editingCoupler.entity.Destroy();
        else
        {
            isFrontToggle.isOn = lastIsFront;
            reflect();
        }

        show(false);
        Main.editingCoupler = null;
    }

    public void onEndEdit()
    {
        save();
    }
}
