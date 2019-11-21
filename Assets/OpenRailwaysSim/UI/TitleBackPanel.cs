using UnityEngine;

/// <summary>
/// タイトルに戻るときに表示される確認画面
/// </summary>
public class TitleBackPanel : GamePanel
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void OKButton()
    {
        show(false);
        Main.INSTANCE.closeMap();
    }
}
