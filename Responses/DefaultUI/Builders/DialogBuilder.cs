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
    /// Opens a dynamic dialog with the given heading and message lines to the given page.
    /// </summary>
    public static void Open(Page page, IconAndText heading, List<AbstractElement> elements, ActionHandlerAsync action)
        => page.OpenDynamicDialog(heading, elements, action);
    
    /// <summary>
    /// Opens a dynamic dialog with the given heading and message lines to the given page.
    /// </summary>
    public static void Open(Page page, IconAndText heading, List<AbstractElement> elements, ActionHandler action)
        => Open(page, heading, elements, action.ToAsync());

    /// <summary>
    /// Opens a dynamic dialog with the given heading and message lines to the given page.
    /// </summary>
    public static void Open(Page page, IconAndText heading, params string[] messages)
        => page.OpenDynamicDialog(
            heading,
            [
                ..messages.Select(message => new Paragraph(message)),
                new SubmitButton("Okay")
            ],
            _ => Back(page)
        );

    /// <summary>
    /// Opens a dynamic error popup with the given message lines to the given page.
    /// </summary>
    public static void Error(Page page, params string[] messages)
        => Open(page, "Error", messages);

    /// <summary>
    /// Opens a dynamic info popup with the given message lines to the given page.
    /// </summary>
    public static void Info(Page page, params string[] messages)
        => Open(page, "Info", messages);

    /// <summary>
    /// Closes any dynamic dialogs.
    /// </summary>
    public static void Close(Page page)
        => page.CloseDynamicDialog();

    /// <summary>
    /// Returns the dynamic dialog to its previous state or closes it if no previous state is present.
    /// </summary>
    public static void Back(Page page)
        => page.ReturnDynamicDialog();

    /// <summary>
    /// Opens a dialog to create or edit an object.
    /// </summary>
    public static void SaveObject<C>(Page page, C obj, IconAndText heading, List<IInputBuilder<C>> fields, Action<C>? additionalApplicator, Func<Action<C>, Task> saver) where C : AbstractTableValue
    {
        List<AbstractElement> elements = [];
        foreach (var field in fields)
            elements.Add(field.Initialize(obj, page));
        elements.Add(new Row(
            new ContinueButton(),
            new DialogBackButton(page)
        ));
        
        Open(
            page,
            heading,
            elements,
            async _ =>
            {
                foreach (var field in fields)
                {
                    var message = await field.ValidateAsync();
                    if (message != null)
                    {
                        Error(page, message);
                        return;
                    }
                }
                
                await saver(o =>
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
    public static void DeleteObject<C>(Page page, C obj, Table<C> table, IconAndText heading, string name, string returnLocation) where C : AbstractTableValue
        => Open(
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
                page.Navigate(returnLocation);
            }
        );
}