using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
	public enum Type { A,B,C,D};
	public Type enemyType;

    public int maxHealth;
    public int curHealth;
	public int score;
	public GameManager manager;
	public Transform target;
	public bool isChase;
	public BoxCollider meleeArea;
	public GameObject bullet;
	public GameObject[] coins;

	public bool isAttack;
	public bool isDead;

    protected Rigidbody rigid;
	protected BoxCollider boxCollider;
	protected MeshRenderer[] meshs;
	protected NavMeshAgent navAgent;
	protected Animator anim;

	void OnEnable()
	{
		navAgent.enabled = true;
	}

	void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		boxCollider = GetComponent<BoxCollider>();
		meshs = GetComponentsInChildren<MeshRenderer>();
		navAgent = GetComponent<NavMeshAgent>();
		anim = GetComponentInChildren<Animator>();

		if(enemyType != Type.D)
		{
			Invoke("ChaseStart", 2);
		}
	}

	void ChaseStart()
	{
		anim.SetBool("isWalk", true);
	}

	void Update()
	{
		if (navAgent.enabled && enemyType != Type.D) 
		{
			navAgent.SetDestination(target.position);
			navAgent.isStopped = !isChase;
		}
	}

	void FreezeVelocity()
	{
        if (isChase)
        {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero; 
		}
	}

	void Targeting()
	{
		if(!isDead && enemyType != Type.D)
		{
			float targetRadius = 0;
			float targetRange = 0;

			switch (enemyType)
			{
				case Type.A:
					targetRadius = 1.5f;
					targetRange = 3f;
					break;
				case Type.B:
					targetRadius = 1f;
					targetRange = 12f;
					break;
				case Type.C:
					targetRadius = 0.5f;
					targetRange = 30f;
					break;
			}

			RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
															targetRadius,
															transform.forward,
															targetRange,
															LayerMask.GetMask("Player"));

			if (rayHits.Length > 0 && !isAttack) //충돌된 오브젝트가 있다면
			{
				StartCoroutine("Attack");
			}
		}
	}

	IEnumerator Attack()
	{
		isChase = false;
		isAttack = true;
		anim.SetBool("isAttack", true);

		switch (enemyType)
		{
			case Type.A:
				yield return new WaitForSeconds(0.5f);
				meleeArea.enabled = true; //타격 범위 콜라이더 활성화

				yield return new WaitForSeconds(1f);
				meleeArea.enabled = false;

				yield return new WaitForSeconds(1f);
				break;
			case Type.B:
				yield return new WaitForSeconds(0.1f);
				rigid.AddForce(transform.forward * 30, ForceMode.Impulse);
				meleeArea.enabled = true;

				yield return new WaitForSeconds(0.5f);
				rigid.velocity = Vector3.zero;
				meleeArea.enabled = false;

				yield return new WaitForSeconds(2f);
				break;
			case Type.C:
				yield return new WaitForSeconds(0.5f);
				GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
				Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
				rigidBullet.velocity = transform.forward * 20;

				yield return new WaitForSeconds(2f);
				break;
		}

		isChase = true;
		isAttack = false;
		anim.SetBool("isAttack", false);
	}

	void FixedUpdate()
	{
		Targeting();
		FreezeVelocity();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Melee")
		{
			Weapon weapon = other.GetComponent<Weapon>();
			curHealth -= weapon.damage;
			Vector3 reactVec = transform.position - other.transform.position;

			StartCoroutine(OnDamage(reactVec,false));
		}
		else if(other.tag == "Bullet")
		{
			Bullet bullet = other.GetComponent<Bullet>();
			curHealth -= bullet.damage;
			Vector3 reactVec = transform.position - other.transform.position;
			Destroy(other.gameObject);

			StartCoroutine(OnDamage(reactVec,false));
		}
	}

	public void HitByGrenade(Vector3 explosionPos)
	{
		curHealth -= 100;
		Vector3 reactVec = transform.position - explosionPos;
		StartCoroutine(OnDamage(reactVec,true));
	}

	IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
	{
		yield return null;

		if(curHealth > 0)
		{
			foreach (MeshRenderer mesh in meshs)
			{
				mesh.material.color = Color.red;
			}
			yield return new WaitForSeconds(0.2f);

			foreach (MeshRenderer mesh in meshs)
			{
				mesh.material.color = Color.white;
			}
		}
		else
		{
			foreach (MeshRenderer mesh in meshs)
			{
				mesh.material.color = Color.gray;
			}
			gameObject.layer = 12;
			isDead = true;
			isChase = false;
			navAgent.enabled = false;
			anim.SetTrigger("doDie");
			Player player = target.GetComponent<Player>();
			player.score += score;
			int ranCoin = Random.Range(0, 3);
			Instantiate(coins[ranCoin],transform.position, Quaternion.identity);

			switch (enemyType)
			{
				case Type.A:
					manager.enemyA--;
					break;
				case Type.B:
					manager.enemyB--;
					break;
				case Type.C:
					manager.enemyC--;
					break;
				case Type.D:
					manager.enemyD--;
					break;
			}

			if (isGrenade)
			{
				reactVec = reactVec.normalized;
				reactVec += Vector3.up * 3;

				rigid.freezeRotation = false;
				rigid.AddForce(reactVec * 5, ForceMode.Impulse);
				rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
			}
			else
			{
				reactVec = reactVec.normalized;
				reactVec += Vector3.up;
				rigid.AddForce(reactVec * 5, ForceMode.Impulse);
			}
				Destroy(gameObject, 3);
		}
	}
}
