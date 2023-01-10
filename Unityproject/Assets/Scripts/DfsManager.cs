using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using DG.Tweening;
using Random = UnityEngine.Random;
public class DfsManager : MonoBehaviour
{
    public static DfsManager Dfs;
    //dfs�������·������
    public List<int> step;
    public List<int> maxStep;
    //�Ƿ�������ݼ���
    public bool isLoaded = false;
    //�Ƿ���ɼ���
    public bool isCalc = false;
    //dfs��������
    public int dfsMaxLength;
    //dfs�ĵ�ǰ���
    public int dfsLength;
    //�趨ģ���ʱ��
    public Text calcTime;
    //ģ����յ�ʱ��
    public int targetTime;
    //��ͼ�Ͽ��Ե���Ŀ�������
    public List<int> avMap;
    //ÿ���Ӧ�Ŀ���
    public List<List<int>> pLevelMap;
    public List<List<int>> mapList;
    public List<bool> isOnMap;
    public List<bool> isOnShowMap;
    public int[] boardScore = new[] { 0, 10000, 100000 };
    public double cT = 0;
    //���Ƶ�����
    public List<Card> cards;
    //���ɿ��Ƶĸ�����
    public GameObject content;
    //�������ŵĲ���
    public int aniStep = 0; 
    //���������������
    public int boardNumber;
    //������ÿ�������������Ŀ
    public int[] boardType;
    //�����������˳��
    public List<int> boardOrder;
    //ʵ����ʾ�а���������˳��
    public List<int> showBoardOrder;
    //�����ϵ�����
    public GameObject[] boardObject;
    //չʾ��Ϣ���������
    public Text informationText;
    void Start()
    {
        Dfs = GetComponent<DfsManager>();
    }
    //��ʼ����
    public void StartCalc()
    {
        if (!isLoaded)
        {
            ShowTipMessage.Tip.ShowTip("���ȼ�������");
            return;
        }
        int t = int.Parse(calcTime.text);
        if (calcTime.text == ""||t <= 0)
        {
            ShowTipMessage.Tip.ShowTip("ģ��ʱ�����ô���");
            return;
        }
        targetTime = t;
        cT = 0;
        dfs(0, 0, 0, 0, 0, 0, 0);
        string res = "";
        for (int i = 0; i < maxStep.Count; i++)
        {
            if (i != maxStep.Count - 1) res += maxStep[i].ToString() + "->";
            else res += maxStep[i].ToString();
        }
        print(res);
        string showText = "";
        showText += "DFSִ�д���:" + t.ToString() + "\n";
        showText += "DFS������:" + dfsMaxLength.ToString() + "\n";
        showText += "���������:" + maxStep.Count + "\n";
        showText += "�������Ž�:\n";
        showText += res;
        informationText.text = showText;
        isCalc = true;
    }
    //��������
    public void LoadData()
    {
        //���֮ǰ������
        if (cards != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].gameObject.SetActive(false);
            }
        }
        isLoaded = true;
        isCalc = false;
        //�洢���Ƶ�����
        cards = new List<Card>();
        pLevelMap = new List<List<int>>();
        mapList = new List<List<int>>();
        isOnMap = new List<bool>();
        //��ȡjson����
        JObject jsonData = MapDataGetter.mapData.jsonData;
        //����Json�е�levelData����(����ÿ�����Ƶ�λ��)
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
        //����blockTypeData����(����ÿ�ֿ��Ƶ�����)
        JObject typeData = JObject.Parse(jsonData["blockTypeData"].ToString());
        //�������ɿ�����������
        List<int> rList = new List<int>();
        foreach (KeyValuePair<string, JToken> m in typeData)
        {
            int typeNum = int.Parse(m.Key.ToString());
            int typeCount = int.Parse(m.Value.ToString()) * 3;
            for (int i = 0; i < typeCount; i++)
            {
                rList.Add(typeNum);
            }
        }

        //��������������
        for (int i = 0; i < rList.Count; i++)
        {
            int index = Random.Range(i, rList.Count);
            int tmp = rList[i];
            int ran = rList[index];
            rList[i] = ran;
            rList[index] = tmp;
        }

        int level = 1;
        int idx = 0;
        List<string> idList = new List<string>();
        foreach (KeyValuePair<string, JToken> m in levelData)
        {
            //��ȡ��������
            List<int> subLevelList = new List<int>();
            JArray subList = JArray.Parse(m.Value.ToString());
            foreach (JToken j in subList)
            {
                List<int> mapSubList = new List<int>();
                mapSubList.Add(level);
                mapSubList.Add(int.Parse(j["rolNum"].ToString()));
                mapSubList.Add(int.Parse(j["rowNum"].ToString()));
                subLevelList.Add(idx);
                mapSubList.Add(rList[idx++]);
                mapList.Add(mapSubList);
                idList.Add(j["id"].ToString());
                GameObject o = Instantiate(MapDataGetter.mapData.card, content.transform);
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[mapSubList[3] - 1];
                Card c = o.GetComponent<Card>();
                c.type = mapSubList[3] - 1;
                c.id = j["id"].ToString();
                c.xPos = mapSubList[1];
                c.yPos = mapSubList[2];
                c.status = Card.st.OnMap;
                c.level = level;
                c.cid = idx - 1;
                c.ShowId();
                //��Card��������б�
                cards.Add(o.GetComponent<Card>());
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
            pLevelMap.Add(subLevelList);
            level++;
        }
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(360, 300);
        for (int i = 0; i < mapList.Count; i++)
        {
            isOnMap.Add(true);
        }

        for (int i = 0; i < pLevelMap.Count; i++)
        {
            string s = "";
            for (int j = 0; j < pLevelMap[i].Count; j++)
            {
                s += pLevelMap[i][j].ToString() + " ";
            }
            print(s);
        }

        for (int i = 0; i < mapList.Count; i++)
        {
            string w = "";
            for (int j = 0; j < mapList[i].Count; j++)
            {
                w += mapList[i][j].ToString() + " ";
            }
            print(w);
        }
        UpdateAvailableShowInBoard(24);
    }

    void UpdateAvailableShowInBoard(int level)
    {
        for (int i = 0; i < level; i++)
        {
            for (int j = 0; j < pLevelMap[i].Count; j++)
            {
                if (cards[pLevelMap[i][j]].status == Card.st.OnBoard) continue;
                else
                {
                    cards[pLevelMap[i][j]].status = Card.st.onMapZ;
                    cards[pLevelMap[i][j]].GetComponent<Image>().color = Color.gray;
                    bool flag = false;
                    for (int m = i + 1; m < level; m++)
                    {
                        for (int n = 0; n < pLevelMap[m].Count; n++)
                        {
                            if (cards[pLevelMap[m][n]].status == Card.st.OnBoard) continue;
                            if (Mathf.Abs(mapList[pLevelMap[m][n]][1] - mapList[pLevelMap[i][j]][1]) < 8 &&
                                Mathf.Abs(mapList[pLevelMap[m][n]][2] - mapList[pLevelMap[i][j]][2]) < 8)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if(flag)break;
                    }

                    if (!flag)
                    {
                        cards[pLevelMap[i][j]].GetComponent<Image>().color = Color.white;
                        cards[pLevelMap[i][j]].status = Card.st.OnMap;
                    }
                }
            }
        }
    }

    void UpdateAvailable(int level)
    {
        avMap = new List<int>();
        for (int i = 0; i < level; i++)
        {
            for (int j = 0; j < pLevelMap[i].Count; j++)
            {
                if (!isOnMap[pLevelMap[i][j]]) continue;
                else
                {
                    bool flag = false;
                    for (int m = i + 1; m < level; m++)
                    {
                        for (int n = 0; n < pLevelMap[m].Count; n++)
                        {
                            if (!isOnMap[pLevelMap[m][n]]) continue;
                            if (Mathf.Abs(mapList[pLevelMap[m][n]][1] - mapList[pLevelMap[i][j]][1]) < 8 &&
                                Mathf.Abs(mapList[pLevelMap[m][n]][2] - mapList[pLevelMap[i][j]][2]) < 8)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if(flag)break;
                    }
                    if(!flag)avMap.Add(pLevelMap[i][j]);
                }
            }
        }
    }
    public void StartAni()
    {
        if (!isLoaded)
        {
            ShowTipMessage.Tip.ShowTip("���ȼ�������");
            return;
        }
        if (!isCalc)
        {
            ShowTipMessage.Tip.ShowTip("���Ƚ��м���");
            return;
        }
        isLoaded = false;
        isCalc = false;
        //��ʼ������,ȫ������
        boardType = new int[GameManager.Gm.cards.Length];
        for (int i = 0; i < GameManager.Gm.cards.Length; i++)
        {
            boardType[i] = 0;
        }
        boardOrder = new List<int>();
        isOnShowMap = new List<bool>();
        for (int i = 0; i < cards.Count; i++)
        {
            isOnShowMap.Add(true);
        }

        for (int i = 0; i < boardObject.Length; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        aniStep = 0;
    }
    //��������Э��
    public void MainAni()
    {
        if (aniStep == maxStep.Count)
        {
            ShowTipMessage.Tip.ShowTip("�Ѿ������һ��");
            return;
        }
        if(GameManager.Gm.gameStatus != GameManager.GameStatus.PlayingA)return;
        //��ȡ��ǰ����Ļ�����Ϣ
        int cardId = maxStep[aniStep];
        int type = mapList[cardId][3] - 1;
        int level = mapList[cardId][0];
        cards[cardId].DfsDown();
        isOnShowMap[cardId] = false;
        UpdateAvailableShowInBoard(24);
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
            boardObject[orderInBoard].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            //��֮���Sprite���¸�ֵ
            for (int i = 0; i < moveArray.Count; i++)
            {
                boardObject[orderInBoard + i + 1].GetComponent<Image>().sprite = GameManager.Gm.cards[moveArray[i]];
            }
        }

        //�����ƶ��������λ������
        Vector2 pos = boardObject[orderInBoard].transform.position;
        //�������ƶ�������
        cards[cardId].transform.DOMove(pos,1.0f);
        //��ʼЭ�̶�ʱ
        //��Ҫһ��Э���ڶ������Ž���֮��ִ��һЩ����
        boardType[type]++;
        bool isPop;
        if (boardType[type] == 3) isPop = true;
        else isPop = false;
        StartCoroutine(AfterMove(cards[cardId].gameObject,orderInBoard,type,isPop,orderInBoard));
        boardNumber++;
        
        //�������߼�
        if (boardType[type] == 3)
        {
            //��������
            boardType[type] = 0;
            boardNumber -= 3;
            boardOrder.Remove(type);
        }
        aniStep++;
        ShowTipMessage.Tip.ShowTip("��" + aniStep + "��/��" + maxStep.Count + "��");
    }
    IEnumerator AfterMove(GameObject o,int pos,int type,bool isPop,int orderInBoard)
    {
        //�ȴ�1.0��ִ��
        yield return new WaitForSeconds(1.0f);
        if (showBoardOrder.Count >= pos)
        {
            o.SetActive(false);
            boardObject[pos].GetComponent<Image>().sprite = GameManager.Gm.cards[type];
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
                    boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.cards[showBoardOrder[i]];
                }
                else
                {
                    boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
                }
            }

            yield break;
        }
    }
    void dfs(int pos1, int pos2, int pos3, int pos4, int pos5, int pos6, int pos7)
    {
        dfsMaxLength = Mathf.Max(dfsLength, dfsMaxLength);
        cT++;
        if (cT > targetTime) return;
        UpdateAvailable(24);
        //����avMap�е�����(���)
        List<int> cAvMap = new List<int>();
        for (int i = 0; i < avMap.Count; i++)
        {
            cAvMap.Add(avMap[i]);
        }
        //�������Ŀ��Ƹ���
        int boardCount = 7;
        List<int> posList = new List<int>(){pos1,pos2,pos3,pos4,pos5,pos6,pos7};
        for (int i = 0; i < 7; i++)
        {
            if (posList[i] == 0)
            {
                boardCount = i;
                break;
            }
        }
        //�жϸ���������״̬
        if (boardCount == 7)
        {
            if (step.Count >= maxStep.Count)
            {
                maxStep = new List<int>();
                for (int i = 0; i < step.Count; i++)
                {
                    maxStep.Add(step[i]);
                }
            }
            return;
        }
        //��������и��࿨�Ƶĳ��ִ���
        Dictionary<int, int> boardTypeCount = new Dictionary<int, int>();
        for (int i = 0; i < 7; i++)
        {
            if (!boardTypeCount.ContainsKey(posList[i]))
            {
                boardTypeCount.Add(posList[i],1);
            }
            else
            {
                boardTypeCount[posList[i]]++;
            }
        }
        if (boardTypeCount.Count == 6)return;
        //���ݳ��ϵĿ������ɿ������͵ļ���
        List<int> curTypeList = new List<int>();
        for (int i = 0; i < cAvMap.Count; i++)
        {
            curTypeList.Add(mapList[cAvMap[i]][3]);
        }
        //���㳡�ϸ����Ϳ��Ƶĳ��ִ���
        Dictionary<int, int> curTypeCounter = new Dictionary<int, int>();
        for (int i = 0; i < curTypeList.Count; i++)
        {
            if (!curTypeCounter.ContainsKey(curTypeList[i]))
            {
                curTypeCounter.Add(curTypeList[i],1);
            }
            else
            {
                curTypeCounter[curTypeList[i]]++;
            }
        }

        List<int> popCard = new List<int>();
        List<int> popIdList = new List<int>();
        if (boardCount <= 4)
        {
            //�ҵ���������������
            foreach (KeyValuePair<int,int> i in curTypeCounter)
            {
                if(i.Value >= 3)popCard.Add(i.Key);
            }
            //�ҵ������������͵Ŀ���
            for (int i = 0; i < popCard.Count; i++)
            {
                int tcCount = 0;
                for (int j = 0; j < cAvMap.Count; j++)
                {
                    if(tcCount == 3)break;
                    if (mapList[cAvMap[j]][3] == popCard[i])
                    {
                        tcCount = tcCount + 1;
                        popIdList.Add(cAvMap[j]);
                    }
                }
            }
            //ִ����������
            for (int i = 0; i < popIdList.Count; i++)
            {
                isOnMap[popIdList[i]] = false;
                step.Add(popIdList[i]);
            }
            UpdateAvailable(24);
            cAvMap = new List<int>();
            for (int i = 0; i < avMap.Count; i++)
            {
                cAvMap.Add(avMap[i]);
            }
        }

        Dictionary<int, int> scoreDict = new Dictionary<int, int>();
        for (int i = 0; i < cAvMap.Count; i++)
        {
            //���㿨�Ʊ����֮�����Ŀ�����Ŀ
            isOnMap[cAvMap[i]] = false;
            UpdateAvailable(mapList[cAvMap[i]][0]);
            int c = avMap.Count - cAvMap.Count;
            //���㵱ǰ�����Ǳ�ڼ�ֵ
            List<int> pTypeList = new List<int>();
            for (int j = 0; j < avMap.Count; j++)
            {
                pTypeList.Add(mapList[avMap[j]][3]);
            }
            Dictionary<int, int> pTypeCount = new Dictionary<int, int>();
            for (int j = 0; j < pTypeList.Count; j++)
            {
                if (!pTypeCount.ContainsKey(pTypeList[j]))
                {
                    pTypeCount.Add(pTypeList[j],1);
                }
                else
                {
                    pTypeCount[pTypeList[j]]++;
                }
            }
            //���ݿ�����������Ŀ��������Ŀ����÷�
            int p = 0;
            foreach (KeyValuePair<int,int> j in pTypeCount)
            {
                if (j.Value == 2) p += 100;
                else if (j.Value >= 3) p += 1000; 
            }
            //���㳡�ϵĿ�������濨�Ƶ�ƥ�����
            foreach (KeyValuePair<int,int> j in pTypeCount)
            {
                if (boardTypeCount.ContainsKey(j.Key))
                {
                    if (j.Value + boardTypeCount[j.Key] == 2) p += 100;
                    else if (j.Value + boardTypeCount[j.Key] >= 3) p += 1000;
                }
            }
            //�ָ�״̬
            isOnMap[cAvMap[i]] = true;
            //�����ܵ÷�
            int cType = 0;
            if(boardTypeCount.ContainsKey(mapList[cAvMap[i]][3]))cType = boardTypeCount[mapList[cAvMap[i]][3]];
            int score = boardScore[cType] + p * 2;
            scoreDict.Add(cAvMap[i],score);
        }
        //Ϊ����ĵ÷�����
        Dictionary<int,int> sortedDict = scoreDict.OrderBy(x => x.Value).ToDictionary(x => x.Key,x=>x.Value);
        //����״̬��ʼdfs
        foreach (KeyValuePair<int, int> i in sortedDict)
        {
            int cardId = i.Key;
            int cardType = mapList[cardId][3];
            isOnMap[cardId] = false;
            UpdateAvailable(mapList[cardId][0]);
            List<int> tPosList = new List<int>();
            for (int j = 0; j < posList.Count; j++)
            {
                tPosList.Add(posList[j]);
            }
            if (!boardTypeCount.ContainsKey(cardType)||boardTypeCount[cardType] != 2)
            {
                tPosList[boardCount] = cardType;
            }
            else
            {
                tPosList.Remove(cardType);
                tPosList.Remove(cardType);
                tPosList.Add(0);
                tPosList.Add(0);
            }
            step.Add(cardId);
            dfsLength++;
            dfs(tPosList[0],tPosList[1],tPosList[2],tPosList[3],tPosList[4],tPosList[5],tPosList[6]);
            dfsLength--;
            isOnMap[cardId] = true;
            step.RemoveAt(step.Count - 1);
        }
        for (int i = 0; i < popIdList.Count; i++)
        {
            isOnMap[popIdList[i]] = true;
            step.RemoveAt(step.Count - 1);
        }
    }

    public void BackToMenu()
    {
        GameManager.Gm.gameStatus = GameManager.GameStatus.OnMenu;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }
        cards = new List<Card>();
        for (int i = 0; i < 7; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        GameManager.Gm.playingPanelA.SetActive(false);
        GameManager.Gm.menuPanel.SetActive(true);
    }
}
