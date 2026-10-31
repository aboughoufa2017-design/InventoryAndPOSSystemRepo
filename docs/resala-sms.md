# Resala SMS / OTP client

Typed client: `IResalaSmsClient` (`Warehouse.Application.Messaging`) implemented by `ResalaSmsClient`.

## Configuration

| Variable | Purpose |
| --- | --- |
| `RESALA_API_KEY` / `Resala__ApiKey` | Bearer key (`rsl_…`). Required to send. Never commit real keys. |
| `RESALA_BASE_URL` / `Resala__BaseUrl` | Default `https://dev.resala.ly` |
| `RESALA_SENDER` / `Resala__DefaultSender` | Optional default `?sender=` (exact active name) |

## Usage

```csharp
try
{
    var pin = await resala.CreatePinAsync(
        new CreatePinRequest("218910001234"),
        new CreatePinQuery(ServiceName: "Warehouse", Len: 6));

    var sent = await resala.SendTemplateAsync(
        new SendTemplateRequest
        {
            Records =
            [
                new Dictionary<string, object?>
                {
                    ["phone"] = ResalaPhone.NormalizeLibyanMobile("0910001234")!,
                    ["$1"] = "مودرن تك",
                    ["$2"] = "أحمد",
                },
            ],
        },
        new SendTemplateQuery(SmsTemplateId: templateId));

    var history = await resala.GetSentViewAsync(new SentViewQuery(Page: 1, Paginate: 20));
}
catch (ResalaApiException ex)
{
    // Branch on Type — never on localized Message.
    switch (ex.Type)
    {
        case ResalaErrorTypes.InsufficientCredit:
        case ResalaErrorTypes.AccountExpired:
            // billing / account
            break;
        case ResalaErrorTypes.InputValidation:
            // ex.Errors has per-field messages
            break;
        case ResalaErrorTypes.Unauthorized:
        case ResalaErrorTypes.TokenExpired:
            // bad / expired API key
            break;
        default:
            break;
    }
}
```

Do **not** auto-retry a failed send; the message may already have been accepted.

## Order receipt (mobile POS)

Staff receipt screen calls:

- `POST /api/v1/receipts/send-email`
- `POST /api/v1/receipts/send-sms`
- `POST /api/v1/receipts/send` (email and/or SMS)

SMS uses `RESALA_RECEIPT_SMS_TEMPLATE_ID` with `$1` order id, `$2` total, `$3` lines, `$4` Scan & Go code.

Email uses SMTP (`SmtpSettings` / `SMTP_*`) via MailKit — see also the SMTP guide.
