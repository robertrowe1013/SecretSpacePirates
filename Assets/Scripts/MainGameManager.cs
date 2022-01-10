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
    #region Variables
    //networking vars
    PhotonView myPv;
    public GameObject playerPrefab;
    // UI Elements
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI[] displayNames;
    public TextMeshProUGUI autoMoveNum;
    // loyalty window
    public GameObject loyaltyPopup;
    public TextMeshProUGUI loyaltyText;
    public GameObject otherPiratePopup;
    public TextMeshProUGUI otherPirateName;
    public TextMeshProUGUI pirateLeaderName;
    // voting phase popups
    public GameObject voteForCaptainPopup;
    public TextMeshProUGUI voteForCaptainName;
    public TextMeshProUGUI voteForFirstMateName;    
    public TextMeshProUGUI topText;
    public GameObject voteTallyPopup;
    public TextMeshProUGUI listOfVotes;
    public TextMeshProUGUI voteTotalsText;
    public GameObject closeVotePopupButton;
    // path selection popups
    public GameObject fOChoosePathPopup;
    public TextMeshProUGUI fOPathOne;
    public TextMeshProUGUI fOPathTwo;
    public TextMeshProUGUI fOPathThree;
    public GameObject captainChoosePathPopup;
    public TextMeshProUGUI captainPathOne;
    public TextMeshProUGUI captainPathTwo;
    public GameObject captainPathChosenPopup;
    public TextMeshProUGUI pathResultsText;
    // debug window
    public TextMeshProUGUI debugCardsText;
    public TextMeshProUGUI debugOfficersText;
    // game functionality elements
    public int maxPlayers = 8;
    public Player pirateLeader;
    public Player pirateCrew1;
    public Player pirateCrew2;
    public Player firstMate;
    public Player captain;
    public Player captainElect;
    public Player previousCaptain;
    public Player previousFO;
    public Dictionary<string, string> allVotes = new Dictionary<string, string>();
    public int voteTally;
    public bool votePassed;
    public int autoMoveCount;
    public int waitingForPlayers;
    public int blueDraw;
    public int redDraw;
    public int blueDiscard;
    public int redDiscard;
    public int bluePlayed;
    public int redPlayed;
    public string card1;
    public string card2;
    public string card3;
    #endregion Variables
    void Start()
    {
        myPv = this.GetComponent<PhotonView>();
        roomName.text = "Room Name: " + PlayerPrefs.GetString("RName");
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.NickName = PlayerPrefs.GetString("PName");
        // set starting values for game elements.
        waitingForPlayers = 0;
        autoMoveCount = 3;
        autoMoveNum.text = autoMoveCount.ToString();
        blueDraw = 6;
        redDraw = 11;
        blueDiscard = 0;
        redDiscard = 0;
        //Populate player names in UI
        if (PhotonNetwork.PlayerList.Length <= maxPlayers)
        {
            myPv.RPC("RPCupdateName", RpcTarget.All);
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
            myPv.RPC("RPCstartGame", RpcTarget.All, numList as object);
        }
        updateCards();
    }
    public void updateName()
    {
        // update player names in display across network
        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            displayNames[i].text = player.NickName;
            i++;
        }
    }
    public void startGame(int[] ranNumArr)
    {
        // set loyalties across network, assign first player
        pirateLeader = PhotonNetwork.PlayerList[ranNumArr[0]];
        pirateCrew1 = PhotonNetwork.PlayerList[ranNumArr[1]];
        pirateCrew2 = PhotonNetwork.PlayerList[ranNumArr[2]];
        firstMate = PhotonNetwork.PlayerList[ranNumArr[3]];
        captain = firstMate;
        myPv.RPC("RPCgameLoopStart", RpcTarget.All);
    }
    public void gameLoopStart()
    {
        waitingForPlayers = 0;
        topText.text = firstMate.NickName + " is choosing a Captain.";
        if (PhotonNetwork.LocalPlayer == firstMate)
        {
            topText.text = "You are the First Mate! Choose a Captain.";
            activatePlayerChoices("chooseCaptain");
        }
    }
    public void gameLoopEnd()
    {
        waitingForPlayers += 1;
        if (waitingForPlayers == maxPlayers)
        {
            myPv.RPC("RPCgameLoopStart", RpcTarget.All);
        }
    }
    public void setCaptain(Player player)
    {
        captain = player;
    }
    public void setCaptainElect(Player player)
    {
        captainElect = player;
    }
    
    public void voteForCaptain()
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
    public void tallyVotes(string name, string vote)
    {
        allVotes.Add(name, vote);
        if (allVotes.Count == 8)
        {
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
            topText.text = "";
            closeVotePopupButton.SetActive(true);
            allVotes.Clear();
        }
    }
    public void playCard(string card)
    {
        if (card == "Blue")
        {
            blueDraw -= 1;
            bluePlayed += 1;
        }
        else
        {
            redDraw -= 1;
            redPlayed += 1;
        }
        updateCards();
    }
    public void discardCard(string card)
    {
        if (card == "Blue")
        {
            blueDraw -= 1;
            blueDiscard += 1;
        }
        else
        {
            redDraw -= 1;
            redDiscard += 1;
        }
        updateCards();
    }
    public void reshuffle()
    {
        blueDraw = blueDraw + blueDiscard;
        blueDiscard = 0;
        redDraw = redDraw + redDiscard;
        redDiscard = 0;
        updateCards();
    }
    public void captainChoosesPath(string card1passed, string card2passed)
    {
        topText.text = captain.NickName + " is choosing a path.";
        if (PhotonNetwork.LocalPlayer == captain)
        {
            card1 = card1passed;
            card2 = card2passed;
            topText.text = "";
            captainChoosePathPopup.SetActive(true);
            captainPathOne.text = card1;
            captainPathTwo.text = card2;
        }
    }
    public void pathResults()
    {
        captainPathChosenPopup.SetActive(true);
        pathResultsText.text = bluePlayed + "\n" + redPlayed;
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
    #region toggleAndButtonFunctions
    public void togglePlayerOne()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[0]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerTwo()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[1]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerThree()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[2]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerFour()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[3]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerFive()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[4]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerSix()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[5]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerSeven()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[6]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void togglePlayerEight()
    {
        myPv.RPC("RPCsetCaptainElect", RpcTarget.All, PhotonNetwork.PlayerList[7]);
        myPv.RPC("RPCvoteForCaptain", RpcTarget.All);
    }
    public void fOPathSelectButtonOne()
    {
        string path = GameObject.Find("FOPathButton1").GetComponentInChildren<TextMeshProUGUI>().text;
        fOChoosePathPopup.SetActive(false);
        card1 = card3;
        pathSelected(path, "FO");
    }
    public void fOPathSelectButtonTwo()
    {
        string path = GameObject.Find("FOPathButton2").GetComponentInChildren<TextMeshProUGUI>().text;
        fOChoosePathPopup.SetActive(false);
        card2 = card3;
        pathSelected(path, "FO");
    }
    public void fOPathSelectButtonThree()
    {
        string path = GameObject.Find("FOPathButton3").GetComponentInChildren<TextMeshProUGUI>().text;
        fOChoosePathPopup.SetActive(false);
        pathSelected(path, "FO");
    }
    public void captainPathSelectButtonOne()
    {
        string path = GameObject.Find("captainPathButton1").GetComponentInChildren<TextMeshProUGUI>().text;
        captainChoosePathPopup.SetActive(false);
        myPv.RPC("RPCdiscardCard", RpcTarget.All, card1);
        card1 = card2;
        pathSelected(path, "captain");
    }
        public void captainPathSelectButtonTwo()
    {
        string path = GameObject.Find("captainPathButton2").GetComponentInChildren<TextMeshProUGUI>().text;
        captainChoosePathPopup.SetActive(false);
        myPv.RPC("RPCdiscardCard", RpcTarget.All, card2);
        pathSelected(path, "captain");
    }
    public void voteAye()
    {
        myPv.RPC("RPCtallyVotes", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, "Aye");
        voteForCaptainPopup.SetActive(false);
        topText.text = "Tallying votes...";
    }
    public void voteNay()
    {
        myPv.RPC("RPCtallyVotes", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, "Nay");
        voteForCaptainPopup.SetActive(false);
        topText.text = "Tallying votes...";
    }
    public void pathResultsContinue()
    {
        captainPathChosenPopup.SetActive(false);
        firstMate = firstMate.GetNext();
        topText.text = "Waiting for players...";
        myPv.RPC("RPCgameLoopEnd", RpcTarget.All);
    }
    #endregion toggleAndButtonFunctions
    public void continueButtonClick()
    {
        topText.text = "";
        closeVotePopupButton.SetActive(false);
        voteTallyPopup.SetActive(false);
        if (votePassed)
        {
            topText.text = firstMate.NickName + " is choosing a path.";
            if (PhotonNetwork.LocalPlayer == firstMate)
            {
                topText.text = "";
                myPv.RPC("RPCsetCaptain", RpcTarget.All, captainElect);
                List<string> deck = new List<string>();
                deck = buildDeck();
                if (deck.Count == 0)
                {
                    myPv.RPC("RPCreshuffle", RpcTarget.All);
                    deck = buildDeck();
                }
                if (deck.Count > 2)
                {
                    List<int> ranNums = new List<int>();
                    while (ranNums.Count < 3)
                    {
                        int n = Random.Range(0, deck.Count);
                        if (!ranNums.Contains(n))
                        {
                            ranNums.Add(n);
                        }
                    }
                    card1 = deck[ranNums[0]];
                    card2 = deck[ranNums[1]];
                    card3 = deck[ranNums[2]];
                }
                else if (deck.Count == 2)
                {
                    card1 = deck[0];
                    card2 = deck[1];
                    List<string> discards = new List<string>();
                    discards = drawFromDiscards(1);
                    card3 = discards[0];
                    myPv.RPC("RPCreshuffle", RpcTarget.All);
                }
                else if (deck.Count == 1)
                {
                    card1 = deck[0];
                    List<string> discards = new List<string>();
                    discards = drawFromDiscards(2);
                    card2 = discards[0];
                    card3 = discards[1];
                    myPv.RPC("RPCreshuffle", RpcTarget.All);
                }
                fOChoosePathPopup.SetActive(true);
                fOPathOne.text = card1;
                fOPathTwo.text = card2;
                fOPathThree.text = card3;
            }
        }
        else
        {
            autoMoveCount -= 1;
            autoMoveNum.text = autoMoveCount.ToString();
            if (autoMoveCount == 0)
            {
                autoMoveCount = 3;
                autoMoveNum.text = autoMoveCount.ToString();
                if (PhotonNetwork.LocalPlayer == firstMate)
                {
                    List<string> deck = new List<string>();
                    deck = buildDeck();
                    string card = deck[Random.Range(0, deck.Count)];
                    myPv.RPC("RPCdiscardCard", RpcTarget.All, card);
                    myPv.RPC("RPCplayCard", RpcTarget.All, card);
                }
            }
            // bug for first vote failing.
            firstMate = firstMate.GetNext();
            topText.text = "Waiting for players to continue...";
            myPv.RPC("RPCgameLoopEnd", RpcTarget.All);
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
        if (deck.Count == 0)
        {
            myPv.RPC("RPCreshuffle", RpcTarget.All);
            deck = buildDeck();
        }
        return (deck);
    }
    public List<string> drawFromDiscards(int n)
    {
        List<string> deck = new List<string>();
        for (int i = 0; i < blueDiscard; i++)
        {
            deck.Add("Blue");
        }
        for (int i = 0; i < redDiscard; i++)
        {
            deck.Add("Red");
        }
        if (n == 1)
        {
            deck[0] = deck[Random.Range(0, deck.Count)];
        }
        if (n == 2)
        {
            int i1 = Random.Range(0, deck.Count);
            int i2 = Random.Range(0, deck.Count);
            while (i1 == i2)
            {
                i2 = Random.Range(0, deck.Count);
            }
            if (i2 == 0)
            {
                i2 = i1;
                i1 = 0;
            }
            deck[0] = deck[i1];
            deck[1] = deck[i2];
        }
        return (deck);
    }
    public void discardPath(string path)
    {
        if (path == "blue")
        {
            myPv.RPC("RPCdiscardCard", RpcTarget.All, "Blue");
        }
        else
        {
            myPv.RPC("RPCdiscardCard", RpcTarget.All, "Red");
        }
    }
    public void pathSelected(string path, string phase)
    {
        if (phase == "FO")
        {
            discardPath(path);
            myPv.RPC("RPCcaptainChoosesPath", RpcTarget.All, card1, card2);
        }
        if (phase == "captain")
        {
            discardPath(card1);
            myPv.RPC("RPCplayCard", RpcTarget.All, path);
            myPv.RPC("RPCpathResults", RpcTarget.All);
        }
    }
    public void updateCards()
    {
        debugCardsText.text = blueDraw.ToString() + "/" + redDraw.ToString() + "\n" + blueDiscard.ToString() + "/" + redDiscard.ToString() + "\n" + bluePlayed.ToString() + "/" + redPlayed.ToString();
    }
    #region RPCFunctions
    [PunRPC]
    void RPCupdateName()
    {
        updateName();
    }
    [PunRPC]
    void RPCstartGame(int[] ranNumArr)
    {
        startGame(ranNumArr);
    }
    [PunRPC]
    void RPCgameLoopStart()
    {
        gameLoopStart();
    }
    [PunRPC]
    void RPCgameLoopEnd()
    {
        gameLoopEnd();
    }
    [PunRPC]
    void RPCsetCaptain(Player player)
    {
        setCaptain(player);
    }
    [PunRPC]
    void RPCsetCaptainElect(Player player)
    {
        setCaptainElect(player);
    }
    [PunRPC]
    void RPCvoteForCaptain()
    {
        voteForCaptain();
    }
    [PunRPC]
    void RPCtallyVotes(string name, string vote)
    {
        tallyVotes(name, vote);
    }
    [PunRPC]
    void RPCplayCard(string card)
    {
        playCard(card);
    }
    [PunRPC]
    void RPCdiscardCard(string card)
    {
        discardCard(card);
    }
    [PunRPC]
    void RPCreshuffle()
    {
        reshuffle();
    }
    [PunRPC]
    void RPCcaptainChoosesPath(string card1passed, string card2passed)
    {
        captainChoosesPath(card1passed, card2passed);
    }
    [PunRPC]
    void RPCpathResults()
    {
        pathResults();
    }
    #endregion RPCFunctions
}