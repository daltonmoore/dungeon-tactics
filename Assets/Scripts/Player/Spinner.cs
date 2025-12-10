using System;
using System.Collections;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float spinRate;

    private void Update()
    {
        transform.rotation *= Quaternion.Euler(0, 0, spinRate * Time.deltaTime);
    }
}
