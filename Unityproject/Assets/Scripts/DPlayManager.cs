using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

public class DPlayManager : MonoBehaviour
{
    //��������
    #region Var
    public enum WaitingStatus
    {
        WaitingUser,MeetUser,Online,Offline,OnPlaying
    }
    public GameObject wPanel;
    //�Ƿ����Լ��Ļغ�
    public bool isMyTime = false;
    //�ȴ��������
    public GameObject waitingPanel;
    //�ȴ����������
    public Text waitingText;
    //���ֵ��û���
    public string opUser;
    //����ĵ�����
    public static DPlayManager PlayManager;
    //˫����Ϸ��״̬
    public WaitingStatus status;
    //�����û�����İ�ť
    public Button acceptBtn;
    //�ܾ��û�����İ�ť
    public Button refuseBtn;
    //����Ŀ����б�
    public List<int> myCardList;
    //�Է��Ŀ����б�
    public List<int> opCardList;
    //������ʾ�Ŀ����б�
    public List<int> myCardListInShow;
    //�Է���ʾ���Ƶ��б�
    public List<int> opCardListInShow;
    //�Ƿ���Ҫ����
    public bool needUpDateMapMy = false;
    public bool needUpDateMapOp = false;
    public bool needUpDateShowMy = false;
    public bool needUpDateShowOp = false;
    public string updateId;
    //�����ϵ�����
    public GameObject[] myCardBoardObjectList;
    public GameObject[] opCardBoardObjectList;
    //����string���CardId�ҵ�Card������
    public Dictionary<string, int> CardIdToIndex;
    //���ɿ��ƵĽ���
    public GameObject dCardContent;
    public GameObject dCard;
    //�����еĿ����б�
    public List<DCard> dCards;
    //���Ƶ�������
    public int maxLevel;
    //ÿ��Ķ�Ӧ���Ʊ��
    public List<List<int>> LevelCard;
    #endregion

