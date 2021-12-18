using UnityEngine;
using Photon.Pun;
using TMPro;

public class MainGameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public TextMeshProUGUI roomName;
    // Start is called before the first frame update
    void Start()
    {
        roomName.text = "Room Name: " + PlayerPrefs.GetString("RName");
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PName");
        Debug.Log(PhotonNetwork.NickName);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
