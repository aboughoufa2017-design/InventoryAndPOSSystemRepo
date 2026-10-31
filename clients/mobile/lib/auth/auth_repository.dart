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
  static const _tenantKey = 'remember_tenant';
  static const _emailKey = 'remember_email';
  static const _displayNameKey = 'remember_display_name';

  Uri _uri(String path) => Uri.parse('$apiBaseUrl$path');

  /// True if a refresh or access token is stored (survives cold start).
  Future<bool> get isLoggedIn async {
    final access = await _storage.read(key: _accessKey);
    if (access != null && access.isNotEmpty) return true;
    final refresh = await _storage.read(key: _refreshKey);
    return refresh != null && refresh.isNotEmpty;
  }

  Future<String?> get accessToken => _storage.read(key: _accessKey);

  Future<String?> get rememberedTenantCode => _storage.read(key: _tenantKey);

  Future<String?> get rememberedEmail => _storage.read(key: _emailKey);

  Future<String?> get rememberedDisplayName => _storage.read(key: _displayNameKey);

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

  /// Cold-start session restore: refresh when online; keep tokens when offline.
  Future<bool> ensureSession({required bool isOnline}) async {
    final access = await _storage.read(key: _accessKey);
    final refresh = await _storage.read(key: _refreshKey);
    final hasAccess = access != null && access.isNotEmpty;
    final hasRefresh = refresh != null && refresh.isNotEmpty;
    if (!hasAccess && !hasRefresh) {
      return false;
    }

    if (!isOnline) {
      return true;
    }

    if (hasRefresh) {
      try {
        await this.refresh();
        return true;
      } catch (_) {
        if (hasAccess) {
          return true;
        }
        await logout();
        return false;
      }
    }

    return hasAccess;
  }

  Future<void> login({
    required String tenantCode,
    required String emailOrPhone,
    required String password,
  }) async {
    final deviceKey = await getOrCreateDeviceKey();
    final response = await _http.post(
      _uri('/api/v1/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'tenantCode': tenantCode,
        'emailOrPhone': emailOrPhone,
        'email': emailOrPhone,
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
    await _persistProfile(
      tenantCode: tenantCode.trim(),
      emailOrPhone: emailOrPhone.trim(),
      displayName: (data['displayName'] ?? data['DisplayName'])?.toString(),
    );
  }

  Future<void> registerStaff({
    required String tenantCode,
    required String email,
    required String phoneNumber,
    required String password,
    required String displayName,
  }) async {
    final deviceKey = await getOrCreateDeviceKey();
    final response = await _http.post(
      _uri('/api/v1/auth/register'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'tenantCode': tenantCode,
        'email': email,
        'phoneNumber': phoneNumber,
        'password': password,
        'displayName': displayName,
        'deviceKey': deviceKey,
        'deviceName': _deviceName,
        'applicationVersion': '1.0.0+1',
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw AuthException(
        'Register failed (${response.statusCode}): ${response.body}',
      );
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    await _persistTokens(data);
    await _persistProfile(
      tenantCode: tenantCode.trim(),
      emailOrPhone: email.trim(),
      displayName: (data['displayName'] ?? data['DisplayName'])?.toString() ?? displayName,
    );
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
    final display = (data['displayName'] ?? data['DisplayName'])?.toString();
    final email = (data['email'] ?? data['Email'])?.toString();
    if (display != null && display.isNotEmpty) {
      await _storage.write(key: _displayNameKey, value: display);
    }
    if (email != null && email.isNotEmpty) {
      await _storage.write(key: _emailKey, value: email);
    }
  }

  Future<void> logout() async {
    await _storage.delete(key: _accessKey);
    await _storage.delete(key: _refreshKey);
    await _storage.delete(key: _displayNameKey);
    // Keep device_key + remembered tenant/email for next login prefill.
  }

  Future<void> _persistProfile({
    required String tenantCode,
    required String emailOrPhone,
    String? displayName,
  }) async {
    await _storage.write(key: _tenantKey, value: tenantCode);
    await _storage.write(key: _emailKey, value: emailOrPhone);
    if (displayName != null && displayName.isNotEmpty) {
      await _storage.write(key: _displayNameKey, value: displayName);
    }
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
