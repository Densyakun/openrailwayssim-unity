using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 車体の設定画面
/// </summary>
public class BodySettingPanel : GamePanel
{

    public static string frontCabText_DEF = "前方の運転台";
    public static string backCabText_DEF = "後方の運転台";
    public Text frontCabText;
    public Toggle frontCabToggle;
    public Text backCabText;
    public Toggle backCabToggle;

    private bool lastFrontCab;
    private bool lastBackCab;

    [NonSerialized]
    public bool isNew;

    void Update()
    {
        reflect();
    }

    public void load()
    {
        frontCabToggle.isOn = (lastFrontCab = Main.editingBody.frontCab != null);
        backCabToggle.isOn = (lastBackCab = Main.editingBody.backCab != null);
    }

    new public void show(bool show)
    {
        if (show)
        {
            frontCabText.text = frontCabText_DEF + ": ";
            backCabText.text = backCabText_DEF + ": ";

            load();
        }

        base.show(show);
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        Main.editingBody.frontCab = frontCabToggle.isOn ? new Cab(Main.editingBody, true) : null;
        Main.editingBody.backCab = backCabToggle.isOn ? new Cab(Main.editingBody, false) : null;
        Main.editingBody.reloadEntity();
        Main.INSTANCE.playingPanel.reloadInteractable();
    }

    public void save()
    {
        reflect();
        if (isNew)
            Main.playingmap.addObject(Main.editingBody);
        Main.editingBody = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        if (isNew)
            Main.editingBody.entity.Destroy();
        else
        {
            frontCabToggle.isOn = lastFrontCab;
            backCabToggle.isOn = lastBackCab;
            reflect();
        }

        show(false);
        Main.editingBody = null;
    }
}
