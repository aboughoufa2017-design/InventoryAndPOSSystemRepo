import 'package:flutter/services.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:local_auth/local_auth.dart';

/// Secure vault for staff password unlocked via device biometrics.
class BiometricCredentials {
  BiometricCredentials({
    FlutterSecureStorage? storage,
    LocalAuthentication? localAuth,
  })  : _storage = storage ?? const FlutterSecureStorage(),
        _localAuth = localAuth ?? LocalAuthentication();

  final FlutterSecureStorage _storage;
  final LocalAuthentication _localAuth;

  static const _tenantKey = 'bio_tenant';
  static const _identifierKey = 'bio_identifier';
  static const _passwordKey = 'bio_password';

  Future<bool> get isAvailable async {
    try {
      final can = await _localAuth.canCheckBiometrics || await _localAuth.isDeviceSupported();
      return can;
    } on PlatformException {
      return false;
    }
  }

  Future<bool> get hasSavedCredentials async {
    final password = await _storage.read(key: _passwordKey);
    return password != null && password.isNotEmpty;
  }

  Future<void> save({
    required String tenantCode,
    required String emailOrPhone,
    required String password,
  }) async {
    await _storage.write(key: _tenantKey, value: tenantCode.trim());
    await _storage.write(key: _identifierKey, value: emailOrPhone.trim());
    await _storage.write(key: _passwordKey, value: password);
  }

  Future<void> clear() async {
    await _storage.delete(key: _tenantKey);
    await _storage.delete(key: _identifierKey);
    await _storage.delete(key: _passwordKey);
  }

  /// Returns credentials after successful biometric prompt, or null if cancelled/failed.
  Future<({String tenantCode, String emailOrPhone, String password})?> unlock({
    required String reason,
  }) async {
    try {
      final ok = await _localAuth.authenticate(
        localizedReason: reason,
        options: const AuthenticationOptions(
          biometricOnly: false,
          stickyAuth: true,
        ),
      );
      if (!ok) return null;
    } on PlatformException {
      return null;
    }

    final tenant = await _storage.read(key: _tenantKey);
    final identifier = await _storage.read(key: _identifierKey);
    final password = await _storage.read(key: _passwordKey);
    if (tenant == null ||
        tenant.isEmpty ||
        identifier == null ||
        identifier.isEmpty ||
        password == null ||
        password.isEmpty) {
      return null;
    }
    return (tenantCode: tenant, emailOrPhone: identifier, password: password);
  }
}
