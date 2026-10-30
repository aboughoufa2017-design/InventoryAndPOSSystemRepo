import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../db/app_database.dart';

class ProductsScreen extends StatefulWidget {
  const ProductsScreen({super.key});

  @override
  State<ProductsScreen> createState() => _ProductsScreenState();
}

class _ProductsScreenState extends State<ProductsScreen> {
  final _query = TextEditingController();

  @override
  void dispose() {
    _query.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final db = context.read<AppDatabase>();

    return Scaffold(
      appBar: AppBar(title: const Text('Products / barcode')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(12),
            child: TextField(
              controller: _query,
              decoration: const InputDecoration(
                labelText: 'Search SKU, name, or barcode',
                prefixIcon: Icon(Icons.search),
                border: OutlineInputBorder(),
              ),
              onChanged: (_) => setState(() {}),
            ),
          ),
          Expanded(
            child: StreamBuilder<List<ProductsCacheData>>(
              stream: db.select(db.productsCache).watch(),
              builder: (context, snapshot) {
                if (snapshot.hasError) {
                  return Center(child: Text(snapshot.error.toString()));
                }
                if (!snapshot.hasData) {
                  return const Center(child: CircularProgressIndicator());
                }
                final q = _query.text.trim().toLowerCase();
                final rows = snapshot.data!.where((p) {
                  if (q.isEmpty) return true;
                  return p.sku.toLowerCase().contains(q) ||
                      p.name.toLowerCase().contains(q) ||
                      (p.barcode?.toLowerCase().contains(q) ?? false);
                }).toList();
                if (rows.isEmpty) {
                  return const Center(
                    child: Text('No products match. Pull via Sync first.'),
                  );
                }
                return ListView.separated(
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
                            'Barcode: ${p.barcode}',
                        ].join(' · '),
                      ),
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
