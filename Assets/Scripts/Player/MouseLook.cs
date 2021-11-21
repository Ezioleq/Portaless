using UnityEngine;

public class MouseLook : MonoBehaviour {
	private Controls _controls;

	private Camera _camera;
	private Health health;

	[Header("General")]
	public string mouseYAxis = "Mouse Y";
	public string mouseXAxis = "Mouse X";
	public float mouseYRotation;
	
	[Header("Values")]
	[Range(1, 10)]
	public float mouseSensitivity = 4.5f;
	[Range(0, 90)]
	public float maxYRotation = 90;
	[Range(-90, 0)]
	public float minYRotation = -90;

	[Header("Zoom")]
	public float zoomingFOV = 30f;
	public float zoomSpeed = 10f;
	private float _defaultFOV;
	
	[Header("Locks")]
	public bool lockMouse;
	public bool showMouse;
	
	private void Awake() {
		_controls = new Controls();
	}

	private void Start() {
		_camera = Camera.main;
		_defaultFOV = _camera.fieldOfView;
		health = GetComponent<Health>();
	}

	private void Update() {
		if (!lockMouse && health.IsAlive())
			Mouse();
		
		if (_controls.Player.Zoom.ReadValue<float>() > 0.1f)
			_camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, zoomingFOV, zoomSpeed * Time.deltaTime);
		else
			_camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _defaultFOV, zoomSpeed * Time.deltaTime);

		if (showMouse){
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			lockMouse = true;
		} else {
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			lockMouse = false;
		}
	}

	public void Mouse() {
		// FIXME: Doesn't work well
		float mouseX = _controls.Player.Look.ReadValue<Vector2>().x * mouseSensitivity;

		mouseYRotation += _controls.Player.Look.ReadValue<Vector2>().y * mouseSensitivity;
		mouseYRotation = Mathf.Clamp(mouseYRotation, minYRotation, maxYRotation);

		_camera.transform.localEulerAngles = new Vector3(-mouseYRotation, 0, 0);
		transform.Rotate(0, mouseX, 0);
	}

	private void OnEnable() {
		_controls.Enable();
	}

	private void OnDisable() {
		_controls.Disable();
	}
}
