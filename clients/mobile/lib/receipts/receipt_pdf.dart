import 'dart:typed_data';

import 'package:intl/intl.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';

import 'local_receipt_store.dart';

/// Labels passed from the UI locale (avoid depending on Flutter widgets here).
class ReceiptPdfLabels {
  const ReceiptPdfLabels({
    required this.brand,
    required this.receipt,
    required this.orderCode,
    required this.total,
    required this.item,
    required this.qty,
    required this.amount,
    this.rtl = false,
  });

  final String brand;
  final String receipt;
  final String orderCode;
  final String total;
  final String item;
  final String qty;
  final String amount;
  final bool rtl;
}

class ReceiptPdf {
  ReceiptPdf._();

  static Future<Uint8List> build(
    LocalReceipt receipt, {
    required ReceiptPdfLabels labels,
  }) async {
    // Noto supports Arabic product names / labels in the PDF.
    final base = await PdfGoogleFonts.notoNaskhArabicRegular();
    final bold = await PdfGoogleFonts.notoNaskhArabicBold();
    final theme = pw.ThemeData.withFont(base: base, bold: bold);

    final doc = pw.Document(theme: theme);
    final dateStr = DateFormat.yMMMd().add_jm().format(receipt.createdAt.toLocal());
    final currency = receipt.currencyCode;
    final textDir = labels.rtl ? pw.TextDirection.rtl : pw.TextDirection.ltr;

    doc.addPage(
      pw.MultiPage(
        pageFormat: PdfPageFormat.a4,
        margin: const pw.EdgeInsets.all(40),
        theme: theme,
        textDirection: textDir,
        build: (context) => [
          pw.Text(
            labels.brand,
            style: pw.TextStyle(
              fontSize: 22,
              fontWeight: pw.FontWeight.bold,
              color: PdfColor.fromHex('#0A42A0'),
            ),
          ),
          pw.SizedBox(height: 4),
          pw.Text(labels.receipt, style: const pw.TextStyle(fontSize: 14)),
          pw.SizedBox(height: 12),
          pw.Text('${labels.receipt}: ${receipt.id}', style: const pw.TextStyle(fontSize: 10)),
          if (receipt.orderCode != null && receipt.orderCode!.isNotEmpty)
            pw.Text(
              '${labels.orderCode}: ${receipt.orderCode}',
              style: const pw.TextStyle(fontSize: 11),
            ),
          pw.Text(dateStr, style: const pw.TextStyle(fontSize: 10, color: PdfColors.grey700)),
          pw.SizedBox(height: 16),
          pw.TableHelper.fromTextArray(
            headers: [labels.item, labels.qty, labels.amount],
            data: [
              for (final line in receipt.lines)
                [
                  line.name,
                  _fmtQty(line.quantity),
                  (line.quantity * line.unitPrice).toStringAsFixed(2),
                ],
            ],
            headerStyle: pw.TextStyle(fontWeight: pw.FontWeight.bold, color: PdfColors.white),
            headerDecoration: const pw.BoxDecoration(color: PdfColor.fromInt(0xFF0A42A0)),
            cellAlignment: pw.Alignment.centerLeft,
            cellAlignments: {
              1: pw.Alignment.centerRight,
              2: pw.Alignment.centerRight,
            },
            columnWidths: {
              0: const pw.FlexColumnWidth(3),
              1: const pw.FlexColumnWidth(1),
              2: const pw.FlexColumnWidth(1.2),
            },
          ),
          pw.SizedBox(height: 16),
          pw.Align(
            alignment: pw.Alignment.centerRight,
            child: pw.Text(
              '${labels.total}: ${receipt.grandTotal.toStringAsFixed(2)} $currency',
              style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold),
            ),
          ),
        ],
      ),
    );

    return doc.save();
  }

  static String _fmtQty(double q) =>
      q == q.roundToDouble() ? q.toStringAsFixed(0) : q.toStringAsFixed(3);
}
