using UnityEngine;

public class PausePanel : BPPanel {

	public void ResumeButton () {
		Main.setPause (false);
	}

	public void SaveButton () {
		MapManager.saveMap (Main.playingmap);
	}
}
