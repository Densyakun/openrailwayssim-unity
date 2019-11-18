using UnityEngine;

/// <summary>
/// ScrollControllerで管理する要素
/// </summary>
public class ScrollItem : MonoBehaviour
{

    public int n = -1;
    public ScrollController sc;

    public void click()
    {
        sc.select(n);
    }
}
