using System;
using UnityEngine.UI;

/// <summary>
/// 線形の縦断曲線を設定する画面
/// </summary>
public class VerticalSegmentSettingPanel : GamePanel
{

    public static string verticalSegmentText_DEF = "縦断曲線セグメント";
    public static string lengthText_DEF = "長さ";
    public static string radiusText_DEF = "半径";

    public Text titleText;
    public Text lengthText;
    public InputField lengthInput;
    public Text radiusText;
    public InputField radiusInput;

    [NonSerialized]
    public int n = -1;

    public void addButton()
    {
        Main.INSTANCE.shapeSettingPanel.addVerticalSegment(n);
    }

    public void removeButton()
    {
        Main.INSTANCE.shapeSettingPanel.removeVerticalSegment(n);
    }
}
