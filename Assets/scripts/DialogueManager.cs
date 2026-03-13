using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public GameObject textbox;
    public GameObject athenaImage;
    public TextMeshProUGUI dialogueText;

    public string[] dialogues;

    public float typingSpeed = 0.03f;

    int index = 0;

    void Start()
    {
        StartDialogue();
    }

    void StartDialogue()
    {
        textbox.SetActive(true);
        athenaImage.SetActive(true);

        StartCoroutine(TypeSentence());
    }

    IEnumerator TypeSentence()
    {
        dialogueText.text = "";

        foreach (char letter in dialogues[index])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void NextDialogue()
    {
        if (dialogueText.text == dialogues[index])
        {
            index++;

            if (index < dialogues.Length)
            {
                StartCoroutine(TypeSentence());
            }
            else
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
        else
        {
            StopAllCoroutines();
            dialogueText.text = dialogues[index];
        }
    }

    public void SkipDialogue()
    {
        StopAllCoroutines();

        index = dialogues.Length - 1;
        dialogueText.text = dialogues[index];
    }
}