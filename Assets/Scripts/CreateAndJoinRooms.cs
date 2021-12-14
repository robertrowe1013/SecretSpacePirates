using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI roomName;

    public void CreateRoom()
    {
        // null check for room and player names
        PhotonNetwork.CreateRoom(roomName.text);
    }
    public void JoinRoom()
    {
        // null check for room and player names
        PhotonNetwork.JoinRoom(roomName.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MainScreen");
    }
}