import 'dart:convert';

import 'package:drift/drift.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:http/http.dart' as http;
import 'package:uuid/uuid.dart';

import '../auth/auth_repository.dart';
import '../db/app_database.dart';
import 'outbox.dart';

/// Push/pull against `/api/v1/sync`. Posting rules stay server-side.
class SyncClient {
  SyncClient({
    required this.apiBaseUrl,
    required AuthRepository auth,
    required AppDatabase db,
    required Outbox outbox,
    http.Client? httpClient,
    Connectivity? connectivity,
  })  : _auth = auth,
        _db = db,
        _outbox = outbox,
        _http = httpClient ?? http.Client(),
        _connectivity = connectivity ?? Connectivity();

  final String apiBaseUrl;
  final AuthRepository _auth;
  final AppDatabase _db;
  final Outbox _outbox;
  final http.Client _http;
  final Connectivity _connectivity;
  final _uuid = const Uuid();

  Uri _uri(String path) => Uri.parse('$apiBaseUrl$path');

  Future<bool> get isOnline async {
    final results = await _connectivity.checkConnectivity();
    return results.any((r) => r != ConnectivityResult.none);
  }

  Future<SyncResult> sync() async {
    if (!await isOnline) {
      return const SyncResult(pushed: 0, pulled: 0, offline: true);
    }

    final token = await _auth.accessToken;
    if (token == null || token.isEmpty) {
      throw SyncException('Not authenticated');
    }

    final pushed = await _push(token);
    final pulled = await _pull(token);
    return SyncResult(pushed: pushed, pulled: pulled);
  }

  Future<int> _push(String token) async {
    final pending = await _outbox.pending();
    if (pending.isEmpty) return 0;

    final operations = <Map<String, dynamic>>[];
    for (final op in pending) {
      final raw = jsonDecode(op.payloadJson);
      if (raw is! Map<String, dynamic>) continue;

      var entityType = raw['entityType']?.toString() ?? 'Product';
      var operationType = raw['operationType']?.toString() ?? 'Upsert';
      if (op.opType.contains(':')) {
        final parts = op.opType.split(':');
        entityType = parts[0];
        operationType = parts.length > 1 ? parts[1] : operationType;
      }

      final entityId = raw['entityId']?.toString() ?? _uuid.v4();
      final payload = Map<String, dynamic>.from(raw)
        ..remove('entityType')
        ..remove('operationType')
        ..remove('entityId')
        ..remove('clientCommandId');

      operations.add({
        'operationId': op.id,
        'entityType': entityType,
        'entityId': entityId,
        'operationType': operationType,
        'payloadJson': jsonEncode(payload),
        'clientTimestampUtc': op.createdAt.toUtc().toIso8601String(),
      });
    }

    final response = await _http.post(
      _uri('/api/v1/sync/push'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({'operations': operations}),
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      for (final op in pending) {
        await _outbox.bumpAttempts(op.id);
      }
      throw SyncException(
        'Push failed (${response.statusCode}): ${response.body}',
      );
    }

    for (final op in pending) {
      await _outbox.remove(op.id);
    }
    return pending.length;
  }

  Future<int> _pull(String token) async {
    // API maps POST /api/v1/sync/pull (GET returns 405).
    final response = await _http.post(
      _uri('/api/v1/sync/pull'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
        'Accept': 'application/json',
      },
      body: jsonEncode({
        'afterChangeVersion': 0,
        'limit': 200,
      }),
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw SyncException(
        'Pull failed (${response.statusCode}): ${response.body}',
      );
    }

    final data = jsonDecode(response.body);
    if (data is! Map<String, dynamic>) return 0;

    var count = 0;
    final changes = data['changes'];
    if (changes is List) {
      for (final raw in changes) {
        if (raw is! Map<String, dynamic>) continue;
        final entityType = raw['entityType']?.toString();
        final payloadRaw = raw['payloadJson'];
        Map<String, dynamic>? payload;
        if (payloadRaw is String) {
          final decoded = jsonDecode(payloadRaw);
          if (decoded is Map<String, dynamic>) payload = decoded;
        } else if (payloadRaw is Map<String, dynamic>) {
          payload = payloadRaw;
        }
        if (payload == null) continue;

        if (entityType == 'Product') {
          final id = payload['id']?.toString() ?? raw['entityId']?.toString();
          if (id == null) continue;
          await _db.into(_db.productsCache).insertOnConflictUpdate(
                ProductsCacheCompanion.insert(
                  id: id,
                  sku: payload['sku']?.toString() ?? '',
                  name: payload['name']?.toString() ?? '',
                  barcode: Value(payload['barcode']?.toString()),
                  payloadJson: Value(jsonEncode(payload)),
                ),
              );
          count++;
        }
      }
    }

    return count;
  }

  void dispose() => _http.close();
}

class SyncResult {
  const SyncResult({
    required this.pushed,
    required this.pulled,
    this.offline = false,
  });

  final int pushed;
  final int pulled;
  final bool offline;
}

class SyncException implements Exception {
  SyncException(this.message);
  final String message;

  @override
  String toString() => 'SyncException: $message';
}
