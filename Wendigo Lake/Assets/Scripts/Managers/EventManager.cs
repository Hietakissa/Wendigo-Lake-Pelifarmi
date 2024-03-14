using System;

public static class EventManager
{
    public static void EnterCamera() => OnEnteredCamera?.Invoke();
    public static event Action OnEnteredCamera;

    public static void ExitCamera() => OnExitCamera?.Invoke();
    public static event Action OnExitCamera;

    public static void PlayMonologue(TextCollectionSO textCollection) => OnPlayMonologue?.Invoke(textCollection);
    public static event Action<TextCollectionSO> OnPlayMonologue;
}
