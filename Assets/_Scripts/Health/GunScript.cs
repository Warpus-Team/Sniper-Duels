using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : NetworkBehaviour
{
    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void Update()
    {
        
    }
}
