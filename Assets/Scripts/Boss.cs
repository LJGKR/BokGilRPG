using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;

    Vector3 lookVec;
    Vector3 tauntVec;
    public bool isLook;

	void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		boxCollider = GetComponent<BoxCollider>();
		meshs = GetComponentsInChildren<MeshRenderer>();
		navAgent = GetComponent<NavMeshAgent>();
		anim = GetComponentInChildren<Animator>();

		navAgent.isStopped = true;
		StartCoroutine("Think");
	}

	void Update()
	{
		if (isDead)
		{
			StopAllCoroutines();
			return;
		}

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
		else
		{
			navAgent.SetDestination(tauntVec);
		}
	}

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);
        int ranAction = Random.Range(0, 5);
        switch(ranAction)
        {
            case 0:
            case 1: //미사일 발사
				StartCoroutine("MissileShot");
				break;
			case 2:
            case 3: //돌 굴리기
				StartCoroutine("RockShot");
				break;
			case 4: //점프 공격
				StartCoroutine("Taunt");
				break;
		}
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
		yield return new WaitForSeconds(0.2f);
		GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
		BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
		bossMissileA.target = target;

		yield return new WaitForSeconds(0.3f);
		GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortA.rotation);
		BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
		bossMissileB.target = target;

		yield return new WaitForSeconds(2f);

		StartCoroutine("Think");
	}

	IEnumerator RockShot()
	{
		isLook = false;
		anim.SetTrigger("doRock");
		Instantiate(bullet, transform.position, transform.rotation);
		yield return new WaitForSeconds(3f);

		isLook = true;
		StartCoroutine("Think");
	}

	IEnumerator Taunt()
	{
		tauntVec = target.position + lookVec;

		isLook = false;
		navAgent.isStopped = false;
		boxCollider.enabled = false;
		anim.SetTrigger("doTaunt");

		yield return new WaitForSeconds(1.5f);
		meleeArea.enabled = true;

		yield return new WaitForSeconds(0.5f);
		meleeArea.enabled = false;

		yield return new WaitForSeconds(1f);
		isLook = true;
		navAgent.isStopped = true;
		boxCollider.enabled = true;
		StartCoroutine("Think");
	}
}
