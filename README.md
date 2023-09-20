# WebFramework
Cross-platform .NET library written in C# that allows you to create a web server for dynamic and/or static websites and web apps with ease.

Website: https://uwap.org/projects/webframework

Changelog: https://uwap.org/changes/webframework

Guides: https://uwap.org/guides/webframework

## Main features
- Events for different types of HTTP requests (app, API, post, upload, download) with objects for easy access to everything you need to handle those requests.
- Automatic SSL certificates using Let's Encrypt
- Pages and elements so you don't have to write any HTML code
- .wfpg files to quickly create static web pages
- Accounts with everything one would expect from an account
- Mail server (incoming and outgoing)
- Object-oriented database
- File server with server cache and browser cache management
- Worker to regularly update files, handle memory integrity, certificates and more
- Plugins (see more below)
- Option for usage as a local web server for local web apps as interfaces for proper apps on a computer
 
...and much more!

Most features have plugins as interfaces.

## Installation
You can get the NuGet package here: [uwap.WebFramework](https://www.nuget.org/packages/uwap.WebFramework/)

You can also download the source code from GitHub and add a reference to it from your project.

This library is based on ASP.NET, so it's best to create an empty ASP.NET project rather than a console app (those will terminate unless paused otherwise).

## Official plugins
- Server (interface for server management by administrator accounts)
- Users (interface for accounts)
- Notes (web app for user's notes)
- Redirects (allows for static redirects)

## Planned plugins
- Mail (mailbox system and interface for the mail server)
- Files (file management for admins and interface for a file server)
- Git (interface for a git server running on the same machine)
- Finances (simple finance manager for users)

## Used libraries
- [Certes](https://github.com/fszlin/certes) to talk to Let's Encrypt
- [DnsClient.NET](https://github.com/MichaCo/DnsClient.NET) for DNS requests to find mail servers and check mail authentication
- [MailKit](https://github.com/jstedfast/MailKit) to send emails via SMTP and parse received emails
- [SmtpServer](https://github.com/cosullivan/SmtpServer) to listen for incoming emails on the raw SMTP level
- [Otp.NET](https://github.com/kspearrin/Otp.NET) to generate and check time-based one-time passwords (TOTP) for two-factor authentication
- [QRCoder](https://github.com/codebude/QRCoder) to generate QR codes for 2FA setup strings