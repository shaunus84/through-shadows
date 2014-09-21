using UnityEngine;
using System.Collections;

public class TopDownController : MonoBehaviour
{
		// inspector vars
		public float walkSpeed;
		public float runSpeed;
		public float sneakSpeed;
		public float lookSmoothing;

		// components
		private Animator anim;
		private CharacterController controller;

		// private vars
		private float moveSpeed;

		void Awake ()
		{
				anim = GetComponent<Animator> ();
				controller = GetComponent<CharacterController> ();
		}
	
		// Update is called once per frame
		void Update ()
		{
				anim.SetBool ("Moving", (Input.GetAxisRaw ("Horizontal") != 0 || Input.GetAxisRaw ("Vertical") != 0));
				anim.SetBool ("Sneaking", Input.GetButton ("Fire1"));
		
				if (Input.GetButton ("Fire1")) {
						moveSpeed = sneakSpeed;
				} else {
						moveSpeed = walkSpeed;
				}
				Vector3 dir = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * moveSpeed;

				controller.SimpleMove (dir);


				if (dir != Vector3.zero) {
						transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (dir), Time.deltaTime * 10f);
				}

		}
}
