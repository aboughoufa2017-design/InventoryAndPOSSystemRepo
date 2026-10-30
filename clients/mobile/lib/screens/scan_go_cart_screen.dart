import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../l10n/app_strings.dart';
import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import '../widgets/barcode_scanner_sheet.dart';
import 'scan_go_transfer_screen.dart';
import 'scan_go_welcome_screen.dart';

class ScanGoCartScreen extends StatefulWidget {
  const ScanGoCartScreen({super.key, required this.session});

  final SelfCheckoutSession session;

  @override
  State<ScanGoCartScreen> createState() => _ScanGoCartScreenState();
}

class _ScanGoCartScreenState extends State<ScanGoCartScreen> {
  late SelfCheckoutSession _session;
  final _barcodeCtrl = TextEditingController();
  bool _busy = false;
  String? _message;

  @override
  void initState() {
    super.initState();
    _session = widget.session;
  }

  @override
  void dispose() {
    _barcodeCtrl.dispose();
    super.dispose();
  }

  Future<void> _scan() async {
    final code = await scanBarcode(context);
    if (!mounted || code == null || code.isEmpty) return;
    _barcodeCtrl.text = code;
    await _add();
  }

  Future<void> _add() async {
    final barcode = _barcodeCtrl.text.trim();
    if (barcode.isEmpty) return;
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      final next = await api.addLine(accessToken: _session.accessToken, barcode: barcode);
      setState(() {
        _session = next;
        _barcodeCtrl.clear();
        _message = S.added;
      });
    } catch (e) {
      setState(() => _message = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _remove(SelfCheckoutLine line) async {
    setState(() => _busy = true);
    try {
      final api = context.read<SelfCheckoutApi>();
      final next = await api.removeLine(accessToken: _session.accessToken, lineId: line.id);
      setState(() => _session = next);
    } catch (e) {
      setState(() => _message = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _finish() async {
    if (_session.lines.isEmpty) {
      setState(() => _message = S.emptyCart);
      return;
    }
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      final transfer = await api.transfer(_session.accessToken);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ScanGoTransferScreen(transfer: transfer),
        ),
      );
    } catch (e) {
      setState(() => _message = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        title: const Text(S.scanGo),
        leading: IconButton(
          icon: const Icon(Icons.close),
          onPressed: () {
            Navigator.of(context).pushReplacement(
              MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
            );
          },
        ),
      ),
      body: Column(
        children: [
          Expanded(
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                Text(S.cartHint, style: theme.textTheme.bodyMedium?.copyWith(color: ModernTechColors.muted)),
                const SizedBox(height: 16),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _barcodeCtrl,
                        decoration: const InputDecoration(labelText: S.barcode),
                        onSubmitted: (_) => _busy ? null : _add(),
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton.filled(
                      tooltip: S.scanProduct,
                      onPressed: _busy ? null : _scan,
                      style: IconButton.styleFrom(
                        backgroundColor: ModernTechColors.royalBlue,
                        foregroundColor: Colors.white,
                        minimumSize: const Size(52, 52),
                      ),
                      icon: const Icon(Icons.qr_code_scanner),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                FilledButton.tonal(
                  onPressed: _busy ? null : _add,
                  child: const Text(S.addItem),
                ),
                const SizedBox(height: 20),
                Text(
                  S.items,
                  style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: 8),
                if (_session.lines.isEmpty)
                  Padding(
                    padding: const EdgeInsets.symmetric(vertical: 32),
                    child: Text(
                      S.cartEmptyHint,
                      textAlign: TextAlign.center,
                      style: theme.textTheme.bodyLarge?.copyWith(color: ModernTechColors.muted),
                    ),
                  )
                else
                  ..._session.lines.map(
                    (l) => Card(
                      margin: const EdgeInsets.only(bottom: 8),
                      child: ListTile(
                        title: Text(l.name, style: const TextStyle(fontWeight: FontWeight.w600)),
                        subtitle: Text(
                          '${l.quantity} × ${l.unitPrice.toStringAsFixed(2)} ${l.sku}',
                        ),
                        trailing: IconButton(
                          icon: const Icon(Icons.remove_circle_outline),
                          onPressed: _busy ? null : () => _remove(l),
                        ),
                      ),
                    ),
                  ),
                if (_message != null) ...[
                  const SizedBox(height: 8),
                  Text(_message!, textAlign: TextAlign.center),
                ],
              ],
            ),
          ),
          Material(
            elevation: 8,
            color: ModernTechColors.white,
            child: SafeArea(
              top: false,
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _TotalRow(label: S.subtotal, value: '${_session.subtotal.toStringAsFixed(2)} ${_session.currencyCode}'),
                    _TotalRow(label: S.tax, value: _session.taxTotal.toStringAsFixed(2)),
                    _TotalRow(
                      label: S.total,
                      value: '${_session.grandTotal.toStringAsFixed(2)} ${_session.currencyCode}',
                      emphasize: true,
                    ),
                    const SizedBox(height: 12),
                    FilledButton.icon(
                      onPressed: _busy || _session.lines.isEmpty ? null : _finish,
                      icon: const Icon(Icons.qr_code_2),
                      label: const Text(S.finishTransfer),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _TotalRow extends StatelessWidget {
  const _TotalRow({required this.label, required this.value, this.emphasize = false});

  final String label;
  final String value;
  final bool emphasize;

  @override
  Widget build(BuildContext context) {
    final style = emphasize
        ? Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w800)
        : Theme.of(context).textTheme.bodyMedium;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        children: [
          Text(label, style: style),
          const Spacer(),
          Text(value, style: style),
        ],
      ),
    );
  }
}
