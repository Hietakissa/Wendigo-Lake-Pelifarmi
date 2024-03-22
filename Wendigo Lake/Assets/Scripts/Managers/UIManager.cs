using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] float typeSpeed;

    [Header("References")]
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TextMeshProUGUI dialogueText;

    [SerializeField] GameObject puzzleUI;
    [SerializeField] TextCollectionSO[] clueCompletionDialogues;

    [SerializeField] DraggableClue[] imageClues;
    [SerializeField] DraggableClue[] textClues;


    List<DraggableClue> draggableClues = new List<DraggableClue>();

    Queue<TextCollectionSO> dialogueQueue = new Queue<TextCollectionSO>();
    Coroutine dialogueRoutine;

    Canvas canvas;


    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }


    void EventManager_OnPlayDialogue(TextCollectionSO textCollection)
    {
        Debug.Log($"Queueing dialogue.");
        dialogueQueue.Enqueue(textCollection);
        if (dialogueRoutine == null)
        {
            Debug.Log($"Dialogue routine not running, starting...");
            dialogueRoutine = StartCoroutine(PlayDialogue());
        }
    }

    IEnumerator PlayDialogue()
    {
        dialogueUI.SetActive(true);

        while (dialogueQueue.Count > 0)
        {
            TextCollectionSO textCollection = dialogueQueue.Dequeue();
            Debug.Log($"dequeued from dialogue queue, {dialogueQueue.Count} left");

            int maxDialogueIndex = textCollection.dialogue.Length;

            if (textCollection.mode == TextCollectionMode.Random)
            {
                yield return DisplayDialogue(textCollection.dialogue.RandomElement());
            }
            else if (textCollection.mode == TextCollectionMode.Sequential)
            {
                for (int i = 0; i < maxDialogueIndex; i++)
                {
                    yield return DisplayDialogue(textCollection.dialogue[i]);
                }
            }

            Debug.Log($"finished playing current dialogue, has more: {dialogueQueue.Count > 0}");

            if (dialogueQueue.Count > 0)
            {
                // only wait if there's more dialogue left

                

                dialogueUI.SetActive(false);
                yield return QOL.GetWaitForSeconds(3f);
                dialogueUI.SetActive(true);
            }
        }

        dialogueUI.SetActive(false);

        Debug.Log($"Dialogue routine finished successfully.");
        dialogueRoutine = null;


        IEnumerator DisplayDialogue(DialogueElement dialogue)
        {
            int endIndex = 0;
            int maxIndex = dialogue.Text.Length;
            float typeTime = 0f;
            while (true)
            {
                //typeTime += typeSpeed * Time.deltaTime;
                typeTime += typeSpeed * Time.unscaledDeltaTime;

                int newIndices = typeTime.RoundDown();
                typeTime -= newIndices;
                endIndex = Mathf.Min(maxIndex, endIndex + newIndices);

                //monologueText.text = $"{(dialogue.speaker == Person.None ? "" : $"{dialogue.speaker}: ")}{dialogue.Text.Substring(0, endIndex)}";
                dialogueText.text = $"{FormatSpeaker(dialogue.speaker)}{dialogue.Text.Substring(0, endIndex)}";
                if (endIndex == maxIndex) break;
                yield return null;
            }

            //yield return QOL.GetWaitForSeconds(3f);
            yield return QOL.GetUnscaledWaitForSeconds(3f);
        }
    }

    string FormatSpeaker(Person person) => $"{(person == Person.None ? "" : $"{person}: ")}";

    void EventManager_OnEndDrag(DraggableClue clue)
    {
        if (clue.clueData.GetClueType != ClueType.Text) return;

        foreach (DraggableClue currentClue in draggableClues)
        {
            if (TypeMatch(clue, currentClue)) continue;
            if (!IDMatch(clue, currentClue)) continue;
            if (CheckForOverlap((RectTransform)currentClue.transform, (RectTransform)clue.transform))
            {
                TextCollectionSO clueCompletion = clueCompletionDialogues[clue.clueData.ID];
                if (clueCompletion) EventManager.UI.PlayDialogue(clueCompletion);

                Destroy(clue.gameObject);
                Destroy(currentClue.gameObject);
            }
        }


        bool TypeMatch(DraggableClue a, DraggableClue b) => a.clueData.GetClueType == b.clueData.GetClueType;
        bool IDMatch(DraggableClue a, DraggableClue b) => a.clueData.ID == b.clueData.ID;

        bool CheckForOverlap(RectTransform rect, RectTransform draggedRect)
        {
            Vector3 rectPos = canvas.GetCanvasPositionForElement(rect);
            Vector3 point = canvas.GetCanvasPositionForElement(draggedRect);

            const float size = 0.5f;
            return point.x > rectPos.x - rect.sizeDelta.x * size && point.x < rectPos.x + rect.sizeDelta.x * size && point.y > rectPos.y - rect.sizeDelta.y * size && point.y < rectPos.y + rect.sizeDelta.y * size;
        }
    }

    void EventManager_OnRegisterDraggableClue(DraggableClue clue) => draggableClues.Add(clue);

    void UI_OnUnlockClue(ClueSO clue)
    {
        Debug.Log($"unlock clue: {clue.GetClueType}{clue.ID}");

        if (clue.GetClueType == ClueType.Image && imageClues[clue.ID]) imageClues[clue.ID].gameObject.SetActive(true);
        else if (clue.GetClueType == ClueType.Text && textClues[clue.ID]) textClues[clue.ID].gameObject.SetActive(true);
    }


    void EventManager_OnPause()
    {
        puzzleUI.SetActive(true);
    }

    void EventManager_OnUnPause()
    {
        puzzleUI.SetActive(false);
    }


    void OnEnable()
    {
        EventManager.UI.OnPlayDialogue += EventManager_OnPlayDialogue;
        EventManager.UI.OnEndDrag += EventManager_OnEndDrag;

        EventManager.UI.OnRegisterDraggableClue += EventManager_OnRegisterDraggableClue;

        EventManager.UI.OnUnlockClue += UI_OnUnlockClue;

        EventManager.OnPause += EventManager_OnPause;
        EventManager.OnUnPause += EventManager_OnUnPause;
    }

    void OnDisable()
    {
        EventManager.UI.OnPlayDialogue -= EventManager_OnPlayDialogue;
        EventManager.UI.OnEndDrag -= EventManager_OnEndDrag;

        EventManager.UI.OnRegisterDraggableClue -= EventManager_OnRegisterDraggableClue;

        EventManager.UI.OnUnlockClue -= UI_OnUnlockClue;

        EventManager.OnPause -= EventManager_OnPause;
        EventManager.OnUnPause -= EventManager_OnUnPause;
    }
}
