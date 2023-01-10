using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class OnlineUserItem : MonoBehaviour
{
    public Text userNameText;
    public Image onlineImage;
    public Color busyColor;
    public Color normalColor;
    public Button connectBtn;
    public string userId;

    public void Init(string userName, bool isOnline)
    {
        userId = userName;
        if (userName == GameManager.Gm.userId) userName += "(��)";
        userNameText.text = userName;
        if (isOnline) onlineImage.color = normalColor;
        else onlineImage.color = busyColor;
        connectBtn.onClick.RemoveAllListeners();
        connectBtn.onClick.AddListener(delegate
        {
            if (DPlayManager.PlayManager.status != DPlayManager.WaitingStatus.Online) return;
            if(userId == GameManager.Gm.userId)ShowTipMessage.Tip.ShowTip("���ܺ��Լ�����");
            else
            {
                Client.clientManager.ConnectWithUser(userId);
                //ShowTipMessage.Tip.ShowTip("���û�"+userId+"�������ѷ���");
                //�򿪵ȴ����
                DPlayManager.PlayManager.WaitUser(userId);
            }
        });
    }
}
