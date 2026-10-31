import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../l10n/locale_controller.dart';
import '../receipts/local_receipt_store.dart';
import '../widgets/offline_banner.dart';
import 'receipt_screen.dart';

class OrderHistoryScreen extends StatefulWidget {
  const OrderHistoryScreen({super.key});

  @override
  State<OrderHistoryScreen> createState() => _OrderHistoryScreenState();
}

class _OrderHistoryScreenState extends State<OrderHistoryScreen> {
  List<LocalReceipt> _rows = const [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _refresh());
  }

  Future<void> _refresh() async {
    setState(() => _loading = true);
    final rows = await context.read<LocalReceiptStore>().listNewestFirst();
    if (!mounted) return;
    setState(() {
      _rows = rows;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    return OfflineAwareScaffold(
      appBar: AppBar(
        title: Text(s.orderHistory),
        actions: [
          IconButton(
            tooltip: s.refresh,
            onPressed: _loading ? null : _refresh,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _rows.isEmpty
              ? Center(child: Text(s.noReceipts))
              : RefreshIndicator(
                  onRefresh: _refresh,
                  child: ListView.separated(
                    itemCount: _rows.length,
                    separatorBuilder: (_, __) => const Divider(height: 1),
                    itemBuilder: (context, index) {
                      final r = _rows[index];
                      final subtitle = [
                        DateFormat.yMMMd().add_jm().format(r.createdAt.toLocal()),
                        if (r.orderCode != null && r.orderCode!.isNotEmpty) r.orderCode!,
                      ].join(' · ');
                      return ListTile(
                        title: Text(
                          '${r.grandTotal.toStringAsFixed(2)} ${r.currencyCode}',
                          style: const TextStyle(fontWeight: FontWeight.w600),
                        ),
                        subtitle: Text(subtitle),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () async {
                          await Navigator.of(context).push(
                            MaterialPageRoute<void>(
                              builder: (_) => ReceiptScreen(receiptId: r.id),
                            ),
                          );
                          if (mounted) _refresh();
                        },
                      );
                    },
                  ),
                ),
    );
  }
}
