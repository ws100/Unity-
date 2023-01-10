using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DCard : Card
{
    public enum Dst
    {
        OnMap,
        OnMyBoard,
        OnOpBoard,
    }

    public Dst dStatus;
    public void SetButton()
    {
        btn.onClick.AddListener(MouseDown);
    }

    private void MouseDown()
    {
        //�жϿ����Ƿ��ڵ�ͼ��
        if (dStatus != Dst.OnMap) return;
        //�ж��Ƿ��ڱ�
        if (status != st.OnMap) return;
        //�ж���Ϸ�Ƿ�ʼ
        if (DPlayManager.PlayManager.status != DPlayManager.WaitingStatus.OnPlaying) return;
        //�ж��Ƿ����Լ��Ļغ�
        if (!DPlayManager.PlayManager.isMyTime) return;
        //������Ŀ��ƴ���������Ӧ�����
        DPlayManager.PlayManager.FetchCard(id,true);
        //������ID��������
        Client.clientManager.SendCardId(id);
        //����״̬
        status = st.OnBoard;
        dStatus = Dst.OnMyBoard;
        DPlayManager.PlayManager.isMyTime = false;
    }
}
