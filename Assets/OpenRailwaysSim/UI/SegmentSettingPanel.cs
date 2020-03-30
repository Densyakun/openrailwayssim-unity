using System;
using UnityEngine.UI;

/// <summary>
/// 線形の平面曲線を設定する画面
/// </summary>
public class SegmentSettingPanel : GamePanel
{

    public static string segmentText_DEF = "平面曲線セグメント";
    public static string lengthText_DEF = "長さ";
    public static string radiusText_DEF = "半径";
    public static string cantText_DEF = "カント";
    public static string cantRotationText_DEF = "軌道中心を基準に傾ける";

    public Text titleText;
    public Text lengthText;
    public InputField lengthInput;
    public Text radiusText;
    public InputField radiusInput;
    public Text cantText;
    public InputField cantInput;
    public Text cantRotationText;
    public Toggle cantRotationToggle;

    [NonSerialized]
    public int n = -1;

    public void addButton()
    {
        Main.INSTANCE.shapeSettingPanel.addSegment(n);
    }

    public void removeButton()
    {
        Main.INSTANCE.shapeSettingPanel.removeSegment(n);
    }
}
