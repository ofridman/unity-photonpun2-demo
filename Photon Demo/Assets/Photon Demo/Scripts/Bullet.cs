using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    public int Damage = 10;
    private void OnCollisionEnter(Collision collision)
    {
        // only master client can detect if bullet hit another player ( this is to avoid hacking or lag issues)
        if (PhotonNetwork.IsMasterClient)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
                player.photonView.RPC("Damage", RpcTarget.AllViaServer, Damage);
          

        }
    }
    private void Start()
    {
        Destroy(this.gameObject, 5f);
    }
}
