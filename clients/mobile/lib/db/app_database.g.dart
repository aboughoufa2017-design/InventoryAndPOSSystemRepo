// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_database.dart';

// ignore_for_file: type=lint
class $OutboxOpsTable extends OutboxOps
    with TableInfo<$OutboxOpsTable, OutboxOp> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $OutboxOpsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<String> id = GeneratedColumn<String>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _opTypeMeta = const VerificationMeta('opType');
  @override
  late final GeneratedColumn<String> opType = GeneratedColumn<String>(
    'op_type',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _payloadJsonMeta = const VerificationMeta(
    'payloadJson',
  );
  @override
  late final GeneratedColumn<String> payloadJson = GeneratedColumn<String>(
    'payload_json',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _createdAtMeta = const VerificationMeta(
    'createdAt',
  );
  @override
  late final GeneratedColumn<DateTime> createdAt = GeneratedColumn<DateTime>(
    'created_at',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _attemptsMeta = const VerificationMeta(
    'attempts',
  );
  @override
  late final GeneratedColumn<int> attempts = GeneratedColumn<int>(
    'attempts',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: false,
    defaultValue: const Constant(0),
  );
  @override
  List<GeneratedColumn> get $columns => [
    id,
    opType,
    payloadJson,
    createdAt,
    attempts,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'outbox_ops';
  @override
  VerificationContext validateIntegrity(
    Insertable<OutboxOp> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    } else if (isInserting) {
      context.missing(_idMeta);
    }
    if (data.containsKey('op_type')) {
      context.handle(
        _opTypeMeta,
        opType.isAcceptableOrUnknown(data['op_type']!, _opTypeMeta),
      );
    } else if (isInserting) {
      context.missing(_opTypeMeta);
    }
    if (data.containsKey('payload_json')) {
      context.handle(
        _payloadJsonMeta,
        payloadJson.isAcceptableOrUnknown(
          data['payload_json']!,
          _payloadJsonMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_payloadJsonMeta);
    }
    if (data.containsKey('created_at')) {
      context.handle(
        _createdAtMeta,
        createdAt.isAcceptableOrUnknown(data['created_at']!, _createdAtMeta),
      );
    } else if (isInserting) {
      context.missing(_createdAtMeta);
    }
    if (data.containsKey('attempts')) {
      context.handle(
        _attemptsMeta,
        attempts.isAcceptableOrUnknown(data['attempts']!, _attemptsMeta),
      );
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  OutboxOp map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return OutboxOp(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}id'],
      )!,
      opType: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}op_type'],
      )!,
      payloadJson: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}payload_json'],
      )!,
      createdAt: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}created_at'],
      )!,
      attempts: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}attempts'],
      )!,
    );
  }

  @override
  $OutboxOpsTable createAlias(String alias) {
    return $OutboxOpsTable(attachedDatabase, alias);
  }
}

