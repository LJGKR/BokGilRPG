using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
	public GameObject[] weapons;
	public bool[] hasWeapons;
	public GameObject[] grenades;
	public int hasGrenades;
	public GameObject grenadeObj;
	public Camera followCamera;
	public GameManager manager;

	public int ammo;
	public int coin;
	public int health;
	public int score;
	

	public int maxAmmo;
	public int maxCoin;
	public int maxHealth;
	public int maxHasGrenades;

	float h;
    float v;

    bool wDown; //이동
	bool jDown; //점프
	bool fDown; //공격
	bool gDown; //수류탄
	bool rDown; //재장전
	bool iDown; //상호작용
	bool sDown1; //무기 스왑1
	bool sDown2; //무기 스왑2
	bool sDown3; //무기 스왑3

	bool isJump;
	bool isDodge;
	bool isSwap;
	bool isFireReady = true;
	bool isReload = false;
	bool isBorder;
	bool isDamage;
	bool isShop;
	bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
	Rigidbody rigid;
	MeshRenderer[] meshs;

	GameObject nearObject;
	public Weapon curWeapon;
	int curWeaponIndex = -1;
	float fireDelay; //공격 딜레이

	void Awake()
	{
		anim = GetComponentInChildren<Animator>();
		rigid = GetComponent<Rigidbody>();
		meshs = GetComponentsInChildren<MeshRenderer>();
		isDead = false;
	}

	void Update()
    {
		GetInput();
		Move();
		Turn();
		Jump();
		Attack();
		Grenade();
		Reload();
		Dodge();
		Swap();
		Interaction();
	}

    void GetInput()
    {
		h = Input.GetAxisRaw("Horizontal");
		v = Input.GetAxisRaw("Vertical");
		wDown = Input.GetButton("Walk");
		jDown = Input.GetButtonDown("Jump");
		fDown = Input.GetButton("Fire1");
		gDown = Input.GetButtonDown("Fire2");
		rDown = Input.GetButtonDown("Reload");
		iDown = Input.GetButtonDown("Interaction");
		sDown1 = Input.GetButtonDown("Swap1");
		sDown2 = Input.GetButtonDown("Swap2");
		sDown3 = Input.GetButtonDown("Swap3");
	}

	void Move()
	{
		moveVec = new Vector3(h, 0, v).normalized; //대각선 이동 보정을 위해

		if(isDodge)
			moveVec = dodgeVec;

		if (isSwap || isReload || !isFireReady || isDead)
			moveVec = Vector3.zero;

		if (!isBorder)
		{
			transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
		}

		anim.SetBool("isRun", moveVec != Vector3.zero);
		anim.SetBool("isWalk", wDown);
	}

	void Turn()
	{
		//캐릭터 회전
		transform.LookAt(transform.position + moveVec);

		//공격할 때 마우스 방향에 대한 회전
		if (fDown && !isShop && !isDead)
		{
			Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit, 100)) //out은 '함수 안'에서 사용되어 반환된 값을 '함수 밖'의 주어진 변수에 저장해준다.
			{
				Vector3 nextVec = rayHit.point - transform.position;
				nextVec.y = 0;
				transform.LookAt(transform.position + nextVec);
			}
		}
	}

	void Jump()
	{
		if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead)
		{
			rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
			anim.SetBool("isJump", true);
			anim.SetTrigger("doJump");
			isJump = true;
		}
	}

	void Attack()
	{
		if (curWeapon == null)
			return;

		fireDelay += Time.deltaTime;
		isFireReady = curWeapon.rate < fireDelay;

		if(fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
		{
			curWeapon.Use(); //코루틴 Swing() 포함 함수
			anim.SetTrigger(curWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
			fireDelay = 0;
		}
	}

	void Grenade()
	{
		if (hasGrenades == 0)
			return;

        if (gDown && !isReload && !isSwap && !isDead)
        {
			Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit, 100)) //out은 '함수 안'에서 사용되어 반환된 값을 '함수 밖'의 주어진 변수에 저장해준다.
			{
				Vector3 nextVec = rayHit.point - transform.position;
				nextVec.y = 10;

				GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
				Rigidbody rigid = instantGrenade.GetComponent<Rigidbody>();

				rigid.AddForce(nextVec, ForceMode.Impulse);
				rigid.AddTorque(Vector3.back * 10, ForceMode.Impulse);
				hasGrenades--;
				grenades[hasGrenades].SetActive(false);
			}
		}
    }

	void Reload()
	{
		if(curWeapon == null) return;

		if (curWeapon.type == Weapon.Type.Melee) return;

		if (ammo == 0) return;

		if(ammo == 8) return;

		if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead)
		{
			anim.SetTrigger("doReload");
			isReload = true;
			Invoke("ReloadOut", 2f);
		}
	}

	void ReloadOut()
	{
		int reAmmo = ammo < curWeapon.maxAmmo ? ammo : curWeapon.maxAmmo;
		curWeapon.curAmmo = reAmmo;
		ammo -= reAmmo;
		isReload = false;
	}

	void Dodge()
	{
		if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead)
		{
			dodgeVec = moveVec;
			speed *= 2;
			anim.SetTrigger("doDodge");
			isDodge = true;

			Invoke("DodgeOut", 0.5f);
		}
	}

	void DodgeOut()
	{
		speed *= 0.5f;
		isDodge = false;
	}

	void Swap()
	{
		if (sDown1 && (!hasWeapons[0] || curWeaponIndex == 0))
			return;
		if (sDown2 && (!hasWeapons[1] || curWeaponIndex == 1))
			return;
		if (sDown3 && (!hasWeapons[2] || curWeaponIndex == 2))
			return;

		int weaponIndex = -1;
		if (sDown1) weaponIndex = 0;
		if (sDown2) weaponIndex = 1;
		if (sDown3) weaponIndex = 2;

		if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isShop && !isDead)
		{
			if (curWeapon != null)
				curWeapon.gameObject.SetActive(false);

			curWeaponIndex = weaponIndex;
			curWeapon = weapons[weaponIndex].GetComponent<Weapon>();
			curWeapon.gameObject.SetActive(true);

			anim.SetTrigger("doSwap");
			isSwap = true;

			Invoke("swapOut", 0.4f);
		}
	}

	void swapOut()
	{
		isSwap = false;
	}

	void Interaction()
	{
		if(iDown && nearObject != null && !isJump && !isDodge && !isDead)
		{
			if(nearObject.tag == "Weapon")
			{
				Item item = nearObject.GetComponent<Item>();
				int weaponIndex = item.value;
				hasWeapons[weaponIndex] = true;

				Destroy(nearObject);
			}
			else if (nearObject.tag == "Shop")
			{
				Shop shop = nearObject.GetComponent<Shop>();
				shop.Enter(this);
				isShop = true;
			}
		}
	}

	void FreezeRotation()
	{
		rigid.angularVelocity = Vector3.zero; //항상 물리력에 의한 회전력을 0으로
	}

	void StopToWall()
	{
		Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
		isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
	}

	void FixedUpdate()
	{
		FreezeRotation();
		StopToWall();
	}

	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Floor")
		{
			anim.SetBool("isJump", false);
			isJump = false;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Item")
		{
			Item item = other.GetComponent<Item>();
			switch (item.type)
			{
				case Item.Type.Ammo:
					ammo += item.value;
					if (ammo > maxAmmo)
						ammo = maxAmmo;
					break;
				case Item.Type.Coin:
					coin += item.value;
					if (coin > maxCoin)
						coin = maxCoin;
					break;
				case Item.Type.Heart:
					health += item.value;
					if (health > maxHealth)
						health = maxHealth;
					break;
				case Item.Type.Grenade:
					grenades[hasGrenades].SetActive(true);
					hasGrenades += item.value;
					if (hasGrenades > maxHasGrenades)
						hasGrenades = maxHasGrenades;
					break;
			}
			Destroy(other.gameObject);
		}
		else if (other.gameObject.tag == "EnemyBullet")
		{
			if (!isDamage)
			{
				Bullet bullet = other.GetComponent<Bullet>();
				health -= bullet.damage;

				bool isBossAttack = other.name == "Boss Melee Area";
				StartCoroutine(OnDamage(isBossAttack));
			}

			if (other.GetComponent<Rigidbody>() != null)
			{
				Destroy(other.gameObject);
			}
		}
	}

	IEnumerator OnDamage(bool isBossAttack)
	{
		isDamage = true;
		foreach( MeshRenderer mesh in meshs)
		{
			mesh.material.color = Color.yellow;
		}

		if (isBossAttack)
		{
			rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
		}

		yield return new WaitForSeconds(1f);

		isDamage = false;
		foreach (MeshRenderer mesh in meshs)
		{
			mesh.material.color = Color.white;
		}

		if (isBossAttack)
		{
			rigid.velocity = Vector3.zero;
		}

		if(health <= 0)
		{
			OnDie();
		}

	}

	void OnDie()
	{
		gameObject.layer = 11;
		anim.SetTrigger("doDie");
		isDead = true;
		manager.GameOver();
	}

	void OnTriggerStay(Collider other)
	{
		if(other.tag == "Weapon" || other.tag == "Shop")
			nearObject = other.gameObject;
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Weapon")
			nearObject = null;
		else if(other.tag == "Shop")
		{
			Shop shop = other.GetComponent<Shop>();
			shop.Exit();
			isShop = false;
			nearObject = null;
		}

	}
}
