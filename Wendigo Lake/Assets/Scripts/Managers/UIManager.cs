using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] float typeSpeed;

    [SerializeField] GameObject monologue;
    [SerializeField] TextMeshProUGUI monologueText;

    Coroutine monologueRoutine;

    void EventManager_OnPlayMonologue(TextCollectionSO textCollection)
    {
        if (monologueRoutine != null) StopCoroutine(monologueRoutine);
        monologueRoutine = StartCoroutine(PlayMonologueCor(textCollection));
    }

    IEnumerator PlayMonologueCor(TextCollectionSO textCollection)
    {
        monologue.SetActive(true);

        int maxMonologueIndex = textCollection.texts.Length;

        string str = "";
        if (textCollection.mode == TextCollectionMode.Random)
        {
            str = textCollection.texts.RandomElement();
            yield return DisplayString(str);
        }
        else if (textCollection.mode == TextCollectionMode.Sequential)
        {
            for (int i = 0; i < maxMonologueIndex; i++)
            {
                str = textCollection.texts[i];
                yield return DisplayString(str);
            }
            //while (monologueIndex < maxMonologueIndex)
            //{
            //    str = textCollection.texts[monologueIndex];
            //    yield return DisplayString(str);
            //    monologueIndex++;
            //}
        }
        
        monologue.SetActive(false);


        IEnumerator DisplayString(string str)
        {
            int endIndex = 0;
            int maxIndex = str.Length;
            float typeTime = 0f;
            while (true)
            {
                typeTime += typeSpeed * Time.deltaTime;

                int newIndices = typeTime.RoundDown();
                typeTime -= newIndices;
                endIndex = Mathf.Min(maxIndex, endIndex + newIndices);

                monologueText.text = $"Me: {str.Substring(0, endIndex)}";
                if (endIndex == maxIndex) break;
                yield return null;
            }

            yield return QOL.GetWaitForSeconds(3f);
        }
    }


    void OnEnable() => EventManager.OnPlayMonologue += EventManager_OnPlayMonologue;
    void OnDisable() => EventManager.OnPlayMonologue -= EventManager_OnPlayMonologue;
}
