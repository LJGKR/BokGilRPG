using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range};
    public Type type;
    public int damage; //공격력
    public float rate; //공격 속도
    public int maxAmmo;
    public int curAmmo;


    public BoxCollider meleeArea; //근접공격 판정 범위 콜라이더
    public TrailRenderer trailEffect; //적중 효과
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if(type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
			StartCoroutine("Shot");
        }
    }

	// Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴
	// Use() 메인루틴 + Swing() 코루틴 : 코루틴 함수는 이후에 실행되는것이 아닌 동시에 추가로 실행된다.
	IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f); // 1프레임 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;

		yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

		yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
	}

    IEnumerator Shot()
    {
        //총알 발사
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50; //앞으로 발사 이동

		yield return null;
        //탄피 배출
		GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
		Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
		caseRigid.AddForce(caseVec,ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
	}
}
