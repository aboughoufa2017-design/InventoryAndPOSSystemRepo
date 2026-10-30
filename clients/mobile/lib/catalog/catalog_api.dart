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

class CatalogProduct {
  CatalogProduct({
    required this.id,
    required this.sku,
    required this.name,
    required this.unitOfMeasureId,
    required this.purchasePrice,
    required this.salePrice,
    required this.isTaxable,
    required this.trackInventory,
    required this.isActive,
    required this.rowVersion,
    this.categoryId,
    this.description,
    this.brand,
    this.minimumStock,
    this.maximumStock,
    this.reorderPoint,
    this.barcode,
    this.barcodes = const [],
  });

  final String id;
  final String sku;
  final String name;
  final String unitOfMeasureId;
  final double purchasePrice;
  final double salePrice;
  final bool isTaxable;
  final bool trackInventory;
  final bool isActive;
  final int rowVersion;
  final String? categoryId;
  final String? description;
  final String? brand;
  final double? minimumStock;
  final double? maximumStock;
  final double? reorderPoint;
  final String? barcode;
  final List<CatalogBarcode> barcodes;

  String get label => sku.isEmpty ? name : '$name · $sku';

  factory CatalogProduct.fromJson(Map<String, dynamic> json) {
    final barcodeList = <CatalogBarcode>[];
    final rawBarcodes = json['barcodes'] ?? json['Barcodes'];
    if (rawBarcodes is List) {
      for (final b in rawBarcodes) {
        if (b is Map) {
          barcodeList.add(CatalogBarcode.fromJson(Map<String, dynamic>.from(b)));
        }
      }
    }
    String? primary;
    for (final b in barcodeList) {
      if (b.isPrimary) {
        primary = b.barcode;
        break;
      }
    }
    primary ??= barcodeList.isNotEmpty ? barcodeList.first.barcode : null;

    double? asDouble(dynamic v) => v == null ? null : (v as num).toDouble();

    return CatalogProduct(
      id: json['id'].toString(),
      sku: (json['sku'] ?? json['Sku'] ?? '').toString(),
      name: (json['name'] ?? json['Name'] ?? '').toString(),
      unitOfMeasureId: (json['unitOfMeasureId'] ?? json['UnitOfMeasureId']).toString(),
      purchasePrice: ((json['purchasePrice'] ?? json['PurchasePrice'] ?? 0) as num).toDouble(),
      salePrice: ((json['salePrice'] ?? json['SalePrice'] ?? 0) as num).toDouble(),
      isTaxable: json['isTaxable'] as bool? ?? json['IsTaxable'] as bool? ?? false,
      trackInventory: json['trackInventory'] as bool? ?? json['TrackInventory'] as bool? ?? true,
      isActive: json['isActive'] as bool? ?? json['IsActive'] as bool? ?? true,
      rowVersion: (json['rowVersion'] ?? json['RowVersion'] ?? 1) as int,
      categoryId: json['categoryId']?.toString() ?? json['CategoryId']?.toString(),
      description: json['description']?.toString() ?? json['Description']?.toString(),
      brand: json['brand']?.toString() ?? json['Brand']?.toString(),
      minimumStock: asDouble(json['minimumStock'] ?? json['MinimumStock']),
      maximumStock: asDouble(json['maximumStock'] ?? json['MaximumStock']),
      reorderPoint: asDouble(json['reorderPoint'] ?? json['ReorderPoint']),
      barcode: primary,
      barcodes: barcodeList,
    );
  }
}

class CatalogBarcode {
  CatalogBarcode({required this.id, required this.barcode, required this.isPrimary});

  final String id;
  final String barcode;
  final bool isPrimary;

  factory CatalogBarcode.fromJson(Map<String, dynamic> json) => CatalogBarcode(
        id: json['id'].toString(),
        barcode: (json['barcode'] ?? json['Barcode'] ?? '').toString(),
        isPrimary: json['isPrimary'] as bool? ?? json['IsPrimary'] as bool? ?? false,
      );
}

