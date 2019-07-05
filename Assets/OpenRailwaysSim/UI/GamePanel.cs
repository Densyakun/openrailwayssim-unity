using UnityEngine;

//画面を管理するスクリプト
public class GamePanel : MonoBehaviour
{

    public void show(bool show)
    {
        gameObject.SetActive(show);
    }

    public bool isShowing()
    {
        return gameObject.activeSelf;
    }

    public float getWidth()
    {
        return ((RectTransform)transform).rect.width;
    }

    public float getHeight()
    {
        return ((RectTransform)transform).rect.width;
    }
}
