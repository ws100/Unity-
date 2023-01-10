using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    //���Ƶ�״̬
    public enum st { OnMap,OnBoard,onMapZ};
    //����
    public int level;
    //״̬
    public st status;
    public string id;
    public int type;
    public int xPos;
    public int yPos;
    public Button btn;
    public Text idText;
    public int cid;
    //�������Button���ע�����¼�
    public void SetButton()
    {
        btn.onClick.AddListener(MouseDown);
    }
    //����¼�����
    private void MouseDown()
    {
        if (GameManager.Gm.gameStatus != GameManager.GameStatus.PlayingS) return; 
        print("down");
        //�ж��Ƿ��ڵ�ͼ��
        if (status != st.OnMap) return;
        //����״̬
        status = st.OnBoard;
        //����λ���ƶ�����,�������ƶ���������
        GameManager.Gm.EnterBoard(type,gameObject,level);
        
    }

    public void DfsDown()
    {
        if(GameManager.Gm.gameStatus != GameManager.GameStatus.PlayingA)return;
        if(status != st.OnMap)return;
        status = st.OnBoard;
        //�ƶ��¼���dfs�ű���ִ��
    }

    public void ShowId()
    {
        idText.text = cid.ToString();
    }
}
