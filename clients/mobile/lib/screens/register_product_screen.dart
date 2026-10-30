import 'dart:convert';

import 'package:drift/drift.dart' show Value;
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:provider/provider.dart';

import '../auth/auth_repository.dart';
import '../catalog/catalog_api.dart';
import '../db/app_database.dart';
import '../main.dart';
import '../widgets/barcode_scanner_sheet.dart';

class RegisterProductScreen extends StatefulWidget {
  const RegisterProductScreen({super.key});

  @override
  State<RegisterProductScreen> createState() => _RegisterProductScreenState();
}

class _RegisterProductScreenState extends State<RegisterProductScreen> {
  final _barcodeCtrl = TextEditingController();
  final _skuCtrl = TextEditingController();
  final _nameCtrl = TextEditingController();
  final _saleCtrl = TextEditingController(text: '0');
  final _purchaseCtrl = TextEditingController(text: '0');
  final _storage = const FlutterSecureStorage();

  CatalogApi? _catalog;

  List<UnitOfMeasure> _units = [];
  String? _unitId;
  var _trackInventory = true;
  var _loadingUnits = true;
  var _saving = false;
  String? _error;
  String? _success;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _catalog = CatalogApi(
        apiBaseUrl: kApiBaseUrl,
        auth: context.read<AuthRepository>(),
      );
      _loadUnits();
    });
  }

  @override
  void dispose() {
    _barcodeCtrl.dispose();
    _skuCtrl.dispose();
    _nameCtrl.dispose();
    _saleCtrl.dispose();
    _purchaseCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadUnits() async {
    final catalog = _catalog;
    if (catalog == null) return;
    setState(() {
      _loadingUnits = true;
      _error = null;
    });
    try {
      final units = await catalog.listUnits();
      final last = await _storage.read(key: CatalogApi.lastUnitKey);
      String? selected;
      if (last != null && units.any((u) => u.id == last)) {
        selected = last;
      } else if (units.isNotEmpty) {
        selected = units.first.id;
      }
      if (!mounted) return;
      setState(() {
        _units = units;
        _unitId = selected;
        _loadingUnits = false;
        if (units.isEmpty) {
          _error = 'No units of measure. Go online and ensure catalog units exist.';
        }
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loadingUnits = false;
        _units = [];
        _unitId = null;
        _error =
            'Need network to register products. ${e is CatalogApiException ? e.message : e}';
      });
    }
  }

  Future<void> _scan() async {
    final code = await scanBarcode(context);
    if (!mounted || code == null || code.isEmpty) return;
    setState(() {
      _barcodeCtrl.text = code;
      if (_skuCtrl.text.trim().isEmpty) {
        _skuCtrl.text = code;
      }
      _success = null;
    });
  }

  Future<void> _submit() async {
    final barcode = _barcodeCtrl.text.trim();
    final sku = _skuCtrl.text.trim();
    final name = _nameCtrl.text.trim();
    final unitId = _unitId;
    final sale = double.tryParse(_saleCtrl.text.trim().replaceAll(',', '.'));
    final purchase =
        double.tryParse(_purchaseCtrl.text.trim().replaceAll(',', '.')) ?? 0;

    if (sku.isEmpty || name.isEmpty || unitId == null || sale == null) {
      setState(() => _error = 'SKU, name, unit, and sale price are required.');
      return;
    }

    setState(() {
      _saving = true;
      _error = null;
      _success = null;
    });

    final catalog = _catalog;
    if (catalog == null) return;

    try {
      final created = await catalog.createProduct(
        sku: sku,
        name: name,
        unitOfMeasureId: unitId,
        purchasePrice: purchase,
        salePrice: sale,
        trackInventory: _trackInventory,
        isTaxable: false,
        barcode: barcode.isEmpty ? null : barcode,
      );

      await _storage.write(key: CatalogApi.lastUnitKey, value: unitId);

      if (!mounted) return;
      final db = context.read<AppDatabase>();
      await db.into(db.productsCache).insertOnConflictUpdate(
            ProductsCacheCompanion(
              id: Value(created.id),
              sku: Value(created.sku),
              name: Value(created.name),
              barcode: Value(created.barcode ?? (barcode.isEmpty ? null : barcode)),
              payloadJson: Value(
                jsonEncode({
                  'salePrice': created.salePrice,
                  'id': created.id,
                  'sku': created.sku,
                  'name': created.name,
                }),
              ),
            ),
          );

      if (!mounted) return;
      setState(() {
        _success = 'Registered ${created.sku} — available in POS cache.';
        _barcodeCtrl.clear();
        _skuCtrl.clear();
        _nameCtrl.clear();
        _saleCtrl.text = '0';
        _purchaseCtrl.text = '0';
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Register product')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const Text(
            'Online only: creates the product on the server, then adds it to the '
            'local POS cache. Scan a barcode or type it.',
          ),
          const SizedBox(height: 16),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: TextField(
                  controller: _barcodeCtrl,
                  decoration: const InputDecoration(labelText: 'Barcode'),
                  onChanged: (v) {
                    if (_skuCtrl.text.isEmpty || _skuCtrl.text == _barcodeCtrl.text) {
                      // keep sku in sync only when user hasn't customized yet — handled on scan
                    }
                    if (_skuCtrl.text.trim().isEmpty) {
                      _skuCtrl.text = v;
                    }
                  },
                ),
              ),
              const SizedBox(width: 8),
              IconButton.filledTonal(
                tooltip: 'Scan barcode',
                onPressed: _scan,
                icon: const Icon(Icons.qr_code_scanner),
              ),
            ],
          ),
          TextField(
            controller: _skuCtrl,
            decoration: const InputDecoration(labelText: 'SKU'),
            textCapitalization: TextCapitalization.characters,
          ),
          TextField(
            controller: _nameCtrl,
            decoration: const InputDecoration(labelText: 'Name'),
          ),
          TextField(
            controller: _saleCtrl,
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            decoration: const InputDecoration(labelText: 'Sale price'),
          ),
          TextField(
            controller: _purchaseCtrl,
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            decoration: const InputDecoration(labelText: 'Purchase price'),
          ),
          const SizedBox(height: 8),
          if (_loadingUnits)
            const LinearProgressIndicator()
          else if (_units.isEmpty)
            TextButton(
              onPressed: _loadUnits,
              child: const Text('Retry load units'),
            )
          else
            DropdownButtonFormField<String>(
              initialValue: _unitId,
              decoration: const InputDecoration(labelText: 'Unit of measure'),
              items: _units
                  .map(
                    (u) => DropdownMenuItem(
                      value: u.id,
                      child: Text('${u.code} — ${u.name}'),
                    ),
                  )
                  .toList(),
              onChanged: (v) => setState(() => _unitId = v),
            ),
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: const Text('Track inventory'),
            value: _trackInventory,
            onChanged: (v) => setState(() => _trackInventory = v),
          ),
          const SizedBox(height: 12),
          FilledButton(
            onPressed: _saving || _units.isEmpty ? null : _submit,
            child: _saving
                ? const SizedBox(
                    height: 20,
                    width: 20,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Text('Create product'),
          ),
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: TextStyle(color: Theme.of(context).colorScheme.error)),
          ],
          if (_success != null) ...[
            const SizedBox(height: 12),
            Text(_success!, style: TextStyle(color: Theme.of(context).colorScheme.primary)),
          ],
        ],
      ),
    );
  }
}
