using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 操作方法を表示するスクリプト
/// </summary>
public class ControlText : MonoBehaviour
{

    public RectTransform bgImage;

    void Update()
    {
        string text = "";
        if (Main.playingmap != null && !Main.INSTANCE.pausePanel.isShowing())
        {
            //text += "E: メニューを開く ";
        }

        text = text.Trim();

        bgImage.gameObject.SetActive(text != "");
        GetComponent<Text>().text = text;
    }
}
