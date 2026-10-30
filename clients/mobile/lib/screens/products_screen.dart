import 'package:drift/drift.dart' hide Column;
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../catalog/catalog_api.dart';
import '../db/app_database.dart';
import '../l10n/locale_controller.dart';
import '../widgets/offline_banner.dart';

class ProductsScreen extends StatefulWidget {
  const ProductsScreen({super.key});

  @override
  State<ProductsScreen> createState() => _ProductsScreenState();
}

class _ProductsScreenState extends State<ProductsScreen> {
  final _query = TextEditingController();
  List<CatalogProduct> _products = const [];
  bool _loading = true;
  bool _offline = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _refresh());
  }

  @override
  void dispose() {
    _query.dispose();
    super.dispose();
  }

  Future<void> _refresh() async {
    final s = context.read<LocaleController>().s;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final api = context.read<CatalogApi>();
      final list = await api.listProducts(activeOnly: true);
      await _replaceCache(list);
      if (!mounted) return;
      setState(() {
        _products = list;
        _offline = false;
        _loading = false;
      });
    } catch (e) {
      final db = context.read<AppDatabase>();
      final cached = await db.select(db.productsCache).get();
      if (!mounted) return;
      setState(() {
        _products = cached
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
        _offline = true;
        _loading = false;
        _error = _products.isEmpty ? e.toString() : null;
      });
      if (_products.isEmpty && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(s.noProducts)));
      }
    }
  }

  Future<void> _replaceCache(List<CatalogProduct> list) async {
    final db = context.read<AppDatabase>();
    await db.transaction(() async {
      await db.delete(db.productsCache).go();
      for (final p in list) {
        await db.into(db.productsCache).insert(
              ProductsCacheCompanion.insert(
                id: p.id,
                sku: p.sku,
                name: p.name,
                barcode: Value(p.barcode),
                payloadJson: Value(null),
              ),
            );
      }
    });
  }

  Future<void> _edit(CatalogProduct product) async {
    final s = context.read<LocaleController>().s;
    if (_offline || product.unitOfMeasureId.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(s.offlineCacheBanner)),
      );
      return;
    }
    final nameCtrl = TextEditingController(text: product.name);
    final priceCtrl = TextEditingController(text: product.salePrice.toStringAsFixed(2));
    final saved = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      builder: (ctx) {
        return Padding(
          padding: EdgeInsets.only(
            left: 16,
            right: 16,
            top: 16,
            bottom: MediaQuery.viewInsetsOf(ctx).bottom + 16,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(s.edit, style: Theme.of(ctx).textTheme.titleLarge),
              const SizedBox(height: 12),
              TextField(
                controller: nameCtrl,
                decoration: InputDecoration(labelText: s.productName),
              ),
              const SizedBox(height: 8),
              TextField(
                controller: priceCtrl,
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                decoration: InputDecoration(labelText: s.salePrice),
              ),
              if (product.barcode != null) ...[
                const SizedBox(height: 8),
                Text('${s.barcode}: ${product.barcode}'),
              ],
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () => Navigator.pop(ctx, true),
                child: Text(s.save),
              ),
              TextButton(
                onPressed: () => Navigator.pop(ctx, false),
                child: Text(s.cancel),
              ),
            ],
          ),
        );
      },
    );
    if (saved != true || !mounted) return;
    final price = double.tryParse(priceCtrl.text.trim());
    try {
      final updated = await context.read<CatalogApi>().updateProduct(
            current: product,
            name: nameCtrl.text.trim(),
            salePrice: price,
          );
      setState(() {
        _products = [
          for (final p in _products)
            if (p.id == updated.id) updated else p,
        ];
      });
      await _replaceCache(_products);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
      }
    } finally {
      nameCtrl.dispose();
      priceCtrl.dispose();
    }
  }

  Future<void> _delete(CatalogProduct product) async {
    final s = context.read<LocaleController>().s;
    if (_offline || product.unitOfMeasureId.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(s.offlineCacheBanner)),
      );
      return;
    }
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(s.delete),
        content: Text(s.confirmDelete),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(s.cancel)),
          FilledButton(onPressed: () => Navigator.pop(ctx, true), child: Text(s.delete)),
        ],
      ),
    );
    if (ok != true || !mounted) return;
    try {
      await context.read<CatalogApi>().updateProduct(current: product, isActive: false);
      final next = _products.where((p) => p.id != product.id).toList();
      setState(() => _products = next);
      await _replaceCache(next);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    final q = _query.text.trim().toLowerCase();
    final rows = _products.where((p) {
      if (q.isEmpty) return true;
      return p.sku.toLowerCase().contains(q) ||
          p.name.toLowerCase().contains(q) ||
          (p.barcode?.toLowerCase().contains(q) ?? false);
    }).toList();

    return OfflineAwareScaffold(
      appBar: AppBar(
        title: Text(s.products),
        actions: [
          IconButton(
            tooltip: s.refresh,
            onPressed: _loading ? null : _refresh,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: Column(
        children: [
          if (_offline)
            Material(
              color: Colors.amber.shade100,
              child: Padding(
                padding: const EdgeInsets.all(8),
                child: Text(s.offlineCacheBanner, textAlign: TextAlign.center),
              ),
            ),
          Padding(
            padding: const EdgeInsets.all(12),
            child: TextField(
              controller: _query,
              decoration: InputDecoration(
                labelText: s.searchProducts,
                prefixIcon: const Icon(Icons.search),
              ),
              onChanged: (_) => setState(() {}),
            ),
          ),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : rows.isEmpty
                    ? Center(child: Text(_error ?? s.noProducts))
                    : RefreshIndicator(
                        onRefresh: _refresh,
                        child: ListView.separated(
                          itemCount: rows.length,
                          separatorBuilder: (context, index) => const Divider(height: 1),
                          itemBuilder: (context, index) {
                            final p = rows[index];
                            return ListTile(
                              title: Text(p.name),
                              subtitle: Text(
                                [
                                  if (p.sku.isNotEmpty) 'SKU: ${p.sku}',
                                  if (p.barcode != null && p.barcode!.isNotEmpty)
                                    '${s.barcode}: ${p.barcode}',
                                  if (p.salePrice > 0)
                                    '${s.salePrice}: ${p.salePrice.toStringAsFixed(2)}',
                                ].join(' · '),
                              ),
                              trailing: PopupMenuButton<String>(
                                onSelected: (v) {
                                  if (v == 'edit') _edit(p);
                                  if (v == 'delete') _delete(p);
                                },
                                itemBuilder: (_) => [
                                  PopupMenuItem(value: 'edit', child: Text(s.edit)),
                                  PopupMenuItem(value: 'delete', child: Text(s.delete)),
                                ],
                              ),
                            );
                          },
                        ),
                      ),
          ),
        ],
      ),
    );
  }
}
