namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form.
/// </summary>
public class ContinueButton()
    : SubmitButton(
        new("bi bi-arrow-return-right", "Continue")
    );