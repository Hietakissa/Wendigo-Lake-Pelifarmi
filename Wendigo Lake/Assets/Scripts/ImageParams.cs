public struct ImageParams
{
    public readonly bool UsedFlash;
    public readonly bool WasHidden;

    public ImageParams(bool usedFlash, bool wasHidden)
    {
        this.UsedFlash = usedFlash;
        this.WasHidden = wasHidden;
    }
}
