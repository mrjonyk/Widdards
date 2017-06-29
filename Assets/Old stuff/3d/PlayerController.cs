using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	public float speed = 6.0F;
	public float jumpSpeed = 8.0F;
	public float gravity = 20.0F;
	public float mouseSensitivity = 5.0F;
	private Vector3 moveDirection = Vector3.zero;
	private CharacterController controller;
	private SpellCaster spellcaster;

	void Start() {
		controller = GetComponent<CharacterController>();
		spellcaster = GetComponentInChildren<SpellCaster> ();
	}

	void Update()
	{
		if (!isLocalPlayer)
		{
			return;
		}
		if (isClient) 
			Debug.Log (spellcaster.transform.childCount);
		if (controller.isGrounded) {
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= speed;
			if (Input.GetButton("Jump"))
				moveDirection.y = jumpSpeed;
			
		}
		moveDirection.y -= gravity * Time.deltaTime;
		controller.Move(moveDirection * Time.deltaTime);
		
		transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0)* mouseSensitivity);
		
		if (Input.GetButtonDown ("Fire1")) {
			CmdCreateFire();
		}
		if (Input.GetButtonUp ("Fire1")) {
			CmdFireFire();
		}
	}

	[Command]
	public void CmdCreateFire() {
		var fireball = spellcaster.CreateFire ();
		NetworkServer.Spawn (fireball);
	}
	[Command]
	public void CmdFireFire() {
		spellcaster.FireFire ();
	}

	public override void OnStartLocalPlayer()
	{
		Camera.main.transform.SetParent(transform);
	}
}