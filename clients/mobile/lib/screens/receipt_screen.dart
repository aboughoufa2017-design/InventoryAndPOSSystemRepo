import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:share_plus/share_plus.dart';

import '../l10n/locale_controller.dart';
import '../receipts/local_receipt_store.dart';
import '../theme/modern_tech_theme.dart';
import '../widgets/offline_banner.dart';

class ReceiptScreen extends StatefulWidget {
  const ReceiptScreen({
    super.key,
    required this.receiptId,
    this.popToPos = false,
  });

  final String receiptId;
  final bool popToPos;

  @override
  State<ReceiptScreen> createState() => _ReceiptScreenState();
}

class _ReceiptScreenState extends State<ReceiptScreen> {
  LocalReceipt? _receipt;
  final _phoneCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    _phoneCtrl.dispose();
    _emailCtrl.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    final row = await context.read<LocalReceiptStore>().getById(widget.receiptId);
    if (!mounted) return;
    setState(() {
      _receipt = row;
      _phoneCtrl.text = row?.customerPhone ?? '';
      _emailCtrl.text = row?.customerEmail ?? '';
      _loading = false;
    });
  }

  String _shareBody() {
    final s = context.read<LocaleController>().s;
    final r = _receipt!;
    final buf = StringBuffer()
      ..writeln(s.brand)
      ..writeln('${s.receipt}: ${r.id}')
      ..writeln(DateFormat.yMd().add_jm().format(r.createdAt.toLocal()));
    if (r.orderCode != null && r.orderCode!.isNotEmpty) {
      buf.writeln('${s.orderCodeLabel}: ${r.orderCode}');
    }
    buf.writeln('---');
    for (final line in r.lines) {
      final lineTotal = line.quantity * line.unitPrice;
      buf.writeln(
        '${line.name} × ${line.quantity} @ ${line.unitPrice} = ${lineTotal.toStringAsFixed(2)}',
      );
    }
    buf
      ..writeln('---')
      ..writeln('${s.total}: ${r.grandTotal.toStringAsFixed(2)} ${r.currencyCode}');
    final phone = _phoneCtrl.text.trim();
    final email = _emailCtrl.text.trim();
    if (phone.isNotEmpty) buf.writeln('${s.phone}: $phone');
    if (email.isNotEmpty) buf.writeln('${s.email}: $email');
    return buf.toString();
  }

  Future<void> _share() async {
    if (_receipt == null) return;
    final store = context.read<LocalReceiptStore>();
    await store.updateContacts(
      id: _receipt!.id,
      customerPhone: _phoneCtrl.text.trim().isEmpty ? null : _phoneCtrl.text.trim(),
      customerEmail: _emailCtrl.text.trim().isEmpty ? null : _emailCtrl.text.trim(),
    );
    await Share.share(_shareBody());
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    return OfflineAwareScaffold(
      appBar: AppBar(title: Text(s.receipt)),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _receipt == null
              ? Center(child: Text(s.noReceipts))
              : ListView(
                  padding: const EdgeInsets.all(16),
                  children: [
                    Text(
                      DateFormat.yMMMd().add_jm().format(_receipt!.createdAt.toLocal()),
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                            color: ModernTechColors.muted,
                          ),
                    ),
                    if (_receipt!.orderCode != null && _receipt!.orderCode!.isNotEmpty) ...[
                      const SizedBox(height: 4),
                      Text('${s.orderCodeLabel}: ${_receipt!.orderCode}'),
                    ],
                    const SizedBox(height: 16),
                    ..._receipt!.lines.map((line) {
                      final lineTotal = line.quantity * line.unitPrice;
                      return ListTile(
                        contentPadding: EdgeInsets.zero,
                        title: Text(line.name),
                        subtitle: Text('${line.sku} × ${line.quantity}'),
                        trailing: Text(lineTotal.toStringAsFixed(2)),
                      );
                    }),
                    const Divider(),
                    Text(
                      '${s.total}: ${_receipt!.grandTotal.toStringAsFixed(2)} ${_receipt!.currencyCode}',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w700,
                          ),
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _phoneCtrl,
                      keyboardType: TextInputType.phone,
                      decoration: InputDecoration(labelText: s.customerPhone),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: _emailCtrl,
                      keyboardType: TextInputType.emailAddress,
                      decoration: InputDecoration(labelText: s.customerEmail),
                    ),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: _share,
                      icon: const Icon(Icons.share),
                      label: Text(s.shareReceipt),
                    ),
                    if (widget.popToPos) ...[
                      const SizedBox(height: 8),
                      OutlinedButton(
                        onPressed: () => Navigator.of(context).pop(),
                        child: Text(s.backToPos),
                      ),
                    ],
                  ],
                ),
    );
  }
}
