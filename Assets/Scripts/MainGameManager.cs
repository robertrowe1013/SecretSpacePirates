using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainGameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI[] displayNames;
    PhotonView myPv;
    public GameObject loyaltyPopup;
    public TextMeshProUGUI loyaltyText;
    public string testString;

    // Start is called before the first frame update
    void Start()
    {
        myPv = this.GetComponent<PhotonView>();
        roomName.text = "Room Name: " + PlayerPrefs.GetString("RName");
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PName");
        //Populate player names in UI
        if (PhotonNetwork.PlayerList.Length <= 8)
        {
            myPv.RPC("updateName", RpcTarget.All);
        }
        // Start Game once 8th player joins
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            // set loyalties
            List<int> ranNums = new List<int>();
            while (ranNums.Count < 3)
            {
                int n = Random.Range(0, 8);
                if (!ranNums.Contains(n))
                {
                    ranNums.Add(n);
                }
            }
            int[] numList = new int[3];
            numList = ranNums.ToArray();
            myPv.RPC("startGame", RpcTarget.All, numList as object);
        }
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

    [PunRPC]
    void startGame(int[] ranNumArr)
    {
        // int[] ranNumArr = numList;
        Debug.Log(ranNumArr);   
        loyaltyText.text = ranNumArr[0].ToString() + ranNumArr[1].ToString() + ranNumArr[2].ToString();
    }

    public void loyaltyToggle()
    {
        if (loyaltyPopup.activeSelf == true)
        {
            loyaltyPopup.SetActive(false);
        }
        else
        {
            loyaltyPopup.SetActive(true);
        }
    }
}
