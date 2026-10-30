import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:uuid/uuid.dart';

import '../db/app_database.dart';
import '../sync/outbox.dart';

/// Offline POS cart: commits sale + outbox atomically via Outbox enqueue after local note.
class PosCheckoutScreen extends StatefulWidget {
  const PosCheckoutScreen({super.key});

  @override
  State<PosCheckoutScreen> createState() => _PosCheckoutScreenState();
}

class _PosCheckoutScreenState extends State<PosCheckoutScreen> {
  final _uuid = const Uuid();
  final _barcodeCtrl = TextEditingController();
  final _warehouseCtrl = TextEditingController();
  final _lines = <_CartLine>[];
  String? _message;

  @override
  void dispose() {
    _barcodeCtrl.dispose();
    _warehouseCtrl.dispose();
    super.dispose();
  }

  Future<void> _addByBarcode() async {
    final q = _barcodeCtrl.text.trim().toLowerCase();
    if (q.isEmpty) return;
    final db = context.read<AppDatabase>();
    final products = await db.select(db.productsCache).get();
    final match = products.where((p) {
      final barcode = p.barcode?.toLowerCase() ?? '';
      return barcode == q || p.sku.toLowerCase() == q || p.name.toLowerCase().contains(q);
    }).toList();
    if (match.isEmpty) {
      setState(() => _message = 'Product not in cache — sync first');
      return;
    }
    final p = match.first;
    setState(() {
      final existing = _lines.where((l) => l.productId == p.id).firstOrNull;
      if (existing != null) {
        existing.quantity += 1;
      } else {
        _lines.add(_CartLine(productId: p.id, sku: p.sku, name: p.name, quantity: 1, unitPrice: 0));
      }
      _barcodeCtrl.clear();
      _message = null;
    });
  }

  Future<void> _finalizeOffline() async {
    final warehouseId = _warehouseCtrl.text.trim();
    if (warehouseId.isEmpty || _lines.isEmpty) {
      setState(() => _message = 'Warehouse and cart required');
      return;
    }

    final commandId = _uuid.v4();
    final payload = {
      'warehouseId': warehouseId,
      'locationId': null,
      'deviceId': null,
      'customerId': null,
      'cashShiftId': null,
      'currencyCode': 'LYD',
      'notes': 'offline-pos',
      'paymentMethod': 0,
      'lines': _lines
          .map(
            (l) => {
              'productId': l.productId,
              'quantity': l.quantity,
              'unitPrice': l.unitPrice,
              'discountAmount': 0,
            },
          )
          .toList(),
    };

    final outbox = context.read<Outbox>();
    // Durable outbox — server applies PosSale/Finalize with operationId = outbox id.
    await outbox.enqueue(
      opType: 'PosSale:Finalize',
      payloadJson: jsonEncode({
        ...payload,
        'clientCommandId': commandId,
        'entityType': 'PosSale',
        'operationType': 'Finalize',
      }),
    );

    setState(() {
      _lines.clear();
      _message = 'Sale saved offline ($commandId). Sync when online.';
    });
  }

  @override
  Widget build(BuildContext context) {
    final total = _lines.fold<double>(0, (s, l) => s + l.quantity * l.unitPrice);
    return Scaffold(
      appBar: AppBar(title: const Text('POS checkout')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const Text(
            'Offline cash sale: queue PosSale Finalize to outbox. '
            'Server posts stock once (idempotent).',
          ),
          TextField(
            controller: _warehouseCtrl,
            decoration: const InputDecoration(labelText: 'Warehouse / branch id'),
          ),
          TextField(
            controller: _barcodeCtrl,
            decoration: const InputDecoration(labelText: 'Barcode / SKU'),
            onSubmitted: (_) => _addByBarcode(),
          ),
          FilledButton(onPressed: _addByBarcode, child: const Text('Add to cart')),
          const SizedBox(height: 12),
          ..._lines.map(
            (l) => ListTile(
              title: Text(l.name),
              subtitle: Text('${l.sku} × ${l.quantity}'),
              trailing: IconButton(
                icon: const Icon(Icons.remove_circle_outline),
                onPressed: () => setState(() => _lines.remove(l)),
              ),
            ),
          ),
          Text('Total (prices from cache payload may be 0): ${total.toStringAsFixed(2)}'),
          const SizedBox(height: 12),
          FilledButton(onPressed: _finalizeOffline, child: const Text('Complete offline sale')),
          if (_message != null) ...[
            const SizedBox(height: 8),
            Text(_message!),
          ],
        ],
      ),
    );
  }
}

class _CartLine {
  _CartLine({
    required this.productId,
    required this.sku,
    required this.name,
    required this.quantity,
    required this.unitPrice,
  });

  final String productId;
  final String sku;
  final String name;
  double quantity;
  final double unitPrice;
}
