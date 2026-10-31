import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:provider/provider.dart';

import 'auth/auth_repository.dart';
import 'auth/session_controller.dart';
import 'catalog/catalog_api.dart';
import 'connectivity/connectivity_controller.dart';
import 'db/app_database.dart';
import 'l10n/locale_controller.dart';
import 'receipts/local_receipt_store.dart';
import 'receipts/receipt_api.dart';
import 'scan_go/self_checkout_api.dart';
import 'screens/home_screen.dart';
import 'screens/scan_go_welcome_screen.dart';
import 'sync/outbox.dart';
import 'sync/sync_client.dart';
import 'theme/modern_tech_theme.dart';
import 'widgets/offline_banner.dart';

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

  final connectivity = ConnectivityController();
  await connectivity.start();

  final session = SessionController(auth);
  await auth.ensureSession(isOnline: connectivity.isOnline);
  await session.hydrate();
  final receipts = LocalReceiptStore();
  final receiptApi = ReceiptApi(apiBaseUrl: kApiBaseUrl, auth: auth);

  runApp(
    MultiProvider(
      providers: [
        Provider<AppDatabase>.value(value: db),
        Provider<AuthRepository>.value(value: auth),
        Provider<Outbox>.value(value: outbox),
        Provider<SyncClient>.value(value: sync),
        Provider<SelfCheckoutApi>.value(value: selfCheckout),
        Provider<CatalogApi>.value(value: catalog),
        Provider<LocalReceiptStore>.value(value: receipts),
        Provider<ReceiptApi>.value(value: receiptApi),
        ChangeNotifierProvider<LocaleController>.value(value: locale),
        ChangeNotifierProvider<ConnectivityController>.value(value: connectivity),
        ChangeNotifierProvider<SessionController>.value(value: session),
      ],
      child: const WarehouseApp(),
    ),
  );
}

class WarehouseApp extends StatelessWidget {
  const WarehouseApp({super.key});

  @override
  Widget build(BuildContext context) {
    final localeCtrl = context.watch<LocaleController>();
    final session = context.watch<SessionController>();
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
        return OfflineBackOnlineListener(
          child: Directionality(
            textDirection: localeCtrl.textDirection,
            child: child ?? const SizedBox.shrink(),
          ),
        );
      },
      home: session.isAuthenticated ? const HomeScreen() : const ScanGoWelcomeScreen(),
    );
  }
}
