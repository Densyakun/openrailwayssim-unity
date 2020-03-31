using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 時間設定画面
/// </summary>
public class TimeSettingPanel : GamePanel
{

    // settings by Inspector
    public Text timeText;
    public InputField timeInput;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void load()
    {
        timeInput.text = Main.INSTANCE.getTimeText();
    }

    new public void show(bool show)
    {
        if (show)
            load();

        base.show(show);
    }

    public void set()
    {
        try
        {
            var s = timeInput.text.Split(':');
            Main.playingmap.time = (float.Parse(s[0]) * 60f + float.Parse(s[1])) * 60f + float.Parse(s[2]);
            Main.INSTANCE.reloadLighting();

            show(false);
        }
        catch { }
    }
}