class OutboxOp extends DataClass implements Insertable<OutboxOp> {
  final String id;
  final String opType;
  final String payloadJson;
  final DateTime createdAt;
  final int attempts;
  const OutboxOp({
    required this.id,
    required this.opType,
    required this.payloadJson,
    required this.createdAt,
    required this.attempts,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<String>(id);
    map['op_type'] = Variable<String>(opType);
    map['payload_json'] = Variable<String>(payloadJson);
    map['created_at'] = Variable<DateTime>(createdAt);
    map['attempts'] = Variable<int>(attempts);
    return map;
  }

  OutboxOpsCompanion toCompanion(bool nullToAbsent) {
    return OutboxOpsCompanion(
      id: Value(id),
      opType: Value(opType),
      payloadJson: Value(payloadJson),
      createdAt: Value(createdAt),
      attempts: Value(attempts),
    );
  }

  factory OutboxOp.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return OutboxOp(
      id: serializer.fromJson<String>(json['id']),
      opType: serializer.fromJson<String>(json['opType']),
      payloadJson: serializer.fromJson<String>(json['payloadJson']),
      createdAt: serializer.fromJson<DateTime>(json['createdAt']),
      attempts: serializer.fromJson<int>(json['attempts']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<String>(id),
      'opType': serializer.toJson<String>(opType),
      'payloadJson': serializer.toJson<String>(payloadJson),
      'createdAt': serializer.toJson<DateTime>(createdAt),
      'attempts': serializer.toJson<int>(attempts),
    };
  }

  OutboxOp copyWith({
    String? id,
    String? opType,
    String? payloadJson,
    DateTime? createdAt,
    int? attempts,
  }) => OutboxOp(
    id: id ?? this.id,
    opType: opType ?? this.opType,
    payloadJson: payloadJson ?? this.payloadJson,
    createdAt: createdAt ?? this.createdAt,
    attempts: attempts ?? this.attempts,
  );
  OutboxOp copyWithCompanion(OutboxOpsCompanion data) {
    return OutboxOp(
      id: data.id.present ? data.id.value : this.id,
      opType: data.opType.present ? data.opType.value : this.opType,
      payloadJson: data.payloadJson.present
          ? data.payloadJson.value
          : this.payloadJson,
      createdAt: data.createdAt.present ? data.createdAt.value : this.createdAt,
      attempts: data.attempts.present ? data.attempts.value : this.attempts,
    );
  }

  @override
  String toString() {
    return (StringBuffer('OutboxOp(')
          ..write('id: $id, ')
          ..write('opType: $opType, ')
          ..write('payloadJson: $payloadJson, ')
          ..write('createdAt: $createdAt, ')
          ..write('attempts: $attempts')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(id, opType, payloadJson, createdAt, attempts);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is OutboxOp &&
          other.id == this.id &&
          other.opType == this.opType &&
          other.payloadJson == this.payloadJson &&
          other.createdAt == this.createdAt &&
          other.attempts == this.attempts);
}

class OutboxOpsCompanion extends UpdateCompanion<OutboxOp> {
  final Value<String> id;
  final Value<String> opType;
  final Value<String> payloadJson;
  final Value<DateTime> createdAt;
  final Value<int> attempts;
  final Value<int> rowid;
  const OutboxOpsCompanion({
    this.id = const Value.absent(),
    this.opType = const Value.absent(),
    this.payloadJson = const Value.absent(),
    this.createdAt = const Value.absent(),
    this.attempts = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  OutboxOpsCompanion.insert({
    required String id,
    required String opType,
    required String payloadJson,
    required DateTime createdAt,
    this.attempts = const Value.absent(),
    this.rowid = const Value.absent(),
  }) : id = Value(id),
       opType = Value(opType),
       payloadJson = Value(payloadJson),
       createdAt = Value(createdAt);
  static Insertable<OutboxOp> custom({
    Expression<String>? id,
    Expression<String>? opType,
    Expression<String>? payloadJson,
    Expression<DateTime>? createdAt,
    Expression<int>? attempts,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (opType != null) 'op_type': opType,
      if (payloadJson != null) 'payload_json': payloadJson,
      if (createdAt != null) 'created_at': createdAt,
      if (attempts != null) 'attempts': attempts,
      if (rowid != null) 'rowid': rowid,
    });
  }

  OutboxOpsCompanion copyWith({
    Value<String>? id,
    Value<String>? opType,
    Value<String>? payloadJson,
    Value<DateTime>? createdAt,
    Value<int>? attempts,
    Value<int>? rowid,
  }) {
    return OutboxOpsCompanion(
      id: id ?? this.id,
      opType: opType ?? this.opType,
      payloadJson: payloadJson ?? this.payloadJson,
      createdAt: createdAt ?? this.createdAt,
      attempts: attempts ?? this.attempts,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<String>(id.value);
    }
    if (opType.present) {
      map['op_type'] = Variable<String>(opType.value);
    }
    if (payloadJson.present) {
      map['payload_json'] = Variable<String>(payloadJson.value);
    }
    if (createdAt.present) {
      map['created_at'] = Variable<DateTime>(createdAt.value);
    }
    if (attempts.present) {
      map['attempts'] = Variable<int>(attempts.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('OutboxOpsCompanion(')
          ..write('id: $id, ')
          ..write('opType: $opType, ')
          ..write('payloadJson: $payloadJson, ')
          ..write('createdAt: $createdAt, ')
          ..write('attempts: $attempts, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $ProductsCacheTable extends ProductsCache
    with TableInfo<$ProductsCacheTable, ProductsCacheData> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $ProductsCacheTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<String> id = GeneratedColumn<String>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _skuMeta = const VerificationMeta('sku');
  @override
  late final GeneratedColumn<String> sku = GeneratedColumn<String>(
    'sku',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _barcodeMeta = const VerificationMeta(
    'barcode',
  );
  @override
  late final GeneratedColumn<String> barcode = GeneratedColumn<String>(
    'barcode',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _payloadJsonMeta = const VerificationMeta(
    'payloadJson',
  );
  @override
  late final GeneratedColumn<String> payloadJson = GeneratedColumn<String>(
    'payload_json',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  @override
  List<GeneratedColumn> get $columns => [id, sku, name, barcode, payloadJson];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'products_cache';
  @override
  VerificationContext validateIntegrity(
    Insertable<ProductsCacheData> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    } else if (isInserting) {
      context.missing(_idMeta);
    }
    if (data.containsKey('sku')) {
      context.handle(
        _skuMeta,
        sku.isAcceptableOrUnknown(data['sku']!, _skuMeta),
      );
    } else if (isInserting) {
      context.missing(_skuMeta);
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('barcode')) {
      context.handle(
        _barcodeMeta,
        barcode.isAcceptableOrUnknown(data['barcode']!, _barcodeMeta),
      );
    }
    if (data.containsKey('payload_json')) {
      context.handle(
        _payloadJsonMeta,
        payloadJson.isAcceptableOrUnknown(
          data['payload_json']!,
          _payloadJsonMeta,
        ),
      );
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  ProductsCacheData map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return ProductsCacheData(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}id'],
      )!,
      sku: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}sku'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      barcode: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}barcode'],
      ),
      payloadJson: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}payload_json'],
      ),
    );
  }

  @override
  $ProductsCacheTable createAlias(String alias) {
    return $ProductsCacheTable(attachedDatabase, alias);
  }
}

class ProductsCacheData extends DataClass
    implements Insertable<ProductsCacheData> {
  final String id;
  final String sku;
  final String name;
  final String? barcode;
  final String? payloadJson;
  const ProductsCacheData({
    required this.id,
    required this.sku,
    required this.name,
    this.barcode,
    this.payloadJson,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<String>(id);
    map['sku'] = Variable<String>(sku);
    map['name'] = Variable<String>(name);
    if (!nullToAbsent || barcode != null) {
      map['barcode'] = Variable<String>(barcode);
    }
    if (!nullToAbsent || payloadJson != null) {
      map['payload_json'] = Variable<String>(payloadJson);
    }
    return map;
  }

  ProductsCacheCompanion toCompanion(bool nullToAbsent) {
    return ProductsCacheCompanion(
      id: Value(id),
      sku: Value(sku),
      name: Value(name),
      barcode: barcode == null && nullToAbsent
          ? const Value.absent()
          : Value(barcode),
      payloadJson: payloadJson == null && nullToAbsent
          ? const Value.absent()
          : Value(payloadJson),
    );
  }

  factory ProductsCacheData.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return ProductsCacheData(
      id: serializer.fromJson<String>(json['id']),
      sku: serializer.fromJson<String>(json['sku']),
      name: serializer.fromJson<String>(json['name']),
      barcode: serializer.fromJson<String?>(json['barcode']),
      payloadJson: serializer.fromJson<String?>(json['payloadJson']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<String>(id),
      'sku': serializer.toJson<String>(sku),
      'name': serializer.toJson<String>(name),
      'barcode': serializer.toJson<String?>(barcode),
      'payloadJson': serializer.toJson<String?>(payloadJson),
    };
  }

  ProductsCacheData copyWith({
    String? id,
    String? sku,
    String? name,
    Value<String?> barcode = const Value.absent(),
    Value<String?> payloadJson = const Value.absent(),
  }) => ProductsCacheData(
    id: id ?? this.id,
    sku: sku ?? this.sku,
    name: name ?? this.name,
    barcode: barcode.present ? barcode.value : this.barcode,
    payloadJson: payloadJson.present ? payloadJson.value : this.payloadJson,
  );
  ProductsCacheData copyWithCompanion(ProductsCacheCompanion data) {
    return ProductsCacheData(
      id: data.id.present ? data.id.value : this.id,
      sku: data.sku.present ? data.sku.value : this.sku,
      name: data.name.present ? data.name.value : this.name,
      barcode: data.barcode.present ? data.barcode.value : this.barcode,
      payloadJson: data.payloadJson.present
          ? data.payloadJson.value
          : this.payloadJson,
    );
  }

  @override
  String toString() {
    return (StringBuffer('ProductsCacheData(')
          ..write('id: $id, ')
          ..write('sku: $sku, ')
          ..write('name: $name, ')
          ..write('barcode: $barcode, ')
          ..write('payloadJson: $payloadJson')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(id, sku, name, barcode, payloadJson);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is ProductsCacheData &&
          other.id == this.id &&
          other.sku == this.sku &&
          other.name == this.name &&
          other.barcode == this.barcode &&
          other.payloadJson == this.payloadJson);
}

class ProductsCacheCompanion extends UpdateCompanion<ProductsCacheData> {
  final Value<String> id;
  final Value<String> sku;
  final Value<String> name;
  final Value<String?> barcode;
  final Value<String?> payloadJson;
  final Value<int> rowid;
  const ProductsCacheCompanion({
    this.id = const Value.absent(),
    this.sku = const Value.absent(),
    this.name = const Value.absent(),
    this.barcode = const Value.absent(),
    this.payloadJson = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  ProductsCacheCompanion.insert({
    required String id,
    required String sku,
    required String name,
    this.barcode = const Value.absent(),
    this.payloadJson = const Value.absent(),
    this.rowid = const Value.absent(),
  }) : id = Value(id),
       sku = Value(sku),
       name = Value(name);
  static Insertable<ProductsCacheData> custom({
    Expression<String>? id,
    Expression<String>? sku,
    Expression<String>? name,
    Expression<String>? barcode,
    Expression<String>? payloadJson,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (sku != null) 'sku': sku,
      if (name != null) 'name': name,
      if (barcode != null) 'barcode': barcode,
      if (payloadJson != null) 'payload_json': payloadJson,
      if (rowid != null) 'rowid': rowid,
    });
  }

  ProductsCacheCompanion copyWith({
    Value<String>? id,
    Value<String>? sku,
    Value<String>? name,
    Value<String?>? barcode,
    Value<String?>? payloadJson,
    Value<int>? rowid,
  }) {
    return ProductsCacheCompanion(
      id: id ?? this.id,
      sku: sku ?? this.sku,
      name: name ?? this.name,
      barcode: barcode ?? this.barcode,
      payloadJson: payloadJson ?? this.payloadJson,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<String>(id.value);
    }
    if (sku.present) {
      map['sku'] = Variable<String>(sku.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (barcode.present) {
      map['barcode'] = Variable<String>(barcode.value);
    }
    if (payloadJson.present) {
      map['payload_json'] = Variable<String>(payloadJson.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('ProductsCacheCompanion(')
          ..write('id: $id, ')
          ..write('sku: $sku, ')
          ..write('name: $name, ')
          ..write('barcode: $barcode, ')
          ..write('payloadJson: $payloadJson, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $BalancesCacheTable extends BalancesCache
    with TableInfo<$BalancesCacheTable, BalancesCacheData> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $BalancesCacheTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<String> id = GeneratedColumn<String>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _warehouseIdMeta = const VerificationMeta(
    'warehouseId',
  );
  @override
  late final GeneratedColumn<String> warehouseId = GeneratedColumn<String>(
    'warehouse_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _productIdMeta = const VerificationMeta(
    'productId',
  );
  @override
  late final GeneratedColumn<String> productId = GeneratedColumn<String>(
    'product_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _quantityMeta = const VerificationMeta(
    'quantity',
  );
  @override
  late final GeneratedColumn<double> quantity = GeneratedColumn<double>(
    'quantity',
    aliasedName,
    false,
    type: DriftSqlType.double,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [id, warehouseId, productId, quantity];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'balances_cache';
  @override
  VerificationContext validateIntegrity(
    Insertable<BalancesCacheData> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    } else if (isInserting) {
      context.missing(_idMeta);
    }
    if (data.containsKey('warehouse_id')) {
      context.handle(
        _warehouseIdMeta,
        warehouseId.isAcceptableOrUnknown(
          data['warehouse_id']!,
          _warehouseIdMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_warehouseIdMeta);
    }
    if (data.containsKey('product_id')) {
      context.handle(
        _productIdMeta,
        productId.isAcceptableOrUnknown(data['product_id']!, _productIdMeta),
      );
    } else if (isInserting) {
      context.missing(_productIdMeta);
    }
    if (data.containsKey('quantity')) {
      context.handle(
        _quantityMeta,
        quantity.isAcceptableOrUnknown(data['quantity']!, _quantityMeta),
      );
    } else if (isInserting) {
      context.missing(_quantityMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  BalancesCacheData map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return BalancesCacheData(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}id'],
      )!,
      warehouseId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}warehouse_id'],
      )!,
      productId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}product_id'],
      )!,
      quantity: attachedDatabase.typeMapping.read(
        DriftSqlType.double,
        data['${effectivePrefix}quantity'],
      )!,
    );
  }

  @override
  $BalancesCacheTable createAlias(String alias) {
    return $BalancesCacheTable(attachedDatabase, alias);
  }
}

class BalancesCacheData extends DataClass
    implements Insertable<BalancesCacheData> {
  final String id;
  final String warehouseId;
  final String productId;
  final double quantity;
  const BalancesCacheData({
    required this.id,
    required this.warehouseId,
    required this.productId,
    required this.quantity,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<String>(id);
    map['warehouse_id'] = Variable<String>(warehouseId);
    map['product_id'] = Variable<String>(productId);
    map['quantity'] = Variable<double>(quantity);
    return map;
  }

  BalancesCacheCompanion toCompanion(bool nullToAbsent) {
    return BalancesCacheCompanion(
      id: Value(id),
      warehouseId: Value(warehouseId),
      productId: Value(productId),
      quantity: Value(quantity),
    );
  }

  factory BalancesCacheData.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return BalancesCacheData(
      id: serializer.fromJson<String>(json['id']),
      warehouseId: serializer.fromJson<String>(json['warehouseId']),
      productId: serializer.fromJson<String>(json['productId']),
      quantity: serializer.fromJson<double>(json['quantity']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<String>(id),
      'warehouseId': serializer.toJson<String>(warehouseId),
      'productId': serializer.toJson<String>(productId),
      'quantity': serializer.toJson<double>(quantity),
    };
  }

  BalancesCacheData copyWith({
    String? id,
    String? warehouseId,
    String? productId,
    double? quantity,
  }) => BalancesCacheData(
    id: id ?? this.id,
    warehouseId: warehouseId ?? this.warehouseId,
    productId: productId ?? this.productId,
    quantity: quantity ?? this.quantity,
  );
  BalancesCacheData copyWithCompanion(BalancesCacheCompanion data) {
    return BalancesCacheData(
      id: data.id.present ? data.id.value : this.id,
      warehouseId: data.warehouseId.present
          ? data.warehouseId.value
          : this.warehouseId,
      productId: data.productId.present ? data.productId.value : this.productId,
      quantity: data.quantity.present ? data.quantity.value : this.quantity,
    );
  }

  @override
  String toString() {
    return (StringBuffer('BalancesCacheData(')
          ..write('id: $id, ')
          ..write('warehouseId: $warehouseId, ')
          ..write('productId: $productId, ')
          ..write('quantity: $quantity')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(id, warehouseId, productId, quantity);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is BalancesCacheData &&
          other.id == this.id &&
          other.warehouseId == this.warehouseId &&
          other.productId == this.productId &&
          other.quantity == this.quantity);
}

class BalancesCacheCompanion extends UpdateCompanion<BalancesCacheData> {
  final Value<String> id;
  final Value<String> warehouseId;
  final Value<String> productId;
  final Value<double> quantity;
  final Value<int> rowid;
  const BalancesCacheCompanion({
    this.id = const Value.absent(),
    this.warehouseId = const Value.absent(),
    this.productId = const Value.absent(),
    this.quantity = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  BalancesCacheCompanion.insert({
    required String id,
    required String warehouseId,
    required String productId,
    required double quantity,
    this.rowid = const Value.absent(),
  }) : id = Value(id),
       warehouseId = Value(warehouseId),
       productId = Value(productId),
       quantity = Value(quantity);
  static Insertable<BalancesCacheData> custom({
    Expression<String>? id,
    Expression<String>? warehouseId,
    Expression<String>? productId,
    Expression<double>? quantity,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (warehouseId != null) 'warehouse_id': warehouseId,
      if (productId != null) 'product_id': productId,
      if (quantity != null) 'quantity': quantity,
      if (rowid != null) 'rowid': rowid,
    });
  }

  BalancesCacheCompanion copyWith({
    Value<String>? id,
    Value<String>? warehouseId,
    Value<String>? productId,
    Value<double>? quantity,
    Value<int>? rowid,
  }) {
    return BalancesCacheCompanion(
      id: id ?? this.id,
      warehouseId: warehouseId ?? this.warehouseId,
      productId: productId ?? this.productId,
      quantity: quantity ?? this.quantity,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<String>(id.value);
    }
    if (warehouseId.present) {
      map['warehouse_id'] = Variable<String>(warehouseId.value);
    }
    if (productId.present) {
      map['product_id'] = Variable<String>(productId.value);
    }
    if (quantity.present) {
      map['quantity'] = Variable<double>(quantity.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('BalancesCacheCompanion(')
          ..write('id: $id, ')
          ..write('warehouseId: $warehouseId, ')
          ..write('productId: $productId, ')
          ..write('quantity: $quantity, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $OutboxOpsTable outboxOps = $OutboxOpsTable(this);
  late final $ProductsCacheTable productsCache = $ProductsCacheTable(this);
  late final $BalancesCacheTable balancesCache = $BalancesCacheTable(this);
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [
    outboxOps,
    productsCache,
    balancesCache,
  ];
}

typedef $$OutboxOpsTableCreateCompanionBuilder =
    OutboxOpsCompanion Function({
      required String id,
      required String opType,
      required String payloadJson,
      required DateTime createdAt,
      Value<int> attempts,
      Value<int> rowid,
    });
typedef $$OutboxOpsTableUpdateCompanionBuilder =
    OutboxOpsCompanion Function({
      Value<String> id,
      Value<String> opType,
      Value<String> payloadJson,
      Value<DateTime> createdAt,
      Value<int> attempts,
      Value<int> rowid,
    });

class $$OutboxOpsTableFilterComposer
    extends Composer<_$AppDatabase, $OutboxOpsTable> {
  $$OutboxOpsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get opType => $composableBuilder(
    column: $table.opType,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get payloadJson => $composableBuilder(
    column: $table.payloadJson,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get createdAt => $composableBuilder(
    column: $table.createdAt,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get attempts => $composableBuilder(
    column: $table.attempts,
    builder: (column) => ColumnFilters(column),
  );
}

class $$OutboxOpsTableOrderingComposer
    extends Composer<_$AppDatabase, $OutboxOpsTable> {
  $$OutboxOpsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get opType => $composableBuilder(
    column: $table.opType,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get payloadJson => $composableBuilder(
    column: $table.payloadJson,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get createdAt => $composableBuilder(
    column: $table.createdAt,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get attempts => $composableBuilder(
    column: $table.attempts,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$OutboxOpsTableAnnotationComposer
    extends Composer<_$AppDatabase, $OutboxOpsTable> {
  $$OutboxOpsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get opType =>
      $composableBuilder(column: $table.opType, builder: (column) => column);

  GeneratedColumn<String> get payloadJson => $composableBuilder(
    column: $table.payloadJson,
    builder: (column) => column,
  );

  GeneratedColumn<DateTime> get createdAt =>
      $composableBuilder(column: $table.createdAt, builder: (column) => column);

  GeneratedColumn<int> get attempts =>
      $composableBuilder(column: $table.attempts, builder: (column) => column);
}

class $$OutboxOpsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $OutboxOpsTable,
          OutboxOp,
          $$OutboxOpsTableFilterComposer,
          $$OutboxOpsTableOrderingComposer,
          $$OutboxOpsTableAnnotationComposer,
          $$OutboxOpsTableCreateCompanionBuilder,
          $$OutboxOpsTableUpdateCompanionBuilder,
          (OutboxOp, BaseReferences<_$AppDatabase, $OutboxOpsTable, OutboxOp>),
          OutboxOp,
          PrefetchHooks Function()
        > {
  $$OutboxOpsTableTableManager(_$AppDatabase db, $OutboxOpsTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$OutboxOpsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$OutboxOpsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$OutboxOpsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> id = const Value.absent(),
                Value<String> opType = const Value.absent(),
                Value<String> payloadJson = const Value.absent(),
                Value<DateTime> createdAt = const Value.absent(),
                Value<int> attempts = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => OutboxOpsCompanion(
                id: id,
                opType: opType,
                payloadJson: payloadJson,
                createdAt: createdAt,
                attempts: attempts,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String id,
                required String opType,
                required String payloadJson,
                required DateTime createdAt,
                Value<int> attempts = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => OutboxOpsCompanion.insert(
                id: id,
                opType: opType,
                payloadJson: payloadJson,
                createdAt: createdAt,
                attempts: attempts,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$OutboxOpsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $OutboxOpsTable,
      OutboxOp,
      $$OutboxOpsTableFilterComposer,
      $$OutboxOpsTableOrderingComposer,
      $$OutboxOpsTableAnnotationComposer,
      $$OutboxOpsTableCreateCompanionBuilder,
      $$OutboxOpsTableUpdateCompanionBuilder,
      (OutboxOp, BaseReferences<_$AppDatabase, $OutboxOpsTable, OutboxOp>),
      OutboxOp,
      PrefetchHooks Function()
    >;
typedef $$ProductsCacheTableCreateCompanionBuilder =
    ProductsCacheCompanion Function({
      required String id,
      required String sku,
      required String name,
      Value<String?> barcode,
      Value<String?> payloadJson,
      Value<int> rowid,
    });
typedef $$ProductsCacheTableUpdateCompanionBuilder =
    ProductsCacheCompanion Function({
      Value<String> id,
      Value<String> sku,
      Value<String> name,
      Value<String?> barcode,
      Value<String?> payloadJson,
      Value<int> rowid,
    });

class $$ProductsCacheTableFilterComposer
    extends Composer<_$AppDatabase, $ProductsCacheTable> {
  $$ProductsCacheTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get sku => $composableBuilder(
    column: $table.sku,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get barcode => $composableBuilder(
    column: $table.barcode,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get payloadJson => $composableBuilder(
    column: $table.payloadJson,
    builder: (column) => ColumnFilters(column),
  );
}

class $$ProductsCacheTableOrderingComposer
    extends Composer<_$AppDatabase, $ProductsCacheTable> {
  $$ProductsCacheTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get sku => $composableBuilder(
    column: $table.sku,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get barcode => $composableBuilder(
    column: $table.barcode,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get payloadJson => $composableBuilder(
    column: $table.payloadJson,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$ProductsCacheTableAnnotationComposer
    extends Composer<_$AppDatabase, $ProductsCacheTable> {
  $$ProductsCacheTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get sku =>
      $composableBuilder(column: $table.sku, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<String> get barcode =>
      $composableBuilder(column: $table.barcode, builder: (column) => column);

  GeneratedColumn<String> get payloadJson => $composableBuilder(
    column: $table.payloadJson,
    builder: (column) => column,
  );
}

class $$ProductsCacheTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $ProductsCacheTable,
          ProductsCacheData,
          $$ProductsCacheTableFilterComposer,
          $$ProductsCacheTableOrderingComposer,
          $$ProductsCacheTableAnnotationComposer,
          $$ProductsCacheTableCreateCompanionBuilder,
          $$ProductsCacheTableUpdateCompanionBuilder,
          (
            ProductsCacheData,
            BaseReferences<
              _$AppDatabase,
              $ProductsCacheTable,
              ProductsCacheData
            >,
          ),
          ProductsCacheData,
          PrefetchHooks Function()
        > {
  $$ProductsCacheTableTableManager(_$AppDatabase db, $ProductsCacheTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$ProductsCacheTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$ProductsCacheTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$ProductsCacheTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> id = const Value.absent(),
                Value<String> sku = const Value.absent(),
                Value<String> name = const Value.absent(),
                Value<String?> barcode = const Value.absent(),
                Value<String?> payloadJson = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => ProductsCacheCompanion(
                id: id,
                sku: sku,
                name: name,
                barcode: barcode,
                payloadJson: payloadJson,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String id,
                required String sku,
                required String name,
                Value<String?> barcode = const Value.absent(),
                Value<String?> payloadJson = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => ProductsCacheCompanion.insert(
                id: id,
                sku: sku,
                name: name,
                barcode: barcode,
                payloadJson: payloadJson,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$ProductsCacheTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $ProductsCacheTable,
      ProductsCacheData,
      $$ProductsCacheTableFilterComposer,
      $$ProductsCacheTableOrderingComposer,
      $$ProductsCacheTableAnnotationComposer,
      $$ProductsCacheTableCreateCompanionBuilder,
      $$ProductsCacheTableUpdateCompanionBuilder,
      (
        ProductsCacheData,
        BaseReferences<_$AppDatabase, $ProductsCacheTable, ProductsCacheData>,
      ),
      ProductsCacheData,
      PrefetchHooks Function()
    >;
typedef $$BalancesCacheTableCreateCompanionBuilder =
    BalancesCacheCompanion Function({
      required String id,
      required String warehouseId,
      required String productId,
      required double quantity,
      Value<int> rowid,
    });
typedef $$BalancesCacheTableUpdateCompanionBuilder =
    BalancesCacheCompanion Function({
      Value<String> id,
      Value<String> warehouseId,
      Value<String> productId,
      Value<double> quantity,
      Value<int> rowid,
    });

class $$BalancesCacheTableFilterComposer
    extends Composer<_$AppDatabase, $BalancesCacheTable> {
  $$BalancesCacheTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get warehouseId => $composableBuilder(
    column: $table.warehouseId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get productId => $composableBuilder(
    column: $table.productId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<double> get quantity => $composableBuilder(
    column: $table.quantity,
    builder: (column) => ColumnFilters(column),
  );
}

class $$BalancesCacheTableOrderingComposer
    extends Composer<_$AppDatabase, $BalancesCacheTable> {
  $$BalancesCacheTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get warehouseId => $composableBuilder(
    column: $table.warehouseId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get productId => $composableBuilder(
    column: $table.productId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<double> get quantity => $composableBuilder(
    column: $table.quantity,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$BalancesCacheTableAnnotationComposer
    extends Composer<_$AppDatabase, $BalancesCacheTable> {
  $$BalancesCacheTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get warehouseId => $composableBuilder(
    column: $table.warehouseId,
    builder: (column) => column,
  );

  GeneratedColumn<String> get productId =>
      $composableBuilder(column: $table.productId, builder: (column) => column);

  GeneratedColumn<double> get quantity =>
      $composableBuilder(column: $table.quantity, builder: (column) => column);
}

class $$BalancesCacheTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $BalancesCacheTable,
          BalancesCacheData,
          $$BalancesCacheTableFilterComposer,
          $$BalancesCacheTableOrderingComposer,
          $$BalancesCacheTableAnnotationComposer,
          $$BalancesCacheTableCreateCompanionBuilder,
          $$BalancesCacheTableUpdateCompanionBuilder,
          (
            BalancesCacheData,
            BaseReferences<
              _$AppDatabase,
              $BalancesCacheTable,
              BalancesCacheData
            >,
          ),
          BalancesCacheData,
          PrefetchHooks Function()
        > {
  $$BalancesCacheTableTableManager(_$AppDatabase db, $BalancesCacheTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$BalancesCacheTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$BalancesCacheTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$BalancesCacheTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> id = const Value.absent(),
                Value<String> warehouseId = const Value.absent(),
                Value<String> productId = const Value.absent(),
                Value<double> quantity = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => BalancesCacheCompanion(
                id: id,
                warehouseId: warehouseId,
                productId: productId,
                quantity: quantity,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String id,
                required String warehouseId,
                required String productId,
                required double quantity,
                Value<int> rowid = const Value.absent(),
              }) => BalancesCacheCompanion.insert(
                id: id,
                warehouseId: warehouseId,
                productId: productId,
                quantity: quantity,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$BalancesCacheTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $BalancesCacheTable,
      BalancesCacheData,
      $$BalancesCacheTableFilterComposer,
      $$BalancesCacheTableOrderingComposer,
      $$BalancesCacheTableAnnotationComposer,
      $$BalancesCacheTableCreateCompanionBuilder,
      $$BalancesCacheTableUpdateCompanionBuilder,
      (
        BalancesCacheData,
        BaseReferences<_$AppDatabase, $BalancesCacheTable, BalancesCacheData>,
      ),
      BalancesCacheData,
      PrefetchHooks Function()
    >;

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$OutboxOpsTableTableManager get outboxOps =>
      $$OutboxOpsTableTableManager(_db, _db.outboxOps);
  $$ProductsCacheTableTableManager get productsCache =>
      $$ProductsCacheTableTableManager(_db, _db.productsCache);
  $$BalancesCacheTableTableManager get balancesCache =>
      $$BalancesCacheTableTableManager(_db, _db.balancesCache);
}
