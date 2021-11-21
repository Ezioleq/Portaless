using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FpsUI : MonoBehaviour {
	private Controls _controls;

	[SerializeField] private TextMeshProUGUI _fpsCounterText;
	// Defines how often fps will be updated on HUD
	[SerializeField] private float _fpsRefreshRate = 1f;
	private float _fpsCounter;
	private float _timer;

	private string _showFpsStateKey = "show_fps";
 
	private void Awake() {
		_controls = new Controls();
		_controls.Player.FPSToggle.performed += (InputAction.CallbackContext context) => ToggleFPS();

		// Allow only one instance by destroying another ones
		DontDestroyOnLoad(this.gameObject);
		if (FindObjectsOfType(GetType()).Length > 1)
			Destroy(this.gameObject);
		
		LoadState();
	}

	private void Update() {
		if (Time.time > _timer) {
			_fpsCounter = (int)(1f / Time.unscaledDeltaTime);
			_fpsCounterText.text = _fpsCounter.ToString() + " FPS";
			_timer += _fpsRefreshRate;
		}
	}

	public void ToggleFPS() {
		SaveState();
		LoadState();
	}

	private void SaveState() {
		PlayerPrefs.SetInt(
			_showFpsStateKey,
			Convert.ToInt32(!_fpsCounterText.gameObject.activeSelf)
		);
	}

	private void LoadState() {
		_fpsCounterText.gameObject.SetActive(
			Convert.ToBoolean(PlayerPrefs.GetInt(_showFpsStateKey, 0))
		);
	}

	private void OnEnable() {
		_controls.Enable();
	}

	private void OnDisable() {
		_controls.Disable();
	}
}
