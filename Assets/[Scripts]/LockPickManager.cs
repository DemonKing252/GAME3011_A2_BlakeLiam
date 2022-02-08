using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockPickManager : MonoBehaviour
{
    [SerializeField] private GameObject lock1; 
    [SerializeField] private GameObject lock2; 



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //float mouseX = Input.GetAxis("Mouse X");   
        //float mouseY = Input.GetAxis("Mouse Y");

        Vector3 localPosition = Input.mousePosition - lock1.GetComponent<RectTransform>().position;

        float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;

        lock1.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        //Debug.Log("Local mouse: " + localPosition.ToString());

    
    }
}
