import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:share_plus/share_plus.dart';

import '../connectivity/connectivity_controller.dart';
import '../l10n/locale_controller.dart';
import '../receipts/local_receipt_store.dart';
import '../receipts/receipt_api.dart';
import '../receipts/receipt_share_links.dart';
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
  bool _sending = false;
  String? _status;

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

  Future<void> _persistContacts() async {
    if (_receipt == null) return;
    await context.read<LocalReceiptStore>().updateContacts(
          id: _receipt!.id,
          customerPhone: _phoneCtrl.text.trim().isEmpty ? null : _phoneCtrl.text.trim(),
          customerEmail: _emailCtrl.text.trim().isEmpty ? null : _emailCtrl.text.trim(),
        );
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
    return buf.toString();
  }

  Future<void> _share() async {
    if (_receipt == null) return;
    await _persistContacts();
    await Share.share(_shareBody());
  }

  Future<void> _shareWhatsApp() async {
    final s = context.read<LocaleController>().s;
    final phone = _phoneCtrl.text.trim();
    if (phone.isEmpty) {
      setState(() => _status = s.needPhone);
      return;
    }
    if (ReceiptShareLinks.normalizeWhatsAppDigits(phone) == null) {
      setState(() => _status = s.invalidPhoneForShare);
      return;
    }
    try {
      await _persistContacts();
      final ok = await ReceiptShareLinks.openWhatsApp(
        phone: phone,
        text: _shareBody(),
      );
      if (!mounted) return;
      setState(() => _status = ok ? s.receiptOpenedInApp : s.cannotOpenWhatsApp);
    } catch (_) {
      if (mounted) setState(() => _status = s.cannotOpenWhatsApp);
    }
  }

  Future<void> _shareDeviceSms() async {
    final s = context.read<LocaleController>().s;
    final phone = _phoneCtrl.text.trim();
    if (phone.isEmpty) {
      setState(() => _status = s.needPhone);
      return;
    }
    if (ReceiptShareLinks.normalizeSmsDigits(phone) == null) {
      setState(() => _status = s.invalidPhoneForShare);
      return;
    }
    try {
      await _persistContacts();
      final ok = await ReceiptShareLinks.openSms(
        phone: phone,
        text: _shareBody(),
      );
      if (!mounted) return;
      setState(() => _status = ok ? s.receiptOpenedInApp : s.cannotOpenSms);
    } catch (_) {
      if (mounted) setState(() => _status = s.cannotOpenSms);
    }
  }

  Future<void> _sendEmail() async {
    final s = context.read<LocaleController>().s;
    final email = _emailCtrl.text.trim();
    if (email.isEmpty) {
      setState(() => _status = s.needEmail);
      return;
    }
    if (!context.read<ConnectivityController>().isOnline) {
      setState(() => _status = s.offlineBanner);
      return;
    }
    setState(() {
      _sending = true;
      _status = s.sendingReceipt;
    });
    try {
      await _persistContacts();
      await context.read<ReceiptApi>().sendEmail(
            receipt: _receipt!,
            customerEmail: email,
          );
      if (!mounted) return;
      setState(() => _status = s.receiptSentOk);
    } catch (e) {
      if (mounted) setState(() => _status = e.toString());
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  Future<void> _sendSms() async {
    final s = context.read<LocaleController>().s;
    final phone = _phoneCtrl.text.trim();
    if (phone.isEmpty) {
      setState(() => _status = s.needPhone);
      return;
    }
    if (!context.read<ConnectivityController>().isOnline) {
      setState(() => _status = s.offlineBanner);
      return;
    }
    setState(() {
      _sending = true;
      _status = s.sendingReceipt;
    });
    try {
      await _persistContacts();
      await context.read<ReceiptApi>().sendSms(
            receipt: _receipt!,
            customerPhone: phone,
          );
      if (!mounted) return;
      setState(() => _status = s.receiptSentOk);
    } catch (e) {
      if (mounted) setState(() => _status = e.toString());
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  Future<void> _sendBoth() async {
    final s = context.read<LocaleController>().s;
    final email = _emailCtrl.text.trim();
    final phone = _phoneCtrl.text.trim();
    if (email.isEmpty && phone.isEmpty) {
      setState(() => _status = s.needEmailOrPhone);
      return;
    }
    if (!context.read<ConnectivityController>().isOnline) {
      setState(() => _status = s.offlineBanner);
      return;
    }
    setState(() {
      _sending = true;
      _status = s.sendingReceipt;
    });
    try {
      await _persistContacts();
      final result = await context.read<ReceiptApi>().sendBoth(
            receipt: _receipt!,
            customerEmail: email.isEmpty ? null : email,
            customerPhone: phone.isEmpty ? null : phone,
          );
      if (!mounted) return;
      if (result.emailSent && result.smsSent) {
        setState(() => _status = s.receiptSentOk);
      } else if (result.emailSent || result.smsSent) {
        final parts = <String>[
          s.receiptSentPartial,
          if (result.emailError != null) result.emailError!,
          if (result.smsError != null) result.smsError!,
        ];
        setState(() => _status = parts.join('\n'));
      } else {
        setState(
          () => _status = [result.emailError, result.smsError]
              .whereType<String>()
              .join('\n'),
        );
      }
    } catch (e) {
      if (mounted) setState(() => _status = e.toString());
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    final online = context.watch<ConnectivityController>().isOnline;
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
                    Row(
                      children: [
                        Expanded(
                          child: FilledButton.icon(
                            onPressed: _sending ? null : _shareWhatsApp,
                            icon: const Icon(Icons.chat),
                            label: Text(s.shareViaWhatsApp),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: FilledButton.tonalIcon(
                            onPressed: _sending ? null : _shareDeviceSms,
                            icon: const Icon(Icons.sms),
                            label: Text(s.shareViaDeviceSms),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    OutlinedButton.icon(
                      onPressed: _sending ? null : _share,
                      icon: const Icon(Icons.share),
                      label: Text(s.shareReceipt),
                    ),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: (_sending || !online) ? null : _sendBoth,
                      icon: _sending
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                            )
                          : const Icon(Icons.send),
                      label: Text(s.sendEmailAndSms),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton.icon(
                            onPressed: (_sending || !online) ? null : _sendSms,
                            icon: const Icon(Icons.sms_outlined),
                            label: Text(s.sendSms),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: OutlinedButton.icon(
                            onPressed: (_sending || !online) ? null : _sendEmail,
                            icon: const Icon(Icons.email_outlined),
                            label: Text(s.sendEmail),
                          ),
                        ),
                      ],
                    ),
                    if (_status != null) ...[
                      const SizedBox(height: 12),
                      Text(_status!, textAlign: TextAlign.center),
                    ],
                    if (widget.popToPos) ...[
                      const SizedBox(height: 8),
                      TextButton(
                        onPressed: () => Navigator.of(context).pop(),
                        child: Text(s.backToPos),
                      ),
                    ],
                  ],
                ),
    );
  }
}
