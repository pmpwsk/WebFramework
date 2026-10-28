namespace uwap.WebFramework.Localization;

/// <summary>
/// Contains translations for common texts.
/// </summary>
public static class CommonTexts
{
    /// <summary>
    /// Creates a new translation with the given icon and values for english and german.
    /// </summary>
    public static Translation Create(string? icon, string english, string german)
        => new(icon, ("en", english), ("de", german));
    
    /// <summary>
    /// Creates a new translation with the given icon and values for english and german.
    /// </summary>
    public static Translation Create(string english, string german)
        => Create(null, english, german);
    
    public static readonly Translation AllRightsReserved = Create(
        "All rights reserved.",
        "Alle Rechte vorbehalten."
    );
    
    public static readonly Translation Unknown = Create(
        "Unknown",
        "Unbekannt"
    );
    
    public static readonly Translation Untitled = Create(
        "Untitled",
        "Unbenannt"
    );
    
    public static readonly Translation Current = Create(
        "Current",
        "Aktuell"
    );
    
    public static readonly Translation Continue = Create(
        "bi bi-arrow-return-right",
        "Continue",
        "Weiter"
    );
        
    public static readonly Translation Cancel = Create(
        "bi bi-x-circle",
        "Cancel",
        "Abbrechen"
    );
    
    public static readonly Translation Download = Create(
        "bi bi-download",
        "Download",
        "Herunterladen"
    );
    
    public static readonly Translation Back = Create(
        "Back",
        "Zurück"
    );
    
    public static readonly Translation Okay = Create(
        "bi bi-arrow-return-right",
        "Okay",
        "Okay"
    );
    
    public static readonly Translation Info = Create(
        "bi bi-info-circle",
        "Info",
        "Info"
    );
    
    public static readonly Translation Error = Create(
        "bi bi-exclamation-circle",
        "Error",
        "Fehler"
    );
    
    public static readonly Translation Alert = Create(
        "bi bi-exclamation-circle",
        "Alert",
        "Warnung"
    );
        
    public static readonly Translation Add = Create(
        "bi bi-arrow-return-right",
        "Add",
        "Hinzufügen"
    );
        
    public static readonly Translation Delete = Create(
        "bi bi-trash",
        "Delete",
        "Entfernen"
    );
        
    public static readonly Translation Rename = Create(
        "bi bi-tag",
        "Rename",
        "Umbenennen"
    );
        
    public static readonly Translation Settings = Create(
        "bi bi-gear",
        "Settings",
        "Einstellungen"
    );
    
    public static readonly Translation Home = Create(
        "bi bi-house",
        "Home",
        "Start"
    );
    
    public static readonly Translation GoTo = Create(
        "bi bi-journal-bookmark",
        "Go to",
        "Gehe zu"
    );
    
    public static readonly Translation Menu = Create(
        "bi bi-list",
        "Menu",
        "Menü"
    );
    
    public static readonly Translation ContactSupport = Create(
        "bi bi-question-circle",
        "Contact support",
        "Support kontaktieren"
    );
    
    public static readonly Translation Account = Create(
        "bi bi-person",
        "Account",
        "Konto"
    );
    
    public static readonly Translation Login = Create(
        "bi bi-lock",
        "Login",
        "Login"
    );
    
    public static readonly Translation Logout = Create(
        "bi bi-box-arrow-left",
        "Logout",
        "Logout"
    );
    
    public static readonly Translation Register = Create(
        "bi bi-person-vcard",
        "Register",
        "Registrieren"
    );
    
    public static readonly Translation Username = Create(
        "bi bi-person",
        "Username",
        "Benutzername"
    );
    
    public static readonly Translation UsernamePlaceholder = Create(
        "Enter your username...",
        "Benutzernamen eingeben..."
    );
    
    public static readonly Translation UsernameFormat = Create(
        "Usernames must be at least three characters long and only contain lowercase letters, digits, dashes, dots and underscores. The first and last characters can only be letters or digits.",
        "Benutzernamen müssen mindestens drei Zeichen lang sein und dürfen nur Kleinbuchstaben, Ziffern, Bindestriche, Punkte und Unterstriche enthalten. Das erste und das letzte Zeichen dürfen nur Kleinbuchstaben und Ziffern sein."
    );
    
