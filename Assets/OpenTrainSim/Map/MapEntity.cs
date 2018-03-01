using UnityEngine;

public class MapEntity : MonoBehaviour {
	public virtual MapObject obj { get; protected set; }
	bool initialized = false;

	void Start () {
		obj.reloadEntity ();
	}

	public virtual void init (MapObject obj) {
		this.obj = obj;
		initialized = true;
	}

	public virtual void Destroy () {
		obj.SyncFromEntity ();
		Destroy (gameObject);
	}
}
