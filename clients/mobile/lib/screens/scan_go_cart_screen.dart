import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../l10n/locale_controller.dart';
import '../scan_go/pending_transfer_store.dart';
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
        _message = context.read<LocaleController>().s.added;
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

  Future<void> _setQty(SelfCheckoutLine line, double quantity) async {
    setState(() => _busy = true);
    try {
      final api = context.read<SelfCheckoutApi>();
      final next = await api.setLineQuantity(
        accessToken: _session.accessToken,
        lineId: line.id,
        quantity: quantity,
      );
      setState(() => _session = next);
    } catch (e) {
      setState(() => _message = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _finish() async {
    if (_session.lines.isEmpty) {
      setState(() => _message = context.read<LocaleController>().s.emptyCart);
      return;
    }
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      final transfer = await api.transfer(_session.accessToken);
      await PendingTransferStore().saveFromTransfer(
        transfer: transfer,
        accessToken: _session.accessToken,
        expiresAtUtc: _session.expiresAtUtc,
      );
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ScanGoTransferScreen(
            transfer: transfer,
            accessToken: _session.accessToken,
          ),
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
    final s = context.watch<LocaleController>().s;
    return Scaffold(
      appBar: AppBar(
        title: Text(s.scanGo),
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
                Text(s.cartHint, style: theme.textTheme.bodyMedium?.copyWith(color: ModernTechColors.muted)),
                const SizedBox(height: 16),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _barcodeCtrl,
                        decoration: InputDecoration(labelText: s.barcode),
                        onSubmitted: (_) => _busy ? null : _add(),
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton.filled(
                      tooltip: s.scanProduct,
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
                  child: Text(s.addItem),
                ),
                const SizedBox(height: 20),
                Text(
                  s.items,
                  style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: 8),
                if (_session.lines.isEmpty)
                  Padding(
                    padding: const EdgeInsets.symmetric(vertical: 32),
                    child: Text(
                      s.cartEmptyHint,
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
                          '${l.unitPrice.toStringAsFixed(2)} · ${l.sku}',
                        ),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(
                              tooltip: s.decreaseQty,
                              icon: const Icon(Icons.remove_circle_outline),
                              onPressed: _busy
                                  ? null
                                  : () {
                                      final next = l.quantity - 1;
                                      if (next <= 0) {
                                        _remove(l);
                                      } else {
                                        _setQty(l, next);
                                      }
                                    },
                            ),
                            Text(
                              l.quantity == l.quantity.roundToDouble()
                                  ? l.quantity.toStringAsFixed(0)
                                  : l.quantity.toStringAsFixed(3),
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            IconButton(
                              tooltip: s.increaseQty,
                              icon: const Icon(Icons.add_circle_outline),
                              onPressed: _busy ? null : () => _setQty(l, l.quantity + 1),
                            ),
                          ],
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
                    _TotalRow(label: s.subtotal, value: '${_session.subtotal.toStringAsFixed(2)} ${_session.currencyCode}'),
                    _TotalRow(label: s.tax, value: _session.taxTotal.toStringAsFixed(2)),
                    _TotalRow(
                      label: s.total,
                      value: '${_session.grandTotal.toStringAsFixed(2)} ${_session.currencyCode}',
                      emphasize: true,
                    ),
                    const SizedBox(height: 12),
                    FilledButton.icon(
                      onPressed: _busy || _session.lines.isEmpty ? null : _finish,
                      icon: const Icon(Icons.qr_code_2),
                      label: Text(s.finishTransfer),
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
