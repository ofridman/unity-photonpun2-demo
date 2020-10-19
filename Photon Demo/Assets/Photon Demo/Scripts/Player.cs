using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
    public Transform ShootBase;

    public float Speed = 5f;
    public int Health = 100;

    Vector3 newPosition;
    Animator animator;
    Vector3 lookAtPos;

    bool canLerp;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        newPosition = transform.position;
    }
    private void Update()
    {
        Move();
        Animate();

        if (!photonView.IsMine)
            return;
        DetectInput();

    }
    void Animate()
    {
        if (Vector3.Distance(transform.position, newPosition) > 0.2f)
        {
            animator.SetBool("Run", true);
        }
        else
            animator.SetBool("Run", false);

    }
    void DetectInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                lookAtPos = hit.point;
                lookAtPos.y = 0f;
                transform.LookAt(lookAtPos, transform.up);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    newPosition = hit.point;
                else if (hit.collider.gameObject != this.gameObject)
                    //  Shoot();
                    photonView.RPC("Shoot", RpcTarget.All);
            }
        }
    }
    void Move()
    {
        if (!photonView.IsMine)
            transform.LookAt(lookAtPos, transform.up);
        if(canLerp || photonView.IsMine)
        transform.position = Vector3.Lerp(transform.position, newPosition, Speed * Time.deltaTime);
    }
    [PunRPC]
    void Damage(int amount)
    {
            Health -= amount;

        if ( Health <= 0)
            animator.SetTrigger("Dead");
    }
    [PunRPC]
    void Shoot()
    {
        // Instantiate bullet
        GameObject bullet = Instantiate(GameManager.Instance.BulletPrefab, ShootBase.position, ShootBase.rotation);
        Rigidbody bulletRigidBody = bullet.GetComponent<Rigidbody>();
        bulletRigidBody.velocity = transform.TransformDirection(new Vector3(0, 0, 50f));
       
        GameManager.Instance.AudioSource.PlayOneShot(GameManager.Instance.ShootClip);

    }
    [PunRPC]
    void AskData()
    {
        photonView.RPC("SendData", RpcTarget.Others, newPosition, Health);
    }
    [PunRPC]
    void SendData(Vector3 _newPosition, int _currentHealth)
    {

        newPosition = _newPosition;
        Health = _currentHealth;
        transform.position = newPosition;
        canLerp = true;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(newPosition);
            stream.SendNext(lookAtPos);
        }
        else
        {
            newPosition = (Vector3)stream.ReceiveNext();
            lookAtPos = (Vector3)stream.ReceiveNext();
        }

    }
    private void OnEnable()
    {

            foreach (var player in PhotonNetwork.PlayerListOthers)
            {
                photonView.RPC("AskData", player);
            }
      
    }
}
