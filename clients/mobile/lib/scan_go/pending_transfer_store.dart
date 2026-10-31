import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';

import 'self_checkout_api.dart';

/// Guest handoff QR kept on-device until cashier closes the session (or guest discards).
class PendingTransfer {
  PendingTransfer({
    required this.transfer,
    required this.accessToken,
    this.expiresAtUtc,
    DateTime? savedAt,
  }) : savedAt = savedAt ?? DateTime.now().toUtc();

  final SelfCheckoutTransfer transfer;
  final String accessToken;
  final String? expiresAtUtc;
  final DateTime savedAt;

  bool get isPastExpiry {
    if (expiresAtUtc == null || expiresAtUtc!.isEmpty) return false;
    final exp = DateTime.tryParse(expiresAtUtc!);
    if (exp == null) return false;
    return DateTime.now().toUtc().isAfter(exp.toUtc());
  }

  Map<String, dynamic> toJson() => {
        'sessionId': transfer.sessionId,
        'code': transfer.code,
        'qrPayload': transfer.qrPayload,
        'auditRequired': transfer.auditRequired,
        'auditItemCount': transfer.auditItemCount,
        'status': transfer.status,
        'grandTotal': transfer.grandTotal,
        'currencyCode': transfer.currencyCode,
        'accessToken': accessToken,
        'expiresAtUtc': expiresAtUtc,
        'savedAt': savedAt.toIso8601String(),
      };

  factory PendingTransfer.fromJson(Map<String, dynamic> json) => PendingTransfer(
        transfer: SelfCheckoutTransfer(
          sessionId: json['sessionId'] as String? ?? '',
          code: json['code'] as String? ?? '',
          qrPayload: json['qrPayload'] as String? ?? '',
          auditRequired: json['auditRequired'] as bool? ?? false,
          auditItemCount: json['auditItemCount'] as int? ?? 0,
          status: json['status'] as String? ?? '',
          grandTotal: (json['grandTotal'] as num?)?.toDouble() ?? 0,
          currencyCode: json['currencyCode'] as String? ?? 'LYD',
        ),
        accessToken: json['accessToken'] as String? ?? '',
        expiresAtUtc: json['expiresAtUtc'] as String?,
        savedAt: DateTime.tryParse(json['savedAt'] as String? ?? '')?.toUtc() ??
            DateTime.now().toUtc(),
      );
}

class PendingTransferStore {
  static const _fileName = 'pending_scan_go_transfer.json';

  Future<File> _file() async {
    final dir = await getApplicationDocumentsDirectory();
    return File(p.join(dir.path, _fileName));
  }

  Future<PendingTransfer?> load() async {
    try {
      final f = await _file();
      if (!await f.exists()) return null;
      final raw = await f.readAsString();
      if (raw.trim().isEmpty) return null;
      final pending = PendingTransfer.fromJson(
        jsonDecode(raw) as Map<String, dynamic>,
      );
      if (pending.transfer.qrPayload.isEmpty || pending.accessToken.isEmpty) {
        await clear();
        return null;
      }
      if (pending.isPastExpiry) {
        await clear();
        return null;
      }
      return pending;
    } catch (_) {
      return null;
    }
  }

  Future<void> save(PendingTransfer pending) async {
    final f = await _file();
    await f.writeAsString(jsonEncode(pending.toJson()), flush: true);
  }

  Future<void> saveFromTransfer({
    required SelfCheckoutTransfer transfer,
    required String accessToken,
    String? expiresAtUtc,
  }) {
    return save(
      PendingTransfer(
        transfer: transfer,
        accessToken: accessToken,
        expiresAtUtc: expiresAtUtc,
      ),
    );
  }

  Future<void> clear() async {
    try {
      final f = await _file();
      if (await f.exists()) await f.delete();
    } catch (_) {}
  }

  static bool isTerminalStatus(String status) {
    final s = status.toLowerCase();
    return s == 'closed' || s == 'expired' || s == 'cancelled' || s == 'canceled';
  }
}
