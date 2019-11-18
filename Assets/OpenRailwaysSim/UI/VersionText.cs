using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームのバージョンを表示するスクリプト
/// </summary>
public class VersionText : MonoBehaviour
{

    void OnEnable()
    {
        GetComponent<Text>().text = "Ver: " + Main.VERSION;
    }
}
