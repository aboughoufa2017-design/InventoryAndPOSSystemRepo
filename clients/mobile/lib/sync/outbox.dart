import 'package:drift/drift.dart';
import 'package:uuid/uuid.dart';

import '../db/app_database.dart';

/// Local durable queue of mutations to push when connectivity returns.
class Outbox {
  Outbox(this._db, {Uuid? uuid}) : _uuid = uuid ?? const Uuid();

  final AppDatabase _db;
  final Uuid _uuid;

  Future<void> enqueue({
    required String opType,
    required String payloadJson,
  }) async {
    await _db.into(_db.outboxOps).insert(
          OutboxOpsCompanion.insert(
            id: _uuid.v4(),
            opType: opType,
            payloadJson: payloadJson,
            createdAt: DateTime.now().toUtc(),
          ),
        );
  }

  Future<List<OutboxOp>> pending({int limit = 100}) {
    return (_db.select(_db.outboxOps)
          ..orderBy([(t) => OrderingTerm.asc(t.createdAt)])
          ..limit(limit))
        .get();
  }

  Future<void> remove(String id) {
    return (_db.delete(_db.outboxOps)..where((t) => t.id.equals(id))).go();
  }

  Future<void> bumpAttempts(String id) async {
    final row = await (_db.select(_db.outboxOps)
          ..where((t) => t.id.equals(id)))
        .getSingleOrNull();
    if (row == null) return;
    await (_db.update(_db.outboxOps)..where((t) => t.id.equals(id))).write(
      OutboxOpsCompanion(attempts: Value(row.attempts + 1)),
    );
  }
}
