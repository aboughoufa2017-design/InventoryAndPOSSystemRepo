import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../auth/auth_repository.dart';
import '../sync/sync_client.dart';
import 'inventory_ops_screen.dart';
import 'pos_checkout_screen.dart';
import 'products_screen.dart';
import 'register_product_screen.dart';
import 'scan_go_welcome_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String? _status;
  bool _syncing = false;

  Future<void> _sync() async {
    setState(() {
      _syncing = true;
      _status = null;
    });
    try {
      final result = await context.read<SyncClient>().sync();
      setState(() {
        _status = result.offline
            ? 'Offline — sync skipped'
            : 'Pushed ${result.pushed}, pulled ${result.pulled}';
      });
    } catch (e) {
      setState(() => _status = e.toString());
    } finally {
      if (mounted) setState(() => _syncing = false);
    }
  }

  Future<void> _logout() async {
    await context.read<AuthRepository>().logout();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Modern Tech'),
        actions: [
          IconButton(
            tooltip: 'Sign out',
            onPressed: _logout,
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const Text(
              'Offline-first scaffold. Mutations queue locally; sync talks to /api/v1/sync. '
              'Posting rules remain server-side.',
            ),
            const SizedBox(height: 24),
            FilledButton.icon(
              onPressed: _syncing ? null : _sync,
              icon: _syncing
                  ? const SizedBox(
                      height: 18,
                      width: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.sync),
              label: const Text('Sync now'),
            ),
            if (_status != null) ...[
              const SizedBox(height: 12),
              Text(_status!),
            ],
            const SizedBox(height: 24),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => const ProductsScreen(),
                  ),
                );
              },
              child: const Text('Products / barcode'),
            ),
            const SizedBox(height: 12),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => const RegisterProductScreen(),
                  ),
                );
              },
              child: const Text('Register product (scan)'),
            ),
            const SizedBox(height: 12),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => const InventoryOpsScreen(),
                  ),
                );
              },
              child: const Text('Inventory ops (offline queue)'),
            ),
            const SizedBox(height: 12),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => const PosCheckoutScreen(),
                  ),
                );
              },
              child: const Text('POS checkout + Scan & Go receive'),
            ),
            const SizedBox(height: 12),
            OutlinedButton(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => const ScanGoWelcomeScreen(),
                  ),
                );
              },
              child: const Text('Customer Scan & Go'),
            ),
          ],
        ),
      ),
    );
  }
}
