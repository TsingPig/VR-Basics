using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clock : MonoBehaviour
{
    public GameObject secHand;
    public GameObject minHand;
    public GameObject hrHand;
    string oldSeconds;

    // Update is called once per frame
    void Update()
    {
        secHand.transform.rotation = Quaternion.Euler(0, 0, System.DateTime.Now.Second);
        minHand.transform.rotation = Quaternion.Euler(0, 0, System.DateTime.Now.Minute);
        hrHand.transform.rotation = Quaternion.Euler(0, 0, System.DateTime.Now.Hour);
    }
}
