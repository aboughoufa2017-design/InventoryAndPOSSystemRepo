import 'package:drift/drift.dart';
import 'package:drift_flutter/drift_flutter.dart';

part 'app_database.g.dart';

/// Local SQLite cache and outbox for offline-first mobile.
class OutboxOps extends Table {
  TextColumn get id => text()();
  TextColumn get opType => text()();
  TextColumn get payloadJson => text()();
  DateTimeColumn get createdAt => dateTime()();
  IntColumn get attempts => integer().withDefault(const Constant(0))();

  @override
  Set<Column<Object>> get primaryKey => {id};
}

class ProductsCache extends Table {
  TextColumn get id => text()();
  TextColumn get sku => text()();
  TextColumn get name => text()();
  TextColumn get barcode => text().nullable()();
  TextColumn get payloadJson => text().nullable()();

  @override
  Set<Column<Object>> get primaryKey => {id};
}

class BalancesCache extends Table {
  TextColumn get id => text()();
  TextColumn get warehouseId => text()();
  TextColumn get productId => text()();
  RealColumn get quantity => real()();

  @override
  Set<Column<Object>> get primaryKey => {id};
}

@DriftDatabase(tables: [OutboxOps, ProductsCache, BalancesCache])
class AppDatabase extends _$AppDatabase {
  AppDatabase([QueryExecutor? executor])
      : super(executor ?? driftDatabase(name: 'warehouse_mobile'));

  @override
  int get schemaVersion => 1;
}
