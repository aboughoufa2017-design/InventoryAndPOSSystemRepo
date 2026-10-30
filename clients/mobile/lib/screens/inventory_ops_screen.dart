import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:uuid/uuid.dart';

import '../catalog/catalog_api.dart';
import '../db/app_database.dart';
import '../l10n/app_strings.dart';
import '../l10n/locale_controller.dart';
import '../sync/outbox.dart';
import '../widgets/offline_banner.dart';

/// Queue offline inventory mutations; server remains authority on push.
class InventoryOpsScreen extends StatefulWidget {
  const InventoryOpsScreen({super.key});

  @override
  State<InventoryOpsScreen> createState() => _InventoryOpsScreenState();
}

class _InventoryOpsScreenState extends State<InventoryOpsScreen> {
  final _uuid = const Uuid();
  final _qtyCtrl = TextEditingController(text: '1');
  final _productFilterCtrl = TextEditingController();
  String _opType = 'receive';
  String? _warehouseId;
  String? _productId;
  String? _message;
  bool _loading = true;
  List<WarehouseItem> _warehouses = const [];
  List<CatalogProduct> _products = const [];
  List<BalancesCacheData> _balances = const [];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _bootstrap());
  }

  @override
  void dispose() {
    _qtyCtrl.dispose();
    _productFilterCtrl.dispose();
    super.dispose();
  }

  Future<void> _bootstrap() async {
    setState(() => _loading = true);
    final catalog = context.read<CatalogApi>();
    final db = context.read<AppDatabase>();
    try {
      final warehouses = await catalog.listWarehouses();
      List<CatalogProduct> products;
      try {
        products = await catalog.listProducts(activeOnly: true);
      } catch (_) {
        final cached = await db.select(db.productsCache).get();
        products = cached
            .map(
              (p) => CatalogProduct(
                id: p.id,
                sku: p.sku,
                name: p.name,
                unitOfMeasureId: '',
                purchasePrice: 0,
                salePrice: 0,
                isTaxable: false,
                trackInventory: true,
                isActive: true,
                rowVersion: 1,
                barcode: p.barcode,
              ),
            )
            .toList();
      }
      final balances = await db.select(db.balancesCache).get();
      if (!mounted) return;
      setState(() {
        _warehouses = warehouses;
        _products = products;
        _balances = balances;
        _warehouseId = warehouses.isNotEmpty ? warehouses.first.id : null;
        _productId = products.isNotEmpty ? products.first.id : null;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _message = e.toString();
      });
    }
  }

  Future<void> _enqueue() async {
    final s = context.read<LocaleController>().s;
    final warehouseId = _warehouseId;
    final productId = _productId;
    final qty = double.tryParse(_qtyCtrl.text.trim());
    if (warehouseId == null || productId == null || qty == null || qty == 0) {
      setState(() => _message = s.needWarehouseProductQty);
      return;
    }

    final outbox = context.read<Outbox>();
    await outbox.enqueue(
      opType: _opType,
      payloadJson: jsonEncode({
        'warehouseId': warehouseId,
        'productId': productId,
        'quantity': qty,
        'idempotencyKey': _uuid.v4(),
      }),
    );

    setState(() => _message = s.queuedOp(_opLabel(s, _opType)));
  }

  String _opLabel(AppStrings s, String op) {
    switch (op) {
      case 'issue':
        return s.opIssue;
      case 'adjust':
        return s.opAdjust;
      case 'transfer':
        return s.opTransfer;
      default:
        return s.opReceive;
    }
  }

  String _warehouseName(String id) {
    final match = _warehouses.where((w) => w.id == id);
    return match.isEmpty ? id : match.first.label;
  }

  String _productName(String id) {
    final match = _products.where((p) => p.id == id);
    return match.isEmpty ? id : match.first.label;
  }

  List<CatalogProduct> get _filteredProducts {
    final q = _productFilterCtrl.text.trim().toLowerCase();
    if (q.isEmpty) return _products;
    return _products
        .where(
          (p) =>
              p.name.toLowerCase().contains(q) ||
              p.sku.toLowerCase().contains(q) ||
              (p.barcode?.toLowerCase().contains(q) ?? false),
        )
        .toList();
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    final filtered = _filteredProducts;
    if (_productId != null && filtered.every((p) => p.id != _productId)) {
      // keep selection even if filtered out of dropdown items — use full list for value
    }

    return OfflineAwareScaffold(
      appBar: AppBar(title: Text(s.inventoryTitle)),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                Text(s.inventoryHint),
                const SizedBox(height: 16),
                DropdownButtonFormField<String>(
                  value: _opType,
                  items: [
                    DropdownMenuItem(value: 'receive', child: Text(s.opReceive)),
                    DropdownMenuItem(value: 'issue', child: Text(s.opIssue)),
                    DropdownMenuItem(value: 'adjust', child: Text(s.opAdjust)),
                    DropdownMenuItem(value: 'transfer', child: Text(s.opTransfer)),
                  ],
                  onChanged: (v) => setState(() => _opType = v ?? 'receive'),
                  decoration: InputDecoration(labelText: s.operation),
                ),
                const SizedBox(height: 12),
                if (_warehouses.isEmpty)
                  Text(s.noWarehouses)
                else
                  DropdownButtonFormField<String>(
                    value: _warehouseId,
                    items: _warehouses
                        .map(
                          (w) => DropdownMenuItem(value: w.id, child: Text(w.label)),
                        )
                        .toList(),
                    onChanged: (v) => setState(() => _warehouseId = v),
                    decoration: InputDecoration(labelText: s.warehouse),
                  ),
                const SizedBox(height: 12),
                TextField(
                  controller: _productFilterCtrl,
                  decoration: InputDecoration(
                    labelText: s.filterProducts,
                    prefixIcon: const Icon(Icons.search),
                  ),
                  onChanged: (_) => setState(() {}),
                ),
                const SizedBox(height: 8),
                if (_products.isEmpty)
                  Text(s.noProducts)
                else
                  DropdownButtonFormField<String>(
                    value: _productId != null &&
                            _products.any((p) => p.id == _productId)
                        ? _productId
                        : null,
                    items: filtered
                        .map(
                          (p) => DropdownMenuItem(value: p.id, child: Text(p.label)),
                        )
                        .toList(),
                    onChanged: (v) => setState(() => _productId = v),
                    decoration: InputDecoration(labelText: s.product),
                    isExpanded: true,
                  ),
                const SizedBox(height: 12),
                TextField(
                  controller: _qtyCtrl,
                  keyboardType: const TextInputType.numberWithOptions(decimal: true),
                  decoration: InputDecoration(labelText: s.quantity),
                ),
                const SizedBox(height: 12),
                FilledButton(onPressed: _enqueue, child: Text(s.queueOffline)),
                if (_message != null) ...[
                  const SizedBox(height: 8),
                  Text(_message!),
                ],
                const SizedBox(height: 24),
                Row(
                  children: [
                    Text(s.cachedBalances, style: const TextStyle(fontWeight: FontWeight.bold)),
                    const Spacer(),
                    IconButton(
                      onPressed: () async {
                        final db = context.read<AppDatabase>();
                        final rows = await db.select(db.balancesCache).get();
                        if (mounted) setState(() => _balances = rows);
                      },
                      icon: const Icon(Icons.refresh),
                    ),
                  ],
                ),
                if (_balances.isEmpty) Text(s.noBalances),
                ..._balances.map(
                  (b) => ListTile(
                    dense: true,
                    title: Text(_productName(b.productId)),
                    subtitle: Text(_warehouseName(b.warehouseId)),
                    trailing: Text(b.quantity.toString()),
                  ),
                ),
              ],
            ),
    );
  }
}
