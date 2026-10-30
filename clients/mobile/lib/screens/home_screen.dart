import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../auth/session_controller.dart';
import '../connectivity/connectivity_controller.dart';
import '../l10n/locale_controller.dart';
import '../sync/outbox.dart';
import '../sync/sync_client.dart';
import '../theme/modern_tech_theme.dart';
import '../widgets/offline_banner.dart';
import 'inventory_ops_screen.dart';
import 'pos_checkout_screen.dart';
import 'products_screen.dart';
import 'register_product_screen.dart';
import 'scan_go_welcome_screen.dart';

/// Staff-only home. Guests never reach this screen.
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String? _status;
  bool _syncing = false;
  int _pendingOps = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadPending());
  }

  Future<void> _loadPending() async {
    final pending = await context.read<Outbox>().pending();
    if (mounted) setState(() => _pendingOps = pending.length);
  }

  Future<void> _sync() async {
    final s = context.read<LocaleController>().s;
    final online = context.read<ConnectivityController>().isOnline;
    if (!online) {
      setState(() => _status = s.syncDisabledOffline);
      return;
    }
    setState(() {
      _syncing = true;
      _status = null;
    });
    try {
      final result = await context.read<SyncClient>().sync();
      setState(() {
        _status = result.offline
            ? s.syncOffline
            : s.syncResult(result.pushed, result.pulled);
      });
      await _loadPending();
    } catch (e) {
      final text = e.toString();
      setState(() {
        _status = text.contains('Device id claim') ? s.syncNeedRelogin : text;
      });
    } finally {
      if (mounted) setState(() => _syncing = false);
    }
  }

  Future<void> _logout() async {
    await context.read<SessionController>().markLoggedOut();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute<void>(builder: (_) => const ScanGoWelcomeScreen()),
      (_) => false,
    );
  }

  void _open(Widget screen) {
    Navigator.of(context).push(MaterialPageRoute<void>(builder: (_) => screen)).then((_) {
      if (mounted) _loadPending();
    });
  }

  @override
  Widget build(BuildContext context) {
    final locale = context.watch<LocaleController>();
    final connectivity = context.watch<ConnectivityController>();
    final session = context.watch<SessionController>();
    final s = locale.s;
    final online = connectivity.isOnline;

    return OfflineAwareScaffold(
      appBar: AppBar(
        title: Text(s.staffHome),
        actions: [
          TextButton(
            onPressed: () => locale.toggle(),
            child: Text(s.languageToggle, style: const TextStyle(color: Colors.white)),
          ),
          IconButton(
            tooltip: s.signOut,
            onPressed: _logout,
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          if (session.email != null || session.displayName != null) ...[
            Text(
              '${s.signedInAs} ${session.displayName ?? session.email}',
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: ModernTechColors.muted,
                    fontWeight: FontWeight.w600,
                  ),
            ),
            const SizedBox(height: 8),
          ],
          Text(
            s.staffHomeHint,
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: ModernTechColors.muted),
          ),
          if (_pendingOps > 0) ...[
            const SizedBox(height: 12),
            Row(
              children: [
                Icon(Icons.pending_actions, size: 18, color: ModernTechColors.royalBlue),
                const SizedBox(width: 8),
                Expanded(child: Text(s.pendingOutbox(_pendingOps))),
              ],
            ),
          ],
          const SizedBox(height: 16),
          FilledButton.icon(
            onPressed: (_syncing || !online) ? null : _sync,
            icon: _syncing
                ? const SizedBox(
                    height: 18,
                    width: 18,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                  )
                : Icon(online ? Icons.sync : Icons.cloud_off),
            label: Text(online ? s.syncNow : s.syncDisabledOffline),
          ),
          if (_status != null) ...[
            const SizedBox(height: 12),
            Text(_status!, textAlign: TextAlign.center),
          ],
          const SizedBox(height: 24),
          _StaffTile(
            icon: Icons.point_of_sale_outlined,
            title: s.posCheckout,
            onTap: () => _open(const PosCheckoutScreen()),
          ),
          _StaffTile(
            icon: Icons.inventory_2_outlined,
            title: s.products,
            onTap: () => _open(const ProductsScreen()),
          ),
          _StaffTile(
            icon: Icons.qr_code_scanner,
            title: s.registerProduct,
            onTap: () => _open(const RegisterProductScreen()),
          ),
          _StaffTile(
            icon: Icons.warehouse_outlined,
            title: s.inventoryOps,
            onTap: () => _open(const InventoryOpsScreen()),
          ),
        ],
      ),
    );
  }
}

class _StaffTile extends StatelessWidget {
  const _StaffTile({
    required this.icon,
    required this.title,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: ListTile(
        leading: Icon(icon, color: ModernTechColors.royalBlue),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.w600)),
        trailing: Icon(Directionality.of(context) == TextDirection.rtl
            ? Icons.chevron_left
            : Icons.chevron_right),
        onTap: onTap,
      ),
    );
  }
}
