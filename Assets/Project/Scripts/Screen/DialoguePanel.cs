using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : BasePanel
{
    private DialogueData_SO CurrentDialogue;
    public Text CharacterName;
    public Text CharacterContent;
    public Button Next;
    private int CurrentIndex;

    private bool InputLocked = false;

    public override void Show()
    {
        base.Show();

        Next.onClick.AddListener(() =>
        {
            CurrentIndex++;
            if (CurrentIndex >= CurrentDialogue.Pieces.Count)
            {
                DialogueSystem.Instance.EndDialogue();
                return;
            }
            OnNext();
        });
    }

    public void Init(DialogueData_SO Data)
    {
        CurrentDialogue = Data;
        CurrentIndex = 0;
        OnNext();
    }

    private void OnNext()
    {
        InputLocked = true;

        CharacterName.text = CurrentDialogue.Pieces[CurrentIndex].CharacterName;
        StartCoroutine(IEShowText(CurrentDialogue.Pieces[CurrentIndex].CharacterContent));
    }

    private void Update()
    {
        if (Next.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            Next.onClick.Invoke();
        }
    }

    private bool IsInteractKey()
    {
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
    }

    private IEnumerator IEShowText(string txt)
    {
        Next.gameObject.SetActive(false);
        CharacterContent.text = "";

        yield return null;
        InputLocked = false;

        char[] chars = txt.ToCharArray();
        bool skip = false;

        foreach (char c in chars)
        {
            float timer = 0f;

            while (timer < 0.05f)
            {
                if (!InputLocked && IsInteractKey())
                {
                    skip = true;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (skip)
            {
                CharacterContent.text = txt;
                break;
            }
            else
            {
                CharacterContent.text += c;
            }
        }
        Next.gameObject.SetActive(true);
    }
}
