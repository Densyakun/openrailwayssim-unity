using UnityEngine;
using UnityEngine.UI;

//ゲーム中に表示される画面
public class PlayingPanel : GamePanel
{

    public Button removeButton;

    public void ConstTrackButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_CONSTRUCT_TRACK ? 0 : Main.MODE_CONSTRUCT_TRACK;
    }

    public void PlaceAxleButton()
    {
        Main.main.mode = Main.main.mode == Main.MODE_PLACE_AXLE ? 0 : Main.MODE_PLACE_AXLE;
    }

    public void RemoveButton()
    {
        Main.removeSelectingObjs();
    }

    public void PlaceBogieFrameButton()
    {
    }
}
