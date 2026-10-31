import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../l10n/locale_controller.dart';
import '../scan_go/pending_transfer_store.dart';
import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import 'scan_go_cart_screen.dart';
import 'scan_go_welcome_screen.dart';

/// Guest handoff QR. Pending transfer is kept on disk until paid/expired/discarded.
class ScanGoTransferScreen extends StatefulWidget {
  const ScanGoTransferScreen({
    super.key,
    required this.transfer,
    required this.accessToken,
  });

  final SelfCheckoutTransfer transfer;
  final String accessToken;

  @override
  State<ScanGoTransferScreen> createState() => _ScanGoTransferScreenState();
}

class _ScanGoTransferScreenState extends State<ScanGoTransferScreen> {
  late SelfCheckoutTransfer _transfer;
  Timer? _poll;
  bool _paid = false;
  bool _busy = false;
  String? _statusNote;

  @override
  void initState() {
    super.initState();
    _transfer = widget.transfer;
    _poll = Timer.periodic(const Duration(seconds: 6), (_) => _refreshStatus());
    WidgetsBinding.instance.addPostFrameCallback((_) => _refreshStatus());
  }

  @override
  void dispose() {
    _poll?.cancel();
    super.dispose();
  }

  Future<void> _refreshStatus() async {
    if (_paid || !mounted || widget.accessToken.isEmpty) return;
    try {
      final session = await context.read<SelfCheckoutApi>().getCurrent(widget.accessToken);
      if (!mounted) return;
      if (PendingTransferStore.isTerminalStatus(session.status)) {
        _poll?.cancel();
        await PendingTransferStore().clear();
        if (!mounted) return;
        setState(() {
          _paid = true;
          _statusNote = context.read<LocaleController>().s.transferPaidThanks;
        });
      }
    } catch (_) {
      // Keep showing QR if offline / transient errors.
    }
  }

  Future<void> _editOrder() async {
    final s = context.read<LocaleController>().s;
    setState(() => _busy = true);
    try {
      final api = context.read<SelfCheckoutApi>();
      final session = await api.reopen(widget.accessToken);
      await PendingTransferStore().clear();
      if (!mounted) return;
      _poll?.cancel();
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ScanGoCartScreen(session: session),
        ),
      );
    } catch (_) {
      if (mounted) {
        setState(() => _statusNote = s.reopenFailed);
      }
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _goHome() {
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = context.watch<LocaleController>().s;
    return Scaffold(
      appBar: AppBar(title: Text(s.showAtCashier)),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              _paid ? (_statusNote ?? s.transferPaidThanks) : s.handQr,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: ModernTechColors.muted,
                    height: 1.4,
                  ),
            ),
            if (!_paid) ...[
              const SizedBox(height: 12),
              Text(
                s.transferQrKeptHint,
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: ModernTechColors.muted,
                    ),
              ),
            ],
            if (_statusNote != null && !_paid) ...[
              const SizedBox(height: 8),
              Text(
                _statusNote!,
                textAlign: TextAlign.center,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
            ],
            const SizedBox(height: 28),
            if (!_paid)
              Center(
                child: Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: ModernTechColors.white,
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: ModernTechColors.royalBlue.withValues(alpha: 0.15)),
                  ),
                  child: QrImageView(
                    data: _transfer.qrPayload,
                    version: QrVersions.auto,
                    size: 240,
                    backgroundColor: ModernTechColors.white,
                  ),
                ),
              )
            else
              const Icon(Icons.check_circle, size: 96, color: Colors.green),
            const SizedBox(height: 20),
            Text(
              _transfer.code,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    letterSpacing: 3,
                    fontWeight: FontWeight.w800,
                    color: ModernTechColors.royalBlue,
                  ),
            ),
            const SizedBox(height: 8),
            Text(
              '${s.total} ${_transfer.grandTotal.toStringAsFixed(2)} ${_transfer.currencyCode}',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
            ),
            const Spacer(),
            if (!_paid)
              OutlinedButton.icon(
                onPressed: _busy ? null : _editOrder,
                icon: _busy
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.edit_outlined),
                label: Text(s.editOrder),
              ),
            const SizedBox(height: 8),
            OutlinedButton(
              onPressed: _busy ? null : _goHome,
              child: Text(_paid ? s.done : s.hideQrKeepPending),
            ),
          ],
        ),
      ),
    );
  }
}
