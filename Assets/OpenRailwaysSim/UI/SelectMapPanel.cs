using UnityEngine;
using UnityEngine.UI;

public class SelectMapPanel : GamePanel, ScrollController.Listener {
	public static string selectedMap; //最後に選択されたマップ
	private static bool openMap = false;
	public Button addMapButton;
	public Button selectMapButton;
	public Button deleteMapButton;
	public Text selectButtonText;
	public Text mapNameText;
	public Image mapImage; //TODO 未使用
	public ScrollController sc;
	string[] mapList = new string[0];

	void OnEnable () {
		sc.listeners.Add (this);
		reloadContents ();
	}

	void Update () {
		//TODO Window(Panel)のフォーカス機能を追加し、一番手前に出ているWindowでのみ操作が機能するようにする。そのためにはCanvasに各WindowやPanelをまとめる。
		if (Input.GetKeyDown (KeyCode.Escape) &&
		    !GameCanvas.addMapPanel.gameObject.activeInHierarchy &&
		    !GameCanvas.unsupportedMapPanel.gameObject.activeInHierarchy &&
		    !GameCanvas.deleteMapPanel.gameObject.activeInHierarchy) {
			show (false);
		}
	}

	new public void show (bool show) {
		base.show (show);
		if (!show) {
			resetSetting ();
		}
	}

	public void show (bool show, string selectText) {
		this.show (show);
		selectButtonText.text = selectText;
	}

	public void show (bool show, string selectText, bool addable) {
		this.show (show);
		selectButtonText.text = selectText;
		setMapAddable (addable);
	}

	private static void resetSetting () {
		openMap = false;
	}

	public void setOpenMap () {
		openMap = true;
	}

	void a () {
		bool interactable = sc.n != -1;
		deleteMapButton.interactable = selectMapButton.interactable = interactable;
		mapNameText.text = interactable ? mapList [sc.n] : "";
	}

	public void reloadContents () {
		a ();
		mapList = MapManager.getMapList ();
		sc.setContents (mapList);
	}

	public void setMapAddable (bool addable) {
		addMapButton.interactable = addable;
	}

	void ScrollController.Listener.Select(ScrollController sc) {
		a ();
	}

	public void OKButton () {
		selectedMap = sc.n == -1 ? null : mapList [sc.n];
		sc.n = -1;
		a ();

		if (openMap) {
			show (false);
			Main.main.StartCoroutine (Main.openMap (SelectMapPanel.selectedMap));
		}
	}

	public void DeleteButton () {
		GameCanvas.deleteMapPanel.show (true);
	}

	public void Delete () {
		selectedMap = null;
		if (MapManager.deleteMap (mapList [sc.n])) {
			sc.n = -1;
			reloadContents ();
		}
	}
}
