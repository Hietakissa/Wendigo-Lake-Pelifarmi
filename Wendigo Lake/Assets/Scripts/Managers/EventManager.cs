using UnityEngine;
using System;

public static class EventManager
{
    public static void EnterCamera() => OnEnteredCamera?.Invoke();
    public static event Action OnEnteredCamera;

    public static void ExitCamera() => OnExitCamera?.Invoke();
    public static event Action OnExitCamera;


    public static void Pause() => OnPause?.Invoke();
    public static event Action OnPause;
    public static void UnPause() => OnUnPause?.Invoke();
    public static event Action OnUnPause;


    public static void PlaySoundAtPosition(SoundCollectionSO soundCollection, Vector3 position) => OnPlaySoundAtPosition?.Invoke(soundCollection, position);
    public static event Action<SoundCollectionSO,Vector3> OnPlaySoundAtPosition;


    public static void PlayerDied() => OnPlayerDied?.Invoke();
    public static event Action OnPlayerDied;

    public static void CollectedAllClues() => OnCollectedAllClues?.Invoke();
    public static event Action OnCollectedAllClues;

    public static void WonGame() => OnWonGame?.Invoke();
    public static event Action OnWonGame;


    public static class UI
    {
        public static void PlayDialogue(TextCollectionSO textCollection) => OnPlayDialogue?.Invoke(textCollection);
        public static event Action<TextCollectionSO> OnPlayDialogue;

        public static void EndDrag(DraggableClue clue) => OnEndDrag?.Invoke(clue);
        public static event Action<DraggableClue> OnEndDrag;

        public static void RegisterDraggableClue(DraggableClue clue) => OnRegisterDraggableClue?.Invoke(clue);
        public static event Action<DraggableClue> OnRegisterDraggableClue;

        public static void UnlockClue(ClueSO clue) => OnUnlockClue?.Invoke(clue);
        public static event Action<ClueSO> OnUnlockClue;
    }

    public static class Photography
    {
        public static void PhotographDeer(DeerAI deer) => OnPhotographDeer?.Invoke(deer);
        public static event Action<DeerAI> OnPhotographDeer;

        public static void DeerDied(DeerAI deer) => OnDeerDied?.Invoke(deer);
        public static event Action<DeerAI> OnDeerDied;
    }
}
