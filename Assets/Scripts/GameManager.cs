using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public GameObject menuCam;
	public GameObject gameCam;
	public Player player;
	public Boss boss;
	public GameObject itemShop;
	public GameObject weaponShop;
	public GameObject startZone;
	public int stage;
	public float playTime;
	public bool isBattle;
	public int enemyA;
	public int enemyB;
	public int enemyC;
	public int enemyD;

	public Transform[] enemyZones;
	public GameObject[] enemies;
	public List<int> enemyList;

	//스테이지, 타임, 점수
	public GameObject menuPanel;
	public GameObject gamePanel;
	public GameObject overPanel;
	public Text maxScoreText;
	public Text scoreText;
	public Text stageText;
	public Text playTimeText;

	//플레이어 정보
	public Text playerHealthText;
	public Text playerAmmoText;
	public Text playerCoinText;

	//플레이어 무기
	public Image weaponImage1;
	public Image weaponImage2;
	public Image weaponImage3;
	public Image weaponImageR;

	//적의 수
	public Text enemyAText;
	public Text enemyBText;
	public Text enemyCText;

	//보스 UI
	public RectTransform bossBarTrans;
	public RectTransform bossBarHealth;
	public Text curScore;
	public Text bestScore;

	void Awake()
	{
		maxScoreText.text = string.Format("{0:n0}",PlayerPrefs.GetInt("MaxScore"));
		enemyList = new List<int>();

		if (PlayerPrefs.HasKey("MaxScore"))
		{
			PlayerPrefs.SetInt("MaxScore", 0);
		}
	}

	public void GameStart()
	{
		menuCam.SetActive(false);
		gameCam.SetActive(true);

		menuPanel.SetActive(false);
		gamePanel.SetActive(true);

		player.gameObject.SetActive(true);
	}

	public void GameOver()
	{
		gamePanel.SetActive(false);
		overPanel.SetActive(true);
		curScore.text = scoreText.text;

		int maxScore = PlayerPrefs.GetInt("MaxScore");
		if(player.score > maxScore)
		{
			bestScore.gameObject.SetActive(true);
			PlayerPrefs.SetInt("MaxScore", player.score);
		}
	}

	public void ReStart()
	{
		SceneManager.LoadScene(0);
	}

	public void StageStart()
	{
		itemShop.SetActive(false);
		weaponShop.SetActive(false);
		startZone.SetActive(false);

		foreach(Transform zone in enemyZones)
		{
			zone.gameObject.SetActive(true);
		}

		isBattle = true;
		StartCoroutine("InBattle");
	}

	public void StageEnd()
	{
		player.transform.position = Vector3.up * 0.61f;
		itemShop.SetActive(true);
		weaponShop.SetActive(true);
		startZone.SetActive(true);

		foreach (Transform zone in enemyZones)
		{
			zone.gameObject.SetActive(false);
		}

		isBattle = false;

		stage++;
	}

	IEnumerator InBattle()
	{
		if(stage % 5 == 0)
		{
			enemyD++;
			GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
			Enemy enemy = instantEnemy.GetComponent<Enemy>();
			enemy.target = player.transform;
			enemy.manager = this;
			boss = instantEnemy.GetComponent<Boss>();
		}
		else
		{
			for (int i = 0; i < stage; i++)
			{
				int ran = Random.Range(0, 3);
				enemyList.Add(ran);

				switch (ran)
				{
					case 0:
						enemyA++;
						break;
					case 1:
						enemyB++;
						break;
					case 2:
						enemyC++;
						break;
				}
			}

			while (enemyList.Count > 0)
			{
				int ranZone = Random.Range(0, 4);
				GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
				Enemy enemy = instantEnemy.GetComponent<Enemy>();
				enemy.target = player.transform;
				enemy.manager = this;
				enemyList.RemoveAt(0);
				yield return new WaitForSeconds(5);
			}
		}

		while(enemyA + enemyB + enemyC + enemyD > 0)
		{
			yield return null;
		}
		yield return new WaitForSeconds(4);

		boss = null;
		StageEnd();
	}

	void Update()
	{
		if(isBattle)
		{
			playTime += Time.deltaTime;
		}
	}
	void LateUpdate()
	{
		//점수. 스테이지
		scoreText.text = string.Format("{0:n0}", player.score);
		stageText.text = "STAGE " + stage;

		//플레이 타임
		int hour = (int)playTime / 3600;
		int min = (int)((playTime - hour * 3600) / 60);
		int second = (int)(playTime % 60);
		playTimeText.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

		//플레이어 상태
		playerHealthText.text = player.health + " / " + player.maxHealth;
		playerCoinText.text = string.Format("{0:n0}", player.coin);
		if(player.curWeapon == null)
		{
			playerAmmoText.text = "- / " + player.ammo;
		}
		else if(player.curWeapon.type == Weapon.Type.Melee){
			playerAmmoText.text = "- / " + player.ammo;
		}
		else
		{
			playerAmmoText.text = player.curWeapon.curAmmo + " / " + player.ammo;
		}

		//플레이어 무기
		weaponImage1.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
		weaponImage2.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
		weaponImage3.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
		weaponImageR.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

		//몬스터 숫자
		enemyAText.text = " x " + enemyA.ToString();
		enemyBText.text = " x " + enemyB.ToString();
		enemyCText.text = " x " + enemyC.ToString();

		//보스 체력
		if (boss != null)
		{
			bossBarTrans.anchoredPosition = Vector3.down * 30;
			bossBarHealth.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
		}
		else
		{
			bossBarTrans.anchoredPosition = Vector3.up * 200;
		}
	}
}
