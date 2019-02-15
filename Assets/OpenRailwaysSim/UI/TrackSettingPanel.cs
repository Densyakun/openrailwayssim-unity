using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//軌道の設定画面
public class TrackSettingPanel : GamePanel
{
    //TODO 多言語対応化
    public static string lengthText_DEF = "長さ";
    public static string gaugeText_DEF = "軌間";
    public static string repeatText_DEF = "繰り返す";
    public static string radiusText_DEF = "半径";
    public static string cantText_DEF = "カント";
    public static string cantRotationText_DEF = "軌道の中心を カントの基準にする";
    public static string isVerticalCurveText_DEF = "縦曲線";
    public Text lengthText;
    public InputField lengthInput;
    public Text gaugeText;
    public InputField gaugeInput;
    public Text repeatText;
    public InputField repeatInput;
    public GameObject curveSettingPanel;
    public Text radiusText;
    public InputField radiusInput;
    public Text cantText;
    public InputField cantInput;
    public Text cantRotationText;
    public Toggle cantRotationToggle;
    public Text isVerticalCurveText;
    public Toggle isVerticalCurveToggle;

    private float lastLength;
    private float lastGauge;
    private float lastRadius;
    private float lastCant;
    private bool lastCantRotation;
    private bool lastIsVerticalCurve;

    void Update()
    {
        reflect();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputField input = EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() : null;
            EventSystem.current.SetSelectedGameObject(((input != null && input.isFocused) ?
            input == lengthInput ? gaugeInput :
            input == gaugeInput ? repeatInput :
            input == repeatInput ? Main.editingTracks[0] is Curve ? radiusInput : lengthInput :
            input == radiusInput ? cantInput :
            lengthInput : lengthInput).gameObject);
        }
    }

    public void load()
    {
        lengthInput.text = (lastLength = Main.editingTracks[0].length).ToString();
        gaugeInput.text = (lastGauge = Main.editingTracks[0].gauge).ToString();
        repeatInput.text = "1";
        if (Main.editingTracks[0] is Curve)
        {
            radiusInput.text = (lastRadius = ((Curve)Main.editingTracks[0]).radius).ToString();
            cantInput.text = (lastCant = ((Curve)Main.editingTracks[0]).cant).ToString();
            cantRotationToggle.isOn = lastCantRotation = ((Curve)Main.editingTracks[0]).cantRotation;
            isVerticalCurveToggle.isOn = lastIsVerticalCurve = ((Curve)Main.editingTracks[0]).isVerticalCurve;
        }
    }

    new public void show(bool show)
    {
        if (show)
        {
            lengthText.text = lengthText_DEF + ": ";
            gaugeText.text = gaugeText_DEF + ": ";
            repeatText.text = repeatText_DEF + ": ";
            if (Main.editingTracks[0] is Curve)
            {
                radiusText.text = radiusText_DEF + ": ";
                cantText.text = cantText_DEF + ": ";
                cantRotationText.text = cantRotationText_DEF + ": ";
                isVerticalCurveText.text = isVerticalCurveText_DEF + ": ";
            }

            load();

            if (Main.editingTracks[0] is Curve)
            {
                ((RectTransform)transform).rect.Set(((RectTransform)transform).rect.x, ((RectTransform)transform).rect.y, ((RectTransform)transform).rect.width, 240);
                curveSettingPanel.gameObject.SetActive(true);
            }
            else
            {
                curveSettingPanel.gameObject.SetActive(false);
                ((RectTransform)transform).rect.Set(((RectTransform)transform).rect.x, ((RectTransform)transform).rect.y, ((RectTransform)transform).rect.width, 120);
            }
        }

        base.show(show);
    }

    new public float getHeight()
    {
        return (curveSettingPanel.activeSelf ? ((RectTransform)curveSettingPanel.transform).rect.height : 0f) + ((RectTransform)transform).rect.height;
    }

    public void reflect()
    {
        if (!isShowing())
            return;

        try
        {
            Main.editingTracks[0].length = float.Parse(lengthInput.text);
            Main.editingTracks[0].gauge = float.Parse(gaugeInput.text);
            if (Main.editingTracks[0] is Curve)
            {
                ((Curve)Main.editingTracks[0]).radius = float.Parse(radiusInput.text);
                ((Curve)Main.editingTracks[0]).cant = float.Parse(cantInput.text);
                ((Curve)Main.editingTracks[0]).cantRotation = cantRotationToggle.isOn;
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
        Main.gauge = Main.editingTracks[0].gauge;
        if (Main.editingTracks[0] is Curve)
        {
            Main.cant = ((Curve)Main.editingTracks[0]).cant;
            Main.cantRotation = ((Curve)Main.editingTracks[0]).cantRotation;
        }
        Main.main.trackEdited0();
        Main.editingTracks.Clear();
        Main.editingRot = null;

        if (!Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void cancel()
    {
        lengthInput.text = lastLength.ToString();
        gaugeInput.text = lastGauge.ToString();
        repeatInput.text = "1";
        radiusInput.text = lastRadius.ToString();
        cantInput.text = lastCant.ToString();
        cantRotationToggle.isOn = lastCantRotation;
        isVerticalCurveToggle.isOn = lastIsVerticalCurve;
        reflect();

        show(false);
    }
}
