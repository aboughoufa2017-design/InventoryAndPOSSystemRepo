import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:uuid/uuid.dart';

import '../catalog/catalog_api.dart';
import '../db/app_database.dart';
import '../l10n/locale_controller.dart';
import '../scan_go/self_checkout_api.dart';
import '../sync/outbox.dart';
import '../widgets/barcode_scanner_sheet.dart';
import '../widgets/offline_banner.dart';

/// Staff POS: offline cart, Scan & Go order-QR import, optional random audit, then pay.
class PosCheckoutScreen extends StatefulWidget {
  const PosCheckoutScreen({super.key});

  @override
  State<PosCheckoutScreen> createState() => _PosCheckoutScreenState();
}

class _PosCheckoutScreenState extends State<PosCheckoutScreen> {
  final _uuid = const Uuid();
  final _barcodeCtrl = TextEditingController();
  final _orderCodeCtrl = TextEditingController();
  final _lines = <_CartLine>[];
  String? _warehouseId;
  List<WarehouseItem> _warehouses = const [];
  String? _message;
  SelfCheckoutSession? _imported;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadWarehouses());
  }

  @override
  void dispose() {
    _barcodeCtrl.dispose();
    _orderCodeCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadWarehouses() async {
    try {
      final list = await context.read<CatalogApi>().listWarehouses();
      if (!mounted) return;
      setState(() {
        _warehouses = list;
        _warehouseId ??= list.isNotEmpty ? list.first.id : null;
      });
    } catch (e) {
      if (mounted) setState(() => _message = e.toString());
    }
  }

  Future<void> _scanBarcode() async {
    final code = await scanBarcode(context);
    if (!mounted || code == null || code.isEmpty) return;
    _barcodeCtrl.text = code;
    await _addByBarcode();
  }

  Future<void> _scanOrderQr() async {
    final code = await scanBarcode(context);
    if (!mounted || code == null || code.isEmpty) return;
    _orderCodeCtrl.text = code;
    await _importOrder();
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
      setState(() => _message = context.read<LocaleController>().s.noProducts);
      return;
    }
    final p = match.first;
    setState(() {
      final existing = _lines.where((l) => l.productId == p.id).firstOrNull;
      if (existing != null) {
        existing.quantity += 1;
      } else {
        _lines.add(
          _CartLine(
            productId: p.id,
            sku: p.sku,
            name: p.name,
            barcode: p.barcode,
            quantity: 1,
            unitPrice: 0,
          ),
        );
      }
      _barcodeCtrl.clear();
      _message = null;
    });
  }

  Future<void> _importOrder() async {
    final s = context.read<LocaleController>().s;
    final raw = _orderCodeCtrl.text.trim();
    if (raw.isEmpty) {
      setState(() => _message = s.orderQrHint);
      return;
    }
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      var session = await api.importByCode(raw);
      if (session.auditRequired && !session.auditCompleted) {
        final audited = await _runAuditFlow(session);
        if (audited == null) {
          setState(() => _message = s.auditCancelled);
          return;
        }
        session = audited;
      }
      setState(() {
        _imported = session;
        if (_warehouses.any((w) => w.id == session.warehouseId)) {
          _warehouseId = session.warehouseId;
        }
        _lines
          ..clear()
          ..addAll(
            session.lines.map(
              (l) => _CartLine(
                productId: l.productId,
                sku: l.sku,
                name: l.name,
                barcode: l.barcode,
                quantity: l.quantity,
                unitPrice: l.unitPrice,
              ),
            ),
          );
        _message =
            '${session.code} · ${session.grandTotal.toStringAsFixed(2)} ${session.currencyCode}';
      });
    } catch (e) {
      setState(() => _message = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<SelfCheckoutSession?> _runAuditFlow(SelfCheckoutSession session) async {
    final s = context.read<LocaleController>().s;
    final api = context.read<SelfCheckoutApi>();
    try {
      await api.startAudit(session.id);
    } catch (_) {}

    final targets = List<String>.from(session.auditTargetBarcodes);
    final scanned = <String>{};

    while (mounted && scanned.length < targets.length) {
      final left = targets.where((t) => !scanned.any((x) => x.toLowerCase() == t.toLowerCase())).toList();
      if (left.isEmpty) break;

      if (!mounted) return null;
      final proceed = await showDialog<bool>(
        context: context,
        barrierDismissible: false,
        builder: (ctx) => AlertDialog(
          title: Text(s.randomAudit),
          content: Text('${left.length} / ${targets.length}:\n\n${left.join('\n')}'),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(s.auditCancel)),
            FilledButton(onPressed: () => Navigator.pop(ctx, true), child: Text(s.scanNext)),
          ],
        ),
      );
      if (proceed != true || !mounted) return null;

      final code = await scanBarcode(context);
      if (!mounted || code == null || code.isEmpty) return null;
      scanned.add(code.trim());
    }

    if (!mounted) return null;
    return api.completeAudit(sessionId: session.id, scannedBarcodes: scanned.toList());
  }

  Future<void> _finalizeOffline() async {
    final s = context.read<LocaleController>().s;
    final warehouseId = _warehouseId;
    if (warehouseId == null || warehouseId.isEmpty || _lines.isEmpty) {
      setState(() => _message = s.needWarehouseCart);
      return;
    }

    if (_imported != null && _imported!.auditRequired && !_imported!.auditCompleted) {
      setState(() => _message = s.completeAuditFirst);
      return;
    }

    final commandId = _uuid.v4();
    final payload = {
      'warehouseId': warehouseId,
      'locationId': null,
      'deviceId': null,
      'customerId': null,
      'cashShiftId': null,
      'currencyCode': _imported?.currencyCode ?? 'LYD',
      'notes': _imported != null ? 'scan-go:${_imported!.code}' : 'offline-pos',
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
    final selfCheckout = context.read<SelfCheckoutApi>();
    final importedId = _imported?.id;
    await outbox.enqueue(
      opType: 'PosSale:Finalize',
      payloadJson: jsonEncode({
        ...payload,
        'clientCommandId': commandId,
        'entityType': 'PosSale',
        'operationType': 'Finalize',
      }),
    );

    if (importedId != null) {
      try {
        await selfCheckout.close(importedId);
      } catch (_) {}
    }

    if (!mounted) return;
    setState(() {
      _lines.clear();
      _imported = null;
      _orderCodeCtrl.clear();
      _message = commandId;
    });
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    final total = _lines.fold<double>(0, (sum, l) => sum + l.quantity * l.unitPrice);
    return OfflineAwareScaffold(
      appBar: AppBar(title: Text(s.posCheckout)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text(s.orderQr, style: Theme.of(context).textTheme.titleSmall),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: TextField(
                  controller: _orderCodeCtrl,
                  decoration: InputDecoration(labelText: s.orderQrHint),
                ),
              ),
              const SizedBox(width: 8),
              IconButton.filledTonal(
                tooltip: s.importOrder,
                onPressed: _busy ? null : _scanOrderQr,
                icon: const Icon(Icons.qr_code_scanner),
              ),
            ],
          ),
          const SizedBox(height: 8),
          OutlinedButton(
            onPressed: _busy ? null : _importOrder,
            child: Text(s.importOrder),
          ),
          const Divider(height: 32),
          if (_warehouses.isEmpty)
            Text(s.noWarehouses)
          else
            DropdownButtonFormField<String>(
              value: _warehouseId,
              items: _warehouses
                  .map((w) => DropdownMenuItem(value: w.id, child: Text(w.label)))
                  .toList(),
              onChanged: (v) => setState(() => _warehouseId = v),
              decoration: InputDecoration(labelText: s.warehouse),
            ),
          const SizedBox(height: 12),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: TextField(
                  controller: _barcodeCtrl,
                  decoration: InputDecoration(labelText: s.barcode),
                  onSubmitted: (_) => _addByBarcode(),
                ),
              ),
              const SizedBox(width: 8),
              IconButton.filledTonal(
                tooltip: s.scanProduct,
                onPressed: _scanBarcode,
                icon: const Icon(Icons.qr_code_scanner),
              ),
            ],
          ),
          const SizedBox(height: 8),
          FilledButton(onPressed: _addByBarcode, child: Text(s.addToCart)),
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
          Text('${s.total}: ${total.toStringAsFixed(2)}'),
          const SizedBox(height: 12),
          FilledButton(onPressed: _finalizeOffline, child: Text(s.completePay)),
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
    this.barcode,
  });

  final String productId;
  final String sku;
  final String name;
  final String? barcode;
  double quantity;
  final double unitPrice;
}
