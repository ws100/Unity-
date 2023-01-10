using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoginManager : MonoBehaviour
{
    public Text userNameInput;
    public Text passWordInput;
    public static LoginManager loginManager;
    private void Awake()
    {
        loginManager = GetComponent<LoginManager>();
    }

    public void Login()
    {
        string userName = userNameInput.text;
        string passWord = passWordInput.text;
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
        HttpServer.hts.LogIn(userName, passWord);
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }

    public void LoginBack(string res,string userName)
    {
        if (res == "��¼�ɹ�")
        {
            GameManager.Gm.userId = userName;
            GameManager.Gm.isLogin = true;
            ShowTipMessage.Tip.ShowTip("��¼�ɹ�");
            gameObject.SetActive(false);
        }
        else if(res == "����ʧ��")
        {
            ShowTipMessage.Tip.ShowTip("�������");
        }
        else if (res == "�û�������")
        {
            ShowTipMessage.Tip.ShowTip("δ֪����");
        }
    }
}
