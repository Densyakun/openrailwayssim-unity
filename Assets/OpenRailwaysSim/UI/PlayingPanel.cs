using UnityEngine;
using UnityEngine.UI;

//ゲーム中に表示される画面
public class PlayingPanel : GamePanel
{
    public void ConstTrackButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_CONSTRUCT_TRACK ? 0 : Main.MODE_CONSTRUCT_TRACK;
    }

    public void RemoveTrackButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_REMOVE_TRACK ? 0 : Main.MODE_REMOVE_TRACK;
    }
    
    public void PlaceCarButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_PLACE_CAR ? 0 : Main.MODE_PLACE_CAR;
    }
}
