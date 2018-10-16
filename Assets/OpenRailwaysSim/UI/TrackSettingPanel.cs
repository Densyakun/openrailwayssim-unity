using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//軌道の設定画面
public class TrackSettingPanel : GamePanel
{
    //TODO 多言語対応化
    public static string lengthText_DEF = "長さ";
    public static string radiusText_DEF = "半径";
    public static string isVerticalCurveText_DEF = "縦曲線";
    public Text lengthText;
    public InputField lengthInput;
    public GameObject curveSettingPanel;
    public Text radiusText;
    public InputField radiusInput;
    public Text isVerticalCurveText;
    public Toggle isVerticalCurveToggle;

    private float lastLength;
    private float lastRadius;
    private bool lastIsVerticalCurve;

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
                if (input == lengthInput)
                {
                    if (Main.editingTrack is Curve)
                        EventSystem.current.SetSelectedGameObject(radiusInput.gameObject);
                }
                else
                    EventSystem.current.SetSelectedGameObject(lengthInput.gameObject);
            }
            else
                EventSystem.current.SetSelectedGameObject(lengthInput.gameObject);
        }
    }

    public void load()
    {
        lengthInput.text = (lastLength = Main.editingTrack.length).ToString();
        if (Main.editingTrack is Curve)
        {
            radiusInput.text = (lastRadius = ((Curve)Main.editingTrack).radius).ToString();
            isVerticalCurveToggle.isOn = lastIsVerticalCurve = ((Curve)Main.editingTrack).isVerticalCurve;
        }
    }

    new public void show(bool show)
    {
        if (show)
        {
            lengthText.text = lengthText_DEF + ": ";
            if (Main.editingTrack is Curve)
            {
                radiusText.text = radiusText_DEF + ": ";
                isVerticalCurveText.text = isVerticalCurveText_DEF + ": ";
            }

            load();

            if (Main.editingTrack is Curve)
            {
                ((RectTransform)transform).rect.Set(((RectTransform)transform).rect.x, ((RectTransform)transform).rect.y, ((RectTransform)transform).rect.width, 60);
                curveSettingPanel.gameObject.SetActive(true);
            }
            else
            {
                curveSettingPanel.gameObject.SetActive(false);
                ((RectTransform)transform).rect.Set(((RectTransform)transform).rect.x, ((RectTransform)transform).rect.y, ((RectTransform)transform).rect.width, 30);
            }
        }

        base.show(show);
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        try
        {
            Main.editingTrack.length = float.Parse(lengthInput.text);
            if (Main.editingTrack is Curve)
            {
                ((Curve)Main.editingTrack).radius = float.Parse(radiusInput.text);
                ((Curve)Main.editingTrack).isVerticalCurve = isVerticalCurveToggle.isOn;
            }
            Main.editingTrack.reloadEntity();
        }
        catch (FormatException) { }
        catch (OverflowException) { }
    }

    public void save()
    {
        reflect();
        Main.main.trackEdited0();
        Main.editingTrack = null;
        Main.editingRot = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        lengthInput.text = lastLength.ToString();
        radiusInput.text = lastRadius.ToString();
        isVerticalCurveToggle.isOn = lastIsVerticalCurve;
        reflect();

        show(false);
    }
}
