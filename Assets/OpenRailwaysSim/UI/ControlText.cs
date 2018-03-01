using UnityEngine;
using UnityEngine.UI;

public class ControlText : MonoBehaviour {
	public RectTransform bgImage;

	void Update () {
		string text = "";
		if (Main.playingmap != null && !GameCanvas.pausePanel.isShowing ()) {
			//text += "E: メニューを開く ";
		}

		text = text.Trim ();

		bgImage.gameObject.SetActive (text != "");
		GetComponent<Text> ().text = text;
	}
}
