using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//軌道の設定画面
public class MapPinSettingPanel : GamePanel
{
    //TODO 多言語対応化
    public static string titleText_DEF = "タイトル";
    public static string descriptionText_DEF = "説明";
    public Text titleText;
    public InputField titleInput;
    public Text descriptionText;
    public InputField descriptionInput;

    private string lastTitle;
    private string lastDescription;
    private bool tab = false;

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
                {
                    tab = true;
                    EventSystem.current.SetSelectedGameObject(descriptionInput.gameObject);
                    tab = false;
                }
                else
                {
                    tab = true;
                    EventSystem.current.SetSelectedGameObject(titleInput.gameObject);
                    tab = false;
                }
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
        Main.editingMapPin = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        titleInput.text = lastTitle;
        descriptionInput.text = lastDescription;
        reflect();

        show(false);
    }

    public void onEndEdit()
    {
        if (!tab)
            save();
    }
}
