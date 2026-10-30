import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:uuid/uuid.dart';

import '../sync/outbox.dart';
import '../db/app_database.dart';

/// Queue offline inventory mutations; server remains authority on push.
class InventoryOpsScreen extends StatefulWidget {
  const InventoryOpsScreen({super.key});

  @override
  State<InventoryOpsScreen> createState() => _InventoryOpsScreenState();
}

class _InventoryOpsScreenState extends State<InventoryOpsScreen> {
  final _uuid = const Uuid();
  String _opType = 'receive';
  final _warehouseCtrl = TextEditingController();
  final _productCtrl = TextEditingController();
  final _qtyCtrl = TextEditingController(text: '1');
  String? _message;
  List<BalancesCacheData> _balances = const [];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadBalances());
  }

  @override
  void dispose() {
    _warehouseCtrl.dispose();
    _productCtrl.dispose();
    _qtyCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadBalances() async {
    final db = context.read<AppDatabase>();
    final rows = await db.select(db.balancesCache).get();
    if (!mounted) return;
    setState(() => _balances = rows);
  }

  Future<void> _enqueue() async {
    final warehouseId = _warehouseCtrl.text.trim();
    final productId = _productCtrl.text.trim();
    final qty = double.tryParse(_qtyCtrl.text.trim());
    if (warehouseId.isEmpty || productId.isEmpty || qty == null || qty == 0) {
      setState(() => _message = 'Warehouse, product, and non-zero qty required');
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

    setState(() => _message = 'Queued $_opType offline (will push on sync)');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Inventory ops')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const Text(
            'Queue receive / issue / adjust / transfer locally. '
            'Sync push sends idempotent ops; the API posts stock.',
          ),
          const SizedBox(height: 16),
          DropdownButtonFormField<String>(
            initialValue: _opType,
            items: const [
              DropdownMenuItem(value: 'receive', child: Text('Receive')),
              DropdownMenuItem(value: 'issue', child: Text('Issue')),
              DropdownMenuItem(value: 'adjust', child: Text('Adjust')),
              DropdownMenuItem(value: 'transfer', child: Text('Transfer')),
            ],
            onChanged: (v) => setState(() => _opType = v ?? 'receive'),
            decoration: const InputDecoration(labelText: 'Operation'),
          ),
          TextField(
            controller: _warehouseCtrl,
            decoration: const InputDecoration(labelText: 'Warehouse id'),
          ),
          TextField(
            controller: _productCtrl,
            decoration: const InputDecoration(labelText: 'Product id'),
          ),
          TextField(
            controller: _qtyCtrl,
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            decoration: const InputDecoration(labelText: 'Quantity'),
          ),
          const SizedBox(height: 12),
          FilledButton(onPressed: _enqueue, child: const Text('Queue offline')),
          if (_message != null) ...[
            const SizedBox(height: 8),
            Text(_message!),
          ],
          const SizedBox(height: 24),
          Row(
            children: [
              const Text('Cached balances', style: TextStyle(fontWeight: FontWeight.bold)),
              const Spacer(),
              IconButton(onPressed: _loadBalances, icon: const Icon(Icons.refresh)),
            ],
          ),
          if (_balances.isEmpty) const Text('No balances cached — pull sync first.'),
          ..._balances.map(
            (b) => ListTile(
              dense: true,
              title: Text(b.productId),
              subtitle: Text('WH ${b.warehouseId}'),
              trailing: Text(b.quantity.toString()),
            ),
          ),
        ],
      ),
    );
  }
}
