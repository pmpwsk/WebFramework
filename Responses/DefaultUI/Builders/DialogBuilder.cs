using uwap.WebFramework.Database;
using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// Helper class to build dynamic dialogs.
/// </summary>
public static class DialogBuilder
{
    /// <summary>
    /// Opens a dynamic dialog with the given heading and message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Nothing DynamicDialogAction(Page page, IconAndText heading, List<AbstractElement> elements, ActionHandler action)
    {
        page.OpenDynamicDialog(heading, elements, action);
        return new Nothing();
    }

    /// <summary>
    /// Opens a dynamic dialog with the given heading and message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Task<IActionResponse> DynamicDialogActionAsync(Page page, IconAndText heading, List<AbstractElement> elements, ActionHandler action)
        => Task.FromResult<IActionResponse>(DynamicDialogAction(page, heading, elements, action));

    /// <summary>
    /// Opens a dynamic dialog with the given heading and message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Nothing DynamicDialogAction(Page page, IconAndText heading, params string[] messages)
    {
        page.OpenDynamicDialog(
            heading,
            [
                ..messages.Select(message => new Paragraph(message)),
                new SubmitButton("Okay")
            ],
            _ => DynamicDialogBackActionAsync(page)
        );
        return new Nothing();
    }

    /// <summary>
    /// Opens a dynamic dialog with the given heading and message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Task<IActionResponse> DynamicDialogActionAsync(Page page, IconAndText heading, params string[] messages)
        => Task.FromResult<IActionResponse>(DynamicDialogAction(page, heading, messages));

    /// <summary>
    /// Opens a dynamic error popup with the given message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Nothing DynamicErrorAction(Page page, params string[] messages)
        => DynamicDialogAction(page, "Error", messages);

    /// <summary>
    /// Opens a dynamic error popup with the given message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Task<IActionResponse> DynamicErrorActionAsync(Page page, params string[] messages)
        => Task.FromResult<IActionResponse>(DynamicErrorAction(page, messages));

    /// <summary>
    /// Opens a dynamic info popup with the given message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Nothing DynamicInfoAction(Page page, params string[] messages)
        => DynamicDialogAction(page, "Info", messages);

    /// <summary>
    /// Opens a dynamic info popup with the given message lines to the given page, and returns an empty action response.
    /// </summary>
    public static Task<IActionResponse> DynamicInfoActionAsync(Page page, params string[] messages)
        => Task.FromResult<IActionResponse>(DynamicInfoAction(page, messages));

    /// <summary>
    /// Closes any dynamic dialogs, and returns an empty action response.
    /// </summary>
    public static Nothing DynamicDialogCloseAction(Page page)
    {
        page.CloseDynamicDialog();
        return new Nothing();
    }

    /// <summary>
    /// Closes any dynamic dialogs, and return an empty action response.
    /// </summary>
    public static Task<IActionResponse> DynamicDialogCloseActionAsync(Page page)
        => Task.FromResult<IActionResponse>(DynamicDialogCloseAction(page));

    /// <summary>
    /// Returns the dynamic dialog to its previous state or closes it if no previous state is present and returns an empty action response.
    /// </summary>
    public static Nothing DynamicDialogBackAction(Page page)
    {
        page.ReturnDynamicDialog();
        return new Nothing();
    }

    /// <summary>
    /// Returns the dynamic dialog to its previous state or closes it if no previous state is present and returns an empty action response.
    /// </summary>
    public static Task<IActionResponse> DynamicDialogBackActionAsync(Page page)
        => Task.FromResult<IActionResponse>(DynamicDialogBackAction(page));
    
    /// <summary>
    /// Opens a dialog to create or edit an object.
    /// </summary>
    public static Task<IActionResponse> SaveObjectDialogActionAsync<C>(Page page, C obj, IconAndText heading, List<IInputBuilder<C>> fields, Action<C>? additionalApplicator, Func<Action<C>, Task<IActionResponse>> saver) where C : AbstractTableValue
    {
        List<AbstractElement> elements = [];
        foreach (var field in fields)
            elements.Add(field.Initialize(obj, page));
        elements.Add(new Row(
            new ContinueButton(),
            new DialogBackButton(page)
        ));
        
        return DynamicDialogActionAsync(
            page,
            heading,
            elements,
            async _ =>
            {
                foreach (var field in fields)
                {
                    var message = await field.ValidateAsync();
                    if (message != null)
                        return DynamicErrorAction(page, message);
                }
                
                return await saver(o =>
                {
                    foreach (var field in fields)
                        field.Apply(o);

                    additionalApplicator?.Invoke(o);
                });
            }
        );
    }
    
    /// <summary>
    /// Opens a dialog to delete an object.
    /// </summary>
    public static Task<IActionResponse> DeleteObjectDialogActionAsync<C>(Page page, C obj, Table<C> table, IconAndText heading, string name, string returnLocation) where C : AbstractTableValue
        => DynamicDialogActionAsync(
            page,
            heading,
            [
                new Paragraph($"Are you sure you want to delete \"{name}\"?"),
                new Row(
                    new ContinueButton(),
                    new DialogCancelButton(page)
                )
            ],
            async _ =>
            {
                await table.DeleteAsync(obj);
                return new Navigate(returnLocation);
            }
        );
}