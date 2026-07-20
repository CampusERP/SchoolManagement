# Phase 6: Email Services

## Summary

Implemented full email infrastructure: MailKit SMTP transport, email channel delivery
for notifications, and forgot/reset password endpoints. Build passes with 0 errors.

## What Was Implemented

### New Files (8)

| File | Purpose |
|------|---------|
| `Infrastructure/Email/EmailConfiguration.cs` | Configuration model for SMTP settings |
| `Infrastructure/Email/EmailService.cs` | `IEmailService` implementation using MailKit |
| `Application/Features/Identity/ForgotPassword/ForgotPasswordCommand.cs` | Command: request password reset |
| `Application/Features/Identity/ForgotPassword/ForgotPasswordCommandHandler.cs` | Handler: generates token, sends email |
| `Application/Features/Identity/ForgotPassword/ForgotPasswordCommandValidator.cs` | FluentValidation: valid email required |
| `Application/Features/Identity/ResetPassword/ResetPasswordCommand.cs` | Command: reset password with token |
| `Application/Features/Identity/ResetPassword/ResetPasswordCommandHandler.cs` | Handler: validates token, resets password |
| `Application/Features/Identity/ResetPassword/ResetPasswordCommandValidator.cs` | FluentValidation: password complexity rules |

### Modified Files (7)

| File | Change |
|------|--------|
| `Infrastructure/Infrastructure.csproj` | Added `MailKit` 4.10.0 NuGet package |
| `Api/appsettings.json` | Added `Email` section with SMTP configuration |
| `Infrastructure/DependencyInjection.cs` | Registered `EmailConfiguration` + `IEmailService` → `EmailService` |
| `Application/Common/Interfaces/Services/IIdentityService.cs` | Added `GetByEmailAsync`, `GeneratePasswordResetTokenAsync`, `ResetPasswordAsync` |
| `Infrastructure/Identity/IdentityService.cs` | Implemented 3 new identity methods |
| `Infrastructure/Outbox/OutboxMessageHandlers.cs` | `DeliverNotificationBatchHandler` now supports Email channel |
| `Api/Controllers/AuthController.cs` | Added `forgot-password` and `reset-password` endpoints |

## New API Endpoints

### POST /api/auth/forgot-password

Request a password reset link via email. Always returns success to prevent email enumeration.

```
Body: { "email": "user@example.com" }
Anonymous: Yes
Response: { "message": "If an account with that email exists, a reset link has been sent." }
```

### POST /api/auth/reset-password

Reset password using the token from the email link.

```
Body: { "email": "user@example.com", "token": "...", "newPassword": "NewPass123!" }
Anonymous: Yes
Response: Result (success/failure)
```

## Email Configuration (appsettings.json)

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromAddress": "noreply@campuserp.com",
    "FromName": "School Management",
    "UseSsl": true
  }
}
```

**Note:** `SmtpUsername` and `SmtpPassword` must be configured with real SMTP credentials before email delivery works.

## Email Channel Delivery

The `DeliverNotificationBatchHandler` now routes by channel:

| Channel | Behavior |
|---------|----------|
| `InApp` | Marks notifications as Delivered (unchanged) |
| `Email` | Sends via `IEmailService` to each recipient, marks Delivered or Failed |
| `SMS` | Throws `NotSupportedException` (not yet implemented) |
| `Push` | Throws `NotSupportedException` (not yet implemented) |

## Build Status

```
Build succeeded.
0 Error(s)
10 Warning(s)  (MailKit/MimeKit vulnerability warnings + existing NU1903)
```
