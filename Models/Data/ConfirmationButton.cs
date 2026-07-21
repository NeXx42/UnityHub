namespace Models.Data;

public struct ConfirmationButton
{
    public string label;
    public string? className;

    public ConfirmationButton(string lbl, bool isPrimary = false)
    {
        label = lbl;

        if (isPrimary)
            className = "Primary";
    }
}
