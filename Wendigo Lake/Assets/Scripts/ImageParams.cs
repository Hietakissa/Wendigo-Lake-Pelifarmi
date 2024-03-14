public struct ImageParams
{
    public readonly bool usedFlash;
    public readonly bool wasHidden;

    public ImageParams(bool usedFlash, bool wasHidden)
    {
        this.usedFlash = usedFlash;
        this.wasHidden = wasHidden;
    }
}
