using uwap.WebFramework.Database;
using uwap.WebFramework.Localization;
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
                new SubmitButton(CommonTexts.Okay)
            ],
            _ => Back(page)
        );

    /// <summary>
    /// Opens a dynamic error popup with the given message lines to the given page.
    /// </summary>
    public static void Error(Page page, params string[] messages)
        => Open(page, CommonTexts.Error, messages);

    /// <summary>
    /// Opens a dynamic info popup with the given message lines to the given page.
    /// </summary>
    public static void Info(Page page, params string[] messages)
        => Open(page, CommonTexts.Info, messages);

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
    public static void SaveObject<C>(
        Page page,
        Table<C> table,
        C obj,
        string createdUrlPrefix,
        IconAndText heading,
        List<IInputBuilder<C>> fields
    ) where C : AbstractTableValue
    {
        List<AbstractElement> elements = [];
        bool isFirst = true;
        foreach (var field in fields)
        {
            elements.Add(field.Initialize(obj, page, isFirst));
            isFirst = false;
        }
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
                
                if (obj.IdNullable == null)
                {
                    foreach (var field in fields)
                        field.Apply(obj);
                    await table.CreateAsync(obj);
                    
                    await page.NavigateAsync($"{createdUrlPrefix}{obj.Id}");
                }
                else
                {
                    await using (table.StartModifying(ref obj))
                        foreach (var field in fields)
                            field.Apply(obj);
                        
                    await page.ReloadAsync();
                }
            }
        );
    }
    
    /// <summary>
    /// Opens a dialog to delete an object.
    /// </summary>
    public static void DeleteObject<C>(
        Page page,
        C obj,
        Table<C> table,
        string returnLocation,
        IconAndText heading,
        string name
    ) where C : AbstractTableValue
        => Open(
            page,
            heading,
            [
                new Paragraph($"{CommonTexts.DeleteConfirmationStart}{name}{CommonTexts.DeleteConfirmationEnd}"),
                new Row(
                    new ContinueButton(),
                    new DialogBackButton(page)
                )
            ],
            async _ =>
            {
                await table.DeleteAsync(obj);
                await page.NavigateAsync(returnLocation);
            }
        );
}