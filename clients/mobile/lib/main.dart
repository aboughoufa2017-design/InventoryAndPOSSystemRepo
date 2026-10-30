import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'auth/auth_repository.dart';
import 'db/app_database.dart';
import 'screens/home_screen.dart';
import 'screens/login_screen.dart';
import 'sync/outbox.dart';
import 'sync/sync_client.dart';

/// API origin, e.g. `https://api.example.com` (no trailing slash).
const String kApiBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: 'http://localhost:3000',
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

  final loggedIn = await auth.isLoggedIn;

  runApp(
    MultiProvider(
      providers: [
        Provider<AppDatabase>.value(value: db),
        Provider<AuthRepository>.value(value: auth),
        Provider<Outbox>.value(value: outbox),
        Provider<SyncClient>.value(value: sync),
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
      title: 'Warehouse',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF0F4C5C)),
        useMaterial3: true,
      ),
      home: loggedIn ? const HomeScreen() : const LoginScreen(),
    );
  }
}
