using Photon.Pun;
using UnityEngine;

public class RotationMimic : MonoBehaviourPun
{
    [SerializeField] private Transform mimicObject;

    private void Start()
    {
        enabled = photonView.IsMine;
    }

    private void Update()
    {
        if (mimicObject == null)
            return;

        transform.rotation = mimicObject.rotation;

        //tem q rotacionar a arma ou mudar o script pra inverter
    }
}
