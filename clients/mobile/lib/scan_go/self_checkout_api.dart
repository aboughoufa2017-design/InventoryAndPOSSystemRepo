import 'dart:convert';

import 'package:http/http.dart' as http;

import '../auth/auth_repository.dart';

class SelfCheckoutException implements Exception {
  SelfCheckoutException(this.message);
  final String message;

  @override
  String toString() => message;
}

class SelfCheckoutLine {
  SelfCheckoutLine({
    required this.id,
    required this.productId,
    required this.sku,
    required this.name,
    required this.barcode,
    required this.quantity,
    required this.unitPrice,
    required this.lineTotal,
  });

  factory SelfCheckoutLine.fromJson(Map<String, dynamic> json) => SelfCheckoutLine(
        id: json['id'] as String,
        productId: json['productId'] as String,
        sku: json['sku'] as String? ?? '',
        name: json['name'] as String? ?? '',
        barcode: json['barcode'] as String?,
        quantity: (json['quantity'] as num).toDouble(),
        unitPrice: (json['unitPrice'] as num).toDouble(),
        lineTotal: (json['lineTotal'] as num).toDouble(),
      );

  final String id;
  final String productId;
  final String sku;
  final String name;
  final String? barcode;
  final double quantity;
  final double unitPrice;
  final double lineTotal;
}

class SelfCheckoutSession {
  SelfCheckoutSession({
    required this.id,
    required this.warehouseId,
    required this.code,
    required this.accessToken,
    required this.isGuest,
    required this.status,
    required this.currencyCode,
    required this.subtotal,
    required this.taxTotal,
    required this.grandTotal,
    required this.auditRequired,
    required this.auditItemCount,
    required this.auditTargetBarcodes,
    required this.auditCompleted,
    required this.expiresAtUtc,
    required this.qrPayload,
    required this.lines,
  });

  factory SelfCheckoutSession.fromJson(Map<String, dynamic> json) => SelfCheckoutSession(
        id: json['id'] as String,
        warehouseId: json['warehouseId'] as String,
        code: json['code'] as String? ?? '',
        accessToken: json['accessToken'] as String? ?? '',
        isGuest: json['isGuest'] as bool? ?? true,
        status: json['status'] as String? ?? '',
        currencyCode: json['currencyCode'] as String? ?? 'LYD',
        subtotal: (json['subtotal'] as num?)?.toDouble() ?? 0,
        taxTotal: (json['taxTotal'] as num?)?.toDouble() ?? 0,
        grandTotal: (json['grandTotal'] as num?)?.toDouble() ?? 0,
        auditRequired: json['auditRequired'] as bool? ?? false,
        auditItemCount: json['auditItemCount'] as int? ?? 0,
        auditTargetBarcodes: (json['auditTargetBarcodes'] as List?)
                ?.map((e) => e.toString())
                .toList() ??
            const [],
        auditCompleted: json['auditCompleted'] as bool? ?? false,
        expiresAtUtc: json['expiresAtUtc'] as String? ?? '',
        qrPayload: json['qrPayload'] as String? ?? '',
        lines: (json['lines'] as List? ?? [])
            .map((e) => SelfCheckoutLine.fromJson(e as Map<String, dynamic>))
            .toList(),
      );

  final String id;
  final String warehouseId;
  final String code;
  final String accessToken;
  final bool isGuest;
  final String status;
  final String currencyCode;
  final double subtotal;
  final double taxTotal;
  final double grandTotal;
  final bool auditRequired;
  final int auditItemCount;
  final List<String> auditTargetBarcodes;
  final bool auditCompleted;
  final String expiresAtUtc;
  final String qrPayload;
  final List<SelfCheckoutLine> lines;
}

class SelfCheckoutTransfer {
  SelfCheckoutTransfer({
    required this.sessionId,
    required this.code,
    required this.qrPayload,
    required this.auditRequired,
    required this.auditItemCount,
    required this.status,
    required this.grandTotal,
    required this.currencyCode,
  });

  factory SelfCheckoutTransfer.fromJson(Map<String, dynamic> json) => SelfCheckoutTransfer(
        sessionId: json['sessionId'] as String,
        code: json['code'] as String? ?? '',
        qrPayload: json['qrPayload'] as String? ?? '',
        auditRequired: json['auditRequired'] as bool? ?? false,
        auditItemCount: json['auditItemCount'] as int? ?? 0,
        status: json['status'] as String? ?? '',
        grandTotal: (json['grandTotal'] as num?)?.toDouble() ?? 0,
        currencyCode: json['currencyCode'] as String? ?? 'LYD',
      );

  final String sessionId;
  final String code;
  final String qrPayload;
  final bool auditRequired;
  final int auditItemCount;
  final String status;
  final double grandTotal;
  final String currencyCode;
}

class SelfCheckoutWaitingItem {
  SelfCheckoutWaitingItem({
    required this.sessionId,
    required this.code,
    required this.status,
    required this.grandTotal,
    required this.currencyCode,
    required this.lineCount,
    required this.updatedAtUtc,
    required this.auditRequired,
    required this.auditCompleted,
  });

