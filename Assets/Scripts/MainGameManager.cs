using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject voteForCaptainPopup;
    public TextMeshProUGUI voteForCaptainName;
    public TextMeshProUGUI voteForFirstMateName;    
    public TextMeshProUGUI topText;
    public GameObject voteTallyPopup;
    public TextMeshProUGUI listOfVotes;
    public TextMeshProUGUI voteTotalsText;
    public GameObject continueButton;
    public TextMeshProUGUI autoMoveNum;
    //Game Elements
    public int maxPlayers = 8;
    public Player pirateLeader;
    public Player pirateCrew1;
    public Player pirateCrew2;
    public Player firstMate;
    public Player captain;
    public Player captainElect;
    public Dictionary<string, string> allVotes = new Dictionary<string, string>();
    int voteTally;
    bool votePassed;
    public int autoMoveCount;
    public int blueDraw;
    public int redDraw;
    public int blueDiscard;
    public int redDiscard;
    void Start()
    {
        myPv = this.GetComponent<PhotonView>();
        roomName.text = "Room Name: " + PlayerPrefs.GetString("RName");
        autoMoveCount = 3;
        autoMoveNum.text = autoMoveCount.ToString();
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PName");
        blueDraw = 6;
        redDraw = 11;
        blueDiscard = 0;
        redDiscard = 0;
        //Populate player names in UI
        if (PhotonNetwork.PlayerList.Length <= maxPlayers)
        {
            myPv.RPC("updateName", RpcTarget.All);
        }
        // Start Game once 8th player joins
        if (PhotonNetwork.PlayerList.Length == maxPlayers)
        {
            // randomize loyalties
            List<int> ranNums = new List<int>();
            while (ranNums.Count < 3)
            {
                int n = Random.Range(0, maxPlayers);
                if (!ranNums.Contains(n))
                {
                    ranNums.Add(n);
                }
            }
            int i = Random.Range(0, maxPlayers);
            ranNums.Add(i);
            int[] numList = new int[4];
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
        //set loyalties across network, assign first player
        pirateLeader = PhotonNetwork.PlayerList[ranNumArr[0]];
        pirateCrew1 = PhotonNetwork.PlayerList[ranNumArr[1]];
        pirateCrew2 = PhotonNetwork.PlayerList[ranNumArr[2]];
        firstMate = PhotonNetwork.PlayerList[ranNumArr[3]];
        captain = firstMate;
        myPv.RPC("startGameLoop", RpcTarget.All);
    }
    [PunRPC]
    void startGameLoop()
    {
        topText.text = firstMate.NickName + " is choosing a Captian.";
        if (PhotonNetwork.LocalPlayer == firstMate)
        {
            topText.text = "You are the First Mate! Choose a Captain.";
            activatePlayerChoices("chooseCaptain");
        }
    }
    [PunRPC]
    void setCaptain(int n)
    {
        captain = PhotonNetwork.PlayerList[n];
    }
    [PunRPC]
    void setCaptainElect(int n)
    {
        captainElect = PhotonNetwork.PlayerList[n];
    }
    [PunRPC]
    void voteForCaptain()
    {
        topText.text = "";
        foreach (TextMeshProUGUI name in displayNames)
        {
            Transform toggle = name.gameObject.transform.GetChild(0);
            toggle.gameObject.SetActive(false);
        }
        voteForCaptainPopup.SetActive(true);
        voteForCaptainName.text = captainElect.NickName;
        voteForFirstMateName.text = firstMate.NickName;
    }
    [PunRPC]
    void tallyVotes(string name, string vote)
    {
        allVotes.Add(name, vote);
        if (allVotes.Count == 8)
        {
            topText.text = "";
            voteTally = 0;
            listOfVotes.text = "";
            voteTallyPopup.SetActive(true);
            foreach ( KeyValuePair<string, string> votes in allVotes)
            {
                listOfVotes.text = listOfVotes.text + votes.Key + " voted " + votes.Value + "\n";
                if (votes.Value == "Aye")
                {
                    voteTally += 1;
                }
            }
            if (voteTally > maxPlayers / 2)
            {
                voteTotalsText.text = "The vote passes!";
                votePassed = true;
            }
            else
            {
                voteTotalsText.text = "The vote fails!";
                votePassed = false;
            }
            continueButton.SetActive(true);
            allVotes.Clear();
        }
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
                    loyaltyText.text = "Loyal Crew!";
                }
            }
        }
    }
    public void activatePlayerChoices(string activeType)
    {
        if (activeType == "chooseCaptain")
        {
            foreach (TextMeshProUGUI name in displayNames)
            {
                
                if (name.text == captain.NickName || name.text == firstMate.NickName)
                {
                    continue;
                }
                else
                {
                    Transform toggle = name.gameObject.transform.GetChild(0);
                    toggle.gameObject.SetActive(true);
                }
            }
        }
    }
    public void togglePlayerOne()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 0);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerTwo()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 1);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerThree()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 2);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerFour()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 3);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerFive()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 4);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerSix()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 5);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerSeven()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 6);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void togglePlayerEight()
    {
        myPv.RPC("setCaptainElect", RpcTarget.All, 7);
        myPv.RPC("voteForCaptain", RpcTarget.All);
    }
    public void voteAye()
    {
        myPv.RPC("tallyVotes", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, "Aye");
        voteForCaptainPopup.SetActive(false);
        topText.text = "Tallying votes...";
    }
    public void voteNay()
    {
        myPv.RPC("tallyVotes", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, "Nay");
        voteForCaptainPopup.SetActive(false);
        topText.text = "Tallying votes...";
    }
    public void continueButtonClick()
    {
        continueButton.SetActive(false);
        voteTallyPopup.SetActive(false);
        if (votePassed)
        {
            //firstofficerbuilddeck
            //firstofficerdrawthree
            //firstofficerdiscardoneRPC
            //captaindiscardoneRPC
            //playoneRPC
        }
        else
        {
            autoMoveCount -= 1;
            autoMoveNum.text = autoMoveCount.ToString();
            if (autoMoveCount == 0)
            {
                autoMoveCount = 3;
                autoMoveNum.text = autoMoveCount.ToString();
                List<string> deck = new List<string>();
                deck = buildDeck();
                // drawone
                // playoneRPC
            }
            if (captain == firstMate)
            {
                captain = captain.GetNext();
            }
            firstMate = firstMate.GetNext();
            myPv.RPC("startGameLoop", RpcTarget.All);
        }
    }
    public List<string> buildDeck()
    {
        List<string> deck = new List<string>();
        for (int i = 0; i < blueDraw; i++)
        {
            deck.Add("Blue");
        }
        for (int i = 0; i < redDraw; i++)
        {
            deck.Add("Red");
        }
        return (deck);
    }
}