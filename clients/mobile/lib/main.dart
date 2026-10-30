import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'auth/auth_repository.dart';
import 'db/app_database.dart';
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

  final loggedIn = await auth.isLoggedIn;

  runApp(
    MultiProvider(
      providers: [
        Provider<AppDatabase>.value(value: db),
        Provider<AuthRepository>.value(value: auth),
        Provider<Outbox>.value(value: outbox),
        Provider<SyncClient>.value(value: sync),
        Provider<SelfCheckoutApi>.value(value: selfCheckout),
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
    return MaterialApp(
      title: 'Modern Tech',
      theme: buildModernTechTheme(),
      home: loggedIn ? const HomeScreen() : const ScanGoWelcomeScreen(),
    );
  }
}
