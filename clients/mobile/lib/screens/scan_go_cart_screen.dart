import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../scan_go/self_checkout_api.dart';
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
        _message = 'Added';
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
      setState(() => _message = 'Cart is empty');
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
    return Scaffold(
      appBar: AppBar(
        title: const Text('Scan & Go'),
        leading: IconButton(
          icon: const Icon(Icons.close),
          onPressed: () {
            Navigator.of(context).pushReplacement(
              MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
            );
          },
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text(
            'Scan products as you shop. Show the transfer QR at checkout.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const SizedBox(height: 12),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: TextField(
                  controller: _barcodeCtrl,
                  decoration: const InputDecoration(labelText: 'Barcode'),
                  onSubmitted: (_) => _busy ? null : _add(),
                ),
              ),
              const SizedBox(width: 8),
              IconButton.filledTonal(
                onPressed: _busy ? null : _scan,
                icon: const Icon(Icons.qr_code_scanner),
              ),
            ],
          ),
          const SizedBox(height: 8),
          FilledButton(onPressed: _busy ? null : _add, child: const Text('Add item')),
          const SizedBox(height: 16),
          ..._session.lines.map(
            (l) => ListTile(
              title: Text(l.name),
              subtitle: Text('${l.sku} · ${l.quantity} × ${l.unitPrice.toStringAsFixed(2)}'),
              trailing: IconButton(
                icon: const Icon(Icons.remove_circle_outline),
                onPressed: _busy ? null : () => _remove(l),
              ),
            ),
          ),
          const Divider(),
          Text('Subtotal: ${_session.subtotal.toStringAsFixed(2)} ${_session.currencyCode}'),
          Text('Tax: ${_session.taxTotal.toStringAsFixed(2)}'),
          Text(
            'Total: ${_session.grandTotal.toStringAsFixed(2)} ${_session.currencyCode}',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 16),
          FilledButton.icon(
            onPressed: _busy ? null : _finish,
            icon: const Icon(Icons.qr_code_2),
            label: const Text('Finish & Transfer'),
          ),
          if (_message != null) ...[
            const SizedBox(height: 12),
            Text(_message!),
          ],
        ],
      ),
    );
  }
}
