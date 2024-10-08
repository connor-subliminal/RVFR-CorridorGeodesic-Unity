//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes: Created following the portal rendering tutorial by Brackeys
// Source: https://www.youtube.com/watch?v=cuQao3hEKfs
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
	//=-----------------=
	// Public Variables
	//=-----------------=


	//=-----------------=
	// Private Variables
	//=-----------------=


	//=-----------------=
	// Reference Variables
	//=-----------------=
	[SerializeField] private Camera cameraA, cameraB;
	[SerializeField] private  Material cameraMatA, cameraMatB;



	//=-----------------=
	// Mono Functions
	//=-----------------=
	void Update () {
		if (cameraA.targetTexture != null)
		{
			cameraA.targetTexture.Release();
		}
		cameraA.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
		cameraMatA.mainTexture = cameraA.targetTexture;

		if (cameraB.targetTexture != null)
		{
			cameraB.targetTexture.Release();
		}
		cameraB.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
		cameraMatB.mainTexture = cameraB.targetTexture;
	}
	

	//=-----------------=
	// Internal Functions
	//=-----------------=


	//=-----------------=
	// External Functions
	//=-----------------=
}

