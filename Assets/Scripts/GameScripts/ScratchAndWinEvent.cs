using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class ScratchAndWinEvent : MonoBehaviour
{
    [Range(0f, 1f)] public float revealPercent = 0.6f;
    [Range(0f, 1f)] public float winRatio = 0.33f;
    public GameObject LosePanel;
    private ScratchAndWinController controller;
    private GameObject[] numbers = new GameObject[0];
    private bool win;
    private int winRandomNumber;

    void Start()
    {
        LosePanel.gameObject.SetActive(false);

        win = UnityEngine.Random.Range(0f, 1f) < winRatio;
        winRandomNumber = UnityEngine.Random.Range(1, 999);
        controller = GetComponent<ScratchAndWinController>();
        controller.OnSetPixelsBrush += SetPixelsBrush;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<Canvas>()) {
                numbers = GetArrayOfChildren(child.gameObject);
            }
        }
        SetNumbersRandom();
    }

    void OnWin(GameObject[] winners)
    {
        for (int i = 0; i < winners.Length; i++)
        {
            var winner = winners[i];
            var listOfChild = winner.GetComponentsInChildren<Animator>(true);
            if (listOfChild.Length == 0)
                continue;
            var child = listOfChild[0].gameObject;
            child.SetActive(true);
        }
    }

    void OnLose()
    {
        LosePanel.gameObject.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void SetPixelsBrush(float progress)
    {
        if (progress >= revealPercent)
        {
            controller.RevealCanvas();
            if (win)
            {
                var winners = new List<GameObject>();
                for (int i = 0; i < numbers.Length; i++)
                {
                    var number = numbers[i];
                    var text = number.GetComponentInChildren<TMP_Text>();
                    if (text.text == winRandomNumber.ToString())
                    {
                        winners.Add(number);
                    }
                }
                OnWin(winners.ToArray());
            }
            else
            {
                OnLose();
            }
        }
    }

    GameObject[] GetArrayOfChildren(GameObject root)
    {
        var list = new List<GameObject>();
        for (int i = 0; i < root.transform.childCount; i++)
        {
            var child = root.transform.GetChild(i);
            list.Add(child.gameObject);
        }
        return list.ToArray();
    }

    void SetNumbersRandom()
    {
        int[] winners = new int[3];
        if (win)
            winners = GetRandomUniqueNumbers(3, numbers.Length - 1);
        for (int i = 0; i < numbers.Length; i++)
        {
            var number = numbers[i];
            int randomNumber = UnityEngine.Random.Range(1, 999);
            if (win)
            {
                if (winners.Contains(i))
                {
                    randomNumber = winRandomNumber;
                }
            }
            var text = number.GetComponentInChildren<TMP_Text>();
            text.SetText(randomNumber.ToString());
        }
    }

    int[] GetRandomUniqueNumbers(int count, int max)
    {
        var numbers = new List<int>();
        while (numbers.Count < count)
        {
            int number = UnityEngine.Random.Range(0, max);
            if (!numbers.Contains(number))
                numbers.Add(number);
        }
        return numbers.ToArray();
    }
}
