import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../l10n/locale_controller.dart';
import '../scan_go/pending_transfer_store.dart';
import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import 'login_screen.dart';
import 'scan_go_cart_screen.dart';
import 'scan_go_transfer_screen.dart';

/// Guest-only entry for Scan & Go. Staff login is a discreet secondary action.
class ScanGoWelcomeScreen extends StatefulWidget {
  const ScanGoWelcomeScreen({super.key});

  @override
  State<ScanGoWelcomeScreen> createState() => _ScanGoWelcomeScreenState();
}

class _ScanGoWelcomeScreenState extends State<ScanGoWelcomeScreen> {
  final _tenantCtrl = TextEditingController();
  final _pendingStore = PendingTransferStore();
  bool _busy = false;
  bool _checkingPending = true;
  PendingTransfer? _pending;
  String? _error;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadPending());
  }

  @override
  void dispose() {
    _tenantCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadPending() async {
    setState(() => _checkingPending = true);
    var pending = await _pendingStore.load();
    if (!mounted) return;
    final api = context.read<SelfCheckoutApi>();
    if (pending != null && pending.accessToken.isNotEmpty) {
      try {
        final session = await api.getCurrent(pending.accessToken);
        if (PendingTransferStore.isTerminalStatus(session.status)) {
          await _pendingStore.clear();
          pending = null;
        } else if (session.qrPayload.isNotEmpty) {
          // Refresh totals/status from server while keeping the same handoff code.
          pending = PendingTransfer(
            transfer: SelfCheckoutTransfer(
              sessionId: session.id,
              code: session.code.isNotEmpty ? session.code : pending.transfer.code,
              qrPayload:
                  session.qrPayload.isNotEmpty ? session.qrPayload : pending.transfer.qrPayload,
              auditRequired: session.auditRequired,
              auditItemCount: session.auditItemCount,
              status: session.status,
              grandTotal: session.grandTotal,
              currencyCode: session.currencyCode,
            ),
            accessToken: pending.accessToken,
            expiresAtUtc: session.expiresAtUtc.isNotEmpty
                ? session.expiresAtUtc
                : pending.expiresAtUtc,
            savedAt: pending.savedAt,
          );
          await _pendingStore.save(pending);
        }
      } catch (_) {
        // Keep local pending if offline.
      }
    }
    if (!mounted) return;
    setState(() {
      _pending = pending;
      _checkingPending = false;
    });
  }

  void _openPendingQr() {
    final pending = _pending;
    if (pending == null) return;
    Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => ScanGoTransferScreen(
          transfer: pending.transfer,
          accessToken: pending.accessToken,
        ),
      ),
    ).then((_) {
      if (mounted) _loadPending();
    });
  }

  Future<void> _discardPending() async {
    final s = context.read<LocaleController>().s;
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        content: Text(s.discardSavedQrConfirm),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(s.cancel)),
          FilledButton(onPressed: () => Navigator.pop(ctx, true), child: Text(s.startNewShop)),
        ],
      ),
    );
    if (ok != true) return;
    await _pendingStore.clear();
    if (!mounted) return;
    setState(() => _pending = null);
  }

  Future<void> _startGuest() async {
    final s = context.read<LocaleController>().s;
    if (_pending != null) {
      final ok = await showDialog<bool>(
        context: context,
        builder: (ctx) => AlertDialog(
          content: Text(s.discardSavedQrConfirm),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(s.cancel)),
            TextButton(onPressed: () => Navigator.pop(ctx, null), child: Text(s.showSavedQr)),
            FilledButton(onPressed: () => Navigator.pop(ctx, true), child: Text(s.startNewShop)),
          ],
        ),
      );
      if (ok == null) {
        _openPendingQr();
        return;
      }
      if (ok != true) return;
      await _pendingStore.clear();
      if (!mounted) return;
      setState(() => _pending = null);
    }

    final code = _tenantCtrl.text.trim();
    if (code.isEmpty) {
      setState(() => _error = s.needStoreCode);
      return;
    }
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      final session = await api.startGuest(tenantCode: code);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ScanGoCartScreen(session: session),
        ),
      );
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = context.watch<LocaleController>();
    final s = locale.s;
    final theme = Theme.of(context);
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              Color(0xFFE8EEF8),
              ModernTechColors.ice,
              ModernTechColors.white,
            ],
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(24, 16, 24, 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: TextButton(
                    onPressed: () => locale.toggle(),
                    child: Text(s.languageToggle),
                  ),
                ),
                const Spacer(flex: 2),
                Text(
                  s.brand,
                  textAlign: TextAlign.center,
                  style: theme.textTheme.displaySmall?.copyWith(
                    color: ModernTechColors.royalBlue,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  s.guestTagline,
                  textAlign: TextAlign.center,
                  style: theme.textTheme.titleMedium?.copyWith(
                    color: ModernTechColors.muted,
                    height: 1.4,
                  ),
                ),
                if (_checkingPending) ...[
                  const SizedBox(height: 24),
                  const Center(child: CircularProgressIndicator()),
                ] else if (_pending != null) ...[
                  const SizedBox(height: 28),
                  FilledButton.icon(
                    onPressed: _busy ? null : _openPendingQr,
                    icon: const Icon(Icons.qr_code_2),
                    label: Text(s.showSavedQr),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    '${_pending!.transfer.code} · ${_pending!.transfer.grandTotal.toStringAsFixed(2)} ${_pending!.transfer.currencyCode}',
                    textAlign: TextAlign.center,
                    style: theme.textTheme.bodyMedium?.copyWith(color: ModernTechColors.muted),
                  ),
                  TextButton(
                    onPressed: _busy ? null : _discardPending,
                    child: Text(s.startNewShop),
                  ),
                  const SizedBox(height: 12),
                ] else ...[
                  const SizedBox(height: 40),
                ],
                TextField(
                  controller: _tenantCtrl,
                  textCapitalization: TextCapitalization.characters,
                  textInputAction: TextInputAction.done,
                  onSubmitted: (_) => _busy ? null : _startGuest(),
                  decoration: InputDecoration(
                    labelText: s.storeCode,
                    hintText: s.storeCodeHint,
                    prefixIcon: const Icon(Icons.storefront_outlined),
                  ),
                ),
                if (_error != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    _error!,
                    textAlign: TextAlign.center,
                    style: TextStyle(color: theme.colorScheme.error),
                  ),
                ],
                const SizedBox(height: 20),
                FilledButton(
                  onPressed: _busy || _checkingPending ? null : _startGuest,
                  child: _busy
                      ? const SizedBox(
                          height: 22,
                          width: 22,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                        )
                      : Text(s.continueGuest),
                ),
                const Spacer(flex: 3),
                TextButton(
                  onPressed: _busy
                      ? null
                      : () {
                          Navigator.of(context).push(
                            MaterialPageRoute<void>(
                              builder: (_) => const LoginScreen(),
                            ),
                          );
                        },
                  child: Text(s.staffEntry),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
