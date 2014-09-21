using UnityEngine;
using System.Collections;

public class PlayerDetector : MonoBehaviour 
{
	private GameObject player;
	private Light pointLight;
	private SkinnedMeshRenderer _renderer;
	private Transform lightVisibilityCheck;

	// Use this for initialization
	void Awake () 
	{
		player = GameObject.FindGameObjectWithTag ("Player");
		lightVisibilityCheck = GameObject.Find ("LightVisibilityChecker").transform;
		pointLight = GetComponent<Light> ();
		_renderer = GameObject.Find("vincent_Casual_Male_Lod_1").GetComponent<SkinnedMeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Debug.DrawRay (pointLight.transform.position, (lightVisibilityCheck.position - pointLight.transform.position).normalized * (pointLight.range * 0.85f));
		RaycastHit hit;
		_renderer.material.color = Color.white;
		if (Physics.Raycast (pointLight.transform.position, (lightVisibilityCheck.transform.position - pointLight.transform.position).normalized, out hit, pointLight.range * 0.85f)) 
		{
			if(hit.collider.gameObject.tag == "Player")
			{
				_renderer.material.color = Color.red;
			}
		} 
	}
}
