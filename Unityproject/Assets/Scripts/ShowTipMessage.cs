using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShowTipMessage : MonoBehaviour
{
    public GameObject tipMessage;
    public GameObject tipContent;
    //�ö��д�����õ�Tip����
    public Queue<GameObject> TipObjects;
    public static ShowTipMessage Tip;

    public void Init()
    {
        Tip = GetComponent<ShowTipMessage>();
        TipObjects = new Queue<GameObject>();
    }

    public void ShowTip(string tipString)
    {
        if (TipObjects.Count == 0)
        {
            //�����µ���Ϣ��ʾ
            GameObject o = Instantiate(tipMessage,tipContent.transform);
            o.GetComponentInChildren<Text>().text = tipString;
            StartCoroutine(WaitAnimation(o));
        }
        else
        {
            GameObject o = TipObjects.Dequeue();
            o.GetComponentInChildren<Text>().text = tipString;
            o.SetActive(true);
            StartCoroutine(WaitAnimation(o));
        }
    }
    //����Э�̵ȴ�����֮���Զ�����
    IEnumerator WaitAnimation(GameObject o)
    {
        yield return new WaitForSeconds(1.0f);
        o.SetActive(false);
        TipObjects.Enqueue(o);
        yield break;
    }
}
