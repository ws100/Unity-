using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegManger : MonoBehaviour
{
    public Text userNameInput;
    public Text passWordInput;
    public Text passWordInputV;
    public static RegManger regManger;

    private void Awake()
    {
        regManger = GetComponent<RegManger>();
    }

    public void Reg()
    {
        string userName = userNameInput.text;
        string passWord = passWordInput.text;
        string passWordV = passWordInputV.text;
        if (userName == "")
        {
            ShowTipMessage.Tip.ShowTip("�û���Ϊ��");
            return;
        }
        if (passWord == "")
        {
            ShowTipMessage.Tip.ShowTip("����Ϊ��");
            return;
        }
        if (passWord != passWordV)
        {
            ShowTipMessage.Tip.ShowTip("�����������벻һ��");
            return;
        }

        HttpServer.hts.Reg(userName, passWord);
        
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }

    public void RegBack(string res,string userName)
    {
        if (res == "ע��ɹ�")
        {
            GameManager.Gm.userId = userName;
            GameManager.Gm.isLogin = true;
            ShowTipMessage.Tip.ShowTip("ע��ɹ�");
            gameObject.SetActive(false);
        }
        else if (res == "�û��Ѵ���")
        {
            ShowTipMessage.Tip.ShowTip("�û��Ѵ���");
        }
        else
        {
            ShowTipMessage.Tip.ShowTip("δ֪����");
        }
    }
}
