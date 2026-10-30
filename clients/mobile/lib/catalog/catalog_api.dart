import 'dart:convert';

import 'package:http/http.dart' as http;

import '../auth/auth_repository.dart';

class UnitOfMeasure {
  UnitOfMeasure({
    required this.id,
    required this.code,
    required this.name,
    required this.isActive,
  });

  final String id;
  final String code;
  final String name;
  final bool isActive;

  factory UnitOfMeasure.fromJson(Map<String, dynamic> json) {
    return UnitOfMeasure(
      id: json['id'].toString(),
      code: (json['code'] ?? json['Code'] ?? '').toString(),
      name: (json['name'] ?? json['Name'] ?? '').toString(),
      isActive: json['isActive'] as bool? ?? json['IsActive'] as bool? ?? true,
    );
  }
}

class CreatedProduct {
  CreatedProduct({
    required this.id,
    required this.sku,
    required this.name,
    required this.salePrice,
    this.barcode,
  });

  final String id;
  final String sku;
  final String name;
  final double salePrice;
  final String? barcode;

  factory CreatedProduct.fromJson(Map<String, dynamic> json, {String? fallbackBarcode}) {
    final barcodes = json['barcodes'] ?? json['Barcodes'];
    String? barcode = fallbackBarcode;
    if (barcodes is List && barcodes.isNotEmpty) {
      final first = barcodes.first;
      if (first is Map) {
        barcode = (first['barcode'] ?? first['Barcode'])?.toString() ?? barcode;
      }
    }
    return CreatedProduct(
      id: json['id'].toString(),
      sku: (json['sku'] ?? json['Sku'] ?? '').toString(),
      name: (json['name'] ?? json['Name'] ?? '').toString(),
      salePrice: ((json['salePrice'] ?? json['SalePrice'] ?? 0) as num).toDouble(),
      barcode: barcode,
    );
  }
}

/// Online catalog calls for register-product (units list + create).
class CatalogApi {
  CatalogApi({
    required this.apiBaseUrl,
    required AuthRepository auth,
    http.Client? httpClient,
  })  : _auth = auth,
        _http = httpClient ?? http.Client();

  final String apiBaseUrl;
  final AuthRepository _auth;
  final http.Client _http;

  static const lastUnitKey = 'last_unit_of_measure_id';

  Uri _uri(String path) => Uri.parse('$apiBaseUrl$path');

  Future<Map<String, String>> _headers() async {
    final token = await _auth.accessToken;
    if (token == null || token.isEmpty) {
      throw AuthException('Not signed in');
    }
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    };
  }

  Future<List<UnitOfMeasure>> listUnits() async {
    final response = await _http.get(
      _uri('/api/v1/units'),
      headers: await _headers(),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw CatalogApiException(
        'Could not load units (${response.statusCode}). Go online and sync, then retry.',
        response.body,
      );
    }
    final decoded = jsonDecode(response.body);
    if (decoded is! List) {
      throw CatalogApiException('Unexpected units response', response.body);
    }
    return decoded
        .whereType<Map>()
        .map((e) => UnitOfMeasure.fromJson(Map<String, dynamic>.from(e)))
        .where((u) => u.isActive)
        .toList();
  }

  Future<CreatedProduct> createProduct({
    required String sku,
    required String name,
    required String unitOfMeasureId,
    required double purchasePrice,
    required double salePrice,
    required bool trackInventory,
    required bool isTaxable,
    String? barcode,
  }) async {
    final body = <String, dynamic>{
      'sku': sku,
      'name': name,
      'unitOfMeasureId': unitOfMeasureId,
      'purchasePrice': purchasePrice,
      'salePrice': salePrice,
      'isTaxable': isTaxable,
      'trackInventory': trackInventory,
      if (barcode != null && barcode.isNotEmpty) 'barcodes': [barcode],
    };
    final response = await _http.post(
      _uri('/api/v1/products'),
      headers: await _headers(),
      body: jsonEncode(body),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw CatalogApiException(
        'Create product failed (${response.statusCode})',
        response.body,
      );
    }
    final data = jsonDecode(response.body);
    if (data is! Map<String, dynamic>) {
      throw CatalogApiException('Unexpected create product response', response.body);
    }
    return CreatedProduct.fromJson(data, fallbackBarcode: barcode);
  }
}

class CatalogApiException implements Exception {
  CatalogApiException(this.message, [this.body]);
  final String message;
  final String? body;

  @override
  String toString() {
    if (body == null || body!.isEmpty) return message;
    return '$message: $body';
  }
}
