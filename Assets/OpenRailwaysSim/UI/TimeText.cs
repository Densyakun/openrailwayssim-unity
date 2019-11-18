using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム内の時間を表示するスクリプト。TextコンポーネントのあるGameObjectに追加して使用する
/// </summary>
public class TimeText : MonoBehaviour
{

    Text text;

    void OnEnable()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (Main.playingmap == null)
            text.text = "---";
        else
        {
            text.text = "現在時刻: " + (Main.playingmap.getDays() + 1) + "日目 " +
            Main.playingmap.getHours() + ":" + string.Format("{0:00}", Main.playingmap.getMinutes()) +
            ":" + string.Format("{0:00}", Main.playingmap.getSeconds());
        }
    }
}
