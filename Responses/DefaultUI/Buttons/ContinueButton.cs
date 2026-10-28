using uwap.WebFramework.Localization;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that submits a form with the text "Continue".
/// </summary>
public class ContinueButton() : SubmitButton(CommonTexts.Continue);