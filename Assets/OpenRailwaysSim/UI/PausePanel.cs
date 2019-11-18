/// <summary>
/// ポーズしているときに表示される画面
/// </summary>
public class PausePanel : GamePanel
{

    public void ResumeButton()
    {
        Main.INSTANCE.setPause(false);
    }

    public void SaveButton()
    {
        MapManager.saveMap(Main.playingmap);
    }
}
