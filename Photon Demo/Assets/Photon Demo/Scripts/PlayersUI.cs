using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersUI : MonoBehaviour
{

    public Text PlayersNameTxt;
    public Text PlayersLvlTxt;

    public void SetPlayerStats(string _playerName, int lvl)
    {
        PlayersNameTxt.text = _playerName;
        PlayersLvlTxt.text = "Level " + lvl;
    }

}
