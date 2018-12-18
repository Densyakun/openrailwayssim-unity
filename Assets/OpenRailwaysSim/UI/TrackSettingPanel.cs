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
    public static string repeatText_DEF = "繰り返す";
    public static string isVerticalCurveText_DEF = "縦曲線";
    public Text lengthText;
    public InputField lengthInput;
    public Text repeatText;
    public InputField repeatInput;
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
            InputField input = EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() : null;
            EventSystem.current.SetSelectedGameObject(((input != null && input.isFocused) ? input == lengthInput ? repeatInput : input == repeatInput ? Main.editingTracks[0] is Curve ? radiusInput : lengthInput : lengthInput : lengthInput).gameObject);
        }
    }

    public void load()
    {
        lengthInput.text = (lastLength = Main.editingTracks[0].length).ToString();
        repeatInput.text = "1";
        if (Main.editingTracks[0] is Curve)
        {
            radiusInput.text = (lastRadius = ((Curve)Main.editingTracks[0]).radius).ToString();
            isVerticalCurveToggle.isOn = lastIsVerticalCurve = ((Curve)Main.editingTracks[0]).isVerticalCurve;
        }
    }

    new public void show(bool show)
    {
        if (show)
        {
            lengthText.text = lengthText_DEF + ": ";
            repeatText.text = repeatText_DEF + ": ";
            if (Main.editingTracks[0] is Curve)
            {
                radiusText.text = radiusText_DEF + ": ";
                isVerticalCurveText.text = isVerticalCurveText_DEF + ": ";
            }

            load();

            if (Main.editingTracks[0] is Curve)
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
            Main.editingTracks[0].length = float.Parse(lengthInput.text);
            if (Main.editingTracks[0] is Curve)
            {
                ((Curve)Main.editingTracks[0]).radius = float.Parse(radiusInput.text);
                ((Curve)Main.editingTracks[0]).isVerticalCurve = isVerticalCurveToggle.isOn;
            }
            Main.main.setTrackRepeat(int.Parse(repeatInput.text));
        }
        catch (FormatException) { }
        catch (OverflowException) { }
    }

    public void save()
    {
        reflect();
        Main.main.trackEdited0();
        Main.editingTracks.Clear();
        Main.editingRot = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        lengthInput.text = lastLength.ToString();
        repeatInput.text = "1";
        radiusInput.text = lastRadius.ToString();
        isVerticalCurveToggle.isOn = lastIsVerticalCurve;
        reflect();

        show(false);
    }
}
