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


    public static class UI
    {
        public static void PlayDialogue(TextCollectionSO textCollection) => OnPlayDialogue?.Invoke(textCollection);
        public static event Action<TextCollectionSO> OnPlayDialogue;

        public static void EndDrag(DraggableClue clue) => OnEndDrag?.Invoke(clue);
        public static event Action<DraggableClue> OnEndDrag;

        public static void RegisterDraggableClue(DraggableClue clue) => OnRegisterDraggableClue?.Invoke(clue);
        public static event Action<DraggableClue> OnRegisterDraggableClue;
    }
}
