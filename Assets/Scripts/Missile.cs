using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Missile : MonoBehaviour
{
    void Update()
    {
		transform.Rotate(Vector3.right * 30 * Time.deltaTime);
	}
}
