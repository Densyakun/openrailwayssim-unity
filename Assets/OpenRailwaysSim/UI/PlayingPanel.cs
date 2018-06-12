using UnityEngine;
using UnityEngine.UI;

//ゲーム中に表示される画面
public class PlayingPanel : GamePanel
{
    public Button constTrackButton;
    public Button removeTrackButton;

    public void ConstTrackButton()
    {
        if (Main.mode == Main.MODE_CONSTRUCT_TRACK)
            Main.mode = 0;
        else
            Main.mode = Main.MODE_CONSTRUCT_TRACK;
    }

    public void RemoveTrackButton()
    {
        if (Main.mode == Main.MODE_REMOVE_TRACK)
            Main.mode = 0;
        else
            Main.mode = Main.MODE_REMOVE_TRACK;
    }
}
