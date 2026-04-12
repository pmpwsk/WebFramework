using uwap.WebFramework.Responses.Actions;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI button that closes any open dynamic dialogs.
/// </summary>
public class DialogCancelButton(Page page)
    : ServerSubmitButton(
        "Cancel",
        _ => DialogBuilder.DynamicDialogCloseActionAsync(page)
    );