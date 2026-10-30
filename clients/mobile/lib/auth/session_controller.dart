import 'package:flutter/foundation.dart';

import 'auth_repository.dart';

/// Staff session state for routing and header display.
class SessionController extends ChangeNotifier {
  SessionController(this._auth);

  final AuthRepository _auth;

  bool _isAuthenticated = false;
  String? _email;
  String? _displayName;
  String? _tenantCode;

  bool get isAuthenticated => _isAuthenticated;
  String? get email => _email;
  String? get displayName => _displayName;
  String? get tenantCode => _tenantCode;

  Future<void> hydrate() async {
    _isAuthenticated = await _auth.isLoggedIn;
    if (_isAuthenticated) {
      _email = await _auth.rememberedEmail;
      _displayName = await _auth.rememberedDisplayName;
      _tenantCode = await _auth.rememberedTenantCode;
    } else {
      _email = null;
      _displayName = null;
      // Keep tenant for login prefill even when logged out.
      _tenantCode = await _auth.rememberedTenantCode;
    }
    notifyListeners();
  }

  Future<void> markLoggedIn() async {
    _isAuthenticated = true;
    _email = await _auth.rememberedEmail;
    _displayName = await _auth.rememberedDisplayName;
    _tenantCode = await _auth.rememberedTenantCode;
    notifyListeners();
  }

  Future<void> markLoggedOut() async {
    await _auth.logout();
    _isAuthenticated = false;
    _email = null;
    _displayName = null;
    _tenantCode = await _auth.rememberedTenantCode;
    notifyListeners();
  }
}
