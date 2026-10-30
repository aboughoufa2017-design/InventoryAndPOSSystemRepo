import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../connectivity/connectivity_controller.dart';
import '../l10n/locale_controller.dart';
import '../theme/modern_tech_theme.dart';

/// Persistent strip under the app bar when the device has no network.
class OfflineBanner extends StatelessWidget {
  const OfflineBanner({super.key});

  @override
  Widget build(BuildContext context) {
    final online = context.watch<ConnectivityController>().isOnline;
    if (online) return const SizedBox.shrink();

    final s = context.watch<LocaleController>().s;
    return Material(
      color: const Color(0xFFFFF4E5),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
        child: Row(
          children: [
            Icon(Icons.cloud_off_outlined, color: Colors.orange.shade800, size: 20),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                s.offlineBanner,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: ModernTechColors.slate,
                      fontWeight: FontWeight.w600,
                    ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Wraps a staff screen body with [OfflineBanner].
class OfflineAwareScaffold extends StatelessWidget {
  const OfflineAwareScaffold({
    super.key,
    required this.appBar,
    required this.body,
    this.floatingActionButton,
  });

  final PreferredSizeWidget appBar;
  final Widget body;
  final Widget? floatingActionButton;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: appBar,
      floatingActionButton: floatingActionButton,
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const OfflineBanner(),
          Expanded(child: body),
        ],
      ),
    );
  }
}

/// Listens once at the app root for offline → online and shows a snackbar.
class OfflineBackOnlineListener extends StatefulWidget {
  const OfflineBackOnlineListener({super.key, required this.child});

  final Widget child;

  @override
  State<OfflineBackOnlineListener> createState() => _OfflineBackOnlineListenerState();
}

class _OfflineBackOnlineListenerState extends State<OfflineBackOnlineListener> {
  ConnectivityController? _connectivity;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final next = context.read<ConnectivityController>();
    if (!identical(_connectivity, next)) {
      _connectivity?.removeListener(_onConnectivity);
      _connectivity = next;
      _connectivity!.addListener(_onConnectivity);
    }
  }

  void _onConnectivity() {
    if (!mounted || _connectivity == null) return;
    if (_connectivity!.consumeBackOnline()) {
      final s = context.read<LocaleController>().s;
      final messenger = ScaffoldMessenger.maybeOf(context);
      messenger?.showSnackBar(SnackBar(content: Text(s.backOnline)));
    }
  }

  @override
  void dispose() {
    _connectivity?.removeListener(_onConnectivity);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => widget.child;
}
