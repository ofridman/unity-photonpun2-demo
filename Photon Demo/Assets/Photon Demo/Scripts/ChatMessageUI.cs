using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour
{
    public Text UsernameTxt;
    public Text MessageTxt;

    public void SetValues(string _username, string _message)
    {
        UsernameTxt.text = _username;
        MessageTxt.text = _message;
        if (_username != PhotonNetwork.LocalPlayer.NickName)
        {
            UsernameTxt.alignment = TextAnchor.UpperRight;
            MessageTxt.alignment = TextAnchor.UpperRight;

        }

    }
}
