import 'package:url_launcher/url_launcher.dart';

/// Device-side WhatsApp / SMS launch helpers for POS receipts.
class ReceiptShareLinks {
  ReceiptShareLinks._();

  /// Practical limit for `sms:` URI body (device / OEM dependent).
  static const int smsBodyMaxChars = 480;

  /// Digits only; Libyan local `09x…` → `2189…` for `wa.me`.
  static String? normalizeWhatsAppDigits(String? phone) {
    if (phone == null || phone.trim().isEmpty) return null;
    final digits = phone.replaceAll(RegExp(r'\D'), '');
    if (digits.isEmpty) return null;

    if (digits.length == 10 &&
        digits.startsWith('0') &&
        _isLibyanCarrier(digits.substring(1, 3))) {
      return '218${digits.substring(1)}';
    }
    if (digits.length == 12 &&
        digits.startsWith('218') &&
        _isLibyanCarrier(digits.substring(3, 5))) {
      return digits;
    }
    // Allow other E.164-ish numbers (8–15 digits) for WhatsApp.
    if (digits.length >= 8 && digits.length <= 15) {
      return digits.startsWith('0') ? digits.substring(1) : digits;
    }
    return null;
  }

  /// Digits for SMS composer (prefer local form when Libyan).
  static String? normalizeSmsDigits(String? phone) {
    if (phone == null || phone.trim().isEmpty) return null;
    final digits = phone.replaceAll(RegExp(r'\D'), '');
    if (digits.isEmpty) return null;
    if (digits.length == 12 &&
        digits.startsWith('218') &&
        _isLibyanCarrier(digits.substring(3, 5))) {
      return '0${digits.substring(3)}';
    }
    if (digits.length >= 8 && digits.length <= 15) return digits;
    return null;
  }

  static bool _isLibyanCarrier(String two) =>
      two == '91' || two == '92' || two == '93' || two == '94';

  static Uri whatsAppUri({required String phone, required String text}) {
    final digits = normalizeWhatsAppDigits(phone);
    if (digits == null) {
      throw ArgumentError('Invalid phone for WhatsApp');
    }
    return Uri.https('wa.me', '/$digits', {'text': text});
  }

  static Uri smsUri({required String phone, required String text}) {
    final digits = normalizeSmsDigits(phone);
    if (digits == null) {
      throw ArgumentError('Invalid phone for SMS');
    }
    final body = text.length <= smsBodyMaxChars
        ? text
        : '${text.substring(0, smsBodyMaxChars - 1)}…';
    // sms: scheme — some Android builds prefer ?body=, iOS uses &body= after ;
    return Uri(
      scheme: 'sms',
      path: digits,
      queryParameters: {'body': body},
    );
  }

  static Future<bool> openWhatsApp({
    required String phone,
    required String text,
  }) async {
    final uri = whatsAppUri(phone: phone, text: text);
    if (!await canLaunchUrl(uri)) return false;
    return launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  static Future<bool> openSms({
    required String phone,
    required String text,
  }) async {
    final uri = smsUri(phone: phone, text: text);
    if (!await canLaunchUrl(uri)) {
      // Fallback: smsto: without relying on canLaunchUrl quirks.
      final digits = normalizeSmsDigits(phone)!;
      final body = text.length <= smsBodyMaxChars
          ? text
          : '${text.substring(0, smsBodyMaxChars - 1)}…';
      final fallback = Uri.parse(
        'sms:$digits?body=${Uri.encodeComponent(body)}',
      );
      return launchUrl(fallback, mode: LaunchMode.externalApplication);
    }
    return launchUrl(uri, mode: LaunchMode.externalApplication);
  }
}
