using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;

    public GameObject[] Items;
    public int[] itemPrice;
    public Transform[] itemPos;
    public Text NPCText;
    public string[] talkData;

    Player enterPlayer;

    public void Enter(Player player)
    {
       enterPlayer = player;
       uiGroup.anchoredPosition = Vector3.zero; //ȭ�� �߾�����
    }
	public void Exit()
    {
        anim.SetTrigger("doHello");
		uiGroup.anchoredPosition = Vector3.down * 1000;
	}

    public void Buy(int index)
    {
        int price = itemPrice[index];
        if(price > enterPlayer.coin)
        {
			StopCoroutine("Talk");
			StartCoroutine("Talk");
            return;
        }

        enterPlayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                         + Vector3.forward * Random.Range(-3,3);
        Instantiate(Items[index], itemPos[index].position + ranVec, itemPos[index].rotation);
    }

    IEnumerator Talk()
    {
		NPCText.text = talkData[1];
		yield return new WaitForSeconds(2f);
		NPCText.text = talkData[0];
	}
}
