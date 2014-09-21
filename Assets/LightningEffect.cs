using UnityEngine;
using System.Collections;

public class LightningEffect : MonoBehaviour 
{
	private bool doingLightningSequence = false;
	private Light spotlight;
	// Use this for initialization
	void Start () 
	{
		spotlight = GetComponent<Light>();
		StartCoroutine(WaitForLightningSequence());
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(doingLightningSequence)
		{
			if(Random.value <= 0.9f)
			{
				spotlight.enabled = true;
			}
			else
			{
				spotlight.enabled = false;
			}
		}
	}

	private IEnumerator StopLightningSequence()
	{
		yield return new WaitForSeconds(Random.Range(1, 3));

		spotlight.enabled = false;
		doingLightningSequence = false;
		StartCoroutine(WaitForLightningSequence());
	}

	private IEnumerator WaitForLightningSequence()
	{
		yield return new WaitForSeconds(Random.Range(5, 15));

		doingLightningSequence = true;

		StartCoroutine(StopLightningSequence());
	}
}
