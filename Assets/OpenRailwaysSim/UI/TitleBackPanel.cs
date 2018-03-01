using UnityEngine;

public class TitleBackPanel : BPPanel {

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			show (false);
	}

	public void OKButton () {
		//TODO マルチプレイ対応予定

		show (false);
		BPCanvas.pausePanel.show (false);
		Main.closeMap ();
		BPCanvas.titlePanel.show (true);
	}
}
