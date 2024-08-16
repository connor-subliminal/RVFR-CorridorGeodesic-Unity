//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorGeo_ActorData : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] public Vector3 homePosition;
    [SerializeField] public Vector3 homeScale;
    [SerializeField] public Transform homeParent;
    [SerializeField] public bool nullSpace = false;
    [SerializeField] public bool activeInNullSpace = false;
    [SerializeField] public bool diesInKillTrigger;
    public event Action OnRiftRestore;

    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=

    private void Start ()
    {
        homePosition = transform.position;
        homeScale = transform.localScale;
        homeParent = transform.parent;
        ALTItem_Geodesic_Utility_GeoFolder.CorGeo_ActorDatas.Add(this);
        if (TryGetComponent<Light> (out Light light))
        {
            activeInNullSpace = true;
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=

    public void GoHome ()
    {
        OnRiftRestore?.Invoke();
        gameObject.SetActive (true);
        transform.SetParent(homeParent);
        transform.localScale = homeScale;
        if (nullSpace)
        {
            transform.position = homePosition;
            return;
        }
        if (ALTItem_Geodesic_Utility_GeoFolder.plane1.GetDistanceToPoint (transform.position) > 0)
        {
            if (!ALTItem_Geodesic_Utility_GeoFolder.deployedRift) return;
            //move actor away from collapse direction scaled by the rift timer's progress
            transform.position += ALTItem_Geodesic_Utility_GeoFolder.deployedRift.transform.forward * 
                                  ALTItem_Geodesic_Utility_GeoFolder.riftWidth * 
                                  (ALTItem_Geodesic_Utility_GeoFolder.lerpAmount);
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
