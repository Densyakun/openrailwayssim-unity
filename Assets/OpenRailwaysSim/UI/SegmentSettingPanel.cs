using UnityEngine.UI;

/// <summary>
/// 線形の線分を設定する画面
/// </summary>
public class SegmentSettingPanel : GamePanel
{

    public static string segmentText_DEF = "平面曲線セグメント";
    public static string verticalSegmentText_DEF = "縦断曲線セグメント";
    public static string lengthText_DEF = "長さ";
    public static string radiusText_DEF = "半径";

    public Text titleText;
    public Text lengthText;
    public InputField lengthInput;
    public Text radiusText;
    public InputField radiusInput;

    public int n = -1;
    public bool isVertical = false;

    public void addButton()
    {
        if (isVertical)
            GameCanvas.shapeSettingPanel.addVerticalSegment(n);
        else
            GameCanvas.shapeSettingPanel.addSegment(n);
    }

    public void removeButton()
    {
        if (isVertical)
            GameCanvas.shapeSettingPanel.removeVerticalSegment(n);
        else
            GameCanvas.shapeSettingPanel.removeSegment(n);
    }
}