    //���ߺ���
    #region UserFunction
    //�ҵ����һ��Ԫ�ط�������
    public int FindLastInList(int cardType,List<int> cardList)
    {
        int pos = -1;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] == cardType) pos = i;
        }
        return pos;
    }
    //�ҵ���һ��Ԫ�ط�������
    public int FindFirstInList(int cardType,List<int> cardList)
    {
        int pos = -1;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] == cardType)
            {
                pos = i;
                break;
            }
        }
        return pos;
    }
    //�ҵ��������ŵ����ֲ�����
    public int FindThreeInList(List<int> cardList)
    {
        int foundCount = 0;
        int prId = -1;
        for (int i = 0; i < cardList.Count; i++)
        {
            //��֮ǰ��type�����
            if (cardList[i] != prId)
            {
                prId = cardList[i];
                foundCount = 1;
            }
            //��֮ǰ��cardType���
            else
            {
                foundCount++;
                if (foundCount == 3) return i;
            }
        }
        return -1;
    }
    #endregion
    
    private void Awake()
    {
        status = WaitingStatus.Offline;
        PlayManager = GetComponent<DPlayManager>();
    }
    public void WaitUser(string user)
    {
        opUser = user;
        //�򿪵ȴ������
        waitingPanel.SetActive(true);
        waitingText.text = "�ȴ��û�" + user + "������";
        acceptBtn.gameObject.SetActive(false);
        refuseBtn.gameObject.SetActive(false);
    }
    public void UserAccept(int randomSeed)
    {
        ShowTipMessage.Tip.ShowTip("�û�" + opUser + "��ͬ��");
        Boolean b = new bool();
        b = true;
        EnterBtn(b,randomSeed);
    }
    public void MeetUser(string user)
    {
        //ShowTipMessage.Tip.ShowTip("�û�" + opUser + "��������");
        waitingPanel.SetActive(true);
        opUser = user;
        waitingText.text = "�û�" + user + "��������";
        acceptBtn.gameObject.SetActive(true);
        refuseBtn.gameObject.SetActive(true);
    }
    public void UserRefuse()
    {
        ShowTipMessage.Tip.ShowTip("�û�"+opUser+"�ܾ�����");
        waitingPanel.SetActive(false);
        opUser = "";
        status = WaitingStatus.Online;
    }
    public void EnterBtn(bool t,int randomSeed)
    {
        //����˫�˳���
        waitingPanel.SetActive(false);
        wPanel.SetActive(false);
        status = WaitingStatus.OnPlaying;
        GameManager.Gm.playingPanelB.SetActive(true);
        GameManager.Gm.gameStatus = GameManager.GameStatus.PlayingB;
        if (t == true) isMyTime = true;
        else
        {
            isMyTime = false;
        }
        isMyTime = t;
        //���س���
        //��ʼ������
        dCards = new List<DCard>();
        CardIdToIndex = new Dictionary<string, int>();
        myCardList = new List<int>();
        opCardList = new List<int>();
        myCardListInShow = new List<int>();
        opCardListInShow = new List<int>();
        LevelCard = new List<List<int>>();
        //��MapGetter�л�ȡ��Ϸ��ͼ����
        JObject jsonData = MapDataGetter.mapData.jsonData;
        RectTransform conT = dCardContent.GetComponent<RectTransform>();
        //����Json�е�levelData����(����ÿ�����Ƶ�λ��)
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
        //����blockTypeData����(����ÿ�ֿ��Ƶ�����)
        JObject TypeData = JObject.Parse(jsonData["blockTypeData"].ToString());
        //�������ɿ�����������
        List<int> rList = new List<int>();
        foreach (KeyValuePair<string, JToken> m in TypeData)
        {
            int typeNum = int.Parse(m.Key.ToString());
            int typeCount = int.Parse(m.Value.ToString())*3;
            for (int i = 0; i < typeCount; i++)
            {
                rList.Add(typeNum);
            }
        }
        //��������������
        Random.InitState(1000);
        for (int i = 0; i < rList.Count; i++)
        {
            int index = Random.Range(i, rList.Count);
            int tmp = rList[i];
            int ran = rList[index];
            rList[i] = ran;
            rList[index] = tmp;
        }
        int idx = 0;
        //���ɿ��Ƶĺ�������
        string parserCsv = "";
        int level = 1;
        foreach (KeyValuePair<string, JToken> m in levelData)
        {
            //��ȡ��������
            List<int> subLevelList = new List<int>();
            JArray subList = JArray.Parse(m.Value.ToString());
            foreach (JToken j in subList)
            {
                //������Ϸ����
                GameObject o = Instantiate(dCard,dCardContent.transform);
                //��ȡ���Ƶ�����
                int cardNum = rList[idx++] - 1;
                //��ȡ�����������Ӧ��ͼƬ
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[cardNum];
                //��ȡCard���
                DCard c = o.GetComponent<DCard>();
                CardIdToIndex.Add(j["id"].ToString(),idx - 1);
                //��Json����Ϣת����csv
                parserCsv += j["id"].ToString() + "," + level + "," + j["rowNum"] + "," + j["rolNum"] + "\n";
                //��������״̬
                c.type = cardNum;
                c.id = j["id"].ToString();
                c.xPos = int.Parse(j["rowNum"].ToString());
                c.yPos = int.Parse(j["rolNum"].ToString());
                c.status = DCard.st.OnMap;
                c.level = level;
                c.SetButton();
                //��card����ż��������
                subLevelList.Add(idx - 1);
                //��Card��������б�
                dCards.Add(o.GetComponent<DCard>());
                //��ȡ���������RectTransform
                RectTransform rt = o.GetComponent<RectTransform>();
                //���忨�Ƶĳ���λ��
                Vector2 cardPos = new Vector2(int.Parse(j["rolNum"].ToString()) * 10 - 640, int.Parse(j["rowNum"].ToString()) * 10 - 560);
                //���忨������֮���λ��
                Vector2 gPos = new Vector2(int.Parse(j["rolNum"].ToString()) * 10 - 640, 1000);
                //������Ų������λ��
                rt.anchoredPosition = gPos;
                //��������λ�õ�����λ�õĶ���
                rt.DOAnchorPos(cardPos,0.3f+idx*0.006f);
            }
            LevelCard.Add(subLevelList);
            level++;
        }
        maxLevel = level - 1;
        UpdateCardStatus(level-2);
        dCardContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(360, 300);
    }
    //��Է�����ͬ�������
    public void Accept()
    {
        int seed = Random.Range(1, 1000000);
        Client.clientManager.AcceptUser(opUser,seed);
        EnterBtn(false,seed);
    }
    //��Է����;ܽӵ�����
    public void Refuse()
    {
        Client.clientManager.RefuseUser(opUser);
        waitingPanel.SetActive(false);
    }
    private void Update()
    {
        //�����б�
        if (status != WaitingStatus.OnPlaying) return;
        //����İ�����Ҫ����
        if (needUpDateShowMy)
        {
            needUpDateShowMy = false;
            //�������������ʾ
            for (int i = 0; i < myCardListInShow.Count; i++)
            {
                int cardType = myCardListInShow[i];
                myCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.cards[cardType];
            }
            //����ʣ��Ŀհ���
            for (int i = myCardListInShow.Count; i < 7; i++)
            {
                myCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            }
        }
        //�Է��İ�����Ҫ����
        if (needUpDateShowOp)
        {
            needUpDateShowOp = false;
            //�������������ʾ
            for (int i = 0; i < opCardListInShow.Count; i++)
            {
                int cardType = opCardListInShow[i];
                opCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.cards[cardType];
            }
            //����ʣ��Ŀհ���
            for (int i = opCardListInShow.Count; i < 7; i++)
            {
                opCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            }
        }
        //�ҷ����
        if (needUpDateMapMy)
        {
            needUpDateMapMy = false;
            //1.ͨ��id��������
            int cardId = CardIdToIndex[updateId];
            Card c = dCards[cardId];
            int type = c.type;
            //2.���㿨�����µ�λ��
            int pos;
            if (FindLastInList(type, myCardList) == -1) pos = myCardList.Count;
            else pos = FindLastInList(type, myCardList) + 1;
            //3.�޸�Board��ʾ����ͼ�ڵ���ʾ,��ʼ���ƶ���
            UpdateCardStatus(c.level);
            for (int i = pos; i < myCardList.Count; i++)
            {
                myCardBoardObjectList[i + 1].GetComponent<Image>().sprite = GameManager.Gm.cards[myCardList[i]];
            }
            myCardBoardObjectList[pos].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            Vector3 tarPos = myCardBoardObjectList[pos].transform.position;
            c.gameObject.transform.DOMove(tarPos,1.0f);
            //4.����CardList����(�����߼�)
            myCardList.Insert(pos,type);
            if (FindThreeInList(myCardList) != -1)
            {
                while (myCardList.FindAll(t => t == type).Count > 0)
                {
                    myCardList.Remove(type);
                }
            }
            //5.����Э�̶�ʱ����CardListInShow����(���벢����)
            StartCoroutine(UpdateMyAnimation(c.gameObject, type));
        }
        //�Է����
        if (needUpDateMapOp)
        {
            needUpDateMapOp = false;
            //1.ͨ��id��������
            int cardId = CardIdToIndex[updateId];
            Card c = dCards[cardId];
            int type = c.type;
            //2.���㿨�����µ�λ��
            int pos;
            if (FindLastInList(type, opCardList) == -1) pos = opCardList.Count;
            else pos = FindLastInList(type, opCardList) + 1;
            //3.�޸�Board��ʾ,��ʼ���ƶ���
            UpdateCardStatus(c.level);
            for (int i = pos; i < opCardList.Count; i++)
            {
                opCardBoardObjectList[i + 1].GetComponent<Image>().sprite = GameManager.Gm.cards[opCardList[i]];
            }
            opCardBoardObjectList[pos].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            Vector3 tarPos = opCardBoardObjectList[pos].transform.position;
            c.gameObject.transform.DOMove(tarPos,1.0f);
            //4.����CardList����(�����߼�)
            opCardList.Insert(pos,type);
            if (FindThreeInList(opCardList) != -1)
            {
                while (opCardList.FindAll(t => t == type).Count > 0)
                {
                    opCardList.Remove(type);
                }
            }
            //5.����Э�̶�ʱ����CardListInShow����(���벢����)
            StartCoroutine(UpdateOpAnimation(c.gameObject,type));
        }
    }
    //��ʼЭ�̵ȴ�����
    IEnumerator UpdateMyAnimation(GameObject o, int type)
    {
        //�ȴ�1��֮��ִ��
        yield return new WaitForSeconds(1.0f);
        //���ô����µĿ���
        needUpDateShowMy = true;
        //�������Ŀ��Ƽ���,�����������߼�
        int pos = FindLastInList(type,myCardListInShow);
        if (pos == -1) pos = myCardListInShow.Count;
        //���洢�Ŀ��Ʋ�����ʾ������
        myCardListInShow.Insert(pos,type);
        //�жϿ�������
        if (FindThreeInList(myCardListInShow) != -1)
        {
            while (myCardListInShow.FindAll(t => t == type).Count > 0)
            {
                myCardListInShow.Remove(type);
            }
        }
        o.gameObject.SetActive(false);
        yield break;
    }
    IEnumerator UpdateOpAnimation(GameObject o, int type)
    {
        //�ȴ�1��֮��ִ��
        yield return new WaitForSeconds(1.0f);
        //���ô����µĿ���
        needUpDateShowOp = true;
        //�������Ŀ��Ƽ���,�����������߼�
        int pos = FindLastInList(type,opCardListInShow);
        if (pos == -1) pos = opCardListInShow.Count;
        //���洢�Ŀ��Ʋ�����ʾ������
        opCardListInShow.Insert(pos,type);
        //�жϿ�������
        if (FindThreeInList(opCardListInShow) != -1)
        {
            while (opCardListInShow.FindAll(t => t == type).Count > 0)
            {
                opCardListInShow.Remove(type);
            }
        }
        o.gameObject.SetActive(false);
        yield break;
    }
    //���е�ͼ�ϵĿ��Ƶ��ڵ���ϵ
    //����Ϊ��ǰ����߲�
    public void UpdateCardStatus(int level)
    {
        //��ÿһ�����Ƹ���(����Ҫ���±ȵ�ǰ�߲�Ŀ���)
        for (int i = 0; i < level; i++)
        {
            for (int j = 0;j < LevelCard[i].Count; j++)
            {
                UpdateCardStatus(dCards[LevelCard[i][j]]);
            }
        }
    }
    public void UpdateCardStatus(Card c)
    {
        //����ÿһ����ͼ�ϵĿ���
        for (int i = c.level; i < maxLevel; i++)
        {
            for (int j = 0; j < LevelCard[i].Count; j++)
            {
                Card tc = dCards[LevelCard[i][j]];
                if(tc.status == Card.st.OnBoard)continue;
                //�жϿ����Ƿ����ڵ���Χ��
                if (Mathf.Abs(tc.xPos - c.xPos) < 8 && Mathf.Abs(tc.yPos - c.yPos) < 8)
                {
                    //���ñ��ڵ�֮���״̬
                    c.status = Card.st.onMapZ;
                    c.GetComponent<Image>().color = Color.gray;
                    return;
                }
            }
        }
        //û�б��ڵ���ԭ״̬
        if (c.status == Card.st.onMapZ)
        {
            c.status = Card.st.OnMap;
            c.GetComponent<Image>().color = Color.white;
        }
    }
    public void FetchCard(string id,bool isMyOpt)
    {
        //ͨ��id����������Ϣ
        int index = CardIdToIndex[id];
        DCard dCard = dCards[index];
        int type = dCard.type;
        //�ҷ����Ƶ���ز���
        if (isMyOpt)
        {
            //��ʾ��Ϣ��ʾ
            ShowTipMessage.Tip.ShowTip("�ȴ��Է�����");
            needUpDateMapMy = true;
            updateId = id;
        }
        else
        {
            //��ʾ��Ϣ��ʾ
            ShowTipMessage.Tip.ShowTip("�Է��ѳ���");
            //����������Ϣ
            dCard.dStatus = DCard.Dst.OnOpBoard;
            dCard.status = Card.st.OnBoard;
            needUpDateMapOp = true;
            isMyTime = true;
            updateId = id;
        }
        
    }

    public void BackToMenu()
    {
        for (int i = 0; i < dCards.Count; i++)
        {
            dCards[i].gameObject.SetActive(false);
        }
        dCards = new List<DCard>();
        myCardList = new List<int>();
        opCardList = new List<int>();
        myCardListInShow = new List<int>();
        opCardListInShow = new List<int>();
        for (int i = 0; i < myCardBoardObjectList.Length; i++)
        {
            myCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        for (int i = 0; i < opCardBoardObjectList.Length; i++)
        {
            opCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        waitingPanel.SetActive(false);
        wPanel.SetActive(false);
        GameManager.Gm.gameStatus = GameManager.GameStatus.OnMenu;
        GameManager.Gm.menuPanel.SetActive(true);
        GameManager.Gm.playingPanelB.SetActive(false);
    }
}

