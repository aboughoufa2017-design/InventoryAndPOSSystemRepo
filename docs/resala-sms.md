# Resala SMS / OTP client

Typed client: `IResalaSmsClient` (`Warehouse.Application.Messaging`) implemented by `ResalaSmsClient`.

## Configuration

| Variable | Purpose |
| --- | --- |
| `RESALA_API_KEY` / `Resala__ApiKey` | Bearer key (`rsl_…`). Required to send. Never commit real keys. |
| `RESALA_BASE_URL` / `Resala__BaseUrl` | Host only, e.g. `https://dev.resala.ly` — **do not** append `/api/v1` (the client adds paths). |
| `RESALA_SENDER` / `Resala__DefaultSender` | Optional default `?sender=` (exact active name) |
| `RESALA_RECEIPT_SMS_TEMPLATE_ID` | Approved template GUID for order receipts |

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
- `POST /api/v1/receipts/send` (email and/or SMS in **parallel**)

`POST /send` uses independent per-channel timeouts (email ~15s, SMS ~25s) so a hung SMTP connect does not cancel SMS. Partial success returns `{ emailSent, smsSent, emailError, smsError }`.

SMS uses `RESALA_RECEIPT_SMS_TEMPLATE_ID` with four placeholders:

| Placeholder | Content | Max length |
| --- | --- | --- |
| `$1` | Order code (or order id) | 20 |
| `$2` | Grand total + currency | 20 |
| `$3` | Line items `name×qty` (comma-separated) | 70 |
| `$4` | Order time UTC (`yyyy-MM-dd HH:mm`) | 20 |

Paste this body in the Resala dashboard (then approve if required):

```
اهلا بك، تفاصيل الفاتورة:
الطلب: $1
المبلغ: $2
الأصناف: $3
الوقت: $4
```

Long carts may append `+N` inside `$3` when truncated. Full receipt remains on email / Share.

### WhatsApp

This API sends **SMS via Resala only**. WhatsApp is not wired up. To send receipts on WhatsApp you need a WhatsApp Business channel (Meta Cloud API, or a provider that offers WhatsApp templates in Libya). If Resala adds WhatsApp template send on your account, we can add a parallel channel; until then use SMS + Share / email.

Email uses SMTP (`SmtpSettings` / `SMTP_*`) via MailKit (`SmtpClient.Timeout` ~12s).

### Render / production notes

- Outbound **SMTP is often blocked** on Render. Prefer `SMTP_SERVER=smtp.gmail.com`, `SMTP_PORT=587`, and a Gmail **app password**. Office 365 (`smtp.office365.com`) may hang or refuse from cloud hosts.
- After the DB seed/restore has run once, set `WAREHOUSE_RESTORE_ON_START=false` so startup does not re-apply a dump that conflicts with live schema/data.
- Never commit or paste real `RESALA_API_KEY` / `SMTP_PASSWORD` values into docs or tickets; rotate any key that was exposed.