    public static readonly Translation UsernameSame = Create(
        "The provided username is the same as the old one.",
        "Dieser Benutzername entspricht dem alten Benutzernamen."
    );
    
    public static readonly Translation UsernameExists = Create(
        "This username is already being used by another account.",
        "Dieser Benutzername wird bereits von einem anderen Konto verwendet."
    );
    
    public static readonly Translation Mail = Create(
        "bi bi-envelope",
        "Email address",
        "Email-Adresse"
    );
    
    public static readonly Translation MailPlaceholder = Create(
        "Enter your email address...",
        "Email-Adresse eingeben..."
    );
    
    public static readonly Translation MailEmpty = Create(
        "Please enter your email address.",
        "Bitte geben Sie Ihre Email-Adresse ein."
    );
    
    public static readonly Translation MailNotFound = Create(
        "This email address isn't associated with any account.",
        "Zu dieser Email-Adresse wurde kein Konto gefunden."
    );
    
    public static readonly Translation MailFormat = Create(
        "The provided email address is invalid.",
        "Die Email-Adresse ist ungültig."
    );
    
    public static readonly Translation MailSame = Create(
        "The provided email address is the same as the old one.",
        "Diese Email-Adresse entspricht der alten Email-Adresse."
    );
    
    public static readonly Translation MailExists = Create(
        "This email address is already being used by another account.",
        "Diese Email-Adresse wird bereits von einem anderen Konto verwendet."
    );
    
    public static readonly Translation Password = Create(
        "bi bi-key",
        "Password",
        "Passwort"
    );
    
    public static readonly Translation PasswordPlaceholder = Create(
        "Enter your password...",
        "Passwort eingeben..."
    );
    
    public static readonly Translation OldPassword = Create(
        "bi bi-key",
        "Current Password",
        "Altes Passwort"
    );
    
    public static readonly Translation OldPasswordPlaceholder = Create(
        "Enter your current password...",
        "Altes Passwort eingeben..."
    );
    
    public static readonly Translation PasswordFormat = Create(
        "Passwords must be at least eight characters long and contain at least one uppercase letter, lowercase letter, digit and special character.",
        "Passwörter müssen mindestens acht Zeichen lang sein und mindestens einen Großbuchstaben, Kleinbuchstaben, Ziffer und Symbol enthalten."
    );
    
    public static readonly Translation PasswordSame = Create(
        "The provided password is the same as the old one.",
        "Dieses Passwort entspricht dem alten Passwort."
    );
    
    public static readonly Translation TwoFactorCode = Create(
        "bi bi-phone",
        "2FA code",
        "2FA-Code"
    );
    
    public static readonly Translation TwoFactorOrRecovery = Create(
        "bi bi-phone",
        "2FA code / recovery",
        "2FA-Code / Backup-Code"
    );
    
    public static readonly Translation TwoFactorPlaceholder = Create(
        "Enter the current code...",
        "Code eingeben..."
    );
    
    public static readonly Translation Banned = Create(
        "bi bi-slash-circle",
        "Banned",
        "Gesperrt"
    );
    
    public static readonly Translation BannedParagraph1 = Create(
        "You have been temporarily banned from authenticating yourself to avoid brute-force attacks.",
        "Sie wurden vorübergehend für den Login gesperrt, um Brute-Force-Angriffe zu vermeiden."
    );
    
    public static readonly Translation BannedParagraph2 = Create(
        "This may have been caused by someone else on your network (home network or VPN).",
        "Dies könnte auch durch eine andere Person in Ihrem Netzwerk (Heimnetzwerk oder VPN) ausgelöst worden sein."
    );
    
    public static readonly Translation BannedParagraph3 = Create(
        "You may try to log in again later.",
        "Sie können später erneut versuchen, sich anzumelden."
    );
    
    public static readonly Translation DeleteConfirmationStart = Create(
        "Are you sure you want to delete \"",
        "Möchten Sie \""
    );
    
    public static readonly Translation DeleteConfirmationEnd = Create(
        "\"?",
        "\" wirklich löschen?"
    );
    
    public static readonly Translation EmptyFields = Create(
        "Please fill out all fields.",
        "Bitte füllen Sie alle Felder aus."
    );
}