import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';

class LocalReceiptLine {
  const LocalReceiptLine({
    required this.productId,
    required this.sku,
    required this.name,
    this.barcode,
    required this.quantity,
    required this.unitPrice,
  });

  final String productId;
  final String sku;
  final String name;
  final String? barcode;
  final double quantity;
  final double unitPrice;

  Map<String, dynamic> toJson() => {
        'productId': productId,
        'sku': sku,
        'name': name,
        'barcode': barcode,
        'quantity': quantity,
        'unitPrice': unitPrice,
      };

  factory LocalReceiptLine.fromJson(Map<String, dynamic> json) => LocalReceiptLine(
        productId: json['productId']?.toString() ?? '',
        sku: json['sku']?.toString() ?? '',
        name: json['name']?.toString() ?? '',
        barcode: json['barcode']?.toString(),
        quantity: (json['quantity'] as num?)?.toDouble() ?? 0,
        unitPrice: (json['unitPrice'] as num?)?.toDouble() ?? 0,
      );
}

class LocalReceipt {
  LocalReceipt({
    required this.id,
    required this.createdAt,
    this.orderCode,
    required this.warehouseId,
    required this.currencyCode,
    required this.grandTotal,
    required this.lines,
    this.notes,
    this.customerPhone,
    this.customerEmail,
  });

  final String id;
  final DateTime createdAt;
  final String? orderCode;
  final String warehouseId;
  final String currencyCode;
  final double grandTotal;
  final List<LocalReceiptLine> lines;
  String? notes;
  String? customerPhone;
  String? customerEmail;

  Map<String, dynamic> toJson() => {
        'id': id,
        'createdAt': createdAt.toUtc().toIso8601String(),
        'orderCode': orderCode,
        'warehouseId': warehouseId,
        'currencyCode': currencyCode,
        'grandTotal': grandTotal,
        'lines': lines.map((l) => l.toJson()).toList(),
        'notes': notes,
        'customerPhone': customerPhone,
        'customerEmail': customerEmail,
      };

  factory LocalReceipt.fromJson(Map<String, dynamic> json) => LocalReceipt(
        id: json['id']?.toString() ?? '',
        createdAt: DateTime.tryParse(json['createdAt']?.toString() ?? '')?.toUtc() ??
            DateTime.now().toUtc(),
        orderCode: json['orderCode']?.toString(),
        warehouseId: json['warehouseId']?.toString() ?? '',
        currencyCode: json['currencyCode']?.toString() ?? 'LYD',
        grandTotal: (json['grandTotal'] as num?)?.toDouble() ?? 0,
        lines: ((json['lines'] as List?) ?? const [])
            .map((e) => LocalReceiptLine.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
        notes: json['notes']?.toString(),
        customerPhone: json['customerPhone']?.toString(),
        customerEmail: json['customerEmail']?.toString(),
      );
}

/// Device-local POS receipts (JSON file) for history + share.
class LocalReceiptStore {
  List<LocalReceipt>? _cache;
  File? _file;

  Future<File> _ensureFile() async {
    if (_file != null) return _file!;
    final dir = await getApplicationDocumentsDirectory();
    _file = File(p.join(dir.path, 'local_receipts.json'));
    if (!await _file!.exists()) {
      await _file!.writeAsString('[]');
    }
    return _file!;
  }

  Future<List<LocalReceipt>> _load() async {
    if (_cache != null) return _cache!;
    final file = await _ensureFile();
    final raw = await file.readAsString();
    final decoded = jsonDecode(raw);
    if (decoded is! List) {
      _cache = [];
      return _cache!;
    }
    _cache = decoded
        .map((e) => LocalReceipt.fromJson(Map<String, dynamic>.from(e as Map)))
        .toList();
    return _cache!;
  }

  Future<void> _persist() async {
    final file = await _ensureFile();
    final list = _cache ?? [];
    await file.writeAsString(jsonEncode(list.map((e) => e.toJson()).toList()));
  }

  Future<void> insert(LocalReceipt receipt) async {
    final list = await _load();
    list.insert(0, receipt);
    await _persist();
  }

  Future<List<LocalReceipt>> listNewestFirst() async {
    final list = await _load();
    list.sort((a, b) => b.createdAt.compareTo(a.createdAt));
    return List.unmodifiable(list);
  }

  Future<LocalReceipt?> getById(String id) async {
    final list = await _load();
    for (final r in list) {
      if (r.id == id) return r;
    }
    return null;
  }

  Future<void> updateContacts({
    required String id,
    String? customerPhone,
    String? customerEmail,
  }) async {
    final list = await _load();
    for (final r in list) {
      if (r.id == id) {
        r.customerPhone = customerPhone;
        r.customerEmail = customerEmail;
        await _persist();
        return;
      }
    }
  }
}
