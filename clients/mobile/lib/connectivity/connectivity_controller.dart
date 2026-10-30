import 'dart:async';

import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/foundation.dart';

/// App-wide online/offline status for banners and disabled actions.
class ConnectivityController extends ChangeNotifier {
  ConnectivityController({Connectivity? connectivity})
      : _connectivity = connectivity ?? Connectivity();

  final Connectivity _connectivity;
  StreamSubscription<List<ConnectivityResult>>? _sub;
  bool _isOnline = true;
  bool _wasOffline = false;

  bool get isOnline => _isOnline;

  /// True once after transitioning offline → online (for snackbar).
  bool consumeBackOnline() {
    if (_wasOffline && _isOnline) {
      _wasOffline = false;
      return true;
    }
    return false;
  }

  Future<void> start() async {
    final initial = await _connectivity.checkConnectivity();
    _apply(initial);
    _sub = _connectivity.onConnectivityChanged.listen(_apply);
  }

  void _apply(List<ConnectivityResult> results) {
    final online = results.any((r) => r != ConnectivityResult.none);
    if (!online) {
      _wasOffline = true;
    }
    if (online == _isOnline) return;
    _isOnline = online;
    notifyListeners();
  }

  @override
  void dispose() {
    _sub?.cancel();
    super.dispose();
  }
}
