import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import 'scan_go_welcome_screen.dart';

class ScanGoTransferScreen extends StatelessWidget {
  const ScanGoTransferScreen({super.key, required this.transfer});

  final SelfCheckoutTransfer transfer;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Show at cashier')),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'Hand this QR to the cashier to pay.',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 24),
            Center(
              child: Container(
                padding: const EdgeInsets.all(16),
                color: ModernTechColors.white,
                child: QrImageView(
                  data: transfer.qrPayload,
                  version: QrVersions.auto,
                  size: 240,
                  backgroundColor: ModernTechColors.white,
                ),
              ),
            ),
            const SizedBox(height: 16),
            Text(
              transfer.code,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    letterSpacing: 4,
                    fontWeight: FontWeight.w700,
                    color: ModernTechColors.royalBlue,
                  ),
            ),
            const SizedBox(height: 8),
            Text(
              'Total ${transfer.grandTotal.toStringAsFixed(2)} ${transfer.currencyCode}',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleLarge,
            ),
            const Spacer(),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).pushAndRemoveUntil(
                  MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
                  (_) => false,
                );
              },
              child: const Text('Done'),
            ),
          ],
        ),
      ),
    );
  }
}