  factory SelfCheckoutWaitingItem.fromJson(Map<String, dynamic> json) => SelfCheckoutWaitingItem(
        sessionId: json['sessionId'] as String? ?? '',
        code: json['code'] as String? ?? '',
        status: json['status'] as String? ?? '',
        grandTotal: (json['grandTotal'] as num?)?.toDouble() ?? 0,
        currencyCode: json['currencyCode'] as String? ?? 'LYD',
        lineCount: json['lineCount'] as int? ?? 0,
        updatedAtUtc: json['updatedAtUtc'] as String? ?? '',
        auditRequired: json['auditRequired'] as bool? ?? false,
        auditCompleted: json['auditCompleted'] as bool? ?? false,
      );

  final String sessionId;
  final String code;
  final String status;
  final double grandTotal;
  final String currencyCode;
  final int lineCount;
  final String updatedAtUtc;
  final bool auditRequired;
  final bool auditCompleted;
}

/// Guest + cashier client for `/api/v1/pos/self-checkout`.
class SelfCheckoutApi {
  SelfCheckoutApi({
    required this.apiBaseUrl,
    this.auth,
    http.Client? httpClient,
  }) : _http = httpClient ?? http.Client();

  final String apiBaseUrl;
  final AuthRepository? auth;
  final http.Client _http;

  Uri _uri(String path, [Map<String, String>? query]) =>
      Uri.parse('$apiBaseUrl$path').replace(queryParameters: query);

  Future<Map<String, String>> _headers({bool authRequired = false}) async {
    final headers = <String, String>{'Content-Type': 'application/json'};
    if (authRequired) {
      final token = await auth?.accessToken;
      if (token == null || token.isEmpty) {
        throw SelfCheckoutException('Sign in required');
      }
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  Future<SelfCheckoutSession> startGuest({
    required String tenantCode,
    String? warehouseId,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions'),
      headers: await _headers(),
      body: jsonEncode({
        'tenantCode': tenantCode,
        'warehouseId': warehouseId,
        'isGuest': true,
      }),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> getCurrent(String accessToken) async {
    final response = await _http.get(
      _uri('/api/v1/pos/self-checkout/sessions/current', {'accessToken': accessToken}),
      headers: await _headers(),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> addLine({
    required String accessToken,
    required String barcode,
    double quantity = 1,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions/current/lines'),
      headers: await _headers(),
      body: jsonEncode({
        'accessToken': accessToken,
        'barcode': barcode,
        'quantity': quantity,
      }),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> removeLine({
    required String accessToken,
    required String lineId,
  }) async {
    final response = await _http.delete(
      _uri(
        '/api/v1/pos/self-checkout/sessions/current/lines/$lineId',
        {'accessToken': accessToken},
      ),
      headers: await _headers(),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> setLineQuantity({
    required String accessToken,
    required String lineId,
    required double quantity,
  }) async {
    final response = await _http.put(
      _uri('/api/v1/pos/self-checkout/sessions/current/lines/$lineId'),
      headers: await _headers(),
      body: jsonEncode({
        'accessToken': accessToken,
        'quantity': quantity,
      }),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutTransfer> transfer(String accessToken) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions/current/transfer'),
      headers: await _headers(),
      body: jsonEncode({'accessToken': accessToken}),
    );
    return SelfCheckoutTransfer.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> reopen(String accessToken) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions/current/reopen'),
      headers: await _headers(),
      body: jsonEncode({'accessToken': accessToken}),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<List<SelfCheckoutWaitingItem>> listWaiting(String warehouseId) async {
    final response = await _http.get(
      _uri('/api/v1/pos/self-checkout/waiting', {'warehouseId': warehouseId}),
      headers: await _headers(authRequired: true),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      _decode(response); // throws
    }
    final body = jsonDecode(response.body);
    if (body is! List) return const [];
    return body
        .whereType<Map>()
        .map((e) => SelfCheckoutWaitingItem.fromJson(Map<String, dynamic>.from(e)))
        .toList();
  }

  Future<SelfCheckoutSession> importByCode(String codeOrQrPayload) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/import'),
      headers: await _headers(authRequired: true),
      body: jsonEncode({'codeOrQrPayload': codeOrQrPayload}),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> startAudit(String sessionId) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions/$sessionId/audit/start'),
      headers: await _headers(authRequired: true),
      body: '{}',
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> completeAudit({
    required String sessionId,
    required List<String> scannedBarcodes,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions/$sessionId/audit/complete'),
      headers: await _headers(authRequired: true),
      body: jsonEncode({'scannedBarcodes': scannedBarcodes}),
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Future<SelfCheckoutSession> close(String sessionId) async {
    final response = await _http.post(
      _uri('/api/v1/pos/self-checkout/sessions/$sessionId/close'),
      headers: await _headers(authRequired: true),
      body: '{}',
    );
    return SelfCheckoutSession.fromJson(_decode(response));
  }

  Map<String, dynamic> _decode(http.Response response) {
    if (response.statusCode < 200 || response.statusCode >= 300) {
      String message = 'Request failed (${response.statusCode})';
      try {
        final body = jsonDecode(response.body);
        if (body is Map && body['message'] != null) {
          message = body['message'].toString();
        } else {
          message = '$message: ${response.body}';
        }
      } catch (_) {
        message = '$message: ${response.body}';
      }
      throw SelfCheckoutException(message);
    }
    return jsonDecode(response.body) as Map<String, dynamic>;
  }
}
