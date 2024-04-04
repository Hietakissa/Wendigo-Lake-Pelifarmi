using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine.Audio;
using HietakissaUtils;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UIManager : Manager
{
    const int CONST_CLUES_TO_SOLVE = 3;
    int solvedClues;

    [SerializeField] float typeSpeed;

    [Header("Dialogue")]
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TextMeshProUGUI dialogueText;

    [SerializeField] Image playerExpressionImage;
    [SerializeField] Image delilahExpressionImage;

    [Header("Puzzle")]
    [SerializeField] GameObject pauseUI;
    //[SerializeField] TextCollectionSO[] clueCompletionDialogues;

    //[SerializeField] DraggableClue[] imageClues;
    //[SerializeField] DraggableClue[] textClues;
    //List<DraggableClue> imageClues = new List<DraggableClue>();
    //List<DraggableClue> textClues = new List<DraggableClue>();


    List<DraggableClue> draggableClues = new List<DraggableClue>();

    Queue<TextCollectionSO> dialogueQueue = new Queue<TextCollectionSO>();
    Coroutine dialogueRoutine;

    Canvas canvas;

    [SerializeField] DraggableClue imageCluePrefab;
    [SerializeField] DraggableClue textCluePrefab;
    [SerializeField] Transform imageParent;
    [SerializeField] Transform textParent;

    [SerializeField] TextMeshProUGUI scuffedObjectivesText;
    [SerializeField] CanvasGroup fadeToBlackUI;
    [SerializeField] CanvasGroup wonGameUI;

    [SerializeField] JournalTab[] tabs;

    [SerializeField] AudioMixerGroup masterMixer;


    void Awake()
    {
        canvas = GetComponent<Canvas>();

        scuffedObjectivesText.text = $"Objectives:\r\n-Find clues {solvedClues}/{CONST_CLUES_TO_SOLVE}\r\n-Escape";
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
            Debug.Log($"Starting to display dialogue...");

            switch (dialogue.Speaker)
            {
                case Person.None:
                    playerExpressionImage.gameObject.SetActive(false);
                    delilahExpressionImage.gameObject.SetActive(false);
                    break;

                case Person.Me:
                    playerExpressionImage.gameObject.SetActive(true);
                    playerExpressionImage.sprite = dialogue.Expression.ExpressionSprite;

                    delilahExpressionImage.gameObject.SetActive(false);
                    break;

                case Person.Delilah:
                    playerExpressionImage.gameObject.SetActive(false);

                    delilahExpressionImage.gameObject.SetActive(true);
                    delilahExpressionImage.sprite = dialogue.Expression.ExpressionSprite;
                    break;
            }

            int endIndex = 0;
            int maxIndex = dialogue.Text.Length;
            float typeTime = 0f;
            while (true)
            {
                Debug.Log($"Writing text: {((float)endIndex / maxIndex * 100f).RoundDown()}%");

                //typeTime += typeSpeed * Time.unscaledDeltaTime;
                typeTime += typeSpeed * Time.deltaTime;

                int newIndices = typeTime.RoundDown();
                typeTime -= newIndices;
                endIndex = Mathf.Min(maxIndex, endIndex + newIndices);

                //monologueText.text = $"{(dialogue.speaker == Person.None ? "" : $"{dialogue.speaker}: ")}{dialogue.Text.Substring(0, endIndex)}";
                //dialogueText.text = $"{FormatSpeaker(dialogue.speaker)}{dialogue.Text.Substring(0, endIndex)}";
                dialogueText.text = $"{dialogue.Text.Substring(0, endIndex)}";
                if (endIndex == maxIndex) break;
                yield return null;
            }

            //yield return QOL.GetUnscaledWaitForSeconds(3f);
            yield return QOL.GetWaitForSeconds(3f);
        }
    }

    //string FormatSpeaker(Person person) => $"{(person == Person.None ? "" : $"{person}: ")}";

    void EventManager_OnEndDrag(DraggableClue clue)
    {
        //if (clue.clueData.GetClueType != ClueType.Text) return;

        Debug.Log($"draggable clues: {draggableClues.Count}");
        for (int i = draggableClues.Count - 1; i >= 0; i--)
        {
            DraggableClue currentClue = draggableClues[i];

            if (TypeMatch(clue, currentClue)) continue;
            if (!IDMatch(clue, currentClue)) continue;
            if (CheckForOverlap((RectTransform)currentClue.transform, (RectTransform)clue.transform))
            {
                //TextCollectionSO clueCompletion = clueCompletionDialogues[clue.clueData.ID];
                Debug.Log($"Checking dragged: {clue.gameObject.name}, other: {currentClue.gameObject.name}");

                TextCollectionSO clueCompletion = clue.clueData.CompletionDialogue;
                if (clueCompletion) EventManager.UI.PlayDialogue(clueCompletion);

                Debug.Log($"removing {clue.gameObject.name} and {currentClue.gameObject.name}");

                Destroy(clue.gameObject);
                Destroy(currentClue.gameObject);
                draggableClues.Remove(clue);
                draggableClues.Remove(currentClue);
                solvedClues++;
                break;
            }
        }

        scuffedObjectivesText.text = $"Objectives:\r\n-Find clues {solvedClues}/{CONST_CLUES_TO_SOLVE}\r\n-Escape";
        if (solvedClues == CONST_CLUES_TO_SOLVE) EventManager.CollectedAllClues();


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

    //void EventManager_OnRegisterDraggableClue(DraggableClue clue) => draggableClues.Add(clue);

    void UI_OnUnlockClue(ClueSO clue)
    {
        Debug.Log($"unlock clue: {clue.GetClueType}{clue.ID}");

        float x = Random.Range(-(Screen.width * 0.8f * 0.5f), Screen.width * 0.8f * 0.5f);
        float y = Random.Range(-(Screen.height * 0.8f * 0.5f), Screen.height * 0.8f * 0.5f);
        Debug.Log($"width: {Screen.width}, x: {x}, height: {Screen.height}, y: {y}");

        DraggableClue draggableClue = null;
        switch (clue.GetClueType)
        {
            case ClueType.Image:
                ImageClueSO imageClue = clue as ImageClueSO;

                draggableClue = Instantiate(imageCluePrefab, imageParent);
                draggableClue.SetClueData(clue);
                draggableClue.Image.sprite = imageClue.Image;
                ((RectTransform)draggableClue.transform).sizeDelta = new Vector2(imageClue.Image.rect.width, imageClue.Image.rect.height) * 0.5f;
                break;

            case ClueType.Text:
                draggableClue = Instantiate(textCluePrefab, textParent);
                draggableClue.SetClueData(clue);
                draggableClue.Text.text = (clue as TextClueSO).Text;
                break;
        }

        if (draggableClue != null)
        {
            draggableClues.Add(draggableClue);
            //draggableClue.transform.position = new Vector3(x, y, draggableClue.transform.position.z);
            ((RectTransform)draggableClue.transform).anchoredPosition = new Vector2(x, y);
        }
        else Debug.Log("COULD NOT CREATE UI CLUE ELEMENT FOR SOME REASON.");
        

        //if (clue.GetClueType == ClueType.Image && imageClues[clue.ID]) imageClues[clue.ID].gameObject.SetActive(true);
        //else if (clue.GetClueType == ClueType.Text && textClues[clue.ID]) textClues[clue.ID].gameObject.SetActive(true);
    }


    public void OpenTab(JournalTab tab)
    {
        foreach (JournalTab journalTab in tabs)
        {
            journalTab.gameObject.SetActive(journalTab.TabName == tab.TabName);
        }
    }


    public void ChangeSensitivity(float sensitivity)
    {
        GameManager.Instance.Sensitivity = sensitivity;
    }

    public void ChangeVolume(float volume)
    {
        masterMixer.audioMixer.SetFloat("Master Volume", Maf.ReMap(0f, 100f, -20f, 20f, volume));
    }

    int[] fpsCap = new int[] { 0, 30, 60, 90, 120, 144, 240};
    public void ChangeFPSCap(int index)
    {
        if (index == 7)
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
        else Application.targetFrameRate = fpsCap[index];
    }


    void EventManager_OnPause()
    {
        pauseUI.SetActive(true);
    }

    void EventManager_OnUnPause()
    {
        pauseUI.SetActive(false);
    }

    void EventManager_OnPlayerDied()
    {
        StartCoroutine(PlayerDiedCor());
    }

    IEnumerator PlayerDiedCor()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;
            float progress = time / 2.7f;
            fadeToBlackUI.alpha = progress;

            if (progress >= 1f) break;
            yield return null;
        }
    }

    void EventManager_OnWonGame()
    {
        StartCoroutine(WonGameCor());
    }

    IEnumerator WonGameCor()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;
            float progress = time / 2.7f;
            wonGameUI.alpha = progress;

            if (progress >= 1f) break;
            yield return null;
        }
    }


    void OnEnable()
    {
        EventManager.UI.OnPlayDialogue += EventManager_OnPlayDialogue;
        EventManager.UI.OnEndDrag += EventManager_OnEndDrag;

        //EventManager.UI.OnRegisterDraggableClue += EventManager_OnRegisterDraggableClue;

        EventManager.UI.OnUnlockClue += UI_OnUnlockClue;

        EventManager.OnPause += EventManager_OnPause;
        EventManager.OnUnPause += EventManager_OnUnPause;

        EventManager.OnPlayerDied += EventManager_OnPlayerDied;

        EventManager.OnWonGame += EventManager_OnWonGame;
    }

    void OnDisable()
    {
        EventManager.UI.OnPlayDialogue -= EventManager_OnPlayDialogue;
        EventManager.UI.OnEndDrag -= EventManager_OnEndDrag;

        //EventManager.UI.OnRegisterDraggableClue -= EventManager_OnRegisterDraggableClue;

        EventManager.UI.OnUnlockClue -= UI_OnUnlockClue;

        EventManager.OnPause -= EventManager_OnPause;
        EventManager.OnUnPause -= EventManager_OnUnPause;

        EventManager.OnPlayerDied -= EventManager_OnPlayerDied;

        EventManager.OnWonGame -= EventManager_OnWonGame;
    }
}