class WarehouseItem {
  WarehouseItem({
    required this.id,
    required this.code,
    required this.name,
    required this.isActive,
  });

  final String id;
  final String code;
  final String name;
  final bool isActive;

  String get label => code.isEmpty ? name : '$name ($code)';

  factory WarehouseItem.fromJson(Map<String, dynamic> json) => WarehouseItem(
        id: json['id'].toString(),
        code: (json['code'] ?? json['Code'] ?? '').toString(),
        name: (json['name'] ?? json['Name'] ?? '').toString(),
        isActive: json['isActive'] as bool? ?? json['IsActive'] as bool? ?? true,
      );
}

typedef CreatedProduct = CatalogProduct;

/// Online catalog / warehouse calls for mobile staff screens.
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

  Uri _uri(String path, [Map<String, String>? query]) =>
      Uri.parse('$apiBaseUrl$path').replace(queryParameters: query);

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
    _ensureOk(response, 'Could not load units');
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

  Future<List<CatalogProduct>> listProducts({bool activeOnly = true, String? search}) async {
    final query = <String, String>{
      'activeOnly': activeOnly.toString(),
      if (search != null && search.isNotEmpty) 'search': search,
    };
    final response = await _http.get(
      _uri('/api/v1/products', query),
      headers: await _headers(),
    );
    _ensureOk(response, 'Could not load products');
    final decoded = jsonDecode(response.body);
    if (decoded is! List) {
      throw CatalogApiException('Unexpected products response', response.body);
    }
    return decoded
        .whereType<Map>()
        .map((e) => CatalogProduct.fromJson(Map<String, dynamic>.from(e)))
        .toList();
  }

  Future<CatalogProduct> updateProduct({
    required CatalogProduct current,
    String? name,
    double? salePrice,
    double? purchasePrice,
    bool? isActive,
  }) async {
    final body = {
      'name': name ?? current.name,
      'unitOfMeasureId': current.unitOfMeasureId,
      'purchasePrice': purchasePrice ?? current.purchasePrice,
      'salePrice': salePrice ?? current.salePrice,
      'isTaxable': current.isTaxable,
      'trackInventory': current.trackInventory,
      'isActive': isActive ?? current.isActive,
      'categoryId': current.categoryId,
      'description': current.description,
      'brand': current.brand,
      'minimumStock': current.minimumStock,
      'maximumStock': current.maximumStock,
      'reorderPoint': current.reorderPoint,
      'expectedRowVersion': current.rowVersion,
    };
    final response = await _http.put(
      _uri('/api/v1/products/${current.id}'),
      headers: await _headers(),
      body: jsonEncode(body),
    );
    _ensureOk(response, 'Update product failed');
    final data = jsonDecode(response.body);
    if (data is! Map<String, dynamic>) {
      throw CatalogApiException('Unexpected update product response', response.body);
    }
    return CatalogProduct.fromJson(data);
  }

  Future<List<WarehouseItem>> listWarehouses() async {
    final response = await _http.get(
      _uri('/api/v1/warehouses'),
      headers: await _headers(),
    );
    _ensureOk(response, 'Could not load warehouses');
    final decoded = jsonDecode(response.body);
    if (decoded is! List) {
      throw CatalogApiException('Unexpected warehouses response', response.body);
    }
    return decoded
        .whereType<Map>()
        .map((e) => WarehouseItem.fromJson(Map<String, dynamic>.from(e)))
        .where((w) => w.isActive)
        .toList();
  }

  Future<CatalogProduct> createProduct({
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
    _ensureOk(response, 'Create product failed');
    final data = jsonDecode(response.body);
    if (data is! Map<String, dynamic>) {
      throw CatalogApiException('Unexpected create product response', response.body);
    }
    return CatalogProduct.fromJson(data);
  }

  void _ensureOk(http.Response response, String message) {
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw CatalogApiException('$message (${response.statusCode})', response.body);
    }
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
