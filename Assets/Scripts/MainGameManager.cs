using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainGameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI[] displayNames;
    PhotonView myPv;
    // Start is called before the first frame update
    void Start()
    {
        myPv = this.GetComponent<PhotonView>();
        roomName.text = "Room Name: " + PlayerPrefs.GetString("RName");
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PName");
        Debug.Log("Player Name: " + PhotonNetwork.NickName);
        Debug.Log("Player List:");
        Debug.Log("Count: " + PhotonNetwork.PlayerList.Length);
        //Populate player names in UI
        if (PhotonNetwork.PlayerList.Length < 9)
        {
            myPv.RPC("updateName", RpcTarget.All);
        }
        if (PhotonNetwork.PlayerList.Length == 8)
        {
            Debug.Log("Start Game");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [PunRPC]
    void updateName()
    {
        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            displayNames[i].text = player.NickName;
            i++;
        }
        i = 0;
    }
}
