import 'dart:convert';

import 'package:http/http.dart' as http;

import '../auth/auth_repository.dart';
import 'local_receipt_store.dart';

class ReceiptSendResult {
  ReceiptSendResult({
    required this.emailSent,
    required this.smsSent,
    this.emailError,
    this.smsError,
  });

  final bool emailSent;
  final bool smsSent;
  final String? emailError;
  final String? smsError;

  factory ReceiptSendResult.fromJson(Map<String, dynamic> json) => ReceiptSendResult(
        emailSent: json['emailSent'] as bool? ?? json['EmailSent'] as bool? ?? false,
        smsSent: json['smsSent'] as bool? ?? json['SmsSent'] as bool? ?? false,
        emailError: (json['emailError'] ?? json['EmailError'])?.toString(),
        smsError: (json['smsError'] ?? json['SmsError'])?.toString(),
      );
}

/// Staff API for email / Resala SMS order receipts.
class ReceiptApi {
  ReceiptApi({required this.apiBaseUrl, required AuthRepository auth, http.Client? httpClient})
      : _auth = auth,
        _http = httpClient ?? http.Client();

  final String apiBaseUrl;
  final AuthRepository _auth;
  final http.Client _http;

  Uri _uri(String path) => Uri.parse('$apiBaseUrl$path');

  Future<Map<String, String>> _headers() async {
    final token = await _auth.accessToken;
    if (token == null || token.isEmpty) {
      throw StateError('Not signed in');
    }
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    };
  }

  Map<String, dynamic> _body({
    required LocalReceipt receipt,
    String? customerEmail,
    String? customerPhone,
  }) =>
      {
        'orderId': receipt.id,
        'orderCode': receipt.orderCode,
        'customerEmail': customerEmail,
        'customerPhone': customerPhone,
        'currencyCode': receipt.currencyCode,
        'grandTotal': receipt.grandTotal,
        'createdAtUtc': receipt.createdAt.toUtc().toIso8601String(),
        'lines': receipt.lines
            .map(
              (l) => {
                'name': l.name,
                'sku': l.sku,
                'quantity': l.quantity,
                'unitPrice': l.unitPrice,
              },
            )
            .toList(),
      };

  Future<void> sendEmail({
    required LocalReceipt receipt,
    required String customerEmail,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/receipts/send-email'),
      headers: await _headers(),
      body: jsonEncode(_body(receipt: receipt, customerEmail: customerEmail)),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Email send failed (${response.statusCode}): ${response.body}');
    }
  }

  Future<void> sendSms({
    required LocalReceipt receipt,
    required String customerPhone,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/receipts/send-sms'),
      headers: await _headers(),
      body: jsonEncode(_body(receipt: receipt, customerPhone: customerPhone)),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('SMS send failed (${response.statusCode}): ${response.body}');
    }
  }

  Future<ReceiptSendResult> sendBoth({
    required LocalReceipt receipt,
    String? customerEmail,
    String? customerPhone,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/receipts/send'),
      headers: await _headers(),
      body: jsonEncode(
        _body(
          receipt: receipt,
          customerEmail: customerEmail,
          customerPhone: customerPhone,
        ),
      ),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Send failed (${response.statusCode}): ${response.body}');
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    return ReceiptSendResult.fromJson(data);
  }
}
