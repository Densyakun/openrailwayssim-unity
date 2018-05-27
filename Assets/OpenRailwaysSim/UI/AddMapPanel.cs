using UnityEngine;
using UnityEngine.UI;

//マップを追加する画面
public class AddMapPanel : GamePanel {
	public InputField mapnameInput;
    
	void OnEnable () {
		mapnameInput.text = MapManager.getRandomMapName ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			show (false);
		}
	}

	public void AddMap () {
		string mapname = mapnameInput.text.Trim ();

		if (mapname.Length == 0) {
			//TODO 警告ダイアログ
		} else {
			string[] a = MapManager.getMapList ();
			for (int b = 0; b < a.Length; b++) {
				if (a [b].ToLower ().Equals (mapname.ToLower ())) {
					//TODO ダイアログ
					return;
				}
			}

			MapManager.saveMap (new Map (mapname));
			show (false);
			GameCanvas.selectMapPanel.show (false);
			Main.main.StartCoroutine (Main.openMap (mapname));
		}
	}
}
