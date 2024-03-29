# WebFramework
Cross-platform .NET library written in C# that allows you to create a web server for dynamic and/or static websites and web apps with ease.

Website: https://uwap.org/projects/webframework

Changelog: https://uwap.org/changes/webframework

Guides: https://uwap.org/guides/webframework

## Main features
- Modular structure for high customizability
- Events for different types of HTTP requests (app, API, post, upload, download) with objects for easy access to everything you need to handle those requests
- Automatic SSL certificates using Let's Encrypt
- Pages and elements so you don't have to write any HTML code
- .wfpg files to quickly create static web pages
- Accounts with everything one would expect from an account
- Mail server (incoming and outgoing) with SPF, DKIM and DMARC
- Object-oriented database
- File server with server cache and browser cache management
- Worker to regularly update files, handle memory integrity, certificates and more
- Plugins (see more below)
- Option for usage as a local web server for local web apps as interfaces for proper apps on a computer
- Backups for the database and external files
 
...and much more!

Most features have plugins as interfaces.

## Installation
You can get the NuGet package here: [uwap.WebFramework](https://www.nuget.org/packages/uwap.WebFramework/)

You can also download the source code from GitHub and add a reference to it from your project.

This library is based on ASP.NET, so it's best to create an empty ASP.NET project rather than a console app (those will terminate unless paused otherwise).

## Official plugins
- ServerPlugin (interface for server management by administrator accounts)
- UsersPlugin (interface for accounts)
- NotesPlugin (web app for user's notes)
- MailPlugin (mailbox system and interface for the mail server)
- RedirectPlugin (allows for static redirects)
- BackupDownloaderPlugin (for off-site backup storage)

## Planned plugins
- FilesPlugin (file management for admins and interface for a file server)
- GitPlugin (interface for a git server running on the same machine)
- FinancesPlugin (simple finance manager for users)

## Used libraries
- [Certes](https://github.com/fszlin/certes) to talk to Let's Encrypt
- [DnsClient.NET](https://github.com/MichaCo/DnsClient.NET) for DNS requests to find mail servers and check mail authentication
- [MailKit](https://github.com/jstedfast/MailKit) to decode and encode emails from/to SMTP-compliant messages, send emails on the SMTP level once a suitable mail server for the recipient has been found, and validate DKIM signatures of messages against the keys found in the DNS records
- [SmtpServer](https://github.com/cosullivan/SmtpServer) to listen for incoming emails on the raw SMTP level
- [Otp.NET](https://github.com/kspearrin/Otp.NET) to generate and check time-based one-time passwords (TOTP) for two-factor authentication
- [QRCoder](https://github.com/codebude/QRCoder) to generate QR codes for 2FA setup strings
- [Isopoh.Cryptography.Argon2](https://github.com/mheyman/Isopoh.Cryptography.Argon2) to hash passwords using Argon2
- [IPAddressRange](https://github.com/jsakamoto/ipaddressrange) to parse IP address ranges and check whether a given IP address is within that range (used for mail authentication)