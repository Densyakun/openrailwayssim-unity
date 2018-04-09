using UnityEngine;
using UnityEngine.UI;

public class TrackInfoPanel : GamePanel {
	//TODO 多言語対応化
	public static string lengthText_DEF = "長さ";
	public Text lengthText;

	void Update () {
		lengthText.text = lengthText_DEF + ": " + Main.editingTrack.length + "m ";
	}
}
