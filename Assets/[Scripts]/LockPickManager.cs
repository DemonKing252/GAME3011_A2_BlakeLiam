using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum LockNum
{
    LockOne,
    LockTwo
}
public enum Difficulty
{
    Easy,
    Normal,
    Hard,

    NumDifficulties
}
public enum Btn
{
    Start,
    PlayAgain,
    Quit,

}




public class LockPickManager : MonoBehaviour
{
    [SerializeField] private float startingTime = 180f;
    [SerializeField] private GameObject lock1; 
    [SerializeField] private GameObject lock2;


    [SerializeField] Canvas gameUI;
    [SerializeField] Canvas startCanvas;
    [SerializeField] Canvas winCanvas;
    [SerializeField] Canvas loseCanvas;

    [SerializeField]
    private float skill = 50f;

    [SerializeField] private TMP_Text lock1Status;
    [SerializeField] private TMP_Text lock2Status;

    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text gameStatus;

    [SerializeField] private TMP_Text playerSkillText;

    [SerializeField, Range(0f, 100f)]
    private float lockOnelerpSpeedAtSkillZero;

    [SerializeField, Range(0f, 100f)]
    private float lockOnelerpSpeedAtSkillOneHundred;


    [SerializeField, Range(0f, 100f)]
    private float lockTwolerpSpeedAtSkillZero;

    [SerializeField, Range(0f, 100f)]
    private float lockTwolerpSpeedAtSkillOneHundred;

    [SerializeField]
    private Difficulty difficulty;

    [SerializeField]
    private bool cheatMode = false;


    public delegate void SetLockColorDelegate(LockNum lockNum, Color col);
    public event SetLockColorDelegate onSetLockColor;

    public delegate void ResetLockDelegate(LockNum lockNum);
    public event ResetLockDelegate onResetLock;

    private bool lockOneControl = true;
    private bool lockTwoControl = false;

    private bool lockOneUnlocked = false;
    private bool lockTwoUnlocked = false;
    private float timeRemaining;

    [SerializeField]
    AudioSource lockPickAudioQueue;

    private bool alreadyPlayed = false;

    public float GetRange()
    {
        float range = difficulty switch
        {
            Difficulty.Easy     => 20f,
            Difficulty.Normal   => 10f,
            Difficulty.Hard     =>  5f,
            _ => throw new System.Exception("Unknown difficulty!")
        };
        return range;
    }

    private static LockPickManager instance = null;
    public static LockPickManager Instance() { return instance; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // duplicated lock pick manager
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        timeRemaining = startingTime;

        Time.timeScale = 0f;

    }

    // Update is called once per frame
    void Update()
    {

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, Mathf.Infinity);

        int realMinutes = Mathf.FloorToInt(timeRemaining / 60f);
        int realSeconds = (int)timeRemaining % 60;
        timeText.text = realMinutes + ":" + realSeconds.ToString("00");

        if (timeRemaining <= 0f)
        {
            loseCanvas.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }

