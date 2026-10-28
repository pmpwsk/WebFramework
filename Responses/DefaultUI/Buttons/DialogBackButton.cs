using uwap.WebFramework.Localization;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that navigates back on any open dynamic dialogs.
/// </summary>
public class DialogBackButton(Page page)
    : ServerSubmitButton(
        CommonTexts.Back,
        _ => DialogBuilder.Back(page)
    );