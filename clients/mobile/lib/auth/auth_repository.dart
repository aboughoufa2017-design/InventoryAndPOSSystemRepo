import 'dart:convert';
import 'dart:io' show Platform;

import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;
import 'package:uuid/uuid.dart';

/// Authenticates against `/api/v1/auth` and stores tokens in secure storage.
///
/// Always registers a stable [deviceKey] on login so JWTs carry `device_id`
/// (required for `/api/v1/sync/push`).
class AuthRepository {
  AuthRepository({
    required this.apiBaseUrl,
    http.Client? httpClient,
    FlutterSecureStorage? secureStorage,
  })  : _http = httpClient ?? http.Client(),
        _storage = secureStorage ?? const FlutterSecureStorage();

  final String apiBaseUrl;
  final http.Client _http;
  final FlutterSecureStorage _storage;
  final _uuid = const Uuid();

  static const _accessKey = 'access_token';
  static const _refreshKey = 'refresh_token';
  static const _deviceKey = 'device_key';

  Uri _uri(String path) => Uri.parse('$apiBaseUrl$path');

  Future<bool> get isLoggedIn async =>
      (await _storage.read(key: _accessKey))?.isNotEmpty == true;

  Future<String?> get accessToken => _storage.read(key: _accessKey);

  /// Stable per-install device key used for login + JWT `device_id` claim.
  Future<String> getOrCreateDeviceKey() async {
    final existing = await _storage.read(key: _deviceKey);
    if (existing != null && existing.isNotEmpty) {
      return existing;
    }
    final created = _uuid.v4();
    await _storage.write(key: _deviceKey, value: created);
    return created;
  }

  String get _deviceName {
    if (kIsWeb) return 'Warehouse Web';
    try {
      if (Platform.isAndroid) return 'Warehouse Android';
      if (Platform.isIOS) return 'Warehouse iOS';
    } catch (_) {
      // Ignore platform probe failures.
    }
    return 'Warehouse Mobile';
  }

  Future<void> login({
    required String tenantCode,
    required String email,
    required String password,
  }) async {
    final deviceKey = await getOrCreateDeviceKey();
    final response = await _http.post(
      _uri('/api/v1/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'tenantCode': tenantCode,
        'email': email,
        'password': password,
        'deviceKey': deviceKey,
        'deviceName': _deviceName,
        'applicationVersion': '1.0.0+1',
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw AuthException(
        'Login failed (${response.statusCode}): ${response.body}',
      );
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    await _persistTokens(data);
  }

  Future<void> refresh() async {
    final refreshToken = await _storage.read(key: _refreshKey);
    if (refreshToken == null || refreshToken.isEmpty) {
      throw AuthException('No refresh token available');
    }
    final response = await _http.post(
      _uri('/api/v1/auth/refresh'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'refreshToken': refreshToken}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw AuthException(
        'Refresh failed (${response.statusCode}): ${response.body}',
      );
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    await _persistTokens(data);
  }

  Future<void> logout() async {
    await _storage.delete(key: _accessKey);
    await _storage.delete(key: _refreshKey);
    // Keep device_key so the same device re-registers on next login.
  }

  Future<void> _persistTokens(Map<String, dynamic> data) async {
    final access = data['accessToken'] ?? data['access_token'];
    final refresh = data['refreshToken'] ?? data['refresh_token'];
    if (access is! String || access.isEmpty) {
      throw AuthException('Login response missing access token');
    }
    await _storage.write(key: _accessKey, value: access);
    if (refresh is String && refresh.isNotEmpty) {
      await _storage.write(key: _refreshKey, value: refresh);
    }
  }

  void dispose() => _http.close();
}

class AuthException implements Exception {
  AuthException(this.message);
  final String message;

  @override
  String toString() => 'AuthException: $message';
}
