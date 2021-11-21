using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterMotor : MonoBehaviour {
	private Controls _controls;
	private float _horizontalAxis;
	private float _verticalAxis;

	private CharacterController cc;
	private Vector3 moveDirection = Vector3.zero;
	private Vector3 playerVelocity = Vector3.zero;
	private float moveSpeed;
	private float gravityForce = 16;
	[SerializeField] private bool noclip;
	
	[Header("General")]
	public float walkSpeed = 4f;
	public float runSpeed = 6.5f;
	public float midairSpeed = 3;
	float maxSpeed = 0;
	float groundSpeed = 0;
	public float maxAirSpeed = 3;
	public float maxWalkSpeed = 20;
	public float maxRunSpeed = 30;
	private bool _isRunning = false;
	public float drag = 0;
	public float jumpForce = 5;
	float adhesionForce = 0.4f;
	public bool lockKeyboard;
	float groundTimer;
	private int layerMask;

	private void Awake() {
		_controls = new Controls();
		_controls.Player.Noclip.performed += (InputAction.CallbackContext ctx) => {
			noclip = !noclip;
		};
	}

	private void Start() {
		cc = gameObject.GetComponent<CharacterController>();
		layerMask = LayerMask.GetMask("Player");
		layerMask = ~layerMask;
	}

	private void Update() {
		if (!lockKeyboard)
			Keyboard();
		
		if (!noclip)
			cc.Move(playerVelocity * Time.deltaTime);
	}

	private void FixedUpdate() {
		groundSpeed = _controls.Player.Sprint.ReadValue<float>() > 0 ? maxWalkSpeed : maxRunSpeed;
		// maxSpeed differs on the ground and in the air
		maxSpeed = cc.isGrounded ? groundSpeed : maxAirSpeed;
		
		if (cc.isGrounded)
			// delay ground detection to allow bhopping
			groundTimer += Time.fixedDeltaTime;
		else
			groundTimer = 0;
		
		if (groundTimer > 0.08f) {
			// on the ground use GroundAccelerate (friction)
			playerVelocity = GroundAccelerate(playerVelocity, moveDirection, moveSpeed);
		} else {
			// in the air use AirAccelerate (no friction)
			playerVelocity = AirAccelerate(playerVelocity, moveDirection, midairSpeed);
			// apply gravity 
			playerVelocity.y -= gravityForce * Time.deltaTime;
		}
	}

	public void Keyboard() {
		_isRunning = _controls.Player.Sprint.ReadValue<float>() > 0;
		_horizontalAxis = _controls.Player.Movement.ReadValue<Vector2>().x;
		_verticalAxis = _controls.Player.Movement.ReadValue<Vector2>().y;

		// calculate moveDirection 
		moveDirection = (
			_horizontalAxis * transform.right +
			_verticalAxis * transform.forward
		).normalized;
		moveSpeed = _isRunning ? runSpeed : walkSpeed;

		if (!noclip) {
			GetComponent<CharacterController>().enabled = true;
			if (cc.isGrounded) {
				if (_controls.Player.Jump.ReadValue<float>() > 0) {
					playerVelocity.y = Mathf.Clamp(playerVelocity.y, 0, Mathf.Infinity);
					playerVelocity.y += jumpForce;
				}		
			} 
			
		} else {
			GetComponent<CharacterController>().enabled = false;
			transform.Translate(0, 0, _verticalAxis * moveSpeed * Time.deltaTime, Space.Self);
			transform.Translate(_horizontalAxis * moveSpeed * Time.deltaTime, 0, 0, Space.Self);

			if (_isRunning)
				transform.Translate(0, moveSpeed * Time.deltaTime, 0, Space.World);

			if (_controls.Player.Crouch.ReadValue<float>() > 0)
				transform.Translate(0, -moveSpeed * Time.deltaTime, 0, Space.World);
		}
	}

	private Vector3 Accelerate(Vector3 currentVelocity, Vector3 direction, float acceleration) {
		// calculate dot product of velocity and move direction 
		float currentSpeed = Vector3.Dot(new Vector3(currentVelocity.x, 0, currentVelocity.z), direction);
		// calculate acceleration
        float addSpeed = acceleration * Time.fixedDeltaTime;

		if(currentSpeed + addSpeed > maxSpeed)
			// don't accelerate if current speed is equal or exceeds max speed
			addSpeed = Mathf.Clamp(maxSpeed - currentSpeed, 0, maxSpeed);
		
		// return velocity + acceleration 
		return new Vector3(currentVelocity.x, playerVelocity.y, currentVelocity.z) + addSpeed * direction;
	}

	private Vector3 GroundAccelerate(Vector3 currentVelocity, Vector3 direction, float acceleration) {
       	currentVelocity = ApplyFriction(currentVelocity, drag); 
       	return Accelerate(currentVelocity, direction, acceleration);
    }	
    
	private Vector3 AirAccelerate(Vector3 currentVelocity, Vector3 direction, float acceleration) {
       	return Accelerate(currentVelocity, direction, acceleration);
    }

	private Vector3 ApplyFriction(Vector3 currentVelocity, float friction) {
       	return currentVelocity * (1 / (friction + 1)); 
    }

	private void OnControllerColliderHit(ControllerColliderHit collision) {
		RaycastHit stepCast;
		float stepCastDepth = 0.1f;

		// check if player is able to step
		if (!Physics.Raycast(
			transform.position - (cc.height/2) * transform.up + cc.stepOffset * transform.up,
			new Vector3(playerVelocity.x, 0, playerVelocity.z).normalized,
			out stepCast,
			cc.radius/2+stepCastDepth,
			layerMask,
			QueryTriggerInteraction.Ignore)) 
		{

			// calculate velocity product in direction of collision normal
			float momentum = Vector3.Dot(playerVelocity, collision.normal);
			// subtract it from velocity (only if calculated momentum points against the normal)
			if (momentum < 0) playerVelocity -= collision.normal * momentum;
			// apply additional adhesion force (required for cc to detect collisions properly)
			if (collision.normal.y > 0) playerVelocity -= collision.normal * adhesionForce;
		}
	}

	public void AddMomentum(Vector3 momentum) {
		playerVelocity += momentum;
	}

	private void OnEnable() {
		_controls.Enable();
	}

	private void OnDisable() {
		_controls.Disable();
	}
}
