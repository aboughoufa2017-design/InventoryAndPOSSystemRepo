import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../l10n/locale_controller.dart';
import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import 'scan_go_welcome_screen.dart';

class ScanGoTransferScreen extends StatelessWidget {
  const ScanGoTransferScreen({super.key, required this.transfer});

  final SelfCheckoutTransfer transfer;

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
              s.handQr,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: ModernTechColors.muted,
                    height: 1.4,
                  ),
            ),
            const SizedBox(height: 28),
            Center(
              child: Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: ModernTechColors.white,
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: ModernTechColors.royalBlue.withValues(alpha: 0.15)),
                ),
                child: QrImageView(
                  data: transfer.qrPayload,
                  version: QrVersions.auto,
                  size: 240,
                  backgroundColor: ModernTechColors.white,
                ),
              ),
            ),
            const SizedBox(height: 20),
            Text(
              transfer.code,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    letterSpacing: 3,
                    fontWeight: FontWeight.w800,
                    color: ModernTechColors.royalBlue,
                  ),
            ),
            const SizedBox(height: 8),
            Text(
              '${s.total} ${transfer.grandTotal.toStringAsFixed(2)} ${transfer.currencyCode}',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
            ),
            const Spacer(),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).pushAndRemoveUntil(
                  MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
                  (_) => false,
                );
              },
              child: Text(s.done),
            ),
          ],
        ),
      ),
    );
  }
}