        if (cheatMode)
        {
            Debug.Log(lock1.transform.rotation.eulerAngles.z + " - One - " + lock1.GetComponent<Lock>().selectedAngle + ", " +
            lock2.transform.rotation.eulerAngles.z + " - Two - " + lock2.GetComponent<Lock>().selectedAngle);
        }
        if (!lockOneUnlocked && lockOneControl)
        {
            Vector3 localPosition = Input.mousePosition - lock1.GetComponent<RectTransform>().position;
            float targetAngle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;

            // --------------------
            // 0 = 0.5
            // 
            // 100 = 5 

            float determinedLerpedSpeed = Mathf.Lerp(lockOnelerpSpeedAtSkillZero, lockOnelerpSpeedAtSkillOneHundred, skill / 100f);
            float eulerZCoord = lock1.transform.rotation.eulerAngles.z;
            eulerZCoord = Mathf.LerpAngle(eulerZCoord, targetAngle, determinedLerpedSpeed * Time.deltaTime);

            lock1.transform.rotation = Quaternion.Euler(0f, 0f, eulerZCoord);

            
            float range = GetRange();
            float selectedAngle = lock1.GetComponent<Lock>().selectedAngle;
            if (eulerZCoord >= selectedAngle - (range * 0.5f) && eulerZCoord <= selectedAngle + (range * 0.5f))
            {
                if (!alreadyPlayed)
                {
                    if (lockPickAudioQueue.isPlaying)
                        lockPickAudioQueue.Stop();

                    lockPickAudioQueue.Play();

                    alreadyPlayed = true;
                }
            }
            else
            {

                alreadyPlayed = false;
            }
            if (Input.GetMouseButtonDown(0) && Time.timeScale != 0f)
            {
                if (eulerZCoord >= selectedAngle - (range * 0.5f) && eulerZCoord <= selectedAngle + (range * 0.5f))
                {
                    Debug.Log("Picked locked successfully!");
                    lock1Status.text = "Lock #1 - <color=green>Unlocked</color>";
                    gameStatus.text = "Lock #1 picked successfully!";
                    onSetLockColor.Invoke(LockNum.LockOne, Color.green);
                    lockOneUnlocked = true;
                    lockTwoControl = true;
                    alreadyPlayed = false;
                }
                else
                {
                    timeRemaining -= 10f;
                    lock1Status.text = "Lock #1 - <color=red>Wrong Orientation</color>";
                    gameStatus.text = "Lock #1 picked unsuccessfully. 10s punishment";
                    onSetLockColor.Invoke(LockNum.LockOne, Color.red);
                    Debug.Log("Sorry, you didn't pick the lock!");
                    StartCoroutine(ResetLock(LockNum.LockOne));

                }



            }
        }
        if (!lockTwoUnlocked && lockTwoControl)
        {
            float hAxis = Input.GetAxis("Horizontal");

            float sensitivity = Mathf.Lerp(lockTwolerpSpeedAtSkillZero, lockTwolerpSpeedAtSkillOneHundred, skill / 100f);
            
            lock2.transform.rotation *= Quaternion.Euler(0f, 0f, hAxis * sensitivity * Time.deltaTime);
            
            
            float range = GetRange();
            float selectedAngle = lock2.GetComponent<Lock>().selectedAngle;
            float eulerZCoord = lock2.transform.rotation.eulerAngles.z;
            if (eulerZCoord >= selectedAngle - (range * 0.5f) && eulerZCoord <= selectedAngle + (range * 0.5f))
            {
                if (!alreadyPlayed)
                {
                    if (lockPickAudioQueue.isPlaying)
                        lockPickAudioQueue.Stop();

                    lockPickAudioQueue.Play();

                    alreadyPlayed = true;
                }
            }
            else
            {
                alreadyPlayed = false;

            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (eulerZCoord >= selectedAngle - (range * 0.5f) && eulerZCoord <= selectedAngle + (range * 0.5f))
                {
                    Debug.Log("Picked locked successfully!");
                    lock2Status.text = "Lock #2 - <color=green>Unlocked</color>";
                    gameStatus.text = "Lock #2 picked successfully!";
                    onSetLockColor.Invoke(LockNum.LockTwo, Color.green);
                    lockTwoUnlocked = true;

                    winCanvas.gameObject.SetActive(true);
                    Time.timeScale = 0f;
                }
                else
                {
                    timeRemaining -= 10f;
                    onSetLockColor.Invoke(LockNum.LockTwo, Color.red);
                    gameStatus.text = "Lock #2 picked unsuccessfully. 10s punishment";
                    Debug.Log("Sorry, you didn't pick the lock!");
                    StartCoroutine(ResetLock(LockNum.LockTwo));
                }
            }

        }

    }
    public void OnButtonPressed(int buttonKey)
    {
        // Can't have an enum as a parameter for a unity event unfortuntately
        Btn btnPressed = buttonKey switch 
        {
            0 => Btn.Start,
            1 => Btn.PlayAgain,
            2 => Btn.Quit,
            _ => throw new System.Exception("Unknown button key!")
        };

        switch (btnPressed)
        {
            case Btn.Start:
                playerSkillText.text = string.Format("Player Skill: {0}/100", skill.ToString("F1"));
                startCanvas.gameObject.SetActive(false);
                gameUI.gameObject.SetActive(true);
                Time.timeScale = 1f;
                break;
            case Btn.PlayAgain:
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                break;
            case Btn.Quit:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
                break;
        
        }


    }
    public void OnSliderFloat(float val)
    {
        skill = val;
    }
    public void OnSelection(System.Int32 index)
    {
        difficulty = (Difficulty)index;
    }

    public IEnumerator ResetLock(LockNum lockNo)
    {
        if (lockNo == LockNum.LockOne)
            lockOneControl = false;
        else if (lockNo == LockNum.LockTwo)
            lockTwoControl = false;

        float t = 0f;
        while (t <= 3f)
        {
            t += Time.deltaTime;

            Vector3 deltaR = new Vector3(0f, 0f, 1440f * Time.deltaTime);
            
            if (lockNo == LockNum.LockOne)
                lock1.transform.rotation *= Quaternion.Euler(deltaR);
            else if (lockNo == LockNum.LockTwo)
                lock2.transform.rotation *= Quaternion.Euler(deltaR);

            yield return null;
        }

        onSetLockColor.Invoke(lockNo, new Color(1f, 162f / 255f, 0f, 1f));
        onResetLock.Invoke(lockNo);

        if (lockNo == LockNum.LockOne)
            lockOneControl = true;
        else if (lockNo == LockNum.LockTwo)
            lockTwoControl = true;
    }
}
