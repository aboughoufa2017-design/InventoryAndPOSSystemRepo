import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'app_strings.dart';

/// Persists and notifies AR/EN locale (default Arabic).
class LocaleController extends ChangeNotifier {
  LocaleController({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  static const _key = 'app_locale';
  final FlutterSecureStorage _storage;

  Locale _locale = const Locale('ar');

  Locale get locale => _locale;

  bool get isArabic => _locale.languageCode == 'ar';

  AppStrings get s => AppStrings(_locale);

  TextDirection get textDirection =>
      isArabic ? TextDirection.rtl : TextDirection.ltr;

  Future<void> load() async {
    final raw = await _storage.read(key: _key);
    if (raw == 'en') {
      _locale = const Locale('en');
    } else {
      _locale = const Locale('ar');
    }
    notifyListeners();
  }

  Future<void> setLocale(Locale locale) async {
    final code = locale.languageCode == 'en' ? 'en' : 'ar';
    _locale = Locale(code);
    await _storage.write(key: _key, value: code);
    notifyListeners();
  }

  Future<void> toggle() async {
    await setLocale(isArabic ? const Locale('en') : const Locale('ar'));
  }
}
