using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class MapDataGetter : MonoBehaviour
{
    public Camera mainCamera;
    public RectTransform canvasTransform;
    //�������洢��ͼ���ݵĵ�ַ
    public string MapJsonUrl = "http://43.139.170.217/ylgy/level1.json";
    //��������
    public static MapDataGetter mapData;
    //Json����
    public JObject jsonData;
    //���ɿ��Ƶĸ�����
    public GameObject content;
    //�����
    public GameObject[] level;
    //���Ƶ�����
    public List<Card> cards;
    //���Ƶ���Ϸ����
    public GameObject card;
    //������
    public int maxLevel;
    //��ʼ������
    public List<List<int>> LevelCard;
    public void Init()
    {
        //��ʼ��������
        mapData = GetComponent<MapDataGetter>();
        //��ʼ��ȡ��ͼ����
        StartCoroutine(Get());
    }
    //��ȡ����
    IEnumerator Get()
    {
        //����Get����
        UnityWebRequest request = UnityWebRequest.Get(MapJsonUrl);
        //��ӡ���������
        print(MapJsonUrl);
        //�ж��Ƿ��������
        yield return request.SendWebRequest();
        //��ӡ״̬��
        print(request.responseCode);
        //�ж�����ɹ�
        if(request.responseCode == 200)
        {
            //�����������ת����string
            string receiveContent = request.downloadHandler.text;
            //��string������json����
            jsonData = JObject.Parse(receiveContent);
            //��ӡ��������json����
            print(jsonData.ToString());
        }

    }
    

    //���ɿ��Ƶĺ���
    public void GenerateCards()
    {
        //��ʼ����������
        LevelCard = new List<List<int>>();
        //����Json�е�levelData����(����ÿ�����Ƶ�λ��)
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
        print(levelData.ToString());
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
        for (int i = 0; i < rList.Count; i++)
        {
            int index = Random.Range(i, rList.Count);
            int tmp = rList[i];
            int ran = rList[index];
            rList[i] = ran;
            rList[index] = tmp;
        }
        //��ӡ����֮�������
        print(rList.ToString());
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
                GameObject o = Instantiate(card,content.transform);
                //��ȡ���Ƶ�����
                int cardNum = rList[idx++] - 1;
                //��ȡ�����������Ӧ��ͼƬ
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[cardNum];
                //��ȡCard���
                Card c = o.GetComponent<Card>();
                //��Json����Ϣת����csv
                parserCsv += j["id"].ToString() + "," + level + "," + j["rowNum"] + "," + j["rolNum"] + "\n";
                //��������״̬
                c.type = cardNum;
                c.id = j["id"].ToString();
                c.xPos = int.Parse(j["rowNum"].ToString());
                c.yPos = int.Parse(j["rolNum"].ToString());
                c.status = Card.st.OnMap;
                c.level = level;
                c.SetButton();
                //��card����ż��������
                subLevelList.Add(idx-1);
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
            LevelCard.Add(subLevelList);
            level++;
        }
        maxLevel = level - 1;
        UpdateCardStatus(level-2);
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(360, 300);
        
    }
    //����Ϊ��ǰ����߲�
    public void UpdateCardStatus(int level)
    {
        //��ÿһ�����Ƹ���(����Ҫ���±ȵ�ǰ�߲�Ŀ���)
        for (int i = 0; i < level; i++)
        {
            for (int j = 0;j < LevelCard[i].Count; j++)
            {
                UpdateCardStatus(cards[LevelCard[i][j]]);
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
                Card tc = cards[LevelCard[i][j]];
                if(tc.status == Card.st.OnBoard)continue;
                //�жϿ����Ƿ����ڵ���Χ��
                if (Mathf.Abs(tc.xPos - c.xPos) < 8 && Mathf.Abs(tc.yPos - c.yPos) < 8)
                {
                    //���ñ��ڵ�֮���״̬
                    print("z");
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

    public void ResetCard()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }
        cards = new List<Card>();
    }
}
