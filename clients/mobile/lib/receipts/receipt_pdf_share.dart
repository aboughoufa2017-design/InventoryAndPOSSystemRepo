import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:share_plus/share_plus.dart';

import 'local_receipt_store.dart';
import 'receipt_pdf.dart';

/// Builds a receipt PDF to a temp file and opens the system share sheet (WhatsApp can attach it).
class ReceiptPdfShare {
  ReceiptPdfShare._();

  static Future<ShareResult> share({
    required LocalReceipt receipt,
    required ReceiptPdfLabels labels,
    String? caption,
  }) async {
    final bytes = await ReceiptPdf.build(receipt, labels: labels);
    final dir = await getTemporaryDirectory();
    final safeId = receipt.id.replaceAll(RegExp(r'[^\w\-]+'), '_');
    final file = File(p.join(dir.path, 'receipt_$safeId.pdf'));
    await file.writeAsBytes(bytes, flush: true);

    return Share.shareXFiles(
      [
        XFile(
          file.path,
          mimeType: 'application/pdf',
          name: 'receipt_$safeId.pdf',
        ),
      ],
      text: caption,
      subject: labels.receipt,
    );
  }
}
