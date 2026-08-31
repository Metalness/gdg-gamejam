using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextSequence : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text outputText;
    public List<string> texts = new List<string>();

    [Header("Timing")]
    public float timePerWord = 0.15f;

    [Header("Scene")]
    public string nextScene;

    void Start()
    {
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        foreach (string line in texts)
        {
            outputText.text = line;

            int wordCount = line.Split(
                new char[] { ' ', '\n', '\t' },
                System.StringSplitOptions.RemoveEmptyEntries
            ).Length;

            yield return new WaitForSeconds(wordCount * timePerWord);
        }

        outputText.text = "";

        SceneManager.LoadScene(nextScene);
    }
}