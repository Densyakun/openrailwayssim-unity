using UnityEngine;

/// <summary>
/// マップを削除するときに表示される確認画面
/// </summary>
public class DeleteMapPanel : GamePanel
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }

    public void OKButton()
    {
        Main.INSTANCE.selectMapPanel.Delete();
        show(false);
    }
}
