using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// マップピンの設定画面
/// </summary>
public class MapPinSettingPanel : GamePanel
{

    public static string titleText_DEF = "タイトル";
    public static string descriptionText_DEF = "説明";
    public Text titleText;
    public InputField titleInput;
    public Text descriptionText;
    public InputField descriptionInput;

    private string lastTitle;
    private string lastDescription;

    [NonSerialized]
    public bool isNew;

    void Update()
    {
        reflect();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputField input = null;
            if (EventSystem.current.currentSelectedGameObject != null)
                input = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (input != null && input.isFocused)
            {
                if (input == titleInput)
                    EventSystem.current.SetSelectedGameObject(descriptionInput.gameObject);
                else
                    EventSystem.current.SetSelectedGameObject(titleInput.gameObject);
            }
            else
                EventSystem.current.SetSelectedGameObject(titleInput.gameObject);
        }
    }

    public void load()
    {
        titleInput.text = lastTitle = Main.editingMapPin.title;
        descriptionInput.text = lastDescription = Main.editingMapPin.description;
    }

    new public void show(bool show)
    {
        if (show)
        {
            titleText.text = titleText_DEF + ": ";
            descriptionText.text = descriptionText_DEF + ": ";

            load();
        }

        base.show(show);
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        Main.editingMapPin.title = titleInput.text;
        Main.editingMapPin.description = descriptionInput.text;
        Main.editingMapPin.reloadEntity();
    }

    public void save()
    {
        reflect();
        if (isNew)
            Main.playingmap.addObject(Main.editingMapPin);
        Main.editingMapPin = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        if (isNew)
            Main.editingMapPin.entity.Destroy();
        else
        {
            titleInput.text = lastTitle;
            descriptionInput.text = lastDescription;
            reflect();
        }

        show(false);
        Main.editingMapPin = null;
    }
}
