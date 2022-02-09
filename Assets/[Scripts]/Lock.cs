using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lock : MonoBehaviour
{
    public LockNum lockNo;

    public float selectedAngle;

    // Start is called before the first frame update
    void Start()
    {
        selectedAngle = Random.Range(1f, 360f);


        LockPickManager.Instance().onSetLockColor += OnColorChange;
        LockPickManager.Instance().onResetLock += OnResetLock;
    }
    public void OnColorChange(LockNum lockNum, Color col)
    {
        if (lockNum == this.lockNo)
        {
            GetComponent<Image>().color = col;
        }
    }
    public void OnResetLock(LockNum lockNum)
    {
        float range = LockPickManager.Instance().GetRange();

        if (lockNum == this.lockNo)
        {
            selectedAngle = Random.Range(1f, 360f);

            float tempAngle = 0f;
            
            // Prevent the selected angle and target angle from being the same
            do
            {
                tempAngle = Random.Range(1f, 360f);
            } while (selectedAngle <= tempAngle + range && selectedAngle >= tempAngle - range);

            if (lockNum == LockNum.LockOne)
                transform.rotation = Quaternion.Euler(0f, 0f, tempAngle);

        }

    }
}
