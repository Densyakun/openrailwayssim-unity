using UnityEngine;
using UnityEngine.UI;

public class MapPinEntity : MonoBehaviour
{

    public MapPin mapPin;
    public Text text;

    void Start()
    {
    }

    void Update()
    {
        if (!GameCanvas.runPanel.isShowing() && Main.main.showGuide)
        {
            if (text == null)
            {
                text = gameObject.AddComponent<Text>();
                text.font = Main.main.mapPinFont;
                text.rectTransform.sizeDelta = new Vector2(160f, 20f);
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 1;
                text.resizeTextMaxSize = text.fontSize;
                text.color = new Color(1f, 1f, 1f, 3f / 4f);
            }
            text.text = mapPin.title;

            var p = Camera.main.WorldToViewportPoint(mapPin.pos);
            transform.position = new Vector3(Screen.width * p.x, Screen.height * p.y);
        }
        else if (text != null)
        {
            GameObject.Destroy(text);
            text = null;
        }
    }
}
