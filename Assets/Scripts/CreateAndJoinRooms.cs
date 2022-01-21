using UnityEngine;
using Photon.Pun;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI roomName;

    void Start()
    {
        if (PlayerPrefs.HasKey("RName"))
        {
            playerName.text = PlayerPrefs.GetString("PName");
            roomName.text = PlayerPrefs.GetString("RName");
        }
    }
    public void CreateRoom()
    {
        // null check for room and player names
        PlayerPrefs.SetString("PName", playerName.text);
        PlayerPrefs.SetString("RName", roomName.text);
        PlayerPrefs.Save();
        //Debug.Log("Room Name: " + roomName.text);
        //Debug.Log("RName in Prefs: " + PlayerPrefs.GetString("RName"));
        PhotonNetwork.CreateRoom(roomName.text);
    }
    public void JoinRoom()
    {
        // null check for room and player names
        PlayerPrefs.SetString("PName", playerName.text);
        PlayerPrefs.SetString("RName", roomName.text);
        PlayerPrefs.Save();
        // Debug.Log("Room Name: " + roomName.text);
        // Debug.Log("RName in Prefs: " + PlayerPrefs.GetString("RName"));
        PhotonNetwork.JoinRoom(roomName.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MainScreen");
    }
}