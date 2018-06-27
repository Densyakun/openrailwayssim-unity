using UnityEngine;

//MapObjectの実体
public class MapEntity : MonoBehaviour {
	public virtual MapObject obj { get; protected set; }

	void Start () {
		obj.reloadEntity ();
	}

	void Update()
	{
		if (!Main.main.pause && obj is Bogie)
			obj.reloadEntity();
	}

	void FixedUpdate()
	{
		if (!Main.main.pause && obj is Bogie)
			((Bogie) obj).fixedUpdate();
	}

	//Startメソッドが実行される前に、MapObjectを設定する
	public virtual void init (MapObject obj) {
		this.obj = obj;
	}

	public virtual void Destroy () {
		obj.SyncFromEntity ();
		Destroy (gameObject);
	}
}
