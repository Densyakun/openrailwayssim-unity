using UnityEngine;

/// <summary>
/// 対応していないマップを開いたときに表示される画面
/// </summary>
public class UnsupportedMapPanel : GamePanel
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            show(false);
    }
}
