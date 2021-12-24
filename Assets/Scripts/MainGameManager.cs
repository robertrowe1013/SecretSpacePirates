using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainGameManager : MonoBehaviourPunCallbacks
{
    PhotonView myPv;
    public GameObject playerPrefab;
    //UI elements
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI[] displayNames;

    public GameObject loyaltyPopup;
    public TextMeshProUGUI loyaltyText;
    public GameObject otherPiratePopup;
    public TextMeshProUGUI otherPirateName;
    public TextMeshProUGUI pirateLeaderName;
    public string testString;
    //Game Elements
    public int maxPlayers = 8;
    public Player pirateLeader;
    public Player pirateCrew1;
    public Player pirateCrew2;

    // Start is called before the first frame update
    void Start()
    {
        myPv = this.GetComponent<PhotonView>();
        roomName.text = "Room Name: " + PlayerPrefs.GetString("RName");
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PName");
        //Populate player names in UI
        if (PhotonNetwork.PlayerList.Length <= maxPlayers)
        {
            myPv.RPC("updateName", RpcTarget.All);
        }
        // Start Game once 8th player joins
        if (PhotonNetwork.PlayerList.Length == maxPlayers)
        {
            // set loyalties
            List<int> ranNums = new List<int>();
            while (ranNums.Count < 3)
            {
                int n = Random.Range(0, maxPlayers);
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
        pirateLeader = PhotonNetwork.PlayerList[ranNumArr[0]];
        pirateCrew1 = PhotonNetwork.PlayerList[ranNumArr[1]];
        pirateCrew2 = PhotonNetwork.PlayerList[ranNumArr[2]];
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
            if (pirateLeader != null)
            {
                if (PhotonNetwork.LocalPlayer == pirateLeader)
                {
                    loyaltyText.text = "the Pirate Leader!";
                }
                else if (PhotonNetwork.LocalPlayer == pirateCrew1)
                {
                    loyaltyText.text = "a Secret Pirate!";
                    otherPiratePopup.SetActive(true);
                    otherPirateName.text = pirateCrew2.NickName;
                    pirateLeaderName.text = pirateLeader.NickName;
                }
                else if (PhotonNetwork.LocalPlayer == pirateCrew2)
                {
                    loyaltyText.text = "a Secret Pirate!";
                    otherPiratePopup.SetActive(true);
                    otherPirateName.text = pirateCrew1.NickName;
                    pirateLeaderName.text = pirateLeader.NickName;
                }
                else
                {
                    loyaltyText.text = "a Loyal Crewman!";
                }
            }
        }
    }
}
