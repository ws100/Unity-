using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //��������
    public static GameManager Gm;
    //��ȡ��ͼ�Ľű�����
    public MapDataGetter mapData;
    //��Ϸ��ʾ����
    public ShowTipMessage tip;
    //���Ƶ�ͼƬ����
    public Sprite[] cards;
    //�����ϵ�����
    public GameObject[] boardObject;
    //���������������
    public int boardNumber;
    //������ÿ�������������Ŀ
    public int[] boardType;
    //�����������˳��
    public List<int> boardOrder;
    //ʵ����ʾ�а���������˳��
    public List<int> showBoardOrder;
    //��ʾΪ�յ�Sprite
    public Sprite noneObjSprite;
    //�ͻ��˵�Socket������
    public Client client;
    //��¼״̬
    public bool isLogin;
    public string userId;
    //��Ϸ״̬
    public enum GameStatus {OnMenu,PlayingS,PlayingA,PlayingAI,PlayingB,OnWaiting,Pause };
    public GameStatus gameStatus;
    //��Ϸ���
    public GameObject menuPanel;
    public GameObject playingPanelS;
    public GameObject playingPanelA;
    public GameObject playingPanelAI;
    public GameObject playingPanelB;
    public Text pauseButton;
    public GameObject lostPanel;
    //��Ϸ��������
    public GameObject userContent;
    public GameObject playerRoomPanel;
    //�����û��б�
    public struct UserItemStruct
    {
        public string User;
        public bool IsOnline;
    }
    public List<UserItemStruct> onlineUsers;
    public GameObject onlineUserItemGameObject;
    public List<GameObject> onlineUserGameObjectList;
    //���а����
    public GameObject rankListPanel;
    public GameObject rankListItem;
    public GameObject rankListContent;
    //���ù���
    public GameObject settingsPanel;
    //��������
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    //��¼ע�����
    public GameObject loginPanel;
    public GameObject regPanel;
    private void Awake()
    {
        //��ʼ��GameManager�ĵ���
        Gm = GetComponent<GameManager>();
        //��ʼ����ͼ��ȡ�Ľű�
        mapData.Init();
        //��ʼ�����
        InitBoard();
        //��ʼ����Ϣ��ʾ
        tip.Init();
        //��ʼ��������ҵ�����List
        onlineUsers = new List<UserItemStruct>();
        onlineUserGameObjectList = new List<GameObject>();
        isLogin = false;
        gameStatus = GameStatus.OnMenu;
    }
    public void InitBoard()
    {
        //��ʼ������,ȫ������
        boardType = new int[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            boardType[i] = 0;
        }
        boardOrder = new List<int>();
    }
    //��ʼ��ť�ĵ���¼�
    public void GameStart()
    {
        //���ɿ���
        mapData.GenerateCards();
        playingPanelS.SetActive(true);
        menuPanel.SetActive(false);
        gameStatus = GameStatus.PlayingS;
        for (int i = 0; i < 7; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
        }
    }
    //�������ƶ�������
    public void EnterBoard(int type,GameObject o,int level)
    {
        //�����ڵ���ϵ
        MapDataGetter.mapData.UpdateCardStatus(level);
        //����Ӧ���ƶ�����λ��
        int orderInBoard = -1;
        //�Ƿ��б����Ѵ���
        bool isNeedMove = false;
        for (int i = 0; i < boardOrder.Count; i++)
        {
            //�ͱ�����ƥ��
            if (boardOrder[i] == type) orderInBoard = i;
        }
        //������б��д��������¼���
        
        if (orderInBoard != -1)
        {
            orderInBoard = 0;
            isNeedMove = true;
            
            for (int i = 0; i < boardOrder.Count; i++)
            {
                //�ͱ�����ƥ��
                if (boardOrder[i] == type)
                {
                    //�ҵ�type������
                    orderInBoard += boardType[boardOrder[i]];
                    break;
                }
                //��type��������������
                orderInBoard += boardType[boardOrder[i]];
            }
        }
        else
        {
            //�ŵ�ĩβ
            orderInBoard = boardNumber;
            //���˿��Ʒŵ����
            boardOrder.Add(type);
        }
        //���еĸ��Ӻ���
        //�ж��Ƿ���Ҫ����
        if (type == boardOrder[boardOrder.Count - 1]) isNeedMove = false;
        print(isNeedMove);
        if (isNeedMove)
        {
            //��boardOrder֮���Ԫ�غ���һ����λ
            //�������Ԫ�ص���������
            List<int> moveArray = new List<int>();
            bool flag = false;//�Ƿ��һ��ƥ�䵽����
            for (int i = 0; i < boardOrder.Count; i++)
            {
                if (boardOrder[i] == type) flag = true;
                if (flag == true&&boardOrder[i] != type)
                {
                    for (int j = 0; j < boardType[boardOrder[i]]; j++)
                    {
                        moveArray.Add(boardOrder[i]);
                    }
                }
            }
            //����λ��Sprite�ƿ�
            boardObject[orderInBoard].GetComponent<Image>().sprite = noneObjSprite;
            //��֮���Sprite���¸�ֵ
            for (int i = 0; i < moveArray.Count; i++)
            {
                boardObject[orderInBoard + i + 1].GetComponent<Image>().sprite = cards[moveArray[i]];
            }
        }

        //�����ƶ��������λ������
        Vector2 pos = boardObject[orderInBoard].transform.position;
        //�������ƶ�������
        o.transform.DOMove(pos,1.0f);
        //��ʼЭ�̶�ʱ
        //��Ҫһ��Э���ڶ������Ž���֮��ִ��һЩ����
        boardType[type]++;
        bool isPop;
        if (boardType[type] == 3) isPop = true;
        else isPop = false;
        StartCoroutine(AfterMove(o,orderInBoard,type,isPop,orderInBoard));
        boardNumber++;
        
        //�������߼�
        if (boardType[type] == 3)
        {

            //��������
            boardType[type] = 0;
            boardNumber -= 3;
            boardOrder.Remove(type);
        }
        //��Ϸʧ��
        if (boardNumber == 7)
        {
            gameStatus = GameStatus.Pause;
            lostPanel.SetActive(true);
            print("��Ϸʧ��");
        }
    }
    IEnumerator AfterMove(GameObject o,int pos,int type,bool isPop,int orderInBoard)
    {
        //�ȴ�1.0��ִ��
        yield return new WaitForSeconds(1.0f);
        //����ʼ������ر�
        
        //�������ϵĿ�������ʾ��Ӧ��ͼ��
        //ˢ�°����ͼ��(����)
        //int objCount = 0;
        //for (int i = 0; i < boardOrder.Count; i++)
        //{
        //    int m = boardType[boardOrder[i]];
        //    for (int j = 0; j < m; j++)
        //    {
        //        boardObject[objCount++].GetComponent<Image>().sprite = cards[boardOrder[i]];
        //    }
        //}
        ////��ʣ���ͼƬ�ÿ�
        //for (int i = objCount; i < 7; i++)
        //{
        //    boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
        //}
        if (showBoardOrder.Count >= pos)
        {
            o.SetActive(false);
            boardObject[pos].GetComponent<Image>().sprite = cards[type];
            showBoardOrder.Insert(pos, type);
            if (isPop == true)
            {
                while (showBoardOrder.FindAll(t => t == type).Count > 0)
                {
                    showBoardOrder.Remove(type);
                }
            }

            for (int i = 0; i < 7; i++)
            {
                if (i < showBoardOrder.Count)
                {
                    boardObject[i].GetComponent<Image>().sprite = cards[showBoardOrder[i]];
                }
                else
                {
                    boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
                }
            }

            yield break;
        }
    }

    public void BackToMenu()
    {
        playingPanelA.SetActive(false);
        playingPanelB.SetActive(false);
        playerRoomPanel.SetActive(false);
        playingPanelS.SetActive(false);
        playingPanelAI.SetActive(false);
        gameStatus = GameStatus.OnMenu;
        menuPanel.SetActive(true);
    }

    public void ShowRank()
    {
        rankListPanel.SetActive(true);
    }

    public void Login()
    {
        loginPanel.SetActive(true);
    }

    public void Reg()
    {
        regPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);   
    }

    public void StartGameAuto()
    {
        playingPanelA.SetActive(true);
        menuPanel.SetActive(false);
        gameStatus = GameStatus.PlayingA;
    }

    public void StartGameAI()
    {
        playingPanelAI.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void StartGameBoth()
    {
        
    }

    public void OpenPlayerRoom()
    {
        
        print("������Ϸ����");
        if (isLogin == true)
        {
            playerRoomPanel.SetActive(true);
            ShowTipMessage.Tip.ShowTip("�û�"+userId+"���");
            //��ʼ����������
            client.InitClient();
            client.GetUserList();
            //������Ϣ��ȡ����б�
            gameStatus = GameStatus.OnWaiting;
            DPlayManager.PlayManager.status = DPlayManager.WaitingStatus.Online;
            menuPanel.SetActive(false);
        }
        else
        {
            ShowTipMessage.Tip.ShowTip("�û�δ��¼");
        }
    }

    public void RefreshOnlineList()
    {
        for (int i = 0; i < onlineUsers.Count; i++)
        {
            if (i < onlineUserGameObjectList.Count)
            {
                onlineUserGameObjectList[i].SetActive(true);
                OnlineUserItem item = onlineUserGameObjectList[i].GetComponent<OnlineUserItem>();
                item.Init(onlineUsers[i].User,onlineUsers[i].IsOnline);
            }
            else
            {
                GameObject o = Instantiate(onlineUserItemGameObject, userContent.transform);
                onlineUserGameObjectList.Add(o);
                OnlineUserItem item = o.GetComponent<OnlineUserItem>();
                item.Init(onlineUsers[i].User, onlineUsers[i].IsOnline);
            }
        }

        for (int i = onlineUsers.Count; i < onlineUserGameObjectList.Count; i++)
        {
            onlineUserGameObjectList[i].SetActive(false);
        }
    }

    public void PlayingPause()
    {
        if (gameStatus == GameStatus.PlayingS)
        {
            gameStatus = GameStatus.Pause;
            pauseButton.text = "����";
        }

        else if (gameStatus == GameStatus.Pause)
        {
            gameStatus = GameStatus.PlayingS;
            pauseButton.text = "��ͣ";
        }

    }
    public void PlayingBack()
    {
        lostPanel.SetActive(false);
        gameStatus = GameStatus.OnMenu;
        playingPanelS.SetActive(false);
        menuPanel.SetActive(true);
        mapData.ResetCard();
        InitBoard();
        boardNumber = 0;
        showBoardOrder = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
        }
    }

}
