import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:provider/provider.dart';

import 'auth/auth_repository.dart';
import 'catalog/catalog_api.dart';
import 'db/app_database.dart';
import 'l10n/locale_controller.dart';
import 'scan_go/self_checkout_api.dart';
import 'screens/home_screen.dart';
import 'screens/scan_go_welcome_screen.dart';
import 'sync/outbox.dart';
import 'sync/sync_client.dart';
import 'theme/modern_tech_theme.dart';

/// API origin, e.g. `https://api.example.com` (no trailing slash).
/// Override locally: `--dart-define=API_BASE_URL=http://10.0.2.2:5011`
const String kApiBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: 'https://warehouse-api-0890.onrender.com',
);

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final db = AppDatabase();
  final auth = AuthRepository(apiBaseUrl: kApiBaseUrl);
  final outbox = Outbox(db);
  final sync = SyncClient(
    apiBaseUrl: kApiBaseUrl,
    auth: auth,
    db: db,
    outbox: outbox,
  );
  final selfCheckout = SelfCheckoutApi(apiBaseUrl: kApiBaseUrl, auth: auth);
  final catalog = CatalogApi(apiBaseUrl: kApiBaseUrl, auth: auth);
  final locale = LocaleController();
  await locale.load();

  final loggedIn = await auth.isLoggedIn;

  runApp(
    MultiProvider(
      providers: [
        Provider<AppDatabase>.value(value: db),
        Provider<AuthRepository>.value(value: auth),
        Provider<Outbox>.value(value: outbox),
        Provider<SyncClient>.value(value: sync),
        Provider<SelfCheckoutApi>.value(value: selfCheckout),
        Provider<CatalogApi>.value(value: catalog),
        ChangeNotifierProvider<LocaleController>.value(value: locale),
      ],
      child: WarehouseApp(loggedIn: loggedIn),
    ),
  );
}

class WarehouseApp extends StatelessWidget {
  const WarehouseApp({super.key, required this.loggedIn});

  final bool loggedIn;

  @override
  Widget build(BuildContext context) {
    final localeCtrl = context.watch<LocaleController>();
    return MaterialApp(
      title: localeCtrl.s.brand,
      debugShowCheckedModeBanner: false,
      locale: localeCtrl.locale,
      supportedLocales: const [Locale('ar'), Locale('en')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      theme: buildModernTechTheme(),
      builder: (context, child) {
        return Directionality(
          textDirection: localeCtrl.textDirection,
          child: child ?? const SizedBox.shrink(),
        );
      },
      home: loggedIn ? const HomeScreen() : const ScanGoWelcomeScreen(),
    );
  }
}
